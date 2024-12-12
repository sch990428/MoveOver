﻿using System.Collections;
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

	private void Awake()
	{
		Critters = new List<CritterController>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.X))
		{
			GameObject go = Instantiate(CritterPrefab);
			if (Critters.Count > 0)
			{
				go.transform.position = Critters[Critters.Count - 1].transform.position;
			}
			else
			{
				go.transform.position = transform.position;
			}
			
			Critters.Add(go.GetComponent<CritterController>());
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

	// 방향에 따른 이동과 회전을 부드럽게 수행
	protected override IEnumerator Move(Vector3 destPosition, float duration)
	{
		yield return StartCoroutine(base.Move(destPosition, duration));
	}
}