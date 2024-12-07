using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private void Start()
	{
		CanvasRenderer canvasRenderer = GetComponent<CanvasRenderer>();
		if (canvasRenderer != null)
		{
			// CanvasRenderer의 Render Queue를 3100으로 설정
			canvasRenderer.SetMaterial(new Material(Shader.Find("Unlit/Transparent")), 0);
			canvasRenderer.GetMaterial(0).renderQueue = 3100;
		}
	}

	private void LateUpdate()
	{
		transform.forward = Camera.main.transform.forward;
	}
}
