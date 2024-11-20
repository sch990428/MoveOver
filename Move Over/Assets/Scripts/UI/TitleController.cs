using System.Collections;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    private bool isSwitching;

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
		GetComponent<Animator>().SetTrigger("Disable");
		yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
	}
}
