using System;

// 设置布局的显示隐藏
public class CmdLayoutManagerVisible : Command
{
	public string mParam;		// 显示隐藏时要传递的参数
	public int mLayoutID;		// 布局ID
	public bool mImmediately;	// 是否跳过显示或隐藏过程
	public bool mVisible;		// 显示或隐藏
	public bool mForce;			// 是否强制执行,强制执行时将不会通知布局脚本,仅仅只是设置布局节点的Active
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
				CMD(out CmdLayoutManagerRenderOrder cmd);
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
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mLayoutID:", mLayoutID).
				append(", mVisible:", mVisible).
				append(", mForce:", mForce).
				append(", mImmediately:", mImmediately).
				append(", mParam:", mParam);
	}
}