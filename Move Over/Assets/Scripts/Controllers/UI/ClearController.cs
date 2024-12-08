using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClearController : MonoBehaviour
{
	[SerializeField] private List<GameObject> Stars;
	private bool isInitialized = false;

	public void Init(int star)
	{
		for (int i = 0; i < star; i++)
		{
			Stars[i].SetActive(true);
		}

		isInitialized = true;
	}

	private void Update()
	{
		if (!isInitialized)
			return;

		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			Time.timeScale = 1.0f;
			GlobalSceneManager.Instance.pause = false;
			GlobalSceneManager.Instance.LoadScene("LobbyScene");
		}
	}
}
