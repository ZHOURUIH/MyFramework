using System;
using static FrameBase;

// 设置布局的显示隐藏
public class CmdLayoutManagerVisible
{
	// layoutType布局ID
	// visible显示或隐藏
	// param显示隐藏时要传递的参数
	// immediately是否跳过显示或隐藏过程
	// force是否强制执行,强制执行时将不会通知布局脚本,仅仅只是设置布局节点的Active
	static public void execute(Type layoutType, bool visible, bool force)
	{
		GameLayout layout = mLayoutManager.getLayout(layoutType);
		if (layout == null)
		{
			return;
		}
		// 自动计算渲染顺序的布局在显示时,需要重新计算当前渲染顺序
		LAYOUT_ORDER orderType = layout.getRenderOrderType();
		if (visible && (orderType == LAYOUT_ORDER.ALWAYS_TOP_AUTO || orderType == LAYOUT_ORDER.AUTO))
		{
			int renderOrder = mLayoutManager.generateRenderOrder(layout, layout.getRenderOrder(), orderType);
			if (layout.getRenderOrder() != renderOrder)
			{
				CmdLayoutManagerRenderOrder.execute(layout, renderOrder);
			}
		}
		if (!force)
		{
			layout.setVisible(visible);
		}
		else
		{
			layout.setVisibleForce(visible);
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutVisible(visible, layout);
	}
}