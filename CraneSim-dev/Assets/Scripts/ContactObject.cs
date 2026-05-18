using UnityEngine;
using System.Collections;

public class ContactObject : MonoBehaviour {
    //public bool inTrigger = false;
    public RopeConnect ropeConnect;
    public GameObject HookBlink;
    public GameObject startOfSlings;
    //public Transform cargo;
    public Transform[] firstSlingPoints;
    public Transform point5;
    public Transform point6;
    public Transform point7;
    public Transform point8;

    //FixedJoint j1, j2, j3, j4;
    Vector3 colliderSize;
    public GameObject ropes;

    Transform [] firstFakeSlingPoint;

    void Start () {
//        GameObject parent = this.transform.parent.gameObject;
        BoxCollider[] bc = GetComponents<BoxCollider>();
        BoxCollider bcTrigger, bcCargo;          //bcChild->Trigger   bcParent->Cargo
        if (bc[0].isTrigger)
        {
            bcTrigger = bc[0];
            bcCargo = bc[1];
        }
        else
        {
            bcTrigger = bc[1];
            bcCargo = bc[0];
        }

        colliderSize = bcCargo.size;

        firstFakeSlingPoint = new Transform[4]; 
        GetFirstFakeSlingPointsPosition();                     //получили детей объекта slingConnetion
        
        //bcTrigger.size = new Vector3(bcCargo.size.x, 1.5f, bcCargo.size.z);
        //Vector3 pos = transform.position;         //parent.transform.position;
        //pos.y += bcCargo.size.y / 2.0f + 0.75f;
        //this.transform.position = pos;
    }

    void OnTriggerEnter(Collider hook) {
        ropes.GetComponent<RopeConnect>().inTrigger = true;
//        startOfSlings.transform.position = gameObject.transform.position + new Vector3(0, 2.2f, 0);   //помещение slingconnection над контейнером

        if (hook.gameObject.name.CompareTo ("point.0.Hook") == 0) { //если имя коллайдера = имени крюка //и строп еще нет
			HookBlink.GetComponent<Renderer> ().enabled = true;
			HookBlink.GetComponent<Animator> ().enabled = true;

            FirstSlingPointsFollowToFakes();

            if (!ropeConnect.isConnect)
            {
                Vector3 positionOfCenter = transform.position;

                //координаты смещения относительно центра контейнера в зависимости от типа груза
//                string objectName = transform.parent.name;
                float x, y, z;
                switch (transform.tag)
                {
                    case "Tubes":
                        x = colliderSize.x / 2;
                        y = colliderSize.y / 2 - 0.1f;
                        z = colliderSize.z / 2 - 0.55f;
                        break;
                    case "BigContainer":
                        x = colliderSize.x / 2f;
                        y = colliderSize.y / 2f;
                        z = colliderSize.z / 2f;
                        break;
                    default:
                        x = colliderSize.x / 2;
                        y = colliderSize.y / 2;
                        z = colliderSize.z / 2;
                        break;
                }
                    
                float a = transform.rotation.eulerAngles.y;

                float cos = Mathf.Cos(Mathf.Deg2Rad * a);
                float sin = Mathf.Sin(Mathf.Deg2Rad * a);

                //перемещаем точки в углы контейнера
                point5.position = new Vector3(positionOfCenter.x + (x * cos - z * sin), positionOfCenter.y + y, positionOfCenter.z - (x * sin + z * cos));
                point6.position = new Vector3(positionOfCenter.x + (x * cos + z * sin), positionOfCenter.y + y, positionOfCenter.z + (-x * sin + z * cos));
                point7.position = new Vector3(positionOfCenter.x - (x * cos + z * sin), positionOfCenter.y + y, positionOfCenter.z - (-x * sin + z * cos));
                point8.position = new Vector3(positionOfCenter.x - (x * cos - z * sin), positionOfCenter.y + y, positionOfCenter.z + (x * sin + z * cos));

                
//                point5.position = new Vector3(positionOfCenter.x + 0.8f, positionOfCenter.y + 0.45f, positionOfCenter.z + -0.3f);
//                point6.position = new Vector3(positionOfCenter.x - 0.2f, positionOfCenter.y + 0.45f, positionOfCenter.z + 0.2f);
//                point7.position = new Vector3(positionOfCenter.x + 0.4f, positionOfCenter.y + 0.45f, positionOfCenter.z - 0.4f);
//                point8.position = new Vector3(positionOfCenter.x + 0.1f, positionOfCenter.y + 0.45f, positionOfCenter.z + -0.6f);

                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                FixedJoint j1 = gameObject.AddComponent<FixedJoint>();
                j1.connectedBody = point5.GetComponent<Rigidbody>();
                FixedJoint j2 = gameObject.AddComponent<FixedJoint>();
                j2.connectedBody = point6.GetComponent<Rigidbody>();
                FixedJoint j3 = gameObject.AddComponent<FixedJoint>();
                j3.connectedBody = point7.GetComponent<Rigidbody>();
                FixedJoint j4 = gameObject.AddComponent<FixedJoint>();
                j4.connectedBody = point8.GetComponent<Rigidbody>();
                gameObject.GetComponent<Rigidbody>().isKinematic = false;

                ropeConnect.cargo = gameObject;
            }
        }
    }

	void OnTriggerExit(Collider hook) {
        ropes.GetComponent<RopeConnect>().inTrigger = false;

        if (hook.gameObject.name.CompareTo ("point.0.Hook") == 0) {
			HookBlink.GetComponent<Renderer> ().enabled = false;
			HookBlink.GetComponent<Animator> ().enabled = false;
		}

        if (!ropeConnect.isConnect)
        {
            FixedJoint[] fj = GetComponents<FixedJoint>();
//            Debug.Log("in CO " + fj + " " + fj.Length);
            if (fj.Length != 0)
            {
                for (int i = 0; i < fj.Length; i++)
                {
                    Destroy(fj[i]);
                }
            }
        }
    }


    void GetFirstFakeSlingPointsPosition()
    {
        int children = startOfSlings.transform.childCount;
        for (int i = 0; i < children; i++)
        {
            firstFakeSlingPoint[i] = startOfSlings.transform.GetChild(i);
        }
    }

    void FirstSlingPointsFollowToFakes()
    {
        for (int i = 0; i < firstSlingPoints.Length; i++)
        {
            FollowTo ft = firstSlingPoints[i].GetComponent<FollowTo>();
            ft.enabled = true;
        }
    }

    

}
