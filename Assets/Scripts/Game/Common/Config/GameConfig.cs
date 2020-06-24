using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameConfig : ConfigBase
{
	public GameConfig(string name)
		:base(name){}
	public override void writeConfig()
	{
		writeTxtFile(CommonDefine.F_CONFIG_PATH + "GameFloatConfig.txt", generateFloatFile());
		writeTxtFile(CommonDefine.F_CONFIG_PATH + "GameStringConfig.txt", generateStringFile());
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected override void addFloat()
	{
		if (mFloatNameToDefine.Count != (int)GAME_DEFINE_FLOAT.GDF_GAME_MAX - (int)GAME_DEFINE_FLOAT.GDF_GAME_MIN - 1)
		{
			logError("not all float parameter added!");
		}
	}
	protected override void addString()
	{
		if (mStringNameToDefine.Count != (int)GAME_DEFINE_STRING.GDS_GAME_MAX - (int)GAME_DEFINE_STRING.GDS_GAME_MIN - 1)
		{
			logError("not all string parameter added!");
		}
	}
	protected override void readConfig()
	{
		readFile(CommonDefine.F_CONFIG_PATH + "GameFloatConfig.txt", true);
		readFile(CommonDefine.F_CONFIG_PATH + "GameStringConfig.txt", false);
	}
}