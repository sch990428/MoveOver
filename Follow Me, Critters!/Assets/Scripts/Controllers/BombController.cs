using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class BombController : MonoBehaviour
{
    // 타이머 관련
    [SerializeField] Image TimerUI;
    private float timer;
    private float maxTimer;

	// 폭발 반경 관련
	[SerializeField] GameObject RangeGridPrefab;
	private List<GameObject> ExplodeAreas;
	private Stack<Vector3> CandidatePosStack;
	private int wide;
	private float height;

	// 폭발 위력 관련
	[SerializeField] GameObject ExplosionEffectPrefab;
	private float damage;
	


	private void OnEnable()
    {
		ExplodeAreas = new List<GameObject>();
		CandidatePosStack = new Stack<Vector3>();
		wide = 0;
		timer = 3f;
        maxTimer = 10f;
        damage = 3f;
		height = 0.01f;
		Init();
        StartCoroutine(CountDown());
	}

    private void Init()
    {
		wide = 3;

		// 기본 폭발 범위 표시
		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				GameObject go = Instantiate(RangeGridPrefab);
				go.transform.position = new Vector3(transform.position.x + x, height, transform.position.z + z);
				ExplodeAreas.Add(go);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("Critter"))
		{
			Debug.Log("강화");
			AddRange();
		}
	}

	public void AddRange()
	{
		// 새로운 폭발 라인으로의 갱신이 필요
		if (ExplodeAreas.Count >= wide * wide)
		{
			List<Vector3> CandidatePosList = new List<Vector3>();

			wide += 2;
			int r = wide / 2;
			Vector3 pos = transform.position;

			// 맨 첫열과 마지막 열을 후보목록에 삽입
			for (int z = -r; z <= r; z++)
			{
				CandidatePosList.Add(new Vector3(pos.x - r, height, pos.z + z));
				CandidatePosList.Add(new Vector3(pos.x + r, height, pos.z + z));
			}
			// 맨 첫행과 마지막 행을 후보목록에 삽입
			for (int x = -r + 1; x < r; x++)
			{
				CandidatePosList.Add(new Vector3(pos.x + x, height, pos.z - r));
				CandidatePosList.Add(new Vector3(pos.x + x, height, pos.z + r));
			}

			// 셔플

			for (int i = CandidatePosList.Count - 1; i > 0; i--)
			{
				int j = Random.Range(0, i + 1);
				(CandidatePosList[i], CandidatePosList[j]) = (CandidatePosList[j], CandidatePosList[i]);
			}

			// 스택에 삽입
			foreach (Vector3 v in CandidatePosList)
			{
				CandidatePosStack.Push(v);
			}
		}

		Vector3 newPos = CandidatePosStack.Pop();
		GameObject go = Instantiate(RangeGridPrefab);
		go.transform.position = newPos;
		ExplodeAreas.Add(go);
		maxTimer += 0.3f;
		timer += 0.3f;
		damage += 1f;
	}

    private IEnumerator CountDown()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            TimerUI.fillAmount = timer / maxTimer;
            yield return null;
        }

        Explode();
    }

    private void Explode()
    {
		foreach (GameObject area in ExplodeAreas)
		{
			GameObject go = Instantiate(ExplosionEffectPrefab);
			Destroy(area);
			go.transform.position = new Vector3(area.transform.position.x, height, area.transform.position.z);
			Destroy(go, 1f);
		}

		Destroy(gameObject);
	}
}
