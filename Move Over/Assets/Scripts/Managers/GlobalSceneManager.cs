using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalSceneManager : Singleton<GlobalSceneManager>
{
	public Dictionary<int, Data.BaseStage> StageDict;

	[SerializeField] private GameObject FadeImage;
	[SerializeField] private Image DamageFade;
	[SerializeField] private List<GameObject> StagePrefabs;

	private Stage stage;
	public int CurrentStage;
	public int CurrentMission;

	public bool pause;
	public int deathCount;

	private Animator animator;

	protected override void Awake() 
	{
		base.Awake();
		StageDict = DataManager.Instance.LoadJsonToDict<Data.BaseStage>("Data/stage");
		animator = GetComponent<Animator>();
	}

	public void LoadScene(string sceneName, bool isReplay = false, int index = 0, float speed = 1f)
	{
		animator.speed = speed;
		StartCoroutine(LoadSceneWithFade(sceneName, index, isReplay));
	}

	public void DamageEffect()
	{
		StartCoroutine(Damage());
	}

	public IEnumerator Damage()
	{
		DamageFade.gameObject.SetActive(true);
		float time = 0f;
		float duration = 0.2f;
		float halfDuration = duration / 2;

		Color32 color = DamageFade.color;

		while (time < duration)
		{
			time += Time.deltaTime;
			float alpha;
			if (time <= halfDuration)
			{
				alpha = Mathf.Lerp(0f, 0.1f, time / halfDuration);
			}
			else
			{
				alpha = Mathf.Lerp(0.1f, 0f, (time - halfDuration) / halfDuration);
			}
			DamageFade.color = new Color(color.r, color.g, color.b, alpha);

			yield return null;
		}

		DamageFade.color = new Color(color.r, color.g, color.b, 0f);
		DamageFade.gameObject.SetActive(false);
	}

	public void FadeOut()
	{
		FadeImage.SetActive(true);
		animator.SetTrigger("Out");
	}

	public void FadeIn()
	{
		StartCoroutine(StartFadeIn());
	}

	public PlayerController GetPlayer()
	{
		return stage.player;
	}

	public GridMap GetCurrentMap()
	{
		return stage.GetGridMap(CurrentStage);
	}

	private IEnumerator StartFadeIn()
	{
		animator.SetTrigger("In");
		yield return new WaitForSeconds(1f);
		FadeImage.SetActive(false);
	}

	private IEnumerator LoadSceneWithFade(string sceneName, int index, bool isReplay)
	{
		FadeImage.SetActive(true);
		animator.SetTrigger("Out");
		yield return new WaitForSeconds(1f);
		AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName);
		yield return null;

		while (!sceneLoad.isDone)
		{
			Debug.Log($"Loading: {sceneLoad.progress}");
			yield return null;
		}

		if (sceneName.Equals("GameScene"))
		{
			GameObject gameUI = ResourceManager.Instance.Instantiate("Prefabs/Stage/UI Canvas");
			GameObject stageObject = Instantiate(StagePrefabs[index]);
			stage = stageObject.GetComponent<Stage0>();
			stage.uiController = gameUI.GetComponent<GameUIController>();

			if (isReplay)
			{
				Debug.Log("다시하기");
				deathCount = 1;
				stage.LoadFromCheckPoint();
			}
			else
			{
				Debug.Log("처음부터");
				deathCount = 0;
				CurrentStage = 0;
				CurrentMission = 0;
				stage.LoadFromCheckPoint();
			}

			gameUI.GetComponent<GameUIController>().UpdateDeathMissionUI();

			CameraController cameraController = gameUI.GetComponent<CameraController>();
		}

		animator.SetTrigger("In");
		yield return new WaitForSeconds(1f);
		FadeImage.SetActive(false);
	}
}
