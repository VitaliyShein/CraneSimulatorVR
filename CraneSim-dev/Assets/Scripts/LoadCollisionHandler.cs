using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCollisionHandler : MonoBehaviour
{
    // обработчик штрафов
    private PenaltyHandler ph;
    
    // скрипт зацепа груза
    private RopeConnect rc;

    // найдены ли необходимые компоненты
    private bool isFindedComponent;

    private void Start()
    {   
        try
        {
            ph = GameObject.Find("PenaltyHandler").GetComponent<PenaltyHandler>();
            rc = GameObject.Find("Ropes & Hook").GetComponent<RopeConnect>();
            isFindedComponent = true;
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
            // столкновения с землей, слой Ground (8), груз соединен, текущий груз = соединенный груз
            if (ph.groundCollisionFlag && collision.gameObject.layer == 8 && rc.isConnect && this.gameObject == rc.cargo)
            {
                ph.groundCollisionsCount++;
                //Debug.Log("Груз столкнулся с землей");
            }

            // столкновения с другими объектами, слой Collided Object (9), груз соединен, текущий груз = соединенный груз
            if (ph.objectsCollisionFlag && collision.gameObject.layer == 9 && rc.isConnect && this.gameObject == rc.cargo)
            {
                ph.objectsCollisionsCount++;
                //Debug.Log("Груз столкнулся с другим объектом " + collision.gameObject.name);
            }  
        }   
    }
}
