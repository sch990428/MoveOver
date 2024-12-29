using UnityEngine;

public class Explodable : MonoBehaviour, IBlock, IExplodable
{
	[SerializeField] private GameObject DestroyEffect;
	private Define.BlockType type;
	private bool isDestroy = false;

	public Define.BlockType BlockType
	{
		get { return type; }
	}

	private void Awake()
	{
		type = Define.BlockType.Explodable;
	}

	public void Explode()
	{
		if (isDestroy) return;
		isDestroy = true;
		GameObject effect = Instantiate(DestroyEffect);
		effect.transform.position = transform.position;
		Destroy(gameObject);
	}
}