using UnityEngine;

public class GreenApple : MonoBehaviour
{
    public float Speed;

	private void Update()
	{
		transform.Translate(Vector3.forward * Speed);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Explodable"))
		{
			Destroy(gameObject);
		}
		
		if (collision.gameObject.CompareTag("Bomb"))
		{
			collision.transform.GetComponent<BombController>().ForcedExplode();
			Destroy(gameObject);
		}
		
		if (collision.gameObject.CompareTag("Tail"))
		{
			CritterController c = collision.gameObject.GetComponent<CritterController>();
			c.player.Damage(c.Order);
			Destroy(gameObject);
		}

		if (collision.gameObject.CompareTag("Player"))
		{
			collision.gameObject.GetComponent<PlayerController>().Damage(-1);
			Destroy(gameObject);
		}
	}
}
