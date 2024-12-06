using System.Collections;
using UnityEngine;

public class Carryable : MonoBehaviour
{
    [SerializeField] GridMap stage;
    [SerializeField] Transform defaultParent;
    [SerializeField] PlayerController player;
    public bool isGrabbed = false;

	private void Update()
	{
		Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one, Quaternion.identity, LayerMask.GetMask("Critter"));
        if (hits.Length >= 8)
        {
            if (!isGrabbed)
            {
                isGrabbed = true;
                player = GlobalSceneManager.Instance.GetPlayer();
				player.Carrying.Add(this);
            }
		}
        else
        {
            if (isGrabbed)
            {
				isGrabbed = false;
				player.Carrying.Remove(this);
			}
		}
	}

	public void Carry(Vector3 dir, float t)
    {
        SoundManager.Instance.PlaySound(SoundManager.GameSound.Break);

        Vector2Int posKey = new Vector2Int((int)transform.position.x, (int)transform.position.z);
		stage.Grids[posKey].IsWalkable = true;
        Debug.Log($"{posKey}열림");
        StartCoroutine(Carrying(dir, t));
    }

    private IEnumerator Carrying(Vector3 dir, float carryDuration)
    {
		Vector3 prePos = transform.position;
		Vector3 destPos = transform.position + dir;
		destPos.y = 0f;

		float elapsedTime = 0f;
		while (elapsedTime < carryDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / carryDuration;

			float height = Mathf.Sin(t * Mathf.PI) * 0.3f;

			transform.position = Vector3.Lerp(new Vector3(prePos.x, height, prePos.z), new Vector3(destPos.x, height + 0.5f, destPos.z), t);
			yield return null;
		}

		Vector2Int posKey = new Vector2Int((int)transform.position.x, (int)transform.position.z);
		stage.Grids[posKey].IsWalkable = false;
		transform.position = destPos;
	}
}
