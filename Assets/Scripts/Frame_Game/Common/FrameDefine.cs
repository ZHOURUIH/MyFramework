using UnityEngine;
using static StringUtility;

// 游戏常量定义-------------------------------------------------------------------------------------------------------------
public class FrameDefine
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 路径定义
	// 文件夹名
	public const string ASSETS = "Assets";
	public const string GAME_RESOURCES = "GameResources";
	public const string HOTFIX = "HotFix";
	public const string SCRIPTS = "Scripts";
	public const string PLUGINS = "Plugins";
	public const string RESOURCES = "Resources";
	public const string ATLAS = "Atlas";
	public const string FONT = "Font";
	public const string KEY_FRAME = "KeyFrame";
	public const string LOWER_KEY_FRAME = "keyframe";
	public const string UI = "UI";
	public const string LOWER_UI = "ui";
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
	public const string UI_PREFAB = "UIPrefab";
	public const string MISC = "Misc";
	public const string EXCEL = "Excel";
	public const string SQLITE = "SQLite";
	public const string EFFECT = "Effect";
	public const string CHARACTER = "Character";
	public const string UNUSED = "Unused";
	public const string VIDEO = "Video";
	public const string PARTICLE = "Particle";
	public const string CUSTOM_SOUND = "CustomSound";
	public const string MODEL = "Model";
	public const string GAME_PLUGIN = "GamePlugin";
	public const string PATH_KEYFRAME = "PathKeyFrame";
	public const string SCRIPT = "Script";
	public const string ANDROID = "Android";
	public const string WINDOWS = "Windows";
	public const string IOS = "iOS";
	public const string MACOS = "MacOS";
	public const string A_GAME_RESOURCES_PATH = GAME_RESOURCES + "/";
	// 相对路径,相对于项目,以P_开头,表示Project,一般以Assets开头
	public const string P_ASSETS_PATH = ASSETS + "/";
	public const string P_SCRIPTS_PATH = P_ASSETS_PATH + SCRIPTS + "/";
	public const string P_SCRIPTS_FRAME_PATH = P_SCRIPTS_PATH + FRAME + "/";
	public const string P_SCRIPTS_GAME_PATH = P_SCRIPTS_PATH + GAME + "/";
	public const string P_GAME_RESOURCES_PATH = P_ASSETS_PATH + GAME_RESOURCES + "/";
	public const string P_RESOURCES_PATH = P_ASSETS_PATH + RESOURCES + "/";
	public const string P_RESOURCES_SCENE_PATH = P_RESOURCES_PATH + SCENE + "/";
	public const string P_RESOURCES_ATLAS_PATH = P_RESOURCES_PATH + ATLAS + "/";
	public const string P_RESOURCES_TEXTURE_PATH = P_RESOURCES_PATH + TEXTURE + "/";
	public const string P_RESOURCES_UI_PATH = P_RESOURCES_PATH + UI + "/";
	public const string P_RESOURCES_UI_PREFAB_PATH = P_RESOURCES_UI_PATH + UI_PREFAB + "/";
	public const string P_ATLAS_PATH = P_GAME_RESOURCES_PATH + ATLAS + "/";
	public const string P_GAME_ATLAS_PATH = P_ATLAS_PATH + GAME_ATLAS + "/";
	public const string P_ATLAS_TEXTURE_ANIM_PATH = P_ATLAS_PATH + TEXTURE_ANIM + "/";
	public const string P_UI_PATH = P_GAME_RESOURCES_PATH + UI + "/";
	public const string P_UI_PREFAB_PATH = P_UI_PATH + UI_PREFAB + "/";
	public const string P_TEXTURE_PATH = P_GAME_RESOURCES_PATH + TEXTURE + "/";
	public const string P_EXCEL_PATH = P_GAME_RESOURCES_PATH + EXCEL + "/";
	public const string P_SQLITE_PATH = P_GAME_RESOURCES_PATH + SQLITE + "/";
	public const string P_SCENE_PATH = P_GAME_RESOURCES_PATH + SCENE + "/";
	public const string P_UNUSED_PATH = P_GAME_RESOURCES_PATH + UNUSED + "/";
	// 相对路径,相对于StreamingAssets,以SA_开头,表示StreamingAssets
	// 由于Android下的StreamingAssets路径不完全以Assets路径开头,与其他平台不一致,所以不定义相对于Asstes的路径
	public const string SA_VIDEO_PATH = VIDEO + "/";
	public const string SA_SOUND_PATH = SOUND + "/";
	// 相对路径,相对于Resources,R_开头,表示Resources
	public const string R_UI_PATH = UI + "/";
	public const string R_UI_PREFAB_PATH = R_UI_PATH + UI_PREFAB + "/";
	public const string R_MISC_PATH = MISC + "/";
	// 绝对路径,以F_开头,表示Full
	public static string F_PROJECT_PATH = getFilePath(Application.dataPath) + "/";
	public static string F_ASSETS_PATH = Application.dataPath + "/";
	public static string F_SCRIPTS_PATH = F_ASSETS_PATH + SCRIPTS + "/";
	public static string F_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
	public static string F_PERSISTENT_ASSETS_PATH = F_PERSISTENT_DATA_PATH + ASSETS + "/";
	public static string F_GAME_RESOURCES_PATH = F_ASSETS_PATH + GAME_RESOURCES + "/";
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
	public const int ALWAYS_TOP_ORDER = 1000;		// 始终在最上层的布局深度从1000开始
	public const float CLICK_LENGTH = 15.0f;		// 点击距离阈值,当鼠标按下和抬起时的距离不超过该值,则认为是有效点击
	public const float CLICK_TIME = 0.5f;			// 单击时间阈值,从按下到抬起的时间低于该值时才有可能认为是一次单击
	public static Vector3 FAR_POSITION = new(99999.0f, 99999.0f, 99999.0f);	// 一个很远的位置,用于移动GameObject到远处来实现隐藏
	//------------------------------------------------------------------------------------------------------------------------------
	public const string AUDIO_HELPER_FILE = R_MISC_PATH + "AudioHelper.prefab";
	public const string STREAMING_ASSET_FILE = "StreamingAssets.bytes";
	// 后缀名
	public const string DATA_SUFFIX = ".bytes";
	public const string ASSET_BUNDLE_SUFFIX = ".unity3d";
	public const string HOTFIX_FRAME_FILE = "Frame_HotFix.dll";
	public const string HOTFIX_FILE = "HotFix.dll";
	public const string RESOURCE_AVAILABLE_FILE = "ResourcesAvailable.txt";
	public const string UI_CAMERA = "UICamera";
	public const string UGUI_ROOT = "UGUIRoot";
	public const string MAIN_CAMERA = "MainCamera";
	// 语言名
	public const string LANGUAGE_CHINESE_TRADITIONAL = "ChineseTraditional";            // 中文繁体语言的名字
	public const string LANGUAGE_CHINESE = "Chinese";                                   // 中文简体语言的名字
	public const string LANGUAGE_ENGLISH = "English";                                   // 英文语言的名字

	// Tag
	public const string TAG_NO_CLICK = "NoClick";
}