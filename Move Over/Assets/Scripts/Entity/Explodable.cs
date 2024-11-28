using UnityEngine;

public class Explodable : MonoBehaviour
{
    public void Explode()
    {
        Destroy(gameObject);
    }
}
