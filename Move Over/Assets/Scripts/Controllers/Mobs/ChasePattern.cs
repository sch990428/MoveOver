using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePatternMob : PathFinding
{
	[SerializeField] private float detectRange;
	public float moveDuration = 0.5f;
	private Coroutine moveCoroutine;

	private void Update()
	{
		if (moveCoroutine == null && Vector3.Distance(transform.position, player.position) <= detectRange)
		{
			// 이동 시작
			moveCoroutine = StartCoroutine(MoveAndUpdatePath());
		}
	}

	private IEnumerator MoveAndUpdatePath()
	{
		while (true)
		{

			Vector2Int enemyPosition = new Vector2Int(
				Mathf.RoundToInt(transform.position.x),
				Mathf.RoundToInt(transform.position.z)
			);

			Vector2Int targetPosition = new Vector2Int(
				Mathf.RoundToInt(player.position.x),
				Mathf.RoundToInt(player.position.z)
			);

			// 경로 계산
			List<Vector2Int> newPath = FindPath(enemyPosition, targetPosition);

			if (newPath == null || newPath.Count < 2)
			{
				Debug.Log("No path found");
				moveCoroutine = null; // 이동 종료
				yield break;
			}

			// 한 칸씩 이동
			Vector2Int nextPosition = newPath[1];
			Vector3 startPosition = transform.position;
			Vector3 targetWorldPosition = new Vector3(nextPosition.x, transform.position.y, nextPosition.y);

			Vector3 direction = (targetWorldPosition - transform.position).normalized;
			Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

			float elapsedTime = 0f;
			

			while (elapsedTime < moveDuration)
			{
				// 부드럽게 회전
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);

				// Lerp로 이동
				transform.position = Vector3.Lerp(startPosition, targetWorldPosition, elapsedTime / moveDuration);

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			// 정확한 위치 설정
			transform.position = targetWorldPosition;
		}
	}
}
