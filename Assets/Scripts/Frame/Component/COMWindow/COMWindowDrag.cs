using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;

// UI拖拽组件,用于实现UI拖拽相关功能
public class COMWindowDrag : ComponentDrag
{
	protected myUIObject mDragHoverWindow;      // 当前拖拽时正在悬停的窗口
	protected myUIObject mWindow;               // 当前组件所属窗口
	protected OnDraging mOnDraging;             // 拖拽中的回调
	protected BOOL mContinueEvent;              // 为了避免GC,因为ILR的原因,委托中尽量避免使用ref,所以使用自定义的基础数据类型代替
	protected bool mMovable;                    // 拖拽时是否允许当前窗口跟随移动
	public COMWindowDrag()
	{
		mContinueEvent = new BOOL();
		mMovable = true;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mWindow = mComponentOwner as myUIObject;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDragHoverWindow = null;
		mWindow = null;
		mOnDraging = null;
		mMovable = true;
		mContinueEvent.set(false);
	}
	public void setDragingCallback(OnDraging callback) { mOnDraging = callback; }
	public override void setActive(bool active)
	{
		if (active == isActive())
		{
			return;
		}
		base.setActive(active);
		// 无论激活还是禁用,都需要将当前悬停的窗口清空
		mDragHoverWindow = null;
	}
	public void setMovable(bool movable) { mMovable = movable; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyScreenPosition(Vector3 screenPos)
	{
		if (mMovable)
		{
			mWindow.setPosition(screenPosToWindow(screenPos - mDragMouseOffset, mWindow.getParent()));
		}
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
	protected override bool mouseInObject(Vector3 mousePosition)
	{
		// 使用当前鼠标位置判断是否悬停,忽略被其他窗口覆盖的情况
		Collider collider = (mComponentOwner as myUIObject).getCollider();
		if (collider == null)
		{
			logError("not find collider, can not drag!");
			return false;
		}
		return collider.Raycast(getUIRay(mousePosition), out _, 10000.0f);
	}
	protected override void onDragEnd(Vector3 mousePos)
	{
		// 拖动结束时,首先通知悬停窗口取消悬停,因为在onDragEnd里面可能会将当前悬停窗口清空
		mDragHoverWindow?.onDragHoverd(mWindow, mousePos, false);
		mDragHoverWindow = null;
		// 在接收逻辑之前通知基类拖拽结束,因为一般在接收拖拽时的逻辑会产生不可预知的结果
		base.onDragEnd(mousePos);
		// 判断当前鼠标所在位置是否有窗口
		using (new ListScope<IMouseEventCollect>(out var receiveWindow))
		{
			mGlobalTouchSystem.getAllHoverWindow(receiveWindow, mousePos, mWindow);
			int count = receiveWindow.Count;
			for (int i = 0; i < count; ++i)
			{
				mContinueEvent.set(true);
				receiveWindow[i].onReceiveDrag(mWindow, mousePos, mContinueEvent);
				if (!mContinueEvent.mValue)
				{
					break;
				}
			}
		}
		// 拖拽操作完全结束
		notifyDragEndTotally();
	}
	protected override void onDraging(Vector3 mousePos)
	{
		base.onDraging(mousePos);
		IMouseEventCollect curHover = mGlobalTouchSystem.getHoverWindow(mousePos, mWindow);
		// 悬停的窗口改变了
		if (curHover != mDragHoverWindow)
		{
			mDragHoverWindow?.onDragHoverd(mWindow, mousePos, false);
			mDragHoverWindow = curHover as myUIObject;
			mDragHoverWindow?.onDragHoverd(mWindow, mousePos, true);
		}
		mOnDraging?.Invoke();
	}
}