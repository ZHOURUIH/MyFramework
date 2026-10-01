using static FrameBaseUtility;

public class PlatformConfigHotFix
{
	public static string getAppID()
	{
		var settings = PlatformSettings.get();
		if (isByteDance())
		{
			return settings.ByteDanceAppID;
		}
		else if (isOppo())
		{
			return settings.OppoAppID;
		}
		else if (isVivo())
		{
			return settings.VivoAppID;
		}
		else
		{
			return "";
		}
	}
	public static string getRewardedVideoID()
	{
		var settings = PlatformSettings.get();
		if (isByteDance())
		{
			return settings.ByteDanceRewardedVideoID;
		}
		else if (isOppo())
		{
			return settings.OppoRewardedVideoID;
		}
		else if (isVivo())
		{
			return settings.VivoRewardedVideoID;
		}
		else
		{
			return "";
		}
	}
	public static string getInterstitialID()
	{
		var settings = PlatformSettings.get();
		if (isByteDance())
		{
			return settings.ByteDanceInterstitialID;
		}
		else if (isOppo())
		{
			return settings.OppoInterstitialID;
		}
		else if (isVivo())
		{
			return settings.VivoInterstitialID;
		}
		else
		{
			return "";
		}
	}
	public static string getPlatformDefine()
	{
		if (isByteDance())
		{
			return "BYTE_DANCE";
		}
		else if (isOppo())
		{
			return "OPPO_MINI_GAME";
		}
		else if (isVivo())
		{
			return "VIVO_MINI_GAME";
		}
		else if (isWeiXin())
		{
			return "UNITY_WEIXINMINIGAME";
		}
		else
		{
			return "";
		}
	}
}
