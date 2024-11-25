using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BombController : MonoBehaviour
{
	[SerializeField] GameObject RangeGrid;

	public int Range;

	public void SetPosition(Vector3 pos)
	{
		Range = 8;
		transform.position = pos;

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				GameObject go = Instantiate(RangeGrid);
				go.transform.position = new Vector3(pos.x + x, 0.01f, pos.z + z);
			}
		}
	}

	public void AddRange()
	{

	}

	void Update()
    {
        
    }
}
