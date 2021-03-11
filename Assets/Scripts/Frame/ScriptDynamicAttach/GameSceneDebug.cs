using UnityEngine;

public class GameSceneDebug : MonoBehaviour
{
	public GameScene mGameScene;
	public string mCurProcedure;
	public void Start()
	{
		mGameScene = FrameBase.getCurScene();
	}
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		SceneProcedure sceneProcedure = mGameScene.getCurProcedure();
		if (sceneProcedure != null)
		{
			mCurProcedure = sceneProcedure.getType().ToString();
		}
	}
}