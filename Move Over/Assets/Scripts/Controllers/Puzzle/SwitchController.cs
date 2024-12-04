using UnityEngine;

public class SwitchController : MonoBehaviour
{
	[SerializeField] GameObject Beam;
	public bool isOn = false;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") || other.CompareTag("Tail"))
		{
			Beam.SetActive(true);
			isOn = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") || other.CompareTag("Tail"))
		{
			Beam.SetActive(false);
			isOn = false;
		}
	}
}
