using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GoldApple : MonoBehaviour
{
	public List<GameObject> ExplodeAreas;
	public PlayerController player;
	public Vector3 targetPos;
	public float Damage;
	public int Rad;

	private void Start()
	{
		ExplodeAreas = new List<GameObject>();
		StartCoroutine(RangeAttack());
	}

	private void Update()
	{
		transform.position += Vector3.up * Time.deltaTime * 8f;
	}

	private IEnumerator RangeAttack()
	{
		yield return new WaitForSeconds(1f);
		Vector3 pos = new Vector3(Mathf.RoundToInt(targetPos.x), 0.1f, Mathf.RoundToInt(targetPos.z));

		for (int x = -Rad + 1; x <= Rad; x++)
		{
			for (int z = -Rad; z <= Rad; z++)
			{
				GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.WarningGrid);
				go.transform.position = new Vector3(pos.x + x, 0.02f, pos.z + z);
				ExplodeAreas.Add(go);
			}
		}

		yield return new WaitForSeconds(3f);

		int minIndex = int.MaxValue;

		foreach (GameObject area in ExplodeAreas)
		{
			GameObject go = ResourceManager.Instance.Instantiate("Prefabs/Effects/Bomb Effect/Smoke");
			go.transform.position = area.transform.position;
			PoolManager.Instance.Destroy(Define.PoolableType.WarningGrid, area);

			Collider[] hits = Physics.OverlapBox(area.transform.position, new Vector3(0.5f, 0.7f, 0.5f), Quaternion.identity, LayerMask.GetMask("Critter", "Enemy", "Obstacle", "Bomb"));
			foreach (Collider hit in hits)
			{
				if (hit.CompareTag("Tail"))
				{
					CritterController c = hit.GetComponent<CritterController>();
					if (!c.isRetire)
					{
						minIndex = Mathf.Min(c.Order, minIndex);
					}
				}
				else if (hit.CompareTag("Player"))
				{
					minIndex = -1;
				}
				else if (hit.CompareTag("Explodable"))
				{
					Explodable e = hit.GetComponent<Explodable>();
					e.Explode();
				}
				else if (hit.CompareTag("Monster"))
				{
					BaseMob b = hit.GetComponent<BaseMob>();
					b.GetDamage(Damage);
				}
				else if (hit.CompareTag("Bomb"))
				{
					hit.GetComponent<BombController>().ForcedExplode();
				}
			}

			ResourceManager.Instance.Destroy(go, 1f);
		}

		if (minIndex < int.MaxValue)
		{ player.Damage(minIndex); }

		SoundManager.Instance.PlaySound(SoundManager.GameSound.Explode);
		Destroy(gameObject);
	}

}
