using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_menu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public static bool ControlsView = false;
    public static bool HelpView = false;
    public GameObject pauseMenuUI;
    public GameObject controlsUI;
    public GameObject gamePanelUI;
    public GameObject rightHelpPanelUI;
    public GameObject leftHelpPanelUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !ControlsView)
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && ControlsView)
        {
            BackToPauseMenu();
        }
    }

    public void Resume()
    {
        gamePanelUI.SetActive(true);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        gamePanelUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Controls()
    {
        pauseMenuUI.SetActive(false);
        controlsUI.SetActive(true);
        ControlsView = true;
    }

    public void BackToPauseMenu()
    {
        pauseMenuUI.SetActive(true);
        controlsUI.SetActive(false);
        ControlsView = false;
    }

    public void OpenHelpPanel()
    {
        if (!HelpView)
        {
            rightHelpPanelUI.SetActive(true);
            leftHelpPanelUI.SetActive(true);
            HelpView = true;
        }
        else
        {
            rightHelpPanelUI.SetActive(false);
            leftHelpPanelUI.SetActive(false);
            HelpView = false;
        }
    }
}
