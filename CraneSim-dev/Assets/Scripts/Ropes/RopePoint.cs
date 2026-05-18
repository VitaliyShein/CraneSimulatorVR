using UnityEngine;
using System.Collections;

public class RopePoint : MonoBehaviour {
	//Старая позиция точки
	public Vector3 prevPos = Vector3.zero;
	//Текущая позиция точки
	public Vector3 curPos = Vector3.zero;
	//Ускорение точки
	public Vector3 a = Vector3.zero;
	//Масса точки
	public float mass = 0;
	//Инверcное значение массы 1/m
	public float invMass;
	//Активность точки
	//true - имеет массу и действует гравитация
	//false - массы нет и гравитация не действует
	public bool IsActive = true;
	//Изменение длины с соединяемой точкой
    public bool IsChangeLen = false;
	//Ограничение (без массы)
	public bool IsHardConstraint = false;
	//Физичность
	public bool IsPhysics = false;
	//Соединение с другими точками
	public bool IsConnected = false;
	//Точки, с которой соединена данная точка
	public RopePoint[] points;
	//Длины соединений до точек, соединеных с данной
	public float[] distToPoints;
	//Максимальная дистанция до следующей точки
	[HideInInspector] public float maxDistToPoints;
	//Минимальная дистанция до следующей точки
	[HideInInspector] public float minDistToPoints;
	//RigidBody компонент точки, если он физичен
	public Rigidbody rb;
	//Количество точек, с которыми соединяется данная точка
	public int countPoint;
	//Сумма сил, приложенные к RigidBody данной точки
	public Vector3 F = Vector3.zero;

	//Определение количества точек, с которыми соединяется данная точка
	public void SpotCountPoint() {
		countPoint = points.Length;
	}

	//Определение длины соединений с точками
	public void SpotDistToPoint() {
		if (IsConnected) {
			for (int i = 0; i < countPoint; i++) {
				if (points [i] != null) {
					distToPoints [i] = CraneFunctions.SpotDistance (curPos, points [i].transform.position);
				} 
			}
				
		}
	}

	//Определение ссылки на компонент RigidBody
	public void SpotRigidbody() {
		if (IsPhysics) {
			rb = GetComponent<Rigidbody> ();
			if (rb != null)
				mass = rb.mass;
		}
	}

	//Определение инверсной массы
	public void SpotInvMass() {
		if (IsActive && mass != 0)
			invMass = 1.0f / mass;
	}

	//Определение позиции в данном кадре
	public void SpotCurPosition() {
		Vector3 pos = this.transform.position;
		curPos.Set (pos.x, pos.y, pos.z);
	}

	//Определение старой позиции точки
	public void SpotPrevPosition() {
		prevPos.Set (curPos.x, curPos.y, curPos.z);
	}

	//Аккумуляция ускорений точки
	public void AddAccel(Vector3 q) {
		a.x += q.x;
		a.y += q.y;
		a.z += q.z;
	}

	//Сброс ускорений точки
	public void ResetAccel() {
		a.Set (0, 0, 0);
	}

	//Функция Верле
	public void SpotVerlet(float dt) {
		Vector3 temp = new Vector3 (curPos.x, curPos.y, curPos.z);
		curPos.x += curPos.x - prevPos.x + a.x * dt * dt;
		curPos.y += curPos.y - prevPos.y + a.y * dt * dt;
		curPos.z += curPos.z - prevPos.z + a.z * dt * dt;
		prevPos.Set (temp.x, temp.y, temp.z);
		ResetAccel ();
	}

	//Расчет ограничений соединений с учетом масс точек
	public void SpotConstraint() {
		for (int i = 0; i < countPoint; i++) {
			RopePoint point = points[i];
			float r = distToPoints [i];
            Vector3 d = point.curPos - curPos;
			float len = Mathf.Sqrt (d.x * d.x + d.y * d.y + d.z * d.z);
            if (len > r)
            {
                float sumInvMass = invMass + point.invMass;
                d *= (len - r) / (len * sumInvMass);
                Vector3 dp1 = d * invMass;
                if (IsPhysics)
                {
                    F += dp1 / Time.fixedDeltaTime;
                }// else {
                curPos += dp1;
                //}
                if (point.IsActive)
                {
                    Vector3 dp2 = d * point.invMass;
                    if (point.IsPhysics)
                    {
                        point.F -= dp2 / Time.fixedDeltaTime;
                    }// else {
                    point.curPos -= dp2;
                    //}
                }
            }
		}
	}

	//Применение изменений позиций
	public void SetPosition() {
		transform.position = curPos;
	}

	//Применение сил к RigidBody точки
	public void SetForce() {
		if (IsPhysics) {
			rb.AddForce (F, ForceMode.VelocityChange);
			F.Set (0, 0, 0);
		}
	}

	//Добавление новой точки, с которой соединяется данная точка
	public void AddConnectedPoint(RopePoint point)
	{
		CheckPoints ();
		//Изменяем массив points
		RopePoint[] tempPointArray = points;
		points = new RopePoint[countPoint + 1];
		for (int i = 0; i < countPoint; i++) {
			points [i] = tempPointArray [i];
		}
		points [countPoint] = point;

		//Изменяем массив distToPoints
		float[] tempDistArray = distToPoints;
		distToPoints = new float[countPoint + 1];
		for (int i = 0; i < countPoint; i++) {
			distToPoints [i] = tempDistArray [i];
		}
		distToPoints [countPoint] = CraneFunctions.SpotDistance (this.transform.position, point.transform.position);
		countPoint += 1;
	}

	void CheckPoints() {
		int count = 0;
		for (int i = 0; i < countPoint; i++) {
			if (points [i] == null)
				count += 1;
		}
		if (count != 0) {
			RopePoint[] tempPointArray = points;
			float[] tempDistArray = distToPoints;
			points = new RopePoint[countPoint - count];
			distToPoints = new float[countPoint - count];
			for (int i = 0, j = 0; i < countPoint; i++) {
				if (tempPointArray [i] != null) {
					points [j] = tempPointArray [i];
					distToPoints [j] = tempDistArray [i];
					j += 1;
				}
			}
			countPoint -= count;
		}
	}

	//Изменение длины соединений
	public void ChangeLen(float delta)
	{
		if (IsChangeLen)
		{
			for (int i = 0; i < countPoint; i++)
			{
//				Debug.Log("distToPoints[i] " + distToPoints[i] + " minDistToPoints " + minDistToPoints + " maxDistToPoints " + maxDistToPoints + " " + delta);
				if (distToPoints[i] - delta >= minDistToPoints && distToPoints[i] - delta <= maxDistToPoints)
				{
//					Debug.Log("Работаем");
					distToPoints[i] -= delta;
				}
				else if (distToPoints[i] - delta <= minDistToPoints)
				{
//					Debug.Log("Хватит сворачивать трос!");
				}
				else if (distToPoints[i] - delta >= maxDistToPoints)
				{
//					Debug.Log("Хватит разворачивать трос!");
				}
			}
		}
	}

	//установка ограничений на расстояние между точками
	public void SpotLenghtConstraints()
	{
		minDistToPoints = 0.5f;
		maxDistToPoints = 13f;
	}
}
