using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
	public Dictionary<int, Data.BaseStage> StageDict;
	public int currentStage = 0;

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Init()
	{
		StageDict = DataManager.Instance.LoadJsonToDict<Data.BaseStage>("Data/stage");
		// Debug.Log(StageDict.Count);
	}

	public void SetStage(int code)
	{
		currentStage = code;
	}
}
