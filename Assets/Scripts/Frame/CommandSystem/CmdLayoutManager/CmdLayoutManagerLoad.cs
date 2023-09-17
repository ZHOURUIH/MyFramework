using System;
using static FrameBase;

// 加载一个布局,一般由LT调用
public class CmdLayoutManagerLoad
{
	// 异步加载时的完成回调
	// 布局的顺序类型
	// 布局显示或隐藏时要传递的参数
	// 渲染顺序,与mOrderType配合使用
	// 布局ID
	// 是否跳过显示动效立即显示
	// 加载完毕后是否显示
	// 是否不挂接到UGUIRoot下,不挂接在UGUIRoot下将由其他摄像机进行渲染
	// 是否异步加载
	public static void execute(int layoutID, int renderOrder, LAYOUT_ORDER orderType, string param, 
		bool immediatelyShow, bool visible, bool isScene, bool async, LayoutAsyncDone callback)
	{
		LayoutInfo info = new LayoutInfo();
		info.mID = layoutID;
		info.mRenderOrder = renderOrder;
		info.mOrderType = orderType;
		info.mIsScene = isScene;
		info.mCallback = callback;
		GameLayout layout = mLayoutManager.createLayout(info, async);
		if (layout == null)
		{
			return;
		}
		// 计算实际的渲染顺序
		int realRenderOrder = mLayoutManager.generateRenderOrder(layout, renderOrder, orderType);
		// 顺序有改变,则设置最新的顺序
		if (layout.getRenderOrder() != realRenderOrder)
		{
			CmdLayoutManagerRenderOrder.execute(layout, realRenderOrder);
		}
		// 显示状态一致,就不需要再继续执行
		if (layout.isVisible() == visible)
		{
			return;
		}
		if (visible)
		{
			layout.setVisible(visible, immediatelyShow, param);
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