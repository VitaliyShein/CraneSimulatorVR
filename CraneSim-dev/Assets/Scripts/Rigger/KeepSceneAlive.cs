using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepSceneAlive : MonoBehaviour {
	public bool KeepSceneViewActive;
	void Start()
	{
		if (this.KeepSceneViewActive && Application.isEditor)
		{
//			UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
		}
	}
}
