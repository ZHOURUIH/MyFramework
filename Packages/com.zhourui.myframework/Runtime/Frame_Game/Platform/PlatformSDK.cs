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
}
