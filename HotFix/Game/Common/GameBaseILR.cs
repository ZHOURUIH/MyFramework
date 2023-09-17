using System;
using static FrameBase;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBR,全称为GameBaseILR
public partial class GBR
{
	// FrameSystem
	public static DemoSystem mDemoSystem;
	// LayoutScript
	public static UILogin mUILogin;
	public static UIGaming mUIGaming;
	public static void constructILRDone()
	{
		getILRSystem(out mDemoSystem);
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected static void getILRSystem<T>(out T system) where T : FrameSystem
	{
		system = mGameFramework.getSystem(typeof(T)) as T;
	}
}