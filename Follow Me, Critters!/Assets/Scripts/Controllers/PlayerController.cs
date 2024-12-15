using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CritterController
{
	// 플레이어 상태 관련
	public enum PlayerState
	{
		Idle,
		Move,
		Attack,
		GameOver,
	}
	private PlayerState state;

	// 플레이어 이동 관련
	private float moveDuration = 0.3f;

	// 플레이어 부하 관련
	[SerializeField] private GameObject CritterPrefab;
	[SerializeField] private List<CritterController> Critters;

	// 이펙트 관련
	[SerializeField] private GameObject SpawnEffect;

	// 폭탄 관련
	[SerializeField] private GameObject Bomb;

	private void Awake()
	{
		Critters = new List<CritterController>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.X))
		{
			GameObject go = Instantiate(CritterPrefab);
			CritterController critter = go.GetComponent<CritterController>();
			if (Critters.Count > 0)
			{
				go.transform.position = Critters[Critters.Count - 1].prevPosition;
				critter.prevPosition = go.transform.position;
				go.transform.rotation = Critters[Critters.Count - 1].transform.rotation;
			}
			else
			{
				go.transform.position = prevPosition;
				critter.prevPosition = go.transform.position;
				go.transform.rotation = transform.rotation;
			}
			
			GameObject effects = Instantiate(SpawnEffect);
			effects.transform.position = go.transform.position + Vector3.up * 0.5f;
			Destroy(effects, 2f);
			Critters.Add(critter);
		}

		// 캐릭터의 상태값에 따른 동작 수행
		switch (state)
		{
			case PlayerState.Idle:
				// Debug.Log("대기");
				break;

			case PlayerState.Move:
				Vector3 destPosition = transform.position + moveDirection;
				if (!isMoving)
				{
					bool isBlocked = false;
					Collider[] hits = Physics.OverlapBox(destPosition, new Vector3(0.49f, 0.7f, 0.49f), Quaternion.identity, LayerMask.GetMask("Obstacle", "WallUp", "WallDown", "WallLeft", "WallRight"));
					if (hits.Length > 0) 
					{
						isBlocked = true;
						transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(destPosition - transform.position), 0.3f);
					}

					if (!isBlocked)
					{
						isMoving = true;
						StartCoroutine(Move(MathUtils.RoundToNearestInt(destPosition), moveDuration));

						// 부하들 순차적으로 이동
						if (Critters.Count > 0)
						{ 
							Critters[0].MoveTo(prevPosition, moveDuration);
							for (int i = 1; i < Critters.Count; i++)
							{
								Critters[i].MoveTo(Critters[i-1].prevPosition, moveDuration);
							}
						}
					}
				}
				break;

			case PlayerState.Attack:
				break;

			case PlayerState.GameOver:
				break;
		}
	}

	// 플레이어의 입력 데이터를 받아 이동 방향과 상태 업데이트
	private void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

		if (Mathf.Abs(input.x) > 0.5f && Mathf.Abs(input.y) > 0.5f)
		{
			// 대각선 입력 방지 규칙
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
			{
				input = new Vector2(input.x, 0);
			}
			else
			{
				input = new Vector2(0, input.y);
			}
		}

		moveDirection = new Vector3(input.x, 0, input.y);

		if (moveDirection == Vector3.zero)
		{
			state = PlayerState.Idle;
		}
		else
		{
			state = PlayerState.Move;
		}

		int viewIndex = Camera.main.GetComponent<CameraController>().viewIndex;
		moveDirection = Quaternion.Euler(0, 90 * (viewIndex + 1), 0) * moveDirection;
		moveDirection.Normalize();
	}

	private void OnAttack(InputValue value)
	{
		GameObject effects = Instantiate(SpawnEffect);
		effects.transform.position = transform.position + Vector3.up * 0.5f;
		Destroy(effects, 2f);
		Instantiate(Bomb, MathUtils.RoundToNearestInt(transform.position), Quaternion.identity);
	}
}
