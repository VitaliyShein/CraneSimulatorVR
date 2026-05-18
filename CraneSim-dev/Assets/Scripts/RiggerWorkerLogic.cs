using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiggerWorkerLogic : MonoBehaviour {


    public GameObject Rigger;
    public Transform cargo;
    public bool startWork = false;
    
    RiggerWorkerController rigger_script;

    void Start()
    {
        rigger_script = Rigger.GetComponent<RiggerWorkerController>();
    }

    // Update is called once per frame
    void Update () {
        if (startWork) {
            if (rigger_script.i_done_work) {
                if (cargo != null)
                {
                    rigger_script.resetAll();
                    rigger_script.i_done_work = false;
                    rigger_script.workType = RiggerWorkerController.WorkType.Rigging;
                    rigger_script.riggingWorkStates = RiggerWorkerController.RiggingWorkStates.state0;
                    rigger_script.cargo = cargo;
                    startWork = false;
                    cargo = null;
                    //
                    rigger_script.waitingForStrapping = true;

                }
                else
                    startWork = false;
            }
        }
	}
}
