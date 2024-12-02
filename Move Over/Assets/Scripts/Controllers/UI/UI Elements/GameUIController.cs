using System;
using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
	[SerializeField] private TMP_Text bombStatUI;
	[SerializeField] private TMP_Text coinStatUI;
	[SerializeField] private TMP_Text critterStatUI;
	[SerializeField] private TMP_Text chapterCode;
	[SerializeField] private TMP_Text chapterName;
	[SerializeField] private TMP_Text stageName;

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
