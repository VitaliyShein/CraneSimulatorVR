using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

public class ControllerLogic : MonoBehaviour
{

	public RiggerClass[] riggers;
	public CargoClass[] cargos;
	
	private bool waitForEndOfProcess = false;
	private CargoClass currentCargo = null; 
	
	void Start () {
	}
	
	void Update () {
		ProcessCargos();
	}

	void ProcessCargos()
	{
		if (currentCargo != null)
			if (currentCargo.deliveryStatus)//груз доставлен
				waitForEndOfProcess = false; //сброс переменной ожидания
		if (!waitForEndOfProcess)
		{
			for (int i = 0; i < cargos.Length; i++)
			{
				if (!cargos[i].deliveryStatus)
				{
					currentCargo = cargos[i];
					waitForEndOfProcess = true;
					break;
				}
				if (i == cargos.Length - 1 && cargos[i].deliveryStatus)
					return;
			}
			RiggerClass currentRigger = FindClossestRigger(currentCargo);
			if (currentRigger != null)
			{
				switch (currentRigger.riggerType)
				{
					case RiggerClass.RiggerType.RiggingAndSignaling:
						currentRigger.cargo = currentCargo;
						currentRigger.shouldIWork = true;
						return;
					case RiggerClass.RiggerType.RiggingOnly:
						currentRigger.cargo = currentCargo;
						currentRigger.shouldIWork = true;
						RiggerClass currentRiggerSignaler = FindeClossestRiggerSignaler(currentCargo);
						currentRiggerSignaler.cargo = currentCargo;
						currentRiggerSignaler.signalersWork = true;
						currentRiggerSignaler.shouldIWork = true;
						return;
				}
			}
		}
	}

	RiggerClass FindeClossestRiggerSignaler(CargoClass thisCargo)
	{
		thisCargo.UpdatePoints();
		RiggerClass clossestRiggerSignaler = null;

		for (int i = 0; i < riggers.Length; i++)
		{
			if (clossestRiggerSignaler == null)
			{
				if (riggers[i].CanIReachThatPoint(thisCargo.points[0].transform.position) > 0 && riggers[i].shouldIWork == false &&
				    (riggers[i].riggerType == RiggerClass.RiggerType.RiggingAndSignaling ||
				     riggers[i].riggerType == RiggerClass.RiggerType.SignalingOnly))
					clossestRiggerSignaler = riggers[i];
			}
			else
			{
				if (riggers[i].CanIReachThatPoint(thisCargo.points[0].transform.position) != -1f &&
				    riggers[i].CanIReachThatPoint(thisCargo.points[0].transform.position) <
				    clossestRiggerSignaler.CanIReachThatPoint(thisCargo.points[0].transform.position) && riggers[i].shouldIWork == false &&
				    (riggers[i].riggerType == RiggerClass.RiggerType.RiggingAndSignaling ||
				     riggers[i].riggerType == RiggerClass.RiggerType.RiggingOnly))
					clossestRiggerSignaler = riggers[i];
			}
		}
		return clossestRiggerSignaler;
	}

	RiggerClass FindClossestRigger(CargoClass thisCargo)
	{
		thisCargo.UpdatePoints();
		RiggerClass clossestRigger = null;
		for (int i = 0; i < riggers.Length; i++)
		{
			if (clossestRigger == null)
			{
				if (riggers[i].CanIReachThatPoint(thisCargo.points[0].transform.position) > 0 && riggers[i].shouldIWork == false &&
				    (riggers[i].riggerType == RiggerClass.RiggerType.RiggingAndSignaling ||
				     riggers[i].riggerType == RiggerClass.RiggerType.RiggingOnly))
					clossestRigger = riggers[i];
			}
			else 
			{
				if (riggers[i].CanIReachThatPoint(thisCargo.points[0].transform.position) != -1f &&
			         riggers[i].CanIReachThatPoint(thisCargo.points[0].transform.position) <
			         clossestRigger.CanIReachThatPoint(thisCargo.points[0].transform.position) && riggers[i].shouldIWork == false &&
			         (riggers[i].riggerType == RiggerClass.RiggerType.RiggingAndSignaling ||
			          riggers[i].riggerType == RiggerClass.RiggerType.RiggingOnly))
				clossestRigger = riggers[i];
			}
		}
		return clossestRigger;
	}

}
