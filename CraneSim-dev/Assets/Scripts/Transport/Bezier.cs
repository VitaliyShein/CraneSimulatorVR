using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{
	public Transform p0;
	public Transform p1;
	public Transform p2;
	public Transform p3;
	
	const int SEGMENT_COUNT = 15;
	private LineRenderer lr;
	
	void Start ()
	{
		lr = GetComponent<LineRenderer>();
		lr.positionCount = SEGMENT_COUNT + 1;
		DrawBezier();

	}
	
	void Update () {
		DrawBezier();
	}
	
	Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		float tt = t*t;
		float uu = u*u;
		float uuu = uu * u;
		float ttt = tt * t;
 
		Vector3 p = uuu * p0;    //first term
		p += 3 * uu * t * p1;    //second term
		p += 3 * u * tt * p2;    //third term
		p += ttt * p3;           //fourth term
 
		return p;
	}

	void DrawBezier()
	{
		for (int i = 0; i <= SEGMENT_COUNT; ++i)
		{
//			Debug.Log("i = " + i);
			var t = i / (float)SEGMENT_COUNT;
			Vector3 pixel = CalculateBezierPoint(t, p0.position, p1.position, p2.position, p3.position);
			lr.SetPosition(i, pixel);
		}
	}
}
