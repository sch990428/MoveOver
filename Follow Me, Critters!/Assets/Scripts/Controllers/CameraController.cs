using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private PlayerController player;
	[SerializeField] private Transform viewpoints;
	[SerializeField] private List<CinemachineCamera> camerasViews;
	private Camera mainCamera;

	public int viewIndex = 0;
	private bool isRotatable = true;

	// 벽 렌더링 마스크
	private int wallNorthMask;
	private int wallEastMask;
	private int wallSouthMask;
	private int wallWestMask;

	private void Start()
	{
		mainCamera = GetComponent<Camera>();
		camerasViews = new List<CinemachineCamera>();
		for (int i = 0; i < viewpoints.childCount; i++)
		{
			Transform child = viewpoints.GetChild(i);
			camerasViews.Add(child.GetComponent<CinemachineCamera>());
		}

		viewIndex = 0;
		camerasViews[viewIndex].gameObject.SetActive(true);

		// 레이어 마스크 초기화
		wallNorthMask = LayerMask.GetMask("WallUp");
		wallEastMask = LayerMask.GetMask("WallRight");
		wallSouthMask = LayerMask.GetMask("WallDown");
		wallWestMask = LayerMask.GetMask("WallLeft");

		UpdateCameraAndMask();
	}

	private void Update()
	{
		if (isRotatable)
		{
			if (Input.GetKeyUp(KeyCode.E))
			{
				viewIndex--;
				if (viewIndex < 0)
				{
					viewIndex = camerasViews.Count - 1;
				}
				UpdateCameraAndMask();
			}
			else if (Input.GetKeyUp(KeyCode.Q))
			{
				viewIndex++;
				if (viewIndex >= camerasViews.Count)
				{
					viewIndex = 0;
				}
				UpdateCameraAndMask();
			}
		}
	}

	private void UpdateCameraAndMask()
	{
		for (int i = 0; i < 4; i++)
		{
			camerasViews[i].gameObject.SetActive(i == viewIndex);
		}

		switch (viewIndex)
		{
			case 0:
				mainCamera.cullingMask = ~(wallSouthMask | wallWestMask);
				break;
			case 1:
				mainCamera.cullingMask = ~(wallNorthMask | wallWestMask);
				break;
			case 2:
				mainCamera.cullingMask = ~(wallNorthMask | wallEastMask);
				break;
			case 3:
				mainCamera.cullingMask = ~(wallSouthMask | wallEastMask);
				break;
			case -1:
				mainCamera.cullingMask = -1;
				break;
		}
	}
}
