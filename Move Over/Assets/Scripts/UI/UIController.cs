using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private bool isSwitching;
	[SerializeField] private GameObject title;
	[SerializeField] private GameObject lobby;

    private void OnEnable()
	{
        isSwitching = true;
		StartCoroutine(InitTitle());
	}

	private void Update()
    {
        if (Input.anyKey && !isSwitching)
        {
            StartCoroutine(GoToLobby());
			isSwitching = true;
		}
    }

	private IEnumerator InitTitle()
	{
		yield return new WaitForSeconds(1f);
		isSwitching = false;
	}

	private IEnumerator GoToLobby()
    {
		GetComponent<Animator>().SetTrigger("ToLobby");
		yield return new WaitForSeconds(1f);

		title.SetActive(false);
		lobby.SetActive(true);
	}
}
