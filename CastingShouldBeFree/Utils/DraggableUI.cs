using UnityEngine;
using UnityEngine.EventSystems;

namespace CastingShouldBeFree.Utils;

public class DraggableUI : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector2 originalPanelLocalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalPanelLocalPosition = rectTransform.localPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out originalLocalPointerPosition);
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
        {
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            rectTransform.localPosition = (Vector3)originalPanelLocalPosition + offsetToOriginal;
        }
    }
}