using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBarrier : Barrier
{

    public BoxBarrier(Vector3 pos, Vector3 size, Vector3 rot)
    {
        var nodes = CreateNodes(pos, size);
        var edges = CreateEdges(nodes);

        body = new GeometryBody(nodes.ToArray(), edges, pos, rot);

    }

    private List<Vector3> CreateNodes(Vector3 pos, Vector3 size)
    {
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
        return nodes;
    }

    private bool[,] CreateEdges(List<Vector3> nodes)
    {
        int equalCoords = 0;
        bool[,] edges = new bool[nodes.Count, nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                equalCoords = 0;
                if (Mathf.Abs(nodes[i].x - nodes[j].x) < 0.00001)
                    equalCoords++;
                if (Mathf.Abs(nodes[i].y - nodes[j].y) < 0.00001)
                    equalCoords++;
                if (Mathf.Abs(nodes[i].z - nodes[j].z) < 0.00001)
                    equalCoords++;

                if (equalCoords == 2)
                {
                    edges[i, j] = true;
                    edges[j, i] = true;
                }
            }
        }
        return edges;
    }

    //основная функция - получить высоту барьера по глобальным координатам
    public override float GetHeight(float x, float y)
    {
        if (x - X < Length / 2 && x - X > -Length / 2 && y - Z < Width / 2 && y - Z > -Width / 2)
            return Height / 2 + Y;
        else
            return 0;
    }
}
