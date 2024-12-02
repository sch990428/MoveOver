using UnityEngine;
using UnityEngine.AI;

public class AssultPattern : MonoBehaviour
{
	public Transform player; // 플레이어 Transform
	public NavMeshAgent agent; // NavMeshAgent 컴포넌트

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	void Update()
	{
		if (player != null)
		{
			// 플레이어 위치를 목표로 설정
			agent.SetDestination(player.position);
		}
	}
}
