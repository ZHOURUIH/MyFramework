#if (OPPO_MINI_GAME && VIVO_MINI_GAME) || ((OPPO_MINI_GAME || VIVO_MINI_GAME) && (BYTE_DANCE || UNITY_WEIXINMINIGAME))
#error A build must select exactly one mini game platform.
#endif

public class PlatformConfig
{
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
