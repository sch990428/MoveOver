using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	[SerializeField] protected bool isMoving = false;
	public Vector3 prePos;
	public float height;
	public bool isSpinned = false;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.CompareTag("Bomb"))
		{
			StartCoroutine(Spin());
		}
	}

	private IEnumerator Spin()
	{
		isSpinned = true;
		float elapsedTime = 0f;
		float spinDuration = 0.4f; // 회전 지속 시간
		float jumpDuration = 0.15f; // 점프 지속 시간
		Vector3 rotationAxis = Vector3.up; // Y축 기준으로 회전

		int r = Random.Range(0, 2);
		int dir = 360;
		if (r == 0) { dir = -dir; }

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
	}

	public void MoveTo(Vector3 destPos, float moveDuration)
	{
		prePos = transform.position;
		StartCoroutine(Move(destPos, moveDuration));
	}

	private IEnumerator Move(Vector3 destPos, float moveDuration)
	{
		Vector3 prePos = transform.position;
		Vector3 destDir = destPos - prePos;
		destDir.Normalize();
		Quaternion targetRotation = Quaternion.LookRotation(destDir);

		isMoving = true;
		float elapsedTime = 0f;

		while (elapsedTime < moveDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / moveDuration;

			if (!isSpinned)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
			}

			transform.position = Vector3.Lerp(new Vector3(prePos.x, height, prePos.z), new Vector3(destPos.x, height, destPos.z), t);

			yield return null;
		}

		transform.position = destPos;
		isMoving = false;
	}
}
