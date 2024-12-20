using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : CritterController
{
	// 플레이어 행동상태 관련
	public enum PlayerState
	{
		Idle,
		Move,
		Attack,
		GameOver,
	}

	public PlayerState state;

	// 플레이어 이동 관련
	public Vector3 moveDir = Vector3.zero;
	[SerializeField] private float moveDuration = 0.3f;
	[SerializeField] private float stepTime = 0.15f;
	[SerializeField] private float moveDistance = 1f;
	private bool sidestep = false;
	private bool sidestepDisable = false;
	public int viewIndex;

	// 플레이어 콜라이더 관련
	private Collider playerCollider;
	private float bombY;

	// 플레이어 부하 관련
	public List<CritterController> Critters;
	public List<Carryable> Carrying;

	// 플레이어 폭탄 관련
	public int maxBomb;
	public int currentBomb;
	public bool bombEnable = true;
	public float bombCooltime;

	// 수집 관련 (추후 게임매니저로 이동)
	public int maxCoin = 10;
	public int currentCoin = 0;
	public int currentKey = 0;

	// 플레이어 체력 관련
	private float HP = 3f;
	private float MaxHP = 3f;
	private bool isDamaged = false;

	public GridMap currentMap;
	public bool isGameover = false;

	// 플레이어 이펙트 관련
	[SerializeField] private GameObject MeleeDamageEffect;

	// UI 관련
	public GameUIController uiController;
	private CameraController mainCamera;

	private void Start()
	{
		mainCamera = Camera.main.GetComponent<CameraController>();
		playerCollider = GetComponent<Collider>();
		bombY = 0f;
		maxBomb = 1;
		currentBomb = 0;
		bombEnable = true;
		bombCooltime = 1f;
		viewIndex = mainCamera.viewIndex;
		currentMap = GlobalSceneManager.Instance.GetCurrentMap();
		BombCountChange();
		CoinCountChange();
		CritterCountChange();
	}

	public void Init(Vector3 pos, GridMap map, int defaultCritter)
	{
		transform.position = pos;

		if (defaultCritter != 0)
		{
			Critters.Clear();
			foreach (var c in Critters)
			{
				PoolManager.Instance.Destroy(Define.PoolableType.Critter, c.gameObject);
			}
		}
		else
		{
			foreach (var c in Critters)
			{
				c.transform.position = pos;
				c.transform.rotation = transform.rotation;
				c.Init();
			}
		}

		currentMap = map;

		if (defaultCritter != 0)
		{
			Critters.Clear();
		}

		for (int i = 0; i < defaultCritter; i++)
		{
			AddCritter();
		}

		isMovable = true;
	}

	public void Heal()
	{
		HP = MaxHP;
		uiController.UpdateHpBar(HP, MaxHP);
	}

	private void Update()
	{
		if (isGameover)
		{
			return;
		}

		sidestep = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift);

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
							else if (hit.CompareTag("Carryable") && hit.GetComponent<Carryable>().isGrabbed)
							{
								if (!sidestep)
								{
									isBlocked = true;
								}
							}
							else
							{
								isBlocked = true;
							}
						}

						if (!sidestep) { transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(destPos - prePos), 0.3f); }
					}

					if (Critters.Count > 0 && validAngle + moveDir == Vector3.zero && !sidestep) { break; }
					if (isBlocked) { break; }
					isMoving = true;

					if (!sidestep)
					{
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
					else
					{
						if (sidestepDisable)
						{
							isMoving = false;
							break;
						}

						bool isSideBlocked = false;
						sidestepDisable = true;
						foreach (CritterController c in Critters)
						{
							if (c.IsBlocked(moveDir))
							{
								isSideBlocked = true;
							}
						}

						if (!isSideBlocked)
						{
							StartCoroutine(SideStep(moveDir, stepTime));
							foreach (CritterController c in Critters)
							{
								c.SideTo(moveDir, stepTime);
							}

							foreach (Carryable g in Carrying)
							{
								g.Carry(moveDir, stepTime);
							}

							StartCoroutine(SidestepDisable());
						}
						else
						{
							isMoving = false;
							sidestepDisable = false;
						}
					}
				}
				break;
		}
	}

	private IEnumerator SidestepDisable()
	{
		yield return new WaitForSeconds(0.7f);
		sidestepDisable = false;
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
		if (isGameover && !GlobalSceneManager.Instance.pause)
		{
			return;
		}
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
				if (o <= 0) { return; }
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
		else if (collision.gameObject.CompareTag("Bomb"))
		{
			BombController b = collision.transform.GetComponent<BombController>();
			if (b.makerExit)
			{
				b.ForcedExplode();
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.CompareTag("Bomb"))
		{
			collision.transform.GetComponent<BombController>().makerExit = true;
		}
	}

	public void Damage(int hitPoint, float damage = 1f)
	{
		int retireAmount = 0;
		if (hitPoint < 0)
		{
			if (!isDamaged)
			{
				isDamaged = true;
				if (Critters.Count > 0)
				{
					foreach (CritterController c in Critters)
					{
						c.Retire();
					}

					retireAmount = Critters.Count;
					Critters.Clear();
				}

				GameObject go = Instantiate(MeleeDamageEffect);
				go.transform.position = transform.position;
				Destroy(go, 1f);

				HP -= damage;
				if (HP <= 0)
				{
					uiController.GameOver();
					isGameover = true;
					HP = 0;
				}

				StartCoroutine(DamageCooltime());
				uiController.UpdateHpBar(HP, MaxHP);
				Camera.main.GetComponent<CameraController>().OnShakeCameraByPosition(0.3f, 0.3f);
				SoundManager.Instance.PlaySound(SoundManager.GameSound.Damage);

				GlobalSceneManager.Instance.DamageEffect();
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

			Camera.main.GetComponent<CameraController>().OnShakeCameraByPosition(0.3f, 0.3f);
			SoundManager.Instance.PlaySound(SoundManager.GameSound.Damage);
		}

		CritterCountChange();

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

		GlobalSceneManager.Instance.DamageEffect();
	}

	private IEnumerator CreateBomb()
	{
		yield return new WaitForSeconds(bombCooltime);
		bombEnable = true;
	}

	private IEnumerator DamageCooltime()
	{
		yield return new WaitForSeconds(0.5f);
		isDamaged = false;
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
		uiController.UpdateCoin(GlobalSceneManager.Instance.CollectedCoinList.Count, maxCoin);
	}
}