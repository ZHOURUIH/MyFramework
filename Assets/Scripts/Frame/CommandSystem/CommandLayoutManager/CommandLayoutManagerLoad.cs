using System;

public class CommandLayoutManagerLoad : Command 
{
	public LayoutAsyncDone mCallback;
	public GameLayout mResultLayout;
	public LAYOUT_ORDER mOrderType;
	public string mParam;
	public bool mImmediatelyShow;
	public bool mAlwaysTop;
	public bool mVisible;
	public bool mIsScene;
	public bool mAsync;
	public int mRenderOrder;
	public int mLayoutID;
	public override void init()
	{
		base.init();
		mCallback = null;
		mResultLayout = null;
		mLayoutID = LAYOUT.NONE;
		mOrderType = LAYOUT_ORDER.ALWAYS_TOP;
		mParam = null;
		mVisible = true;
		mAsync = false;
		mImmediatelyShow = false;
		mIsScene = false;
		mAlwaysTop = false;
		mRenderOrder = 0;
	}
	public override void execute()
	{
		LayoutInfo info = new LayoutInfo();
		info.mID = mLayoutID;
		info.mRenderOrder = mRenderOrder;
		info.mOrderType = mOrderType;
		info.mIsScene = mIsScene;
		info.mCallback = mCallback;
		mResultLayout = mLayoutManager.createLayout(info, mAsync);
		if (mResultLayout == null)
		{
			return;
		}
		// 计算实际的渲染顺序
		int renderOrder = mLayoutManager.generateRenderOrder(mResultLayout, mRenderOrder, mOrderType);
		// 顺序有改变,则设置最新的顺序
		if (mResultLayout.getRenderOrder() != renderOrder)
		{
			mResultLayout.setRenderOrder(renderOrder);
		}
		if (mVisible)
		{
			mResultLayout.setVisible(mVisible, mImmediatelyShow, mParam);
		}
		// 隐藏时需要设置强制隐藏,不通知脚本,因为通常这种情况只是想后台加载布局
		else
		{
			mResultLayout.setVisibleForce(mVisible);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLayoutID:" + mLayoutID + ", mVisible:" + mVisible + ", mResultLayout:" + mResultLayout + ", mRenderOrder:" + mRenderOrder + ", mAsync:" + mAsync + ", mCallback:" + mCallback + ", mParam:" + mParam;
	}
}