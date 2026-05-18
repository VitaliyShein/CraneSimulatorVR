using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerHandler : MonoBehaviour
{
    // обработчик штрафов
    private PenaltyHandler ph;

    // найдены ли необходимые компоненты
    private bool isFindedComponent;

    // отведенное на упражнение время
    private float allotedTime;
    // дельта времени
    private float cooldownTime = 0.1f;
    // флаг таймера
    private bool ready = true;

    void Start()
    {
        try
        {
            ph = GameObject.Find("PenaltyHandler").GetComponent<PenaltyHandler>();
            allotedTime = ph.allotedTime;
            isFindedComponent = true;
        }
        catch
        {
            isFindedComponent = false;
        }
    }

    void Update()
    {
        // старт таймера (можно добавить какое-либо условие)
        if (isFindedComponent)
        {
            if (ph.timerFlag && ready)
            {
                StartCoroutine(CooldownHandler());
            }
        }
    }

    IEnumerator CooldownHandler()
    {
        ready = false;
        if (allotedTime > 0)
        {
            //Debug.Log(allotedTime.ToString("F1"));
            allotedTime -= cooldownTime;
            
            yield return new WaitForSeconds(cooldownTime);
            ready = true;
        }
        else
        {
            //Debug.Log(allotedTime.ToString("F1"));
            allotedTime -= cooldownTime;
            ph.penaltyTime += cooldownTime;

            yield return new WaitForSeconds(cooldownTime);
            ready = true;
        }
    }
}
