using UnityEngine;
using UnityEngine.EventSystems;
using System;

public delegate void VoidDelegate(GameObject go);

public class EventTriggerListener : EventTrigger
{
	public Action<PointerEventData, GameObject> onClick;
	public Action<PointerEventData, GameObject> onDown;
	public Action<PointerEventData, GameObject> onEnter;
	public Action<PointerEventData, GameObject> onExit;
	public Action<PointerEventData, GameObject> onUp;
	public Action<AxisEventData, GameObject> onMove;
	public Action<BaseEventData, GameObject> onSelect;
	public Action<BaseEventData, GameObject> onUpdateSelect;
	public override void OnPointerClick(PointerEventData eventData) { onClick?.Invoke(eventData, gameObject); }
	public override void OnPointerDown(PointerEventData eventData) { onDown?.Invoke(eventData, gameObject); }
	public override void OnPointerEnter(PointerEventData eventData) { onEnter?.Invoke(eventData, gameObject); }
	public override void OnPointerExit(PointerEventData eventData) { onExit?.Invoke(eventData, gameObject); }
	public override void OnPointerUp(PointerEventData eventData) { onUp?.Invoke(eventData, gameObject); }
	public override void OnSelect(BaseEventData eventData) { onSelect?.Invoke(eventData, gameObject); }
	public override void OnUpdateSelected(BaseEventData eventData) { onUpdateSelect?.Invoke(eventData, gameObject); }
	public override void OnMove(AxisEventData eventData) { onMove?.Invoke(eventData, gameObject); }
}