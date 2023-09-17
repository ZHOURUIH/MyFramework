using UnityEngine;
using System.Collections.Generic;
using System;
using static StringUtility;

// 游戏常量定义-------------------------------------------------------------------------------------------------------------
public class FrameDefine
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 路径定义
	// 文件夹名
	public const string ASSETS = "Assets";
	public const string GAME_RESOURCES = "GameResources";
	public const string HOT_FIX = "HotFix";
	public const string SCRIPTS = "Scripts";
	public const string PLUGINS = "Plugins";
	public const string RESOURCES = "Resources";
	public const string ATLAS = "Atlas";
	public const string FONT = "Font";
	public const string KEY_FRAME = "KeyFrame";
	public const string LOWER_KEY_FRAME = "keyframe";
	public const string LAYOUT = "Layout";
	public const string LOWER_LAYOUT = "layout";
	public const string SCENE = "Scene";
	public const string SHADER = "Shader";
	public const string GAME = "Game";
	public const string FRAME = "Frame";
	public const string SKYBOX = "Skybox";
	public const string SOUND = "Sound";
	public const string GAME_SOUND = "GameSound";
	public const string MATERIAL = "Material";
	public const string TEXTURE = "Texture";
	public const string GAME_ATLAS = "GameAtlas";
	public const string GAME_TEXTURE = "GameTexture";
	public const string NUMBER_STYLE = "NumberStyle";
	public const string TEXTURE_ANIM = "TextureAnim";
	public const string UGUI_SUB_PREFAB = "UGUISubPrefab";
	public const string UGUI_PREFAB = "UGUIPrefab";
	public const string PREFAB = "Prefab";
	public const string EXCEL = "Excel";
	public const string SQLITE = "SQLite";
	public const string EFFECT = "Effect";
	public const string CHARACTER = "Character";
	// 安卓真机上没有STREAMING_ASSETS的定义
#if UNITY_EDITOR
	public const string STREAMING_ASSETS = "StreamingAssets";
#elif UNITY_IOS
	public const string STREAMING_ASSETS = "Raw";
#elif !UNITY_ANDROID
	public const string STREAMING_ASSETS = "StreamingAssets";
#endif
	public const string VIDEO = "Video";
	public const string PARTICLE = "Particle";
	public const string CUSTOM_SOUND = "CustomSound";
	public const string MODEL = "Model";
	public const string GAME_PLUGIN = "GamePlugin";
	public const string PATH_KEYFRAME = "PathKeyFrame";
	public const string ILRUNTIME = "ILRuntime";
	public const string UI = "UI";
	public const string SCRIPT = "Script";
	public const string ANDROID = "Android";
	public const string WINDOWS = "Windows";
	public const string IOS = "iOS";
	public const string MACOS = "MacOS";
	public const string A_GAME_RESOURCES_PATH = GAME_RESOURCES + "/";
	// 相对路径,相对于项目,以P_开头,表示Project,一般以Assets开头
	public const string P_ASSETS_PATH = ASSETS + "/";
	public const string P_HOT_FIX_PATH = HOT_FIX + "/";
	public const string P_SCRIPTS_PATH = P_ASSETS_PATH + SCRIPTS + "/";
	public const string P_SCRIPTS_FRAME_PATH = P_SCRIPTS_PATH + FRAME + "/";
	public const string P_SCRIPTS_GAME_PATH = P_SCRIPTS_PATH + GAME + "/";
	public const string P_SCRIPTS_ILRUNTIME_PATH = P_SCRIPTS_GAME_PATH + ILRUNTIME + "/";
	public const string P_GAME_RESOURCES_PATH = P_ASSETS_PATH + GAME_RESOURCES + "/";
	public const string P_RESOURCES_PATH = P_ASSETS_PATH + RESOURCES + "/";
	public const string P_RESOURCES_SCENE_PATH = P_RESOURCES_PATH + SCENE + "/";
	public const string P_RESOURCES_ATLAS_PATH = P_RESOURCES_PATH + ATLAS + "/";
	public const string P_RESOURCES_TEXTURE_PATH = P_RESOURCES_PATH + TEXTURE + "/";

#if UNITY_EDITOR
	public const string P_STREAMING_ASSETS_PATH = P_ASSETS_PATH + STREAMING_ASSETS + "/";
	public const string P_ASSET_BUNDLE_ANDROID_PATH = P_STREAMING_ASSETS_PATH + ANDROID + "/";
	public const string P_ASSET_BUNDLE_WINDOWS_PATH = P_STREAMING_ASSETS_PATH + WINDOWS + "/";
	public const string P_ASSET_BUNDLE_IOS_PATH = P_STREAMING_ASSETS_PATH + IOS + "/";
	public const string P_ASSET_BUNDLE_MACOS_PATH = P_STREAMING_ASSETS_PATH + MACOS + "/";
