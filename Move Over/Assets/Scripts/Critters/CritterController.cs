using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	[SerializeField] protected bool isMoving = false;
	public Vector3 prePos;

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

			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
			transform.position = Vector3.Lerp(prePos, destPos, t);

			yield return null;
		}

		transform.position = destPos;
		isMoving = false;
	}
}
