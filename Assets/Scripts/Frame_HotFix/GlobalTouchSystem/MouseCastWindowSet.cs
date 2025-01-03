using System;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;

// 用于存储触点检测时的窗口
public class MouseCastWindowSet : ClassObject
{
	public static Comparison<MouseCastWindowSet> mComparisonDescend = cameraDepthDescend;	// 避免GC的委托
	public static Comparison<myUIObject> mUIDepthDescend = uiDepthDescend;					// 避免GC的委托
	protected HashSet<myUIObject> mWindowSet = new();										// 用于查找的窗口列表
	protected List<myUIObject> mAvailableList = new();                                      // 当前可见的窗口,即在窗口范围内并且未被CanvasGroup裁剪的窗口,会进行排序
	protected GameCamera mCamera;															// 渲染这些窗口的摄像机
	protected bool mListDirty;                                                              // mAvailableList中的数据是否已经失效,用于确保在一帧中只更新一次列表
	public override void resetProperty()
	{
		base.resetProperty();
		mWindowSet.Clear();
		mAvailableList.Clear();
		mCamera = null;
		mListDirty = false;
	}
	public void update()
	{
		mListDirty = true;
	}
	public void setCamera(GameCamera camera) { mCamera = camera; }
	public void addWindow(myUIObject window)
	{
		if (window.isDestroy())
		{
			logError("窗口已经被销毁,无法访问:" + window.getName());
			return;
		}
		if (!mWindowSet.Add(window))
		{
			return;
		}
		mListDirty = true;
	}
	public bool hasWindow(myUIObject window) { return mWindowSet.Contains(window); }
	public GameCamera getCamera() { return mCamera; }
	public List<myUIObject> getWindowOrderList()
	{
		if (mListDirty)
		{
			mAvailableList.Clear();
			foreach (myUIObject item in mWindowSet)
			{
				if (item.isActiveInHierarchy() && isWindowInScreen(item, mCamera))
				{
					mAvailableList.Add(item);
				}
			}
			quickSort(mAvailableList, mUIDepthDescend);
			mListDirty = false;
		}
		return mAvailableList;
	}
	public void notifyWindowActiveChanged()
	{
		mListDirty = true;
	}
	public bool removeWindow(myUIObject window)
	{
		if (!mWindowSet.Remove(window))
		{
			return false;
		}
		mListDirty = true;
		if (window.isDestroy())
		{
			logError("窗口已经被销毁,无法访问:" + window.getName());
			return false;
		}
		return true;
	}
	public bool isEmpty() { return mWindowSet.Count == 0; }
	//------------------------------------------------------------------------------------------------------------------------------
	// a小于b返回1, a等于b返回0, a大于b返回-1
	protected static int uiDepthDescend(myUIObject a, myUIObject b)
	{
		return UIDepth.compare(a.getDepth(), b.getDepth());
	}
	protected static int cameraDepthDescend(MouseCastWindowSet a, MouseCastWindowSet b)
	{
		return (int)sign(b.mCamera.getCameraDepth() - a.mCamera.getCameraDepth());
	}
}