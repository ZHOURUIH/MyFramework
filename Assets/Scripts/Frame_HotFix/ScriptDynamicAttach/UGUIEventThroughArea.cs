using UnityEngine;
using UnityEngine.EventSystems;
using static UnityUtility;

// 允许部分区域穿透UGUI的鼠标事件
public class UGUIEventThroughArea : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	public Rect mPassOnlyRect;      // 可穿透事件的区域
	protected bool mPassing;		// 当前是否正在穿透事件
	public void setPassOnlyArea(RectTransform rectTransform)
	{
		Rect rect = new();
		// 转换为世界坐标,也就是以屏幕中心为原点的坐标,是在父节点的空间下转换
		if (rectTransform.parent != null)
		{
			rect.min = localToWorld(rectTransform.parent, rectTransform.rect.min);
			rect.max = localToWorld(rectTransform.parent, rectTransform.rect.max);
		}
		else
		{
			rect.min = rectTransform.rect.min;
			rect.max = rectTransform.rect.max;
		}
		setPassOnlyArea(rect);
	}
	public void setPassOnlyArea(Rect rect) { mPassOnlyRect = rect; }
	// 监听按下
	public void OnPointerDown(PointerEventData eventData)
	{
		passEvent(eventData, ExecuteEvents.pointerDownHandler);
	}
	// 监听抬起
	public void OnPointerUp(PointerEventData eventData)
	{
		passEvent(eventData, ExecuteEvents.pointerUpHandler);
	}
	// 监听点击
	public void OnPointerClick(PointerEventData eventData)
	{
		passEvent(eventData, ExecuteEvents.submitHandler);
		passEvent(eventData, ExecuteEvents.pointerClickHandler);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 把事件透下去
	protected void passEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
	{
		// 正在穿透时不能再次穿透,否则会出现无限递归的错误
		if (mPassing)
		{
			return;
		}
		// 触点是否在指定区域内,只允许指定区域内的的事件能穿透下去
		// data.position是以左下角为原点的鼠标坐标,需要转换为以屏幕中心为原点的坐标
		if (!mPassOnlyRect.Contains(data.position - getHalfScreenSize()))
		{
			return;
		}
		mPassing = true;
		using var a = new ListScope<RaycastResult>(out var results);
		UnityEngine.EventSystems.EventSystem.current.RaycastAll(data, results);
		GameObject current = data.pointerCurrentRaycast.gameObject;
		foreach (RaycastResult item in results)
		{
			if (current == item.gameObject)
			{
				continue;
			}
			ExecuteEvents.Execute(item.gameObject, data, function);
		}
		mPassing = false;
	}
}