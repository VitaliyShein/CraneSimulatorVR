using UnityEngine;
using System.Collections;

public class CraneFunctions : MonoBehaviour {
	public static float SpotDistance(Vector3 v1, Vector3 v2) {
		return Mathf.Sqrt (Mathf.Pow (v1.x-v2.x, 2) + Mathf.Pow (v1.y-v2.y, 2) + Mathf.Pow (v1.z-v2.z, 2));
	}

    public static int factorial(int num)
    {
        int fact = 1;
        for (; num > 0; fact *= num--) ;
        return fact;
    }
}
