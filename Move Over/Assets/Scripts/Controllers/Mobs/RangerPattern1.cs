using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class RangerPattern1Mob : BaseMob
{
	[SerializeField] private float detectRange;
	[SerializeField] private float meleeRange;

	private bool isShootable = true;
	public float moveDuration = 1f;
	public float reloadDuration = 5f;

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

			if (Vector3.Distance(transform.position, player.position) > meleeRange && isShootable)
			{
				isShootable = false;
				GameObject go = ResourceManager.Instance.Instantiate("Prefabs/Mobs/Projectiles/GreenApple");
				go.transform.position = transform.position + Vector3.up * 0.2f;
				go.transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
				StartCoroutine(Reload());
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
				// Debug.Log($"{nextPosition}이동 중 차단됨");
				break;
			}

			yield return StartCoroutine(MoveToPosition(nextPosition, moveDuration));
		}
	}

	private IEnumerator Reload()
	{
		yield return new WaitForSeconds(reloadDuration);
		isShootable = true;
	}
}
