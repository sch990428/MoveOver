using System.Collections.Generic;
using UnityEngine;

public class LobbyDetailHandler : MonoBehaviour
{
	[SerializeField] private LobbyUIController uiController;
	[SerializeField] private List<GameObject> details;
	int currentDetail;

	private void Awake()
	{
		currentDetail = 0;
		uiController.onSelectedChange -= OnSelected;
		uiController.onSelectedChange += OnSelected;
	}

	private void OnSelected(int i)
	{
		details[currentDetail].SetActive(false);
		currentDetail = i;
		details[i].SetActive(true);
	}
}
