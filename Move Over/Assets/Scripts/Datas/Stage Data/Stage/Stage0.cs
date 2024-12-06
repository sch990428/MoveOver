using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Stage0 : Stage
{
	// 스테이지 구성 관련
	[SerializeField] private GameObject helper1;
	[SerializeField] private GameObject helper2;
	[SerializeField] private List<BaseMob> Wave1MobList;
	[SerializeField] private List<BaseMob> Wave2MobList;
	[SerializeField] private List<SwitchController> SwitchList;
	[SerializeField] private GameObject Door;
	[SerializeField] private CinemachineCamera DoorCam;
	[SerializeField] private AudioClip bossBGM;

	protected override void Start()
	{
		base.Start();
		UpdateMission(GlobalSceneManager.Instance.CurrentMission);
	}

	private void Update()
	{
        if (GlobalSceneManager.Instance.CurrentMission == 1)
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

		if (GlobalSceneManager.Instance.CurrentMission == 2)
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

		if (GlobalSceneManager.Instance.CurrentMission == 3)
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

		if (GlobalSceneManager.Instance.CurrentMission == 5)
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
					GetComponent<AudioSource>().resource = bossBGM;
					GetComponent<AudioSource>().Play();
				}
			}
		}
	}

	public void UpdateMission(int index)
	{
		GlobalSceneManager.Instance.CurrentMission = index;
		missionText.text = MissionList[GlobalSceneManager.Instance.CurrentMission];
		missionProgressUI.fillAmount = (float)GlobalSceneManager.Instance.CurrentMission / (float)(MissionList.Count - 1);
	}

	public void PassFirstMission()
	{
		if (GlobalSceneManager.Instance.CurrentMission == 0)
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
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(false);
		GlobalSceneManager.Instance.CurrentStage++;
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(true);
		player.Init(new Vector3(-9f, 0f, -9f), StageList[GlobalSceneManager.Instance.CurrentStage]);
		GlobalSceneManager.Instance.FadeIn();
		UpdateMission(5);
		foreach (BaseMob mob in Wave2MobList)
		{
			mob.gameObject.SetActive(true);
		}
	}
}
