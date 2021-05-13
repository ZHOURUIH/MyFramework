using System;

public class ApplicationConfig : ConfigBase
{
	public override void writeConfig()
	{
		// 只有资源目录为本地目录时才可以写入
		if (mResourceManager.isLocalRootPath())
		{
			writeTxtFile(mResourceManager.getResourceRootPath() + FrameDefine.SA_CONFIG_PATH + "ApplicationSetting.txt", generateFloatFile());
		}
	}
	//---------------------------------------------------------------------------------------------------------------------------------
	protected override void addFloat()
	{
		addFloat("FULL_SCREEN", APPLICATION_FLOAT.FULL_SCREEN);
		addFloat("SCREEN_WIDTH", APPLICATION_FLOAT.SCREEN_WIDTH);
		addFloat("SCREEN_HEIGHT", APPLICATION_FLOAT.SCREEN_HEIGHT);
		addFloat("FORCE_TOP", APPLICATION_FLOAT.FORCE_TOP);
		addFloat("USE_FIXED_TIME", APPLICATION_FLOAT.USE_FIXED_TIME);
		addFloat("FIXED_TIME", APPLICATION_FLOAT.FIXED_TIME);
		addFloat("VSYNC", APPLICATION_FLOAT.VSYNC);
	}
	protected override void addString()
	{
		;
	}
	protected override void readConfig()
	{
		readFile(mResourceManager.getResourceRootPath() + FrameDefine.SA_CONFIG_PATH + "ApplicationSetting.txt", true);
	}
}