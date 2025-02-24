using static FrameUtility;
using static FrameBase;

public class MainSceneGaming : SceneProcedure
{
	protected CharacterGame mPlayer;
	protected override void onInit(SceneProcedure lastProcedure, string intent)
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
			CSAttack.send();
		}
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE<UIGaming>();
		mCharacterManager?.destroyCharacter(mPlayer);
	}
}