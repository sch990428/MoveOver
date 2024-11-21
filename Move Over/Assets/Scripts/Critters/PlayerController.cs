using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private bool isMoving = false;
	public Vector3 prePos;

	private Vector3 direction = Vector3.forward;
	private float moveDist = 2f;

	private void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			direction = Vector3.forward;
			MoveWithChild();
		}
		else if (Input.GetKey(KeyCode.S))
		{
			direction = Vector3.back;
			MoveWithChild();
		}
		else if (Input.GetKey(KeyCode.A))
		{
			direction = Vector3.left;
			MoveWithChild();
		}
		else if (Input.GetKey(KeyCode.D))
		{
			direction = Vector3.right;
			MoveWithChild();
		}
	}

	private void MoveWithChild()
	{
		if (!isMoving)
		{
			Move(transform.position + direction * moveDist);
		}
	}

	public void Move(Vector3 targetPos)
	{
		prePos = transform.position;
		StartCoroutine(MoveWithJump(targetPos ));
	}

	protected IEnumerator MoveWithJump(Vector3 targetPos)
	{
		isMoving = true;

		targetPos.y = 0f;

		Vector3 lookDirection = (targetPos - transform.position).normalized;
		Vector3 lookOffset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
		Quaternion targetRotation = Quaternion.LookRotation(lookDirection + lookOffset);

		float elapsedTime = 0;
		float jumpDuration = 0.3f;

		while (elapsedTime < jumpDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / jumpDuration;

			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
			transform.position = Vector3.Lerp(prePos, targetPos, t);

			yield return null;
		}

		transform.position = targetPos;
		isMoving = false;
	}
}
