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
		SceneManager.LoadSceneAsync(sceneName);
		yield return null;
		animator.SetTrigger("In");
		yield return new WaitForSeconds(1f);
		FadeImage.SetActive(false);
	}

}
