using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
//using UnityEditorInternal;

[ExecuteInEditMode]
public class PointClass : POINT {

	[Serializable]
	public struct CommandStruct
	{
		public int indexOfCommand;
		[ReadOnly] public string nameOfCommand;
		public Transform[] arguments;
	}
	
	[Serializable]
	public struct PointStruct
	{
		[HideInInspector] public PointClass self;
		public CommandStruct[] commands;
	}

	public PointStruct point;
	
	public void Start ()
	{
		point.self = this;
	}
	
	void Update ()
	{
		if (point.self == null)
			point.self = this;
	}

	public void updatePointData(PointStruct data)
	{
		point = data;
		point.self = this;
	}
}
