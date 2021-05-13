using UnityEngine;
using System.Collections;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
public class GameBase : FrameBase
{
	// FrameComponent
	public static Game mGame;
	public static BattleSystem mBattleSystem;
	// SQLiteTable
	public static SQLiteDemo mSQLiteDemo;
	// LayoutScript
	public static ScriptDemo mScriptDemo;
	public static ScriptDemoStart mScriptDemoStart;
	public override void notifyConstructDone()
	{
		base.notifyConstructDone();
		mGame = mGameFramework as Game;
		mBattleSystem = mGame.getSystem(typeof(BattleSystem)) as BattleSystem;
	}
}
