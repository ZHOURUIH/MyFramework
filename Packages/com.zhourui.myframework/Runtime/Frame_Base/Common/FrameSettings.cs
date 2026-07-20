using System.Collections.Generic;
using UnityEngine;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 运行时可读取的框架设置资源
public class FrameSettings : ScriptableObject
{
    [Tooltip("桌面端UI标准分辨率宽高,根据此设置来决定UI的适配")]
    public Vector2Int UISizeStandalone = new(1920, 1080);
    [Tooltip("移动端UI标准分辨率宽高,根据此设置来决定UI的适配")]
    public Vector2Int UISizeMobile = new(1920, 1080);
	[Tooltip("允许动态下载的目录列表,GameResources下的相对路径,此列表中的文件不会打包到包体中,也不会在游戏启动时从服务器下载,而是在加载资源时才会进行下载")]
	public List<string> DynamicDownloadList = new();
	[Tooltip("安卓插件的包名,也就是自己的安卓工程代码中定义的包名,用于在C#中访问java代码")]
	public string AndroidPluginBundleName = "com.your.packagename";

	private static FrameSettings mFrameSettings;                    // 当前运行时设置
    private static FrameSettings get()
    {
        if (mFrameSettings != null)
        {
            return mFrameSettings;
        }

        string suffix = ".asset";
        mFrameSettings = Resources.Load<FrameSettings>(RUNTIME_SETTINGS_RES_PATH[..^suffix.Length]);
        if (mFrameSettings != null)
        {
            return mFrameSettings;
        }

        Debug.LogError("未找到运行时框架设置:" + P_RESOURCES_PATH + RUNTIME_SETTINGS_RES_PATH);
        mFrameSettings = CreateInstance<FrameSettings>();
        return mFrameSettings;
    }
    public static Vector2Int getUISize()
    {
        if (isMobile())
        {
            return get().UISizeMobile;
        }
        else
        {
            return get().UISizeStandalone;
        }
    }
    public static List<string> getDynamicDownloadList() { return get().DynamicDownloadList; }
    public static string getAndroidPluginBundleName() { return get().AndroidPluginBundleName; }
}