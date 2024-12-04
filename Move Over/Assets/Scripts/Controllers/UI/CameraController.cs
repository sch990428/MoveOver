using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private List<CinemachineCamera> camerasViews;
	private int preCam;
	private Camera mainCamera;

	public int viewIndex;

	private float shakeTime;
	private float shakeIntensity;

	private Vector3 defaultPos;
	private Vector3 defaultRot;
	private Color32 defaultColor;

	private int wallNorthMask;
	private int wallEastMask;
	private int wallSouthMask;
	private int wallWestMask;

	Material material;
	
	private void Awake()
	{
		mainCamera = GetComponent<Camera>();

		viewIndex = 0;
		camerasViews[viewIndex].gameObject.SetActive(true);

		defaultPos = transform.position; // 카메라 초기위치 저장
		defaultRot = transform.eulerAngles; // 카메라 초기방향 저장

		// 레이어 마스크 초기화
		wallNorthMask = LayerMask.GetMask("WallUp");
		wallEastMask = LayerMask.GetMask("WallRight");
		wallSouthMask = LayerMask.GetMask("WallDown");
		wallWestMask = LayerMask.GetMask("WallLeft");

		UpdateCullingMask();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.E))
		{
			viewIndex--;
			if (viewIndex < 0)
			{
				viewIndex = camerasViews.Count - 1;
			}
			UpdateCullingMask();
		}
		else if (Input.GetKeyUp(KeyCode.Q))
		{
			viewIndex++;
			if (viewIndex >= camerasViews.Count)
			{
				viewIndex = 0;
			}
			UpdateCullingMask();
		}
	}

	private void UpdateCullingMask()
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

	public void SwitchCamera(CinemachineCamera newCam, float time)
	{
		preCam = viewIndex;
		newCam.gameObject.SetActive(true);
		StartCoroutine(Switch(newCam, time));
	}

	protected IEnumerator Switch(CinemachineCamera newCam, float time)
	{
		viewIndex = -1;
		UpdateCullingMask();
		yield return new WaitForSeconds(time);
		newCam.gameObject.SetActive(false);
		viewIndex = preCam;
		UpdateCullingMask();
	}

	// 카메라 위치기반 흔들림 효과
	public void OnShakeCameraByPosition(float shakeTime = 1f, float shakeIntensity = 0.5f)
	{
		this.shakeTime = shakeTime;
		this.shakeIntensity = shakeIntensity;
		// Debug.Log("여러번");
		StopCoroutine(ShakeByPosition());
		StartCoroutine(ShakeByPosition());
	}

	// 위치기반
	private IEnumerator ShakeByPosition()
	{
		CinemachineBasicMultiChannelPerlin perlin =
		camerasViews[0].GetComponent<CinemachineBasicMultiChannelPerlin>();
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
