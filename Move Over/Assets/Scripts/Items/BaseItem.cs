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
	public PlayerController Player;

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

	private void LateUpdate()
	{
		transform.forward = Camera.main.transform.forward;
	}

	public virtual void Collected()
	{
		if (!isCollected)
		{
			isCollected = true;
			animator.SetTrigger("Collected");

			switch (type)
			{
				case Define.ItemType.MaxBomb:
					Player.maxBomb++;
					Player.BombCountChange();
					SoundManager.Instance.PlaySound(SoundManager.GameSound.CollectItem);
					break;
				case Define.ItemType.Coin:
					Player.currentCoin++;
					Player.CoinCountChange();
					SoundManager.Instance.PlaySound(SoundManager.GameSound.CollectItem);
					break;
				case Define.ItemType.Critter:
					Player.AddCritter();
					SoundManager.Instance.PlaySound(SoundManager.GameSound.CollectCritter);
					break;
			}
			
			StartCoroutine(Destroy());
		}
	}

	protected IEnumerator Destroy()
	{
		yield return new WaitForSeconds(0.5f);
		Destroy(gameObject);
	}
}
