using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	[SerializeField]
	private CritterController child;

	// 플레이어 이동 관련;
	private bool isMoving = false;

	void Start()
    {

    }

    void Update()
    {

    }

	public void Move(Vector3 targetPos)
	{
		StartCoroutine(MoveWithJump(targetPos));
	}

	private IEnumerator MoveWithJump(Vector3 targetPos)
	{
		isMoving = true;
		Debug.Log(targetPos);

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
	}
}