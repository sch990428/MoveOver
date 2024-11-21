using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private Image buttonImage;
	private Sprite normalImage;
	private Color32 normalColor;
	private bool isHover;

	[SerializeField]
	private Sprite hoverImage;

	[SerializeField]
	private Color32 hoverColor;

	[SerializeField]
	private Color32 clickColor;

	private void Awake()
    {
        buttonImage = GetComponent<Image>();
		normalImage = buttonImage.sprite;
		normalColor = buttonImage.color;
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
		buttonImage.sprite = hoverImage;
		buttonImage.color = hoverColor;
		isHover = true;
}

	public void OnPointerExit(PointerEventData eventData)
	{
		buttonImage.sprite = normalImage;
		buttonImage.color = normalColor;
		isHover = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		buttonImage.color = clickColor;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (isHover)
		{
			buttonImage.sprite = hoverImage;
			buttonImage.color = hoverColor;
		}
		else
		{
			buttonImage.sprite = normalImage;
			buttonImage.color = normalColor;
		}
		
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log($"버튼 클릭 : {transform.GetChild(0).GetComponent<TMP_Text>().text}");
	}
}
