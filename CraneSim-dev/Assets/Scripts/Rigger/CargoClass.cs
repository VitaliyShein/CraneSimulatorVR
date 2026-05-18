using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] //НУЖНО ДЛЯ ОТЛАДКИ, ПОТОМ УБРАТЬ!
public class CargoClass : MonoBehaviour {

    //public enum CargoType { BigContainer = 0, PipeBundle = 1 }
	
	//public CargoType cargoType;
	public List<PointClass> points = new List<PointClass>();
	public bool deliveryStatus = false;
	public GameObject cargoPhantom = null;
	
	public bool drawGizmos;
	public bool updatePoints;

	// Use this for initialization
    void Start ()
    {
	    //lastPosition = transform.position;
	    //lastQuaternion = transform.rotation;
	    points = new List<PointClass>();
	    Transform childPoints = null;
	    for (int i = 0; i < transform.childCount; i++)
		    if (transform.GetChild(i).GetComponent("POINT") != null)
			    childPoints = transform.GetChild(i);
	    if (childPoints != null)
			for (int i = 0; i < childPoints.childCount; i++)
					if (childPoints.GetChild(i).GetComponent("PointClass") != null)
						points.Add((PointClass)childPoints.GetChild(i).GetComponent("PointClass"));
    }
	
	// Update is called once per frame
	void Update ()
	{
		//test = points;
	}

	public void UpdatePoints()
	{
		Start();
	}

	void OnDrawGizmos()
	{
		if(points.Count != 0 && drawGizmos)
			for (int i = 0; i < points.Count; i++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(points[i].transform.position,0.3f);
				Gizmos.color = Color.white;
				if (i != 0)
					Gizmos.DrawLine(points[i-1].transform.position,points[i].transform.position);
			}
	}
}
