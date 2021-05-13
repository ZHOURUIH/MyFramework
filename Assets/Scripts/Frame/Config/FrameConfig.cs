using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FrameConfig : ConfigBase
{
	public override void writeConfig()
	{
		// 只有资源目录为本地目录时才可以写入
		if (mResourceManager.isLocalRootPath())
		{
			writeTxtFile(mResourceManager.getResourceRootPath() + FrameDefine.SA_CONFIG_PATH + "FrameFloatConfig.txt", generateFloatFile());
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected override void addFloat()
	{
		addFloat("LOAD_RESOURCES", FRAME_FLOAT.LOAD_RESOURCES);
		addFloat("LOG_LEVEL", FRAME_FLOAT.LOG_LEVEL);
	}
	protected override void addString()
	{
		;
	}
	protected override void readConfig()
	{
		readFile(mResourceManager.getResourceRootPath() + FrameDefine.SA_CONFIG_PATH + "FrameFloatConfig.txt", true);
	}
}