using UnityEngine;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
public class GameBase
{
	// FrameComponent
	public static Game mGame;
	public static BattleSystem mBattleSystem;
	// SQLiteTable
	public static SQLiteDemo mSQLiteDemo;
	// LayoutScript
	public static ScriptDemo mScriptDemo;
	public static ScriptDemoStart mScriptDemoStart;
	public static void constructGameDone()
	{
		mGame = FrameBase.mGameFramework as Game;
		getMainSystem(out mBattleSystem);
	}
	//-----------------------------------------------------------------------------------------------------------------------------------------------
	protected static void getMainSystem<T>(out T system) where T : FrameSystem
	{
		system = mGame.getSystem(typeof(T)) as T;
	}
}
