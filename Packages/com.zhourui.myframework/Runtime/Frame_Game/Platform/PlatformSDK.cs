using UnityEngine;
using static FrameBaseUtility;
#if BYTE_DANCE
using TTSDK;
#endif

public class PlatformSDK
{
	public static void initSDK()
	{
#if BYTE_DANCE
		TT.InitSDK((int code, ContainerEnv env) =>
		{
			if (code != 0)
			{
				logWarningBase("抖音 SDK 初始化返回:" + code);
			}
		});
#endif
	}
	public static bool isMobile()
	{
#if OPPO_MINI_GAME || VIVO_MINI_GAME
		return true;
#elif UNITY_WEBGL && !UNITY_EDITOR
		return NativePlatform_IsMobile() != 0;
#else
		return Application.isMobilePlatform;
#endif
	}
#if UNITY_WEBGL && !UNITY_EDITOR
	//------------------------------------------------------------------------------------------------------------------------------
	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern int NativePlatform_IsMobile();
#endif
}
