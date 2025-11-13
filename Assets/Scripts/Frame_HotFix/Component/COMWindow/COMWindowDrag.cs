using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;

// UI拖拽组件,用于实现UI拖拽相关功能
public class COMWindowDrag : ComponentDrag
{
	protected List<myUGUIObject> mDragHoverWindowList = new();	// 当前拖拽时正在悬停的窗口
	protected myUGUIObject mWindow;								// 当前组件所属窗口
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mWindow = mComponentOwner as myUGUIObject;
	}
	public override void initDrag(Vector2 dragDirection, float dragStartAngleRadian, bool centerAlignMouse, bool movable)
	{
		base.initDrag(dragDirection, dragStartAngleRadian, centerAlignMouse, movable);
		mWindow.registeCollider();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDragHoverWindowList.Clear();
		mWindow = null;
	}
	public override void setActive(bool active)
	{
		if (active == isActive())
		{
			return;
		}
		base.setActive(active);
		// 无论激活还是禁用,都需要将当前悬停的窗口清空
		mDragHoverWindowList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyScreenPosition(Vector3 screenPos)
	{
		mWindow.setPosition(screenPosToWindow(screenPos - mDragMouseOffset, mWindow.getParent()));
	}
	protected override Vector3 getScreenPosition()
	{
		Camera camera = mCameraManager.getUICamera().getCamera();
		if (camera != null)
		{
			return camera.WorldToScreenPoint(mWindow.getWorldPosition());
		}
		return Vector3.zero;
	}
	protected override bool mouseInObject(Vector3 touchPosition)
	{
		// 使用当前鼠标位置判断是否悬停,忽略被其他窗口覆盖的情况
		Collider collider = (mComponentOwner as myUGUIObject).getCollider();
		if (collider == null)
		{
			logError("not find collider, can not drag!");
			return false;
		}
		return collider.Raycast(getUIRay(touchPosition), out _, 10000.0f);
	}
	protected override void onDragEnd(Vector3 touchPos, bool cancel)
	{
		// 拖动结束时,首先通知悬停窗口取消悬停,因为在onDragEnd里面可能会将当前悬停窗口清空
		foreach (myUGUIObject item in mDragHoverWindowList)
		{
			item.onDragHovered(mWindow, touchPos, false);
		}
		mDragHoverWindowList.Clear();
		// 在接收逻辑之前通知基类拖拽结束,因为一般在接收拖拽时的逻辑会产生不可预知的结果
		base.onDragEnd(touchPos, cancel);
		// 判断当前鼠标所在位置是否有窗口
		using var a = new ListScope<IMouseEventCollect>(out var receiveWindow);
		mGlobalTouchSystem.getAllHoverObject(receiveWindow, touchPos, mWindow);
		foreach (var window in receiveWindow)
		{
			bool continueEvent = true;
			window.onReceiveDrag(mWindow, touchPos, ref continueEvent);
			if (!continueEvent)
			{
				break;
			}
		}
		// 拖拽操作完全结束
		notifyDragEndTotally(touchPos, cancel);
	}
	protected override void onDragging(Vector3 touchPos)
	{
		base.onDragging(touchPos);
		using var a = new ListScope<IMouseEventCollect>(out var receiveWindow);
		mGlobalTouchSystem.getAllHoverObject(receiveWindow, touchPos, mWindow);
		foreach (IMouseEventCollect item in receiveWindow)
		{
			if (item is not myUGUIObject curWindow)
			{
				continue;
			}
			// 当前悬停的窗口不在之前的悬停列表中,则是新悬停的窗口
			if (!mDragHoverWindowList.Contains(curWindow))
			{
				curWindow.onDragHovered(mWindow, touchPos, true);
			}
		}
		foreach (myUGUIObject item in mDragHoverWindowList)
		{
			// 之前悬停的窗口不在当前的悬停列表中,则是结束悬停的窗口
			if (!receiveWindow.Contains(item))
			{
				item.onDragHovered(mWindow, touchPos, false);
			}
		}
		mDragHoverWindowList.Clear();
		foreach (IMouseEventCollect item in receiveWindow)
		{
			mDragHoverWindowList.addNotNull(item as myUGUIObject);
		}
	}
}