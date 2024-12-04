using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class Stage0 : MonoBehaviour
{
	[SerializeField] private TMP_Text missionText;
	[SerializeField] private PlayerController player;
	[SerializeField] private GameObject helper1;
	[SerializeField] private GameObject helper2;
	[SerializeField] private List<BaseMob> MobList;
	[SerializeField] private List<SwitchController> SwitchList;
	[SerializeField] private GameObject Door;
	[SerializeField] private CinemachineCamera DoorCam;

	public List<string> MissionList;
	public int currentMission;

	private void Awake()
	{
		currentMission = 3;
		MissionList.Add("폭탄으로 나무상자를 부수고 탈출하세요");
		MissionList.Add("모든 부하들을 구출하세요");
		MissionList.Add("폭탄으로 쥐들을 물리치세요");
		MissionList.Add("부하를 모아서 모든 버튼을 누르세요");
		MissionList.Add("창고 깊은 곳으로 들어가세요");
		MissionList.Add("대장을 물리치세요");
		UpdateMission(currentMission);
	}

	private void Update()
	{
        if (currentMission == 1)
        {
            if (player.Critters.Count == 4)
			{
				foreach (BaseMob mob in MobList)
				{
					mob.gameObject.SetActive(true);
				}
				UpdateMission(2);
			}
        }

		if (currentMission == 2)
		{
			int count = 0;
			foreach (BaseMob mob in MobList)
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
    }

	public void UpdateMission(int index)
	{
		currentMission = index;
		missionText.text = MissionList[currentMission];
	}

	public void PassFirstMission()
	{
		if (currentMission == 0)
		{
			UpdateMission(1);
			helper1.SetActive(false);
		}
	}
}
