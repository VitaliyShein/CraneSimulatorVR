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

    private float neutralAngle;
    private float smoothedAngle;
    private bool wasGrabbedLastFrame = false;

    // Для сглаживания скачков
    private float previousRawAngle = 0f;
    private float continuousAngle = 0f;

    void Start()
    {
        if (craneEngine != null)
            lastGear = craneEngine.gearNow;

        neutralAngle = (minAngle + maxAngle) / 2f;
        smoothedAngle = neutralAngle;
        previousRawAngle = neutralAngle;
        continuousAngle = neutralAngle;

        if (debugMode)
        {
            Debug.Log($"[VRAdapter] Диапазон: {minAngle}° до {maxAngle}°, нейтраль: {neutralAngle}°");
            Debug.Log($"[VRAdapter] Отслеживаемая ось: {trackingAxis}");
        }

        if (leverGrabInteractable != null)
        {
            leverGrabInteractable.selectEntered.AddListener(OnLeverGrabbed);
            leverGrabInteractable.selectExited.AddListener(OnLeverReleased);
        }
    }

    void OnLeverGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        // При захвате сразу получаем текущий угол
        smoothedAngle = GetLocalLeverAngle();
        continuousAngle = smoothedAngle;
        previousRawAngle = smoothedAngle;
        if (debugMode) Debug.Log("[VRAdapter] Рычаг захвачен");
    }

    void OnLeverReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (debugMode) Debug.Log("[VRAdapter] Рычаг отпущен");
    }

    void Update()
    {
        if (craneEngine == null || vrLeverTransform == null) return;

        if (isGrabbed)
        {
            float currentAngle = GetLocalLeverAngle();
            
            // Устраняем скачки угла
            currentAngle = UnwrapAngle(currentAngle);
            
            // Ограничиваем диапазоном
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            
            // Сглаживание
            smoothedAngle = Mathf.Lerp(smoothedAngle, currentAngle, Time.deltaTime * 15f);
            lastControlledAngle = smoothedAngle;
            
            if (debugMode && Time.frameCount % 60 == 0)
            {
                Debug.Log($"Angle: {smoothedAngle:F1}°");
            }
        }
        else
        {
            if (!hasControlledAngle) return;
            smoothedAngle = lastControlledAngle;
        }

        UpdateGearFromAngle(smoothedAngle);
    }

    /// <summary>
    /// Получает локальный угол рычага по выбранной оси
    /// </summary>
    private float GetLocalLeverAngle()
    {
        if (vrLeverTransform == null) return neutralAngle;

        float rawAngle = 0f;

        // Считываем угол в зависимости от выбранной оси
        switch (trackingAxis)
        {
            case LeverAxis.X:
                rawAngle = vrLeverTransform.localEulerAngles.x;
                break;
            case LeverAxis.Y:
                rawAngle = vrLeverTransform.localEulerAngles.y;
                break;
            case LeverAxis.Z:
                rawAngle = vrLeverTransform.localEulerAngles.z;
                break;
        }

        // Конвертируем из диапазона 0-360 в диапазон -180..180
        float normalizedAngle = rawAngle;
        if (normalizedAngle > 180f)
            normalizedAngle -= 360f;

        if (debugMode && isGrabbed && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Raw: {rawAngle:F1}° → Normalized: {normalizedAngle:F1}°");
        }

        return normalizedAngle;
    }

    /// <summary>
    /// Устраняет скачки угла (например, переход с 179° на -179°)
    /// </summary>
    private float UnwrapAngle(float rawAngle)
    {
        float delta = rawAngle - previousRawAngle;
        
        // Если разница больше 180°, значит был скачок через границу
        if (delta > 180f)
            delta -= 360f;
        else if (delta < -180f)
            delta += 360f;
        
        continuousAngle += delta;
        previousRawAngle = rawAngle;
        
        return continuousAngle;
    }

    void UpdateGearFromAngle(float angle)
    {
        float deviation = angle - neutralAngle;

        // Нормализуем с учетом разных диапазонов
        float maxDeviationPositive = maxAngle - neutralAngle;
        float maxDeviationNegative = neutralAngle - minAngle;

        float normalizedValue;
        if (deviation >= 0)
        {
            normalizedValue = maxDeviationPositive > 0 ? deviation / maxDeviationPositive : 0;
        }
        else
        {
            normalizedValue = maxDeviationNegative > 0 ? deviation / maxDeviationNegative : 0;
        }

        normalizedValue = Mathf.Clamp(normalizedValue, -1f, 1f);

        // Мертвая зона
        if (Mathf.Abs(deviation) < deadZoneDegrees)
        {
            if (craneEngine.gearNow != 0)
            {
                if (debugMode) Debug.Log($"[VRAdapter] DeadZone → нейтраль");
                SetGear(0);
            }
            return;
        }

        if (invertDirection)
        {
            normalizedValue = -normalizedValue;
        }

        int newGear = CalculateGearFromNormalized(normalizedValue);
        newGear = Mathf.Clamp(newGear, craneEngine.gearsBackward, craneEngine.gearsForward);

        if (debugMode && newGear != craneEngine.gearNow)
        {
            Debug.Log($"[VRAdapter] angle={angle:F1}° → norm={normalizedValue:F3} → gear={newGear}");
        }

        SetGear(newGear);
    }

    private int CalculateGearFromNormalized(float value)
    {
        if (value > 0.05f)
        {
            float t = Mathf.Clamp01(value);
            if (t >= 0.9f) return craneEngine.gearsForward;
            if (t >= 0.6f) return Mathf.Max(1, craneEngine.gearsForward - 1);
            if (t >= 0.3f) return 1;
            return 0;
        }
        else if (value < -0.05f)
        {
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
        if (debugMode) Debug.Log($"[VRAdapter] Передача: {oldGear} → {newGear}");
    }

    void TriggerHapticFeedback(int oldGear, int newGear)
    {
        if (!isGrabbed) return;
        if (Time.time - lastHapticTime < hapticCooldown) return;
        if (controllerInteractor == null) return;

        var xrController = controllerInteractor.xrController;
        if (xrController == null) return;

        float gearDelta = Mathf.Abs(newGear - oldGear);
        float dynamicAmplitude = Mathf.Clamp(hapticAmplitude * (0.5f + gearDelta * 0.25f), 0.2f, 1f);
        xrController.SendHapticImpulse(dynamicAmplitude, hapticDuration);
        lastHapticTime = Time.time;
    }

    public void EmergencyStop()
    {
        smoothedAngle = neutralAngle;
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