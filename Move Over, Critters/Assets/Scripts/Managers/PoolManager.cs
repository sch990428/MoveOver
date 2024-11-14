using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
	public enum PoolableType
	{
		Player,
		Projectile,
	}

	Dictionary<int, Data.Poolable> poolingDict;

	protected override void Awake()
	{
		base.Awake();

		poolingDict = new Dictionary<int, Data.Poolable>();
		Init();
	}

	private void Init()
	{
		poolingDict = DataManager.Instance.LoadJsonToDict<Data.Poolable>("Data/Poolable");
		
		foreach (var poolPair in poolingDict)
		{
			for (int i = 0; i < poolPair.Value.PoolingAmount; i++)
			{
				GameObject go = ResourceManager.Instance.Instantiate(poolPair.Value.ResourcePath);
				go.transform.SetParent(transform);
				go.name = ((PoolableType)poolPair.Key).ToString();
				go.SetActive(false);
				poolPair.Value.PoolingQueue.Enqueue(go);
			}
		}
	}
}
