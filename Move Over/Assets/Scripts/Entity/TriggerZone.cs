using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
	public UnityEvent onTriggerEnterEvent;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			onTriggerEnterEvent.Invoke();
		}
	}
}
