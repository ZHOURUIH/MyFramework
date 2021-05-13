using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MainSceneGaming : SceneProcedure
{
	protected CharacterGame mPlayer;
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		LT.LOAD_UGUI_SHOW(LAYOUT_ILR.GAMING);
		uint id = makeID();
		GB.CMD_ILR(out CmdCharacterManagerCreateCharacter cmd);
		cmd.mCharacterType = typeof(CharacterGame);
		cmd.mName = "test";
		cmd.mID = id;
		pushCommand(cmd, mCharacterManager);
		mPlayer = mCharacterManager.getCharacter(id) as CharacterGame;
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE_LAYOUT(LAYOUT_ILR.GAMING);
		GB.CMD_ILR(out CmdCharacterManagerDestroy cmd);
		cmd.mGUID = mPlayer.getGUID();
		pushCommand(cmd, mCharacterManager);
	}
}