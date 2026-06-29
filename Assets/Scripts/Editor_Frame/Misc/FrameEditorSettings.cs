using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;

// 框架的设置
public class FrameEditorSettings : ScriptableObject
{
    [Tooltip("不开启mipmaps的目录")]
    public List<string> NoMipmapsPath = new();
    [Tooltip("清理时需要保留的目录和目录的meta")]
    public List<string> KeepFolder = new() { "Version" };
    [Tooltip("不需要打包AssetBundle的目录,GameResources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录,已经打包图集,并且没有被引用的图片也不打包AB")]
    public List<string> UnpackFolder = new();
    [Tooltip("强制为每个文件单独打AssetBundle的目录,GameResources下的目录,带相对路径")]
    public List<string> ForceSingleFolder = new();
    [Tooltip("检测所有代码时需要忽略的目录")]
    public List<string> IgnoreScriptCheck = new();
    [Tooltip("检查代码注释时需要忽略的文件")]
    public List<string> IgnoreComment = new();
    [Tooltip("检查成员变量的初始化时需要忽略的文件")]
    public List<string> IgnoreConstructValue = new();
    [Tooltip("检测布局脚本的代码时需要忽略的脚本")]
    public List<string> IgnoreLayoutScript = new();
    [Tooltip("检测代码行长度时额外忽略的文件")]
    public List<string> IgnoreCodeWidth = new()
    {
        "/MyStringBuilder.cs",
        "/ToolAudio.cs",
        "/ToolLayout.cs",
        "/ToolFrame.cs",
        "/ToolObject.cs",
        "/Kernel32.cs",
        "/ImageXBR4.cs",
        "/NetConnectTCPFrame.cs",
        "/BinaryUtility.cs",
        "/TimeUtility.cs",
        "/UnityUtility.cs",
        "/WidgetUtility.cs",
        "/StringUtility.cs",
    };
    [Tooltip("检测命名规范时额外忽略的文件")]
    public List<string> IgnoreVariableCheck = new()
    {
        "/EventTriggerListener.cs",
        "/GaussianBlur.cs",
        "/SafeFloat.cs",
        "/SafeInt.cs",
        "/SafeLong.cs",
        "/MostSafeFloat.cs",
        "/MostSafeInt.cs",
        "/MostSafeLong.cs",
    };
    [Tooltip("检查命名规范时需要忽略的类名包含如下字符串的类")]
    public List<string> IgnoreCheckClass = new();
    [Tooltip("检查函数命名规范需要忽略的文件")]
    public List<string> IgnoreFileCheckFunction = new()
    {
        "/StringUtility.cs",
        "/MathUtility.cs",
        "/Utility/Struct/",
        "/GameFramework.cs",
        "/Serialize/",
        "/MyEmptyDictionary.cs",
        "/PurchasingSystem.cs",
        "/UIGradient.cs",
    };
    [Tooltip("检查函数命名规范需要忽略的函数名")]
    public List<string> IgnoreCheckFunction = new()
    {
        "Reset",
        "Awake",
        "OnEnable",
        "Start",
        "FixedUpdate",
        "Update",
        "LateUpdate",
        "OnGUI",
        "OnDisable",
        "OnDestroy",
        "OnDrawGizmos",
        "DrawGizmos",
        "OnApplicationQuit",
        "OnValidate",
        "GetHashCode",
        "ToString",
        "Equals",
        "OnTriggerEnter",
        "OnTriggerStay",
        "OnTriggerExit",
        "OnCollisionEnter",
        "OnCollisionStay",
        "OnCollisionExit",
        "OnPointerDown",
        "OnPointerEnter",
        "OnPointerUp",
        "OnPointerClick",
        "OnStateEnter",
        "OnStateUpdate",
        "OnStateExit",
        "OnPopulateMesh",
        "SetVerticesDirty",
        "Typeof",
        "CompareTo",
        "Dispose",
    };
    [Tooltip("检查内置函数调用时需要忽略的文件")]
    public List<string> IgnoreSystemFunctionCheck = new();
    [Tooltip("检查是否有适配组件时需要忽略的prefab,不含路径,带后缀名")]
    public List<string> IgnoreScaleAnchorCheck = new();

    private const string SETTINGS_PATH = "ProjectSettings/MyFrameworkSettings.asset";
    private static FrameEditorSettings mInstance;
    public static FrameEditorSettings getInstance()
    {
        if (mInstance == null)
        {
            Object[] arr = InternalEditorUtility.LoadSerializedFileAndForget(SETTINGS_PATH);
            if (arr != null && arr.Length > 0 && arr[0] is FrameEditorSettings settings)
            {
                mInstance = settings;
            }
            else
            {
                mInstance = mInstance != null ? mInstance : CreateInstance<FrameEditorSettings>();
            }
        }
        return mInstance;
    }
    public static void save()
    {
        if (mInstance == null)
        {
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(SETTINGS_PATH));
        InternalEditorUtility.SaveToSerializedFileAndForget(new FrameEditorSettings[1] { mInstance }, SETTINGS_PATH, true);
    }
}