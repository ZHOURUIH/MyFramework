using UnityEngine;
using static FrameBaseDefine;
public class PlatformSettings : ScriptableObject
{
	public const string RESOURCE_PATH = "Settings/PlatformSettings";
	[Header("抖音")]
	public string ByteDanceAppID = "";
	public string ByteDanceRewardedVideoID = "";
	public string ByteDanceInterstitialID = "";
	[Tooltip("资源目录支持 {GameName} 代表游戏名称，也可以直接填写完整目录")]
	public string ByteDanceResourceFolder = "{GameName}_ByteDance/";
	[Header("OPPO")]
	public string OppoAppID = "";
	public string OppoRewardedVideoID = "";
	public string OppoInterstitialID = "";
	[Tooltip("资源目录支持 {GameName} 代表游戏名称，也可以直接填写完整目录")]
	public string OppoResourceFolder = "{GameName}_OPPO/";
	[Header("vivo")]
	public string VivoAppID = "";
	public string VivoRewardedVideoID = "";
	public string VivoInterstitialID = "";
	[Tooltip("资源目录支持 {GameName} 代表游戏名称，也可以直接填写完整目录")]
	public string VivoResourceFolder = "{GameName}_VIVO/";
	[Header("微信")]
	[Tooltip("资源目录支持 {GameName} 代表游戏名称，也可以直接填写完整目录")]
	public string WeChatResourceFolder = "{GameName}_WeChat/";
	[Header("其他平台")]
	[Tooltip("资源目录支持 {GameName} 代表游戏名称，也可以直接填写完整目录")]
	public string DefaultResourceFolder = "{GameName}";
	protected static PlatformSettings mPlatformSettings;
	public static PlatformSettings get()
	{
		if (mPlatformSettings != null)
		{
			return mPlatformSettings;
		}
		mPlatformSettings = Resources.Load<PlatformSettings>(RESOURCE_PATH);
		if (mPlatformSettings != null)
		{
			return mPlatformSettings;
		}
		Debug.LogError("未找到平台配置: " + P_RESOURCES_PATH + RESOURCE_PATH + ".asset");
		return CreateInstance<PlatformSettings>();
	}
}
