using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Класс представляет собой геометрическое тело состоящее из точек и ребер (многогранник)
public class GeometryBody
{
    public float X
    {
        get
        {
            return Center.x;
        }
    }
    public float Y
    {
        get
        {
            return Center.y;
        }
    }
    public float Z
    {
        get
        {
            return Center.z;
        }
    }
    //public Vector3 Position { get; set; }

    protected List<Vector3> nodes = new List<Vector3>(); //вершины без поворотов
    protected List<Vector3> rotatedNodes = new List<Vector3>(); // повернутые вершины
    protected bool[,] edges; // ребра, edges[i, j] == 1 => есть ребро между i & j; i,j - индексы в nodes

    public List<Vector3> Nodes
    {
        get
        {
            return rotatedNodes;
        }
    }
    public bool[,] Edges
    {
        get
        {
            return edges;
        }
    }

    protected Vector3 _rotation;

    public float Length
    {
        get
        {
            return Nodes.Select(x => x.x).Max() - Nodes.Select(x => x.x).Min();
        }
    }

    public float Height
    {
        get
        {
            return Nodes.Select(x => x.y).Max() - Nodes.Select(x => x.y).Min();
        }
    }

    public float Width
    {
        get
        {
            return Nodes.Select(x => x.z).Max() - Nodes.Select(x => x.z).Min();
        }
    }

    public Vector3 Center
    {
        get
        {
            if(Nodes.Count == 0)
            {
                throw new Exception("Не заданы точки объекта");
            }
            return new Vector3(
                (Nodes.Select(x => x.x).Max() + Nodes.Select(x => x.x).Min())/2,
                (Nodes.Select(x => x.y).Max() + Nodes.Select(x => x.y).Min())/2,
                (Nodes.Select(x => x.z).Max() + Nodes.Select(x => x.z).Min())/2
                );
        }
    }

    public Vector3 rotation
    {
        get
        {
            return _rotation;
        }

        set
        {
            _rotation = value;
            for (int i = 0; i < nodes.Count; i++)
            {
                rotatedNodes[i] = _rotateNodes(nodes[i], new Vector3(X, Y, Z), _rotation);
            }
        }
    }

    public GeometryBody(Vector3[] nodes, bool[,] edges, Vector3 pos, Vector3 rotation)
    {
        foreach (var node in nodes)
        {
            this.nodes.Add(node);
        }
        //Position = pos;

        rotatedNodes.Clear();
        foreach (var node in nodes)
        {
            rotatedNodes.Add(_rotateNodes(node, pos, rotation));
        }
        _rotation = rotation;

        this.edges = new bool[edges.GetLength(0), edges.GetLength(1)];
        for (int i = 0; i < edges.GetLength(0); i++)
        {
            for (int j = 0; j < edges.GetLength(1); j++)
            {
                this.edges[i, j] = edges[i, j];
            }
        }
    }

