using UnityEngine;
using System.Collections;

public class CommandLayoutManagerLoadLayout : Command 
{
	public LAYOUT mLayoutType;
	public bool mVisible;
	public GameLayout mResultLayout;
	public int mRenderOrder;
	public bool mAsync;
	public LayoutAsyncDone mCallback;
	public string mParam;
	public bool mImmediatelyShow;
	public bool mIsNGUI;
	public bool mIsScene;
	public override void init()
	{
		base.init();
		mLayoutType = LAYOUT.L_MAX;
		mVisible = true;
		mResultLayout = null;
		mRenderOrder = 0;
		mAsync = false;
		mCallback = null;
		mParam = EMPTY_STRING;
		mImmediatelyShow = false;
		mIsNGUI = true;
		mIsScene = false;
	}
	public override void execute()
	{
		mResultLayout = mLayoutManager.createLayout(mLayoutType, mRenderOrder, mAsync, mCallback, mIsNGUI, mIsScene);
		if(mResultLayout != null && mResultLayout.getRenderOrder() != mRenderOrder)
		{
			mResultLayout.setRenderOrder(mRenderOrder);
		}
		// 只有同步加载时才能立即设置布局的显示
		if (mResultLayout != null && !mAsync)
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
		return base.showDebugInfo() + ": mLayoutType:" + mLayoutType + ", mVisible:" + mVisible + ", mResultLayout:" + mResultLayout + ", mRenderOrder:" + mRenderOrder + ", mAsync:" + mAsync + ", mCallback:" + mCallback + ", mParam:" + mParam;
	}
}