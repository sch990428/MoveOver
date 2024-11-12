using System.Collections;
using UnityEngine;

public class PlayerController : CritterController
{
	// 플레이어 이동 관련
	private Vector3 direction = Vector3.forward;
	private float defaultMoveTimer = 0.3f; 
	private float currentMoveTimer;
	private float moveDist = 1f;

	private void Start()
	{
		currentMoveTimer = defaultMoveTimer;
	}

    private void Update()
    {
		// 일정 시간마다 이동
        if (!isMoving && currentMoveTimer < 0)
        {
			Move(transform.position + direction * moveDist);
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
}