using System.Collections;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
	// 플레이어 이동 관련
	private Vector3 direction = Vector3.forward;
	private float defaultMoveTimer = 0.3f; 
	private float currentMoveTimer;
	private float moveDist = 1f;
	private bool isMoving = false;

	[SerializeField]
	private CritterController child;

	private void Start()
	{
		currentMoveTimer = defaultMoveTimer;
	}

    private void Update()
    {
		// 일정 시간마다 이동
        if (!isMoving && currentMoveTimer < 0)
        {
            StartCoroutine(MoveWithJump());
			currentMoveTimer = defaultMoveTimer;
		}
        else
        {
			currentMoveTimer -= Time.deltaTime;
        }

		// 방향 전환
		if (Input.GetKeyUp(KeyCode.W))
		{
			direction = Vector3.forward;
		}
		else if (Input.GetKeyUp(KeyCode.S))
		{
			direction = Vector3.back;
		}
		else if (Input.GetKeyUp(KeyCode.A))
		{
			direction = Vector3.left;
		}
		else if (Input.GetKeyUp(KeyCode.D))
		{
			direction = Vector3.right;
		}
	}

    private IEnumerator MoveWithJump()
    {
		isMoving = true;

		Vector3 startPos = transform.position;
		Vector3 targetPos = startPos + direction * moveDist;

		child.Move(startPos);
		transform.LookAt(targetPos);

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