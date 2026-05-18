using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Surfaces;
using System.Linq;

public class PathFinder
{
    //Глубина оптимизации
    const int OPTIMIZE_DEEPTH = 300;

    private Surface surface;
    //private int[,] discretSurface;

    //Списки для отрисовки. Необходимо добавить в Kernel, потом в объекте Unity в Update отрисовывать
    public List<KeyValuePair<Vector3, Vector3>> debugLines = new List<KeyValuePair<Vector3, Vector3>>();
    public List<KeyValuePair<Vector3, Vector3>> debugMagnetaLines = new List<KeyValuePair<Vector3, Vector3>>();

    public PathFinder(Surface surface)
    {
        this.surface = surface;
        //discretSurface = surface.ToIntArray();

        Kernel.draws.Add(debugLines);
        Kernel.magDraws.Add(debugMagnetaLines);
    }

    //массив направлений движения алгоритма
    //a[x,y,z] == 0 это свободное поле
    //В a[x,y,z] == 0 записываются направления, с какой стороны алгоритм пришел, в виде чисел от 1 до 27, см. _directions
    //Например, a[x,y,z]=1,   _directions[0]  = [-1,-1,-1] значит в эту клетку пришел из a[x+1, y+1, z+1]
    //          a[x,y,z]=15,  _directions[14] = [ 0, 0, 1] значит в эту клетку пришел из a[x, y, z-1]
    private byte [][][] CreateAllowedArea(Surface surface)
    {
        var length = surface.GetDiscretLength();
        var maxY = surface.ToDiscretCoord(new Vector3(0, surface.Height, 0)).y;
        var width = surface.GetDiscretWidth();

        var result = new byte[length][][];
        //int[][][] fromStart = new int[cameFrom.GetLength(0)][][];
        for (int i = 0; i < length; i++)
        {
            result[i] = new byte[maxY][];
            for (int j = 0; j < maxY; j++)
            {
                result[i][j] = new byte[width];
            }
        }

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var y = surface.ToDiscretCoord(new Vector3(0, surface[i, j], 0)).y;
                if(y != 0)
                    for (int h = 0; h <= y; h++)
                    {
                        result[i][h][j]  = 255;
                    }

            }
        }

        return result;
    }

    class ComparerKVP : IComparer<int>, IComparer<float>
    {

        public int Compare(int x, int y)
        {
            if (x > y)
                return 1;
            else
                return -1;
        }

        public int Compare(float x, float y)
        {
            if (x > y)
                return 1;
            else
                return -1;
        }

    }

    //Поиск пути, возвращает список последовательных точек пути
    public List<Vector3> GetPath(Vector3 start, Vector3 end, Euristic euristic)
    {
        //Ограничение против зависания
        int MAX_WHILE_COUNTER = 8000;

        //метка начала пути
        byte BEGIN_PATH = 123;


        var startDiscretCoord = surface.ToDiscretCoord(start);
        var endDiscretCoord = surface.ToDiscretCoord(end);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        //массив направлений движения алгоритма
        byte[][][] cameFrom = CreateAllowedArea(surface);
        //массив посещена ли вершина
        bool[][][] visited = new bool[cameFrom.Length][][];
        //массив длин пути до начала
        float[][][] fromStart = new float[cameFrom.Length][][];
        //массив полных длин пути через точку
        //float[][][] totalPathLength = new float[cameFrom.Length][][];
        for (int i = 0; i < cameFrom.Length; i++)
        {
            visited[i] = new bool[cameFrom[i].Length][];
            fromStart[i] = new float[cameFrom[i].Length][];
            //totalPathLength[i] = new float[cameFrom[i].Length][];
            for (int j = 0; j < cameFrom[i].Length; j++)
            {
                visited[i][j] = new bool[cameFrom[i][j].Length];
                fromStart[i][j] = new float[cameFrom[i][j].Length];
                //totalPathLength[i][j] = new int[cameFrom[i][j].Length];
            }
        }
        watch.Stop();
        Debug.Log(string.Format("-End creating arrays: {0} ms", watch.ElapsedMilliseconds));

        //Список вершин, которые надо просмотреть
        //var slist = new SortedList<int, Vector3Int>(new ComparerKVP(totalPathLength)) { { 0, startDiscretCoord } };
        var slist = new SortedList<float, Vector3Int>(new ComparerKVP()) { { 0, startDiscretCoord } };

        var closed = new List<Vector3Int>();

        List<KeyValuePair<byte, Vector3Int>> neighbours;

        //текущая вершина
        Vector3Int cur = startDiscretCoord;

        //суммарный путь до начала
        float totalCost;

        int debugCounter = 0;
        fromStart[cur.x][cur.y][cur.z] = 0;
        cameFrom[cur.x][cur.y][cur.z] = BEGIN_PATH;
        visited[cur.x][cur.y][cur.z] = true;


        while (cur.x != endDiscretCoord.x || cur.y != endDiscretCoord.y || cur.z != endDiscretCoord.z)
        {
            debugCounter++;
            if (debugCounter > MAX_WHILE_COUNTER)
            {
                break;
            }

            //Выбираем новую вершину
            cur = slist.First().Value;
            slist.RemoveAt(0);
            visited[cur.x][cur.y][cur.z] = true;
            closed.Add(cur);

            //Смотрим соседей
            neighbours = GetNeightbours(cur, cameFrom);

            Vector3Int nbr;
            byte direction;
            float currentDistance;
            foreach (var nbr_pair in neighbours)
            {
                nbr = nbr_pair.Value; //вершина
                direction = nbr_pair.Key; //направление откуда пришел алгоритм в эту вершину

                currentDistance = fromStart[cur.x][cur.y][cur.z] + CostByDirection(direction);

                if ((visited[nbr.x][nbr.y][nbr.z] != true || currentDistance < fromStart[nbr.x][nbr.y][nbr.z]))
                {
                    totalCost = GetEuristic(euristic, nbr, endDiscretCoord, startDiscretCoord) + currentDistance;
                    fromStart[nbr.x][nbr.y][nbr.z] = currentDistance;
                    if (cameFrom[nbr.x][nbr.y][nbr.z] == 0)
                    {
                        slist.Add(totalCost, nbr);
                    }
                    else if (visited[nbr.x][nbr.y][nbr.z] != true)
                    {
                        slist.RemoveAt(slist.IndexOfValue(nbr));
                        slist.Add(totalCost, nbr);
                    }
                    cameFrom[nbr.x][nbr.y][nbr.z] = direction;
                }

            }


            if (slist.Count == 0) { break; }

        }

        if (debugCounter > MAX_WHILE_COUNTER)
        {

            Debug.Log("Too large path");

            // Показать просмотренные вершины
            debugLines.Clear();
            foreach (var n in slist.Select(x => x.Value))//result)
            {
                var m = surface.ToGlobalCoord(n);
                m.y = m.y - 0.2f;
                debugLines.Add(new KeyValuePair<Vector3, Vector3>(m, surface.ToGlobalCoord(n)));
            }
            return new List<Vector3>();

        }

        if (slist.Count == 0)
        {
            Debug.Log("No path");
            return new List<Vector3>();
        }

        //debug
        {
            debugMagnetaLines.Clear();
            foreach (var n in closed)//result)
            {
                if (cameFrom[n.x][n.y][n.z] == 123) continue;
                var dir = _directions[cameFrom[n.x][n.y][n.z] - 1];
                var m = new Vector3Int(n.x - dir[0], n.y - dir[1], n.z - dir[2]);
            }
        }

        // Обратный ход - восстановление пути
        cur = endDiscretCoord;
        List<Vector3Int> result = new List<Vector3Int>() { cur };

        int _i = 0;
        while (cameFrom[cur.x][cur.y][cur.z] != BEGIN_PATH)
        {
            if (cameFrom[cur.x][cur.y][cur.z] == 0)
                throw new System.Exception("Хмммм, 0...");

            var n = cur;
            var offset = _directions[cameFrom[cur.x][cur.y][cur.z] - 1];
            n.x -= offset[0];
            n.y -= offset[1];
            n.z -= offset[2];
            result.Add(n);
            cur = n;


            _i++;
            if (_i > 2000)
                throw new System.Exception("_i > max");

        }

        Debug.LogFormat("Path length (points): {0}", _i);

        var pathGlobalCoord = new List<Vector3>();
        foreach (var node in result)
        {
            pathGlobalCoord.Add(surface.ToGlobalCoord(node));
        }

        return pathGlobalCoord;
    }

    //Поиск пути, возвращает список последовательных точек пути
    //public List<Vector3> GetPathInt(Vector3 start, Vector3 end)
    //{
    //    //Ограничение против зависания
    //    int MAX_WHILE_COUNTER = 8000;

    //    //метка начала пути
    //    byte BEGIN_PATH = 123;


    //    var startDiscretCoord = surface.ToDiscretCoord(start);
    //    var endDiscretCoord = surface.ToDiscretCoord(end);

    //    var watch = System.Diagnostics.Stopwatch.StartNew();
    //    //массив направлений движения алгоритма
    //    byte[][][] cameFrom = CreateAllowedArea(surface);
    //    //массив посещена ли вершина
    //    bool[][][] visited = new bool[cameFrom.Length][][];
    //    //массив длин пути до начала
    //    int[][][] fromStart = new int[cameFrom.Length][][];
    //    //массив полных длин пути через точку
    //    int[][][] totalPathLength = new int[cameFrom.Length][][];
    //    for (int i = 0; i < cameFrom.Length; i++)
    //    {
    //        visited[i] = new bool[cameFrom[i].Length][];
    //        fromStart[i] = new int[cameFrom[i].Length][];
    //        totalPathLength[i] = new int[cameFrom[i].Length][];
    //        for (int j = 0; j < cameFrom[i].Length; j++)
    //        {
    //            visited[i][j] = new bool[cameFrom[i][j].Length];
    //            fromStart[i][j] = new int[cameFrom[i][j].Length];
    //            totalPathLength[i][j] = new int[cameFrom[i][j].Length];
    //        }
    //    }
    //    watch.Stop();
    //    Debug.Log(string.Format("-End creating arrays: {0} ms", watch.ElapsedMilliseconds));

    //    //Список вершин, которые надо просмотреть
    //    //var slist = new SortedList<int, Vector3Int>(new ComparerKVP(totalPathLength)) { { 0, startDiscretCoord } };
    //    var slist = new SortedList<int, Vector3Int>(new ComparerKVP()) { { 0, startDiscretCoord } };

    //    var closed = new List<Vector3Int>();

    //    List<KeyValuePair<byte, Vector3Int>> neighbours;

    //    //текущая вершина
    //    Vector3Int cur = startDiscretCoord;

    //    //суммарный путь до начала
    //    int totalCost;

    //    int debugCounter = 0;
    //    fromStart[cur.x][cur.y][cur.z] = 0;
    //    cameFrom[cur.x][cur.y][cur.z] = BEGIN_PATH; 
    //    visited[cur.x][cur.y][cur.z] = true;
        

    //    while (cur.x != endDiscretCoord.x || cur.y != endDiscretCoord.y || cur.z != endDiscretCoord.z)
    //    {
    //        debugCounter++;
    //        if(debugCounter > MAX_WHILE_COUNTER)
    //        {
    //            break;
    //        }

    //        //Выбираем новую вершину
    //        //slist.Sort((x, y) => { return x.Key.CompareTo(y.Key); });
    //        //slist = slist.OrderBy(x => x.Key).ToList();
    //        //slist.Sort(comparer);
    //        cur = slist.First().Value;
    //        slist.RemoveAt(0);
    //        visited[cur.x][cur.y][cur.z] = true;
    //        closed.Add(cur);
            
    //        //Смотрим соседей
    //        neighbours = GetNeightbours(cur, cameFrom);
    //        //Debug.Log(neighbours.Count);
    //        Vector3Int nbr;
    //        byte direction;
    //        int currentDistance;
    //        foreach (var nbr_pair in neighbours)
    //        {
    //            nbr = nbr_pair.Value; //вершина
    //            direction = nbr_pair.Key; //направление откуда пришел алгоритм в эту вершину

    //            currentDistance = fromStart[cur.x][cur.y][cur.z] + CostByDirection(direction);

    //            if ( (visited[nbr.x][nbr.y][nbr.z] != true || currentDistance < fromStart[nbr.x][nbr.y][nbr.z]))
    //            {
    //                totalCost = GetEuristic(Euristic.ManCheb, nbr, endDiscretCoord) + currentDistance;
    //                fromStart[nbr.x][nbr.y][nbr.z] = currentDistance;
    //                if (cameFrom[nbr.x][nbr.y][nbr.z] == 0)
    //                {
    //                    //slist.Add(new KeyValuePair<float, Vector3Int>(totalCost, nbr));
    //                    totalPathLength[nbr.x][nbr.y][nbr.z] = totalCost;
    //                    slist.Add(totalCost, nbr);
    //                }
    //                else if (visited[nbr.x][nbr.y][nbr.z] != true)
    //                {
    //                    slist.RemoveAt(slist.IndexOfValue(nbr));
    //                    //Debug.LogFormat("Removed {0} {1} {2}", nbr.x,nbr.y,nbr
    //                    totalPathLength[nbr.x][nbr.y][nbr.z] = totalCost;
                        
    //                    //var node = slist.IndexOfValue(nbr);// x => x.Value.x == nbr.x && x.Value.y == nbr.y && x.Value.z == nbr.z);
    //                    slist.Add(totalCost, nbr);
    //                }
    //                cameFrom[nbr.x][nbr.y][nbr.z] = direction;
    //            }
                
    //        }
            
            
    //        if(slist.Count == 0) { break; }

    //    }

    //    if (debugCounter > MAX_WHILE_COUNTER)
    //    {
            
    //        Debug.Log("Too large path");

    //        // Показать просмотренные вершины
    //        debugLines.Clear();
    //        foreach (var n in slist.Select(x => x.Value))//result)
    //        {
    //            var m = surface.ToGlobalCoord(n);
    //            m.y = m.y - 0.2f;
    //            debugLines.Add(new KeyValuePair<Vector3, Vector3>(m, surface.ToGlobalCoord(n)));
    //        }
    //        return new List<Vector3>();
            
    //    }

    //    if (slist.Count == 0)
    //    {
    //        Debug.Log("No path");
    //        return new List<Vector3>();
    //    }

    //    //debug
    //    {
    //        debugMagnetaLines.Clear();
    //        foreach (var n in closed)//result)
    //        {
    //            if (cameFrom[n.x][n.y][n.z] == 123) continue;
    //            //Debug.LogFormat("CLoseD: {0}, {1}, {2} = {3}", n.x, n.y, n.z, cameFrom[n.x, n.y, n.z]);
    //            var dir = _directions[cameFrom[n.x][n.y][n.z] - 1];
    //            var m = new Vector3Int(n.x-dir[0], n.y-dir[1], n.z-dir[2]);
    //            //debugMagnetaLines.Add(new KeyValuePair<Vector3, Vector3>(surface.ToGlobalCoord(m), surface.ToGlobalCoord(n)));
    //        }
    //    }

    //    // Обратный ход - восстановление пути
    //    cur = endDiscretCoord;
    //    List<Vector3Int> result = new List<Vector3Int>() { cur };
        
    //    int _i = 0;
    //    while(cameFrom[cur.x][cur.y][cur.z] != BEGIN_PATH)
    //    {
    //        if (cameFrom[cur.x][cur.y][cur.z] == 0)
    //            throw new System.Exception("Хмммм, 0...");

    //        var n = cur;
    //        //Debug.Log($"Back {n.x}, {n.y}, {n.z}");
    //        var offset = _directions[cameFrom[cur.x][cur.y][cur.z] - 1];
    //        //Debug.LogFormat("{0}, {1}, {2}",offset[0], offset[1], offset[2]);
    //        n.x -= offset[0];
    //        n.y -= offset[1];
    //        n.z -= offset[2];
    //        result.Add(n);
    //        cur = n;


    //        _i++;
    //        if(_i > 2000)
    //            throw new System.Exception("_i > max");

    //    }

    //    Debug.LogFormat("Path length (points): {0}", _i);

    //    var pathGlobalCoord = new List<Vector3>();
    //    foreach (var node in result)
    //    {
    //        pathGlobalCoord.Add(surface.ToGlobalCoord(node));
    //    }
        
    //    return pathGlobalCoord;
    //}

    private static readonly float _sqrt2 = Mathf.Sqrt(2);
    private static readonly float _sqrt3 = Mathf.Sqrt(3);

    // Направления движения алгоритма: от центра самого куба к углу кубической области допустимых направлений, к краю (к ребру) и к центрам граней 
    // Направления в виде индексов, соответствия направлений и индексов: _direction (см. CreateAllowedArea)
    private static byte[] _cornerDirections = new byte[] { 1, 3, 7, 9, 25, 27, 19, 21 };
    private static byte[] _edgeDirections = new byte[] { 2, 4, 6, 8, 16, 18, 10, 12, 26, 22, 20, 24 };
    private static byte[] _centerDirections = new byte[] { 5, 11, 13, 15, 17, 23 };

    public float CostByDirection(byte direction)
    {
        switch (direction)
        {
            case 1:
            case 3:
            case 7:
            case 9:
            case 25:
            case 27:
            case 19: 
            case 21:
                return _sqrt3; // ~sqrt(3)*10
            case 2:
            case 4:
            case 6:
            case 8:
            case 16:
            case 18:
            case 10:
            case 12:
            case 26:
            case 22:
            case 20:
            case 24:
                return _sqrt2; //~sqrt(2)*10
            case 5:
            case 11:
            case 13:
            case 15:
            case 17:
            case 23:
                return 1;
        }
        return 0;
    }


    private readonly static int[][] _directions = new int[27][]{
            new   int[] { -1, -1, -1 } ,
            new   int[] { -1, -1, 0 } ,
            new   int[] { -1, -1, 1 } ,
            new   int[] { -1, 0, -1 } ,
            new   int[] { -1, 0, 0 } ,
            new   int[] { -1, 0, 1 } ,

            new   int[] { -1, 1, -1 } ,
            new   int[] { -1, 1, 0 } ,
            new   int[] { -1, 1, 1 } ,
            new   int[] { 0, -1, -1} ,
            new   int[] { 0, -1, 0 } ,
            new   int[] { 0, -1, 1 } ,

            new   int[] { 0, 0, -1 } ,
            new   int[] { 0, 0, 0 } ,
            new   int[] { 0, 0, 1 } ,
            new   int[] { 0, 1, -1 } ,
            new   int[] { 0, 1, 0 } ,
            new   int[] { 0, 1, 1 } ,

            new   int[] { 1, -1, -1 } ,
            new   int[] { 1, -1, 0 } ,
            new   int[] { 1, -1, 1 } ,
            new   int[] { 1, 0, -1 } ,
            new   int[] { 1, 0, 0 } ,
            new   int[] { 1, 0, 1 } ,

            new   int[] { 1, 1, -1 } ,
            new   int[] { 1, 1, 0 } ,
            new   int[] { 1, 1, 1 } ,
    };
    //min - направления только по осям
    //max - все направления
    private static readonly byte[] _minDirs = new byte[] { 4, 16, 12, 14, 10, 22 };
    private static readonly byte[] _maxDirs = new byte[27] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
    private static readonly byte[] _horDirs = new byte[] { 4, 16, 12, 14, 10, 22, 3, 5,21, 23};

    // Получить соседние точки
    public List<KeyValuePair<byte, Vector3Int>> GetNeightbours(Vector3Int point, byte[][][] searchArea)
    {
        var result = new List<KeyValuePair<byte, Vector3Int>>();
        int[] d = new int[3];
        foreach (byte i in _maxDirs)
        {
            if (i == 13) continue;//skip center
            d[0] = _directions[i][0] + point.x;
            d[1] = _directions[i][1] + point.y;
            d[2] = _directions[i][2] + point.z;
            if (d[0] >= 0 &&
                d[0] < searchArea.Length &&
                d[1] >= 0 &&
                d[1] < searchArea[d[0]].Length &&
                d[2] >= 0 &&
                d[2] < searchArea[d[0]][d[1]].Length )
            {
                if(searchArea[d[0]][d[1]][d[2]] != 255)
                    result.Add(new KeyValuePair<byte, Vector3Int>((byte)(i+1), new Vector3Int(d[0], d[1], d[2])));
            }
        
        }
        return result;
    }

    // Эвристические функции
    public enum Euristic
    {
        Manhetten,
        Pifagor,
        Chebyshev,
        ManCheb,
        Angle,
        Default
    }

    int dx, dy, dz;

    private float GetEuristic(Euristic e, Vector3Int point, Vector3Int aim, Vector3Int start)
    {
        float result;
        switch (e)
        {
            case Euristic.Manhetten:
                result = Mathf.Abs(point.x - aim.x) + Mathf.Abs(point.y - aim.y) + Mathf.Abs(point.z - aim.z);
                break;
            case Euristic.Pifagor:
                result = Vector3Int.Distance(point, aim);
                break;
            case Euristic.Chebyshev:
                result = Mathf.Max(Mathf.Abs(point.x - aim.x), Mathf.Abs(point.y - aim.y), Mathf.Abs(point.z - aim.z));
                break;
            case Euristic.ManCheb:
                dx = Mathf.Abs(point.x - aim.x);
                dy = Mathf.Abs(point.y - aim.y);
                dz = Mathf.Abs(point.z - aim.z);
                if( dx > dy && dx > dz)
                {
                    result = dx + ((dy + dz) >> 1);
                }
                else if(dy > dz)
                {
                    result = dy + ((dx + dz) >> 1);
                }
                else
                {
                    result = dz + ((dx + dy) >> 1);
                }
                break;
            case Euristic.Angle:
                result = -1000*((aim.x - point.x)* (point.x - start.x) + (aim.y - point.y) * (point.y - start.y) + (aim.z - point.z) * (point.z - start.z)) / Vector3Int.Distance(point, start) / Vector3Int.Distance(point, aim);
                break;
            default:
                result = Mathf.Pow(point.x - aim.x, 2) + Mathf.Pow(point.y - aim.y, 2) + Mathf.Pow(point.z - aim.z, 2);
                break;

        }
        return result;
    } 
    
    // Локальная оптимизация пути
    public List<Vector3> LocalOptimizePath(List<Vector3> path)
    {
        var surface = CreateAllowedArea(this.surface);

        List<Vector3> result = new List<Vector3>(path);
        Vector3 currentNode, a, b;
        float curNodeNbrsDist, newNodeNbrsDist;
        int counter = 0; // от зависания
        bool flag = true;
        while (counter < OPTIMIZE_DEEPTH && flag)
        {
            flag = false;
            counter++;

            var offset = (counter % 4) + 1;

            for (int i = offset; i < path.Count - offset; i++)
            {
                //соседние
                a = result[i - offset];
                b = result[i + offset];
                //сама точка
                currentNode = result[i];

                //текущее расстояние
                curNodeNbrsDist = Vector3.Distance(a, currentNode) + Vector3.Distance(b, currentNode);

                //минимальное расстояние: новая точка - середина отрезка, концами которой являются соседи
                newNodeNbrsDist = Vector3.Distance(a, b);

                //если новая точка находится над поверхностью барьеров
                if (this.surface[(a.x + b.x) / 2, (a.z + b.z) / 2] < (a.y + b.y) / 2)
                {
                    result[i] = new Vector3((a.x + b.x) / 2, (a.y + b.y) / 2, (a.z + b.z) / 2);
                    flag = true;
                }
            }
           

        }
        
        return result;
    }

    private void Clear(int[][][] array)
    {
        for(int i = 0; i < array.Length; i++)
        {
            for(int j = 0; j < array[i].Length; j++)
            {
                for (int k = 0; k < array[i][j].Length; k++)
                {
                    array[i][j][k] = 0;
                }
            }
        }
    }
    
}
