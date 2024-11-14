using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
	public Dictionary<int, Data.Poolable> poolingDict;

	protected override void Awake()
	{
		base.Awake();

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
				go.name = ((Define.PoolableType)poolPair.Key).ToString();
				go.SetActive(false);
				poolPair.Value.PoolingQueue.Enqueue(go);
			}
		}
	}

	public GameObject Instantiate(Define.PoolableType type)
	{
		GameObject go;

		if (poolingDict[(int)type].PoolingQueue.Count > 0)
		{
			go = poolingDict[(int)type].PoolingQueue.Dequeue();
			go.transform.SetParent(null);
			go.SetActive(true);
		}
		else
		{
			go = ResourceManager.Instance.Instantiate(poolingDict[(int)type].ResourcePath);
		}

		return go;
	}

	public void Destroy(Define.PoolableType type, GameObject go, float t = 0f)
	{
		StartCoroutine(EnqueueOrDestroy(type, go, t));
	}

	private IEnumerator EnqueueOrDestroy(Define.PoolableType type, GameObject go, float t)
	{
		yield return new WaitForSeconds(t);
		if (poolingDict[(int)type].PoolingQueue.Count < poolingDict[(int)type].PoolingAmount)
		{
			go.SetActive(false);
			go.name = type.ToString();
			poolingDict[(int)type].PoolingQueue.Enqueue(go);
			go.transform.SetParent(transform);
		}
		else
		{
			ResourceManager.Instance.Destroy(go, t);
		}
	}
}
