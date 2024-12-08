using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class Stage0 : Stage
{
	// 스테이지 구성 관련
	[SerializeField] private GameObject helper1;
	[SerializeField] private GameObject helper2;
	[SerializeField] private List<BaseMob> Wave1MobList;
	[SerializeField] private List<BaseMob> Wave2MobList;
	[SerializeField] private List<SwitchController> Stage1SwitchList;
	[SerializeField] private List<SwitchController> Stage2SwitchList;
	[SerializeField] private GameObject Door;
	[SerializeField] private CinemachineCamera DoorCam;
	[SerializeField] private AudioClip bossBGM;

	[SerializeField] private CinemachineCamera BossCam;
	[SerializeField] private GameObject bossAltar;
	[SerializeField] private GameObject bossRat;
	[SerializeField] private GameObject foodGrid;

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
			}

			if (count == 2)
			{
				UpdateMission(3);
				helper2.SetActive(true);
			}
		}

		if (GlobalSceneManager.Instance.CurrentMission == 3)
		{
			foreach (SwitchController s in Stage1SwitchList)
			{
				if (!s.isOn)
				{
					return;
				}
			}

			helper2.SetActive(false);
			foreach (SwitchController s in Stage1SwitchList)
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
			}

			if (count == 0)
			{
				UpdateMission(6);
				StartCoroutine(DownAltar());
			}
		}

		if (GlobalSceneManager.Instance.CurrentMission == 6)
		{
			Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 2, Quaternion.identity, LayerMask.GetMask("Obstacle"));

			int count = 0;
			foreach (Collider c in hits)
			{
				if (c.transform.CompareTag("Carryable"))
				{
					count++;
				}
			}

			// Debug.Log(count);

			if (count == 3)
			{
				foreach (Collider c in hits)
				{
					c.transform.SetParent(bossAltar.transform);
				}

				UpdateMission(7);	
			}
		}

		if (GlobalSceneManager.Instance.CurrentMission == 7)
		{
			foreach (SwitchController s in Stage2SwitchList)
			{
				if (!s.isOn)
				{
					return;
				}
			}

			foreach (SwitchController s in Stage2SwitchList)
			{
				s.Complete();
			}

			StartCoroutine(UpAltar());
			bossRat.SetActive(true);
			Camera.main.GetComponent<CameraController>().SwitchCamera(BossCam, 5f);
			UpdateMission(8);
			StartCoroutine(TableBreak());
		}
	}

	private IEnumerator TableBreak()
	{
		yield return new WaitForSeconds(2f);
		Animator animator = bossAltar.GetComponent<Animator>();
		animator.enabled = false;

		Rigidbody _rigidBody = bossAltar.GetComponent<Rigidbody>();
		_rigidBody.constraints = RigidbodyConstraints.None;
		_rigidBody.isKinematic = false;

		_rigidBody.AddForce(Vector3.up * 100, ForceMode.Impulse);
		_rigidBody.AddTorque(Vector3.one * 50, ForceMode.Impulse);

		Destroy(bossAltar, 2f);
	}

	private IEnumerator DownAltar()
	{
		bossAltar.GetComponent<Animator>().SetTrigger("Down");
		yield return null;
	}

	private IEnumerator UpAltar()
	{
		bossAltar.GetComponent<Animator>().SetTrigger("Up");
		yield return new WaitForSeconds(4f);
		GlobalSceneManager.Instance.GetCurrentMap().MakeGridMap();
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
