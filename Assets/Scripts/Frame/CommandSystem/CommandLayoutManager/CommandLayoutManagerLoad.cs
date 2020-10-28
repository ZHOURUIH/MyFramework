using UnityEngine;
using System.Collections;

public class CommandLayoutManagerLoad : Command 
{
	public LayoutAsyncDone mCallback;
	public GameLayout mResultLayout;
	public LAYOUT_ORDER mOrderType;
	public GUI_TYPE mGUIType;
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
		mGUIType = GUI_TYPE.UGUI;
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
		mResultLayout = mLayoutManager.createLayout(mLayoutID, mRenderOrder, mOrderType, mAsync, mCallback, mGUIType, mIsScene);
		if (mResultLayout == null)
		{
			return;
		}
		// 计算实际的渲染顺序
		mRenderOrder = mLayoutManager.generateRenderOrder(mRenderOrder, mOrderType);
		// 顺序有改变,则设置最新的顺序
		if (mResultLayout.getRenderOrder() != mRenderOrder)
		{
			mResultLayout.setRenderOrder(mRenderOrder);
		}
		// 只有同步加载时才能立即设置布局的显示
		if (!mAsync)
		{
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
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLayoutID:" + mLayoutID + ", mVisible:" + mVisible + ", mResultLayout:" + mResultLayout + ", mRenderOrder:" + mRenderOrder + ", mAsync:" + mAsync + ", mCallback:" + mCallback + ", mParam:" + mParam;
	}
}