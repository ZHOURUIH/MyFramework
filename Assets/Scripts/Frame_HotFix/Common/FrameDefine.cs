using UnityEngine;
using static StringUtility;
using static FrameBaseDefine;

// 游戏常量定义-------------------------------------------------------------------------------------------------------------
public class FrameDefine
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 路径定义
	// 文件夹名
	public const string GAME_RESOURCES = "GameResources";
	public const string SCRIPTS = "Scripts";
	public const string PLUGINS = "Plugins";
	public const string ATLAS = "Atlas";
	public const string FONT = "Font";
	public const string KEY_FRAME = "KeyFrame";
	public const string UI = "UI";
	public const string SCENE = "Scene";
	public const string SHADER = "Shader";
	public const string SOUND = "Sound";
	public const string MATERIAL = "Material";
	public const string TEXTURE = "Texture";
	public const string UGUI_SUB_PREFAB = "UGUISubPrefab";
	public const string UI_PREFAB = "UIPrefab";
	public const string MISC = "Misc";
	public const string EXCEL = "Excel";
	public const string SQLITE = "SQLite";
	public const string EFFECT = "Effect";
	public const string UNUSED = "Unused";
	public const string VIDEO = "Video";
	public const string PARTICLE = "Particle";
	public const string GAME_PLUGIN = "GamePlugin";
	public const string PATH_KEYFRAME = "PathKeyFrame";
	public const string SCRIPT = "Script";
	public const string A_GAME_RESOURCES_PATH = GAME_RESOURCES + "/";

	// 相对路径,相对于项目,以P_开头,表示Project,一般以Assets开头
	public const string P_SCRIPTS_PATH = P_ASSETS_PATH + SCRIPTS + "/";
	public const string P_GAME_RESOURCES_PATH = P_ASSETS_PATH + GAME_RESOURCES + "/";
	public const string P_RESOURCES_SCENE_PATH = P_RESOURCES_PATH + SCENE + "/";
	public const string P_RESOURCES_ATLAS_PATH = P_RESOURCES_PATH + ATLAS + "/";
	public const string P_RESOURCES_TEXTURE_PATH = P_RESOURCES_PATH + TEXTURE + "/";
	public const string P_RESOURCES_UI_PATH = P_RESOURCES_PATH + UI + "/";
	public const string P_RESOURCES_UI_PREFAB_PATH = P_RESOURCES_UI_PATH + UI_PREFAB + "/";
	public const string P_ATLAS_PATH = P_GAME_RESOURCES_PATH + ATLAS + "/";
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

	// 相对路径,相对于Resources,R_开头,表示Resources
	public const string R_ATLAS_PATH = ATLAS + "/";
	public const string R_SOUND_PATH = SOUND + "/";
	public const string R_FONT_PATH = FONT + "/";
	public const string R_SHADER_PATH = SHADER + "/";
	public const string R_UI_PATH = UI + "/";
	public const string R_UI_PREFAB_PATH = R_UI_PATH + UI_PREFAB + "/";
	public const string R_KEY_FRAME_PATH = KEY_FRAME + "/";
	public const string R_UGUI_SUB_PREFAB_PATH = R_UI_PATH + UGUI_SUB_PREFAB + "/";
	public const string R_MATERIAL_PATH = MATERIAL + "/";
	public const string R_PARTICLE_PATH = PARTICLE + "/";
	public const string R_SCENE_PATH = SCENE + "/";
	public const string R_EFFECT_PATH = EFFECT + "/";
	public const string R_MISC_PATH = MISC + "/";
	public const string R_EXCEL_PATH = EXCEL + "/";
	public const string R_SQLITE_PATH = SQLITE + "/";
	public const string R_PATH_KEYFRAME_PATH = PATH_KEYFRAME + "/";

	// 绝对路径,以F_开头,表示Full
	public static string F_PROJECT_PATH = getFilePath(Application.dataPath) + "/";
	public static string F_SCRIPTS_PATH = F_ASSETS_PATH + SCRIPTS + "/";
	public static string F_SCRIPTS_HOTFIX_PATH = F_SCRIPTS_PATH + HOTFIX + "/";
	public static string F_SCRIPTS_HOTFIX_UI_PATH = F_SCRIPTS_HOTFIX_PATH + UI + "/";
	public static string F_PLUGINS_PATH = F_ASSETS_PATH + PLUGINS + "/";
	public static string F_GAME_RESOURCES_PATH = F_ASSETS_PATH + GAME_RESOURCES + "/";
	public static string F_RESOURCES_PATH = F_ASSETS_PATH + RESOURCES + "/";
	public static string F_GAME_PLUGIN_PATH = F_STREAMING_ASSETS_PATH + GAME_PLUGIN + "/";
	public static string F_UI_PATH = F_GAME_RESOURCES_PATH + UI + "/";
	public static string F_UI_PREFAB_PATH = F_UI_PATH + UI_PREFAB + "/";
	public static string F_RESORUCES_UI_PATH = F_RESOURCES_PATH + UI + "/";
	public static string F_RESORUCES_UI_PREFAB_PATH = F_UI_PATH + UI_PREFAB + "/";
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
	public const int WEB_SOCKET_RECEIVE_BUFFER = 1024 * 1024;
	public const int CLIENT_MAX_PACKET_SIZE = 256 * 1024;		// 客户端临时缓冲区大小,应该不小于单个消息包最大的大小,256KB
	public const int PACKET_TYPE_SIZE = sizeof(ushort);			// 包头中包体类型占的大小
	public const int PACKET_SEQUENCE_SIZE = sizeof(uint);		// 包头中序列号类型占的大小
	public const int PACKET_CRC_SIZE = sizeof(ushort);			// 消息包中CRC校验码类型占的大小
	public const int ALWAYS_TOP_ORDER = 1000;					// 始终在最上层的布局深度从1000开始
	public const float CLICK_LENGTH = 15.0f;					// 点击距离阈值,当鼠标按下和抬起时的距离不超过该值,则认为是有效点击
	public const float CLICK_TIME = 0.5f;						// 单击时间阈值,从按下到抬起的时间低于该值时才有可能认为是一次单击
	public const float DOUBLE_CLICK_TIME = 0.3f;				// 双击时间阈值,两次单击时间大于0并且小于该值时认为是一次双击
	public const ulong FULL_FIELD_FLAG = 0xFFFFFFFFFFFFFFFF;    // 完全的网络消息标识位
	public static Vector3 FAR_POSITION = new(99999.0f, 99999.0f, 99999.0f);	// 一个很远的位置,用于移动GameObject到远处来实现隐藏
	//------------------------------------------------------------------------------------------------------------------------------
	public const string KEY_FRAME_FILE = R_KEY_FRAME_PATH + "Keyframe.prefab";
	public const string AUDIO_HELPER_FILE = R_MISC_PATH + "AudioHelper.prefab";
	public const string STREAMING_ASSET_FILE = "StreamingAssets.bytes";
	public const string ATLAS_PATH_CONFIG = "AtlasPathConfig.txt";
	// 后缀名
	public const string SPRITE_ATLAS_SUFFIX = ".spriteatlasv2";
	public const string ASSET_BUNDLE_SUFFIX = ".unity3d";
	public const string START_SCENE = P_RESOURCES_SCENE_PATH + "start.unity";
	// dll插件的后缀名
	public const string DLL_PLUGIN_SUFFIX = ".plugin";
	public const string UGUI_DEFAULT_MATERIAL = "UGUIDefault";
	public const string COMMON_NUMBER_STYLE = "CommonNumber";
	public const string UI_CAMERA = "UICamera";
	public const string BLUR_CAMERA = "BlurCamera";
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
	public static int LAYER_INT_UI_BLUR = LayerMask.NameToLayer(LAYER_UI_BLUR);
	public static int LAYER_INT_DEFAULT = LayerMask.NameToLayer(LAYER_DEFAULT);

	// Tag
	public const string TAG_NO_CLICK = "NoClick";
	// Animator状态机参数名hash,数字后缀表示动画层,暂时只列出了2层动画参数
	public const int ANIMATION_LAYER_COUNT = 2;
	public static int[] ANIMATOR_STATE = new int[ANIMATION_LAYER_COUNT] { Animator.StringToHash("State0"), Animator.StringToHash("State1") };
	public static int[] ANIMATOR_DIRTY = new int[ANIMATION_LAYER_COUNT] { Animator.StringToHash("Dirty0"), Animator.StringToHash("Dirty1") };
}