using System.Collections.Generic;
using UnityEngine;

public class Grid
{
	public bool IsWalkable;

	public Grid(bool isWalkable)
	{
		IsWalkable = isWalkable;
	}
}

public class GridMap : MonoBehaviour
{
	// 스테이지에서 A* 노드를 계산할 최대, 최소 좌표값
	public int Min_X;
	public int Max_X;
	public int Min_Z;
	public int Max_Z;

	public Dictionary<Vector2Int, Grid> Grids;

	private void Awake()
	{
		MakeGridMap();
	}

	private void OnEnable()
	{
		MakeGridMap();
	}

	public void MakeGridMap()
	{
		// Grids 채우기
		// 초기에 1회 실행한 이후에는 블록이 파괴되거나 생성될때만 갱신해주면 됨
		Grids = new Dictionary<Vector2Int, Grid>();

		int count = 0;
		for (int x = Min_X; x <= Max_X; x++)
		{
			for (int z = Min_Z; z <= Max_Z; z++)
			{
				Vector2Int position = new Vector2Int(x, z);
				bool isWalkable = !IsObstacleAtGrid(position);
				if (!isWalkable)
				{
					count++;
				}
				Grids[position] = new Grid(isWalkable);
			}
		}

		//Debug.Log(count);
	}

	// 해당 위치에 장애물이 있는지 여부 반환
	private bool IsObstacleAtGrid(Vector2Int pos)
	{
		Vector3 grid = new Vector3(pos.x, 0, pos.y);
		bool blocked = Physics.Raycast(grid + Vector3.down * 0.5f, Vector3.up, 1f, LayerMask.GetMask("Obstacle"));

		return blocked;
	}
}
