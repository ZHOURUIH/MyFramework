using UnityEngine;
using static FrameBaseUtility;
using static FrameBaseDefine;

// 运行时可读取的框架设置资源
public class FrameSettings : ScriptableObject
{
    [Tooltip("桌面端UI标准分辨率宽高,根据此设置来决定UI的适配")]
    public Vector2Int UISizeStandalone = new(1920, 1080);
    [Tooltip("移动端UI标准分辨率宽高,根据此设置来决定UI的适配")]
    public Vector2Int UISizeMobile = new(1920, 1080);

    private static FrameSettings mFrameSettings;                                        // 当前运行时设置
    private static FrameSettings get()
    {
        if (mFrameSettings != null)
        {
            return mFrameSettings;
        }

        mFrameSettings = Resources.Load<FrameSettings>(RUNTIME_SETTINGS_RES_PATH);
        if (mFrameSettings != null)
        {
            return mFrameSettings;
        }

        Debug.LogError("未找到运行时框架设置:" + FrameBaseDefine.P_RESOURCES_PATH + RUNTIME_SETTINGS_RES_PATH);
        mFrameSettings = new();
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
}