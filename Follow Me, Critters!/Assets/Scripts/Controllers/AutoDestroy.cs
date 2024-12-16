using System.Collections;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
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
