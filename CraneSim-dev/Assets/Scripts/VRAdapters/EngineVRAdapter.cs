using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRAdapter : MonoBehaviour
{
    [Header("Связанные компоненты")]
    public Engine craneEngine;
    public Transform vrLeverTransform;

    [Header("Настройки углов рычага")]
    public float maxLeverAngle = 45f;
    [Range(0f, 0.3f)] public float deadZone = 0.1f;

    private enum LeverAxis { X, Y, Z }
    [SerializeField] private LeverAxis trackingAxis = LeverAxis.X;

    // Внутренние переменные для жесткой фиксации
    private Vector3 initialLocalPosition;
    private Transform originalParent;

    void Awake()
    {
        if (vrLeverTransform != null)
        {
            // Запоминаем стартовую локальную позицию внутри кабины
            initialLocalPosition = vrLeverTransform.localPosition;
            originalParent = vrLeverTransform.parent;
        }
    }

    // Используем LateUpdate, так как XR Toolkit двигает объекты в Update/FixedUpdate.
    // Мы перехватываем позицию в самый последний момент перед рендерингом кадра.
    void LateUpdate()
    {
        if (craneEngine == null || vrLeverTransform == null) return;

        // ЖЕСТКАЯ КОРРЕКЦИЯ: Возвращаем рычаг в кабину, если XR Toolkit сменил ему родителя во время хвата
        if (vrLeverTransform.parent != originalParent)
        {
            vrLeverTransform.SetParent(originalParent, false);
        }

        // Принудительно удерживаем рычаг в его локальном гнезде (не даем смещаться по осям X, Y, Z)
        vrLeverTransform.localPosition = initialLocalPosition;

        // Расчет передач (остается прежним)
        float currentAngle = GetLocalLeverAngle();
        float normalizedValue = Mathf.Clamp(currentAngle / maxLeverAngle, -1f, 1f);

        if (Mathf.Abs(normalizedValue) < deadZone)
        {
            craneEngine.gearNow = 0;
            return;
        }

        if (normalizedValue > 0)
        {
            craneEngine.gearNow = Mathf.RoundToInt(Mathf.Lerp(1, craneEngine.gearsForward, normalizedValue));
        }
        else
        {
            craneEngine.gearNow = Mathf.RoundToInt(Mathf.Lerp(-1, craneEngine.gearsBackward, Mathf.Abs(normalizedValue)));
        }
    }

    private float GetLocalLeverAngle()
    {
        float rawAngle = 0f;
        switch (trackingAxis)
        {
            case LeverAxis.X: rawAngle = vrLeverTransform.localEulerAngles.x; break;
            case LeverAxis.Y: rawAngle = vrLeverTransform.localEulerAngles.y; break;
            case LeverAxis.Z: rawAngle = vrLeverTransform.localEulerAngles.z; break;
        }

        if (rawAngle > 180f) rawAngle -= 360f;
        return rawAngle;
    }
}
