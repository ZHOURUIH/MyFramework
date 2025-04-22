using static FrameBaseHotFix;

// 设置布局绝对的渲染顺序
public class CmdLayoutManagerRenderOrder
{
	// layout,布局
	// renderOrder,绝对渲染顺序
	public static void execute(GameLayout layout, int renderOrder)
	{
		if (layout == null || renderOrder == layout.getRenderOrder())
		{
			return;
		}
		layout.setRenderOrder(renderOrder);
		mLayoutManager.notifyLayoutRenderOrder();
	}
}