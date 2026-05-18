using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RiggerWorkerController : MonoBehaviour {

    public enum WorkType { idle = 0, Rigging = 1 }
    public enum RiggingWorkStates { testThings = -1, state0 = 0, state1 = 1, state2 = 2, state3 = 3, state4 = 4, state5 = 5, state6 = 6, state7 = 7, state8 = 8  }

    public WorkType workType;
    public RiggingWorkStates riggingWorkStates;

    public Transform cargo = null;
    public Transform hook;
    public Transform goAwayPoint;
    public Transform connectionPoint;
    //public bool activate = false;
    public bool waitingForStrapping = false;
    public bool waitingForUnStrapping = false;
    public bool i_done_work = true;

    Transform arm;
    public RopeConnect ropeConnect;

    float lookIKWeight = 0.75f; //"Желание" следить за объектом, 0 - не будет следить
    float eyesWeight = 1;
    float headWeight = 0.75f;
    float bodyWeight = 0.5f;
    float clampWeight = 1; //Ограничение поворота, 0 – не ограничено
    Vector3 targetToLook;


    //Для Тестов
    public Ray rayForGizmos;
    public Vector3 testPoint;
    //


    float maxWalkSpeed = 2.2f;
    float rotateSpeed = 2.5f;
    float ladderSpeed = 0.65f;
    //bool[] statesBools = { true, false, false, false, false, false, false, false };
    bool ststrappingOver = false;

    Vector3[] points;
    BoxCollider cargosCollider;

    Animator anim;
    NavMeshAgent nav;

    Vector3 _prevPosition;

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        targetToLook = hook.position;
        arm = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0);
        //Debug.Log(arm.name);
    }

    void PointsGenerator(string tag) {
        if (tag == "BigContainer") {
            points = new Vector3[6];
            cargosCollider = cargo.GetComponent<BoxCollider>();
            //point 0
            float x = -cargosCollider.size.x / 2.6f;
            float y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            float z = -cargosCollider.size.z / 1.3f;
            float angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;



            float xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            float zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            RaycastHit hit;
            rayForGizmos = new Ray(new Vector3(xx, y, zz), Vector3.down);
            Physics.Raycast(rayForGizmos, out hit);
            points[0] = hit.point;
            testPoint = hit.point;

            //points[0] = new Vector3(xx, hit.transform.position.y, zz);

            //point 1
            x = -cargosCollider.size.x / 2.6f;
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            z = -cargosCollider.size.z / 1.8f;
            angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;

            xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            rayForGizmos = new Ray(new Vector3(xx, y, zz), Vector3.down);
            Physics.Raycast(rayForGizmos, out hit);

            points[1] = hit.point;

            //point 2
            x = -cargosCollider.size.x / 2.6f;
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            z = -cargosCollider.size.z / 1.8f;
            angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;

            xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            points[2] = new Vector3(xx, y, zz);

            //point 3
            x = -cargosCollider.size.x / 2.6f;
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            z = -cargosCollider.size.z / 4f;
            angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;

            xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            points[3] = new Vector3(xx, y, zz);

            //point 4 center
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            xx = cargosCollider.transform.position.x;
            zz = cargosCollider.transform.position.z;
            points[4] = new Vector3(xx, y, zz);

            if (goAwayPoint != null)
                points[5] = goAwayPoint.position;

        }
        if (tag == "PipeBundle") {
            points = new Vector3[6];
            cargosCollider = cargo.GetComponent<BoxCollider>();
            //point 0
            float x = -cargosCollider.size.x / 2.6f;
            float y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            float z = -cargosCollider.size.z / 0.6f;
            float angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;



            float xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            float zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            RaycastHit hit;
            rayForGizmos = new Ray(new Vector3(xx, y, zz), Vector3.down);
            Physics.Raycast(rayForGizmos, out hit);
            points[0] = hit.point;
            testPoint = hit.point;

            //points[0] = new Vector3(xx, hit.transform.position.y, zz);

            //point 1
            x = -cargosCollider.size.x / 2.6f;
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            z = -cargosCollider.size.z / 0.9f;
            angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;

            xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            rayForGizmos = new Ray(new Vector3(xx, y, zz), Vector3.down);
            Physics.Raycast(rayForGizmos, out hit);

            points[1] = hit.point;

            //point 2
            x = -cargosCollider.size.x / 2.6f;
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            z = -cargosCollider.size.z / 0.9f;
            angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;

            xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            points[2] = new Vector3(xx, y, zz);

            //point 3
            x = -cargosCollider.size.x / 2.6f;
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            z = -cargosCollider.size.z / 2f;
            angY = Mathf.Deg2Rad * cargosCollider.transform.rotation.eulerAngles.y;

            xx = cargosCollider.transform.position.x + (x * Mathf.Cos(angY) - z * Mathf.Sin(angY));
            zz = cargosCollider.transform.position.z - (x * Mathf.Sin(angY) + z * Mathf.Cos(angY));
            points[3] = new Vector3(xx, y, zz);

            //point 4 center
            y = cargosCollider.transform.position.y + cargosCollider.size.y / 2;
            xx = cargosCollider.transform.position.x;
            zz = cargosCollider.transform.position.z;
            points[4] = new Vector3(xx, y, zz);

            if (goAwayPoint != null)
                points[5] = goAwayPoint.position;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (workType.ToString() == "idle")
        {
            lookIKWeight = 0;
        }
        if (workType.ToString() == "Rigging")
        {
            switch (riggingWorkStates)
            {
                case RiggingWorkStates.testThings:
                    {
                        /*lookIKWeight = 1;
                        targetToLook = hook.position;
                        Vector3 cargoAndHook = cargo.position - hook.position;
                        Vector3 craneAndCargo = Camera.main.transform.position - cargo.position;
                        if (craneAndCargo.z > 0) // z
                        {
                            if (craneAndCargo.x > 0) //Первая четверть x z
                            {
                                Debug.Log("Первая четверть x z");
                                if (Mathf.Abs(cargoAndHook.x) > Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.x > 0)
                                        Debug.Log("Влево");
                                    else
                                        Debug.Log("Вправо");
                                if (Mathf.Abs(cargoAndHook.x) <= Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.z > 0)
                                        Debug.Log("Назад");
                                    else
                                        Debug.Log("Вперед");
                            }
                            else //Вторая четверть -x z
                            {
                                Debug.Log("Вторая четверть -x z");
                                if (Mathf.Abs(cargoAndHook.x) > Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.x > 0)
                                        Debug.Log("Влево");
                                    else
                                        Debug.Log("Вправо");
                                if (Mathf.Abs(cargoAndHook.x) <= Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.z > 0)
                                        Debug.Log("Назад");
                                    else
                                        Debug.Log("Вперед");
                            }
                        }
                        else // -z
                        {
                            if (craneAndCargo.x > 0) //Четвертая четветь x -z
                            {
                                Debug.Log("Четвертая четветь x -z");
                                if (Mathf.Abs(cargoAndHook.x) > Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.x > 0)
                                        Debug.Log("Вправо");
                                    else
                                        Debug.Log("Влево");
                                if (Mathf.Abs(cargoAndHook.x) <= Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.z > 0)
                                        Debug.Log("Вперед");
                                    else
                                        Debug.Log("Назад");
                            }
                            else //Третья четверть -x -z
                            {
                                Debug.Log("Третья четверть -x -z");
                                if (Mathf.Abs(cargoAndHook.x) > Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.x > 0)
                                        Debug.Log("Вправо");
                                    else
                                        Debug.Log("Влево");
                                if (Mathf.Abs(cargoAndHook.x) <= Mathf.Abs(cargoAndHook.z))
                                    if (cargoAndHook.z > 0)
                                        Debug.Log("Вперед");
                                    else
                                        Debug.Log("Назад");
                            }
                        }
                        Debug.Log(craneAndCargo);
                        */
                        break;
                    }
                case RiggingWorkStates.state0: // Формирование путиводных точек
                    {
                        Debug.Log(cargo);
                        if (cargo.tag == "BigContainer")
                        {
                           
                        }
                        riggingWorkStates = RiggingWorkStates.state1;
                        break;
                    }
                case RiggingWorkStates.state1: //Перемещение к грузу 
                    {
                        nav.enabled = true;
                        lookIKWeight = 0.3f; //
                        PointsGenerator(cargo.tag);

                        targetToLook = cargo.position;
                        anim.applyRootMotion = false;
                        nav.SetDestination(points[0]); //Точка назначение. Необходимо расчитать
                        anim.SetBool("walk", true);
                        nav.Resume();
                        if (Vector3.Distance(transform.position, nav.destination) < 0.01f)
                        {
                            lookIKWeight = 0.6f;
                            targetToLook = points[2];
                            anim.SetBool("walk", false);
                            nav.Stop();
                            nav.enabled = false;
                            riggingWorkStates = RiggingWorkStates.state2;
                        }
                        break;
                    }
                case RiggingWorkStates.state2: //Начало подъема
                    {
                        float angle = LookAtPoint(points[1], rotateSpeed);

                        if (angle <= 0.1f && angle >= -0.1f)
                        {
                            riggingWorkStates = RiggingWorkStates.state3;
                            anim.SetBool("ladderUp", true);
                        }
                        break;
                    }
                case RiggingWorkStates.state3: //Подъем
                    {
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("EnterLadderBottom"))
                            anim.applyRootMotion = true;
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("ClimbLadder"))
                        {
                            if (Vector3.Distance(transform.position, points[2]) < 1.1f)
                            {
                                anim.SetBool("ladderUpExit", true);
                                anim.SetBool("ladderUp", false);
                            }
                        }
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                            transform.position = Vector3.MoveTowards(transform.position, points[3], Time.deltaTime * (ladderSpeed + 0.5f));
                        if (Vector3.Distance(transform.position, points[3]) <= 0.01f)
                        {
                            targetToLook = hook.position;
                            anim.SetBool("ladderUpExit", false);
                            riggingWorkStates = RiggingWorkStates.state4;
                            anim.applyRootMotion = false;
                        }
                        break;
                    }
                case RiggingWorkStates.state4: //К центру               
                    {
                        targetToLook = points[4];
                        anim.SetBool("walk", true);
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("WalkBlend"))
                        {
                            Vector3 relative = transform.InverseTransformPoint(points[4]);
                            float angle = (Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg) * Time.deltaTime * rotateSpeed;
                            if (angle >= 0.1f || angle <= -0.1f)
                                transform.Rotate(0, angle, 0);
                            transform.position = Vector3.MoveTowards(transform.position, points[4], Time.deltaTime * maxWalkSpeed);
                        }
                        if (Vector3.Distance(transform.position, points[4]) < 0.5f)
                        {
                            anim.SetBool("walk", false);
                            riggingWorkStates = RiggingWorkStates.state5;
                        }
                        break;
                    }
                case RiggingWorkStates.state5: //Страповка
                    {
                        targetToLook = hook.position;
                        anim.applyRootMotion = true;
                        if (waitingForStrapping && waitingForUnStrapping)
                        {
                            waitingForStrapping = false;
                            waitingForUnStrapping = false;
                        }
                        if (waitingForStrapping)
                            anim.SetBool("strapping", true);
                        if (waitingForUnStrapping)
                            anim.SetBool("unstrapping", true);
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("strapping") || anim.GetCurrentAnimatorStateInfo(0).IsName("unstrapping"))
                        {
                            Debug.Log(hook.transform.position.y - connectionPoint.position.y);      //
                            if (anim.GetCurrentAnimatorStateInfo(0).IsName("strapping") && hook.transform.position.y - connectionPoint.position.y > 0.14f)
                            {
                                connectionPoint.transform.position = new Vector3(connectionPoint.transform.position.x, arm.position.y, connectionPoint.transform.position.z);
                                
                            }
                            //Debug.Log(connectionPoint.position.y - hook.transform.position.y);
                            if (anim.GetCurrentAnimatorStateInfo(0).IsName("unstrapping") && hook.transform.position.y - connectionPoint.position.y < 1.35f)  //НАСТРОИТЬ!
                            {
                                connectionPoint.transform.position = new Vector3(connectionPoint.transform.position.x, arm.position.y, connectionPoint.transform.position.z);

                            }
                            anim.SetBool("strapping", false);
                            anim.SetBool("unstrapping", false);
                            ststrappingOver = true;
                        }
                        /*if (anim.GetCurrentAnimatorStateInfo(0).IsName("unstrapping"))
                        {
                            anim.SetBool("unstrapping", false);
                            ststrappingOver = true;
                        }*/
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && ststrappingOver)
                        {
                            anim.SetBool("strapping", false);
                            anim.SetBool("unstrapping", false);
                            waitingForStrapping = false;
                            waitingForUnStrapping = false;
                            ststrappingOver = false;
                            riggingWorkStates = RiggingWorkStates.state6;
                        }
                        break;
                    }
                case RiggingWorkStates.state6: //Назад к спуску
                    {
                        //соединяем точки и крюк
//                        ropeConnect.FixedJointingOfSlingsToHook(); // соединяем точки и крюк
//                        ropeConnect.FollowToOff();
//                        ropeConnect.HookKinematicOff();
//                        ropeConnect.VerletPointActive(true);

                        targetToLook = points[2];
                        anim.applyRootMotion = false;
                        float angle = LookAtPoint(points[2], rotateSpeed);

                        if (angle <= 0.1f && angle >= -0.1f)
                            anim.SetBool("walk", true);
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("WalkBlend"))
                            transform.position = Vector3.MoveTowards(transform.position, points[3], Time.deltaTime * maxWalkSpeed);
                        if (Vector3.Distance(transform.position, points[3]) < 0.01f)
                        {
                            anim.SetBool("walk", false);
                            float angleToLadder = LookAtPoint(points[2], rotateSpeed);
                            if (angleToLadder <= 0.1f && angleToLadder >= -0.1f)
                            {
                                anim.applyRootMotion = true;
                                anim.SetBool("ladderDown", true);
                                anim.SetBool("turnLeft", false);
                                anim.SetBool("turnRight", false);
                                riggingWorkStates = RiggingWorkStates.state7;
                            }
                        }
                            break;
                        

                    }
                case RiggingWorkStates.state7: //Спуск с груза
                    {
                        targetToLook = points[1];
                        //Debug.Log(Vector3.Distance(transform.position, points[1]));
                        if (Vector3.Distance(transform.position, points[1]) < 0.4f)
                        {
                            anim.SetBool("ladderDownExit", true);
                            anim.SetBool("ladderDown", false);
                        }
                        if (Vector3.Distance(transform.position, points[0]) < 0.4f && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        {
                            anim.SetBool("ladderDownExit", false);
                            anim.applyRootMotion = false;
                            riggingWorkStates = RiggingWorkStates.state8;
                            nav.enabled = true;
                            i_done_work = true;
                        }
                        break;
                    }
                case RiggingWorkStates.state8: //Уход прочь
                    {
                        lookIKWeight = 0;
                        targetToLook = points[5];
                        nav.SetDestination(points[5]);
                        anim.SetBool("walk", true);
                        nav.Resume();

                        if (Vector3.Distance(transform.position, points[5]) < 0.3f)
                        {
                            lookIKWeight = 0.75f;
                            workType = WorkType.idle;
                            resetAll();
                            nav.Stop();
                            //activate = false;
                        }
                        break;
                    }
            }
        }

    }

    public void resetAll()
    {
        riggingWorkStates = RiggingWorkStates.state0;
        cargo = null;
        anim.SetBool("walk", false);
        anim.SetBool("ladderUp", false);
        anim.SetBool("ladderUpExit", false);
        anim.SetBool("turnLeft", false);
        anim.SetBool("turnRight", false);
        anim.SetBool("ladderDown", false);
        anim.SetBool("ladderDownExit", false);
        anim.SetBool("strapping", false);
        anim.SetBool("unstrapping", false);
        anim.applyRootMotion = false;
        nav.enabled = true;
        waitingForStrapping = false;
        waitingForUnStrapping = false;
        ststrappingOver = false;
        i_done_work = true;

    }

    float LookAtPoint(Vector3 destination, float speed = 3)
    {
        Vector3 relative = transform.InverseTransformPoint(destination);
        float angle = (Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg) * Time.deltaTime * rotateSpeed;
        if (angle >= 0.1f || angle <= -0.1f)
        {
            if (angle > 2)
                anim.SetBool("turnRight", true);
            if (angle < -2)
                anim.SetBool("turnLeft", true);
            transform.Rotate(0, angle, 0);

        }
        else
        {
            anim.SetBool("turnLeft", false);
            anim.SetBool("turnRight", false);
        }
        return angle;
    }

    void OnAnimatorIK() {
        if (targetToLook != null)
        {
            anim.SetLookAtWeight(lookIKWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            anim.SetLookAtPosition(targetToLook);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Vector3 size = new Vector3(0.3f, 0.3f, 0.3f);
        if (points != null)
            for (int i = 0; i < points.Length; i++)
                if (points[i] != null)
                    Gizmos.DrawWireSphere(points[i], 0.3f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(rayForGizmos);
        //Gizmos.DrawSphere(testPoint, 0.3f);
    }
}
