using UnityEngine;
using System.Collections;

public class Constraints : MonoBehaviour
{
    int gears_forward;
    int gears_backward;

    void OnTriggerEnter(Collider col)
    {
//        Debug.Log("!!!");
        //ограничение для крана по перемещению вперед/назад
        if (col.gameObject.name == "WorkingCrane")
        {
            Engine engine = col.GetComponent<Engine>();

            engine.gearNow = 0;
            //ограничение вперед
            if (gameObject.name == "crane_forwand")
            {
                gears_forward = engine.gearsForward;
                engine.gearsForward = 0;
                Debug.Log("Вы больше не можете ехать вперед");
            }
            //ограничение назад
            else if (gameObject.name == "crane_backward")
            {
                gears_backward = engine.gearsBackward;
                engine.gearsBackward = 0;
                Debug.Log("Вы больше не можете ехать назад");
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        //ограничение для крана по перемещению вперед/назад
        if (col.name == "WorkingCrane")
        {
            Engine engine = col.GetComponent<Engine>();
            //ограничение вперед
            if (gameObject.name == "crane_forwand")
            {
                engine.gearsForward = gears_forward;
            }
            //ограничение назад
            else if (gameObject.name == "crane_backward")
            {
                engine.gearsBackward = gears_backward;
            }
        }
    }
}
