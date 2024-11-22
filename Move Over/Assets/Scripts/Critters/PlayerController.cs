using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	public enum PlayerState
	{
		Idle,
		Move,
		Attack,
	}

	[SerializeField] private bool isMoving = false;

	public PlayerState state;

	private Vector3 moveDir = Vector3.zero;
	[SerializeField] private float moveDuration = 0.3f;
	[SerializeField] private float moveDistance = 1f;

	private void Update()
	{
		switch (state)
		{
			case PlayerState.Move:
				if (!isMoving)
				{
					StartCoroutine(Move());
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
				input = new Vector2(input.x, 0); // 좌/우 우선
			}
			else
			{
				input = new Vector2(0, input.y); // 상/하 우선
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

	private IEnumerator Move()
	{
		isMoving = true;

		Vector3 prePos = transform.position;
		Vector3 destPos = transform.position + moveDir * moveDistance;
		transform.rotation = Quaternion.LookRotation(moveDir);

		float elapsedTime = 0f;

		while (elapsedTime < moveDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / moveDuration;
			transform.position = Vector3.Lerp(prePos, destPos, t);
			yield return null;
		}
		
		transform.position = destPos;
		isMoving = false;
	}

	private void OnAttack()
	{
		Debug.Log("공격");
	}
}
