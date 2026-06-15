using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit; // Обязательно добавляем этот неймспейс

public class VROpenMenu : MonoBehaviour
{
    [Header("Меню")]
    public Pause_menu menuPrefab;

    [Header("Компоненты луча")]
    // Перетащите сюда ваш XR Ray Interactor с того же контроллера
    public XRRayInteractor rayInteractor;
    // Перетащите сюда XR Interactor Line Visual (визуализацию луча)
    public XRInteractorLineVisual lineVisual;

    private bool isActive = false;

    [Header("Ввод")]
    public InputActionReference menuAction;

    private void OnEnable() => menuAction.action.performed += ToggleMenu;
    private void OnDisable() => menuAction.action.performed -= ToggleMenu;

    private void Start()
    {
        // При старте игры проверяем состояние меню и настраиваем луч
        UpdateRayState(Pause_menu.GameIsPaused);
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        isActive = !isActive; // Переключаем состояние меню

        if (isActive == true)
            menuPrefab.Pause();
        else
            menuPrefab.Resume();

        // Включаем или выключаем луч в зависимости от того, открыто ли меню
        UpdateRayState(isActive);
    }

    private void UpdateRayState(bool isMenuOpen)
    {
        if (rayInteractor != null) rayInteractor.enabled = isMenuOpen;
        if (lineVisual != null) lineVisual.enabled = isMenuOpen;
    }
}
