using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CraneVR
{
    /// <summary>
    /// Виртуальный рычаг управления краном.
    /// При захвате и перемещении рычага вычисляется значение от -1 до 1.
    /// </summary>
    public class LeverInteractable : XRGrabInteractable
    {
        [Header("Lever Settings")]
        [Tooltip("Точка поворота рычага (обычно родительский объект)")]
        [SerializeField] private Transform leverPivot;

        [Tooltip("Ось вращения (X, Y или Z)")]
        [SerializeField] private Axis rotationAxis = Axis.X;

        [Tooltip("Угол в нейтральном положении (градусы)")]
        [SerializeField] private float neutralAngle = 0f;

        [Tooltip("Максимальный угол отклонения (градусы)")]
        [SerializeField] private float maxAngle = 45f;

        [Tooltip("Инвертировать направление")]
        [SerializeField] private bool invert = false;

        [Header("Output")]
        [SerializeField] private float outputValue = 0f;

        [Header("Haptics")]
        [SerializeField] private float hapticAmplitude = 0.5f;
        [SerializeField] private float hapticDuration = 0.1f;

        [Header("Return to Neutral")]
        [SerializeField] private bool returnToNeutralOnRelease = true;
        [SerializeField] private float returnSpeed = 90f;

        private XRBaseInteractor currentInteractor;
        private bool isGrabbed = false;
        private float currentAngleNormalized = 0f;
        private float targetAngleNormalized = 0f;

        public System.Action<float> OnValueChanged;
        public float OutputValue => outputValue;
        public bool IsGrabbed => isGrabbed;

        private enum Axis { X, Y, Z }

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(OnGrabbed);
            selectExited.AddListener(OnReleased);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(OnGrabbed);
            selectExited.RemoveListener(OnReleased);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            currentInteractor = args.interactorObject as XRBaseInteractor;
            isGrabbed = true;
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            isGrabbed = false;
            currentInteractor = null;

            if (returnToNeutralOnRelease)
            {
                targetAngleNormalized = 0f;
            }
        }

        private void Update()
        {
            if (isGrabbed)
            {
                UpdateFromGrab();
            }
            else if (returnToNeutralOnRelease)
            {
                UpdateReturnToNeutral();
            }

            ApplyLeverRotation();
        }

        private void UpdateFromGrab()
        {
            if (leverPivot == null) return;

            Vector3 currentRotation = leverPivot.localEulerAngles;
            float rawAngle = GetAxisValue(currentRotation);
            if (rawAngle > 180f) rawAngle -= 360f;

            float angleFromNeutral = rawAngle - neutralAngle;
            angleFromNeutral = Mathf.Clamp(angleFromNeutral, -maxAngle, maxAngle);

            float newValue = angleFromNeutral / maxAngle;
            if (invert) newValue = -newValue;

            targetAngleNormalized = newValue;
            outputValue = Mathf.Lerp(outputValue, targetAngleNormalized, Time.deltaTime * 10f);

            if (Mathf.Abs(outputValue) > 0.95f && Mathf.Abs(targetAngleNormalized) > 0.95f)
            {
                SendHapticFeedback();
            }

            OnValueChanged?.Invoke(outputValue);
        }

        private void UpdateReturnToNeutral()
        {
            targetAngleNormalized = Mathf.MoveTowards(targetAngleNormalized, 0f, returnSpeed * Time.deltaTime / maxAngle);
            outputValue = Mathf.Lerp(outputValue, targetAngleNormalized, Time.deltaTime * 10f);
            OnValueChanged?.Invoke(outputValue);
        }

        private void ApplyLeverRotation()
        {
            if (leverPivot == null) return;

            float targetAngle = neutralAngle + targetAngleNormalized * maxAngle;
            if (invert) targetAngle = neutralAngle - targetAngleNormalized * maxAngle;

            Vector3 newRotation = leverPivot.localEulerAngles;
            SetAxisValue(ref newRotation, targetAngle);
            leverPivot.localRotation = Quaternion.Euler(newRotation);
        }

        private float GetAxisValue(Vector3 rotation)
        {
            switch (rotationAxis)
            {
                case Axis.X: return rotation.x;
                case Axis.Y: return rotation.y;
                case Axis.Z: return rotation.z;
                default: return rotation.x;
            }
        }

        private void SetAxisValue(ref Vector3 rotation, float value)
        {
            switch (rotationAxis)
            {
                case Axis.X: rotation.x = value; break;
                case Axis.Y: rotation.y = value; break;
                case Axis.Z: rotation.z = value; break;
            }
        }

        private void SendHapticFeedback()
        {
            if (currentInteractor != null)
            {
                var controller = currentInteractor.GetComponent<XRBaseControllerInteractor>();
                if (controller != null && controller.xrController != null)
                {
                    controller.xrController.SendHapticImpulse(hapticAmplitude, hapticDuration);
                }
            }
        }
    }
}