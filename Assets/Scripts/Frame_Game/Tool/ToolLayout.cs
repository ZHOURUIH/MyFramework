using System;
using static FrameBase;

// 全部都是对于UI布局或窗口的操作,部分Transformable的通用操作在ToolFrame中
public class LT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 布局
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene, bool isAsync, GameLayoutCallback callback)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, visible, isScene, isAsync, callback);
	}
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, visible, false, false, null);
	}
	public static void LOAD_ASYNC(Type type, int renderOrder, LAYOUT_ORDER orderType, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, orderType, true, false, true, callback);
	}
	public static void LOAD_ASYNC_HIDE(Type type, int renderOrder, LAYOUT_ORDER orderType, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, orderType, false, false, true, callback);
	}
	public static T LOAD_TOP_SHOW<T>(int order) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), order, LAYOUT_ORDER.ALWAYS_TOP, true, false, false, null) as T;
	}
	public static T LOAD_TOP_SHOW<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.ALWAYS_TOP, true, false, false, null) as T;
	}
	public static T LOAD_SHOW<T>(int renderOrder, LAYOUT_ORDER orderType) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, orderType, true, false, false, null) as T;
	}
	public static T LOAD_SHOW<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false, true, null) as T;
	}
	public static void HIDE<T>() where T : LayoutScript
	{
		CmdLayoutManagerVisible.execute(typeof(T), false, false);
	}
	public static void HIDE(Type type)
	{
		CmdLayoutManagerVisible.execute(type, false, false);
	}
	public static void HIDE_FORCE(Type type)
	{
		CmdLayoutManagerVisible.execute(type, false, true);
	}
	public static void VISIBLE<T>(bool visible) where T : LayoutScript
	{
		CmdLayoutManagerVisible.execute(typeof(T), visible, false);
	}
	public static void VISIBLE(Type type, bool visible)
	{
		CmdLayoutManagerVisible.execute(type, visible, false);
	}
	public static void VISIBLE_FORCE(Type type, bool visible)
	{
		CmdLayoutManagerVisible.execute(type, visible, true);
	}
	public static void UNLOAD(Type type)
	{
		// 需要首先强制隐藏布局
		CmdLayoutManagerVisible.execute(type, false, true);
		mLayoutManager.destroyLayout(type);
	}
}