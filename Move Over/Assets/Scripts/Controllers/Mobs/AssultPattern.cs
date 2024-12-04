using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultPatternMob : BaseMob
{
	[SerializeField] private float detectRange;
	public float moveDuration = 0.5f;
	public float assultChannelingDuration = 1f;
	public int assultLength = 5;
	public int assultTerm = 5;

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
		int currentTerm = 0;

		while (true)
		{
			if (isRetire)
			{
				yield break;
			}

			if (currentTerm < assultTerm)
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
					yield return new WaitForSeconds(1f);
					moveCoroutine = null; // 이동 종료
					yield break;
				}

				// 한 칸씩 이동
				Vector3 nextPosition = new Vector3(newPath[1].x, transform.position.y, newPath[1].y);
				bool isBlocked = CheckCollision(nextPosition);
				if (isBlocked)
				{
					// Debug.Log($"{nextPosition}이동 중 차단됨");
					break;
				}

				yield return StartCoroutine(MoveToPosition(nextPosition, moveDuration));
				currentTerm++;
			}
			else
			{
				// Debug.Log("돌진 준비!");
				yield return new WaitForSeconds(assultChannelingDuration);

				for (int i = 0; i < assultLength; i++)
				{
					Vector3 dashTarget = transform.position + transform.forward; // 현재 바라보는 방향으로 한 칸 이동

					// 돌진 이동 중 충돌 여부 검사
					bool isBlocked = CheckCollision(dashTarget);
					if (isBlocked)
					{
						// Debug.Log("돌진 중 차단됨");
						break;
					}

					yield return StartCoroutine(MoveToPosition(dashTarget, moveDuration / 2));
					// Debug.Log($"돌진: {i + 1}/{assultLength}");
					yield return new WaitForSeconds(0.1f);
				}

				currentTerm = 0;
			}
		}
	}

}
