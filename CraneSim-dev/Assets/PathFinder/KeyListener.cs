using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyListener : MonoBehaviour
{
    // СПиски дебаг линий которые надо отрисовать
    public List<KeyValuePair<Vector3, Vector3>> debugLines = new List<KeyValuePair<Vector3, Vector3>>(); //зеленый
    public List<KeyValuePair<Vector3, Vector3>> debugMLines = new List<KeyValuePair<Vector3, Vector3>>(); //магнета
    public List<KeyValuePair<Vector3, Vector3>> debugRLines = new List<KeyValuePair<Vector3, Vector3>>(); // красный
    
    void Start()
    {
        Kernel.draws.Add(debugLines);
        Kernel.magDraws.Add(debugMLines);
        Kernel.redDraws.Add(debugRLines);
    }
    
    void Update()
    {
        
        if (Input.GetKey(KeyCode.L))
        {
            //Обновляем поверхность барьеров
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Kernel.BarriersUpdate();
            watch.Stop();
            Debug.Log(string.Format("Barrier update: {0} ms", watch.ElapsedMilliseconds));

            //Обновляем параметры груза
            watch = System.Diagnostics.Stopwatch.StartNew();
            Kernel.currentCargo.Update();
            watch.Stop();
            Debug.Log(string.Format("Cargo Update: {0} ms", watch.ElapsedMilliseconds));

            //Вычисляем эквидистантную поверхность
            watch = System.Diagnostics.Stopwatch.StartNew();
            Kernel.ComputeEqualDist();
            watch.Stop();
            Debug.Log(string.Format("Compute PolydistSurf: {0} ms", watch.ElapsedMilliseconds));

            watch = System.Diagnostics.Stopwatch.StartNew();
            PathFinder pf = new PathFinder(Kernel.equalDistSurface);
            watch.Stop();
            Debug.Log(string.Format("Create PathFinder: {0} ms", watch.ElapsedMilliseconds));

            //Ищем путь
            watch = System.Diagnostics.Stopwatch.StartNew();
            var path = pf.GetPath(Kernel.currentCargo.Center, Kernel.aim.GetComponent<Transform>().position, PathFinder.Euristic.Manhetten);
            watch.Stop();
            Debug.Log(string.Format("End PathFinding: {0} ms", watch.ElapsedMilliseconds));

            //Оптимизируем путь
            watch = System.Diagnostics.Stopwatch.StartNew();
            var ofpath = pf.LocalOptimizePath(path);
            watch.Stop();
            Debug.Log(string.Format("End Optimize Global Coord: {0} ms", watch.ElapsedMilliseconds));

            //рисуем неоптимизированный путь
            debugLines.Clear();
            for (int i = 0; i < path.Count - 1; i++)
            {
                debugLines.Add(new KeyValuePair<Vector3, Vector3>(path[i], path[i + 1]));
            }
            //рисуем оптимизированный путь
            debugRLines.Clear();
            for (int i = 0; i < ofpath.Count - 1; i++)
            {
                debugRLines.Add(new KeyValuePair<Vector3, Vector3>(ofpath[i] + new Vector3(0.1f, 0.1f, 0.1f), ofpath[i + 1] + new Vector3(0.1f, 0.1f, 0.1f)));
            }
        }

        //Отрисовываем ВСЕ линии из Kernel, можно поместить в отдельный объект Unity 
        foreach (var b in Kernel.draws)
        {
            foreach (var a in b)
            {
                Debug.DrawLine(a.Key, a.Value, Color.green);
            }
        }
        foreach (var b in Kernel.magDraws)
        {
            foreach (var a in b)
            {
                Debug.DrawLine(a.Key, a.Value, Color.magenta);
            }
        }
        foreach (var b in Kernel.redDraws)
        {
            foreach (var a in b)
            {
                Debug.DrawLine(a.Key, a.Value, Color.red);
            }
        }
    }
}
