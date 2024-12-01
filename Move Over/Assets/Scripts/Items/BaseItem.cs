using System.Collections;
using UnityEngine;

public interface IBaseItem
{
	void Init();
	public void Collected();
}

public class BaseItem : MonoBehaviour, IBaseItem
{
	[SerializeField] private float alpha;
	[SerializeField] private Define.ItemType type;

	private Animator animator;
	private Renderer _renderer;
	private bool isCollected = false;

	private void Awake()
    {
		Init();
    }

	public void Init()
	{
		isCollected = false;
		animator = GetComponent<Animator>();
		_renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		if (isCollected)
		{
			Color32 color = _renderer.material.color;
			color.a = (byte)Mathf.RoundToInt(255f * alpha);
			_renderer.material.color = color;
		}
	}

	public virtual void Collected()
	{
		if (!isCollected)
		{
			animator.SetTrigger("Collected");
			StartCoroutine(Destroy());
		}
	}

	protected IEnumerator Destroy()
	{
		isCollected = true;
		yield return new WaitForSeconds(0.5f);
		Destroy(gameObject);
	}
}
