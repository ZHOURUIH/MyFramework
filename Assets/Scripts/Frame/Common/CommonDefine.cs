using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;

// 游戏枚举定义-----------------------------------------------------------------------------------------------
// 停靠位置
public enum DOCKING_POSITION : byte
{
	DP_LEFT,
	DP_CENTER,
	DP_RIGHT,
	DP_TOP,
	DP_BOTTOM,
}
// 数字的排列方向
public enum NUMBER_DIRECTION : byte
{
	ND_HORIZONTAL,
	ND_VERTICAL,
}
// 循环方式
public enum LOOP_MODE : byte
{
	LM_ONCE,
	LM_LOOP,
	LM_PINGPONG,
}
// 播放状态
public enum PLAY_STATE : byte
{
	PS_NONE,
	PS_PLAY,
	PS_PAUSE,
	PS_STOP,
}

public enum LOAD_STATE : byte
{
	LS_NONE,
	LS_UNLOAD,
	LS_WAIT_FOR_LOAD,
	LS_LOADING,
	LS_LOADED,
}

public enum DRAG_DIRECTION : byte
{
	DD_HORIZONTAL,
	DD_VERTICAL,
	DD_FREE,
}

public enum CLAMP_TYPE : byte
{
	CT_CENTER,  // 限制中心点不能超过父窗口的区域
	CT_EDGE,    // 限制边界不能超过父窗口的区域
}

// 网络状态
public enum NET_STATE : byte
{
	NS_CONNECTED,       // 已连接
	NS_SERVER_CLOSE,    // 服务器已关闭
	NS_NET_CLOSE,       // 网络已断开
}

public enum PARSE_RESULT : byte
{
	PR_SUCCESS,
	PR_ERROR,
	PR_NOT_ENOUGH,
}

// 日志等级
public enum LOG_LEVEL : byte
{
	LL_FORCE,   // 强制显示
	LL_HIGH,    // 高
	LL_NORMAL,  // 正常
	LL_LOW,		// 低
	LL_MAX,
}

public enum CAMERA_LINKER_SWITCH : byte
{
	CLS_NONE,
	CLS_LINEAR,
	CLS_CIRCLE,
	CLS_AROUND_TARGET,
}

public enum CHECK_DIRECTION : byte
{
	CD_DOWN,        // 向下碰撞检测,检测到碰撞后,摄像机向上移动
	CD_UP,
	CD_LEFT,
	CD_RIGHT,
	CD_FORWARD,
	CD_BACK,
}

public enum MOUSE_BUTTON : byte
{
	MB_LEFT,
	MB_RIGHT,
	MB_MIDDLE,
}

public enum FOCUS_MASK : ushort
{
	FM_NONE = 0,
	FM_SCENE = 1 << 1,
	FM_UI = 1 << 2,
	FM_OTHER = 1 << 3,
}

public enum SCROLL_STATE : byte
{
	SS_NONE,            // 无
	SS_SCROLL_TARGET,   // 自动匀速滚动到目标点
	SS_DRAGING,         // 鼠标拖动
	SS_SCROLL_TO_STOP,  // 鼠标抬起后自动减速到停止
}

public enum CONNECT_STATE : byte
{
	CS_NOT_CONNECT,
	CS_CONNECTING,
	CS_CONNECTED,
}

public enum SLIDER_MODE : byte
{
	SM_FILL,    // 通过调整图片填充来实现滑动条
	SM_SIZING,  // 通过调整窗口大小来实现滑动条
}

// 添加同一状态时的操作选项
public enum SAME_STATE_OPERATE : byte
{
	SSO_CAN_NOT_ADD_NEW,    // 不可添加相同的新状态
	SSO_REMOVE_OLD,         // 添加新状态,移除互斥的旧状态
	SSO_COEXIST,            // 新旧状态可共存
	SSO_USE_HIGH_PRIORITY,  // 保留新旧状态中优先级最高的
}

// 同一状态组中的状态互斥选项
public enum GROUP_MUTEX_OPERATION : byte
{
	GMO_COEXIST,        // 各状态可完全共存
	GMO_REMOVE_OTHERS,  // 添加新状态时移除组中的其他所有状态
	GMO_NO_NEW,         // 状态组中有状态时不允许添加新状态
	GMO_MUETX_WITH_MAIN,// 仅与主状态互斥,添加主状态时移除其他所有状态,有主状态时不可添加其他状态,没有主状态时可任意添加其他状态
}

public enum EXECUTE_STATE : byte
{
	ES_NOT_EXECUTE,
	ES_EXECUTING,
	ES_EXECUTED,
}

