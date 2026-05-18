using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//оболочка для GeometryBody
public abstract class Barrier
{
    public float X { get { return body.X; } }
    public float Y { get { return body.Y; } }
    public float Z { get { return body.Z; } }

    public Vector3[] Nodes
    {
        get { return body.Nodes.ToArray(); }
    }

    public bool[,] Edges
    {
        get { return body.Edges; }
    }



    public Vector3 Rotation
    {
        get { return body.rotation; }
    }


    public GeometryBody body;

    public abstract float GetHeight(float x, float y);

    public float Length
    {
        get { return body.Length; }
    }

    public float Height
    {
        get { return body.Height; }
    }

    public float Width
    {
        get { return body.Width; }
    }

}


