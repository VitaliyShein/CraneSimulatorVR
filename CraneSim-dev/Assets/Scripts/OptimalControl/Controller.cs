using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameObject desiredObject;
    public GameObject cargoObject; 

    public Method method;

    [Header("Localization method settings")]
    public float kp;
    public float kd;
    public float ke;

    public float maxSpeed;
    
    // Private variables
    private Vector3 desiredPosition;
    private Vector3 oldError = Vector3.zero;
    private Vector3 oldAngles = Vector3.zero;
    private Vector3 sumAngles;

    //Coroutines
    private Coroutine localizationMethodCoroutine;
    private Coroutine proportionalMethodCoroutine;

    private void Start()
    {
        desiredPosition = desiredObject.transform.position;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            switch (method) {
                case Method.Localization:
                {
                    StopAllCoroutines();
                    //if (proportionalMethodCoroutine != null) StopCoroutine(proportionalMethodCoroutine);
                    proportionalMethodCoroutine = StartCoroutine(RunLocalizationMethod());
                    break;
                }
                case Method.Proportional:
                {
                    //if (localizationMethodCoroutine != null) StopCoroutine(localizationMethodCoroutine);
                    StopAllCoroutines();
                    localizationMethodCoroutine = StartCoroutine(RunProportionalController());
                    break;
                }
                default:
                {
                    Debug.LogError("Method don't exist");
                    break;
                }
            }
        }
    }

    IEnumerator RunLocalizationMethod()
    {
        do {
            Vector3 currentPosition = transform.position;
            Vector3 error = currentPosition - desiredPosition;
            Vector3 angles = CalculateCargoAngles();
            sumAngles += angles;

            if(error.magnitude < 1f) sumAngles = Vector3.zero;

            Vector3 F1 = (-kp * error - kd * (error - oldError)) / ke * Time.deltaTime;
            Vector3 F2 = (-kp * angles - kd * (angles - oldAngles) - 0.005f * sumAngles) * Time.deltaTime;
            Vector3 F = F1 + 0.25f * F2;
            //F = F.normalized;

            RestrictVector (ref F, maxSpeed);

            //Debug.Log("F = " + F);

            transform.position += F;

            oldError = error;
            oldAngles = angles;

            yield return null;
        }
        while (/*(transform.position - desiredPosition).magnitude > 1f*/true);
    }

    IEnumerator RunProportionalController()
    {
        do {
            Vector3 currentPosition = transform.position;
            Vector3 error = currentPosition - desiredPosition;

            Vector3 F = (-kp * error) * Time.deltaTime;
            
            //Debug.Log("Before restriction F = " + F.x);
            RestrictVector (ref F, maxSpeed);
            //Debug.Log("After restriction F = " + F.x);

            transform.position += F;

            oldError = error;

            yield return null;
        }
        while ((transform.position - desiredPosition).magnitude > 1f);
    }

    private void RestrictVector(ref Vector3 vector, float limit)
    {
        if(vector.x >= limit) vector = new Vector3(limit, vector.y, vector.z);
        else if (vector.x <= -limit) vector = new Vector3(-limit, vector.y, vector.z);
        
        if(vector.y >= limit) vector = new Vector3(vector.x, limit, vector.z);
        else if (vector.y <= -limit) vector = new Vector3(vector.x, -limit, vector.z);
        
        if(vector.z >= limit) vector = new Vector3(vector.x, vector.y, limit);
        else if (vector.z <= -limit) vector = new Vector3(vector.x, vector.y, -limit);
    
    }

    private Vector3 CalculateCargoAngles()
    {
        float xAngle, zAngle;
        if (cargoObject.transform.localEulerAngles.x > 180) xAngle = cargoObject.transform.localEulerAngles.x - 360;
        else xAngle = cargoObject.transform.localEulerAngles.x;

        if (cargoObject.transform.localEulerAngles.z > 180) zAngle = cargoObject.transform.localEulerAngles.z - 360;
        else zAngle = cargoObject.transform.localEulerAngles.z;

        Debug.Log ("cargo x = " + xAngle + "; z = " + zAngle);
        return new Vector3 (xAngle, 0f, zAngle);
    }
}

public enum Method{ Localization, Proportional }
