using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmMessageController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Message;

    public Button Accept;
	public Button Deny;

    public Action OnAccept;

	private bool isInitialized = false;

	private void Awake()
	{
        Time.timeScale = 0f;
	}

    private void Update()
    {
		if (!isInitialized) return;
		if (Input.GetKeyDown(KeyCode.Escape) && GlobalSceneManager.Instance.pause)
        {
            Close();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) && OnAccept != null)
		{
            OnAccept.Invoke();
			ResourceManager.Instance.Destroy(gameObject);
		}
	}

	public void Init(string msg, Action onAccept)
    {
        Message.text = msg;
        OnAccept = onAccept;
		isInitialized = true;

		Accept.onClick.AddListener(() => {
			if (OnAccept != null)
			{
				OnAccept.Invoke();
			}
		});
	}
    
    public void Close()
    {
		GlobalSceneManager.Instance.pause = false;
		Time.timeScale = 1f;
		ResourceManager.Instance.Destroy(gameObject);
	}
}
