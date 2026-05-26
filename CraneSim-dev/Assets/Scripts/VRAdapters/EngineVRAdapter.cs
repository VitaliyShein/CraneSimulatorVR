using UnityEngine;

public class VRAdapter : MonoBehaviour
{
    [Header("Связанные компоненты")]
    [Tooltip("Ссылка на скрипт Engine конкретного механизма крана")]
    public Engine craneEngine;

    [Tooltip("Трансформ 3D-модели рычага в кабине, который крутит игрок в VR")]
    public Transform vrLeverTransform;

    [Header("Настройки углов рычага")]
    [Tooltip("Максимальный угол отклонения рычага в градусах от центральной точки (например, 45)")]
    public float maxLeverAngle = 45f;

    [Tooltip("Мертвая зона вокруг нуля в процентах (0.0 - 1.0), чтобы рычаг не дергался по центру")]
    [Range(0f, 0.3f)]
    public float deadZone = 0.2f;

    private enum LeverAxis { X, Y, Z }
    [SerializeField]
    [Tooltip("Ось локального вращения 3D-модели рычага для отслеживания наклона")]
    private LeverAxis trackingAxis = LeverAxis.X;

    void Update()
    {
        if (craneEngine == null || vrLeverTransform == null) return;

        // 1. Получаем локальный угол поворота рычага
        float currentAngle = GetLocalLeverAngle();

        // 2. Нормализуем угол в диапазон от -1.0 до 1.0
        float normalizedValue = currentAngle / maxLeverAngle;
        normalizedValue = Mathf.Clamp(normalizedValue, -1f, 1f);

        // 3. Применяем мертвую зону для фиксации нейтральной передачи (0)
        if (Mathf.Abs(normalizedValue) < deadZone)
        {
            craneEngine.gearNow = 0;
            return;
        }

        // 4. Масштабируем значение под количество передач крана вперед/назад
        if (normalizedValue < 0)
        {
            // Рычаг отклонен вперед. Интерполируем между 1 и максимальной передней передачей
            craneEngine.gearNow = Mathf.RoundToInt(Mathf.Lerp(1, craneEngine.gearsForward, normalizedValue));
        }

        if (normalizedValue > 0)
        {
            // Рычаг отклонен назад. Интерполируем между -1 и максимальной задней передачей
            // Значение normalizedValue здесь отрицательное, поэтому используем Mathf.Abs
            craneEngine.gearNow = Mathf.RoundToInt(Mathf.Lerp(-1, craneEngine.gearsBackward, Mathf.Abs(normalizedValue)));
        }
    }

    /// <summary>
    /// Возвращает угол наклона рычага в диапазоне от -180 до 180 градусов
    /// </summary>
    private float GetLocalLeverAngle()
    {
        float rawAngle = 0f;

        // Считываем углы Эйлера в зависимости от того, как сориентирована модель рычага
        switch (trackingAxis)
        {
            case LeverAxis.X: rawAngle = vrLeverTransform.localEulerAngles.x; break;
            case LeverAxis.Y: rawAngle = vrLeverTransform.localEulerAngles.y; break;
            case LeverAxis.Z: rawAngle = vrLeverTransform.localEulerAngles.z; break;
        }

        // Переводим из формата 0..360 в формат -180..180 для правильного определения направления наклона
        if (rawAngle > 180f) rawAngle -= 360f;

        return rawAngle;
    }
}
