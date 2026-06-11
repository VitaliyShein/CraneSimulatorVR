using UnityEngine;
using UnityEngine.InputSystem;

public class VRHookAction : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private RopeConnect ropeConnectScript;

    [Header("Ввод VR (Input System)")]
    [Tooltip("Перетащите сюда нужный InputActionReference (например XRI LeftHand/Activate или Custom)")]
    [SerializeField] private InputActionReference connectButtonAction;

    private void OnEnable()
    {
        if (connectButtonAction != null && connectButtonAction.action != null)
        {
            connectButtonAction.action.performed += OnButtonPressed;
        }
    }

    private void OnDisable()
    {
        if (connectButtonAction != null && connectButtonAction.action != null)
        {
            connectButtonAction.action.performed -= OnButtonPressed;
        }
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        if (ropeConnectScript != null)
        {
            // Вызываем логику сцепки/расцепки в основном скрипте
            ropeConnectScript.OnConnectButtonPressed();
        }
        else
        {
            Debug.LogWarning("VRHookAction: Не назначена ссылка на скрипт RopeConnect!");
        }
    }
}
