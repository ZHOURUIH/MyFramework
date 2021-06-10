using System;

public class CmdLayoutManagerLoad : Command 
{
	public LayoutAsyncDone mCallback;
	public LAYOUT_ORDER mOrderType;
	public string mParam;
	public int mRenderOrder;
	public int mLayoutID;
	public bool mImmediatelyShow;
	public bool mVisible;
	public bool mIsScene;
	public bool mAsync;
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mLayoutID = LAYOUT.NONE;
		mOrderType = LAYOUT_ORDER.ALWAYS_TOP;
		mParam = null;
		mVisible = true;
		mAsync = false;
		mImmediatelyShow = false;
		mIsScene = false;
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
		GameLayout layout = mLayoutManager.createLayout(info, mAsync);
		if (layout == null)
		{
			return;
		}
		// 计算实际的渲染顺序
		int renderOrder = mLayoutManager.generateRenderOrder(layout, mRenderOrder, mOrderType);
		// 顺序有改变,则设置最新的顺序
		if (layout.getRenderOrder() != renderOrder)
		{
			CMD_MAIN(out CmdLayoutManagerRenderOrder cmd);
			cmd.mLayoutID = mLayoutID;
			cmd.mRenderOrder = renderOrder;
			pushCommand(cmd, mLayoutManager);
		}
		if (mVisible)
		{
			layout.setVisible(mVisible, mImmediatelyShow, mParam);
		}
		// 隐藏时需要设置强制隐藏,不通知脚本,因为通常这种情况只是想后台加载布局
		else
		{
			layout.setVisibleForce(mVisible);
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutVisible(mVisible, layout);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mLayoutID:", mLayoutID).
				Append(", mVisible:", mVisible).
				Append(", mRenderOrder:", mRenderOrder).
				Append(", mAsync:", mAsync).
				Append(", mParam:", mParam);
	}
}