using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Цель - необходима только точка куда прокладывать маршрут
public class Aim : MonoBehaviour
{
    void Start()
    {
        Kernel.aim = this;
    }
    
    void Update()
    {

    }
}
