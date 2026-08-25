using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;

// 用于存储触点检测时的窗口
public class MouseCastWindowSet : ClassObject
{
	public static Comparison<MouseCastWindowSet> mComparisonDescend = cameraDepthDescend;	// 避免GC的委托
	public static Comparison<myUGUIObject> mUIDepthDescend = uiDepthDescend;				// 避免GC的委托
	protected HashSet<myUGUIObject> mWindowSet = new();		// 用于查找的窗口列表
	protected List<myUGUIObject> mAvailableList = new();    // 当前可见的窗口,即在窗口范围内并且未被CanvasGroup裁剪的窗口,会进行排序
	protected GameCamera mCamera;							// 渲染这些窗口的摄像机
	protected Matrix4x4 mLastWorldToCameraMatrix;           // 上一次摄像机的世界到相机矩阵,用于检测外部直接修改摄像机Transform
	protected Matrix4x4 mLastProjectionMatrix;              // 上一次摄像机投影矩阵
	protected Rect mLastPixelRect;                          // 上一次摄像机像素区域
	protected Vector2Int mLastScreenSize;                   // 上一次屏幕大小
	protected float mLastCameraDepth;                       // 上一次摄像机深度,深度变化会影响多摄像机MouseCast顺序
	protected bool mCameraStateValid;                       // 摄像机状态缓存是否有效
	protected bool mListDirty;                              // mAvailableList中的数据是否已经失效,只在真正发生变化时重建
	public override void resetProperty()
	{
		base.resetProperty();
		mWindowSet.Clear();
		mAvailableList.Clear();
		mCamera = null;
		mLastWorldToCameraMatrix = Matrix4x4.identity;
		mLastProjectionMatrix = Matrix4x4.identity;
		mLastPixelRect = default;
		mLastScreenSize = default;
		mLastCameraDepth = 0.0f;
		mCameraStateValid = false;
		mListDirty = false;
	}
	// 显式标记可用列表为脏,保留此入口用于特殊场景和性能基准,GlobalTouchSystem不再每帧调用
	public void update() { mListDirty = true; }
	public void notifyWindowChanged() { mListDirty = true; }
	public void setCamera(GameCamera camera)
	{
		if (mCamera == camera)
		{
			return;
		}
		mCamera = camera;
		mCameraStateValid = false;
		mListDirty = true;
	}
	// 添加一个窗口到集合中
	public bool addWindow(myUGUIObject window)
	{
		if (window.isDestroy())
		{
			logError("窗口已经被销毁,无法访问:" + window.getName());
			return false;
		}
		if (!mWindowSet.Add(window))
		{
			return false;
		}
		mListDirty = true;
		return true;
	}
	public bool hasWindow(myUGUIObject window) { return mWindowSet.Contains(window); }
	public GameCamera getCamera() { return mCamera; }
	// 获取按深度排序后的可见窗口列表
	public List<myUGUIObject> getWindowOrderList()
	{
		checkCameraStateChanged();
		if (mListDirty)
		{
			mAvailableList.Clear();
			foreach (myUGUIObject item in mWindowSet)
			{
				mAvailableList.addIf(item, item.isActiveInHierarchy() && isWindowInScreen(item, mCamera));
			}
			quickSort(mAvailableList, mUIDepthDescend);
			mListDirty = false;
		}
		return mAvailableList;
	}
	// 通知窗口激活状态变化,标记列表为脏
	public void notifyWindowActiveChanged() { mListDirty = true; }
	// 从集合中移除指定窗口
	public bool removeWindow(myUGUIObject window)
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
	public bool checkCameraStateChanged()
	{
		Camera camera = mCamera?.getCamera();
		if (camera == null)
		{
			bool changed = mCameraStateValid;
			mCameraStateValid = false;
			if (changed)
			{
				mListDirty = true;
			}
			return changed;
		}
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		Rect pixelRect = camera.pixelRect;
		Vector2Int screenSize = getScreenSize();
		float cameraDepth = camera.depth;
		if (mCameraStateValid &&
			mLastWorldToCameraMatrix == worldToCameraMatrix &&
			mLastProjectionMatrix == projectionMatrix &&
			mLastPixelRect == pixelRect &&
			mLastScreenSize == screenSize &&
			mLastCameraDepth == cameraDepth)
		{
			return false;
		}
		mLastWorldToCameraMatrix = worldToCameraMatrix;
		mLastProjectionMatrix = projectionMatrix;
		mLastPixelRect = pixelRect;
		mLastScreenSize = screenSize;
		mLastCameraDepth = cameraDepth;
		mCameraStateValid = true;
		mListDirty = true;
		return true;
	}
	// a小于b返回1, a等于b返回0, a大于b返回-1
	protected static int uiDepthDescend(myUGUIObject a, myUGUIObject b)
	{
		return UIDepth.compare(a.getDepth(), b.getDepth());
	}
	protected static int cameraDepthDescend(MouseCastWindowSet a, MouseCastWindowSet b)
	{
		return (int)sign(b.mCamera.getCameraDepth() - a.mCamera.getCameraDepth());
	}
}