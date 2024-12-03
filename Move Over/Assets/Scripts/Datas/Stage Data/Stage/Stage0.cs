using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stage0 : MonoBehaviour
{
	[SerializeField] private TMP_Text missionText;
	[SerializeField] private PlayerController player;
	[SerializeField] private GameObject helper;
	public List<string> MissionList;
	public List<BaseMob> MobList;
	public int currentMission;

	private void Awake()
	{
		currentMission = 0;
		MissionList.Add("폭탄으로 나무상자를 부수고 탈출하세요");
		MissionList.Add("모든 부하들을 구출하세요");
		MissionList.Add("폭탄으로 쥐들을 물리치세요");
		MissionList.Add("부하 2마리를 모아서 문을 여세요");
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
				}
			}
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
			helper.SetActive(false);
		}
	}
}
