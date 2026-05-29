using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRAdapter : MonoBehaviour
{
    [Header("Связанные компоненты")]
    public Engine craneEngine;
    public Transform vrLeverTransform;
    public XRBaseControllerInteractor controllerInteractor;
    public XRGrabInteractable leverGrabInteractable;

    [Header("Настройки углов рычага")]
    [Tooltip("Минимальный угол рычага (например, -135)")]
    public float minAngle = -135f;
    
    [Tooltip("Максимальный угол рычага (например, -45)")]
    public float maxAngle = -45f;
    
    [Tooltip("Мёртвая зона в градусах вокруг нейтрали")]
    public float deadZoneDegrees = 5f;
    
    [Tooltip("Инвертировать направление")]
    public bool invertDirection = false;

    [Header("Настройки вибрации")]
    public float hapticAmplitude = 0.5f;
    public float hapticDuration = 0.1f;
    public float hapticCooldown = 0.2f;

    [Header("Отладка")]
    public bool debugMode = false;

    private enum LeverAxis { X, Y, Z }
    [SerializeField]
    private LeverAxis trackingAxis = LeverAxis.X;

    private int lastGear = 0;
    private float lastHapticTime = -1f;
    private bool isGrabbed = false;
    private float lastControlledAngle = 0f;
    private bool hasControlledAngle = false;
    
    // Нейтральное положение рычага (середина диапазона)
    private float neutralAngle;

    [Header("Дополнительная фильтрация")]
    [Tooltip("Плавность изменения угла для устранения дрожания")]
    public float angleSmoothSpeed = 10f;
    private float smoothedAngle;
    // Добавим скрытый флаг, чтобы убрать лишний вес в Update
    private bool wasGrabbedLastFrame = false;
    
    void Start()
    {
        if (craneEngine != null)
            lastGear = craneEngine.gearNow;

        // Вычисляем нейтральное положение (середина диапазона)
        neutralAngle = (minAngle + maxAngle) / 2f;
        
        if (debugMode)
            Debug.Log($"[VRAdapter] Диапазон: {minAngle}° до {maxAngle}°, нейтраль: {neutralAngle}°");

        if (leverGrabInteractable != null)
        {
            leverGrabInteractable.selectEntered.AddListener(OnLeverGrabbed);
            leverGrabInteractable.selectExited.AddListener(OnLeverReleased);
        }
    }

    void OnLeverGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        if (debugMode) Debug.Log("[VRAdapter] Рычаг захвачен");
    }

    void OnLeverReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (debugMode) Debug.Log("[VRAdapter] Рычаг отпущен, угол зафиксирован: " + lastControlledAngle);
    }

 

    void Update()
    {
        // Оставляем только одну быструю валидацию
        if (craneEngine == null || vrLeverTransform == null) return;

        if (isGrabbed)
        {
            float currentAngle = GetLocalLeverAngle();

            // ОПТИМИЗАЦИЯ 1: Мгновенно телепортируем smoothedAngle к первому кадру захвата.
            // Без этого рычаг при взятии будет плавно догонять руку из старого нейтрального положения.
            if (!wasGrabbedLastFrame)
            {
                smoothedAngle = currentAngle;
                wasGrabbedLastFrame = true;
                hasControlledAngle = true;
            }
            else
            {
                // ОПТИМИЗАЦИЯ 2: Обычное сглаживание во время удержания
                smoothedAngle = Mathf.Lerp(smoothedAngle, currentAngle, Time.deltaTime * angleSmoothSpeed);
            }

            lastControlledAngle = smoothedAngle;
        }
        else
        {
            // ОПТИМИЗАЦИЯ 3: Сбрасываем флаг кадра и выходим сразу, если рычаг не трогали
            wasGrabbedLastFrame = false;
            if (!hasControlledAngle) return;
            
            // Если отпустили, просто удерживаем последнее зафиксированное значение
            smoothedAngle = lastControlledAngle;
        }

        // ОПТИМИЗАЦИЯ 4: Убрали промежуточную переменную angleForControl. Передаем напрямую.
        UpdateGearFromAngle(smoothedAngle);
    }


    void UpdateGearFromAngle(float angle)
    {
        // ШАГ 1: Вычисляем отклонение от нейтрали (-90°)
        // if (debugMode) Debug.Log($"angle = {angle}");
        float deviation = angle - neutralAngle;
        
        // ШАГ 2: Нормализуем отклонение в диапазон [-1, 1]
        // Максимальное отклонение в каждую сторону
        float maxDeviation = Mathf.Max(Mathf.Abs(minAngle - neutralAngle), Mathf.Abs(maxAngle - neutralAngle));
        float normalizedValue = deviation / maxDeviation;
        normalizedValue = Mathf.Clamp(normalizedValue, -1f, 1f);
        
        // ШАГ 3: Проверяем deadZone (в градусах)
        if (Mathf.Abs(deviation) < deadZoneDegrees)
        {
            if (craneEngine.gearNow != 0)
            {
                if (debugMode) Debug.Log($"[VRAdapter] DeadZone: угол={angle:F1}°, отклонение={deviation:F1}° < {deadZoneDegrees}° → нейтраль");
                SetGear(0);
            }
            return;
        }
        
        // ШАГ 4: Применяем инверсию (опционально)
        if (invertDirection)
        {
            normalizedValue = -normalizedValue;
        }
        
        // ШАГ 5: Вычисляем передачу
        int newGear = CalculateGearFromNormalized(normalizedValue);
        newGear = Mathf.Clamp(newGear, craneEngine.gearsBackward, craneEngine.gearsForward);

        if (debugMode && newGear != craneEngine.gearNow)
        {
            Debug.Log($"[VRAdapter] Угол={angle:F1}° (отклонение={deviation:F1}°) → норм={normalizedValue:F2} → передача={newGear}");
        }

        SetGear(newGear);
    }

    private int CalculateGearFromNormalized(float value)
    {
        if (value > 0)
        {
            // Движение в одну сторону (например, вперёд)
            float t = Mathf.Clamp01(value);

            if (t >= 0.9f) return craneEngine.gearsForward;
            if (t >= 0.6f) return Mathf.Max(1, craneEngine.gearsForward - 1);
            if (t >= 0.3f) return 1;
            return 0;
        }
        else if (value < 0)
        {
            // Движение в другую сторону (например, назад)
            float t = Mathf.Clamp01(-value);
            int maxBackAbs = Mathf.Abs(craneEngine.gearsBackward);

            if (t >= 0.9f) return -maxBackAbs;
            if (t >= 0.6f) return -Mathf.Max(1, maxBackAbs - 1);
            if (t >= 0.3f) return -1;
            return 0;
        }

        return 0;
    }

    void SetGear(int newGear)
    {
        if (craneEngine.gearNow == newGear) return;

        int oldGear = craneEngine.gearNow;
        craneEngine.gearNow = newGear;
        TriggerHapticFeedback(oldGear, newGear);

        if (debugMode)
        {
            Debug.Log($"[VRAdapter] Передача: {oldGear} → {newGear}");
        }
    }

    void TriggerHapticFeedback(int oldGear, int newGear)
    {
        if (Time.time - lastHapticTime < hapticCooldown) return;
        if (controllerInteractor == null) return;

        var xrController = controllerInteractor.xrController;
        if (xrController == null) return;

        float gearDelta = Mathf.Abs(newGear - oldGear);
        float dynamicAmplitude = Mathf.Clamp(hapticAmplitude * (0.5f + gearDelta * 0.25f), 0.2f, 1f);

        xrController.SendHapticImpulse(dynamicAmplitude, hapticDuration);
        lastHapticTime = Time.time;
    }

    private float GetLocalLeverAngle()
{
    float rawAngle = 0f;

    // Считываем углы Эйлера (в градусах от 0 до 360)
    switch (trackingAxis)
    {
        case LeverAxis.X: rawAngle = vrLeverTransform.localEulerAngles.x; break;
        case LeverAxis.Y: rawAngle = vrLeverTransform.localEulerAngles.y; break;
        case LeverAxis.Z: rawAngle = vrLeverTransform.localEulerAngles.z; break;
    }
    
    if (debugMode) Debug.Log($"rawAngle (0-360) = {rawAngle}");

    // Mathf.DeltaAngle идеально переводит (например, 350 градусов в -10)
    float correctedRawAngle = Mathf.DeltaAngle(0, rawAngle);
    
    if (debugMode) Debug.Log($"correctedRawAngle (-180 to 180) = {correctedRawAngle}");

    return correctedRawAngle;
}

    public void EmergencyStop()
    {
        lastControlledAngle = neutralAngle;
        hasControlledAngle = true;
        SetGear(0);
        Debug.Log("[VRAdapter] Аварийная остановка");
    }

    void OnDestroy()
    {
        if (leverGrabInteractable != null)
        {
            leverGrabInteractable.selectEntered.RemoveListener(OnLeverGrabbed);
            leverGrabInteractable.selectExited.RemoveListener(OnLeverReleased);
        }
    }
}