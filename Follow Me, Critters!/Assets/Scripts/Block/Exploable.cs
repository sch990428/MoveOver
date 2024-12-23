using UnityEngine;

public class Explodable : MonoBehaviour, IBlock, IExplodable
{
	[SerializeField] private Define.BlockType type;
	[SerializeField] private GameObject DestroyEffect;
	private bool isDestroy = false;

	public Define.BlockType BlockType
	{
		get { return type; }
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