public enum NODE_STATE : byte
{
	NS_NONE,
	NS_OPEN,
	NS_CLOSE,
}

public enum LOAD_SOURCE : byte
{
	LS_RESOURCES,
	LS_ASSET_BUNDLE,
}

public enum BUFF_STATE_TYPE : byte
{
	BST_NONE,       // 既不属于buff,也不属于debuff
	BST_BUFF,       // 属于buff
	BST_DEBUFF,     // 属于debuff
}

// 时间字符串显示格式
public enum TIME_DISPLAY : byte
{
	TD_HMSM_0,  // 以Hour:Minute:Second:Millisecond形式显示,并且不补0
	TD_HMS_2,   // 以Hour:Minute:Second形式显示,并且每个数都显示为2位数
	TD_DHMS_ZH,	// 以Day天Hour小时Minute分Second秒的形式显示
}

public class LoadMaterialParam : IClassObject
{
	public string mMaterialName;
	public bool mNewMaterial;
	public void resetProperty()
	{
		mMaterialName = null;
		mNewMaterial = false;
	}
}

// 游戏委托定义-------------------------------------------------------------------------------------------------------------
public delegate void TextureAnimCallBack(IUIAnimation window, bool isBreak);
public delegate void KeyFrameCallback(ComponentKeyFrameBase component, bool breakTremling);
public delegate void LerpCallback(ComponentLerp component, bool breakLerp);
public delegate void CommandCallback(Command cmd);
public delegate void AssetBundleLoadCallback(AssetBundleInfo assetBundle, object userData);
public delegate void AssetLoadDoneCallback(UnityEngine.Object asset, UnityEngine.Object[] assets, byte[] bytes, object[] userData, string loadPath);
public delegate void SceneLoadCallback(float progress, bool done);
public delegate void SceneActiveCallback();
public delegate void LayoutAsyncDone(GameLayout layout);
public delegate void VideoCallback(string videoName, bool isBreak);
public delegate void VideoErrorCallback(ErrorCode errorCode);
public delegate void TrackCallback(ComponentTrackTargetBase component, bool breakTrack);
public delegate void OnReceiveDragCallback(IMouseEventCollect dragObj, ref bool continueEvent);
public delegate void OnDragHoverCallback(IMouseEventCollect dragObj, bool hover);
public delegate void OnMouseEnter(IMouseEventCollect obj);
public delegate void OnMouseLeave(IMouseEventCollect obj);
public delegate void OnMouseDown(Vector2 mousePos);
public delegate void OnMouseUp(Vector2 mousePos);
public delegate void OnMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime);
public delegate void OnMouseStay(Vector2 mousePos);
public delegate void OnScreenMouseUp(IMouseEventCollect obj, Vector2 mousePos);
public delegate void OnLongPress();
public delegate void OnLongPressing(float progress);
public delegate void OnMultiTouchStart(Vector2 touch0, Vector2 touch1);
public delegate void OnMultiTouchMove(Vector2 touch0, Vector2 lastPosition0, Vector2 touch1, Vector2 lastPosition1);
public delegate void OnMultiTouchEnd();
public delegate void OnNGUILineGenerated(NGUILine line);
public delegate void HeadDownloadCallback(Texture head, string openID);
public delegate void OnDragViewCallback();
public delegate void OnDragViewStartCallback(ref bool allowDrag);
public delegate void CustomThreadCallback(ref bool run);    // 返回值表示是否继续运行该线程
public delegate void onPlayingCallback(AnimControl control, int frame, bool isPlaying); // isPlaying表示是否是在播放过程中触发的该回调
public delegate void onPlayEndCallback(AnimControl control, bool callback, bool isBreak);
public delegate void OnDraging();
public delegate void OnScrollItem(IScrollItem item, int index);
public delegate void OnDragCallback(ComponentOwner dragObj);
public delegate void OnDragStartCallback(ComponentOwner dragObj, ref bool allowDrag);
public delegate void StartDownloadCallback(string fileName, long totalSize);
public delegate void DownloadingCallback(string fileName, long fileSize, long downloadedSize);
public delegate void ObjectPreClickCallback(txUIObject obj, object userData);
public delegate void ObjectClickCallback(IMouseEventCollect obj);
public delegate void ObjectDoubleClickCallback(IMouseEventCollect obj);
public delegate void ObjectHoverCallback(IMouseEventCollect obj, bool hover);
public delegate void ObjectPressCallback(IMouseEventCollect obj, bool press);
public delegate void SliderCallback();
public delegate void LayoutScriptCallback(LayoutScript script, bool create);
public delegate void OnCharacterLoaded(Character character, object userData);
public delegate void CreateObjectCallback(GameObject go, object userData);
public delegate void CreateObjectGroupCallback(Dictionary<string, GameObject> go, object userData);
public delegate void OnEffectLoadedCallback(GameEffect effect, object userData);
public delegate void AtlasLoadDone(Texture2D atlas, object userData);
public delegate void OnInputField(string str);
public delegate void OnStateLeave(PlayerState state, bool isBreak, string param);

