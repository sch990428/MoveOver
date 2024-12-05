using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Stage0 : MonoBehaviour
{
	// 스테이지 관련
	[SerializeField] private List<GridMap> StageList;
	public int CurrentStage;
	[SerializeField] private PlayerController player;

	// 미션 관련
	[SerializeField] private TMP_Text missionText;
	[SerializeField] private Image missionProgressUI;
	public List<string> MissionList;
	public int currentMission;

	// 스테이지 구성 관련
	[SerializeField] private GameObject helper1;
	[SerializeField] private GameObject helper2;
	[SerializeField] private List<BaseMob> Wave1MobList;
	[SerializeField] private List<BaseMob> Wave2MobList;
	[SerializeField] private List<SwitchController> SwitchList;
	[SerializeField] private GameObject Door;
	[SerializeField] private CinemachineCamera DoorCam;

	private void Awake()
	{
		MissionList.Add("폭탄으로 나무상자를 부수고 탈출하세요");
		MissionList.Add("모든 부하들을 구출하세요");
		MissionList.Add("폭탄으로 쥐들을 물리치세요");
		MissionList.Add("부하를 모아서 모든 버튼을 누르세요");
		MissionList.Add("창고 깊은 곳으로 들어가세요");
		MissionList.Add("폭탄으로 쥐들을 물리치세요");
		MissionList.Add("대장을 물리치세요");
		MissionList.Add("인간을 구출하세요");
		UpdateMission(currentMission);

		player.Init(player.transform.position, StageList[CurrentStage]);
	}

	private void Update()
	{
        if (currentMission == 1)
        {
            if (player.Critters.Count == 4)
			{
				foreach (BaseMob mob in Wave1MobList)
				{
					mob.gameObject.SetActive(true);
				}
				UpdateMission(2);
			}
        }

		if (currentMission == 2)
		{
			int count = 0;
			foreach (BaseMob mob in Wave1MobList)
			{
				if (mob == null)
				{
					count++;
				}

				if (count == 2)
				{
					UpdateMission(3);
					helper2.SetActive(true);
				}
			}
		}

		if (currentMission == 3)
		{
			foreach (SwitchController s in SwitchList)
			{
				if (!s.isOn)
				{
					return;
				}
			}

			helper2.SetActive(false);
			foreach (SwitchController s in SwitchList)
			{
				s.Complete();
			}

			Camera.main.GetComponent<CameraController>().SwitchCamera(DoorCam, 3f);
			UpdateMission(4);
			Door.GetComponent<Animator>().SetTrigger("Open");
		}

		if (currentMission == 5)
		{
			int count = 0;
			foreach (BaseMob mob in Wave2MobList)
			{
				if (mob == null)
				{
					count++;
				}

				if (count == 4)
				{
					UpdateMission(6);
				}
			}
		}
	}

	public void UpdateMission(int index)
	{
		currentMission = index;
		missionText.text = MissionList[currentMission];
		missionProgressUI.fillAmount = (float)currentMission / (float)(MissionList.Count - 1);
	}

	public void PassFirstMission()
	{
		if (currentMission == 0)
		{
			UpdateMission(1);
			helper1.SetActive(false);
		}
	}

	public void ToNextStage()
	{
		StartCoroutine(LoadNewStage(1f));
	}

	private IEnumerator LoadNewStage(float t)
	{
		player.isMovable = false;
		GlobalSceneManager.Instance.FadeOut();
		yield return new WaitForSeconds(t);
		StageList[CurrentStage].gameObject.SetActive(false);
		CurrentStage++;
		StageList[CurrentStage].gameObject.SetActive(true);
		player.Init(new Vector3(-9f, 0f, -9f), StageList[CurrentStage]);
		GlobalSceneManager.Instance.FadeIn();
		UpdateMission(5);
		foreach (BaseMob mob in Wave2MobList)
		{
			mob.gameObject.SetActive(true);
		}
	}
}
