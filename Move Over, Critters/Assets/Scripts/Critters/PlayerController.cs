using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CritterController
{
	// 플레이어 이동 관련
	private Vector3 direction = Vector3.forward;
	private float defaultMoveTimer = 0.3f; 
	private float currentMoveTimer;
	private float moveDist = 2f;

	// 자식 관련
	public List<CritterController> Tails;

	// 투사체 관련
	public GameObject Projectile;

	private void Start()
	{
		currentMoveTimer = defaultMoveTimer;
		Tails = new List<CritterController>();
		Tails.Add(this);
	}

    private void Update()
    {
		// 일정 시간마다 이동
        if (!isMoving && currentMoveTimer < 0)
        {
			Move(transform.position + direction * moveDist);

			if (Tails.Count > 0)
			{
				for (int i = 1; i < Tails.Count; i++)
				{
					Tails[i].Move(Tails[i - 1].prePos);
				}
			}

			currentMoveTimer = defaultMoveTimer;
		}
        else
        {
			currentMoveTimer -= Time.deltaTime;
        }

		// 방향 전환
		if (Input.GetKeyUp(KeyCode.W))
		{
			direction = Vector3.forward;
		}
		else if (Input.GetKeyUp(KeyCode.S))
		{
			direction = Vector3.back;
		}
		else if (Input.GetKeyUp(KeyCode.A))
		{
			direction = Vector3.left;
		}
		else if (Input.GetKeyUp(KeyCode.D))
		{
			direction = Vector3.right;
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			Vector3 muzzlePos = transform.position;
			muzzlePos.y = 1f;
			GameObject go = Instantiate(Projectile, muzzlePos, Quaternion.identity);
			go.GetComponent<ProjectileController>().dir = direction;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("Item"))
		{
			Destroy(other.gameObject);
			GameObject go = Instantiate((GameObject)Resources.Load("Prefabs/Critters/Chicken"));

			Vector3 createPos = Tails[Tails.Count - 1].transform.position;
			createPos.y = 0.5f;

			go.transform.position = createPos;
			CritterController newChild = go.GetComponent<CritterController>();

			Tails.Add(newChild);

			go = Instantiate((GameObject)Resources.Load("Prefabs/Items/Apple"));
			go.transform.position = new Vector3(Random.Range(-5, 5) * 2, 1, Random.Range(-5, 5) * 2);
		}
	}
}