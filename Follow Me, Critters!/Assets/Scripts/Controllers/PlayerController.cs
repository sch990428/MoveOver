﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.UI;

public class PlayerController : CritterController
{
	// 플레이어 상태 관련
	public enum PlayerState
	{
		Idle,
		Move,
		GameOver,
	}
	private PlayerState state;
	private bool isDamageable = true;

	// 플레이어 이동 관련
	private float moveDuration = 0.3f;
	private float sprintDuration = 0.15f;
	private Vector3 prevDirection;
	private bool isSprint;

	// 플레이어 부하 관련
	[SerializeField] private GameObject CritterPrefab;
	[SerializeField] private List<CritterController> Critters;

	// 이펙트 관련
	[SerializeField] private GameObject SpawnEffect;
	[SerializeField] private GameObject MeleeHitEffect;

	// 폭탄 관련
	[SerializeField] private GameObject Bomb;
	public int MaxBomb = 3;
	public int CurrentBomb = 3;

	private void Awake()
	{
		Critters = new List<CritterController>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.X))
		{
			AddCritter();
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
					if (!isSprint)
					{
						Walk(destPosition);
					}
					else
					{
						Sprint(destPosition);
					}
				}
				break;
			case PlayerState.GameOver:
				break;
		}
	}

	private void AddCritter()
	{
		GameObject go = Instantiate(CritterPrefab);
		CritterController critter = go.GetComponent<CritterController>();
		critter.order = Critters.Count;
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
		Critters.Add(critter);
	}

	// 단순 이동 로직
	private void Walk(Vector3 destPosition)
	{
		bool isBlocked = false;
		Collider[] hits = Physics.OverlapBox(destPosition, new Vector3(0.49f, 0.7f, 0.49f), Quaternion.identity, LayerMask.GetMask("Obstacle", "WallUp", "WallDown", "WallLeft", "WallRight"));
		if (hits.Length > 0)
		{
			isBlocked = true;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(destPosition - transform.position), 0.3f);
		}

		// 부하가 하나라도 존재하면 직접 후진 불가능
		if (Critters.Count > 0 && prevDirection + moveDirection == Vector3.zero)
		{
			isBlocked = true;
		}

		if (!isBlocked)
		{
			isMoving = true;
			prevDirection = moveDirection;
			StartCoroutine(Move(MathUtils.RoundToNearestInt(destPosition), moveDuration));

			// 부하들 순차적으로 이동
			if (Critters.Count > 0)
			{
				Critters[0].MoveTo(prevPosition, moveDuration);
				for (int i = 1; i < Critters.Count; i++)
				{
					Critters[i].MoveTo(Critters[i - 1].prevPosition, moveDuration);
				}
			}
		}
	}

	// 회피 이동 로직
	private void Sprint(Vector3 destPosition)
	{
		// 모든 부하들을 순회하여 방향에 장애물이 없는지 체크
		if (IsBlockedAtDirection(moveDirection)) { return; }
		foreach (CritterController c in Critters)
		{
			if (c.IsBlockedAtDirection(moveDirection)) { return; }
		}

		isMoving = true;
		StartCoroutine(Sprint(MathUtils.RoundToNearestInt(destPosition), sprintDuration));

		// 부하들 순차적으로 이동
		if (Critters.Count > 0)
		{
			foreach (CritterController critter in Critters)
			{
				critter.SprintTo(critter.transform.position + moveDirection, sprintDuration);
			}
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

	// 옆걸음 동작 토글
	private void OnSprint(InputValue value)
	{
		isSprint = value.isPressed;
	}

	// 공격 동작
	private void OnAttack(InputValue value)
	{
		if (CurrentBomb > 0)
		{
			GameObject effects = Instantiate(SpawnEffect);
			effects.transform.position = transform.position + Vector3.up * 0.5f;
			Destroy(effects, 2f);
			GameObject bomb = Instantiate(Bomb, MathUtils.RoundToNearestInt(transform.position), Quaternion.identity);
			bomb.GetComponent<BombController>().Player = this;

			CurrentBomb--;
		}
	}

	// 충돌 판정
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Critter"))
		{
			CritterController critter = other.GetComponent<CritterController>();
			// 첫번째 부하와는 충돌이 불가능
			if (critter.order != 0)
			{
				GameObject effect = Instantiate(MeleeHitEffect);
				effect.transform.position = other.transform.position + Vector3.up * 0.5f;
				Damage(critter.order);
			}
		}
	}

	// 플레이어와 부하들에게 데미지
	public void Damage(int hitPoint)
	{
		if (!isDamageable) { return; }
		isDamageable = false;

		if (hitPoint < 0)
		{
			// 플레이어가 직격당한 경우
			if (Critters.Count > 0)
			{
				foreach (CritterController c in Critters)
				{
					c.Retire();
				}

				Critters.Clear();
			}

			// TODO : 체력에 데미지 및 UI 업데이트
		}
		else if (hitPoint >= Critters.Count)
		{
			return;
		}
		else
		{
			// 부하가 피격당한 경우
			for (int i = hitPoint; i < Critters.Count; i++)
			{
				Critters[i].Retire();
			}

			Critters.RemoveRange(hitPoint, Critters.Count - hitPoint);
		}

		isDamageable = true;
	}
}
