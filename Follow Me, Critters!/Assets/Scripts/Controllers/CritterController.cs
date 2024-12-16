using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	// 이동 관련
	protected Vector3 moveDirection = Vector3.zero;
	public Vector3 prevPosition;
	protected float height;
	protected bool isMoving;
	protected bool isSpinned;

	public void MoveTo(Vector3 destPosition, float moveDuration)
	{
		if (!isMoving)
		{
			isMoving = true;
			StartCoroutine(Move(MathUtils.RoundToNearestInt(destPosition), moveDuration));
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

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Bomb"))
		{
			StartCoroutine(SpinJump());
		}
	}
}
