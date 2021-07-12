using UnityEngine;
using System.Collections.Generic;

// 游戏常量定义-------------------------------------------------------------------------------------------------------------
public class FrameDefine
{
	//----------------------------------------------------------------------------------------------------------------------
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
	public const string LAYOUT_PREFAB = "Layout";
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
	public const string EXCEL = "Excel";
	public const string EFFECT = "Effect";
	public const string CHARACTER = "Character";
#if UNITY_IPHONE
	public const string STREAMING_ASSETS = "Raw";
#elif !UNITY_ANDROID || UNITY_EDITOR
	public const string STREAMING_ASSETS = "StreamingAssets";
#endif
	public const string CONFIG = "Config";
	public const string VIDEO = "Video";
	public const string PARTICLE = "Particle";
	public const string HELPER_EXE = "HelperExe";
	public const string CUSTOM_SOUND = "CustomSound";
	public const string DATA_BASE = "DataBase";
	public const string MODEL = "Model";
	public const string GAME_PLUGIN = "GamePlugin";
	public const string PATH_KEY_FRAME = "PathKeyframe";
	public const string ILRUNTIME = "ILRuntime";
	public const string A_GAME_RESOURCES_PATH = GAME_RESOURCES + "/";
	// 相对路径,相对于项目,以P_开头,表示Project
	public const string P_ASSETS_PATH = ASSETS + "/";
	public const string P_SCRIPTS_PATH = P_ASSETS_PATH + SCRIPTS + "/";
	public const string P_SCRIPTS_FRAME_PATH = P_SCRIPTS_PATH + FRAME + "/";
	public const string P_SCRIPTS_GAME_PATH = P_SCRIPTS_PATH + GAME + "/";
	public const string P_SCRIPTS_ILRUNTIME_PATH = P_SCRIPTS_GAME_PATH + ILRUNTIME + "/";
	public const string P_GAME_RESOURCES_PATH = P_ASSETS_PATH + GAME_RESOURCES + "/";
	public const string P_RESOURCES_PATH = P_ASSETS_PATH + RESOURCES + "/";
	public const string P_RESOURCES_SCENE_PATH = P_RESOURCES_PATH + SCENE + "/";
#if !UNITY_ANDROID || UNITY_EDITOR
	public const string P_STREAMING_ASSETS_PATH = P_ASSETS_PATH + STREAMING_ASSETS + "/";
#endif
	public const string P_ATLAS_PATH = P_GAME_RESOURCES_PATH + ATLAS + "/";
	public const string P_GAME_ATLAS_PATH = P_ATLAS_PATH + GAME_ATLAS + "/";
	public const string P_ATLAS_TEXTURE_ANIM_PATH = P_ATLAS_PATH + TEXTURE_ANIM + "/";
	// 相对路径,相对于StreamingAssets,以SA_开头,表示StreamingAssets
	// 由于Android下的StreamingAssets路径不完全以Assets路径开头,与其他平台不一致,所以不定义相对于Asstes的路径
	public const string SA_CONFIG_PATH = CONFIG + "/";
	public const string SA_VIDEO_PATH = VIDEO + "/";
	public const string SA_DATA_BASE_PATH = DATA_BASE + "/";
	public const string SA_BUNDLE_KEY_FRAME_PATH = LOWER_KEY_FRAME + "/";
	public const string SA_BUNDLE_LAYOU_PATH = LOWER_LAYOUT + "/";
	public const string SA_CUSTOM_SOUND_PATH = CUSTOM_SOUND + "/";
	public const string SA_GAME_PLUGIN = GAME_PLUGIN + "/";
	public const string SA_SOUND_PATH = SOUND + "/";
	public const string SA_KEY_FRAME_PATH = KEY_FRAME + "/";
	public const string SA_LAYOUT_PATH = LAYOUT_PREFAB + "/";
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
	public const string R_LAYOUT_PATH = LAYOUT_PREFAB + "/";
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
	// 绝对路径,以F_开头,表示Full
	public static string F_PROJECT_PATH = StringUtility.getFilePath(Application.dataPath) + "/";
	public static string F_ASSETS_PATH = Application.dataPath + "/";
	// 安卓平台上如果访问StreamingAsset下的AssetBundle需要使用特殊路径,且与Application.streamingAssetsPath不同
#if UNITY_ANDROID && !UNITY_EDITOR
	public static string F_ASSET_BUNDLE_PATH = Application.dataPath + "!assets/";
#endif
	public static string F_SCRIPTS_PATH = F_ASSETS_PATH + SCRIPTS + "/";
	public static string F_SCRIPTS_FRAME_PATH = F_SCRIPTS_PATH + FRAME + "/";
	public static string F_SCRIPTS_GAME_PATH = F_SCRIPTS_PATH + GAME + "/";
	public static string F_SCRIPTS_ILRUNTIME_PATH = F_SCRIPTS_GAME_PATH + ILRUNTIME + "/";
	public static string F_HOT_FIX_PATH = F_PROJECT_PATH + HOT_FIX + "/";
	public static string F_HOT_FIX_GAME_PATH = F_HOT_FIX_PATH + GAME + "/";
	public static string F_PLUGINS_PATH = F_ASSETS_PATH + PLUGINS + "/";
	public static string F_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
	public static string F_TEMPORARY_CACHE_PATH = Application.temporaryCachePath + "/";
	public static string F_STREAMING_ASSETS_PATH = Application.streamingAssetsPath + "/";
	public static string F_GAME_RESOURCES_PATH = F_ASSETS_PATH + GAME_RESOURCES + "/";
	public static string F_RES_ATLAS_PATH = F_GAME_RESOURCES_PATH + ATLAS + "/";
	public static string F_RES_GAME_ATLAS_PATH = F_RES_ATLAS_PATH + GAME_ATLAS + "/";
	public static string F_RES_ATLAS_TEXTURE_ANIM_PATH = F_RES_ATLAS_PATH + TEXTURE_ANIM + "/";
	public static string F_RES_TEXTURE_PATH = F_GAME_RESOURCES_PATH + TEXTURE + "/";
	public static string F_RES_TEXTURE_GAME_TEXTURE_PATH = F_RES_TEXTURE_PATH + GAME_TEXTURE + "/";
	public static string F_RES_TEXTURE_ANIM_PATH = F_RES_TEXTURE_PATH + TEXTURE_ANIM + "/";
#if !UNITY_EDITOR && UNITY_ANDROID
	public static string F_PATH_KEYFRAME_PATH = F_PERSISTENT_DATA_PATH + PATH_KEY_FRAME + "/";
	public static string F_DATA_BASE_PATH = F_PERSISTENT_DATA_PATH + DATA_BASE + "/";
	public static string F_EXCEL_PATH = F_PERSISTENT_DATA_PATH + EXCEL + "/";
	public static string F_GAME_PLUGIN_PATH = F_PERSISTENT_DATA_PATH + GAME_PLUGIN + "/";
	public static string F_CUSTOM_SOUND_PATH = F_PERSISTENT_DATA_PATH + CUSTOM_SOUND + "/";
	public static string F_HELPER_EXE_PATH = F_PERSISTENT_DATA_PATH + HELPER_EXE + "/";
	public static string F_CONFIG_PATH = F_PERSISTENT_DATA_PATH + CONFIG + "/";
	public static string F_VIDEO_PATH = F_PERSISTENT_DATA_PATH + VIDEO + "/";
#else
	public static string F_PATH_KEYFRAME_PATH = F_STREAMING_ASSETS_PATH + PATH_KEY_FRAME + "/";
	public static string F_DATA_BASE_PATH = F_STREAMING_ASSETS_PATH + DATA_BASE + "/";
	public static string F_EXCEL_PATH = F_STREAMING_ASSETS_PATH + EXCEL + "/";
	public static string F_GAME_PLUGIN_PATH = F_STREAMING_ASSETS_PATH + GAME_PLUGIN + "/";
	public static string F_CUSTOM_SOUND_PATH = F_STREAMING_ASSETS_PATH + CUSTOM_SOUND + "/";
	public static string F_HELPER_EXE_PATH = F_STREAMING_ASSETS_PATH + HELPER_EXE + "/";
	public static string F_CONFIG_PATH = F_STREAMING_ASSETS_PATH + CONFIG + "/";
	public static string F_VIDEO_PATH = F_STREAMING_ASSETS_PATH + VIDEO + "/";
#endif
#if UNITY_EDITOR
	public static string FULL_RESOURCE_PATH = F_STREAMING_ASSETS_PATH;
#elif UNITY_STANDALONE_WIN
	public static string FULL_RESOURCE_PATH = F_STREAMING_ASSETS_PATH;
#elif UNITY_ANDROID
	public static string FULL_RESOURCE_PATH = F_PERSISTENT_DATA_PATH;
#elif UNITY_STANDALONE_OSX
	public static string FULL_RESOURCE_PATH = F_STREAMING_ASSETS_PATH;
#elif UNITY_IPHONE
	public static string FULL_RESOURCE_PATH = F_PERSISTENT_DATA_PATH;
#endif
	//-----------------------------------------------------------------------------------------------------------------
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
	public const int ALWAYS_TOP_ORDER = 1000;			// 始终在最上层的布局深度从1000开始
	public const float CLICK_THRESHOLD = 15.0f;			// 点击阈值,当鼠标按下和抬起时的距离不超过该值,则认为是有效点击
	public const float DOUBLE_CLICK_THRESHOLD = 0.3f;   // 双击时间阈值,两次单击时间大于0并且小于该值时认为是一次双击
	//-----------------------------------------------------------------------------------------------------------------
	public const string KEY_FRAME_FILE = R_KEY_FRAME_PATH + "Keyframe";
	public const string ILR_EXPORT = "ILRExport";
	// 后缀名
	public const string DATA_SUFFIX = ".bytes";
	public const string ASSET_BUNDLE_SUFFIX = ".unity3d";
	public const string ILR_FILE = "Game.bytes";
	public const string ILR_PDB_FILE = "GamePDB.bytes";
	public const string STREAMING_ASSETS_VERSION = "StreamingAssets.version";
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
	// 层
	public const string LAYER_UI = "UI";
	public const string LAYER_UI_BLUR = "UIBlur";
	public const string LAYER_DEFAULT = "Default";
	public const string LAYER_UGUI = "UGUI";
}