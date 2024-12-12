using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
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
	private Vector3 moveDirection = Vector3.zero;
	private Vector3 prevPosition;
	private float moveDuration = 0.3f;
	private bool isMoving;

	private void Awake()
	{

	}

	private void Update()
	{
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
	private IEnumerator Move(Vector3 destPosition, float duration)
	{
		Debug.Log($"{moveDirection}");
		prevPosition = MathUtils.RoundToNearestInt(transform.position);

		Vector3 destDirection = destPosition - prevPosition;

		Quaternion lookRotation = Quaternion.LookRotation(destDirection);
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;

			transform.position = Vector3.Slerp(prevPosition, destPosition, t);
			transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, t);

			if (Mathf.Approximately(destDirection.x, 0))
			{
				transform.position = new Vector3(prevPosition.x, 0, transform.position.z);
			}
			if (Mathf.Approximately(destDirection.z, 0))
			{
				transform.position = new Vector3(transform.position.x, 0, prevPosition.z);
			}

			yield return null;
		}

		// 이동 직후 좌표를 정수 위치에 스냅
		transform.position = destPosition;
		transform.rotation = lookRotation;
		isMoving = false;
	}
}
