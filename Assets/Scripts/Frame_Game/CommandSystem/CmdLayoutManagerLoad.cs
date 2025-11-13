using System;
using static FrameBase;

// 加载一个布局,一般由LT调用
public class CmdLayoutManagerLoad
{
	// layoutType:布局类型
	// renderOrder:渲染顺序,与orderType配合使用
	// visible:加载完毕后是否显示
	// callback异步加载时的完成回调
	public static GameLayout execute<T>(int renderOrder, bool visible = true) where T : GameLayout
	{
		Type layoutType = typeof(T);
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
	public static void executeAsync<T>(int renderOrder, GameLayoutCallback callback) where T : GameLayout
	{
		executeAsync<T>(renderOrder, true, callback, null);
	}
	public static void executeAsync<T>(int renderOrder, Action callback) where T : GameLayout
	{
		executeAsync<T>(renderOrder, true, null, callback);
	}
	public static void executeAsync<T>(int renderOrder, bool visible, GameLayoutCallback callback0, Action callback1) where T : GameLayout
	{
		Type layoutType = typeof(T);
		LayoutInfo info = new()
		{
			mType = layoutType,
			mRenderOrder = renderOrder,
		};
		mLayoutManager.createLayoutAsync(info, (GameLayout layout) =>
		{
			layout.setRenderOrder(renderOrder);
			layout.setVisible(visible);
			callback0?.Invoke(layout);
			callback1?.Invoke();
		});
	}
}