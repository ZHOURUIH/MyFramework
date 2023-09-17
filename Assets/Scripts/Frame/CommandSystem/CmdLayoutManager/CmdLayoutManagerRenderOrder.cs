using System;
using static FrameBase;

// 设置布局绝对的渲染顺序
public class CmdLayoutManagerRenderOrder
{
	// layout,布局
	// layoutID,布局ID,如果布局为空,则通过ID获取
	// renderOrder,绝对渲染顺序
	public static void execute(GameLayout layout, int renderOrder)
	{
		if (layout == null)
		{
			return;
		}
		if (renderOrder != layout.getRenderOrder())
		{
			layout.setRenderOrder(renderOrder);
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutRenderOrder();
	}
}