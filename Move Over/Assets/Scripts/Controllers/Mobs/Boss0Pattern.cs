using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Embree;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class Boss0PatternMob : MonoBehaviour
{
	[SerializeField] private float detectRange;
	[SerializeField] private float meleeRange;

	[SerializeField] private bool isCombat = false;
	[SerializeField] private GameObject HealthUI;
	[SerializeField] private Image HPBar;

	private bool isDamaged;
	public Rigidbody _rigidBody;
	public Collider _collider;

	public float MaxHP = 10f; // 최대 체력
	public float HP; // 체력

	Coroutine moveCoroutine;

	public bool isRetire = false;
	public Transform player;
	private bool isShootable = true;
	private bool isJumpable = true;
	public float moveDuration = 1f;
	public float reloadDuration = 5f;
	public float jumpDuration = 10f;

	private void OnEnable()
	{
		HealthUI.SetActive(true);
		_collider = GetComponent<Collider>();
		_rigidBody = GetComponent<Rigidbody>();
		StartCoroutine(ReadyForCombat(10f));
	}

	private void Update()
	{
		if (moveCoroutine == null && Vector3.Distance(transform.position, player.position) <= detectRange && transform.position.y < 1.5f && isCombat)
		{
			// 이동 시작
			moveCoroutine = StartCoroutine(Move());
		}
	}

	private IEnumerator Move()
	{
		while (true)
		{
			if (isRetire)
			{
				yield break;
			}

			if (Vector3.Distance(transform.position, player.position) > meleeRange)
			{
				if (isShootable)
				{
					isShootable = false;
					PlayerController playerController = player.GetComponent<PlayerController>();

					List<RedApple> Apples = new List<RedApple>();
					for (int i = 0; i < 3; i++)
					{
						Apples.Add(ResourceManager.Instance.Instantiate("Prefabs/Mobs/Projectiles/RedApple").GetComponent<RedApple>());
						Apples[i].transform.position = transform.position + Vector3.up * 2 * (i + 1);
						Apples[i].player = playerController;
					}

					Apples[0].targetPos = player.transform.position; // 첫번째 사과는 무조건 플레이어의 위치
					Apples[0].Rad = 2; // 반지름 2 (총 5x5)

					// 두번째 사과는 플레이어가 움직이는 방향의 4칸 앞
					Apples[1].targetPos = playerController.transform.position + playerController.moveDir * 4;
					if (playerController.moveDir == Vector3.zero) // 움직이지 않는 경우 패널티로 반지름 3 (총 7x7)
					{
						Apples[1].Rad = 3;
					}
					else // 움직이고 있는 경우 반지름 2 (총 5x5)
					{
						Apples[1].Rad = 2;
					}

					// 세번째 사과는 플레이어 부하들의 중간 위치에 반지름 2
					if (playerController.Critters.Count / 2 > 0)
					{
						Apples[2].targetPos = playerController.Critters[playerController.Critters.Count / 2].transform.position;
						Apples[2].Rad = 2;
					}
					else // 부하가 없거나 둘이면 4방향 중 랜덤 위치에 반지름 3
					{
						List<Vector3> offset = new List<Vector3>() { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
						int index = Random.Range(0, 4);
						Apples[2].targetPos = playerController.transform.position + offset[index] * 4;
						Apples[2].Rad = 3;
					}

					StartCoroutine(Reload());
				}
			}
			else
			{
				if (isJumpable)
				{
					isJumpable = false;
					RedApple apple = ResourceManager.Instance.Instantiate("Prefabs/Mobs/Projectiles/RedApple").GetComponent<RedApple>();
					apple.player = player.GetComponent<PlayerController>();
					apple.targetPos = transform.position;
					apple.Rad = 2;
					StartCoroutine(PrepareJump());
				}
			}

			yield return null;
		}
	}

	private IEnumerator ReadyForCombat(float t)
	{
		yield return new WaitForSeconds(t);
		isCombat = true;
	}

	private IEnumerator PrepareJump()
	{
		yield return new WaitForSeconds(jumpDuration);
		isJumpable = true;
	}

	private IEnumerator Reload()
	{
		yield return new WaitForSeconds(reloadDuration);
		isShootable = true;
	}

	public void GetDamage(float damage)
	{
		if (!isDamaged)
		{
			isDamaged = true;
			HP -= damage;
			if (HP <= 0)
			{
				HP = 0;
				Retire();
				return;
			}
			HPBar.fillAmount = HP / MaxHP;
			StartCoroutine(DamageTerm());
		}
	}

	private IEnumerator DamageTerm()
	{
		yield return new WaitForSeconds(0.3f);
		isDamaged = false;
	}

	public void Retire()
	{
		if (!isRetire)
		{
			isRetire = true;
			HealthUI.SetActive(false);
			StopCoroutine(moveCoroutine);
			moveCoroutine = null;

			_rigidBody.constraints = RigidbodyConstraints.None;
			_collider.enabled = false;

			Vector3 randomDirection = new Vector3(Random.Range(0.5f, 1f), 0f, Random.Range(0.5f, 1f));

			int r = Random.Range(0, 2);
			if (r == 0)
			{
				randomDirection = -randomDirection;
			}

			randomDirection = randomDirection.normalized;
			randomDirection.y = 1f;

			_rigidBody.AddForce(randomDirection * 20, ForceMode.Impulse);
			_rigidBody.AddTorque(randomDirection * 5, ForceMode.Impulse);

			ResourceManager.Instance.Destroy(gameObject, 1f);
		}
	}
}
