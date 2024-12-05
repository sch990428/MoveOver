using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CritterController
{
	// 플레이어 행동상태 관련
	public enum PlayerState
	{
		Idle,
		Move,
		Attack,
	}

	public PlayerState state;

	// 플레이어 이동 관련
	private Vector3 moveDir = Vector3.zero;
	[SerializeField] private float moveDuration = 0.3f;
	[SerializeField] private float moveDistance = 1f;
	public int viewIndex;

	// 플레이어 콜라이더 관련
	private Collider playerCollider;
	private float bombY;

	// 플레이어 부하 관련
	public List<CritterController> Critters;

	// 플레이어 폭탄 관련
	public int maxBomb;
	public int currentBomb;
	public bool bombEnable = true;
	public float bombCooltime;

	// 코인 수집 관련 (추후 게임매니저로 이동)
	public int maxCoin = 20;
	public int currentCoin = 0;

	public GridMap currentMap;

	// 플레이어 이펙트 관련
	[SerializeField] private GameObject MeleeDamageEffect;

	// UI 관련
	[SerializeField] private GameUIController uiController;
	private CameraController mainCamera;

	private void Awake()
	{
		mainCamera = Camera.main.GetComponent<CameraController>();
		playerCollider = GetComponent<Collider>();
		bombY = playerCollider.bounds.max.y;
		maxBomb = 1;
		currentBomb = 0;
		bombEnable = true;
		bombCooltime = 1f;

		viewIndex = mainCamera.viewIndex;

		BombCountChange();
		CoinCountChange();
		CritterCountChange();
	}

	public void Init(Vector3 pos, GridMap map)
	{
		transform.position = pos;

		foreach (var c in Critters)
		{
			c.transform.position = pos;
			c.transform.rotation = transform.rotation;
			c.Init();
		}

		currentMap = map;
		isMovable = true;
	}

	private void Update()
	{
		// 테스트용 부하 생성
		if (Input.GetKeyDown(KeyCode.X))
		{
			AddCritter();
		}

		switch (state)
		{
			case PlayerState.Move:
				if (!isMoving && isMovable)
				{
					prePos = transform.position;
					Vector3 destPos = transform.position + moveDir * moveDistance;

					// 충돌 처리
					Debug.DrawRay(destPos - Vector3.down * 0.1f, Vector3.up, Color.red, 1f);

					Collider[] hits = Physics.OverlapBox(destPos, new Vector3(0.49f, 0.7f, 0.49f), Quaternion.identity, LayerMask.GetMask("Obstacle", "Explodable", "WallUp", "WallDown", "WallLeft", "WallRight"));
					bool isBlocked = false;
					if (hits.Length > 0) {
						foreach (Collider hit in hits)
						{
							if (hit.CompareTag("Bomb"))
							{
								hit.GetComponent<BombController>().ForcedExplode();
							}
							else
							{
								isBlocked = true;
							}
						}

						transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(destPos - prePos), 0.3f);
					}

					if (Critters.Count > 0 && validAngle + moveDir == Vector3.zero) { break; }
					if (isBlocked) { break; }
					isMoving = true;

					StartCoroutine(Move(destPos, moveDuration));
					// 부하들을 순차적으로 이동
					if (Critters.Count > 0)
					{
						Critters[0].MoveTo(prePos, moveDuration);
						for (int i = 1; i < Critters.Count; i++)
						{
							Critters[i].MoveTo(Critters[i - 1].prePos, moveDuration);
						}
					}
				}
				break;
		}
	}

	public void AddCritter()
	{
		GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.Critter);

		if (Critters.Count > 0)
		{
			go.transform.position = Critters[Critters.Count - 1].transform.position;
			go.transform.rotation = Critters[Critters.Count - 1].transform.rotation;
		}
		else
		{
			go.transform.position = transform.position;
			go.transform.rotation = transform.rotation;
		}

		CritterController c = go.GetComponent<CritterController>();
		c.player = this;
		c.Order = Critters.Count;
		Critters.Add(c);
		GameObject effect = ResourceManager.Instance.Instantiate("Prefabs/Effects/Critter Effect/Creation");
		effect.transform.position = c.transform.position + Vector3.up * 0.5f;
		ResourceManager.Instance.Destroy(effect, 1f);
		c.prePos = c.transform.position;
		c.Init();
		CritterCountChange();
	}

	private void OnMove(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();
		if (Mathf.Abs(input.x) > 0.5f && Mathf.Abs(input.y) > 0.5f)
		{
			// 대각선 입력 방지 규칙
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
			{
				input = new Vector2(input.x, 0); // 좌우 우선
			}
			else
			{
				input = new Vector2(0, input.y); // 상하 우선
			}
		}

		moveDir = new Vector3(input.x, 0, input.y);

		if (moveDir == Vector3.zero)
		{
			state = PlayerState.Idle;
		}
		else
		{
			state = PlayerState.Move;
		}

		viewIndex = mainCamera.viewIndex;
		moveDir = Quaternion.Euler(0, 90 * viewIndex, 0) * moveDir;
		moveDir.Normalize();
	}

	private void OnAttack()
	{
		if (currentBomb < maxBomb && bombEnable)
		{
			currentBomb++;
			bombEnable = false;
			SoundManager.Instance.PlaySound(SoundManager.GameSound.CreateBomb);
			GameObject go = PoolManager.Instance.Instantiate(Define.PoolableType.Bomb);
			Vector3 pos = transform.position;
			pos.y = bombY;
			go.GetComponent<BombController>().SetPosition(new Vector3(Mathf.RoundToInt(pos.x), pos.y, Mathf.RoundToInt(pos.z)));
			bomb = go.GetComponent<BombController>();
			bomb.Player = this;
			StartCoroutine(Spin());
			StartCoroutine(CreateBomb());
			BombCountChange();
		}
		else
		{
			// Debug.Log("불가");
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Tail"))
		{
			CritterController c = collision.transform.GetComponent<CritterController>();

			if (!c.isBirth && !c.isRetire)
			{
				GameObject go = Instantiate(MeleeDamageEffect);
				go.transform.position = transform.position;
				Destroy(go, 1f);

				int o = c.Order;
				Camera.main.GetComponent<CameraController>().OnShakeCameraByPosition(0.3f, 0.3f);
				Damage(o);

				Critters.RemoveRange(c.Order, Critters.Count - o);
				CritterCountChange();
			}
		}
		else if (collision.gameObject.CompareTag("Item"))
		{
			collision.transform.GetComponent<IBaseItem>().Collected();
		}
	}

	public void Damage(int hitPoint)
	{
		int retireAmount = 0;
		if (hitPoint < 0)
		{
			if (Critters.Count > 0)
			{
				foreach (CritterController c in Critters)
				{
					c.Retire();
				}

				retireAmount = Critters.Count;
				Critters.Clear();
			}
			else
			{
				Debug.Log("게임 오버");
			}
		}
		else if (hitPoint > Critters.Count)
		{
			return;
		}
		else
		{
			
			for (int i = hitPoint; i < Critters.Count; i++)
			{
				Critters[i].Retire();
				
			}

			retireAmount = Critters.Count - hitPoint;
			Critters.RemoveRange(hitPoint, Critters.Count - hitPoint);
		}

		CritterCountChange();
		Camera.main.GetComponent<CameraController>().OnShakeCameraByPosition(0.3f, 0.3f);
		SoundManager.Instance.PlaySound(SoundManager.GameSound.Damage);

		HashSet<Vector2Int> respawnPos = new HashSet<Vector2Int>();
		// Debug.Log($"{retireAmount}만큼 만들자");
		while (respawnPos.Count < retireAmount)
		{
			int x = Random.Range(currentMap.Min_X, currentMap.Max_X + 1);
			int z = Random.Range(currentMap.Min_Z, currentMap.Max_Z + 1);

			Vector2Int pos = new Vector2Int(x, z);

			if (currentMap.Grids[pos].IsWalkable)
			{
				respawnPos.Add(pos);
			}
		}

		foreach (Vector2Int pos in respawnPos)
		{
			GameObject go = ResourceManager.Instance.Instantiate("Prefabs/Items/Critter Item");
			go.GetComponent<BaseItem>().Player = this;
			go.transform.position = new Vector3(pos.x, 0.5f, pos.y);
		}
	}

	private IEnumerator CreateBomb()
	{
		yield return new WaitForSeconds(bombCooltime);
		bombEnable = true;
	}

	public void BombCountChange()
	{
		uiController.UpdateBomb(maxBomb - currentBomb, maxBomb);
	}

	public void CritterCountChange()
	{
		uiController.UpdateCritter(Critters.Count + 1);
	}

	public void CoinCountChange()
	{
		uiController.UpdateCoin(currentCoin, maxCoin);
	}
}