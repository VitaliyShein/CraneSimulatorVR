using UnityEngine;

namespace RoboRyanTron.Unite2017.Variables
{
	[CreateAssetMenu]
	public class GearVariable : ScriptableObject
	{
#if UNITY_EDITOR
		[Multiline] public string DeveloperDescription = "";
#endif
		public int MinimumGear;
		public int MaximumGear;
		public int CurrentGear;

		public void SetValue(int value)
		{
			if (CheckBorders(value)) CurrentGear = value;
		}

		public void SetValue(IntegerVariable value)
		{
			if (CheckBorders(value.Value)) CurrentGear = value.Value;
		}

		//Проверяем, не выходит ли передаваемое значение за допустимые границы
		public bool CheckBorders(int value)
		{
			if (value >= MinimumGear && value <= MaximumGear)
				return true;
			else
			{
				Debug.LogError("Значение передачи value = " + value + " выходит за границы");
				return false;
			}
		}
	}
}
