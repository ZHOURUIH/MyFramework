using System;
using static FrameBaseHotFix;
using static FrameUtility;

// 加载一个布局,一般由LT调用
public class CmdLayoutManagerLoad
{
	// layoutType:布局类型
	// renderOrder:渲染顺序,与orderType配合使用
	// orderType:布局的顺序类型
	// visible:加载完毕后是否显示
	// isScene:是否不挂接到UGUIRoot下,不挂接在UGUIRoot下将由其他摄像机进行渲染
	// async:是否异步加载
	// callback异步加载时的完成回调
	public static void executeAsync(Type layoutType, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene)
	{
		executeAsync(layoutType, renderOrder, orderType, visible, isScene, null, null);
	}
	public static void executeAsync(Type layoutType, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene, Action callback)
	{
		executeAsync(layoutType, renderOrder, orderType, visible, isScene, null, callback);
	}
	public static void executeAsync(Type layoutType, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene, GameLayoutCallback callback)
	{
		executeAsync(layoutType, renderOrder, orderType, visible, isScene, callback, null);
	}
	public static LayoutScript execute(Type layoutType, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene)
	{
		using var a = new ClassScope<LayoutInfo>(out var info);
		info.mType = layoutType;
		info.mRenderOrder = renderOrder;
		info.mOrderType = orderType;
		info.mIsScene = isScene;
		GameLayout layout = mLayoutManager.createLayout(info);
		if (layout == null)
		{
			return null;
		}
		postCreate(layout, renderOrder, orderType, visible);
		return layout.getScript();
	}
	protected static void executeAsync(Type layoutType, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene, GameLayoutCallback callback0, Action callback1)
	{
		CLASS(out LayoutInfo info);
		info.mType = layoutType;
		info.mRenderOrder = renderOrder;
		info.mOrderType = orderType;
		info.mIsScene = isScene;
		mLayoutManager.createLayoutAsync(info, (GameLayout layout) =>
		{
			UN_CLASS(info);
			postCreate(layout, renderOrder, orderType, visible);
			callback0?.Invoke(layout);
			callback1?.Invoke();
		});
	}
	//---------------------------------------------------------------------------------------------------------------------------
	protected static void postCreate(GameLayout layout, int renderOrder, LAYOUT_ORDER orderType, bool visible)
	{
		// 计算实际的渲染顺序
		int realRenderOrder = mLayoutManager.generateRenderOrder(layout, renderOrder, orderType);
		// 设置最新的顺序
		CmdLayoutManagerRenderOrder.execute(layout, realRenderOrder);
		// 显示状态一致,就不需要再继续执行
		if (layout.isVisible() == visible)
		{
			return;
		}
		if (visible)
		{
			layout.setVisible(visible);
		}
		// 隐藏时需要设置强制隐藏,不通知脚本,因为通常这种情况只是想后台加载布局
		else
		{
			layout.setVisibleForce(visible);
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutVisible(visible, layout);
	}
}