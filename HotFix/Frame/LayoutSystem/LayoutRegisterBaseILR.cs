using System;
using static FrameBase;
using static UnityUtility;

public class LayoutRegisterBaseILR
{
	protected static void registeLayout<T>(int layout, string mobileName, string standaloneName) where T : LayoutScript
	{
		registeLayout<T>(layout, mobileName, standaloneName, false, LAYOUT_LIFE_CYCLE.PART_USE);
	}
	protected static void registeLayout<T>(int layout, string mobileName, string standaloneName, LAYOUT_LIFE_CYCLE lifeCycle) where T : LayoutScript
	{
		registeLayout<T>(layout, mobileName, standaloneName, false, lifeCycle);
	}
	protected static void registeLayout<T>(int layout, string mobileName, string standaloneName, bool inResource) where T : LayoutScript
	{
		registeLayout<T>(layout, mobileName, standaloneName, inResource, LAYOUT_LIFE_CYCLE.PART_USE);
	}
	protected static void registeLayout<T>(int layout, string mobileName, string standaloneName, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle) where T : LayoutScript
	{
		// 根据平台选择使用的文件,如果为空,则选择其中一个不为空的界面名字
		string layoutName = isMobile() ? mobileName : standaloneName;
		if (layoutName == null)
		{
			layoutName = mobileName != null ? mobileName : standaloneName;
		}
		mLayoutManager.registeLayout(typeof(T), layout, layoutName, inResource, lifeCycle);
	}
	protected static bool assign<T>(ref T thisScript, LayoutScript value, bool created) where T : LayoutScript
	{
		if (typeof(T) == value.GetType())
		{
			thisScript = created ? value as T : null;
			return true;
		}
		return false;
	}
}