
// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBH,全称为GameBaseHotFix
public partial class GBH
{
	// FrameSystem
	public static NetManager mNetManager;
	public static DemoSystem mDemoSystem;
	public static BattleSystem mBattleSystem;
	// 需要添加auto generate LayoutScript start和auto generate LayoutScript end才会自动生成代码
	// auto generate LayoutScript start
	public static UIGaming mUIGaming;
	public static UILogin mUILogin;
	// auto generate LayoutScript end
}