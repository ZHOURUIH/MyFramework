using UnityEngine;
using UnityEngine.EventSystems;
using System;

// 用于监听UGUI鼠标事件的脚本
public class EventTriggerListener : EventTrigger
{
	public Action<PointerEventData, GameObject> mOnClick;		// 点击回调
	public Action<PointerEventData, GameObject> mOnDown;		// 鼠标按下回调
	public Action<PointerEventData, GameObject> mOnEnter;		// 鼠标进入回调
	public Action<PointerEventData, GameObject> mOnExit;		// 鼠标离开回调
	public Action<PointerEventData, GameObject> mOnUp;			// 鼠标抬起回调
	public Action<AxisEventData, GameObject> mOnMove;			// 鼠标移动回调
	public Action<BaseEventData, GameObject> mOnSelect;			// 选择回调
	public Action<BaseEventData, GameObject> mOnUpdateSelect;	// 更新选择回调
	public override void OnPointerClick(PointerEventData eventData) { mOnClick?.Invoke(eventData, gameObject); }
	public override void OnPointerDown(PointerEventData eventData) { mOnDown?.Invoke(eventData, gameObject); }
	public override void OnPointerEnter(PointerEventData eventData) { mOnEnter?.Invoke(eventData, gameObject); }
	public override void OnPointerExit(PointerEventData eventData) { mOnExit?.Invoke(eventData, gameObject); }
	public override void OnPointerUp(PointerEventData eventData) { mOnUp?.Invoke(eventData, gameObject); }
	public override void OnSelect(BaseEventData eventData) { mOnSelect?.Invoke(eventData, gameObject); }
	public override void OnUpdateSelected(BaseEventData eventData) { mOnUpdateSelect?.Invoke(eventData, gameObject); }
	public override void OnMove(AxisEventData eventData) { mOnMove?.Invoke(eventData, gameObject); }
}