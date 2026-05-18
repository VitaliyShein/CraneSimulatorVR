using UnityEngine;
using System.Collections;

public class FollowTo : MonoBehaviour {

    public GameObject goal;
    Transform goalTransform;
    Transform objTransform;

    void Start () {
        goalTransform = goal.GetComponent<Transform>();
        objTransform = GetComponent<Transform>();
        objTransform.position = goalTransform.position;
    }
	
	void Update () {
        objTransform.position = goalTransform.position;
    }
}
