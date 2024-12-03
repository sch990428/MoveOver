using UnityEngine;

public class Explodable : MonoBehaviour
{
    public void Explode()
    {
        SoundManager.Instance.PlaySound(SoundManager.GameSound.Break);

        // 부모의 GridMap 컴포넌트에서 자신이 있던 그리드의 정보를 갱신
        Vector2Int posKey = new Vector2Int((int)transform.position.x, (int)transform.position.z);
		transform.parent.GetComponent<GridMap>().Grids[posKey].IsWalkable = true;
        Debug.Log($"{posKey}열림");

        Destroy(gameObject);
    }
}
