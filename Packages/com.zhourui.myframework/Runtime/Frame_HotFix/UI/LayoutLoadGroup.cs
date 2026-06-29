using System;
using System.Collections.Generic;
using static FrameUtility;
using static MathUtility;
using static LT;

// 用于批量异步加载布局,封装一些通用的逻辑,需要通过LayoutLoadGroup.create来创建,会自动回收
public class LayoutLoadGroup : ClassObject
{
	protected Dictionary<Type, LayoutLoadInfo> mLoadInfo = new();	// 要加载的布局列表
	protected Action mLoadedCallback;								// 所有布局加载完成时的回调
	protected GameLayoutCallback mLoadingCallback;                  // 单个布局加载完成时的回调
	protected int mLoadedCount;                                     // 加载完成数量
	protected bool mAutoDestroy;									// 加载完成后是否自动销毁
	public override void resetProperty()
	{
		base.resetProperty();
		mLoadInfo.Clear();
		mLoadedCallback = null;
		mLoadingCallback = null;
		mLoadedCount = 0;
		mAutoDestroy = false;
	}
	public override void destroy()
	{
		base.destroy();
		UN_CLASS_LIST(mLoadInfo);
	}
	public static LayoutLoadGroup create(bool autoDestroy = true)
	{
		var obj = CLASS<LayoutLoadGroup>();
		obj.mAutoDestroy = autoDestroy;
		return obj;
	}
	public CustomAsyncOperation startLoad(Action loadedCallback, GameLayoutCallback loadingCallback = null)
	{
		CustomAsyncOperation op = new();
		mLoadedCallback = loadedCallback;
		mLoadingCallback = loadingCallback;
		foreach (var item in mLoadInfo)
		{
			if (item.Value.mIsScene)
			{
				LOAD_SCENE_ASYNC_HIDE(item.Key, item.Value.mOrder, (layout) => { onLayoutLoaded(layout, op); });
			}
			else
			{
				LOAD_ASYNC_HIDE(item.Key, item.Value.mOrder, item.Value.mOrderType, (layout) => { onLayoutLoaded(layout, op); });
			}
		}
		return op;
	}
	public void addLayout(Type type, int order = 0, LAYOUT_ORDER orderType = LAYOUT_ORDER.AUTO)
	{
		LayoutLoadInfo info = mLoadInfo.addClass(type);
		info.mType = type;
		info.mOrder = order;
		info.mOrderType = orderType;
		info.mIsScene = false;
	}
	public void addSceneUI(Type type, int order = 0)
	{
		LayoutLoadInfo info = mLoadInfo.addClass(type);
		info.mType = type;
		info.mOrder = order;
		info.mOrderType = LAYOUT_ORDER.FIXED;
		info.mIsScene = true;
	}
	public void addSceneUI<T>(int order = 0) where T : LayoutScript
	{
		addSceneUI(typeof(T), order);
	}
	public void addTopLayout<T>(int order)
	{
		addLayout(typeof(T), order, LAYOUT_ORDER.ALWAYS_TOP);
	}
	public void addTopLayout<T>()
	{
		addLayout(typeof(T), 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO);
	}
	public void addLayout<T>() where T : LayoutScript
	{
		addLayout(typeof(T), 0, LAYOUT_ORDER.AUTO);
	}
	public void addLayout<T>(int order) where T : LayoutScript
	{
		addLayout(typeof(T), order, LAYOUT_ORDER.FIXED);
	}
	public void addLayout<T>(int order, LAYOUT_ORDER orderType) where T : LayoutScript
	{
		addLayout(typeof(T), order, orderType);
	}
	public float getProgress() { return divide(mLoadedCount, mLoadInfo.Count); }
	public bool isAllLoaded() { return mLoadedCount == mLoadInfo.Count; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLayoutLoaded(GameLayout layout, CustomAsyncOperation op)
	{
		if (!mLoadInfo.TryGetValue(layout.getType(), out LayoutLoadInfo info))
		{
			return;
		}
		info.mLayout = layout;
		++mLoadedCount;
		mLoadingCallback?.Invoke(layout);
		if (mLoadedCount < mLoadInfo.Count)
		{
			return;
		}
		delayCall(() =>
		{
			op.setFinish();
			Action temp = mLoadedCallback;
			if (mAutoDestroy)
			{
				UN_CLASS(this);
			}
			temp?.Invoke();
		});
	}
}