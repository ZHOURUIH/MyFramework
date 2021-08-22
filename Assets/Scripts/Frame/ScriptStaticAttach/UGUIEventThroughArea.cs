using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// 允许部分区域穿透UGUI的鼠标事件
public class UGUIEventThroughArea : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	public Rect mPassOnlyRect;
	public void setPassOnlyArea(RectTransform rectTransform)
	{
		Rect rect = new Rect();
		rect.min = UnityUtility.localToWorld(rectTransform, rectTransform.rect.min);
		rect.max = UnityUtility.localToWorld(rectTransform, rectTransform.rect.max);
		setPassOnlyArea(rect);
	}
	public void setPassOnlyArea(Rect rect) { mPassOnlyRect = rect; }
	// 监听按下
	public void OnPointerDown(PointerEventData eventData)
	{
		PassEvent(eventData, ExecuteEvents.pointerDownHandler);
	}
	// 监听抬起
	public void OnPointerUp(PointerEventData eventData)
	{
		PassEvent(eventData, ExecuteEvents.pointerUpHandler);
	}
	// 监听点击
	public void OnPointerClick(PointerEventData eventData)
	{
		PassEvent(eventData, ExecuteEvents.submitHandler);
		PassEvent(eventData, ExecuteEvents.pointerClickHandler);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 把事件透下去
	protected void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
	{
		// 触点是否在指定区域内,只允许指定区域内的的事件能穿透下去
		if (!mPassOnlyRect.Contains(data.position - UnityUtility.getScreenSize() * 0.5f))
		{
			return;
		}
		FrameUtility.LIST(out List<RaycastResult> results);
		UnityEngine.EventSystems.EventSystem.current.RaycastAll(data, results);
		GameObject current = data.pointerCurrentRaycast.gameObject;
		for (int i = 0; i < results.Count; ++i)
		{
			if (current == results[i].gameObject)
			{
				continue;
			}
			ExecuteEvents.Execute(results[i].gameObject, data, function);
		}
		FrameUtility.UN_LIST(results);
	}
}