using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

// ========== ИНТЕРФЕЙС ДОЛЖЕН БЫТЬ ОПРЕДЕЛЁН ДО ЕГО ИСПОЛЬЗОВАНИЯ ==========
public interface IVRInputReceiver
{
    void OnVRInputUpdate(bool primaryPressed, bool secondaryPressed, float leftTrigger, float rightTrigger);
}
// ============================================================================

public class VRInputManager : MonoBehaviour
{
    [Header("=== ССЫЛКИ НА КОМПОНЕНТЫ ===")]
    public List<MonoBehaviour> vrAdapters = new List<MonoBehaviour>();
    
    [Header("=== НАСТРОЙКИ ===")]
    public bool debugMode = true;
    
    public static VRInputManager Instance { get; private set; }
    
    // Состояния кнопок
    public bool IsPrimaryButtonPressed { get; private set; }
    public bool IsSecondaryButtonPressed { get; private set; }
    public bool IsGripPressed { get; private set; }
    public float LeftTriggerValue { get; private set; }
    public float RightTriggerValue { get; private set; }
    
    // События
    public System.Action OnPrimaryButtonPressed;
    public System.Action OnSecondaryButtonPressed;
    
    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private bool lastPrimaryState = false;
    private bool lastSecondaryState = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        FindVRAdapters();
    }
    
    void Update()
    {
        // Получаем устройства
        if (!leftDevice.isValid)
            leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (!rightDevice.isValid)
            rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        UpdateControllerInput();
        BroadcastInputToAdapters();
    }
    
    void FindVRAdapters()
    {
        // Находим все компоненты на этом объекте
        var adapters = GetComponents<MonoBehaviour>();
        foreach (var adapter in adapters)
        {
            if (adapter != this && !vrAdapters.Contains(adapter))
            {
                vrAdapters.Add(adapter);
            }
        }
        
        if (debugMode)
            Debug.Log($"Найдено VR адаптеров: {vrAdapters.Count}");
    }
    
    void UpdateControllerInput()
    {
        bool prevPrimary = IsPrimaryButtonPressed;
        bool prevSecondary = IsSecondaryButtonPressed;
        
        // Проверяем кнопки
        IsPrimaryButtonPressed = IsButtonPressed(CommonUsages.primaryButton);
        IsSecondaryButtonPressed = IsButtonPressed(CommonUsages.secondaryButton);
        IsGripPressed = IsButtonPressed(CommonUsages.gripButton);
        
        // Триггеры
        leftDevice.TryGetFeatureValue(CommonUsages.trigger, out float leftTrigger);
        rightDevice.TryGetFeatureValue(CommonUsages.trigger, out float rightTrigger);
        LeftTriggerValue = leftTrigger;
        RightTriggerValue = rightTrigger;
        
        // События
        if (!prevPrimary && IsPrimaryButtonPressed)
        {
            OnPrimaryButtonPressed?.Invoke();
            if (debugMode) Debug.Log("Primary button pressed");
        }
        
        if (!prevSecondary && IsSecondaryButtonPressed)
        {
            OnSecondaryButtonPressed?.Invoke();
            if (debugMode) Debug.Log("Secondary button pressed");
        }
    }
    
    bool IsButtonPressed(InputFeatureUsage<bool> button)
    {
        bool leftPressed = false, rightPressed = false;
        
        if (leftDevice.isValid)
            leftDevice.TryGetFeatureValue(button, out leftPressed);
        if (rightDevice.isValid)
            rightDevice.TryGetFeatureValue(button, out rightPressed);
        
        return leftPressed || rightPressed;
    }
    
    void BroadcastInputToAdapters()
    {
        foreach (var adapter in vrAdapters)
        {
            if (adapter != null && adapter is IVRInputReceiver receiver)
            {
                receiver.OnVRInputUpdate(IsPrimaryButtonPressed, IsSecondaryButtonPressed, LeftTriggerValue, RightTriggerValue);
            }
        }
    }
    
    // Публичный метод для проверки кнопки из других скриптов
    public bool IsButtonPressed(string buttonName)
    {
        switch (buttonName.ToLower())
        {
            case "primary": return IsPrimaryButtonPressed;
            case "secondary": return IsSecondaryButtonPressed;
            case "grip": return IsGripPressed;
            case "lefttrigger": return LeftTriggerValue > 0.5f;
            case "righttrigger": return RightTriggerValue > 0.5f;
            default: return false;
        }
    }
    
    // Получить значение триггера
    public float GetTrigger(string hand)
    {
        switch (hand.ToLower())
        {
            case "left": return LeftTriggerValue;
            case "right": return RightTriggerValue;
            default: return 0f;
        }
    }
}