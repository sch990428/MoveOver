using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePatternMob : BaseMob
{
	[SerializeField] private float detectRange;
	public float moveDuration = 0.5f;

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
			if (isRetire)
			{
				yield break;
			}

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
				yield return new WaitForSeconds(1f);
				moveCoroutine = null; // 이동 종료
				yield break;
			}

			// 한 칸씩 이동
			Vector3 nextPosition = new Vector3(newPath[1].x, transform.position.y, newPath[1].y);
			bool isBlocked = CheckCollision(nextPosition);
			if (isBlocked)
			{
				break;
			}

			yield return StartCoroutine(MoveToPosition(nextPosition, moveDuration));
		}
	}

	
}
