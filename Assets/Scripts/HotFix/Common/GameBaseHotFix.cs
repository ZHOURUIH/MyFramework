
// 这个类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBH,全称为GameBaseHotFix
public class GBR
{
	// FrameSystem
	public static NetManager mNetManager;
	public static DemoSystem mDemoSystem;
	public static BattleSystem mBattleSystem;

    // 需要添加auto generate Excel start和auto generate Excel end才会自动生成代码
    // auto generate Excel start
    public static ExcelAchivement mExcelAchivement;
    public static ExcelGlobal mExcelGlobal;
    public static ExcelTest mExcelTest;
    // auto generate Excel end

    // 需要添加auto generate LayoutScript start和auto generate LayoutScript end才会自动生成代码
    // auto generate LayoutScript start
    public static UIGame mUIGame;
	public static UILogin mUILogin;
    // auto generate LayoutScript end
}