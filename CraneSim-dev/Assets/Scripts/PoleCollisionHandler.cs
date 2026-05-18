using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoleCollisionHandler : MonoBehaviour
{
    // обработчик штрафов
    private PenaltyHandler ph;

    // найдены ли необходимые компоненты
    private bool isFindedComponent;
    
    // первое столкновение (так как стойка стоит на земле)
    private bool isFirstCollision;

    // упала ли стойка
    private bool isFalled;

    private void Start()
    {
        try
        {
            ph = GameObject.Find("PenaltyHandler").GetComponent<PenaltyHandler>();
            isFindedComponent = true;

            isFirstCollision = true;
            isFalled = false;
        }
        catch
        {
            isFindedComponent = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFindedComponent)
        {
            // столкновения стойки с землей (слой Ground (8))
            if (ph.poleCollisionFlag && collision.gameObject.layer == 8)
            {
                if (!isFirstCollision)
                {
                    if (!isFalled)
                    {
                        ph.poleCollisionsCount++;
                        isFalled = true;

                        //Debug.Log("Стойка упала");
                    }
                }
                else
                {
                    isFirstCollision = false;
                }
            }
        }
    }
}
