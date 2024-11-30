using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
	private bool isSwitching;
	private int selectedDetailID;
	[SerializeField] private GameObject title;
	[SerializeField] private GameObject lobby;
	public Action<int> onSelectedChange;

	private void OnEnable()
	{
		selectedDetailID = 0;
		isSwitching = true;
		StartCoroutine(InitTitle());
	}

	private void Update()
	{
		if (Input.anyKey && !isSwitching)
		{
			StartCoroutine(GoToLobby());
			isSwitching = true;
		}
	}

	private IEnumerator InitTitle()
	{
		yield return new WaitForSeconds(1f);
		isSwitching = false;
	}

	private IEnumerator GoToLobby()
	{
		GetComponent<Animator>().SetTrigger("ToLobby");
		lobby.SetActive(true);
		onSelectedChange.Invoke(0);
		yield return new WaitForSeconds(1f);
	}

	public void SwitchDetail(int index)
	{
		selectedDetailID = index;
		onSelectedChange.Invoke(selectedDetailID);
	}

	public void SwitchScene()
	{
		GetComponent<Animator>().SetTrigger("SwitchScene");
		StartCoroutine(GoToScene());
	}

	private IEnumerator GoToScene()
	{
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene("GameScene");
	}
}