using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
	// 스테이지 관련
	[SerializeField] protected List<GridMap> StageList;
	[SerializeField] protected List<Vector3> StartPos;
	[SerializeField] public List<string> MissionList; // 미션 
	[SerializeField] protected List<int> FirstMissions; // 각 스테이지의 첫번째 미션 집합
	[SerializeField] protected List<int> RequireCritters; // 각 스테이지의 필수 부하
	[SerializeField] public PlayerController player;

	// 게임 UI
	[SerializeField] public GameUIController uiController;

	// 미션 관련
	[SerializeField] protected TMP_Text missionText;
	[SerializeField] protected Image missionProgressUI;
	

	protected virtual void Start()
	{
		missionText = uiController.missionText;
		missionProgressUI = uiController.missionProgressUI;

		player.uiController = uiController;
		player.Init(player.transform.position, StageList[GlobalSceneManager.Instance.CurrentStage], RequireCritters[GlobalSceneManager.Instance.CurrentStage]);
	}

	public void LoadFromCheckPoint()
	{
		for (int i = 0; i < StageList.Count; i++) {
			StageList[i].gameObject.SetActive(GlobalSceneManager.Instance.CurrentStage == i);
		}

		player.transform.position = StartPos[GlobalSceneManager.Instance.CurrentStage];
		GlobalSceneManager.Instance.CurrentMission = FirstMissions[GlobalSceneManager.Instance.CurrentStage];
	}

	public GridMap GetGridMap(int i)
	{
		return StageList[i];
	}
}


