using System.Collections;
using System.Collections.Generic;
using Surfaces;
using UnityEngine;
using Cargos;

public static class Kernel
{
    public static List<Barrier> barriers = new List<Barrier>();
    public const float STEP = 1f; //шаг дискретизации
    public static BarrierSurface barrierSurface = null;
    public static EqualDistSurface equalDistSurface = null;
    public static Cargo currentCargo = null;
    public static Aim aim = null;

    
    public static List<List<KeyValuePair<Vector3, Vector3>>> draws = new List<List<KeyValuePair<Vector3, Vector3>>>();
    public static List<List<KeyValuePair<Vector3, Vector3>>> magDraws = new List<List<KeyValuePair<Vector3, Vector3>>>();
    public static List<List<KeyValuePair<Vector3, Vector3>>> redDraws = new List<List<KeyValuePair<Vector3, Vector3>>>();

    public static void AddBarrier(Barrier barrier)
    {
        barriers.Add(barrier);
        // если барьер появился раньше поверхности он  запоминается, при создании поверхности добавиться
        if (barrierSurface != null)
        {
            barrierSurface.PutBarrier(barrier);
        }
    }

    public static void SetCargo(Cargo cargo)
    {
        currentCargo = cargo;
    }

    public static void CreateSurface(float length, float width)
    {
        barrierSurface = new BarrierSurface(length, width, STEP);
        foreach(var b in barriers)
        {
            barrierSurface.PutBarrier(b);
        }
    }

    public static void BarriersUpdate()
    {
        if (barrierSurface == null)
            throw new System.ArgumentNullException("Не задана поверхность барьеров ");
        barrierSurface.ReBarrier();
    }

    public static void ComputeEqualDist()
    {
        if (currentCargo == null || barrierSurface == null)
            throw new System.ArgumentNullException("Не задана поверхность барьеров или груз");
        equalDistSurface = new EqualDistSurface(barrierSurface, currentCargo.Size);
    }
}
