using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public class Engine : MonoBehaviour {

    [SerializeField]
    public enum EngineType { rotate = 0, move = 1, reach = 2, ChangeLength = 3 }
    public enum Axis { x = 0, y = 1, z = 2 }
    
    public EngineType engineType;       //тип двигателя (воздействие на объект)

    public float power;                 //мощность двигателя
    public int gearNow;                 //TODO:убрать Public
    public int gearsForward;            //количество положений вперед (н-р: 2)
    public int gearsBackward;           //количество положений назад (н-р: -3)

    public float timeOfStarting1;        //время разгона
    public float timeOfStarting2;
    public float timeOfBreaking1;        //время торможения
    public float timeOfBreaking2;
    float t1, t2;
    float k;                            //коэффициент для догонки / перехода между скоростями
    float collectedVelocity;            //сколько оборотов набрал на предыдущей передаче
    float timeElapsed;                  //прошло времени с момента переключения
    int gearOld;                        //для отслеживания состояния рычага на предыдущем кадре
    float outputVelocity;

    [HideInInspector] public bool blocked;    //в случае достижения ограничений
    
    private bool initilize;

    [Header("For moving")]
    public Axis axis;                   //оси, по которым будет осуществляться движение

    [Header("For animation")]
    public Animation anim;

    [Header("For changing length")]
    //public GameObject [] ropeObject;
    RopePoint[] ropePoints;


    void Start () {
        gearNow = 0;
        gearOld = 0;
        timeElapsed = 0;
        collectedVelocity = 0;
        outputVelocity = 0;
//        Debug.Log("In start, OV " + outputVelocity +" " + Time.frameCount );
        k = 0;

        switch (engineType)
        {
            case EngineType.reach:
                {
                    //Debug.Log("before line");
                    anim[anim.clip.name].time = /*0.01F*/0.98F;
                    //Debug.Log("after line");
                    break;
                }
            case EngineType.ChangeLength:
                ropePoints = GetComponentsInChildren<RopePoint>();
                break;
        }
    }
	
	void Update () {
        Control(); // выставляем передачу в зависимости от нажатой клавиши
        CheckingGears();

        if (IsGearChange()) // если передача поменялась
        {
            collectedVelocity = outputVelocity;
            k = gearNow * power - outputVelocity; 
            timeElapsed = 0;
        }

        if (gearNow == 0)
            SetT(timeOfBreaking1, timeOfBreaking2);
        else
            SetT(timeOfStarting1, timeOfStarting2);
//	    Debug.Log("before initialize "  + Time.frameCount);

	    if (!initilize)
	    {
//	        Debug.Log("in initialize " + Time.frameCount);
	        FixedUpdate();
	        initilize = true;
	    }
	    gearOld = gearNow;
}

    //просчет физики вынесен в FixedUpdate
    void FixedUpdate()
    {
        VelocityCalculation();
        //if(!blocked)
        Action();
    }

	//воздействуем на объекты в зависимости от типа двигателя
    void Action()
    {
        switch (engineType)
        {
            case EngineType.rotate:
                if (outputVelocity > 0.01f || outputVelocity <-0.01f)
                    Rotate(outputVelocity);
                break;
            case EngineType.move:
                if (outputVelocity > 0.01f || outputVelocity < -0.01f)
                    Move(outputVelocity);
                break;
            case EngineType.reach:
//                Debug.Log("in action, OV = " + outputVelocity + " " + Time.frameCount);
                    Reach(outputVelocity);
                break;
            case EngineType.ChangeLength:
                if (outputVelocity > 0.005f || outputVelocity < -0.005f)
                {
                    LengthChange(outputVelocity);
                }
                break;
        }
    }

    bool IsGearChange()
    {
        if (gearNow != gearOld)
            return true;
        else
            return false;
    }

	//считаем выходные обороты по формуле апериодического звена 2-го порядка
    void VelocityCalculation()
    {
        timeElapsed += Time.fixedDeltaTime;
        outputVelocity = collectedVelocity + k * (1 - t1 / (t1 - t2) * Mathf.Exp(-timeElapsed / t1)
            + t2 / (t1 - t2) * Mathf.Exp(-timeElapsed / t2)); 
//        Debug.Log("in veloc calc, output velocity " + outputVelocity + " " + Time.frameCount);
    }

    void SetT (float T1, float T2)
    {
        t1 = T1;
        t2 = T2;
    }

	//для поворота башни крана
    void Rotate(float angle)
    {
        Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle * Time.fixedDeltaTime*5);
        transform.rotation = rotation;
    }

	//для движения крана вперед/назад
    void Move(float displacement)
    {
        switch (axis)
        {
            case Axis.x:
                transform.position = new Vector3(transform.position.x + displacement / 10, transform.position.y, transform.position.z);
                break;
            case Axis.y:
                transform.position = new Vector3(transform.position.x, transform.position.y + displacement / 10, transform.position.z);
                break;
            case Axis.z:
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + displacement / 10);
                break;
        }
    }

	//для выноса стрелы
    void Reach(float animSpeed)
    {
        anim["tower_rotate"].speed = animSpeed / 10F;
        if (anim["tower_rotate"].time > 0.98F)
            anim["tower_rotate"].time = 0.98F;
        if (anim["tower_rotate"].time < 0.02F)
            anim["tower_rotate"].time = 0.02F;
    }

	//для изменения длины тросов
    void LengthChange(float outputVelocity)
    {
        for (int i = 0; i < ropePoints.Length; i++)
            ropePoints[i].ChangeLen(outputVelocity/150);
    }

    void CheckingGears()
    {
        if (gearNow > gearsForward)
            gearNow = gearsForward;
        if (gearNow < gearsBackward)
            gearNow = gearsBackward;
    }

	//изменение текущей скорости двигателя в зависимости от нажатой клавиши
    void Control()
    {
        switch (engineType)
        {
            case EngineType.rotate:
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    gearNow = 1;
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    gearNow = -1;
                if (Input.GetKeyDown(KeyCode.B))
                    gearNow = 0;
                break;
            case EngineType.move:
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    gearNow = 1;
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    gearNow = -1;
                if (Input.GetKeyDown(KeyCode.V))
                    gearNow = 0;
                break;
            case EngineType.reach:
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    gearNow = 1;
                if (Input.GetKeyDown(KeyCode.Alpha5))
                    gearNow = 2;
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    gearNow = -1;
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    gearNow = -2;
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    gearNow = 0;
                break;
            case EngineType.ChangeLength:
                if (Input.GetKeyDown(KeyCode.R))
                    gearNow = 3;
                if (Input.GetKeyDown(KeyCode.T))
                    gearNow = 2;
                if (Input.GetKeyDown(KeyCode.Y))
                    gearNow = 1;
                if (Input.GetKeyDown(KeyCode.I))
                    gearNow = -1;
                if (Input.GetKeyDown(KeyCode.O))
                    gearNow = -2;
                if (Input.GetKeyDown(KeyCode.P))
                    gearNow = -3;
                if (Input.GetKeyDown(KeyCode.U))
                    gearNow = 0;
                break;
        }
    }
}
