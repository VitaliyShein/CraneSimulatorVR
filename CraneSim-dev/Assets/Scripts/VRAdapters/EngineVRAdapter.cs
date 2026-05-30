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
    [Tooltip("Опорный объект для расчета угла (например, корпус крана)")]
    public Transform referenceObject; // Объект, относительно которого считаем угол

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

    // Для плавности
    private float smoothedAngle;
    private bool wasGrabbedLastFrame = false;

    // Храним начальное направление рычага
    private Vector3 initialLeverDirection;
    private Quaternion initialLeverRotation;

    void Start()
    {
        if (craneEngine != null)
            lastGear = craneEngine.gearNow;

        neutralAngle = (minAngle + maxAngle) / 2f;

        // Если нет referenceObject, создаем виртуальный
        if (referenceObject == null)
        {
            referenceObject = new GameObject($"[VRAdapter] Reference_{gameObject.name}").transform;
            referenceObject.SetParent(vrLeverTransform.parent);
            referenceObject.position = vrLeverTransform.position;
            referenceObject.rotation = Quaternion.identity;
            if (debugMode) Debug.Log($"Создан виртуальный referenceObject для {gameObject.name}");
        }

        // Сохраняем начальное направление рычага
        if (vrLeverTransform != null)
        {
            initialLeverRotation = vrLeverTransform.localRotation;
            initialLeverDirection = GetLeverDirection();
            if (debugMode) Debug.Log($"Начальное направление рычага: {initialLeverDirection}");
        }

        if (debugMode)
        {
            Debug.Log($"[VRAdapter] Диапазон: {minAngle}° до {maxAngle}°, нейтраль: {neutralAngle}°");
            Debug.Log($"[VRAdapter] Reference object: {(referenceObject != null ? referenceObject.name : "null")}");
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
            float currentAngle = GetAngleRelativeToReference();

            if (!wasGrabbedLastFrame)
            {
                smoothedAngle = currentAngle;
                wasGrabbedLastFrame = true;
                hasControlledAngle = true;
            }
            else
            {
                smoothedAngle = Mathf.Lerp(smoothedAngle, currentAngle, Time.deltaTime * 10f);
            }

            lastControlledAngle = smoothedAngle;
        }
        else
        {
            wasGrabbedLastFrame = false;
            if (!hasControlledAngle) return;
            smoothedAngle = lastControlledAngle;
        }

        UpdateGearFromAngle(smoothedAngle);
    }

    /// <summary>
    /// Рассчитывает угол между текущим положением рычага и опорным объектом
    /// </summary>
    private float GetAngleRelativeToReference()
    {
        if (referenceObject == null || vrLeverTransform == null)
        {
            Debug.LogError("ReferenceObject или vrLeverTransform не назначен!");
            return neutralAngle;
        }

        // Получаем направления
        Vector3 leverDirection = GetLeverDirection();
        Vector3 referenceDirection = GetReferenceDirection();

        // Рассчитываем угол между векторами
        float angle = Vector3.SignedAngle(referenceDirection, leverDirection, GetRotationAxis());

        // Нормализуем угол в диапазон -180..180
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;

        if (debugMode && isGrabbed && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Lever Dir: {leverDirection}, Ref Dir: {referenceDirection}, Angle: {angle:F1}°");
        }

        return angle;
    }

    /// <summary>
    /// Получает направление рычага в локальном пространстве
    /// </summary>
    private Vector3 GetLeverDirection()
    {
        // По умолчанию используем forward, но можно настроить под конкретный рычаг
        return vrLeverTransform.localRotation * Vector3.forward;
    }

    /// <summary>
    /// Получает опорное направление (например, вниз или в сторону)
    /// </summary>
    private Vector3 GetReferenceDirection()
    {
        // По умолчанию используем направление опорного объекта
        // Для вертикального рычага это может быть Vector3.down
        return referenceObject.rotation * Vector3.down;
    }

    /// <summary>
    /// Определяет ось вращения для SignedAngle
    /// </summary>
    private Vector3 GetRotationAxis()
    {
        return trackingAxis switch
        {
            LeverAxis.X => Vector3.right,
            LeverAxis.Y => Vector3.up,
            LeverAxis.Z => Vector3.forward,
            _ => Vector3.right
        };
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