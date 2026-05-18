using UnityEngine;
using System.Collections;

public class DrawRope : MonoBehaviour
{
    public GameObject ropes;
    public GameObject slings;
    public int drawStep;
    int drawCount;
    float[,] P;
    LineRenderer drawLine;
    Transform[] drawPoints;
    //[HideInInspector]
    public bool connect;

    void Start()
    {
        if (tag == "Rope")
            Draw();//для троса
    }

    void Update()
    {
        
    }

    public void Draw()
    {
        drawLine = GetComponent<LineRenderer>();
        //Находим точки, по которым будем отрисовывать трос 
        
        //вынесено под if, так как используется в разное время для разных объектов
        if (tag == "Rope")
        {
            FindAndSort();
            drawPoints[1].transform.SetSiblingIndex(drawPoints.Length - 1);
            FindAndSort();
        }
        if (tag == "Sling")
        {
            FindAndSort();
            drawPoints[1].transform.SetSiblingIndex(drawPoints.Length - 1);
            FindAndSort();
            connect = true;
        }
        drawCount = drawPoints.Length;

        //Коэф-ты Безье для отрисовки троса 
        P = new float[drawCount, drawStep];
        float dDraw = 1f / (drawStep - 1);
        int k = 0;
        int factN = CraneFunctions.factorial(drawCount - 1);
        for (float t = 0; t <= 1f; t += dDraw, k += 1)
        {
            for (int i = 0; i < drawCount; i += 1)
            {
                int n_i = drawCount - 1 - i;
                float b = factN / (CraneFunctions.factorial(i) * CraneFunctions.factorial(n_i));
                P[i, k] = b * Mathf.Pow(1f - t, n_i) * Mathf.Pow(t, i);
            }
        }
        drawLine.SetVertexCount(drawStep);
    }

    void LateUpdate()
    {
        if (tag == "Rope" || connect)
        {
            // Отрисовка линий безье 
            for (int i = 0; i < drawStep; i += 1)
            {
                Vector3 pos = Vector3.zero;
                for (int j = 0; j < drawCount; j += 1)
                {
                    pos.x += P[j, i] * drawPoints[j].transform.position.x;
                    pos.y += P[j, i] * drawPoints[j].transform.position.y;
                    pos.z += P[j, i] * drawPoints[j].transform.position.z;
                }
                drawLine.SetPosition(i, pos);
            }
        }
        // Отрисовка ломанной линии 
        /*for (int i = 0; i < drawCount; i += 1) { 
          Vector3 pos = drawPoints[i].transform.position; 
          drawLine.SetPosition (i, pos); 
        }*/
    }

	public void SetEmpty() {
		drawLine.SetVertexCount (0);
	}

    void FindAndSort()
    {
        Transform[] tempPoints = GetComponentsInChildren<Transform>();
        drawPoints = new Transform[tempPoints.Length - 1];
        for (int i = 1; i < drawPoints.Length + 1; i++)
        {
            drawPoints[i - 1] = tempPoints[i];
        }
    }


}