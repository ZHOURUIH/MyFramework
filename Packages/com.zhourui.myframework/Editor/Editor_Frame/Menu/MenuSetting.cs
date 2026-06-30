using UnityEditor;
using UnityEngine;
using static FileUtility;
using static StringUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;
using static EditorDefine;

// 运行时框架设置资源生成工具
public static class MenuSetting
{
    public const string MENU_NAME = MENU_ROOT_NAME + "设置/";
    [MenuItem(MENU_NAME + "打开编辑器设置")]
    public static void openFrameEditorSetting()
    {
        SettingsService.OpenProjectSettings(SETTING_NAME);
    }
    [MenuItem(MENU_NAME + "打开运行时设置")]
    public static void openFrameRuntimeSetting()
    {
        if (!isFileExist(projectPathToFullPath(P_RESOURCES_PATH + RUNTIME_SETTINGS_RES_PATH)))
        {
            createRuntimeSettinsFile();
        }
        pingAsset(P_RESOURCES_PATH + RUNTIME_SETTINGS_RES_PATH);
    }
    public static void createRuntimeSettinsFile()
    {
        string runtimeSettingsPath = P_RESOURCES_PATH + RUNTIME_SETTINGS_RES_PATH;
        createDir(getFilePath(runtimeSettingsPath));
        var asset = AssetDatabase.LoadAssetAtPath<FrameSettings>(runtimeSettingsPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<FrameSettings>();
            AssetDatabase.CreateAsset(asset, runtimeSettingsPath);
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("已生成运行时框架设置: " + runtimeSettingsPath);
    }
}