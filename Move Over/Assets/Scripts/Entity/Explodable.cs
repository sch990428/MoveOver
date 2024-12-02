using UnityEngine;

public class Explodable : MonoBehaviour
{
    public void Explode()
    {
        SoundManager.Instance.PlaySound(SoundManager.GameSound.Break);
        Destroy(gameObject);
    }
}
