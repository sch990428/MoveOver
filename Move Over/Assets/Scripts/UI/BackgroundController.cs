using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    private Image backgrounds;
    private int min = 76;
    private byte max = 85;

    private void Awake()
    {
        backgrounds = GetComponent<Image>();
        backgrounds.color = new Color32((byte)Random.Range(min, max), (byte)Random.Range(min, max), (byte)Random.Range(min, max), 255);
    }
}
