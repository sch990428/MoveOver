using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class BombController : MonoBehaviour
{
	// ���� ����
	public PlayerController Player;
	private bool passCreator = false;


    // Ÿ�̸� ����
    [SerializeField] Image TimerUI;
    private float timer;
    private float maxTimer;

	// ���� �ݰ� ����
	[SerializeField] GameObject RangeGridPrefab;
	private List<GameObject> ExplodeAreas;
	private Stack<Vector3> CandidatePosStack;
	private int wide;
	private float height;

	// ���� ���� ����
	private float damage;

	// ����Ʈ ����
	[SerializeField] GameObject ExplosionEffectPrefab;
	[SerializeField] GameObject UpgradeEffectPrefab;

	private void OnEnable()
    {
		ExplodeAreas = new List<GameObject>();
		CandidatePosStack = new Stack<Vector3>();
		wide = 0;
		timer = 3f;
        maxTimer = 3f;
        damage = 3f;
		height = 0.01f;
		Init();
        StartCoroutine(CountDown());
	}

    private void Init()
    {
		wide = 3;

		// �⺻ ���� ���� ǥ��
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
		if (other.transform.CompareTag("Player"))
		{
			if (passCreator)
			{
				ForceExplode();
			}
		}
		else if (other.transform.CompareTag("Critter"))
		{
			AddRange();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform.CompareTag("Player"))
		{
			passCreator = true;
		}
	}

	private void ForceExplode()
	{
		timer = 0.0f;
	}

	public void AddRange()
	{
		// ���ο� ���� ���������� ������ �ʿ�
		if (ExplodeAreas.Count >= wide * wide)
		{
			List<Vector3> CandidatePosList = new List<Vector3>();

			wide += 2;
			int r = wide / 2;
			Vector3 pos = transform.position;

			// �� ù���� ������ ���� �ĺ���Ͽ� ����
			for (int z = -r; z <= r; z++)
			{
				CandidatePosList.Add(new Vector3(pos.x - r, height, pos.z + z));
				CandidatePosList.Add(new Vector3(pos.x + r, height, pos.z + z));
			}
			// �� ù��� ������ ���� �ĺ���Ͽ� ����
			for (int x = -r + 1; x < r; x++)
			{
				CandidatePosList.Add(new Vector3(pos.x + x, height, pos.z - r));
				CandidatePosList.Add(new Vector3(pos.x + x, height, pos.z + r));
			}

			// ����
			for (int i = CandidatePosList.Count - 1; i > 0; i--)
			{
				int j = Random.Range(0, i + 1);
				(CandidatePosList[i], CandidatePosList[j]) = (CandidatePosList[j], CandidatePosList[i]);
			}

			// ���ÿ� ����
			foreach (Vector3 v in CandidatePosList)
			{
				CandidatePosStack.Push(v);
			}
		}

		Vector3 newPos = CandidatePosStack.Pop();
		GameObject go = Instantiate(RangeGridPrefab);
		go.transform.position = newPos;
		ExplodeAreas.Add(go);

		GameObject effect = Instantiate(UpgradeEffectPrefab);
		effect.transform.position = transform.position + Vector3.up * 0.5f;

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

	// ���� ������ ����Ʈ�� ����ϰ� ������
    private void Explode()
    {
		int frontest = int.MaxValue;
		// ���� ����Ʈ ��� �� ���� �׸��� ����
		foreach (GameObject area in ExplodeAreas)
		{
			GameObject go = Instantiate(ExplosionEffectPrefab);
			Destroy(area);
			go.transform.position = new Vector3(area.transform.position.x, height, area.transform.position.z);

			// ���� Ȯ��
			Collider[] hits = Physics.OverlapBox(area.transform.position, new Vector3(0.5f, 0.7f, 0.5f), Quaternion.identity, LayerMask.GetMask("Critter", "Enemy", "Obstacle", "Bomb"));

			foreach (Collider hit in hits)
			{
				if (hit.CompareTag("Critter"))
				{
					CritterController critter = hit.GetComponent<CritterController>();
					if (!critter.isRetire)
					{
						frontest = Mathf.Min(critter.order, frontest);
					}
				}
				else if (hit.CompareTag("Player"))
				{
					frontest = -1;
				}
				else if (hit.CompareTag("Obstacle"))
				{
					IBlock block = hit.GetComponent<IBlock>();
					if (block != null)
					{
						switch (block.BlockType)
						{
							case Define.BlockType.Explodable:
								hit.GetComponent<IExplodable>().Explode();
								break;
						}
					}
				}
			}
		}

		if (frontest < int.MaxValue) { Player.Damage(frontest); }

		// ī�޶� ��鸲
		CameraController cam = Camera.main.GetComponent<CameraController>();
		cam.OnShakeCamera();

		Destroy(gameObject);
	}
}
