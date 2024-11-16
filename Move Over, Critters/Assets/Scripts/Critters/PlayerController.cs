using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : CritterController
{
	// 플레이어 이동 관련
	private Vector3 direction = Vector3.forward;
	private float moveDist = 2f;

	// 자식 관련
	public List<CritterController> Tails;

	// 투사체 관련
	public GameObject Projectile;

	private void Start()
	{
		Tails = new List<CritterController>();
		Tails.Add(this);
	}

    private void Update()
    {
		// 방향 전환
		if (Input.GetKey(KeyCode.W))
		{
			direction = Vector3.forward;
			MoveWithChild();
		}
		else if (Input.GetKey(KeyCode.S))
		{
			direction = Vector3.back;
			MoveWithChild();
		}
		else if (Input.GetKey(KeyCode.A))
		{
			direction = Vector3.left;
			MoveWithChild();
		}
		else if (Input.GetKey(KeyCode.D))
		{
			direction = Vector3.right;
			MoveWithChild();
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			Vector3 muzzlePos = transform.position;
			muzzlePos.y = 1f;
			GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.Projectile);
			go.transform.position = muzzlePos;
			go.GetComponent<ProjectileController>().dir = direction;
			PoolManager.Instance.Destroy(Define.PoolableType.Projectile, go, 2f);
		}
	}

	private void MoveWithChild()
	{
		if (!isMoving)
		{
			Move(transform.position + direction * moveDist);

			if (Tails.Count > 0)
			{
				for (int i = 1; i < Tails.Count; i++)
				{
					Tails[i].Move(Tails[i - 1].prePos);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("Item"))
		{
			GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.PlayerTail);

			Vector3 createPos = Tails[Tails.Count - 1].transform.position;
			createPos.y = 0.5f;

			go.transform.position = createPos;
			CritterController newChild = go.GetComponent<CritterController>();
			newChild.Order = Tails.Count;
			Tails.Add(newChild);

			other.transform.position = new Vector3(Random.Range(-5, 5) * 2, 1, Random.Range(-5, 5) * 2);
		}
		else if (other.transform.CompareTag("PlayerTail"))
		{
			CritterController collider = other.GetComponent<CritterController>();
			if (collider.Order != 1)
			{
				for (int i = collider.Order; i < Tails.Count; i++)
				{
					Tails[i].GetComponent<Collider>().enabled = false;
					Tails[i].Retire();
				}
				Tails.RemoveRange(collider.Order, Tails.Count - collider.Order);
			}
		}
	}
}