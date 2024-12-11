using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

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

	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		switch (state)
		{
			case PlayerState.Idle:
				break;
			case PlayerState.Move:
				Vector3 destPosition = transform.position + moveDirection;
				if (!isMoving)
				{
					isMoving = true;
					StartCoroutine(Move(destPosition, moveDuration));
				}
				break;
			case PlayerState.Attack:
				break;
			case PlayerState.GameOver:
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

	public void MoveTo(Vector3 destPosition, float moveDuration)
	{
		StartCoroutine(Move(destPosition, moveDuration));
	}

	private IEnumerator Move(Vector3 destPosition, float duration)
	{
		prevPosition = transform.position;

		Vector3 destDirection = destPosition - prevPosition;

		Quaternion lookRotation = Quaternion.LookRotation(destDirection);
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;

			Vector3 interpolatedPosition = Vector3.Lerp(new Vector3(prevPosition.x, 0, prevPosition.z), new Vector3(destPosition.x, 0, destPosition.z), t);
			Quaternion interpolatedRotation = Quaternion.Lerp(transform.rotation, lookRotation, t);

			rb.MovePosition(interpolatedPosition);
			rb.MoveRotation(interpolatedRotation);
			yield return new WaitForFixedUpdate();
		}

		rb.MovePosition(destPosition);
		rb.MoveRotation(lookRotation);
		isMoving = false;
	}
}
