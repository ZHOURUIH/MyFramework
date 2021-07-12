using System;

// 设置布局绝对的渲染顺序
public class CmdLayoutManagerRenderOrder : Command
{
	public GameLayout mLayout;
	public int mLayoutID;
	public int mRenderOrder;
	public override void resetProperty()
	{
		base.resetProperty();
		mLayout = null;
		mLayoutID = LAYOUT.NONE;
		mRenderOrder = 0;
	}
	public override void execute()
	{
		if(mLayout == null)
		{
			mLayout = mLayoutManager.getGameLayout(mLayoutID);
		}
		if (mLayout == null)
		{
			return;
		}
		if (mRenderOrder != mLayout.getRenderOrder())
		{
			mLayout.setRenderOrder(mRenderOrder);
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutRenderOrder();
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		int layoutID = mLayout != null ? mLayout.getID() : LAYOUT.NONE;
		if(layoutID == LAYOUT.NONE)
		{
			layoutID = mLayoutID;
		}
		builder.Append(": mLayoutID:", layoutID).
				Append(", mRenderOrder:", mRenderOrder);
	}
}