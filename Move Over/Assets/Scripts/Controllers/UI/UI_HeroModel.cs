using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HeroModel : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler, IPointerUpHandler
{
	[SerializeField] private Camera heroModelCamera;
	[SerializeField] private GameObject targetHero; // 타겟이 될 영웅 게임 오브젝트
	[SerializeField] private List<string> HeroTextList;
	[SerializeField] private TMP_Text TextBox;

	private RawImage rawImage;
	private int index = 0;

	private float rotateSpeed = 0.08f;
	private float zoomSpeed = 1.0f;

	private bool isDragging = false;

	private void Awake()
	{
		rawImage = GetComponent<RawImage>();
		TextBox.text = HeroTextList[0];
	}

	private void Update()
	{
		if (targetHero != null && !isDragging)
		{
			targetHero.transform.Rotate(0f, 100f * rotateSpeed * Time.deltaTime, 0f, Space.World);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		// 렌더 텍스쳐(Raw Image)가 클릭되면 클릭된 위치를 0~1 사이의 상대 비율로 계산합니다
		Vector2 localCursor;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rawImage.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor
		);

		Rect rect = rawImage.rectTransform.rect;
		float normalizedX = (localCursor.x - rect.x) / rect.width;
		float normalizedY = (localCursor.y - rect.y) / rect.height;

		// 계산된 상대 비율 좌표로 영웅모델을 찍는 카메라에서 레이캐스팅합니다
		RaycastHit hit;
		Ray ray = heroModelCamera.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0));

		Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);

		// 모델이 감지되면 특정 동작을 하게 합니다
		if (Physics.Raycast(ray, out hit))
		{
			index++;
			if (index >= HeroTextList.Count) { index = 1; }
			TextBox.text = HeroTextList[index];
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		// 렌더 텍스쳐(Raw Image)가 드래그되면 마우스포인터의 델타X값을 기준으로 모델을 회전합니다
		targetHero.transform.Rotate(0f, -eventData.delta.x * rotateSpeed, 0f, Space.World);
		isDragging = true;
	}

	public void OnScroll(PointerEventData eventData)
	{
		// 렌더 텍스쳐(Raw Image)가 스크롤되면 마우스스크롤의 델타y값을 기준으로 영웅 모델 카메라의 FieldOfView를 조정합니다
		heroModelCamera.fieldOfView += -eventData.scrollDelta.y * zoomSpeed;
		heroModelCamera.fieldOfView = Mathf.Clamp(heroModelCamera.fieldOfView, 30f, 60f);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isDragging = false;
	}
}