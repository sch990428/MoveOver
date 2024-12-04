using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private List<CinemachineCamera> camerasViews;
	public int viewIndex;

	private float shakeTime;
	private float shakeIntensity;

	Vector3 defaultPos;
	Vector3 defaultRot;
	Color32 defaultColor;

	Material material;
	
	private void Awake()
	{
		viewIndex = 0;
		camerasViews[viewIndex].gameObject.SetActive(true);

		defaultPos = transform.position; // 카메라 초기위치 저장
		defaultRot = transform.eulerAngles; // 카메라 초기방향 저장
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
		}
		else if (Input.GetKeyUp(KeyCode.Q))
		{
			viewIndex++;
			if (viewIndex >= camerasViews.Count)
			{
				viewIndex = 0;
			}
		}

		for (int i = 0; i < 4; i++)
		{
			camerasViews[i].gameObject.SetActive(i == viewIndex);
		}
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
