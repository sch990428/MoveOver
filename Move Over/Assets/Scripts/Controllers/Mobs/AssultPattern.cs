using System.Collections.Generic;
using UnityEngine;

public class AssultPattern : MonoBehaviour
{
	[SerializeField] private GridMap map;
	private Dictionary<Vector2Int, Grid> Grids;
	public Transform player; // 플레이어 Transform
	public Vector2Int enemyPosition; // 적의 현재 위치 (그리드 좌표)
	public float moveSpeed = 2f; // 이동 속도

	private List<Vector2Int> currentPath; // 계산된 경로
	private bool isMoving = false; // 이동 중 여부

	private void Awake()
	{
		enemyPosition = new Vector2Int();
		map.MakeGridMap();
		Grids = map.Grids;
	}

	private void Update()
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
				Debug.Log("No path found! Enemy is staying still.");
			}
		}
	}

	// A* 알고리즘 구현
	List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
	{
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
	int GetHeuristic(Vector2Int a, Vector2Int b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	// F값이 가장 낮은 노드 찾기
	Vector2Int GetNodeWithLowestFScore(List<Vector2Int> openList, Dictionary<Vector2Int, int> fScore)
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
	List<Vector2Int> GetNeighbors(Vector2Int node)
	{
		List<Vector2Int> neighbors = new List<Vector2Int>();

		Vector2Int[] directions = {
			Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
		};

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
	List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
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

	// 경로를 따라 이동
	System.Collections.IEnumerator MoveAlongPath(List<Vector2Int> path)
	{
		isMoving = true;

		foreach (Vector2Int position in path)
		{
			Vector3 targetPosition = new Vector3(position.x, transform.position.y, position.y);

			while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
			{
				transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
				yield return null;
			}

			enemyPosition = position; // 현재 위치 갱신
		}

		isMoving = false;
	}
}
