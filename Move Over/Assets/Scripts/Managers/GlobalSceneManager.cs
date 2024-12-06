using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalSceneManager : Singleton<GlobalSceneManager>
{
	public Dictionary<int, Data.BaseStage> StageDict;

	[SerializeField] private GameObject FadeImage;
	[SerializeField] private List<GameObject> StagePrefabs;

	private Stage stage;
	public int CurrentStage;
	public int CurrentMission;

	private Animator animator;

	protected override void Awake() 
	{
		base.Awake();
		StageDict = DataManager.Instance.LoadJsonToDict<Data.BaseStage>("Data/stage");
		animator = GetComponent<Animator>();

		LoadScene("GameScene", false);
	}

	public void LoadScene(string sceneName, bool isReplay = false, int index = 0, float speed = 1f)
	{
		animator.speed = speed;
		StartCoroutine(LoadSceneWithFade(sceneName, index, isReplay));
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
				stage.LoadFromCheckPoint();
			}
			else
			{
				CurrentStage = 0;
				CurrentMission = 0;
			}
			
			CameraController cameraController = gameUI.GetComponent<CameraController>();
		}

		animator.SetTrigger("In");
		yield return new WaitForSeconds(1f);
		FadeImage.SetActive(false);
	}
}
