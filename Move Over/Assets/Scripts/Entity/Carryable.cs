using System.Collections;
using UnityEngine;

public class Carryable : MonoBehaviour
{
    [SerializeField] GridMap stage;
    [SerializeField] PlayerController player;

	private float defaultY;
    public bool isGrabbed = false;
    public bool updateGrid = true;

	private void Awake()
	{
		defaultY = transform.position.y;
	}

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
		
		if (updateGrid) 
		{
        Vector2Int posKey = new Vector2Int((int)transform.position.x, (int)transform.position.z);
		stage.Grids[posKey].IsWalkable = true;
			// Debug.Log($"{posKey}열림");
		}
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

			float height = Mathf.Sin(t * Mathf.PI) * 0.3f + defaultY;

			transform.position = Vector3.Lerp(new Vector3(prePos.x, height, prePos.z), new Vector3(destPos.x, height, destPos.z), t);
			yield return null;
		}

		if (updateGrid)
		{
			Vector2Int posKey = new Vector2Int((int)transform.position.x, (int)transform.position.z);
			stage.Grids[posKey].IsWalkable = false;
		}

		transform.position = destPos;
	}
}
