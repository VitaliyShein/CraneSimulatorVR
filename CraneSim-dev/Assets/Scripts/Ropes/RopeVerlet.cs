using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RopeVerlet : MonoBehaviour {
	int countOfRopePoints;
	int countOfSlingPoints;
	List<RopePoint> points;
    //public bool connect;
	[HideInInspector]
	public int initCon = 0;  //индикатор подцепа
                           //1 при зацеплении, 0 при отцеплении
	RopeGeneration ropeGeneration;
	GameObject[] slingPoints; //массив для хранения всех точек троса

	//в функции Start получаем только точки троса
	void Start () {
		//Получаем список точек
		GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("PointVerlet");
		countOfRopePoints = tempPoints.Length; 
		points = new List<RopePoint>();
		for (int i = 0; i < countOfRopePoints; i++) {
			points.Add(tempPoints[i].GetComponent<RopePoint>());
			points[i].SpotCurPosition ();
			points[i].SpotPrevPosition ();
			points[i].SpotCountPoint ();
			points[i].SpotDistToPoint ();
			points[i].SpotRigidbody ();
			points[i].SpotInvMass ();
			points[i].SpotLenghtConstraints();
		}
		ropeGeneration = GetComponent<RopeGeneration>();
	}

	void FixedUpdate() {
		Verlet();
	}
	
	void Verlet()
	{
		for (int i = 0; i < points.Count; i++) {
			points[i].SpotCurPosition ();
			if (points[i].IsActive) {
				if (points[i].IsPhysics) {
					points[i].rb.AddForce(Physics.gravity, ForceMode.Acceleration);
				} else {
					points[i].AddAccel (Physics.gravity);
					points[i].SpotVerlet (Time.fixedDeltaTime); //ToDo: deltaTime
				}
			}
		}
		for (int k = 1; k > 0; k--) {
			for (int i = 0; i < points.Count; i++) {
				if (points[i].IsConnected) {
					points[i].SpotConstraint ();
				}
			}
		}
		for (int i = 0; i < points.Count; i++) {
			if (points [i].IsActive) {
				points [i].SetPosition ();
				points [i].SetForce ();
			}
		}
	}

	void SlingsInit()
	{
		//Получаем точки строп
		slingPoints = GameObject.FindGameObjectsWithTag("SlingPointVerlet");
		countOfSlingPoints = slingPoints.Length;
		//добавляем их в общий массив
		int index = points.Count;
		for (int i = 0; i < countOfSlingPoints; i++)
		{
			points.Add(slingPoints[i].GetComponent<RopePoint>());
			points[index + i].SpotCurPosition();
			points[index + i].SpotPrevPosition();
			points[index + i].SpotCountPoint();
			points[index + i].SpotDistToPoint();
			points[index + i].SpotRigidbody();
			points[index + i].SpotInvMass();
		}
		initCon = 1;
	}

    public void Disconnect()
    {
        points.RemoveRange(countOfRopePoints, countOfSlingPoints);
        for (int i = 0; i < slingPoints.Length; i++)
        {
            string[] index = slingPoints[i].name.Split('.');
            if (index[2] != "1" && index[2] != (countOfSlingPoints/4).ToString())
                Destroy(slingPoints[i]);
        }
		DrawRope[] slings = GetComponentsInChildren<DrawRope>();
		for (int i = 0; i < slings.Length; i++)
		{
			if (slings [i].tag == "Sling") {
				slings [i].connect = false;
				slings [i].SetEmpty ();
			}
		}
        initCon = 0;
    }

	public void Connect()
	{
		ropeGeneration.RopeGenerate(4, ropeGeneration.startSlingPoints, ropeGeneration.endSlingPoints,
			ropeGeneration.amountOfSlingPoints, ropeGeneration.pointSlingPrefab, ropeGeneration.parentForSling);
		SlingsInit(); //применили к точкам строп физические свойства
		DrawRope[] slings = GetComponentsInChildren<DrawRope>();
		for (int i = 0; i < slings.Length; i++)
		{
			if (slings[i].tag == "Sling")
				slings[i].Draw();
		}
	}
}
