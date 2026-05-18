using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEditor;
using UnityEngine.EventSystems;


public class RiggerClass: RIGGER
{
    public enum RiggerType { RiggingAndSignaling = 0, RiggingOnly = 1, SignalingOnly = 2 }

    //public enum ActionType {walkOnNavMesh=0, walk=1, turnToTarget=2}
    
    //GoToPointOnNavMesh_CR_1(testPoint1.position);
    //TurnToPoint_CR_1(testPoint1.position);
    //UpOnLadder_CR_1(testPoint1.position);
    //GoToPointWithoutNavMesh_CR_1(testPoint1.position);
    //DownOnLadder_CR_1(testPoint1.position);
    //Debug.Log(CanIReachThatPoint(testPoint1.position));
    //Signals_CR_2(testPoint1.position, testPoint2.position);
    //FindPointNearToCargo(cargo);

    public RiggerType riggerType = RiggerType.RiggingAndSignaling;

    public bool shouldIWork = false;
    int processPointStatus = 0;
    int processPointCommandStatus = 0;
    //ПОТОМ ПРИВАТ

    
    public CargoClass cargo;
    public GameObject hook;
    [HideInInspector]
    public Transform connectionPoint;
    
    [HideInInspector]
    public Transform testPoint1;
    [HideInInspector]
    public Transform testPoint2;

    public bool signalersWork;
    bool doneSignals = false;
    
    Animator animator;
    public enum AnimatorBoolNameEnum  {sSTOP,walk,ladderUp,ladderUpExit,turnLeft,turnRight,ladderDown,ladderDownExit, strapping, unstrapping};
    string[] animatorBoolName = new string[10]{"sSTOP",
        "walk","ladderUp","ladderUpExit", "turnLeft", "turnRight",
        "ladderDown", "ladderDownExit", "strapping", "unstrapping"};
    NavMeshAgent navMesh;
    
    [HideInInspector]
    public float lookIKWeight = 0.75f; //"Желание" следить за объектом, 0 - не будет следить
    float handIKWeight = 0;
    float curentLookIKWeight = 0;
    float eyesWeight = 1;
    float headWeight = 0.75f;
    float bodyWeight = 0.5f;
    float clampWeight = 1; //Ограничение поворота, 0 – не ограничено
    Vector3 targetToLook;
    
    float maxWalkSpeed = 2.2f;
    float rotateSpeed = 2.5f;
    float ladderSpeed = 0.65f;
    
    int walkSpeed = 3;
    int angularSpeed = 800;
    
    int closeRange = 2;

    private bool enterLadderBool = true;
    private bool ststrappingOver = false;

    private Vector3[] gizmosPoints;

    private Transform arm;

    private bool riseIKEnable = false;

