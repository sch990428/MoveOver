using System.Collections;
using UnityEngine;

public class CritterController : MonoBehaviour
{
	[SerializeField] protected bool isMoving = false;
	public Vector3 prePos;
	public float height;
	public bool isSpinned = false;
	public bool isBirth = true;
	public bool isRetire = false;
	public PlayerController player;
	protected BombController bomb;
	public int Order;

	public Rigidbody _rigidBody;
	public Collider _collider;

	private void OnEnable()
	{
		_collider = GetComponent<Collider>();
		_rigidBody = GetComponent<Rigidbody>();

		_collider.enabled = true;
		_rigidBody.constraints = RigidbodyConstraints.FreezeAll;

		isSpinned = false;
		isBirth = true;
		isRetire = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isRetire) { return; }
		if (collision.transform.CompareTag("Bomb") && !isSpinned)
		{
			bomb = collision.transform.GetComponent<BombController>();
			bomb.AddRange();
			SoundManager.Instance.PlaySound(SoundManager.GameSound.EnhenceBomb);
			StartCoroutine(Spin());
		}
	}

	protected IEnumerator Spin()
	{
		isSpinned = true;
		float elapsedTime = 0f;
		float spinDuration = 0.4f; // 회전 지속 시간
		float jumpDuration = 0.15f; // 점프 지속 시간
		Vector3 rotationAxis = Vector3.up; // Y축 기준으로 회전

		int r = Random.Range(0, 2);
		int dir = 360;
		if (r == 0) { dir = -dir; }

		while (elapsedTime < spinDuration)
		{
			elapsedTime += Time.deltaTime;

			// 한 프레임 동안 회전할 각도 계산
			float angle = (dir / spinDuration) * Time.deltaTime;

			// 특정 축을 기준으로 회전
			transform.RotateAround(transform.position, rotationAxis, angle);

			yield return null;
		}

		isSpinned = false;

		elapsedTime = 0f;

		while (elapsedTime < jumpDuration)
		{
			if (isRetire)
			{
				yield break;
			}
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / jumpDuration;
			height = Mathf.Sin(t * Mathf.PI) * 0.3f;
			yield return null;
		}

		height = 0;
	}

	public void MoveTo(Vector3 destPos, float moveDuration)
	{
		if (isRetire) { return; }

		prePos = transform.position;
		StartCoroutine(Move(destPos, moveDuration));
	}

	protected IEnumerator Move(Vector3 destPos, float moveDuration)
	{
		Vector3 prePos = transform.position;
		Vector3 destDir = destPos - prePos;
		destDir.Normalize();
		Quaternion targetRotation = Quaternion.LookRotation(destDir);

		isMoving = true;
		float elapsedTime = 0f;
		while (elapsedTime < moveDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / moveDuration;
			
			if (!isSpinned)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
			}

			transform.position = Vector3.Lerp(new Vector3(prePos.x, height, prePos.z), new Vector3(destPos.x, height, destPos.z), t);

			yield return null;
		}

		transform.position = destPos;
		isMoving = false;
		if (isBirth) { isBirth = false; }
	}

	public void Retire()
	{
		if (!isRetire)
		{
			isRetire = true;

			_rigidBody.constraints = RigidbodyConstraints.None;
			_collider.enabled = false;

			Vector3 randomDirection = new Vector3(Random.Range(0.5f, 1f), 0f, Random.Range(0.5f, 1f));

			int r = Random.Range(0, 2);
			if (r == 0) { 
				randomDirection = -randomDirection; 
			}

			randomDirection = randomDirection.normalized;
			randomDirection.y = 1f;

			_rigidBody.AddForce(randomDirection * 40, ForceMode.Impulse);
			_rigidBody.AddTorque(randomDirection * 5, ForceMode.Impulse);

			PoolManager.Instance.Destroy(Define.PoolableType.Critter, gameObject, 1f);
		}
	}
}