#if UNITY_ANDROID
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_ANDROID_PATH;
#elif UNITY_STANDALONE_WIN
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_WINDOWS_PATH;
#elif UNITY_IOS
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_IOS_PATH;
#elif UNITY_STANDALONE_OSX
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_MACOS_PATH;
#endif
#endif
	public const string P_ATLAS_PATH = P_GAME_RESOURCES_PATH + ATLAS + "/";
	public const string P_GAME_ATLAS_PATH = P_ATLAS_PATH + GAME_ATLAS + "/";
	public const string P_ATLAS_TEXTURE_ANIM_PATH = P_ATLAS_PATH + TEXTURE_ANIM + "/";
	public const string P_LAYOUT_PATH = P_GAME_RESOURCES_PATH + LAYOUT + "/";
	public const string P_LAYOUT_PREFAB_PATH = P_LAYOUT_PATH + UGUI_PREFAB + "/";
	public const string P_TEXTURE_PATH = P_GAME_RESOURCES_PATH + TEXTURE + "/";
	public const string P_EXCEL_PATH = P_GAME_RESOURCES_PATH + EXCEL + "/";
	public const string P_SQLITE_PATH = P_GAME_RESOURCES_PATH + SQLITE + "/";
	// 相对路径,相对于StreamingAssets,以SA_开头,表示StreamingAssets
	// 由于Android下的StreamingAssets路径不完全以Assets路径开头,与其他平台不一致,所以不定义相对于Asstes的路径
	public const string SA_VIDEO_PATH = VIDEO + "/";
	public const string SA_SQLITE_PATH = SQLITE + "/";
	public const string SA_BUNDLE_KEY_FRAME_PATH = LOWER_KEY_FRAME + "/";
	public const string SA_BUNDLE_LAYOU_PATH = LOWER_LAYOUT + "/";
	public const string SA_CUSTOM_SOUND_PATH = CUSTOM_SOUND + "/";
	public const string SA_GAME_PLUGIN = GAME_PLUGIN + "/";
	public const string SA_SOUND_PATH = SOUND + "/";
	public const string SA_KEY_FRAME_PATH = KEY_FRAME + "/";
	public const string SA_LAYOUT_PATH = LAYOUT + "/";
	public const string SA_PATH_KEYFRAME_PATH = PATH_KEYFRAME + "/";
	// 相对路径,相对于Resources,R_开头,表示Resources
	public const string R_ATLAS_PATH = ATLAS + "/";
	public const string R_ATLAS_GAME_ATLAS_PATH = R_ATLAS_PATH + GAME_ATLAS + "/";
	public const string R_ATLAS_TEXTURE_ANIM_PATH = R_ATLAS_PATH + TEXTURE_ANIM + "/";
	public const string R_ATLAS_NUMBER_STYLE_PATH = R_ATLAS_PATH + NUMBER_STYLE + "/";
	public const string R_SOUND_PATH = SOUND + "/";
	public const string R_FONT_PATH = FONT + "/";
	public const string R_SHADER_PATH = SHADER + "/";
	public const string R_SHADER_FRAME_PATH = R_SHADER_PATH + FRAME + "/";
	public const string R_SHADER_GAME_PATH = R_SHADER_PATH + GAME + "/";
	public const string R_LAYOUT_PATH = LAYOUT + "/";
	public const string R_KEY_FRAME_PATH = KEY_FRAME + "/";
	public const string R_UGUI_SUB_PREFAB_PATH = R_LAYOUT_PATH + UGUI_SUB_PREFAB + "/";
	public const string R_UGUI_PREFAB_PATH = R_LAYOUT_PATH + UGUI_PREFAB + "/";
	public const string R_TEXTURE_PATH = TEXTURE + "/";
	public const string R_GAME_TEXTURE_PATH = R_TEXTURE_PATH + GAME_TEXTURE + "/";
	public const string R_TEXTURE_ANIM_PATH = R_TEXTURE_PATH + TEXTURE_ANIM + "/";
	public const string R_NUMBER_STYLE_PATH = R_TEXTURE_PATH + NUMBER_STYLE + "/";
	public const string R_MATERIAL_PATH = MATERIAL + "/";
	public const string R_PARTICLE_PATH = PARTICLE + "/";
	public const string R_MODEL_PATH = MODEL + "/";
	public const string R_SCENE_PATH = SCENE + "/";
	public const string R_EFFECT_PATH = EFFECT + "/";
	public const string R_CHARACTER_PATH = CHARACTER + "/";
	public const string R_PREFAB_PATH = PREFAB + "/";
	public const string R_EXCEL_PATH = EXCEL + "/";
	public const string R_SQLITE_PATH = SQLITE + "/";
	// 绝对路径,以F_开头,表示Full
	public static string F_PROJECT_PATH = getFilePath(Application.dataPath) + "/";
	public static string F_ASSETS_PATH = Application.dataPath + "/";
	public static string F_SCRIPTS_PATH = F_ASSETS_PATH + SCRIPTS + "/";
	public static string F_SCRIPTS_FRAME_PATH = F_SCRIPTS_PATH + FRAME + "/";
	public static string F_SCRIPTS_GAME_PATH = F_SCRIPTS_PATH + GAME + "/";
	public static string F_SCRIPTS_ILRUNTIME_PATH = F_SCRIPTS_GAME_PATH + ILRUNTIME + "/";
	public static string F_HOT_FIX_PATH = F_PROJECT_PATH + HOT_FIX + "/";
	public static string F_HOT_FIX_GAME_PATH = F_HOT_FIX_PATH + GAME + "/";
	public static string F_HOT_FIX_FRAME_PATH = F_HOT_FIX_PATH + FRAME + "/";
	public static string F_HOT_FIX_UI_PATH = F_HOT_FIX_GAME_PATH + UI + "/";
	public static string F_HOT_FIX_UI_SCRIPT_PATH = F_HOT_FIX_UI_PATH + SCRIPT + "/";
	public static string F_PLUGINS_PATH = F_ASSETS_PATH + PLUGINS + "/";
	public static string F_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
	public static string F_PERSISTENT_ASSETS_PATH = F_PERSISTENT_DATA_PATH + ASSETS + "/";
	public static string F_TEMPORARY_CACHE_PATH = Application.temporaryCachePath + "/";
	public static string F_SCRIPTS_UI_PATH = F_SCRIPTS_GAME_PATH + UI + "/";
	public static string F_SCRIPTS_UI_SCRIPT_PATH = F_SCRIPTS_UI_PATH + SCRIPT + "/";
	// 安卓平台上如果访问StreamingAsset需要使用特殊路径,且与Application.streamingAssetsPath不同
