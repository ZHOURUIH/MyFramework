
// 逻辑场景管理器
public class GameSceneManager : FrameSystem
{
	protected GameScene mCurScene;							// 当前场景
	public GameScene getCurScene(){ return mCurScene; }
	public void enterScene<T>() where T : GameScene, new()
	{
		if (mCurScene != null)
		{
			return;
		}
		mCurScene = new T();
		mCurScene.init();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mCurScene?.update(elapsedTime);
	}
	public override void destroy()
	{
		mCurScene?.exit();
		mCurScene?.destroy();
		mCurScene = null;
		base.destroy();
	}
	public override void willDestroy()
	{
		base.willDestroy();
		mCurScene?.willDestroy();
	}
}