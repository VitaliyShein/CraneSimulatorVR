using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LeverWithLimit : XRBaseInteractable
{
    [Header("Lever Settings")]
    [SerializeField] private float maxForwardAngle = 30f;  // наклон вперёд
    [SerializeField] private float maxBackAngle = -30f;    // наклон назад
    [SerializeField] private float grabRotationSpeed = 5f;
    
    private Quaternion initialRotation;
    private Transform attachTransform;
    private XRBaseInteractor currentInteractor;
    private bool isGrabbed = false;
    
    protected override void Awake()
    {
        base.Awake();
        initialRotation = transform.localRotation;
    }
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentInteractor = args.interactorObject as XRBaseInteractor;
        attachTransform = currentInteractor.GetAttachTransform(this);
        isGrabbed = true;
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        isGrabbed = false;
        currentInteractor = null;
    }
    
    private void Update()
    {
        if (!isGrabbed || attachTransform == null) return;
        
        // Получаем направление от нижней точки ( pivot ) к руке
        Vector3 pivotPoint = transform.position + transform.up * -0.5f; // нижняя точка
        Vector3 handDirection = (attachTransform.position - pivotPoint).normalized;
        
        // Проекция на плоскость движения ( XZ вперёд-назад )
        float forwardDot = Vector3.Dot(handDirection, transform.parent.forward);
        
        // Переводим в угол
        float targetAngle = Mathf.Lerp(maxBackAngle, maxForwardAngle, (forwardDot + 1f) / 2f);
        targetAngle = Mathf.Clamp(targetAngle, maxBackAngle, maxForwardAngle);
        
        // Применяем вращение
        Quaternion targetRotation = initialRotation * Quaternion.Euler(targetAngle, 0, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, grabRotationSpeed * Time.deltaTime);
    }
    
    // Опционально: вывод текущего угла для анимаций
    public float GetNormalizedAngle()
    {
        float angle = transform.localEulerAngles.x;
        if (angle > 180) angle -= 360;
        return Mathf.InverseLerp(maxBackAngle, maxForwardAngle, angle);
    }
}