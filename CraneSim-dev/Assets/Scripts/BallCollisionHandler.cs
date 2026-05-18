using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollisionHandler : MonoBehaviour
{
    // обработчик штрафов
    private PenaltyHandler ph;

    // найдены ли необходимые компоненты
    private bool isFindedComponent;

    // упал ли шарик
    private bool isFalled;

    private void Start()
    {
        try
        {
            ph = GameObject.Find("PenaltyHandler").GetComponent<PenaltyHandler>();
            isFindedComponent = true;
            
            isFalled = false;
        }
        catch
        {
            isFindedComponent = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFindedComponent)
        {
            // столкновения шарика с землей (слой Ground (8))
            if (ph.ballCollisionFlag && collision.gameObject.layer == 8)
            {
                if (!isFalled)
                {
                    ph.ballCollisionsCount++;
                    isFalled = true;

                    //Debug.Log("Шарик упал");
                }
            }
        }  
    }
}
