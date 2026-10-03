using UnityEngine;
using static FrameBaseDefine;
using static FrameBaseUtility;

public class MiniGameSettings : ScriptableObject
{
	public const string RESOURCE_PATH = "Settings/MiniGameSettings";
	[Header("抖音小游戏")]
	public string ByteDancePackageName = "";
	public string ByteDanceAppID = "";
	public string ByteDanceRewardedVideoID = "";
	public string ByteDanceInterstitialID = "";
	[Header("Oppo快游戏")]
	public string OppoPackageName = "";
	public string OppoAppID = "";
	public string OppoRewardedVideoID = "";
	public string OppoInterstitialID = "";
	[Header("Vivo快游戏")]
	public string VivoPackageName = "";
	public string VivoAppID = "";
	public string VivoRewardedVideoID = "";
	public string VivoInterstitialID = "";
	[Header("微信小游戏")]
	public string WeXinPackageName = "";
	public string WeXinAppID = "";
	public string WeXinRewardedVideoID = "";
	public string WeXinInterstitialID = "";
	protected static MiniGameSettings mPlatformSettings;
	public static MiniGameSettings get()
	{
		if (mPlatformSettings != null)
		{
			return mPlatformSettings;
		}
		mPlatformSettings = Resources.Load<MiniGameSettings>(RESOURCE_PATH);
		if (mPlatformSettings != null)
		{
			return mPlatformSettings;
		}
		logErrorBase("未找到平台配置: " + P_RESOURCES_PATH + RESOURCE_PATH + ".asset");
		return CreateInstance<MiniGameSettings>();
	}
	public static string getAppID()
	{
		MiniGameSettings settings = get();
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
		MiniGameSettings settings = get();
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
		MiniGameSettings settings = get();
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
	public static string getPackageName()
	{
		MiniGameSettings settings = get();
		if (isByteDance())
		{
			return settings.ByteDancePackageName;
		}
		else if (isOppo())
		{
			return settings.OppoPackageName;
		}
		else if (isVivo())
		{
			return settings.VivoPackageName;
		}
		else
		{
			return "";
		}
	}
}
