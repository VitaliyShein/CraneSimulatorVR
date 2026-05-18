using UnityEngine;
using System.Collections;

public class RopeGeneration : MonoBehaviour {

    public int amountOfRopePoints;      //количество точек в тросе
    public int amountOfSlingPoints;     //количество точек в стропе
    const int amountOfRopes = 4;        //количество тросов
    public GameObject pointRopePrefab;
    public GameObject pointSlingPrefab;
    public GameObject[] parentForRope;             //массив родительских элементов для тросов
    public GameObject[] startRopePoints;        //массив начальных точек для тросов
    public GameObject[] endRopePoints;          //массив конечных точек для тросов
    public GameObject[] parentForSling;             //массив родительских элементов для строп
    public GameObject[] startSlingPoints;        //массив начальных точек для строп
    public GameObject[] endSlingPoints;          //массив конечных точек для строп
    GameObject prevPoint;

    void Start () {
        RopeGenerate(0, startRopePoints, endRopePoints, amountOfRopePoints, pointRopePrefab, parentForRope);
    }

    public void RopeGenerate(int startIndex, GameObject[] startPoints, GameObject[] endPoints, int amountOfPoints, GameObject prefab, GameObject[] parent)
        //startIndex: 0-трос, 4-стропы
    {
        for (int i = startIndex, k = 0; i < startIndex + 4; i++, k++) //для каждого из 4х тросов // i для названий, k - счетчик для всего остального
        {
            Vector3 difference = (startPoints[k].transform.position - endPoints[k].transform.position); //вычисляем разницу между начальной и конечной точкой
            Vector3 distance = difference / (amountOfPoints - 1); //делим на количество отрезков, чтобы получить равную дистанцию между точками
            //distance += new Vector3 (0.5f, 0.5f, 0.5f);   //todo delete!!!!
            for (int j = 2; j < amountOfPoints; j++) //начинаем с j=2, т.е. точки №2
            {
                GameObject point = Instantiate(prefab, startPoints[k].transform.position - (j - 1) * distance, Quaternion.identity, parent[k].transform) as GameObject;//создаем объект из префаба
                point.name = "point." + (i + 1) + "." + j;//называем объект
                //далее вставляем ссылку на предыдущий элемент троса
                if (j == 2)//для первой точки вставляем уже существующий
                {
                    RopePoint pointComponent = point.GetComponent<RopePoint>();//получаем свой скриптовый компонент 
                    RopePoint prevPointComponent = startPoints[k].GetComponent<RopePoint>();//получаем компонент RopePoint предыдущего объекта
                    pointComponent.AddConnectedPoint(prevPointComponent);
                }
                else
                {
                    RopePoint pointComponent = point.GetComponent<RopePoint>();//получаем свой скриптовый компонент 
                    RopePoint prevPointComponent = prevPoint.GetComponent<RopePoint>();//получаем компонент RopePoint предыдущего объекта
                    pointComponent.AddConnectedPoint(prevPointComponent);
                }

                if (j == amountOfPoints - 1) //в последней итерации добавим в последнюю точку ссылку на текущую
                {
                    RopePoint lastPointComponent = endPoints[k].GetComponent<RopePoint>();//получаем свой скриптовый компонент 
                    RopePoint pointComponent = point.GetComponent<RopePoint>();//получаем компонент RopePoint предыдущего объекта
                    lastPointComponent.AddConnectedPoint(pointComponent);
                    if (startIndex == 0)
                        endPoints[k].name = "point." + (i + 1) + "." + (j + 1) + ".ConHook";
                    if (startIndex == 4)
                        endPoints[k].name = "point." + (i + 1) + "." + (j + 1) + ".ConCargo";
                }
                prevPoint = point;
            }
        }
    }
}
