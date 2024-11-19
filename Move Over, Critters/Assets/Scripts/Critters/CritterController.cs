using System.Collections;
using TMPro;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	// 플레이어 이동 관련;
	protected bool isMoving = false;
	public Vector3 prePos;

	public int Order;

	public TMP_Text text;

	private void OnEnable()
	{
		transform.rotation = Quaternion.Euler(Vector3.forward);
		
	}

	public void Move(Vector3 targetPos)
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;

		prePos = transform.position;

		if (text != null)
		{
			text.text = Order.ToString();
		}
		StartCoroutine(MoveWithJump(targetPos));
	}

	protected IEnumerator MoveWithJump(Vector3 targetPos)
	{
		isMoving = true;

		targetPos.y = 0.5f;

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
		PoolManager.Instance.Destroy(Define.PoolableType.PlayerTail, gameObject, 2f);
	}
}