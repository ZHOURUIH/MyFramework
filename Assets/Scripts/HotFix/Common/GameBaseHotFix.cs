
// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBR,全称为GameBaseHotFix
public partial class GBH
{
	// FrameSystem
	public static NetManager mNetManager;
	public static DemoSystem mDemoSystem;
	public static BattleSystem mBattleSystem;
	// LayoutScript
	public static UILogin mUILogin;
	public static UIGaming mUIGaming;
}