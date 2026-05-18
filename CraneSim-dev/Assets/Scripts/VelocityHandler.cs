using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityHandler : MonoBehaviour
{
    // обработчик штрафов
    private PenaltyHandler ph;

    // найдены ли необходимые компоненты
    private bool isFindedComponent;

    // максимально допустимая скорость(м/с)
    private float maxVelocity;

    // превышен ли лимит скорости
    public bool firstExceededLimit;

    void Start()
    {
        try
        {
            ph = GameObject.Find("PenaltyHandler").GetComponent<PenaltyHandler>();
            maxVelocity = ph.maxVelocity;
            firstExceededLimit = false;
            isFindedComponent = true;
        }
        catch
        {
            isFindedComponent = false;
        }
    }

    void Update()
    {
        //Debug.Log(this.gameObject.GetComponent<Rigidbody>().velocity.magnitude.ToString("F2"));

        if (isFindedComponent && !firstExceededLimit && this.gameObject.GetComponent<Rigidbody>().velocity.magnitude > maxVelocity)
        {
            ph.exceededLimit = true;
            firstExceededLimit = true;
        }
    }
}
