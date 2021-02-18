using System;

public class ApplicationConfig : ConfigBase
{
	public override void writeConfig()
	{
		// 只有资源目录为本地目录时才可以写入
		if (ResourceManager.mLocalRootPath)
		{
			writeTxtFile(ResourceManager.mResourceRootPath + FrameDefine.SA_CONFIG_PATH + "ApplicationSetting.txt", generateFloatFile());
		}
	}
	//---------------------------------------------------------------------------------------------------------------------------------
	protected override void addFloat()
	{
		addFloat(GAME_FLOAT.FULL_SCREEN);
		addFloat(GAME_FLOAT.SCREEN_WIDTH);
		addFloat(GAME_FLOAT.SCREEN_HEIGHT);
		addFloat(GAME_FLOAT.FORCE_TOP);
		addFloat(GAME_FLOAT.USE_FIXED_TIME);
		addFloat(GAME_FLOAT.FIXED_TIME);
		addFloat(GAME_FLOAT.VSYNC);
		if (mFloatNameToDefine.Count != (int)GAME_FLOAT.APPLICATION_MAX - (int)GAME_FLOAT.APPLICATION_MIN - 1)
		{
			logError("not all float parameter added!");
		}
	}
	protected override void addString()
	{
		if (mStringNameToDefine.Count != (int)GAME_STRING.APPLICATION_MAX - (int)GAME_STRING.APPLICATION_MIN - 1)
		{
			logError("not all string parameter added!");
		}
	}
	protected override void readConfig()
	{
		readFile(ResourceManager.mResourceRootPath + FrameDefine.SA_CONFIG_PATH + "ApplicationSetting.txt", true);
	}
}