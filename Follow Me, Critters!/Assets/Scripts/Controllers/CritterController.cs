﻿using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Splines;

public class CritterController : MonoBehaviour
{
	// 부하 상태 관련
	public int order;
	public bool isRetire;

	// 충돌 관련
	private Collider _collider;
	private Rigidbody _rigidBody;

	// 이동 관련
	protected Vector3 moveDirection = Vector3.zero;
	public Vector3 prevPosition;
	protected float height;
	protected bool isMoving;
	protected bool isSpinned;
	private float sprintDelay = 0.4f;

	private void Awake()
	{
		_collider = GetComponent<Collider>();
		_rigidBody = GetComponent<Rigidbody>();
	}

	public void MoveTo(Vector3 destPosition, float moveDuration)
	{
		if (!isMoving)
		{
			isMoving = true;
			StartCoroutine(Move(MathUtils.RoundToNearestInt(destPosition), moveDuration));
		}
	}

	public void SprintTo(Vector3 destPosition, float moveDuration)
	{
		if (!isMoving)
		{
			isMoving = true;
			StartCoroutine(Sprint(MathUtils.RoundToNearestInt(destPosition), moveDuration));
		}
	}

	// 폭탄 전달시 스핀점프
	protected IEnumerator SpinJump()
	{
		isSpinned = true;
		float elapsedTime = 0f;
		float spinDuration = 0.4f; // 회전 지속 시간
		float jumpDuration = 0.15f; // 점프 지속 시간
		Vector3 rotationAxis = Vector3.up; // Y축 기준으로 회전

		int r = Random.Range(0, 2);
		int dir = 360;
		if (r == 0)
		{ dir = -dir; }

		while (elapsedTime < spinDuration)
		{
			elapsedTime += Time.deltaTime;

			// 한 프레임 동안 회전할 각도 계산
			float angle = (dir / spinDuration) * Time.deltaTime;

			// 특정 축을 기준으로 회전
			transform.RotateAround(transform.position, rotationAxis, angle);

			yield return null;
		}

		isSpinned = false;

		elapsedTime = 0f;

		while (elapsedTime < jumpDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / jumpDuration;
			height = Mathf.Sin(t * Mathf.PI) * 0.3f;
			yield return null;
		}

		height = 0;
	}

	// 방향에 따른 이동과 회전을 부드럽게 수행
	protected virtual IEnumerator Move(Vector3 destPosition, float duration)
	{
		prevPosition = MathUtils.RoundToNearestInt(transform.position);

		Vector3 destDirection = destPosition - prevPosition;
		
		Quaternion lookRotation;
		if (destDirection != Vector3.zero)
		{
			 lookRotation = Quaternion.LookRotation(destDirection);
		}
		else
		{
			lookRotation = transform.rotation;
		}

		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			if (isRetire) { yield break; }
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;

			transform.position = Vector3.Slerp(prevPosition, destPosition, t);

			if (!isSpinned) { transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, t); }

			if (Mathf.Approximately(destDirection.x, 0))
			{
				transform.position = new Vector3(prevPosition.x, height, transform.position.z);
			}
			if (Mathf.Approximately(destDirection.z, 0))
			{
				transform.position = new Vector3(transform.position.x, height, prevPosition.z);
			}

			yield return null;
		}

		// 이동 직후 좌표를 정수 위치에 스냅
		transform.position = destPosition;
		transform.rotation = lookRotation;
		isMoving = false;
	}

	// 회피 시 옆으로 점프 이동
	protected virtual IEnumerator Sprint(Vector3 destPosition, float duration)
	{
		prevPosition = MathUtils.RoundToNearestInt(transform.position);

		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;

			height = Mathf.Sin(t * Mathf.PI) * 0.3f;

			transform.position = Vector3.Lerp(new Vector3(prevPosition.x, height, prevPosition.z), new Vector3(destPosition.x, height, destPosition.z), t);
			yield return null;
		}

		transform.position = destPosition;
		height = 0;
		StartCoroutine(SprintDelay());
	}

	private IEnumerator SprintDelay()
	{
		yield return new WaitForSeconds(sprintDelay);
		isMoving = false;
	}

	public bool IsBlockedAtDirection(Vector3 targetDirection)
	{
		Collider[] hits = Physics.OverlapBox(transform.position + targetDirection, new Vector3(0.49f, 0.7f, 0.49f), Quaternion.identity, 
			LayerMask.GetMask("Obstacle", "WallUp", "WallDown", "WallLeft", "WallRight"));

		return hits.Length > 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Bomb"))
		{
			StartCoroutine(SpinJump());
		}
	}

	public void Retire()
	{
		if (!isRetire)
		{
			isRetire = true;

			_rigidBody.constraints = RigidbodyConstraints.None;
			_rigidBody.isKinematic = false;
			_collider.enabled = false;

			Vector3 randomDirection = new Vector3(Random.Range(0.5f, 1f), 0f, Random.Range(0.5f, 1f));

			int r = Random.Range(0, 2);
			if (r == 0)
			{
				randomDirection = -randomDirection;
			}

			randomDirection = randomDirection.normalized;
			randomDirection.y = 1f;

			_rigidBody.AddForce(randomDirection * 40, ForceMode.Impulse);
			_rigidBody.AddTorque(randomDirection * 5, ForceMode.Impulse);

			Destroy(gameObject, 1f);
		}
	}
}
