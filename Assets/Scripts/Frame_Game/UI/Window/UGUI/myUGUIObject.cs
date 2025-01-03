using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;

// UGUI窗口的基类
public class myUGUIObject : myUIObject
{
	protected static Comparison<Transform> mCompareDescend = compareZDecending;	// 避免GC的回调
	protected COMWindowUGUIInteractive mCOMWindowUGUIInteractive;				// UGUI的鼠标键盘响应逻辑的组件
	protected RectTransform mRectTransform;                                     // UGUI的Transform
	public override void init()
	{
		base.init();
		// 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
		if (mObject.TryGetComponent(out mRectTransform))
		{
			mTransform = mRectTransform;
		}
		if (mRectTransform == null)
		{
			if (mTransform != null)
			{
				logError("Transform不是RectTransform,name:" + mTransform.name);
			}
			else
			{
				logError("RectTransform为空");
			}
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
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowTopInParent(uiObj.getWindowTop());
	}
	// 将当前窗口的顶部中心对齐父节点的顶部中心
	public void alignParentTopCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowTopInParent(uiObj.getWindowTop());
		setWindowInParentCenterX();
	}
	// 将当前窗口的底部对齐父节点的底部
	public void alignParentBottom()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowBottomInParent(uiObj.getWindowBottom());
	}
	// 将当前窗口的底部中心对齐父节点的底部中心
	public void alignParentBottomCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowBottomInParent(uiObj.getWindowBottom());
		setWindowInParentCenterX();
	}
	// 将当前窗口的左边界对齐父节点的左边界
	public void alignParentLeft()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowLeftInParent(uiObj.getWindowLeft());
	}
	// 将当前窗口的左边界中心对齐父节点的左边界中心
	public void alignParentLeftCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowLeftInParent(uiObj.getWindowLeft());
		setWindowInParentCenterY();
	}
	// 将当前窗口的右边界对齐父节点的右边界
	public void alignParentRight()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowRightInParent(uiObj.getWindowRight());
	}
	// 将当前窗口的右边界中心对齐父节点的右边界中心
	public void alignParentRightCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowRightInParent(uiObj.getWindowRight());
		setWindowInParentCenterY();
	}
	// 设置窗口在父节点中横向居中
	public void setWindowInParentCenterX() { setPositionX(0.0f); }
	// 设置窗口在父节点中纵向居中
	public void setWindowInParentCenterY() { setPositionY(0.0f); }
	// 设置窗口左边界在父节点中的X坐标
	public void setWindowLeftInParent(float leftInParent) { setPositionX(leftInParent - getWindowLeft()); }
	// 设置窗口右边界在父节点中的X坐标
	public void setWindowRightInParent(float rightInParent) { setPositionX(rightInParent - getWindowRight()); }
	// 设置窗口顶部在父节点中的Y坐标
	public void setWindowTopInParent(float topInParent) { setPositionY(topInParent - getWindowTop()); }
	// 设置窗口底部在父节点中的Y坐标
	public void setWindowBottomInParent(float bottomInParent) { setPositionY(bottomInParent - getWindowBottom()); }
	// 获得窗口左边界在父窗口中的X坐标
	public float getWindowLeftInParent() { return getPosition().x + getWindowLeft(); }
	// 获得窗口右边界在父窗口中的X坐标
	public float getWindowRightInParent() { return getPosition().x + getWindowRight(); }
	// 获得窗口顶部在父窗口中的Y坐标
	public float getWindowTopInParent() { return getPosition().y + getWindowTop(); }
	// 获得窗口底部在父窗口中的Y坐标
	public float getWindowBottomInParent() { return getPosition().y + getWindowBottom(); }
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
	public Vector3 getPositionNoPivot() { return WidgetUtility.getPositionNoPivot(mRectTransform); }
	public Vector2 getPivot() { return mRectTransform.pivot; }
	public void setPivot(Vector2 pivot) { mRectTransform.pivot = pivot; }
	public RectTransform getRectTransform() { return mRectTransform; }
	public void setWindowWidth(float width)
	{
		if (isFloatEqual(mRectTransform.rect.size.x, width))
		{
			return;
		}
		// 还是需要调用setWindowSize,需要触发一些虚函数的调用
		setWindowSize(replaceX(getWindowSize(), width));
	}
	public void setWindowHeight(float height)
	{
		if (isFloatEqual(mRectTransform.rect.size.y, height))
		{
			return;
		}
		setWindowSize(replaceY(getWindowSize(), height));
	}
	public override void setWindowSize(Vector2 size)
	{
		if (isVectorEqual(mRectTransform.rect.size, size))
		{
			return;
		}
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
		using var a = new ListScope<Transform>(out var tempList);
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
		mNeedUpdate = true;
	}
	public void setUGUIMouseStay(Action<Vector3> callback)
	{
		getCOMUGUIInteractive().setUGUIMouseStay(callback);
		mNeedUpdate = true;
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
		return mCOMWindowUGUIInteractive ??= addComponent<COMWindowUGUIInteractive>(true);
	}
}