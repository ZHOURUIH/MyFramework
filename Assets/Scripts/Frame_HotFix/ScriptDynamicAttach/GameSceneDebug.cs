using UnityEngine;
using static FrameBaseHotFix;

// 逻辑场景调试信息
public class GameSceneDebug : MonoBehaviour
{
	protected GameScene mGameScene;	// 所属场景
	public string mCurProcedure;	// 当前流程
	public void setGameScene(GameScene scene) { mGameScene = scene; }
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		SceneProcedure sceneProcedure = mGameScene.getCurProcedure();
		if (sceneProcedure != null)
		{
			mCurProcedure = sceneProcedure.GetType().ToString();
		}
	}
}