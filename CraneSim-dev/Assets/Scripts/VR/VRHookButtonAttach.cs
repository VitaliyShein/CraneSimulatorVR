using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class VRConnectButtonDeviceBased : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private InputHelpers.Button buttonToCheck = InputHelpers.Button.SecondaryButton;
    
    [Header("События")]
    public UnityEvent OnConnectButtonPressed;
    
    private XRController deviceBasedController;
    private bool wasPressed = false;
    
    private void Start()
    {
        deviceBasedController = GetComponent<XRController>();
    }
    
    private void Update()
    {
        if (deviceBasedController == null) return;
        
        // Проверяем, нажата ли кнопка
        bool isPressed = deviceBasedController.inputDevice.IsPressed(buttonToCheck, out bool value, 0.1f);
        
        if (isPressed && !wasPressed)
        {
            OnConnectButtonPressed?.Invoke();
            
            wasPressed = true;
        }
        else if (!isPressed)
        {
            wasPressed = false;
        }
    }
}