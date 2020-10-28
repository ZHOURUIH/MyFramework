using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WindowComponentDrag : ComponentDrag
{
	protected myUIObject mWindow;
	protected myUIObject mDragHoverWindow;
	protected OnDraging mOnDraging;
	protected bool mMovable;
	public WindowComponentDrag()
	{
		mMovable = true;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mWindow = mComponentOwner as myUIObject;
	}
	public void setDragingCallback(OnDraging callback) { mOnDraging = callback; }
	public override void setActive(bool active)
	{
		if(active == isActive())
		{
			return;
		}
		base.setActive(active);
		// 无论激活还是禁用,都需要将当前悬停的窗口清空
		mDragHoverWindow = null;
	}
	public void setMovable(bool movable) { mMovable = movable; }
	//--------------------------------------------------------------------------------------------------------------
	protected override void applyScreenPosition(ref Vector3 screenPos)
	{
		if(mMovable)
		{
			Vector3 pos = screenPosToWindowPos(screenPos - mDragMouseOffset, mWindow.getParent(), true, mWindow.getLayout().getGUIType());
			mWindow.setPosition(pos);
		}
	}
	protected override Vector3 getScreenPosition()
	{
		Camera camera = mCameraManager.getUICamera(mWindow.getLayout().getGUIType()).getCamera();
		if(camera != null)
		{
			return camera.WorldToScreenPoint(mWindow.getWorldPosition());
		}
		return Vector3.zero;
	}
	protected override bool mouseInObject(ref Vector3 mousePosition)
	{
		// 使用当前鼠标位置判断是否悬停,忽略被其他窗口覆盖的情况
		Collider collider = (mComponentOwner as myUIObject).getCollider();
		if (collider == null)
		{
			logError("not find collider, can not drag!");
			return false;
		}
		Ray ray;
		getUIRay(ref mousePosition, out ray, (mComponentOwner as myUIObject).getLayout().getGUIType());
		return collider.Raycast(ray, out _, 10000.0f);
	}
	protected override void onDragEnd(Vector3 mousePos)
	{
		// 拖动结束时,首先通知悬停窗口取消悬停,因为在onDragEnd里面可能会将当前悬停窗口清空
		mDragHoverWindow?.onDragHoverd(mWindow, false);
		mDragHoverWindow = null;
		// 在接收逻辑之前通知基类拖拽结束,因为一般在接收拖拽时的逻辑会产生不可预知的结果
		base.onDragEnd(mousePos);
		// 判断当前鼠标所在位置是否有窗口
		var receiveWindow = mGlobalTouchSystem.getAllHoverWindow(ref mousePos, mWindow);
		int count = receiveWindow.Count;
		for(int i = 0; i < count; ++i)
		{
			bool continueEvent = true;
			receiveWindow[i].onReceiveDrag(mWindow, ref continueEvent);
			if(!continueEvent)
			{
				break;
			}
		}
		// 拖拽操作完全结束
		notifyDragEndTotally();
	}
	protected override void onDraging(ref Vector3 mousePos)
	{
		base.onDraging(ref mousePos);
		IMouseEventCollect curHover = mGlobalTouchSystem.getHoverWindow(ref mousePos, mWindow);
		// 悬停的窗口改变了
		if (curHover != mDragHoverWindow)
		{
			mDragHoverWindow?.onDragHoverd(mWindow, false);
			mDragHoverWindow = curHover as myUIObject;
			mDragHoverWindow?.onDragHoverd(mWindow, true);
		}
		mOnDraging?.Invoke();
	}
	protected override void onInterrupt()
	{
		base.onInterrupt();
		mDragHoverWindow = null;
	}
}