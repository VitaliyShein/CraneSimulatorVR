using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class PointConfigurator : MonoBehaviour
{
	public List<PointClass.PointStruct> points = new List<PointClass.PointStruct>();
	public RIGGER target;

	public string postfix = "CR";
	public bool drowGizmos = true;
	public bool testTarget = false;

	private int processPointStatus = 0;
	private int processPointCommandStatus = 0;
	//private int[] commandArguments;
	
	
	void Start ()
	{
		
	}
	
	void Update () {
		updatePoints();

		if (target != null && testTarget /*&& EditorApplication.isPlaying*/)
		{

			if (processPointCommandStatus == points[processPointStatus].commands.Length)
			{
				processPointStatus++;
				processPointCommandStatus = 0;
			}
			if (processPointStatus == points.Count)
			{
				processPointStatus = 0;
				target.status = 0;
				testTarget = false;
				return;
			}
			
			object[] temp = new object[points[processPointStatus].commands[processPointCommandStatus].arguments.Length];
			for (int i = 0; i < temp.Length; i++)
				temp[i] = points[processPointStatus].commands[processPointCommandStatus].arguments[i].position;

			if (points[processPointStatus].commands[processPointCommandStatus].arguments.Length == 1)
				target.SendMessage(points[processPointStatus].commands[processPointCommandStatus].nameOfCommand, temp[0]);
			else
				target.SendMessage(points[processPointStatus].commands[processPointCommandStatus].nameOfCommand, temp);
			if (target.status == 1)
			{
				processPointCommandStatus++;
				target.status = 0;
			}

			/*for (int i = 0; i < points[processPointStatus].commands.Length; i++)
			{
				object[] temp = new object[points[processPointStatus].commands[i].arguments.Length];
				for (int j = 0; j < temp.Length; j++)
					temp[j] = points[processPointStatus].commands[i].arguments[j].position;
				if (points[processPointStatus].commands[i].arguments.Length == 1)
					target.SendMessage(points[processPointStatus].commands[i].nameOfCommand, temp[0]);
				else
					target.SendMessage(points[processPointStatus].commands[i].nameOfCommand, temp);
				if (target.status == 1)
				{
					processPointCommandStatus++;
					target.status = 0;
				}
			}*/
		}
	}

	public string[] getSetCommands(RIGGER targetArg, string postfixOfCommand, bool fullName = true)
	{
		if (targetArg == null)
		{
			Debug.LogWarning("TARGET is NULL. Set target!");
			return null;
		}
		string[] setCommands;
		int setCommandsLenght = 0;

		Type t = target.GetType();
		MethodInfo[] method1 = t.GetMethods();
		foreach (var m in method1)
			if (m.Name.Contains("_" + postfixOfCommand + "_"))
				setCommandsLenght++;
		setCommands = new string[setCommandsLenght];
		//commandArguments = new int[setCommandsLenght];
		setCommandsLenght = 0;
		
		foreach (var m in method1)
		{
			if (m.Name.Contains("_" + postfixOfCommand + "_"))
			{
				setCommands[setCommandsLenght] = m.ToString();
				//commandArguments[setCommandsLenght] = (int) Char.GetNumericValue(m.Name[m.Name.Length - 1]);
				setCommandsLenght++;
			}
		}

		if (fullName)
			return setCommands;
		else
		{
			string[] shortNameCommand = new string[setCommands.Length];
			for (int i = 0; i < setCommands.Length; i++)
			{
				string[] split1 = setCommands[i].Split(' ');
				string[] split2 = split1[1].Split('(');
				shortNameCommand[i] = split2[0];
			}
			return shortNameCommand;
		}

	}

	public void updatePoints()
	{
		points = new List<PointClass.PointStruct>();
		Transform childPoints = null;
		for (int i = 0; i < transform.childCount; i++)
			if (transform.GetChild(i).GetComponent("POINT") != null)
				childPoints = transform.GetChild(i);
		for (int i = 0; i < childPoints.childCount; i++)
		{
			if (childPoints.GetChild(i).GetComponent("PointClass") != null)
			{
				PointClass targetPoint = (PointClass) childPoints.GetChild(i).GetComponent("PointClass");
				points.Add(targetPoint.point);	
			}
		}
	}

	public void updatePointsData()
	{
		string[] setCommand = getSetCommands(target, postfix, false);
		if (setCommand.Length != 0)
		{
			for (int i = 0; i < points.Count; i++)
			{
				PointClass.PointStruct currentTemp = points[i];
				for (int j = 0; j < points[i].commands.Length; j++)
				{
					if (points[i].commands[j].indexOfCommand >= setCommand.Length)
					{
						Debug.LogWarning("point" + i + " - Нет команды с таким индексом");
						currentTemp.commands[j].indexOfCommand = 0;
					}
					currentTemp.commands[j].nameOfCommand= setCommand[currentTemp.commands[j].indexOfCommand];
					string[] split1 = currentTemp.commands[j].nameOfCommand.Split('_');
					currentTemp.commands[j].arguments = new Transform[Convert.ToInt32(split1[2])];
				
					//УДАЛИТЬ ЕСЛИ НЕ ПОНРАВИТСЯ
					if (currentTemp.commands[j].arguments.Length == 1)
						currentTemp.commands[j].arguments[0] = currentTemp.self.transform;
					points[i] = currentTemp;
				}
			}
		}
		for (int i = 0; i < points.Count; i++)
		{
			points[i].self.updatePointData(points[i]);
		}
	}

	private void OnDrawGizmos()
	{
		if (drowGizmos && points.Count > 0)
			for (int i = 0; i < points.Count; i++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(points[i].self.transform.position,0.3f);
				Gizmos.color = Color.white;
				if (i != 0)
					Gizmos.DrawLine(points[i-1].self.transform.position,points[i].self.transform.position);
			}
	}
	
}
