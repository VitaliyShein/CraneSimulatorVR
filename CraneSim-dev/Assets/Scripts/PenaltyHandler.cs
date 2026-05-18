using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenaltyHandler : MonoBehaviour
{
    // штрафы
    public bool groundCollisionFlag; // касание земли
    public bool objectsCollisionFlag; // столкновение объектов
    public bool poleCollisionFlag; // падение стойки
    public bool ballCollisionFlag; // падение шарика
    public bool timerFlag; // превышение лимита времени
    public bool velocityFlag; // превышение лимита скорости

    // счетчики столкновений
    public int groundCollisionsCount;
    public int objectsCollisionsCount;
    public int poleCollisionsCount;
    public int ballCollisionsCount;

    // отведенное на упражнение время
    public float allotedTime;
    // штрафное время
    public float penaltyTime;

    // максимально допустимая скорость (м/с)
    public float maxVelocity;
    // превышен ли лимит скорости
    public bool exceededLimit;

    private void Start()
    {
        groundCollisionFlag = false;
        objectsCollisionFlag = false;
        poleCollisionFlag = false;
        ballCollisionFlag = false;
        timerFlag = false;
        velocityFlag = false;

        groundCollisionsCount = 0;
        objectsCollisionsCount = 0;
        poleCollisionsCount = 0;
        ballCollisionsCount = 0;

        allotedTime = 5f; // 5 сек
        penaltyTime = 0;

        maxVelocity = 5f; // 5 м/с
        exceededLimit = false;
    }

    private void OnDestroy()
    {
        if (groundCollisionFlag)
        {
            Debug.Log("Столкновений с землей: " + groundCollisionsCount.ToString());
        }

        if (objectsCollisionFlag)
        {
            Debug.Log("Столкновений с другими объектами: " + objectsCollisionsCount.ToString());
        }

        if (poleCollisionFlag)
        {
            Debug.Log("Падений стоек на землю: " + poleCollisionsCount.ToString());
        }

        if (ballCollisionFlag)
        {
            Debug.Log("Падений шариков на землю: " + ballCollisionsCount.ToString());
        }

        if (timerFlag)
        {
            Debug.Log("Время было превышено на: " + penaltyTime.ToString("F1") + " сек");
        }

        if (velocityFlag && exceededLimit)
        {
            Debug.Log("Было превышение скорости");
        }
    }
}
