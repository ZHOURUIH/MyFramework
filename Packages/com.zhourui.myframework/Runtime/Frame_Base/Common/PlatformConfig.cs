using static FrameBaseUtility;

public class PlatformConfig
{
	public static string getResourceFolder(string gameName)
	{
		var settings = PlatformSettings.get();
		string folder;
		if (isByteDance())
		{
			folder = settings.ByteDanceResourceFolder;
		}
		else if (isOppo())
		{
			folder = settings.OppoResourceFolder;
		}
		else if (isVivo())
		{
			folder = settings.VivoResourceFolder;
		}
		else if (isWeiXin())
		{
			folder = settings.WeChatResourceFolder;
		}
		else
		{
			folder = settings.DefaultResourceFolder;
		}
		return folder.Replace("{GameName}", gameName);
	}
}
