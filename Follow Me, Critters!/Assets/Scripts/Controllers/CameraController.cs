using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private PlayerController player;
	[SerializeField] private Transform viewpoints;
	[SerializeField] private List<CinemachineCamera> camerasViews;
	private Camera mainCamera;

	// 시점 관련
	public int viewIndex = 0;
	private bool isRotatable = true;

	// 카메라 흔들림 관련
	private float shakeTime;
	private float shakeIntensity;

	// 벽 렌더링 마스크 관련
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

	// 카메라 시점 스위칭
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

	// 카메라 흔들림 효과
	public void OnShakeCamera(float shakeTime = 0.1f, float shakeIntensity = 0.5f)
	{
		this.shakeTime = shakeTime;
		this.shakeIntensity = shakeIntensity;
		StopCoroutine(Shake());
		StartCoroutine(Shake());
	}

	private IEnumerator Shake()
	{
		CinemachineBasicMultiChannelPerlin perlin =
		camerasViews[viewIndex].GetComponent<CinemachineBasicMultiChannelPerlin>();
		perlin.AmplitudeGain = shakeIntensity;
		perlin.FrequencyGain = shakeIntensity;

		while (shakeTime > 0f)
		{
			shakeTime -= Time.deltaTime;

			yield return null;
		}

		perlin.AmplitudeGain = 0;
		perlin.FrequencyGain = 0;
	}
}
