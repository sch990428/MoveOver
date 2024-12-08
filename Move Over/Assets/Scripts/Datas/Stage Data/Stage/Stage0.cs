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
	[SerializeField] private GameObject helper3;
	[SerializeField] private GameObject helper4;
	[SerializeField] private List<BaseMob> Wave1MobList;
	[SerializeField] private List<BaseMob> Wave2MobList;
	[SerializeField] private List<BaseMob> Wave3MobList;
	[SerializeField] private List<SwitchController> Stage1SwitchList;
	[SerializeField] private List<SwitchController> Stage2SwitchList;
	[SerializeField] private GameObject Door;
	[SerializeField] private GameObject Door2;
	[SerializeField] private GameObject Door3;
	[SerializeField] private CinemachineCamera DoorCam;
	[SerializeField] private CinemachineCamera DoorCam2;
	[SerializeField] private CinemachineCamera DoorCam3;
	[SerializeField] private AudioClip originalBGM;
	[SerializeField] private AudioClip bossBGM;

	[SerializeField] private CinemachineCamera BossCam;
	[SerializeField] private GameObject bossAltar;
	[SerializeField] private GameObject bossRat;
	[SerializeField] private GameObject human;

	bool gameClear = false;

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

			if (count == 4)
			{
				UpdateMission(6);
				helper3.SetActive(false);
				StartCoroutine(DownAltar());
				helper4.SetActive(true);
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
				helper4.SetActive(false);
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

		if (GlobalSceneManager.Instance.CurrentMission == 8)
		{
			if (bossRat == null)
			{
				StartCoroutine(Mission9());
				UpdateMission(9);
			}
		}


		if (GlobalSceneManager.Instance.CurrentMission == 10)
		{
			if (player.currentKey == 4)
			{
				StartCoroutine(Mission10());
				UpdateMission(11);
			}
		}
	}

	private IEnumerator Mission9()
	{
		SoundManager.Instance.PlaySound(SoundManager.GameSound.BossRetire);
		GetComponent<AudioSource>().resource = originalBGM;
		GetComponent<AudioSource>().Play();
		yield return new WaitForSeconds(1f);
		Camera.main.GetComponent<CameraController>().SwitchCamera(DoorCam2, 3f);
		Door2.GetComponent<Animator>().SetTrigger("Open");
	}

	private IEnumerator Mission10()
	{
		yield return new WaitForSeconds(1f);
		Camera.main.GetComponent<CameraController>().SwitchCamera(DoorCam3, 3f);
		Door3.GetComponent<Animator>().SetTrigger("Open");
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
		SoundManager.Instance.PlaySound(SoundManager.GameSound.BossAwake);
		GetComponent<AudioSource>().resource = bossBGM;
		GetComponent<AudioSource>().Play();
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
		if (player.Critters.Count >= RequireCritters[1])
		{
			StartCoroutine(LoadNewStage(1f));
		}
	}

	public void ToNextStage2()
	{
		if (player.Critters.Count >= RequireCritters[2])
		{
			StartCoroutine(LoadNewStage2(1f));
		}
	}

	public void ToNextStage3()
	{
		if (player.Critters.Count >= RequireCritters[3])
		{
			StartCoroutine(LoadNewStage3(1f));
		}
	}

	public void Clear()
	{
		if (human.transform.position.x >= 9.5 && !gameClear)
		{
			gameClear = true;
			uiController.Clear();
		}
	}

	private IEnumerator LoadNewStage(float t)
	{
		player.isMovable = false;
		GlobalSceneManager.Instance.FadeOut();
		yield return new WaitForSeconds(t);
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(false);
		GlobalSceneManager.Instance.CurrentStage++;
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(true);
		player.Init(new Vector3(-9f, 0f, -9f), StageList[GlobalSceneManager.Instance.CurrentStage], 0);
		GlobalSceneManager.Instance.FadeIn();
		UpdateMission(5);
		foreach (BaseMob mob in Wave2MobList)
		{
			mob.gameObject.SetActive(true);
		}
	}

	private IEnumerator LoadNewStage2(float t)
	{
		player.isMovable = false;
		GlobalSceneManager.Instance.FadeOut();
		yield return new WaitForSeconds(t);
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(false);
		GlobalSceneManager.Instance.CurrentStage++;
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(true);
		player.Init(new Vector3(-9f, 0f, -9f), StageList[GlobalSceneManager.Instance.CurrentStage], 0);
		GlobalSceneManager.Instance.FadeIn();
		UpdateMission(10);
		foreach (BaseMob mob in Wave3MobList)
		{
			mob.gameObject.SetActive(true);
		}
	}

	private IEnumerator LoadNewStage3(float t)
	{
		player.isMovable = false;
		GlobalSceneManager.Instance.FadeOut();
		yield return new WaitForSeconds(t);
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(false);
		GlobalSceneManager.Instance.CurrentStage++;
		StageList[GlobalSceneManager.Instance.CurrentStage].gameObject.SetActive(true);
		player.Init(new Vector3(-9f, 0f, -9f), StageList[GlobalSceneManager.Instance.CurrentStage], 0);
		GlobalSceneManager.Instance.FadeIn();
		UpdateMission(12);
	}
}
