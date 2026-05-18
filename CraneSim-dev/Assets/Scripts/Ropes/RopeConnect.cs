using UnityEngine;
using System.Collections;

public class RopeConnect : MonoBehaviour {
	public GameObject hook;
    public GameObject cargo;
    public GameObject slingConnection;
    //Vector3 center;
    [SerializeField] private float timeOfCentering = 3f;//время проесса центровки крюка относительно груза 
    private float timeOfStartCentering;
    private float timeOfEndCentering;                 
    bool connectingProcess = false;        //процесс сцепки
    public bool isCentered = false;
    public bool isConnect = false;         //соединение крюка и груза
    public bool inTrigger = false;         //крюк зашел в триггер груза
//    public bool workOfTheRiggerIsDone = false;
//    public GameObject slingerLogic;        
    bool pressingButton;                   //кнопка M была нажата меньше 3 сек назад
//    RiggerWorkerLogic logicScript;
    public GameObject[] firstPointOfSlings;
    
    //переменные для центровки
    private Vector3 hookStartPosition;
    private Vector3 hookFinishPosition;

    private Coroutine centeringProcess;

	void Start () {
//        logicScript = slingerLogic.GetComponent<RiggerWorkerLogic>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.M))  //при нажатии M
        {
            if (!isConnect && !pressingButton && inTrigger)
            {
//                logicScript.cargo = cargo.transform;
//                logicScript.startWork = true;
                //запускает процесс сцепки
                connectingProcess = true;
                //делаем крюк кинематичным
                hook.GetComponent<Rigidbody>().isKinematic = true;
                StartPressTime();
            }
            else if (isConnect && !pressingButton)
            {
                //отцепка
                GetComponent<RopeVerlet>().Disconnect();
                isConnect = false;
                connectingProcess = false;
                StartPressTime();

                //удаляем FJ с груза, если они есть, а крюк уже за пределами триггера
                if (!inTrigger)
                {
                    FixedJoint[] fj = cargo.GetComponents<FixedJoint>();
                    if (fj.Length != 0)
                    {
                        for (int i = 0; i < fj.Length; i++)
                        {
                            Destroy(fj[i]);
                        }
                    }
                }
            }
        }

        if (connectingProcess)
        {
            if (centeringProcess == null)
                centeringProcess = StartCoroutine(Сentering());
//            Сentering();
        }
        
        if (Time.time > timeOfEndCentering) //отрезаем возможность нажать М повторно в теч. 3 сек
            pressingButton = false;
        
//        if (workOfTheRiggerIsDone)  //когда стропальщик поднимет рукии к крюку
//        {
//            Parenting(hook, slingConnection);    //ToDo: delete!!!
//            hook.GetComponent<Rigidbody>().isKinematic = false;
//        }
    }
        

    private IEnumerator Сentering () //процесс центровки
    {
        Vector3 hookStartPosition = hook.transform.position;
        Vector3 hookFinishPosition = HookConnectionCoordinates();

        Quaternion hookStartAngle = hook.transform.rotation;
        
        //Vector3 distance = hookStartPosition - hookFinishPosition;

        bool connected = false;

        while (!connected)
        {
            Vector3 distance = hook.transform.position - HookConnectionCoordinates();
            if (Mathf.Abs(distance.x) <= 0.002 && Mathf.Abs(distance.y) <= 0.002 && Mathf.Abs(distance.z) <= 1) //если расстояние м/у точкой подцепа меньше 0,02 -> соединяем
            {
                GetComponent<RopeVerlet>().Connect();
                isConnect = true;
                //hook.GetComponent<Rigidbody>().isKinematic = false;
                connectingProcess = false;

                ContactObject co = cargo.GetComponent<ContactObject>();
                co.HookBlink.GetComponent<Renderer>().enabled = true; //прекращаем анимацию крюка
//            co.GetComponent<Animator>().enabled = true; //ToDo:выяснить, зачем это нужно?

                FixedJointingOfSlingsToHook(); // соединяем точки и крюк
                FollowToOff();
                HookKinematicOff();
                VerletPointActive(true);

//            Parenting(hook, slingConnection);    //ToDo: Не "parenting", а "fix joint"
//            hook.GetComponent<Rigidbody>().isKinematic = false;
                connected = true;
                centeringProcess = null;
            }
            else //если нет, приближаем крюк к месте подцепа
            {
                float timeProgress = (Time.time - timeOfStartCentering) / timeOfCentering;
                hook.transform.position = Vector3.Lerp(hookStartPosition, hookFinishPosition, timeProgress);
//                hook.transform.rotationeulerAngles = Vector3.Lerp(hookStartAngle, Vector3.zero, timeProgress);
                hook.transform.rotation = Quaternion.Lerp(hookStartAngle, Quaternion.identity, timeProgress);
                //здесь же выравниваем наклон крюка
                //Debug.Log("centering " + timeProgress);
                
                yield return null;
                
                
            }
        }
    }

    // Возвращает место перемещения крюка
    private Vector3 HookConnectionCoordinates() 
    {
        Vector3 connectionVector;
        
        // Возвращаем смещение от центра груза в зависимости от тэга
        switch (transform.tag)
        {
            case "Tubes":
                connectionVector = new Vector3(0, 3.2f, 0);
                break;
            case "BigContainer":
                connectionVector = new Vector3(0, 3.2f, 0);
                break;
            default:
                connectionVector = new Vector3(0, 3.2f, 0);   
                break;
        }
        
        return cargo.transform.position + connectionVector;
    }

    private void StartPressTime() //для контроля одиночного нажатия
    {
        timeOfStartCentering = Time.time;
        timeOfEndCentering = timeOfStartCentering + timeOfCentering;
        pressingButton = true;
    }

    private void Parenting(GameObject parent, GameObject children)
    {
        children.transform.parent = parent.transform;
    }

    private void FixedJointingOfSlingsToHook()
    {
            FixedJoint j5 = hook.AddComponent<FixedJoint>();
            j5.connectedBody = firstPointOfSlings[0].GetComponent<Rigidbody>();
            FixedJoint j6 = hook.AddComponent<FixedJoint>();
            j6.connectedBody = firstPointOfSlings[1].GetComponent<Rigidbody>();
            FixedJoint j7 = hook.AddComponent<FixedJoint>();
            j7.connectedBody = firstPointOfSlings[2].GetComponent<Rigidbody>();
            FixedJoint j8 = hook.AddComponent<FixedJoint>();
            j8.connectedBody = firstPointOfSlings[3].GetComponent<Rigidbody>();
    }

    public void FollowToOff()
    {
        for (int i = 0; i < firstPointOfSlings.Length; i++)
        {
            FollowTo ft = firstPointOfSlings[i].GetComponent<FollowTo>();
            ft.enabled = false;
        }

    }

    public void HookKinematicOff()
    {
        hook.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void VerletPointActive(bool active)
    {
        for (int i = 0; i < firstPointOfSlings.Length; i++)
        {
            RopePoint rp = firstPointOfSlings[i].GetComponent<RopePoint>();
            rp.IsActive = active;
        }

    }
}
