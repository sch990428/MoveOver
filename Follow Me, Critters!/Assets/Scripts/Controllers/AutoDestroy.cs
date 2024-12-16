using System.Collections;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
	// 특정 시간 후에 자동으로 사라짐
    [SerializeField] private float timer;

	private void Awake()
	{
		StartCoroutine(DestroySelf());
	}

	private IEnumerator DestroySelf()
	{
		float elapsedTime = 0f;
		while (elapsedTime < timer)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		Destroy(gameObject);
	}
}
