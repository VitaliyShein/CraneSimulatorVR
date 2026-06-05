using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerButtonToKeyM : MonoBehaviour
{
    [Header("Настройки контроллера")]
    [Tooltip("Action-based controller (обычно на дочернем объекте)")]
    public ActionBasedController actionBasedController;
    
    [Tooltip("Какая кнопка будет вызывать симуляцию")]
    public string buttonToWatch = "select"; // select, activate, uiPress
    
    [Tooltip("Симулировать при нажатии (true) или при отпускании (false)")]
    public bool simulateOnPress = true;
    
    [Header("Симуляция")]
    [Tooltip("Симулировать как Event (для Unity UI)")]
    public bool simulateAsEvent = true;
    
    [Tooltip("Симулировать как SendKeys (только Windows)")]
    public bool simulateAsSendKeys = false;
    
    [Tooltip("Вызывать метод напрямую (рекомендуется)")]
    public bool callMethodDirectly = true;
    
    [Header("Целевой объект и метод")]
    [Tooltip("Объект с методом для вызова")]
    public GameObject targetObject;
    
    [Tooltip("Имя метода для вызова (например, ToggleGrab)")]
    public string methodName = "ToggleGrab";
    
    [Header("Отладка")]
    public bool debugMode = true;
    
    private InputAction targetAction;
    private bool lastPressedState = false;
    
    void Start()
    {
        // Ищем ActionBasedController если не назначен
        if (actionBasedController == null)
            actionBasedController = GetComponent<ActionBasedController>();
        
        if (actionBasedController == null)
            actionBasedController = GetComponentInParent<ActionBasedController>();
        
        if (actionBasedController == null)
        {
            Debug.LogError("ActionBasedController не найден! Пожалуйста, назначьте его в инспекторе.");
            return;
        }
        
        // Получаем нужный InputAction в зависимости от выбранной кнопки
        switch (buttonToWatch.ToLower())
        {
            case "select":
                targetAction = actionBasedController.selectAction.action;
                break;
            case "activate":
                targetAction = actionBasedController.activateAction.action;
                break;
            case "uipress":
                targetAction = actionBasedController.uiPressAction.action;
                break;
            default:
                Debug.LogWarning($"Неизвестная кнопка: {buttonToWatch}, используем select");
                targetAction = actionBasedController.selectAction.action;
                break;
        }
        
        if (debugMode)
            Debug.Log($"Слежу за кнопкой: {buttonToWatch}, Action: {targetAction?.name}");
    }
    
    void Update()
    {
        if (targetAction == null) return;
        
        // Читаем состояние кнопки
        bool isPressed = targetAction.IsPressed();
        
        // Проверяем изменение состояния
        if (simulateOnPress)
        {
            // Симуляция при нажатии
            if (isPressed && !lastPressedState)
                OnButtonPressed();
        }
        else
        {
            // Симуляция при отпускании
            if (!isPressed && lastPressedState)
                OnButtonPressed();
        }
        
        lastPressedState = isPressed;
    }
    
    void OnButtonPressed()
    {
        if (debugMode)
            Debug.Log($"Кнопка {buttonToWatch} нажата! Симулируем M");
        
        // Способ 1: Симуляция через Event (работает с Unity UI)
        if (simulateAsEvent)
        {
            SimulateKeyMAsEvent();
        }
        
        // Способ 2: Симуляция через SendKeys (только Windows)
        if (simulateAsSendKeys)
        {
            SimulateKeyMAsSendKeys();
        }
        
        // Способ 3: Прямой вызов метода (рекомендуется)
        if (callMethodDirectly && targetObject != null)
        {
            targetObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void SimulateKeyMAsEvent()
    {
        // Создаем событие нажатия клавиши M
        Event keyEvent = new Event
        {
            type = EventType.KeyDown,
            keyCode = KeyCode.M
        };
        
        // Отправляем событие в активное окно
        Event.KeyboardEvent("m");
        
        if (debugMode)
            Debug.Log("Симулировано нажатие клавиши M через Event");
    }
    
    void SimulateKeyMAsSendKeys()
    {
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // Только для Windows сборки
        SendKeys.Send("m");
        #endif
        
        if (debugMode)
            Debug.Log("Симулировано нажатие клавиши M через SendKeys");
    }
}

// Extension метод для проверки IsPressed у InputAction
public static class InputActionExtensions
{
    public static bool IsPressed(this InputAction action)
    {
        if (action == null) return false;
        
        #if INPUT_SYSTEM_1_1_OR_NEWER
        return action.phase == InputActionPhase.Performed;
        #else
        return action.triggered || action.phase == InputActionPhase.Performed;
        #endif
    }
}