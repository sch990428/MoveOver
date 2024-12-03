using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BaseMob : MonoBehaviour
{
	[SerializeField] private GridMap map;
	[SerializeField] protected GameObject MeleeDamageEffect;
	[SerializeField] protected Image HealthUI;

	protected Dictionary<Vector2Int, Grid> Grids;
	public Transform player; // 플레이어 Transform
	protected Vector2Int enemyPosition; // 적의 현재 위치 (그리드 좌표)
	public float moveSpeed = 2f; // 이동 속도
	public float MaxHP = 10f; // 체력
	public float HP; // 체력

	protected bool isRetire = false;
	public Rigidbody _rigidBody;
	public Collider _collider;

	protected List<Vector2Int> currentPath; // 계산된 경로
	protected bool isMoving = false; // 이동 중 여부
	protected Coroutine moveCoroutine;

	private void Awake()
	{
		HP = MaxHP;
		enemyPosition = new Vector2Int();
		//map.MakeGridMap();
		_collider = GetComponent<Collider>();
		_rigidBody = GetComponent<Rigidbody>();
	}

	// A* 알고리즘 구현
	protected List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
	{
		Grids = map.Grids;

		if (Grids == null)
		{
			Debug.LogError("Grids is not initialized.");
			return null; // Grids가 초기화되지 않은 경우
		}

		if (!Grids.ContainsKey(start))
		{
			Debug.LogError($"Start position {start} is not valid in the grid map.");
			return null; // 시작 지점이 잘못된 경우
		}

		if (!Grids.ContainsKey(target))
		{
			Debug.LogError($"Target position {target} is not valid in the grid map.");
			return null; // 목표 지점이 잘못된 경우
		}

		if (!Grids[target].IsWalkable)
		{
			Debug.LogError($"Target position {target} is not walkable.");
			return null; // 목표 지점이 장애물로 막힌 경우
		}

		List<Vector2Int> openList = new List<Vector2Int>();
		HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();
		Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // 부모 노드
		Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();
		Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>();

		// 시작 노드 초기화
		openList.Add(start);
		gScore[start] = 0;
		fScore[start] = GetHeuristic(start, target);

		while (openList.Count > 0)
		{
			// F값이 가장 낮은 노드를 선택
			Vector2Int current = GetNodeWithLowestFScore(openList, fScore);

			if (current == target)
			{
				// 경로를 재구성
				return ReconstructPath(cameFrom, current);
			}

			openList.Remove(current);
			closedList.Add(current);

			// 현재 노드의 이웃 탐색
			foreach (Vector2Int neighbor in GetNeighbors(current))
			{
				if (closedList.Contains(neighbor) || !Grids[neighbor].IsWalkable)
				{
					continue; // 이미 탐색했거나 이동 불가능한 노드
				}

				int tentativeGScore = gScore[current] + 1; // 기본 이동 비용

				if (!openList.Contains(neighbor))
				{
					openList.Add(neighbor);
				}
				else if (tentativeGScore >= gScore[neighbor])
				{
					continue; // 더 나은 경로가 아니므로 무시
				}

				// 최적 경로 갱신
				cameFrom[neighbor] = current;
				gScore[neighbor] = tentativeGScore;
				fScore[neighbor] = gScore[neighbor] + GetHeuristic(neighbor, target);
			}
		}

		return null; // 경로를 찾을 수 없음
	}

	// 휴리스틱 계산 (맨해튼 거리)
	protected int GetHeuristic(Vector2Int a, Vector2Int b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	// F값이 가장 낮은 노드 찾기
	protected Vector2Int GetNodeWithLowestFScore(List<Vector2Int> openList, Dictionary<Vector2Int, int> fScore)
	{
		Vector2Int bestNode = openList[0];
		int lowestFScore = fScore.ContainsKey(bestNode) ? fScore[bestNode] : int.MaxValue;

		foreach (Vector2Int node in openList)
		{
			int score = fScore.ContainsKey(node) ? fScore[node] : int.MaxValue;
			if (score < lowestFScore)
			{
				bestNode = node;
				lowestFScore = score;
			}
		}

		return bestNode;
	}

	// 이웃 노드 가져오기
	protected List<Vector2Int> GetNeighbors(Vector2Int node)
	{
		List<Vector2Int> neighbors = new List<Vector2Int>();

		Vector2Int[] directions = {
			Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
		};

		// 순서를 무작위로 섞음
		System.Random random = new System.Random();
		directions = directions.OrderBy(x => random.Next()).ToArray();

		foreach (Vector2Int dir in directions)
		{
			Vector2Int neighbor = node + dir;
			if (Grids.ContainsKey(neighbor))
			{
				neighbors.Add(neighbor);
			}
		}

		return neighbors;
	}

	// 경로 재구성
	protected List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
	{
		List<Vector2Int> path = new List<Vector2Int> { current };

		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			path.Add(current);
		}

		path.Reverse();
		return path;
	}

	// 이동 함수
	protected IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
	{
		Vector3 startPosition = transform.position;
		Quaternion targetRotation = Quaternion.LookRotation((targetPosition - startPosition).normalized, Vector3.up);

		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			if (isRetire)
			{
				yield break;
			}

			// 부드러운 회전
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);

			// 부드러운 이동
			transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// 최종 위치 보정
		transform.position = targetPosition;
	}

	// 충돌 검사 함수
	protected bool CheckCollision(Vector3 position)
	{
		Collider[] hits = Physics.OverlapBox(
			position,
			new Vector3(0.49f, 0.7f, 0.49f),
			Quaternion.identity,
			LayerMask.GetMask("Obstacle", "Explodable")
		);

		foreach (Collider hit in hits)
		{
			if (hit.CompareTag("Bomb"))
			{
				hit.GetComponent<BombController>().ForcedExplode();
			}
			else
			{
				return true; // 충돌 발생
			}
		}

		return false; // 충돌 없음
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.CompareTag("Tail"))
		{
			ApplyDamage(collision.transform.GetComponent<CritterController>().Order);

		}
		else if (collision.transform.CompareTag("Player"))
		{
			ApplyDamage(-1);
		}
	}

	protected void ApplyDamage(int order)
	{
		player.GetComponent<PlayerController>().Damage(order);

		GameObject go = Instantiate(MeleeDamageEffect);
		go.transform.position = transform.position + Vector3.up / 2;
		Destroy(go, 1f);
	}

	public void GetDamage(float damage)
	{
		HP -= damage;
		if (HP < 0)
		{
			Retire();
			return;
		}
		HealthUI.fillAmount = HP / MaxHP;
	}

	public void Retire()
	{
		if (!isRetire)
		{
			isRetire = true;

			StopCoroutine(moveCoroutine);
			moveCoroutine = null;

			_rigidBody.constraints = RigidbodyConstraints.None;
			_collider.enabled = false;

			Vector3 randomDirection = new Vector3(Random.Range(0.5f, 1f), 0f, Random.Range(0.5f, 1f));

			int r = Random.Range(0, 2);
			if (r == 0)
			{
				randomDirection = -randomDirection;
			}

			randomDirection = randomDirection.normalized;
			randomDirection.y = 1f;

			_rigidBody.AddForce(randomDirection * 40, ForceMode.Impulse);
			_rigidBody.AddTorque(randomDirection * 5, ForceMode.Impulse);

			ResourceManager.Instance.Destroy(gameObject, 1f);
		}
	}
}
