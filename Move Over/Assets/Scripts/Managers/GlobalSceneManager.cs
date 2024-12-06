using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalSceneManager : Singleton<GlobalSceneManager>
{
	[SerializeField] private GameObject FadeImage;
	private Animator animator;

	protected override void Awake() 
	{
		base.Awake();
		animator = GetComponent<Animator>();
	}

	public void LoadScene(string sceneName, float speed = 1f)
	{
		animator.speed = speed;
		StartCoroutine(LoadSceneWithFade(sceneName));
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

	private IEnumerator StartFadeIn()
	{
		animator.SetTrigger("In");
		yield return new WaitForSeconds(1f);
		FadeImage.SetActive(false);
	}

	private IEnumerator LoadSceneWithFade(string sceneName)
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
			GameObject stage = ResourceManager.Instance.Instantiate("Prefabs/Stage/Stage 0");
			Stage0 stage0 = stage.GetComponent<Stage0>();
			stage0.uiController = gameUI.GetComponent<GameUIController>();

			CameraController cameraController = gameUI.GetComponent<CameraController>();
			//cameraController.player = stage0.player;
			//cameraController.Init();
		}

		animator.SetTrigger("In");
		yield return new WaitForSeconds(1f);
		FadeImage.SetActive(false);
	}

}
