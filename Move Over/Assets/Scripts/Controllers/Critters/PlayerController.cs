using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
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

	// 플레이어 이펙트 관련
	[SerializeField] private GameObject MeleeDamageEffect;

	// UI 관련
	[SerializeField] private GameUIController uiController;

	private void Awake()
	{
		playerCollider = GetComponent<Collider>();
		bombY = playerCollider.bounds.max.y;
		maxBomb = 1;
		currentBomb = 0;
		bombEnable = true;
		bombCooltime = 1f;

		BombCountChange();
		CoinCountChange();
		CritterCountChange();
	}

	private void Update()
	{
		// 테스트용 부하 생성
		if (Input.GetKeyDown(KeyCode.X))
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
			c.Order = Critters.Count;
			Critters.Add(c);
			c.prePos = c.transform.position;
			CritterCountChange();
		}

		switch (state)
		{
			case PlayerState.Move:
				if (!isMoving)
				{
					prePos = transform.position;
					Vector3 destPos = transform.position + moveDir * moveDistance;

					// 충돌 처리
					Debug.DrawRay(destPos - Vector3.down * 0.1f, Vector3.up, Color.red, 1f);

					Collider[] hits = Physics.OverlapBox(destPos, new Vector3(0.49f, 0.7f, 0.49f), Quaternion.identity, LayerMask.GetMask("Obstacle", "Explodable"));
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

					if (isBlocked)
					{ break; }
					isMoving = true;

					// 부하들을 순차적으로 이동
					if (Critters.Count > 0)
					{
						Critters[0].MoveTo(prePos, moveDuration);
						for (int i = 1; i < Critters.Count; i++)
						{
							Critters[i].MoveTo(Critters[i - 1].prePos, moveDuration);
						}
					}

					StartCoroutine(Move(destPos, moveDuration));
				}
				break;
		}
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
			Debug.Log("불가");
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
				for (int i = o; i < Critters.Count; i++)
				{
					Critters[i].Retire();
				}

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
		if (hitPoint < 0)
		{
			if (Critters.Count > 0)
			{
				foreach (CritterController c in Critters)
				{
					c.Retire();
				}

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

			Critters.RemoveRange(hitPoint, Critters.Count - hitPoint);
		}

		CritterCountChange();
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