using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameConfig : ConfigBase
{
	public override void writeConfig()
	{
		writeTxtFile(FrameDefine.F_CONFIG_PATH + "GameFloatConfig.txt", generateFloatFile());
		writeTxtFile(FrameDefine.F_CONFIG_PATH + "GameStringConfig.txt", generateStringFile());
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected override void addFloat()
	{
		if (mFloatNameToDefine.Count != (int)GAME_FLOAT.GAME_MAX - (int)GAME_FLOAT.GAME_MIN - 1)
		{
			logError("not all float parameter added!");
		}
	}
	protected override void addString()
	{
		if (mStringNameToDefine.Count != (int)GAME_STRING.GAME_MAX - (int)GAME_STRING.GAME_MIN - 1)
		{
			logError("not all string parameter added!");
		}
	}
	protected override void readConfig()
	{
		readFile(FrameDefine.F_CONFIG_PATH + "GameFloatConfig.txt", true);
		readFile(FrameDefine.F_CONFIG_PATH + "GameStringConfig.txt", false);
	}
}