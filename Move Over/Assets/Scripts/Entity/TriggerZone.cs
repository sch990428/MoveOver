using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
	public UnityEvent onTriggerEnterEvent;
	public UnityEvent onTriggerStayEvent;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			onTriggerEnterEvent.Invoke();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			onTriggerStayEvent.Invoke();
		}
	}
}