// 游戏常量定义-------------------------------------------------------------------------------------------------------------
public class CommonDefine
{
	// 路径定义
	// 文件夹名
	public const string ASSETS = "Assets";
	public const string GAME_RESOURCES = "GameResources";
	public const string PLUGINS = "Plugins";
	public const string RESOURCES = "Resources";
	public const string ATLAS = "Atlas";
	public const string FONT = "Font";
	public const string GAME_DATA_FILE = "GameDataFile";
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
	public const string NGUI_SUB_PREFAB = "NGUISubPrefab";
	public const string UGUI_SUB_PREFAB = "UGUISubPrefab";
	public const string NGUI_PREFAB = "NGUIPrefab";
	public const string UGUI_PREFAB = "UGUIPrefab";
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
	public const string A_GAME_RESOURCES_PATH = GAME_RESOURCES + "/";
	// 相对路径,相对于项目,以P_开头,表示Project
	public const string P_ASSETS_PATH = ASSETS + "/";
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
	public const string SA_GAME_DATA_FILE_PATH = GAME_DATA_FILE + "/";
	public const string SA_DATA_BASE_PATH = DATA_BASE + "/";
	public const string SA_BUNDLE_KEY_FRAME_PATH = LOWER_KEY_FRAME + "/";
	public const string SA_BUNDLE_LAYOU_PATH = LOWER_LAYOUT + "/";
	public const string SA_CUSTOM_SOUND_PATH = CUSTOM_SOUND + "/";
	public const string SA_BUNDLE_NGUI_SUB_PREFAB_PATH = SA_BUNDLE_LAYOU_PATH + NGUI_SUB_PREFAB + "/";
	public const string SA_GAME_PLUGIN = GAME_PLUGIN + "/";
	public const string SA_SOUND_PATH = SOUND + "/";
	public const string SA_KEY_FRAME_PATH = KEY_FRAME + "/";
	public const string SA_LAYOUT_PATH = LAYOUT_PREFAB + "/";
	public const string SA_NGUI_SUB_PREFAB_PATH = SA_LAYOUT_PATH + NGUI_SUB_PREFAB + "/";
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
	public const string R_NGUI_SUB_PREFAB_PATH = R_LAYOUT_PATH + NGUI_SUB_PREFAB + "/";
	public const string R_UGUI_SUB_PREFAB_PATH = R_LAYOUT_PATH + UGUI_SUB_PREFAB + "/";
	public const string R_NGUI_PREFAB_PATH = R_LAYOUT_PATH + NGUI_PREFAB + "/";
	public const string R_UGUI_PREFAB_PATH = R_LAYOUT_PATH + UGUI_PREFAB + "/";
	public const string R_TEXTURE_PATH = TEXTURE + "/";
	public const string R_GAME_TEXTURE_PATH = R_TEXTURE_PATH + GAME_TEXTURE + "/";
	public const string R_TEXTURE_ANIM_PATH = R_TEXTURE_PATH + TEXTURE_ANIM + "/";
	public const string R_NUMBER_STYLE_PATH = R_TEXTURE_PATH + NUMBER_STYLE + "/";
	public const string R_MATERIAL_PATH = MATERIAL + "/";
	public const string R_PARTICLE_PATH = PARTICLE + "/";
	public const string R_MODEL_PATH = MODEL + "/";
	public const string R_SCENE_PATH = SCENE + "/";
	// 绝对路径,以F_开头,表示Full
	public static string F_ASSETS_PATH = Application.dataPath + "/";
	// 安卓平台上如果访问StreamingAsset下的AssetBundle需要使用特殊路径,且与Application.streamingAssetsPath不同
#if UNITY_ANDROID && !UNITY_EDITOR
	public static string F_ASSET_BUNDLE_PATH = Application.dataPath + "!assets/";
#endif
	public static string F_GAME_RESOURCES_PATH = F_ASSETS_PATH + GAME_RESOURCES + "/";
	public static string F_PLUGINS_PATH = F_ASSETS_PATH + PLUGINS + "/";
	public static string F_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
	public static string F_TEMPORARY_CACHE_PATH = Application.temporaryCachePath + "/";
	public static string F_STREAMING_ASSETS_PATH = Application.streamingAssetsPath + "/";
	public static string F_ASSETS_DATA_BASE_PATH = F_STREAMING_ASSETS_PATH + DATA_BASE + "/";
	public static string F_PERSIS_DATA_BASE_PATH = F_PERSISTENT_DATA_PATH + DATA_BASE + "/";
	public static string F_VIDEO_PATH = F_STREAMING_ASSETS_PATH + VIDEO + "/";
	public static string F_CONFIG_PATH = F_STREAMING_ASSETS_PATH + CONFIG + "/";
	public static string F_GAME_DATA_FILE_PATH = F_STREAMING_ASSETS_PATH + GAME_DATA_FILE + "/";
	public static string F_HELPER_EXE_PATH = F_STREAMING_ASSETS_PATH + HELPER_EXE + "/";
	public static string F_CUSTOM_SOUND_PATH = F_STREAMING_ASSETS_PATH + CUSTOM_SOUND + "/";
	public static string F_GAME_PLUGIN_PATH = F_STREAMING_ASSETS_PATH + GAME_PLUGIN + "/";
	public static string F_ATLAS_PATH = F_GAME_RESOURCES_PATH + ATLAS + "/";
	public static string F_GAME_ATLAS_PATH = F_ATLAS_PATH + GAME_ATLAS + "/";
	public static string F_ATLAS_TEXTURE_ANIM_PATH = F_ATLAS_PATH + TEXTURE_ANIM + "/";
	public static string F_TEXTURE_PATH = F_GAME_RESOURCES_PATH + TEXTURE + "/";
	public static string F_TEXTURE_GAME_TEXTURE_PATH = F_TEXTURE_PATH + GAME_TEXTURE + "/";
	public static string F_TEXTURE_ANIM_PATH = F_TEXTURE_PATH + TEXTURE_ANIM + "/";
	public static string F_PATH_KEYFRAME_PATH = F_STREAMING_ASSETS_PATH + PATH_KEY_FRAME + "/";
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
	public const int PACKET_HEADER_SIZE = sizeof(uint) + sizeof(ushort);
	public const float CLICK_THRESHOLD = 15;			// 点击阈值,当鼠标按下和抬起时的距离不超过该值,则认为是有效点击
	public const float DOUBLE_CLICK_THRESHOLD = 0.3f;	// 双击时间阈值,两次单击时间大于0并且小于该值时认为是一次双击
	//-----------------------------------------------------------------------------------------------------------------
	// 后缀名
	public const string DATA_SUFFIX = ".bytes";
	public const string ASSET_BUNDLE_SUFFIX = ".unity3d";
	// dll插件的后缀名
	public const string DLL_PLUGIN_SUFFIX = ".bytes";
	// 常用关键帧名定义
	public const string ONE_ZERO = "OneZero";
	public const string ONE_ZERO_ONE = "OneZeroOne";
	public const string ONE_ZERO_ONE_CURVE = "OneZeroOne_Curve";
	public const string QUADRATIC_CURVE = "Quadratic_Curve";
	public const string SIN_CURVE = "Sin_Curve";
	public const string ZERO_ONE = "ZeroOne";
	public const string ZERO_ONE_ZERO = "ZeroOneZero";
	public const string ZERO_ONE_DELAY = "ZeroOne_Delay";
	// 音效所有者类型名,应该与SOUND_OWNER一致
	public static string[] SOUND_OWNER_NAME = new string[] { "Window", "Scene" };
	public const string NGUI_DEFAULT_MATERIAL = "NGUIDefault";
	public const string UGUI_DEFAULT_MATERIAL = "UGUIDefault";
	public const string COMMON_NUMBER_STYLE = "CommonNumber";
	public const string DESTROY_PLAYER_STATE = "Destroy";
	public const string UI_CAMERA = "UICamera";
	public const string BLUR_CAMERA = "BlurCamera";
	public const string NGUI_ROOT = "NGUIRoot";
	public const string UGUI_ROOT = "UGUIRoot";
	// 材质名
	public const string MAT_MULTIPLE = "Multiple";
	public const string BUILDIN_UI_MATERIAL = "Default UI Material";
	// 层
	public const string LAYER_UI = "UI";
	public const string LAYER_UI_BLUR = "UIBlur";
	public const string LAYER_DEFAULT = "Default";
}