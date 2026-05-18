using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeeAlgorithm : MonoBehaviour
{
	private const int sizeX = 4;						    // Размерность трехмерного массива
	private const int sizeY = 4;						    // Размерность трехмерного массива
	private const int sizeZ = 4;						    // Размерность трехмерного массива
	private int[ , , ] Map = new int[sizeX, sizeY, sizeZ];	// Карта точек

	private int startX = 0, startY = 3, startZ = 2;	    // Координаты стартовой точки
	private int endX = 3, endY = 0, endZ = 2;	        // Координаты финишной точки

	private bool a = false;								// Переменная нужна для единоразового запуска программы
	
	private List<int[,,]> path = new List<int[,,]>();	// Итоговый путь
	
	void Update()
	{
		if (!a)
		{
			Initialization();
			Expansion();
			a = true;
		}
	}
	
	void Initialization()
	{
		Debug.Log("Initialization started");

	    // Присваиваем всем точкам значение = -1
	    for (int x = 0; x < sizeX; x++)
	        for (int y = 0; y < sizeY; y++)
	            for (int z = 0; z < sizeZ; z++)
	                Map[x, y, z] = -1;
	    
		// 
	    // Инициализируем стартовую точку
		Map[startX, startY, startZ] = 0;
		
//		Debug.Log("Initialization finished");
	}

	void Expansion()
	{
//		Debug.Log("Expansion started");

	    int step = 0;
	    
	    do 
	    {
//		    Debug.Log("Step = " + step);
	        // Перебираем все точки карты
		    for (int x = 0; x < sizeX; x++)
		    {
			    for (int y = 0; y < sizeY; y++)
			    {
				    for (int z = 0; z < sizeZ; z++)
				    {
					    // Если находим равную для текущего шага
					    if (Map[x, y, z] == step)
					    {
						    // Просматриваем её соседей. Делаем цикл по ближайшим элементам
						    for (int i = x - 1; i <= x + 1; i++)
						    for (int j = y - 1; j <= y + 1; j++)
						    for (int k = z - 1; k <= z + 1; k++)
						    {
							    //Проверяем, не крайняя ли она
							    if (i >= 0 && j >= 0 && k >= 0 && i < sizeX && j < sizeY && k < sizeZ)
								    // Если точка не занята (-1), то присваиваем следующий шаг
								    if (Map[i, j, k] == -1) Map[i, j, k] = step + 1;
						    }
					    }
				    }
			    }
		    }
		    
//		    Debug.Log("Step " + step + " ended");
		    step++;
		    
	    } while (Map[endX, endY, endZ] == -1 && step < 40);    // Пока не достигнем финишной точки
		
//		Debug.Log("FP = " + Map[endX, endY, endZ]);
		
//		Debug.Log("Expansion finished");

	}

	void Backtrace()
	{
		// Добавляем конечную точку в путь
//		path.Add();
		
		// Начинаем со значения финишной точки
		int step = Map[endX, endY, endZ];
		
		// Начальные значения x, y, z
		int x = endX, y = endY, z = endZ;

		do
		{
			// Просматриваем её соседей. Делаем цикл по ближайшим элементам
			for (int i = x - 1; i <= x + 1; i++)
				for (int j = y - 1; j <= y + 1; j++)
					for (int k = z - 1; k <= z + 1; k++)
					{
						//Проверяем, не крайняя ли она
						if (i >= 0 && j >= 0 && k >= 0 && i < sizeX && j < sizeY && k < sizeZ)
							// Если точка не занята (-1), то присваиваем следующий шаг
							if (Map[i, j, k] == -1) Map[i, j, k] = step + 1;
		}
			
		} while (useGUILayout);
	}
}

//
//namespace AStarInMatrix
//{
//    class AStar
//    {
//        int[,] Map;
//        int MapWidht;
//        int MapHeight;
//
//        int[,] WayMap;
//        /// <summary>
//        /// Конструктор
//        /// </summary>
//        public void ReadMap()
//        {
//            MapWidht = 16;
//            MapHeight = 9;
//            Map = new int[,]{
//                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
//                {1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1},
//                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}};
//            WayMap = new int[10, 10];
//        }
//        /// <summary>
//        /// Отображение карты
//        /// </summary>
//        public void DrawMap()
//        {
//            for (int y = 0; y < MapHeight; y++)
//            {
//                Console.WriteLine();
//                for (int x = 0; x < MapWidht; x++)
//                    if (Map[y,x]==1)
//                        Console.Write("+");
//                    else
//                        Console.Write(" ");
//            }
//            Console.ReadKey();
//            FindWave(1, 1, 3, 4);
//        }
//        /// <summary>
//        /// Поиск пути
//        /// </summary>
//        /// <param name="startX">Координата старта X</param>
//        /// <param name="startY">Координата старта Y</param>
//        /// <param name="targetX">Координата финиша X</param>
//        /// <param name="targetY">Координата финиша Y</param>
//        public void FindWave(int startX, int startY, int targetX, int targetY)
//        {
//            bool add=true;
//            int[,] cMap = new int[MapHeight,MapWidht];
//            int x, y,step=0;
//            for (y = 0; y < MapHeight; y++)
//                for (x = 0; x < MapWidht; x++)
//                {
//                    if (Map[y, x] == 1)
//                        cMap[y, x] = -2;//индикатор стены
//                    else
//                        cMap[y, x] = -1;//индикатор еще не ступали сюда
//                }
//            cMap[targetY,targetX]=0;//Начинаем с финиша
//            while (add==true)
//            {
//                add = false;
//                for (y = 0; y < MapWidht; y++)
//                    for (x = 0; x < MapHeight; x++)
//                    {
//                        if (cMap[x, y] == step)
//                        {
//                            //Ставим значение шага+1 в соседние ячейки (если они проходимы)
//                            if (y - 1 >= 0 && cMap[x - 1, y] != -2 && cMap[x - 1, y] == -1)
//                                cMap[x - 1, y] = step + 1;
//                            if (x - 1 >= 0 && cMap[x, y - 1] != -2 && cMap[x, y - 1] == -1)
//                                cMap[x, y - 1] = step + 1;
//                            if (y + 1 < MapWidht && cMap[x + 1, y] != -2 && cMap[x + 1, y] == -1)
//                                cMap[x + 1, y] = step + 1;
//                            if (x + 1 < MapHeight && cMap[x, y + 1] != -2 && cMap[x, y + 1] == -1)
//                                cMap[x, y + 1] = step + 1;
//                        }
//                     }
//                step++;
//                add = true;
//                if (cMap[startY,startX] != -1)//решение найдено
//                    add = false;
//                if (step > MapWidht * MapHeight)//решение не найдено
//                    add = false;
//            }
//            //Отрисовываем карты
//            for (y = 0; y < MapHeight; y++)
//            {
//                Console.WriteLine();
//                for (x = 0; x < MapWidht; x++)
//                    if (cMap[y, x] == -1)
//                        Console.Write(" ");
//                    else
//                    if (cMap[y, x] == -2)
//                            Console.Write("#");
//                    else
//                    if (y == startY && x == startX)
//                            Console.Write("S");
//                    else
//                    if (y == targetY && x == targetX)
//                            Console.Write("F");
//                    else
//                    if (cMap[y, x] > -1)
//                        Console.Write("{0}", cMap[y, x]);
//
//            }
//            Console.ReadKey();
//        }
//    }
//}