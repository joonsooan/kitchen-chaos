using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnBeginDragHandler = null;
    public Action<PointerEventData> OnEndDragHandler = null;
    public Action<PointerEventData> OnDropHandler = null;

    private bool hovering;

    // 공통 호버 — 살짝 커짐
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        transform.DOScale(1.05f, 0.08f).SetUpdate(true).SetLink(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        transform.DOScale(1f, 0.08f).SetUpdate(true).SetLink(gameObject);
    }

    // 공통 눌림감 — BindEvent 붙은 모든 버튼이 눌릴 때 살짝 쪼그라듦
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(0.92f, 0.06f).SetUpdate(true).SetLink(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(hovering ? 1.05f : 1f, 0.08f).SetUpdate(true).SetLink(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickHandler != null)
            OnClickHandler.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (OnBeginDragHandler != null)
            OnBeginDragHandler.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (OnEndDragHandler != null)
            OnEndDragHandler.Invoke(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (OnDropHandler != null)
            OnDropHandler.Invoke(eventData);
    }
}
