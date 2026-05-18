using UnityEngine;
using System.Collections;

public class DrawUnityRope : MonoBehaviour {
	
	public int countDraw;
	public int num;
	float[,] P;
	LineRenderer drawLine;
	GameObject[] points;
	
	// Use this for initialization
	void Start () {
		drawLine = GetComponent<LineRenderer> ();
		drawLine.SetVertexCount (countDraw);
		//drawLine.SetVertexCount (countDraw);
		//Коэф-ты Безье для отрисовки троса
		P = new float[4, countDraw];
		float dDraw = 1f / (countDraw - 1);
		int k = 0;
		for (float t = 0; t <= 1f; t += dDraw, k += 1) {
			P [0, k] = Mathf.Pow (1f - t, 3f);
			P [1, k] = 3f * t * Mathf.Pow (1f - t, 2f);
			P [2, k] = 3f * (1f - t) * Mathf.Pow (t, 2f);
			P [3, k] = Mathf.Pow (t, 3f);
		}
		//Находим точки, по которым будем отрисовывать трос
		GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("PointUnity");
		int count = tempPoints.Length;
		points = new GameObject[4];
		for(int i = 1; i <= 4; i++) {
			for(int j = 0; j < count; j++) {
				string[] index = tempPoints[j].name.Split('.');
				if (index[1].Equals(num.ToString())) {
					if (index[2].Equals(i.ToString ())) {
						points[i-1] = tempPoints[j];
						break;
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		for (int i = 0; i < countDraw; i += 1) {
			Vector3 pos = new Vector3 ();
			pos.x = P[0,i]*points[0].transform.position.x + 
				P[1,i]*points[1].transform.position.x + 
					P[2,i]*points[2].transform.position.x + 
					P[3,i]*points[3].transform.position.x;
			pos.y = P[0,i]*points[0].transform.position.y + 
				P[1,i]*points[1].transform.position.y + 
					P[2,i]*points[2].transform.position.y + 
					P[3,i]*points[3].transform.position.y;
			pos.z = P[0,i]*points[0].transform.position.z + 
				P[1,i]*points[1].transform.position.z + 
					P[2,i]*points[2].transform.position.z + 
					P[3,i]*points[3].transform.position.z;
			drawLine.SetPosition (i, pos);
		}	
		/*for (int i = 0; i < 4; i += 1) {
			Vector3 pos = points[i].transform.position;
			drawLine.SetPosition (i, pos);
		}*/	
	}
}
