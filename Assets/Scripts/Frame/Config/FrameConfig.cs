using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FrameConfig : ConfigBase
{
	public override void writeConfig()
	{
		// 只有资源目录为本地目录时才可以写入
		if (ResourceManager.mLocalRootPath)
		{
			writeTxtFile(ResourceManager.mResourceRootPath + FrameDefine.SA_CONFIG_PATH + "FrameFloatConfig.txt", generateFloatFile());
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected override void addFloat()
	{
		addFloat(GAME_FLOAT.LOAD_RESOURCES);
		addFloat(GAME_FLOAT.LOG_LEVEL);
		addFloat(GAME_FLOAT.ENABLE_KEYBOARD);
		addFloat(GAME_FLOAT.PERSISTENT_DATA_FIRST);
		if (mFloatNameToDefine.Count != (int)GAME_FLOAT.FRAME_MAX - (int)GAME_FLOAT.FRAME_MIN - 1)
		{
			logError("not all float parameter added!");
		}
	}
	protected override void addString()
	{
		if (mStringNameToDefine.Count != (int)GAME_STRING.FRAME_MAX - (int)GAME_STRING.FRAME_MIN - 1)
		{
			logError("not all string parameter added!");
		}
	}
	protected override void readConfig()
	{
		readFile(ResourceManager.mResourceRootPath + FrameDefine.SA_CONFIG_PATH + "FrameFloatConfig.txt", true);
	}
}