    [HideInInspector]
    public Transform myLeftHand;

    
    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        navMesh = GetComponent<NavMeshAgent>();
        arm = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0);
        lookIKWeight = 0;
        resetAll();
    }


    // Update is called once per frame
    void Update()
    {
        if (shouldIWork)
        {
            switch (riggerType)
            {
                case RiggerType.RiggingOnly:
                    if (cargo != null)
                    {
                        if (processPointCommandStatus == cargo.points[processPointStatus].point.commands.Length)
                        {
                            processPointStatus++;
                            processPointCommandStatus = 0;
                        }
                        if (processPointStatus == cargo.points.Count)
                        {
                            cargo.deliveryStatus = true;
                            processPointStatus = 0;
                            status = 0;
                            shouldIWork = false;
                            resetAll();
                            cargo = null;
                            return;
                        }
                      
                        object[] temp = new object[cargo.points[processPointStatus].point.commands[processPointCommandStatus].arguments.Length];
                        for (int i = 0; i < temp.Length; i++)
                            temp[i] = cargo.points[processPointStatus].point.commands[processPointCommandStatus].arguments[i].position;

                        if (cargo.points[processPointStatus].point.commands[processPointCommandStatus].arguments.Length == 1)
                            transform.SendMessage(cargo.points[processPointStatus].point.commands[processPointCommandStatus].nameOfCommand, temp[0]);
                        else
                            transform.SendMessage(cargo.points[processPointStatus].point.commands[processPointCommandStatus].nameOfCommand, temp);
                        if (status == 1)
                        {
                            processPointCommandStatus++;
                            status = 0;
                        }
                    }
                    break;
                case RiggerType.RiggingAndSignaling:
                    if (signalersWork)
                    {
                        if (cargo != null)
                        {
                            if (status == 0)
                            {
                                object temp = FindPointNearToCargo(cargo);
                                if (GoToPointOnNavMesh_CR_1(temp))
                                    status = 1;
                            }
                            if (status == 1)
                            {
                                object[] temp2 = new object[2]
                                    {hook.transform.position, cargo.points[2].transform.position};
                                if (Signals_CR_2(temp2))
                                {
                                    status = 0;
                                    signalersWork = false;
                                    shouldIWork = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cargo != null)
                        {
                            if (processPointCommandStatus == cargo.points[processPointStatus].point.commands.Length)
                            {
                                processPointStatus++;
                                processPointCommandStatus = 0;
                            }
                            if (processPointStatus == cargo.points.Count)
                            {
                                cargo.deliveryStatus = true;
                                processPointStatus = 0;
                                status = 0;
                                shouldIWork = false;
                                resetAll();
                                cargo = null;
                                return;
                            }
                            object[] temp = new object[cargo.points[processPointStatus].point
                                .commands[processPointCommandStatus].arguments.Length];
                            for (int i = 0; i < temp.Length; i++)
                                temp[i] = cargo.points[processPointStatus].point.commands[processPointCommandStatus]
                                    .arguments[i].position;

                            if (cargo.points[processPointStatus].point.commands[processPointCommandStatus]
                                    .nameOfCommand ==
                                "StrapingOnTopOfCargo_CR_0" && !doneSignals)
                            {
                                object[] temp2 = new object[2]
                                    {hook.transform.position, cargo.points[processPointStatus].transform.position};
                                doneSignals = Signals_CR_2(temp2);
                                if (doneSignals)
                                    status = 0;
                            }
                            else
                            {
                                if (cargo.points[processPointStatus].point.commands[processPointCommandStatus].arguments
                                        .Length == 1)
                                    transform.SendMessage(
                                        cargo.points[processPointStatus].point.commands[processPointCommandStatus]
                                            .nameOfCommand, temp[0]);
                                else
                                    transform.SendMessage(
                                        cargo.points[processPointStatus].point.commands[processPointCommandStatus]
                                            .nameOfCommand, temp);
                            }
                            if (status == 1)
                            {
                                processPointCommandStatus++;
                                status = 0;
                            }
                        }
                    }
                    break;
                case RiggerType.SignalingOnly:
                    if (signalersWork)
                    {
                        if (cargo != null)
                        {
                            if (status == 0)
                            {
                                object temp = FindPointNearToCargo(cargo);
                                if (GoToPointOnNavMesh_CR_1(temp))
                                    status = 1;
                            }
                            if (status == 1)
                            {
                                object[] temp2 = new object[2]
                                    {hook.transform.position, cargo.points[processPointStatus].transform.position};
                                if (Signals_CR_2(temp2))
                                {
                                    status = 0;
                                    signalersWork = false;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        if (riseIKEnable)
        {
            if (handIKWeight < 0.7f)
                handIKWeight += 0.8f * Time.deltaTime;
        }
        else
            if (handIKWeight > 0.01f)
                handIKWeight -= 0.8f * Time.deltaTime;

        if(Math.Abs(curentLookIKWeight - lookIKWeight) > 0.1f)
            if(curentLookIKWeight > lookIKWeight)
                curentLookIKWeight -= 2 * Time.deltaTime;
            else
                curentLookIKWeight += 2 * Time.deltaTime;
    }

    public bool superFunc(Transform target, AnimatorBoolNameEnum animation, bool navMeshWalkEnable = false, bool animatorRootMotion = false, Transform lookTarget = null, float lookWeight = 0 )
    {
        float angle = 0;
        //условие выхода
        switch (animation)
        {
            case AnimatorBoolNameEnum.walk:
                if (navMeshWalkEnable) 
                    if (Vector3.Distance(transform.position, navMesh.destination) < 0.01f)
                    {
                        navMesh.Stop();
                        navMesh.enabled = false;
                        resetAll();
                        status = 1;
                        return true;
                    }
                break;
            case AnimatorBoolNameEnum.turnLeft:
            case AnimatorBoolNameEnum.turnRight:
                angle = LookAtPoint(target.position, rotateSpeed, true);
                if (angle <= 0.1f && angle >= -0.1f)
                {
                    resetAll();
                    status = 1;
                    return true;
                }
                break;
            case AnimatorBoolNameEnum.ladderUp:
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("EnterLadderBottom"))
                {
                    resetAll();
                    status = 1;
                    return true;
                }
                break;
            case AnimatorBoolNameEnum.ladderUpExit:
                if(target.position.y - transform.position.y < 1.1f)
                {
                    animator.SetBool("ladderUp", false);
                    animator.SetBool("ladderUpExit", true);
                    // ДОПОЛНИТЕЛЬНОЕ Условие останова и выключение всего что включили 
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("TopExit") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.98f)
                    {
                        animator.SetBool("ladderUpExit", false);
                        status = 1;
                        return true;
                    }
                    return false;
                }
                break;
            case AnimatorBoolNameEnum.strapping:
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && ststrappingOver)
                {
                    animator.SetBool("strapping", false);
                    animator.SetBool("unstrapping", false);
                    ststrappingOver = false;
                    status = 1;
                    return true;
                }
                break;
            case AnimatorBoolNameEnum.sSTOP:
                if ((animator.GetCurrentAnimatorStateInfo(0).IsName("Stop_noEnter") || animator.GetCurrentAnimatorStateInfo(0).IsName("Stop")) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.98f)
                {
                    resetAll();
                    status = 1;
                    return true;
                }
                break;
            case AnimatorBoolNameEnum.ladderDown:
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("EnterLadderTopMod"))
                {
                    animator.SetBool("ladderDown", false);
                    status = 1;
                    return true;
                }
                break;
            case AnimatorBoolNameEnum.ladderDownExit:
                if(transform.position.y - target.position.y < 0.1f)
                {
                    animator.SetBool("ladderDownExit", true);
                    animator.SetBool("ladderDown", false);
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("ExitLadderBottom") &&animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.98f)
                    {
                        status = 1;
                        return true;
                    }
                    return false;
                }
                break;
        }
        
        //Настройка взгляда
        targetToLook = lookTarget.position;
        lookIKWeight = lookWeight;
        
        //Настройка Аниматора
        animator.applyRootMotion = animatorRootMotion;
        animator.SetBool(animation.ToString(), true);
        
        switch (animation)
        {
            case AnimatorBoolNameEnum.walk:
                if (navMeshWalkEnable)
                {
                    navMesh.enabled = true;
                    navMesh.speed = walkSpeed;
                    navMesh.angularSpeed = angularSpeed;
                    navMesh.SetDestination(target.position);
                    navMesh.Resume();
                }
                else
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("WalkBlend"))
                    {
                        Vector3 relative = transform.InverseTransformPoint(target.position);
                        angle = (Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg) * Time.deltaTime * rotateSpeed;
                        if (angle >= 0.1f || angle <= -0.1f)
                            transform.Rotate(0, angle, 0);
                        transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * maxWalkSpeed);
                    }
                }
                break;
            case AnimatorBoolNameEnum.turnLeft:
            case AnimatorBoolNameEnum.turnRight:
                angle = LookAtPoint(target.position, rotateSpeed);
                break;
            case AnimatorBoolNameEnum.strapping:
                if (Vector3.Distance(transform.position, hook.transform.position) < 2)
                {
                    // Настройка Аниматора
                    animator.applyRootMotion = true;
                    animator.SetBool("strapping", true);
                }
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("strapping"))
                {
                    animator.SetBool("strapping", false);
                    ststrappingOver = true;
                }
                break;
        }
        return false;
    }

    public Vector3 FindPointNearToCargo(CargoClass cargoToGo)
    {
        Ray rayToCargo = new Ray(Camera.main.transform.position, cargoToGo.transform.position - Camera.main.transform.position);
        RaycastHit hit;
        Physics.Raycast(rayToCargo, out hit);
        Ray rayToGround = new Ray(hit.point - (cargoToGo.transform.position - Camera.main.transform.position) / hit.distance*6, Vector3.down);
        Physics.Raycast(rayToGround, out hit);
        return hit.point;
    }

    public float CanIReachThatPoint(Vector3 point)
    {
        navMesh.enabled = true;
        if (navMesh.CalculatePath(point, new NavMeshPath()))
        {
            return Vector3.Distance(transform.position, point);
        }
        navMesh.enabled = false;
        return -1f;
    }
    //public bool GoToPointOnNavMesh_CR(Vector3 targetToGo)
    public bool GoToPointOnNavMesh_CR_1(object targetToGo)
    {
        // Условие останова и выключение всего что включили 
        navMesh.enabled= true;
        navMesh.SetDestination((Vector3)targetToGo);
        if (Vector3.Distance(transform.position, navMesh.destination) < 0.01f)
        {
            lookIKWeight = 0.6f;
            //targetToLook = points[2];
            animator.SetBool("walk", false);
            navMesh.Stop();
            navMesh.enabled = false;
            // сигнализация о достижении цели
            status = 1;
            return true;
        }
        // Настройка "Взгляда"
        if(cargo != null)
            targetToLook = cargo.transform.position;
        lookIKWeight = 0.3f;
        // Настройка Аниматора
        animator.applyRootMotion = false;
        animator.SetBool("walk", true);
        // Настройка/включение навмеша
        navMesh.enabled = true;
        navMesh.speed = walkSpeed;
        navMesh.angularSpeed = angularSpeed;
        navMesh.SetDestination((Vector3)targetToGo);
        navMesh.Resume();
        
        return false;
    }

    //public bool TurnToPoint_CR(Vector3 tagetToTurn)
    /*public bool TurnToPoint_CR_1(object tagetToTurn) //Хочу переписать
    {
        // Настройка Аниматора
        animator.applyRootMotion = false;
        // Подфункция выполняющая поворот объекта (я ее буду переделывать), она описана ниже 
        float angle = LookAtPoint((Vector3)tagetToTurn, rotateSpeed);
        // Условие останова и выключение всего что включили 
        if (angle <= 0.1f && angle >= -0.1f)
        {
                status = 1;
                return true;
        }
        return false;
    }*/
    
    public bool TurnToPoint_CR_1(object tagetToTurn)
    {
        float maxAngle = 0.5f;
        float angle = LookAtPoint((Vector3)tagetToTurn, rotateSpeed, true);
        // Условие останова и выключение всего что включили 
        if (angle <= 0.1f && angle >= -0.1f)
        {
            resetAll();
            status = 1;
            return true;
        }
        // Настройка Аниматора
        animator.applyRootMotion = false;

        if (angle > maxAngle)
        {
            animator.SetBool("turnLeft", false);
            animator.SetBool("turnRight", true);
        }
        if (angle < -maxAngle)
        {
            animator.SetBool("turnRight", false);
            animator.SetBool("turnLeft", true);
        }
        if (angle <= maxAngle && angle >= -maxAngle)
        {
            animator.SetBool("turnLeft", false);
            animator.SetBool("turnRight", false);
        }

        transform.Rotate(0, angle, 0);        
        return false;
    }

    public bool UpLadderStart_CR_0()
    {
        // Условие останова и выключение всего что включили 
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("EnterLadderBottom"))
        {
            animator.SetBool("ladderUp", false);
            status = 1;
            return true;
        }
        // Настройка "Взгляда"
        lookIKWeight = 0;
        // Настройка Аниматора
        animator.applyRootMotion = true;
        animator.SetBool("ladderUp", true);
        return false;
    }

    public bool UpLadderLoopAndExit_CR_1(object upExitPoint)
    {
        // Условие останова и выключение всего что включили 
        if(((Vector3)upExitPoint).y - transform.position.y < 1.1f)
        {
            animator.SetBool("ladderUp", false);
            animator.SetBool("ladderUpExit", true);
            // ДОПОЛНИТЕЛЬНОЕ Условие останова и выключение всего что включили 
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("TopExit") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.98f)
            {
                animator.SetBool("ladderUpExit", false);
                status = 1;
                return true;
            }
            return false;
        }
        // Настройка "Взгляда"
        lookIKWeight = 0;
        // Настройка Аниматора
        animator.applyRootMotion = true;
        animator.SetBool("ladderUp", true);
        return false;
    }

    //public bool GoToPointWithoutNavMesh_CR(Vector3 targetToGo)
    public bool GoToPointWithoutNavMesh_CR_1(object targetToGo)
    {
        // Условие останова и выключение всего что включили 
        if (Vector3.Distance(transform.position, (Vector3)targetToGo) < 0.01f)
        {
            animator.SetBool("walk", false);
            //riggingWorkStates = RiggingWorkStates.state5;
            status = 1;
            return true;
        }
        // Настройка "Взгляда"
        lookIKWeight = 0.1f;
        targetToLook = (Vector3)targetToGo;
        // Настройка Аниматора
        animator.SetBool("walk", true);
        //Перемещение объекта
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("WalkBlend"))
        {
            Vector3 relative = transform.InverseTransformPoint((Vector3)targetToGo);
            float angle = (Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg) * Time.deltaTime * rotateSpeed;
            if (angle >= 0.1f || angle <= -0.1f)
                transform.Rotate(0, angle, 0);
            transform.position = Vector3.MoveTowards(transform.position, (Vector3)targetToGo, Time.deltaTime * maxWalkSpeed);
        }
        return false;
    }

    //public bool strapingOnTopOfCargo_CR()
    public bool StrapingOnTopOfCargo_CR_0()
    {
        // Условие останова и выключение всего что включили 
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && ststrappingOver)
        {
            animator.SetBool("strapping", false);
            animator.SetBool("unstrapping", false);
            ststrappingOver = false;
            status = 1;
            return true;
        }
        
        if (hook != null)
        {
            // Настройка "Взгляда"
            targetToLook = hook.transform.position;
            lookIKWeight = 0.7f;
            
            //(синхронизация начала страповки надо будет обсудить)
            if (Vector3.Distance(transform.position, hook.transform.position) < 2)
            {
                // Настройка Аниматора
                animator.applyRootMotion = true;
                animator.SetBool("strapping", true);
            }
        }
        //Ожидание включения страповки
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("strapping"))
        {
            animator.SetBool("strapping", false);
            ststrappingOver = true;
        }
        return false;
    }

    public bool StartRisingSlings_CR_1(object upEndPoint)
    {
        // Условие останова и выключение всего что включили 
        Vector3 upEndPointTemp = (Vector3) upEndPoint;
        if (upEndPointTemp.y - hook.transform.position.y  < 0.1f)
        {
            lookIKWeight = 0;
            riseIKEnable = false;
            resetAll();
            animSignalsReset();
            status = 1;
            return true;
        }
        // Настройка "Взгляда"
        targetToLook = hook.transform.position;
        lookIKWeight = 1;
        // Настройка Аниматора
        animator.SetBool("sUP", true);
        riseIKEnable = true;
        return false;
    }

    public bool ShowSTOPsignal_CR_0()
    {
        // Условие останова и выключение всего что включили 
        if ((animator.GetCurrentAnimatorStateInfo(0).IsName("Stop_noEnter") || animator.GetCurrentAnimatorStateInfo(0).IsName("Stop")) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.98f)
        {
            resetAll();
            status = 1;
            return true;
        }
        // Настройка "Взгляда"
        targetToLook = hook.transform.position;
        lookIKWeight = 0.7f;
        // Настройка Аниматора
        animator.SetBool("sSTOP", true);
        return false;
    }

    public bool DownLadderStart_CR_0()
    {
        // Условие останова и выключение всего что включили 
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("EnterLadderTopMod"))
        {
            animator.SetBool("ladderDown", false);
            status = 1;
            return true;
        }
        // Настройка "Взгляда"
        lookIKWeight = 0;
        // Настройка Аниматора
        animator.applyRootMotion = true;
        animator.SetBool("ladderDown", true);
        return false;
    }
    
    public bool DownLadderLoopAndExit_CR_1(object downExitPoint)
    {
        // Условие останова и выключение всего что включили 
        if(transform.position.y - ((Vector3)downExitPoint).y < 0.1f)
        {
            animator.SetBool("ladderDownExit", true);
            animator.SetBool("ladderDown", false);
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("ExitLadderBottom") &&animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.98f)
            {
                status = 1;
                return true;
            }
            return false;
        }
        // Настройка "Взгляда"
        lookIKWeight = 0;
        // Настройка Аниматора
        animator.applyRootMotion = true;
        animator.SetBool("ladderDown", true);        
        return false;
    }

    //public bool Signals_CR(Vector3 target, Vector3 goal)
    public bool Signals_CR_2(object[] args)
    {
        animator.applyRootMotion = false;
        float angleToRotate = LookAtPoint((Vector3)args[0], rotateSpeed);
        //Debug.Log(angleToRotate);
        if (angleToRotate < 1 && angleToRotate > -1)
        {
            lookIKWeight = 0.5f;
            targetToLook = (Vector3)args[0];
            
            float angleTotarget = Mathf.Atan2(((Vector3)args[0]).z - transform.position.z, ((Vector3)args[0]).x - transform.position.x) / Mathf.PI *
                180;
            Vector3 targetGoalDirection =
                new Vector3(((Vector3)args[0]).x - ((Vector3)args[1]).x, ((Vector3)args[0]).y - ((Vector3)args[1]).y, ((Vector3)args[0]).z - ((Vector3)args[1]).z);
            //Debug.Log(targetGoalDirection);
            if (Vector3.Distance((Vector3)args[0], (Vector3)args[1]) < closeRange)
            {
                animSignalsReset();
                status = 1;
                return true;
            }
            animSignalsReset();
            if (targetGoalDirection.y < 0.5f)
            {
                animator.SetBool("sUP", true);
                return false;
            }
            if (Mathf.Abs(targetGoalDirection.x) > Mathf.Abs(targetGoalDirection.z))
            {
                if (targetGoalDirection.x < -(closeRange))
                {
                    if (angleTotarget > 45 && angleTotarget <= 135)
                        animator.SetBool("sRIGHT", true);
                    if (angleTotarget > -45 && angleTotarget <= 45)
                        animator.SetBool("sBACKWARD", true);
                    if (angleTotarget > -135 && angleTotarget <= -45)
                        animator.SetBool("sLEFT", true);
                    if (angleTotarget > 135 || angleTotarget <= -135)
                        animator.SetBool("sFORWARD", true);
                    return false;
                }
                if (targetGoalDirection.x > closeRange)
                {
                    if (angleTotarget > 45 && angleTotarget <= 135)
                        animator.SetBool("sLEFT", true);
                    if (angleTotarget > -45 && angleTotarget <= 45)
                        animator.SetBool("sFORWARD", true);
                    if (angleTotarget > -135 && angleTotarget <= -45)
                        animator.SetBool("sRIGHT", true);
                    if (angleTotarget > 135 || angleTotarget <= -135)
                        animator.SetBool("sBACKWARD", true);
                    return false;
                }
            }
            else
            {
                if (targetGoalDirection.z < -(closeRange))
                {
                    if (angleTotarget > 45 && angleTotarget <= 135)
                        animator.SetBool("sBACKWARD", true);
                    if (angleTotarget > -45 && angleTotarget <= 45)
                        animator.SetBool("sLEFT", true);
                    if (angleTotarget > -135 && angleTotarget <= -45)
                        animator.SetBool("sFORWARD", true);
                    if (angleTotarget > 135 || angleTotarget <= -135)
                        animator.SetBool("sRIGHT", true);
                    return false;
                }
                if (targetGoalDirection.z > closeRange)
                {
                    if (angleTotarget > 45 && angleTotarget <= 135)
                        animator.SetBool("sFORWARD", true);
                    if (angleTotarget > -45 && angleTotarget <= 45)
                        animator.SetBool("sRIGHT", true);
                    if (angleTotarget > -135 && angleTotarget <= -45)
                        animator.SetBool("sBACKWARD", true);
                    if (angleTotarget > 135 || angleTotarget <= -135)
                        animator.SetBool("sLEFT", true);
                    return false;
                }
            }
            if (targetGoalDirection.y > closeRange)
                animator.SetBool("sDOWN", true);
        }
        else
            animSignalsReset();
        return false;
    }

    public void resetAll()
    {
        animator.SetBool("walk", false);
        animator.SetBool("ladderUp", false);
        animator.SetBool("ladderUpExit", false);
        animator.SetBool("turnLeft", false);
        animator.SetBool("turnRight", false);
        animator.SetBool("ladderDown", false);
        animator.SetBool("ladderDownExit", false);
        animator.SetBool("strapping", false);
        animator.SetBool("unstrapping", false);
        animator.SetBool("sSTOP", false);
        animator.SetBool("sUP", false);
        animator.SetBool("sDOWN", false);
        animator.SetBool("sLEFT", false);
        animator.SetBool("sRIGHT", false);
        animator.SetBool("sFORWARD", false);
        animator.SetBool("sBACKWARD", false);
        animator.applyRootMotion = false;
        navMesh.enabled = false;
        signalersWork = false;
        lookIKWeight = 0;
    }

    private void animSignalsReset()
    {
        animator.SetBool("sUP", false);
        animator.SetBool("sDOWN", false);
        animator.SetBool("sLEFT", false);
        animator.SetBool("sRIGHT", false);
        animator.SetBool("sFORWARD", false);
        animator.SetBool("sBACKWARD", false);
    }

    private string calcSignal(string signal, double angle)
    {
        if (signal == "sRIGHT")
        {
            if (angle > 45 && angle <= 135)
                return "sRIGHT";
            if (angle > -45 && angle <= 45)
                return "sBACKWARD";
            if (angle > -135 && angle <= -45)
                return "sLEFT";
            if (angle > 135 || angle <= -135)
                return "sFORWARD";
        }
        if (signal == "sLEFT")
        {
            if (angle > 45 && angle <= 135)
                return "sLEFT";
            if (angle > -45 && angle <= 45)
                return "sFORWARD";
            if (angle > -135 && angle <= -45)
                return "sRIGHT";
            if (angle > 135 || angle <= -135)
                return "sBACKWARD";
        }
        if (signal == "sBACKWARD")
        {
            if (angle > 45 && angle <= 135)
                return "sBACKWARD";
            if (angle > -45 && angle <= 45)
                return "sLEFT";
            if (angle > -135 && angle <= -45)
                return "sFORWARD";
            if (angle > 135 || angle <= -135)
                return "sRIGHT";
        }
        if (signal == "sFORWARD")
        {
            if (angle > 45 && angle <= 135)
                return "sFORWARD";
            if (angle > -45 && angle <= 45)
                return "sRIGHT";
            if (angle > -135 && angle <= -45)
                return "sBACKWARD";
            if (angle > 135 || angle <= -135)
                return "sLEFT";
        }
        return "";
    }
    
    float LookAtPoint(Vector3 destination, float speed = 3, bool getAngle = false)
    {
        animator.applyRootMotion = false;
        Vector3 relative = transform.InverseTransformPoint(destination);
        float angle = (Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg) * Time.deltaTime * rotateSpeed;
        if (!getAngle)
        {
            if (angle >= 0.1f || angle <= -0.1f)
            {
                if (angle > 3)
                {
                    animator.SetBool("turnLeft", false);
                    animator.SetBool("turnRight", true);
                }
                if (angle < -2)
                {
                    animator.SetBool("turnRight", false);
                    animator.SetBool("turnLeft", true);
                }
                transform.Rotate(0, angle, 0);
            }
            else
            {
                animator.SetBool("turnLeft", false);
                animator.SetBool("turnRight", false);
            }
        }
        return angle;
    }
    
    void OnAnimatorIK() {
        if (targetToLook != null)
        {
            animator.SetLookAtWeight(curentLookIKWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            animator.SetLookAtPosition(targetToLook);
        }
        if (riseIKEnable)
        {
            animator.SetIKPosition(AvatarIKGoal.LeftHand, hook.transform.position - new Vector3(0,0.2f,0));
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKWeight);
            Quaternion tempAngle = new Quaternion();
            Vector3 relevantPosition = (hook.transform.position - myLeftHand.position);
            relevantPosition.y = 90;
            tempAngle.SetLookRotation(relevantPosition);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, tempAngle);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKWeight);
        }
        else
        {
            if (handIKWeight > 0.05f)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, hook.transform.position - new Vector3(0,0.2f,0));
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKWeight);
                Quaternion tempAngle = new Quaternion();
                Vector3 relevantPosition = (hook.transform.position - myLeftHand.position);
                relevantPosition.y = 90;
                tempAngle.SetLookRotation(relevantPosition);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, tempAngle);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKWeight);
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (gizmosPoints != null)
            for (int i = 0; i < gizmosPoints.Length; i++)
                if (gizmosPoints[i] != null)
                    Gizmos.DrawWireSphere(gizmosPoints[i], 0.3f);
    }
}
