using static FrameUtility;
using static FrameBaseHotFix;
using static UnityUtility;
using static GBH;

public class MainSceneGaming : SceneProcedure
{
	protected CharacterGame mPlayer;
	protected override void onInit(SceneProcedure lastProcedure)
	{
		mPlayer = mCharacterManager.createCharacter<CharacterGame>("test");
		LT.LOAD_SHOW<UIGaming>();
	}
	protected override void onUpdate(float elapsedTime)
	{
		base.onUpdate(elapsedTime);
		
		// 攻击
		if (isKeyCurrentDown(UnityEngine.KeyCode.I))
		{
			if (mNetManager.isConnected())
			{
				CSAttack.send();
			}
			else
			{
				log("正在使用快捷键进行攻击,但是未连接服务器");
			}
		}
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE<UIGaming>();
		mCharacterManager?.destroyCharacter(mPlayer);
	}
}