using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	[SerializeField]
	protected CritterController parent;
	[SerializeField]
	protected CritterController child;

	// 플레이어 이동 관련;
	protected bool isMoving = false;
	protected bool godMode = true;

	protected void Move(Vector3 targetPos)
	{
		StartCoroutine(MoveWithJump(targetPos));
	}

	protected IEnumerator MoveWithJump(Vector3 targetPos)
	{
		isMoving = true;

		Vector3 startPos = transform.position;

		if (child != null)
		{
			child.Move(startPos);
		}

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
			transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * height;

			yield return null;
		}

		transform.position = targetPos;
		isMoving = false;
		godMode = false;
	}

	protected void OnTriggerEnter(Collider other)
	{
		
		if (other.transform.CompareTag("Item"))
		{
			Destroy(other.gameObject);
			GameObject go = Instantiate((GameObject)Resources.Load("Prefabs/Critters/Chicken"));

			CritterController tailCritter = GetLastChild();

			Vector3 createPos = tailCritter.transform.position;
			createPos.y = 0.5f;

			go.transform.position = createPos;
			CritterController newChild = go.GetComponent<CritterController>();
			newChild.parent = tailCritter;
			tailCritter.child = newChild;

			go = Instantiate((GameObject)Resources.Load("Prefabs/Items/Apple"));
			go.transform.position = new Vector3(Random.Range(-5, 5) * 2, 1, Random.Range(-5, 5) * 2);
		}
		else if (other.transform.CompareTag("PlayerHead"))
		{
			if (!godMode)
			{
				Cut();
			}
		}
	}

	protected CritterController GetLastChild()
	{
		CritterController current = this;
		while (current.child != null)
		{
			current = current.child;
		}

		return current;
	}

	private void Cut()
	{
		if (parent != null)
		{
			parent.child = null;
		}

		if (child != null)
		{
			child.Cut();
		}

		Retire();
	}

	private void Retire()
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
		parent = null;
		child = null;

		Destroy(gameObject, 2f);
	}
}