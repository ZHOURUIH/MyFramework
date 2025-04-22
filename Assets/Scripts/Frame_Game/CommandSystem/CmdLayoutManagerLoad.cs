using System;
using static FrameBase;

// 加载一个布局,一般由LT调用
public class CmdLayoutManagerLoad
{
	// layoutType:布局类型
	// renderOrder:渲染顺序,与orderType配合使用
	// visible:加载完毕后是否显示
	// callback异步加载时的完成回调
	public static GameLayout execute(Type layoutType, int renderOrder, bool visible = true)
	{
		LayoutInfo info = new()
		{
			mType = layoutType,
			mRenderOrder = renderOrder,
		};
		GameLayout layout = mLayoutManager.createLayout(info);
		if (layout == null)
		{
			return null;
		}
		layout.setRenderOrder(renderOrder);
		layout.setVisible(visible);
		return layout;
	}
	public static GameLayout executeAsync(Type layoutType, int renderOrder, bool visible = true, GameLayoutCallback callback = null)
	{
		LayoutInfo info = new()
		{
			mType = layoutType,
			mRenderOrder = renderOrder,
		};
		mLayoutManager.createLayoutAsync(info, (GameLayout layout) =>
		{
			layout.setRenderOrder(renderOrder);
			layout.setVisible(visible);
			callback?.Invoke(layout);
		});
		return null;
	}
}