#if (OPPO_MINI_GAME && VIVO_MINI_GAME) || ((OPPO_MINI_GAME || VIVO_MINI_GAME) && (BYTE_DANCE || UNITY_WEIXINMINIGAME))
#error A build must select exactly one mini game platform.
#endif

public class PlatformConfig
{
	public const string BYTE_DANCE_APP_ID = "";
	public const string OPPO_APP_ID = "";
	public const string VIVO_APP_ID = "";
	public const string BYTE_DANCE_REWARDED_VIDEO_ID = "";
	public const string OPPO_REWARDED_VIDEO_ID = "";
	public const string VIVO_REWARDED_VIDEO_ID = "";
	public const string BYTE_DANCE_INTERSTITIAL_ID = "";
	public const string OPPO_INTERSTITIAL_ID = "";
	public const string VIVO_INTERSTITIAL_ID = "";
	public static string getAppID()
	{
#if BYTE_DANCE
		return BYTE_DANCE_APP_ID;
#elif OPPO_MINI_GAME
		return OPPO_APP_ID;
#elif VIVO_MINI_GAME
		return VIVO_APP_ID;
#else
		return "";
#endif
	}
	public static string getRewardedVideoID()
	{
#if BYTE_DANCE
		return BYTE_DANCE_REWARDED_VIDEO_ID;
#elif OPPO_MINI_GAME
		return OPPO_REWARDED_VIDEO_ID;
#elif VIVO_MINI_GAME
		return VIVO_REWARDED_VIDEO_ID;
#else
		return "";
#endif
	}
	public static string getInterstitialID()
	{
#if BYTE_DANCE
		return BYTE_DANCE_INTERSTITIAL_ID;
#elif OPPO_MINI_GAME
		return OPPO_INTERSTITIAL_ID;
#elif VIVO_MINI_GAME
		return VIVO_INTERSTITIAL_ID;
#else
		return "";
#endif
	}
	public static string getPlatformDefine()
	{
#if BYTE_DANCE
		return "BYTE_DANCE";
#elif OPPO_MINI_GAME
		return "OPPO_MINI_GAME";
#elif VIVO_MINI_GAME
		return "VIVO_MINI_GAME";
#elif UNITY_WEIXINMINIGAME
		return "UNITY_WEIXINMINIGAME";
#else
		return "";
#endif
	}
	public static string getResourceFolder(string gameName)
	{
#if BYTE_DANCE
		return gameName + "_ByteDance/";
#elif OPPO_MINI_GAME
		return gameName + "_OPPO/";
#elif VIVO_MINI_GAME
		return gameName + "_VIVO/";
#elif UNITY_WEIXINMINIGAME
		return gameName + "_WeChat/";
#else
		return "";
#endif
	}
}
