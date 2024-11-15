using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	// 플레이어 이동 관련;
	protected bool isMoving = false;
	public Vector3 prePos;

	public int Order;

	public void Move(Vector3 targetPos)
	{
		prePos = transform.position;
		StartCoroutine(MoveWithJump(targetPos));
	}

	protected IEnumerator MoveWithJump(Vector3 targetPos)
	{
		isMoving = true;

		targetPos.y = 0.5f;
		Vector3 lookPos = targetPos;

		transform.LookAt(lookPos);

		float jumpHeight = 0.5f;
		float elapsedTime = 0;
		float jumpDuration = 0.1f;

		while (elapsedTime < jumpDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / jumpDuration;

			float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;
			transform.position = Vector3.Lerp(prePos, targetPos, t) + Vector3.up * height;

			yield return null;
		}

		transform.position = targetPos;
		isMoving = false;
	}

	public void Retire()
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = false;
		rb.useGravity = true;

		Vector3 randomDirection = new Vector3(
					UnityEngine.Random.Range(-1f, 1f),
					UnityEngine.Random.Range(0f, 1f),
					UnityEngine.Random.Range(-1f, 1f)
				).normalized;

		rb.AddForce(randomDirection * 30, ForceMode.Impulse);
		rb.AddTorque(randomDirection * 10, ForceMode.Impulse);
		Destroy(gameObject, 2f);
	}
}