#if UNITY_ANDROID && !UNITY_EDITOR
	public static string F_STREAMING_ASSETS_PATH = Application.dataPath + "!assets/";
#else
	public static string F_STREAMING_ASSETS_PATH = Application.streamingAssetsPath + "/";
#endif
	// 各个平台下的AssetBundle路径不一样,为了避免打包资源时冲突
	public static string F_ASSET_BUNDLE_ANDROID_PATH = F_STREAMING_ASSETS_PATH + ANDROID + "/";
	public static string F_ASSET_BUNDLE_WINDOWS_PATH = F_STREAMING_ASSETS_PATH + WINDOWS + "/";
	public static string F_ASSET_BUNDLE_IOS_PATH = F_STREAMING_ASSETS_PATH + IOS + "/";
	public static string F_ASSET_BUNDLE_MACOS_PATH = F_STREAMING_ASSETS_PATH + MACOS + "/";
#if UNITY_ANDROID
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_ANDROID_PATH;
#elif UNITY_STANDALONE_WIN
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_WINDOWS_PATH;
#elif UNITY_IOS
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_IOS_PATH;
#elif UNITY_STANDALONE_OSX
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_MACOS_PATH;
#endif
	public static string F_GAME_RESOURCES_PATH = F_ASSETS_PATH + GAME_RESOURCES + "/";
	public static string F_RESOURCES_PATH = F_ASSETS_PATH + RESOURCES + "/";
	public static string F_GAME_PLUGIN_PATH = F_STREAMING_ASSETS_PATH + GAME_PLUGIN + "/";
	public static string F_LAYOUT_PATH = F_GAME_RESOURCES_PATH + LAYOUT + "/" + UGUI_PREFAB + "/";
	public static string F_RESOURCES_LAYOUT_PATH = F_RESOURCES_PATH + LAYOUT + "/" ;
	public static string F_RESOURCES_LAYOUT_PREFAB_PATH = F_RESOURCES_LAYOUT_PATH + UGUI_PREFAB + "/";
	public static string F_ATLAS_PATH = F_GAME_RESOURCES_PATH + ATLAS + "/";
	//------------------------------------------------------------------------------------------------------------------------------
	// 常量定义
	// 常量数值定义
	public const long WS_OVERLAPPED = 0x00000000;
	public const long WS_POPUP = 0x80000000;
	public const long WS_CHILD = 0x40000000;
	public const long WS_MINIMIZE = 0x20000000;
	public const long WS_VISIBLE = 0x10000000;
	public const long WS_DISABLED = 0x08000000;
	public const long WS_CLIPSIBLINGS = 0x04000000;
	public const long WS_CLIPCHILDREN = 0x02000000;
	public const long WS_MAXIMIZE = 0x01000000;
	public const long WS_BORDER = 0x00800000;
	public const long WS_DLGFRAME = 0x00400000;
	public const long WS_CAPTION = WS_BORDER | WS_DLGFRAME;
	public const long WS_VSCROLL = 0x00200000;
	public const long WS_HSCROLL = 0x00100000;
	public const long WS_SYSMENU = 0x00080000;
	public const long WS_THICKFRAME = 0x00040000;
	public const long WS_GROUP = 0x00020000;
	public const long WS_TABSTOP = 0x00010000;
	public const long WS_MINIMIZEBOX = 0x00020000;
	public const long WS_MAXIMIZEBOX = 0x00010000;
	public const int GWL_STYLE = -16;
	public const int TCP_RECEIVE_BUFFER = 1024 * 1024;
	public const int TCP_INPUT_BUFFER = 2 * 1024 * 1024;
	public const int CLIENT_MAX_PACKET_SIZE = 256 * 1024;	// 客户端临时缓冲区大小,应该不小于单个消息包最大的大小,256KB
	public const int PACKET_TYPE_SIZE = sizeof(ushort);		// 包头中包体类型占的大小
	public const int PACKET_SEQUENCE_SIZE = sizeof(uint);	// 包头中序列号类型占的大小
	public const int PACKET_CRC_SIZE = sizeof(ushort);		// 消息包中CRC校验码类型占的大小
	public const int ALWAYS_TOP_ORDER = 1000;				// 始终在最上层的布局深度从1000开始
	public const ushort CS_GAME_CORE_MIN = 3000;
	public const ushort CS_GAME_CORE_MAX = 5999;
	public const ushort SC_GAME_CORE_MIN = 6000;
	public const ushort SC_GAME_CORE_MAX = 9999;
	public const ushort CS_GAME_MIN = 10000;
	public const ushort CS_GAME_MAX = 19999;
	public const ushort SC_GAME_MIN = 20000;
	public const ushort SC_GAME_MAX = 29999;
	public const float CLICK_LENGTH = 15.0f;		// 点击距离阈值,当鼠标按下和抬起时的距离不超过该值,则认为是有效点击
	public const float CLICK_TIME = 0.3f;			// 单击时间阈值,从按下到抬起的时间低于该值时才有可能认为是一次单击
	public const float DOUBLE_CLICK_TIME = 0.3f;    // 双击时间阈值,两次单击时间大于0并且小于该值时认为是一次双击
	public const ulong FULL_FIELD_FLAG = 0xFFFFFFFFFFFFFFFF;	// 完全的网络消息标识位
	//------------------------------------------------------------------------------------------------------------------------------
	public const string KEY_FRAME_FILE = R_KEY_FRAME_PATH + "Keyframe.prefab";
	public const string AUDIO_HELPER_FILE = R_PREFAB_PATH + "AudioHelper.prefab";
	public const string ILR_EXPORT = "ILRExport";
	public const string STREAMING_ASSET_FILE = "StreamingAssets.bytes";
	// 后缀名
	public const string DATA_SUFFIX = ".bytes";
	public const string ASSET_BUNDLE_SUFFIX = ".unity3d";
	public const string ILR_FILE = "Game.bytes";
	public const string ILR_PDB_FILE = "GamePDB.bytes";
	public const string START_SCENE = P_RESOURCES_SCENE_PATH + "start.unity";
	// dll插件的后缀名
	public const string DLL_PLUGIN_SUFFIX = ".plugin";
	// 音效所有者类型名,应该与SOUND_OWNER一致
	public static string[] SOUND_OWNER_NAME = new string[] { "Window", "Scene" };
	public const string UGUI_DEFAULT_MATERIAL = "UGUIDefault";
	public const string COMMON_NUMBER_STYLE = "CommonNumber";
	public const string UI_CAMERA = "UICamera";
	public const string BLUR_CAMERA = "BlurCamera";
	public const string UGUI_ROOT = "UGUIRoot";
	public const string MAIN_CAMERA = "MainCamera";
	// 材质名
	public const string MAT_MULTIPLE = "Multiple";
	public const string BUILDIN_UI_MATERIAL = "Default UI Material";
	public const string DEFAULT_MATERIAL = "Default-Material";
	public const string SPRITE_DEFAULT_MATERIAL = "Sprites-Default";
	// 层
	public const string LAYER_UI = "UI";
	public const string LAYER_UI_BLUR = "UIBlur";
	public const string LAYER_DEFAULT = "Default";
	public const string LAYER_UGUI = "UGUI";
	// Tag
	public const string TAG_NO_CLICK = "NoClick";
	// Animator状态机参数名hash,数字后缀表示动画层,暂时只列出了2层动画参数
	public const int ANIMATION_LAYER_COUNT = 2;
	public static int[] ANIMATOR_STATE = new int[ANIMATION_LAYER_COUNT] { Animator.StringToHash("State0"), Animator.StringToHash("State1") };
	public static int[] ANIMATOR_DIRTY = new int[ANIMATION_LAYER_COUNT] { Animator.StringToHash("Dirty0"), Animator.StringToHash("Dirty1") };

	// 以下是可扩展的参数,可以修改为自己项目需要的参数

	// UI的制作标准,所有UI都是按1280*960标准分辨率制作的
	public static int STANDARD_WIDTH = 1280;
	public static int STANDARD_HEIGHT = 960;
	// 清理时需要保留的目录和目录的meta
	public static string[] mKeepFolder = new string[]
	{
		"Version",
		ILR_FILE,
		ILR_PDB_FILE,
		"Video",
		"SQLite",
	};
	// 不需要打包AssetBundle的目录,GameResources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录
	// 已经打包图集,并且没有被引用的图片也不打包AB
	public static string[] mUnPackFolder = new string[]
	{
		"Texture/GameTexture/Unpack/",
		"Unused/",
	};
	// 强制为每个文件单独打AssetBundle的目录,GameResources下的目录,带相对路径
	public static string[] mForceSingleFolder = new string[]
	{
		"Atlas/GameAtlas/",
		"Atlas/Map/Objects/",
		"Atlas/Map/Tiles/",
		"Atlas/Map/TextureAnim/AnimationIcon/",
		"Atlas/Map/TextureAnim/Weapon/",
		"Atlas/Map/TextureAnim/WeaponEffect/",
	};
	// 检测所有代码时需要忽略的目录
	public static string[] IGNORE_SCRIPTS_CHECK = new string[]
	{
		"Game/ILRuntime/GeneratedCLRBinding/",
		"Game/ILRuntime/GeneratedCrossBinding/",
		"Game/ILRuntime/ValueTypeBind/",
		"Game/Bugly/",
	};
	// 检查代码注释时需要忽略的文件
	public static string[] IGNORE_COMMENT = new string[] { };
	// 检查成员变量的初始化时需要忽略的文件
	public static string[] IGNORE_CONSTRUCT_VALUE = new string[]
	{
		"/Socket/",
		"/SQLite/",
	};
	// 检测resetProperty时额外忽略的基类
	public static List<Type> IGNORE_RESETPROPERTY_CLASS = new List<Type>() { };
	// 检测布局脚本的代码时需要忽略的脚本
	public static List<string> IGNORE_LAYOUT_SCRIPT = new List<string>()
	{
		"ScriptLegendBase",
	};
	// 检测代码行长度时额外忽略的文件
	public static string[] IGNORE_CODE_WIDTH = new string[]
	{
		"/PacketRegister.cs",
		"/MyStringBuilder.cs",
		"/ToolAudio.cs",
		"/ToolLayout.cs",
		"/ToolFrame.cs",
		"/ToolObject.cs",
		"/StateRegister.cs",
	};
	// 检测命名规范时额外忽略的文件
	public static string[] IGNORE_VARIABLE_CHECK = new string[]
	{
		"/EventTriggerListener.cs",
		"/GaussianBlur.cs",
	};
	// 检查命名规范时需要忽略的类名包含如下字符串的类
	public static string[] IGNORE_CHECK_CLASS = new string[]
	{
		"SCJsonHttp",
		"CSJsonHttp",
		"JsonUDPData",
	};
	// 检查函数命名规范需要忽略的文件
	public static string[] IGNORE_FILE_CHECK_FUNCTION = new string[]
	{
		"/StringUtility.cs",
		"/MathUtility.cs",
	};
	// 检查函数命名规范需要忽略的函数名
	public static string[] IGNORE_CHECK_FUNCTION = new string[]
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
	};
	// 检查内置函数调用时需要忽略的文件
	public static string[] IGNORE_SYSTEM_FUNCTION_CHECK = new string[]
	{
		"GameUtilityILR.cs",
	};
}