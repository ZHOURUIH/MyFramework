using UnityEngine;
using System.Collections.Generic;
using System;

// 用于批量异步加载布局,封装一些通用的逻辑
public class LayoutLoadGroup : FrameBase
{
	protected Dictionary<int, LayoutLoadInfo> mLoadInfo;	// 要加载的布局列表
	protected LayoutAsyncDone mLoadCallback;				// 单个布局加载完成时的回调
	protected int mLoadedCount;								// 加载完成数量
	public LayoutLoadGroup()
	{
		mLoadInfo = new Dictionary<int, LayoutLoadInfo>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLoadInfo.Clear();
		mLoadCallback = null;
		mLoadedCount = 0;
	}
	public void startLoad()
	{
		foreach (var item in mLoadInfo)
		{
			LT.LOAD_UGUI_ASYNC(item.Key, item.Value.mOrder, item.Value.mOrderType, onLayoutLoaded);
		}
	}
	public void addLoadInfo(int id, int order, LAYOUT_ORDER orderType)
	{
		CLASS(out LayoutLoadInfo info);
		info.mID = id;
		info.mOrder = order;
		info.mOrderType = orderType;
		mLoadInfo.Add(id, info);
	}
	public void clear()
	{
		foreach(var item in mLoadInfo)
		{
			UN_CLASS(item.Value);
		}
		mLoadInfo.Clear();
	}
	public void setLoadCallback(LayoutAsyncDone callback) { mLoadCallback = callback; }
	public float getProgress() { return (float)mLoadedCount / mLoadInfo.Count; }
	public bool isAllLoaded() { return mLoadedCount == mLoadInfo.Count; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLayoutLoaded(GameLayout layout)
	{
		mLoadInfo[layout.getID()].mLayout = layout;
		++mLoadedCount;
		mLoadCallback?.Invoke(layout);
	}
}