    private float[,] _multMatrix(float[,] a, float[,] b)
    {
        if (a.GetLength(1) != b.GetLength(0))
        {
            throw new Exception("Wrong matrix size!");
        }
        float[,] result = new float[a.GetLength(0), b.GetLength(1)];

        float sum;
        for (int i = 0; i < a.GetLength(0); i++)
        {
            for (int j = 0; j < b.GetLength(1); j++)
            {
                sum = 0;
                for (int k = 0; k < a.GetLength(1); k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }

        return result;
    }

    private float[,] RotateX(float angle)
    {
        float[,] rot_x = new float[4, 4];

        angle = (float)(-angle / 180 * Math.PI);
        rot_x[0, 0] = 1;
        rot_x[0, 1] = 0;
        rot_x[0, 2] = 0;
        rot_x[0, 3] = 0;

        rot_x[1, 0] = 0;
        rot_x[1, 1] = (float)Math.Cos(angle);
        rot_x[1, 2] = -(float)Math.Sin(angle);
        rot_x[1, 3] = 0;

        rot_x[2, 0] = 0;
        rot_x[2, 1] = (float)Math.Sin(angle);
        rot_x[2, 2] = (float)Math.Cos(angle);
        rot_x[2, 3] = 0;

        rot_x[3, 0] = 0;
        rot_x[3, 1] = 0;
        rot_x[3, 2] = 0;
        rot_x[3, 3] = 1;

        return rot_x;
    }
    private float[,] RotateY(float angle)
    {
        float[,] rot_y = new float[4, 4];

        angle = (float)(-angle / 180 * Math.PI);

        rot_y[0, 0] = (float)Math.Cos(angle);
        rot_y[0, 1] = 0;
        rot_y[0, 2] = (float)Math.Sin(angle);
        rot_y[0, 3] = 0;

        rot_y[1, 0] = 0;
        rot_y[1, 1] = 1;
        rot_y[1, 2] = 0;
        rot_y[1, 3] = 0;

        rot_y[2, 0] = -(float)Math.Sin(angle);
        rot_y[2, 1] = 0;
        rot_y[2, 2] = (float)Math.Cos(angle);
        rot_y[2, 3] = 0;

        rot_y[3, 0] = 0;
        rot_y[3, 1] = 0;
        rot_y[3, 2] = 0;
        rot_y[3, 3] = 1;

        return rot_y;
    }
    private float[,] RotateZ(float angle)
    {
        float[,] rot_z = new float[4, 4];

        angle = (float)(-angle / 180 * Math.PI);
        rot_z[0, 0] = (float)Math.Cos(angle);
        rot_z[0, 1] = -(float)Math.Sin(angle);
        rot_z[0, 2] = 0;
        rot_z[0, 3] = 0;

        rot_z[1, 0] = (float)Math.Sin(angle);
        rot_z[1, 1] = (float)Math.Cos(angle);
        rot_z[1, 2] = 0;
        rot_z[1, 3] = 0;

        rot_z[2, 0] = 0;
        rot_z[2, 1] = 0;
        rot_z[2, 2] = 1;
        rot_z[2, 3] = 0;

        rot_z[3, 0] = 0;
        rot_z[3, 1] = 0;
        rot_z[3, 2] = 0;
        rot_z[3, 3] = 1;

        return rot_z;
    }
    private float[,] Move(Vector3 move)
    {
        float[,] rot_z = new float[4, 4];

        rot_z[0, 0] = 1;
        rot_z[0, 1] = 0;
        rot_z[0, 2] = 0;
        rot_z[0, 3] = 0;

        rot_z[1, 0] = 0;
        rot_z[1, 1] = 1;
        rot_z[1, 2] = 0;
        rot_z[1, 3] = 0;

        rot_z[2, 0] = 0;
        rot_z[2, 1] = 0;
        rot_z[2, 2] = 1;
        rot_z[2, 3] = 0;

        rot_z[3, 0] = move.x;
        rot_z[3, 1] = move.y;
        rot_z[3, 2] = move.z;
        rot_z[3, 3] = 1;

        return rot_z;
    }
    private float[,] Scale(float scale)
    {
        float[,] rot_z = new float[4, 4];

        rot_z[0, 0] = 1;
        rot_z[0, 1] = 0;
        rot_z[0, 2] = 0;
        rot_z[0, 3] = 0;

        rot_z[1, 0] = 0;
        rot_z[1, 1] = 1;
        rot_z[1, 2] = 0;
        rot_z[1, 3] = 0;

        rot_z[2, 0] = 0;
        rot_z[2, 1] = 0;
        rot_z[2, 2] = 1;
        rot_z[2, 3] = 0;

        rot_z[3, 0] = 0;
        rot_z[3, 1] = 0;
        rot_z[3, 2] = 0;
        rot_z[3, 3] = scale;

        return rot_z;
    }


    private Vector3 _rotateNodes(Vector3 point, Vector3 pos, Vector3 rot)
    {
        float[,] matrix = new float[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } };
        var morph = _multMatrix(matrix, Move(new Vector3(-pos.x, -pos.y, -pos.z)));
        morph = _multMatrix(morph, RotateZ(rot.z));
        morph = _multMatrix(morph, RotateX(rot.x));
        morph = _multMatrix(morph, RotateY(rot.y));
        morph = _multMatrix(morph, Move(pos));
        var result = _multMatrix(new float[,] { { point.x, point.y, point.z, 1 } }, morph);

        return new Vector3(result[0, 0], result[0, 1], result[0, 2]);
    }
    
    public void SetRotation(Vector3 rot)
    {
        rotation = rot;
    }

    public void RotateOn(Vector3 rot)
    {
        rotation = new Vector3(rotation.x + rot.x, rotation.y + rot.y, rotation.z + rot.z);
    }

    //Наипростейшее геометрическое тело - параллелепипед
    public static List<Vector3> CreateBoxNodes(Vector3 pos, Vector3 size)
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

    public static bool[,] CreateBoxEdges(List<Vector3> nodes)
    {
        int equalCoords = 0;
        bool[,] edges = new bool[nodes.Count, nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                equalCoords = 0;
                if (Math.Abs(nodes[i].x - nodes[j].x) < 0.00001)
                    equalCoords++;
                if (Math.Abs(nodes[i].y - nodes[j].y) < 0.00001)
                    equalCoords++;
                if (Math.Abs(nodes[i].z - nodes[j].z) < 0.00001)
                    equalCoords++;

                // если две координаты совпадают, то значит есь ребро (особенность параллелепипедов)
                if (equalCoords == 2)
                {
                    edges[i, j] = true;
                    edges[j, i] = true;
                }
            }
        }
        return edges;
    }

}

