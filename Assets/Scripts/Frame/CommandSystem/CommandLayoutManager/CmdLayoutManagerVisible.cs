using System;

public class CmdLayoutManagerVisible : Command
{
	public string mParam;
	public int mLayoutID;
	public bool mImmediately;
	public bool mVisible;
	public bool mForce;
	public override void resetProperty()
	{
		base.resetProperty();
		mLayoutID = LAYOUT.NONE;
		mParam = null;
		mForce = false;
		mImmediately = false;
		mVisible = true;
	}
	public override void execute()
	{
		GameLayout layout = mLayoutManager.getGameLayout(mLayoutID);
		if (layout == null)
		{
			return;
		}
		// 自动计算渲染顺序的布局在显示时,需要重新计算当前渲染顺序
		LAYOUT_ORDER orderType = layout.getRenderOrderType();
		if (mVisible && (orderType == LAYOUT_ORDER.ALWAYS_TOP_AUTO || orderType == LAYOUT_ORDER.AUTO))
		{
			int renderOrder = mLayoutManager.generateRenderOrder(layout, layout.getRenderOrder(), orderType);
			if (layout.getRenderOrder() != renderOrder)
			{
				CMD_MAIN(out CmdLayoutManagerRenderOrder cmd);
				cmd.mLayout = layout;
				cmd.mRenderOrder = renderOrder;
				pushCommand(cmd, mLayoutManager);
			}
		}
		if (!mForce)
		{
			layout.setVisible(mVisible, mImmediately, mParam);
		}
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
				Append(", mForce:", mForce).
				Append(", mImmediately:", mImmediately).
				Append(", mParam:", mParam);
	}
}