using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssultPattern : PathFinding
{
	[SerializeField] private float detectRange;
	private bool isPlayerDetected = false; // 감지 여부

	private void Update()
	{
		Vector2Int enemyPosition; // 적의 현재 위치 (그리드 좌표)
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (distanceToPlayer <= detectRange)
		{
			isPlayerDetected = true;
		}

		if (isPlayerDetected)
		{
			if (!isMoving)
			{
				// 플레이어 위치를 그리드 좌표로 변환
				Vector2Int targetPosition = new Vector2Int(Mathf.RoundToInt(player.position.x), Mathf.RoundToInt(player.position.z));
				enemyPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

				// A* 알고리즘으로 경로 계산
				currentPath = FindPath(enemyPosition, targetPosition);

				if (currentPath != null && currentPath.Count > 1)
				{
					// 다음 목표 위치로 이동
					StartCoroutine(MoveAlongPath(currentPath));
				}
				else
				{
					// 정지
				}
			}
		}
	}

	// 경로를 따라 이동
	private IEnumerator MoveAlongPath(List<Vector2Int> path)
	{
		isMoving = true;

		foreach (Vector2Int position in path)
		{
			Vector3 targetPosition = new Vector3(position.x, transform.position.y, position.y);

			Vector3 direction = (targetPosition - transform.position).normalized;
			Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

			while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);
				transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

				yield return null;
			}

			enemyPosition = position; // 현재 위치 갱신
		}

		isMoving = false;
	}
}
