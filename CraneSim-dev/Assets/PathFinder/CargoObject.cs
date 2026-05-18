using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cargos;

//Груз упрощен до Box, можно сделать другой класс при необходимости
public class CargoObject : MonoBehaviour
{
    void Start()
    {
        var trans = GetComponent<Transform>();
        Kernel.SetCargo(new BoxCargo(trans.position, trans.lossyScale, trans.eulerAngles));
        Kernel.currentCargo.Bind(this);
    }
    
    void Update()
    {
        
    }
}
