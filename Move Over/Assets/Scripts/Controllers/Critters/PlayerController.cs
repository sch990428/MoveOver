using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CritterController
{
	// 플레이어 행동상태 관련
	public enum PlayerState
	{
		Idle,
		Move,
		Attack,
	}

	public PlayerState state;

	// 플레이어 이동 관련
	private Vector3 moveDir = Vector3.zero;
	[SerializeField] private float moveDuration = 0.3f;
	[SerializeField] private float moveDistance = 1f;

	// 플레이어 콜라이더 관련
	private Collider playerCollider;
	private float bombY;

	// 플레이어 부하 관련
	public List<CritterController> Critters;

	private void Awake()
	{
		playerCollider = GetComponent<Collider>();
		bombY = playerCollider.bounds.max.y;
	}

	private void Update()
	{
		// 테스트용 부하 생성
		if (Input.GetKeyDown(KeyCode.X))
		{
			GameObject go = Instantiate((GameObject)Resources.Load("Prefabs/Critters/Rooster Critter"));

			if (Critters.Count > 0)
			{
				go.transform.position = Critters[Critters.Count - 1].transform.position;
				go.transform.rotation = Critters[Critters.Count - 1].transform.rotation;
			}
			else
			{
				go.transform.position = transform.position;
				go.transform.rotation = transform.rotation;
			}

			CritterController c = go.GetComponent<CritterController>();
			Critters.Add(c);
			c.prePos = c.transform.position;
		}

		switch (state)
		{
			case PlayerState.Move:
				if (!isMoving)
				{
					prePos = transform.position;
					Vector3 destPos = transform.position + moveDir * moveDistance;

					// 충돌 처리
					Debug.DrawRay(prePos, moveDir, Color.red, 1f);
					RaycastHit hit;
					if (Physics.Raycast(prePos, moveDir, out hit, 1f, LayerMask.GetMask("Obstacle")))
					{
						Debug.Log(hit.collider.name);
						break;
					}

					isMoving = true;

					// 부하들을 순차적으로 이동
					if (Critters.Count > 0)
					{
						Critters[0].MoveTo(prePos, moveDuration);
						for (int i = 1; i < Critters.Count; i++)
						{
							Critters[i].MoveTo(Critters[i - 1].prePos, moveDuration);
						}
					}

					StartCoroutine(Move(destPos, moveDuration));
				}
				break;
		}
	}

	private void OnMove(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();

        if (Mathf.Abs(input.x) > 0.5f && Mathf.Abs(input.y) > 0.5f)
        {
			// 대각선 입력 방지 규칙
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
			{
				input = new Vector2(input.x, 0); // 좌우 우선
			}
			else
			{
				input = new Vector2(0, input.y); // 상하 우선
			}
		}

		moveDir = new Vector3(input.x, 0, input.y);

		if (moveDir == Vector3.zero)
		{
			state = PlayerState.Idle;
		}
		else
		{
			state = PlayerState.Move;
		}
		moveDir.Normalize();
	}

	private void OnAttack()
	{
		SoundManager.Instance.PlaySound(SoundManager.GameSound.CreateBomb);
		GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Bomb"));
		Vector3 pos = transform.position;
		pos.y = bombY;
		go.GetComponent<BombController>().SetPosition(new Vector3(Mathf.RoundToInt(pos.x), pos.y, Mathf.RoundToInt(pos.z)));
		bomb = go.GetComponent<BombController>();
		StartCoroutine(Spin());
	}

	private void OnCollisionEnter(Collision collision)
	{
		
	}
}