using System;

// 设置布局绝对的渲染顺序
public class CmdLayoutManagerRenderOrder : Command
{
	public GameLayout mLayout;	// 布局
	public int mLayoutID;		// 布局ID,如果布局为空,则通过ID获取
	public int mRenderOrder;	// 绝对渲染顺序
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
			mLayout = mLayoutManager.getLayout(mLayoutID);
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
		builder.append(": mLayoutID:", layoutID).
				append(", mRenderOrder:", mRenderOrder);
	}
}