using UnityEngine;

public class SwitchController : MonoBehaviour
{
	[SerializeField] GameObject Beam;
	[SerializeField] Color32 NotActiveColor;
	[SerializeField] Color32 ActiveColor;
	[SerializeField] Color32 CompleteColor;
	public bool isOn = false;
	public bool isComplete = false;

	private void Awake()
	{
		Beam.SetActive(false);
		ChangeColor(NotActiveColor);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isComplete && (other.CompareTag("Player") || other.CompareTag("Tail")))
		{
			Beam.SetActive(true);
			ChangeColor(ActiveColor);
			isOn = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!isComplete && (other.CompareTag("Player") || other.CompareTag("Tail")))
		{
			Beam.SetActive(false);
			ChangeColor(NotActiveColor);
			isOn = false;
		}
	}

	private void ChangeColor(Color32 c)
	{
		for (int i = 0; i < 4; i++)
		{
			Beam.transform.GetChild(i).GetComponent<Renderer>().material.color = c;
		}
	}

	public void Complete()
	{
		Beam.SetActive(true);
		ChangeColor(CompleteColor);
		isComplete = true;
	}
}
