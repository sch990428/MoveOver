using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Vector3 dir;
    public float speed = 8f;

    void Update()
    {
        transform.position += speed * Time.deltaTime * dir;
    }
}
