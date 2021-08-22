using UnityEngine;
using System.Collections.Generic;
using System;

public class LayoutLoadGroup : FrameBase
{
	protected Dictionary<int, LayoutLoadInfo> mLoadInfo;
	protected LayoutAsyncDone mLoadCallback;
	protected int mLoadedCount;
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