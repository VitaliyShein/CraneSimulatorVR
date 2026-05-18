using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Experimental.UIElements;
using Image = UnityEngine.UI.Image;

public class script_UI_start : MonoBehaviour
{
    public GameObject pnlStart;
    public GameObject pnlHelp;
    public GameObject btnHelpShow;
    public GameObject btnHelpHide;
    public Image[] elements;
    public float blur = 2f;
    public Material sh;
    private bool isBlur = false;
    
    public void StartBlurOffPanel()
    {
        isBlur = true;
    }
    
    public void ShowHelpPanel()
    {
        btnHelpShow.SetActive(false);
        btnHelpHide.SetActive(true);
        pnlHelp.SetActive(true);
    }
    
    public void HideHelpPanel()
    {
        btnHelpShow.SetActive(true);
        btnHelpHide.SetActive(false);
        pnlHelp.SetActive(false);
    }

    private void Start()
    {
        sh.SetFloat("_Factor", 2);
    }

    private void FixedUpdate()
    {
        if (isBlur && blur > 0f)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var col = elements[i].color;
                col.a -= 0.05f;
                elements[i].color = col;
            }
            blur -= 0.1f;
            sh.SetFloat("_Factor", blur);
            if (blur <= 0f)
            {
                pnlStart.SetActive(false);
                btnHelpShow.SetActive(true);
                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i].gameObject.SetActive(false);
                }
                isBlur = false;
            }
        }
    }
}
