using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;
using static FrameUtility;

// UGUI窗口的基类
public class myUGUIObject : myUIObject
{
	protected static Comparison<Transform> mCompareDescend = compareZDecending;	// 避免GC的回调
	protected COMWindowUGUIInteractive mCOMWindowUGUIInteractive;				// UGUI的鼠标键盘响应逻辑的组件
	protected RectTransform mRectTransform;                                     // UGUI的Transform
	public override void init()
	{
		base.init();
		mRectTransform = mObject.GetComponent<RectTransform>();
		// 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
		if (mRectTransform != null)
		{
			mTransform = mRectTransform;
		}
		mCOMWindowCollider?.setColliderSize(mRectTransform);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCOMWindowUGUIInteractive = null;
		mRectTransform = null;
	}
	public override void onLayoutHide() 
	{
		// 布局隐藏时需要将触点清除
		mCOMWindowUGUIInteractive?.clearMousePointer();
	}
	// 将当前窗口的顶部对齐父节点的顶部
	public void alignParentTop()
	{
		if (!(mParent is myUGUIObject))
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setPositionY((mParent as myUGUIObject).getWindowTop() - getWindowSize().y * 0.5f);
	}
	// 将当前窗口的底部对齐父节点的底部
	public void alignParentBottom()
	{
		if (!(mParent is myUGUIObject))
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setPositionY((mParent as myUGUIObject).getWindowBottom() + getWindowSize().y * 0.5f);
	}
	// 将当前窗口的左边界对齐父节点的左边界
	public void alignParentLeft()
	{
		if (!(mParent is myUGUIObject))
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setPositionX((mParent as myUGUIObject).getWindowLeft() + getWindowSize().x * 0.5f);
	}
	// 将当前窗口的右边界对齐父节点的右边界
	public void alignParentRight()
	{
		if (!(mParent is myUGUIObject))
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setPositionX((mParent as myUGUIObject).getWindowRight() - getWindowSize().x * 0.5f);
	}
	// 获得窗口顶部在窗口中的相对于窗口pivot的Y坐标
	public float getWindowTop() { return getWindowSize().y * (1.0f - getPivot().y); }
	// 获得窗口底部在窗口中的相对于窗口pivot的Y坐标
	public float getWindowBottom() { return -getWindowSize().y * getPivot().y; }
	// 获得窗口左边界在窗口中的相对于窗口pivot的X坐标
	public float getWindowLeft() { return -getWindowSize().x * getPivot().x; }
	// 获得窗口右边界在窗口中的相对于窗口pivot的X坐标
	public float getWindowRight() { return getWindowSize().x * (1.0f - getPivot().x); }
	// 获取不考虑中心点偏移的坐标,也就是固定获取窗口中心的坐标
	// 由于pivot的影响,Transform.localPosition获得的坐标并不一定等于窗口中心的坐标
	public Vector3 getPositionNoPivot()
	{
		return WidgetUtility.getPositionNoPivot(mRectTransform);
	}
	public Vector2 getPivot() { return mRectTransform.pivot; }
	public void setPivot(Vector2 pivot) { mRectTransform.pivot = pivot; }
	public RectTransform getRectTransform() { return mRectTransform; }
	public override void setWindowSize(Vector2 size)
	{
		setRectSize(mRectTransform, size);
		ensureColliderSize();
	}
	public override Vector2 getWindowSize(bool transformed = false)
	{
		Vector2 windowSize = mRectTransform.rect.size;
		if (transformed)
		{
			windowSize = multiVector2(windowSize, getWorldScale());
		}
		return windowSize;
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		if (fadeChild)
		{
			setUGUIChildAlpha(mObject, alpha);
		}
	}
	public void refreshChildDepthByPositionZ()
	{
		// z值越大的子节点越靠后
		using (new ListScope<Transform>(out var tempList))
		{
			int childCount = getChildCount();
			for (int i = 0; i < childCount; ++i)
			{
				tempList.Add(mTransform.GetChild(i));
			}
			quickSort(tempList, mCompareDescend);
			int count = tempList.Count;
			for (int i = 0; i < count; ++i)
			{
				tempList[i].SetSiblingIndex(i);
			}
		}
	}
	public void setUGUIClick(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIClick(callback);
	}
	public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseDown(callback);
		// 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
		mReceiveLayoutHide = true;
	}
	public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseUp(callback);
		// 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
		mReceiveLayoutHide = true;
	}
	public void setUGUIMouseEnter(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseEnter(callback);
	}
	public void setUGUIMouseExit(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseExit(callback);
	}
	public void setUGUIMouseMove(Action<Vector2, Vector3> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseMove(callback);
		// 如果设置了要监听鼠标移动,则需要激活当前窗口
		mEnable = true;
	}
	public void setUGUIMouseStay(Action<Vector3> callback)
	{
		getCOMUGUIInteractive().setUGUIMouseStay(callback);
		mEnable = true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static int compareZDecending(Transform a, Transform b) { return (int)sign(b.localPosition.z - a.localPosition.z); }
	protected override void ensureColliderSize()
	{
		// 确保RectTransform和BoxCollider一样大
		mCOMWindowCollider?.setColliderSize(mRectTransform);
	}
	protected COMWindowUGUIInteractive getCOMUGUIInteractive()
	{
		if (mCOMWindowUGUIInteractive == null)
		{
			mCOMWindowUGUIInteractive = addComponent<COMWindowUGUIInteractive>(true);
		}
		return mCOMWindowUGUIInteractive;
	}
}