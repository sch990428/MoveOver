using System;
using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
	[SerializeField] private TMP_Text bombStatUI;
	[SerializeField] private TMP_Text coinStatUI;
	[SerializeField] private TMP_Text critterStatUI;

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
