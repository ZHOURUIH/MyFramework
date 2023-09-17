using System;

public struct LayoutInfo
{
	public LayoutAsyncDone mCallback;
	public LAYOUT_ORDER mOrderType;
	public string mName;
	public bool mIsScene;
	public int mRenderOrder;
	public int mID;
}