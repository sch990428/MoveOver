using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	// 이동 관련
	protected Vector3 moveDirection = Vector3.zero;
	public Vector3 prevPosition;
	protected bool isMoving;

	public void MoveTo(Vector3 destPosition, float moveDuration)
	{
		if (!isMoving)
		{
			isMoving = true;
			StartCoroutine(Move(MathUtils.RoundToNearestInt(destPosition), moveDuration));
		}
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
