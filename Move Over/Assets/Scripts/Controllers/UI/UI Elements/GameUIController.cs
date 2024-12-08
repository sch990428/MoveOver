using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
	[SerializeField] private TMP_Text bombStatUI;
	[SerializeField] private TMP_Text coinStatUI;
	[SerializeField] private TMP_Text critterStatUI;
	[SerializeField] private TMP_Text chapterCode;
	[SerializeField] private TMP_Text chapterName;
	[SerializeField] private TMP_Text stageName;
	[SerializeField] private Image HPBar;

	[SerializeField] private GameObject StageUI;
	[SerializeField] private GameObject GameUI;
	[SerializeField] private GameObject GameoverUI;

	[SerializeField] public TMP_Text missionText;
	[SerializeField] public Image missionProgressUI;

	private bool gameover = false;
	
	private int currentStage;
	private Data.BaseStage stage;

	private void Awake()
	{
		currentStage = StageManager.Instance.currentStage;
		stage = StageManager.Instance.StageDict[currentStage];
		stageName.text = $"{stage.ChapterCode} - {stage.ChapterName}";
		chapterCode.text = stage.ChapterCode;
		chapterName.text = stage.ChapterName;
	}

	private void Update()
	{
		if (gameover)
		{
			if (Input.GetKeyUp(KeyCode.R))
			{
				Time.timeScale = 1f;
				PoolManager.Instance.DestroyAll();
				GlobalSceneManager.Instance.LoadScene("GameScene");
			}
			else if (Input.GetKeyUp(KeyCode.Escape))
			{
				Time.timeScale = 1f;
				PoolManager.Instance.DestroyAll();
				GlobalSceneManager.Instance.LoadScene("LobbyScene");
			}
			else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				Time.timeScale = 1f;
				PoolManager.Instance.DestroyAll();
				GlobalSceneManager.Instance.LoadScene("GameScene", true);
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Escape) && !gameover && !GlobalSceneManager.Instance.pause)
			{
				GlobalSceneManager.Instance.pause = true;
				Time.timeScale = 0f;
				GameObject go = ResourceManager.Instance.Instantiate("Prefabs/UI/ConfirmMessageCanvas");
				ConfirmMessageController cmc = go.GetComponent<ConfirmMessageController>();
				cmc.Init("로비화면으로 돌아갑니다\n진행중인 게임은 저장되지 않습니다.", () =>
				{
					GlobalSceneManager.Instance.pause = false;
					PoolManager.Instance.DestroyAll();
					GlobalSceneManager.Instance.LoadScene("LobbyScene");
				});
			}
		}
	}

	public void GameOver()
	{
		StartCoroutine(DisplayGameover());	
	}

	private IEnumerator DisplayGameover()
	{
		StageUI.SetActive(false);
		GameoverUI.SetActive(true);
		GetComponent<Animator>().SetTrigger("GameOver");
		yield return new WaitForSeconds(0.5f);
		GameUI.SetActive(false);
		yield return new WaitForSeconds(1f);
		gameover = true;
		Time.timeScale = 0f;
		
		
	}

	public void UpdateHpBar(float hp, float maxhp)
	{
		HPBar.fillAmount = hp / maxhp;
	}

	public void UpdateBomb(int current, int max)
	{
		bombStatUI.text = $"{current} / <b>{max}</b>";
	}

	public void UpdateCritter(int current)
	{
		critterStatUI.text = $"{current}";
	}

	public void UpdateCoin(int current, int max)
	{
		coinStatUI.text = $"{current} / <b>{max}</b>";
	}
}
