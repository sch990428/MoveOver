using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
	[SerializeField] GameObject RangeGridPrefab;
	[SerializeField] GameObject ExplodePrefab;
	[SerializeField] GameObject EnhencePrefab;

	public PlayerController Player;

	[SerializeField] private Image barImage;

	public List<GameObject> ExplodeAreas;
	
	public Stack<Vector3> CandidatePosStack;

	public int Wide;
	public float Damage;
	public bool makerExit = false;

	public float Timer = 0f;
	public float MaxTimer;

	private void OnEnable()
	{
		ExplodeAreas = new List<GameObject>();
		CandidatePosStack = new Stack<Vector3>();
		Timer = 0f;
		makerExit = false;
		MaxTimer = 3f;
		Damage = 3f;
	}

	public void SetPosition(Vector3 pos)
	{
		Wide = 3;
		transform.position = pos;

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.WarningGrid);
				go.transform.position = new Vector3(pos.x + x, 0.01f, pos.z + z);
				ExplodeAreas.Add(go);
			}
		}

		StartCoroutine(Explosion());
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

			// Debug.Log($"범위 갱신{pos} / {CandidatePosStack.Count}");
		}

		Vector3 newPos = CandidatePosStack.Pop();
		GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.WarningGrid);
		GameObject effect = Instantiate(EnhencePrefab);
		effect.transform.position = transform.position;
		Destroy(effect, 0.5f);
		go.transform.position = newPos;
		ExplodeAreas.Add(go);
		MaxTimer += 0.3f;
		Damage += 1f;
	}

	public void ForcedExplode()
	{
		Timer = MaxTimer - 0.01f;
	}

	public IEnumerator Explosion()
	{
		while (Timer < MaxTimer)
		{
			barImage.fillAmount = Timer / MaxTimer;
			Timer += Time.deltaTime;
			yield return null;
		}

		int minIndex = int.MaxValue;
		Physics.SyncTransforms();
		float n = Wide / 15f;
		// Debug.Log(n);
		Camera.main.GetComponent<CameraController>().OnShakeCameraByPosition(n, n);
		foreach (GameObject area in ExplodeAreas)
		{
			GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.ExplodeEffect);
			go.transform.position = area.transform.position;
			PoolManager.Instance.Destroy(Define.PoolableType.WarningGrid, area);

			Collider[] hits = Physics.OverlapBox(area.transform.position, new Vector3(0.49f, 0.7f, 0.49f), Quaternion.identity, LayerMask.GetMask("Critter", "Enemy", "Obstacle"));
			foreach (Collider hit in hits)
			{
				if (hit.CompareTag("Tail"))
				{
					CritterController c = hit.GetComponent<CritterController>();
					if (!c.isRetire)
					{
						minIndex = Mathf.Min(c.Order, minIndex);
					}
				}
				else if (hit.CompareTag("Player"))
				{
					minIndex = -1;
				}
				else if (hit.CompareTag("Explodable"))
				{
					Explodable e = hit.GetComponent<Explodable>();
					e.Explode();
				}
				else if (hit.CompareTag("Monster"))
				{
					BaseMob b = hit.GetComponent<BaseMob>();
					b.GetDamage(Damage);
				}
			}

			PoolManager.Instance.Destroy(Define.PoolableType.ExplodeEffect, go, 1f);
		}

		if (minIndex < int.MaxValue) { Player.Damage(minIndex); }

		SoundManager.Instance.PlaySound(SoundManager.GameSound.Explode);
		Player.currentBomb--;
		Player.BombCountChange();
		PoolManager.Instance.Destroy(Define.PoolableType.Bomb, gameObject);
	}
}
