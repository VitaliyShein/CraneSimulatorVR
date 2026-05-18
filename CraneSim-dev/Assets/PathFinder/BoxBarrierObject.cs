using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBarrierObject : MonoBehaviour
{
    BoxBarrier barrier;
    public Vector3 size;
    public Vector3 localPosition;

    public Color color = Color.red;

    void Start()
    {
        //Добавление барьера типа кран
        barrier = new BoxBarrier(transform.position + localPosition, size, new Vector3());

        Kernel.AddBarrier(barrier);
    }

    void Update()
    {

    }

    public void OnDrawGizmos()
    {
        var pos = transform.position+localPosition;
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
                Debug.DrawLine(nodes[i], nodes[j], color);
            }
        }
    }
}
