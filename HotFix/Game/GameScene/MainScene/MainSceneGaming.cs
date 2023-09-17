using System;
using static CSharpUtility;
using static FrameBase;

public class MainSceneGaming : SceneProcedure
{
	protected CharacterGame mPlayer;
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		LT.LOAD_UGUI_SHOW(LAYOUT_ILR.GAMING);
		mPlayer = mCharacterManager.createCharacter("test", typeof(CharacterGame), makeID()) as CharacterGame;
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE_LAYOUT(LAYOUT_ILR.GAMING);
		mCharacterManager.destroyCharacter(mPlayer);
	}
}