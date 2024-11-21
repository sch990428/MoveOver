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
	private bool isSelected;

	[SerializeField]
	private Sprite hoverImage;

	[SerializeField]
	private Color32 hoverColor;

	[SerializeField]
	private Color32 clickColor;

	[SerializeField]
	private UIController uiController;

	[SerializeField]
	private int index;

	private void Awake()
    {
        buttonImage = GetComponent<Image>();
		normalImage = buttonImage.sprite;
		normalColor = buttonImage.color;
		uiController.onSelectedChange -= OnSelected;
		uiController.onSelectedChange += OnSelected;
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!isSelected)
		{
			buttonImage.sprite = hoverImage;
			buttonImage.color = hoverColor;
			isHover = true;
		}
}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!isSelected)
		{
			buttonImage.sprite = normalImage;
			buttonImage.color = normalColor;
			isHover = false;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!isSelected)
		{
			buttonImage.color = clickColor;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!isSelected)
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
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		uiController.SwitchDetail(index);
		//Debug.Log($"버튼 클릭 : {transform.GetChild(0).GetComponent<TMP_Text>().text}");
	}

	public void OnSelected(int i)
	{
		if (index == i)
		{
			buttonImage.color = clickColor;
			isSelected = true;
		}
		else
		{
			buttonImage.color = normalColor;
			isSelected = false;
		}
	}
}
