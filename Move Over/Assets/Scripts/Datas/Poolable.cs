using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public class Poolable : Data.BaseDataEntity
	{
		public int PoolingAmount;
		public string ResourcePath;
		public Queue<GameObject> PoolingQueue;

		public Poolable(int amount, string resourcePath)
		{
			PoolingAmount = amount;
			ResourcePath = resourcePath;
			PoolingQueue = new Queue<GameObject>();
		}
	}
}