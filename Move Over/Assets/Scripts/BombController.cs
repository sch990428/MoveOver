using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
	[SerializeField] GameObject RangeGridPrefab;
	[SerializeField] GameObject ExplodePrefab;
	[SerializeField] private Image barImage;

	public List<GameObject> ExplodeAreas;
	
	public Stack<Vector3> CandidatePosStack;

	public int Wide;

	public float Timer;
	public float MaxTimer;

	private void Awake()
	{
		CandidatePosStack = new Stack<Vector3>();
		Timer = 3f;
		MaxTimer = 3f;
	}

	public void SetPosition(Vector3 pos)
	{
		Wide = 3;
		transform.position = pos;

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				GameObject go = Instantiate(RangeGridPrefab);
				go.transform.position = new Vector3(pos.x + x, 0.01f, pos.z + z);
				ExplodeAreas.Add(go);
			}
		}
	}

	public void AddRange()
	{
		// 새로운 폭발 라인으로의 갱신이 필요
		if (ExplodeAreas.Count >= Wide * Wide)
		{
			List<Vector3> CandidatePosList = new List<Vector3>();
			
			Wide += 2;
			int r = Wide / 2;
			Vector3 pos = transform.position;

			// 맨 첫열과 마지막 열을 후보목록에 삽입
			for (int z = -r; z <= r; z++) { 
				CandidatePosList.Add(new Vector3(pos.x - r, 0.01f, pos.z + z)); 
				CandidatePosList.Add(new Vector3(pos.x + r, 0.01f, pos.z + z)); 
			}
			// 맨 첫행과 마지막 행을 후보목록에 삽입
			for (int x = -r + 1; x < r; x++) {
				CandidatePosList.Add(new Vector3(pos.x + x, 0.01f, pos.z - r));
				CandidatePosList.Add(new Vector3(pos.x + x, 0.01f, pos.z + r)); 
			}

			// 필요시 셔플
			
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

			Debug.Log($"범위 갱신{pos} / {CandidatePosStack.Count}");
		}

		Vector3 newPos = CandidatePosStack.Pop();
		GameObject go = Instantiate(RangeGridPrefab);
		go.transform.position = newPos;
		ExplodeAreas.Add(go);
		MaxTimer += 0.3f;
		Timer += 0.3f;
	}

	void Update()
    {
		barImage.fillAmount = Timer / MaxTimer;

		if (Timer > 0)
		{
			Timer -= Time.deltaTime;
		}
		else
		{
			foreach(GameObject area in ExplodeAreas)
			{
				GameObject go = Instantiate(ExplodePrefab);
				Camera.main.GetComponent<CameraController>().OnShakeCameraByPosition();
				go.transform.position = area.transform.position;
				Destroy(area);
				Destroy(go, 1f);
			}
			SoundManager.Instance.PlaySound(SoundManager.GameSound.Explode);
			Destroy(gameObject);
		}

    }
}
