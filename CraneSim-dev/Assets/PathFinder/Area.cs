using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    public float Length = 100;
    public float Width = 100;
    public float Height = 100;

    public Color color = Color.yellow;

    void Start()
    {
        var trans = GetComponent<Transform>();
        Kernel.CreateSurface(Length, Width);
        Kernel.barrierSurface.SetPosition(trans.position.x, trans.position.y, trans.position.z);
    }
    
    void Update()
    {
        // Отрисовка полидистантной поверхности
        var barSurf = Kernel.equalDistSurface;
        if (barSurf == null) return;
        var beg = new Vector3(0, 0, 0);

        for(int i = 0; i < barSurf.GetDiscretLength(); i++)
        {
            beg = barSurf.ToGlobalCoord(new Vector3Int(i, 0, 0));
            for (int j = 0; j < barSurf.GetDiscretWidth(); j++)
            {
                var next = barSurf.ToGlobalCoord(new Vector3Int(i, 0, j));
                next.y = barSurf[i, j];
                Debug.DrawLine(beg, next, Color.blue);
                beg = next;
            }
        }

        
    }

    public void OnDrawGizmos()
    {
        var size = new Vector3(Length, Height, Width);
        var pos = transform.position;
        var nodes = new List<Vector3>();
        foreach (var x in new float[] { pos.x - size.x / 2, pos.x + size.x / 2 })
        {
            foreach (var y in new float[] { pos.y - size.y / 2, pos.y + size.y / 2 })
            {
                foreach (var z in new float[] { pos.z - size.z / 2, pos.z + size.z / 2 })
                {
                    nodes.Add(new Vector3(x, y, z));
                }
            }
        }


        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (i == 0 && j == 7 ||
                    i == 1 && j == 6 ||
                    i == 2 && j == 5 ||
                    i == 3 && j == 4) continue;
                Debug.DrawLine(nodes[i], nodes[j], color);
            }
        }
    }
}
