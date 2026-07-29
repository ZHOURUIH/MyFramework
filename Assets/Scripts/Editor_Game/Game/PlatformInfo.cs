using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorCommonUtility;
using static FrameBaseUtility;
using static FrameMacro;

// 游戏上架渠道,用于给添加后缀名,以及注入宏,来执行不同的sdk逻辑
public enum GAME_CHANNEL : byte
{
	NONE,
	TAP_TAP,			// TapTap平台,仅做示例
}

// 这个类负责实现生成一些名字,宏定义,创建当前平台实例等功能
public abstract class PlatformInfo : PlatformBase
{

	public static Dictionary<GAME_CHANNEL, string> GAME_CHANNEL_NAME_LIST = new()
	{
		{ GAME_CHANNEL.TAP_TAP, "_TapTap"},
		{ GAME_CHANNEL.NONE, ""},
	};
	public GAME_CHANNEL mGameChannel;                   // 上架渠道
	public PlatformInfo()
	{
		// 如果用的华为云的Obs作为对象存储,就在这里进行初始化,设置所需的参数
		//mObjectStorageSystem = ObsSystem.get();
		//mObjectStorageSystem.init(OBS_URL, OBS_BUCKET_NAME, OBS_ACCESS_KEY, OBS_SECURE_KEY);
	}
	public static PlatformInfo create()
	{
		BuildTarget target = getBuildTarget();
		PlatformInfo info = null;
		if (target == BuildTarget.Android)
		{
			info = new PlatformAndroid();
		}
		else if (target == BuildTarget.iOS)
		{
			info = new PlatformIOS();
		}
		else if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
		{
			info = new PlatformWindows();
		}
		else if (target == BuildTarget.StandaloneOSX)
		{
			info = new PlatformMacOS();
		}
		else if (target == BuildTarget.WebGL)
		{
			info = new PlatformWebGL();
		}
		else
		{
			Debug.LogError("不支持的平台");
		}
		return info;
	}
	public override void generateFolderPreName()
	{
		string folderPreName = isWindows() ? "我的传奇" : "MicroLegend";
		if (mTestClient)
		{
			folderPreName += "_Test";
		}
		if (mEnableHotFix)
		{
			// 启用热更时,只有测试版才会添加HotFix,这是为了保证正式版的文件名是简洁的
			if (mTestClient)
			{
				folderPreName += "_HotFix";
			}
		}
		else
		{
			folderPreName += "_NoHotFix";
		}
		// 仅安卓平台才会在安装包的名字上面添加游戏渠道的后缀
		if (isAndroid())
		{
			folderPreName += GAME_CHANNEL_NAME_LIST.get(mGameChannel);
		}
		mFolderPreName = folderPreName;
	}
	public override string getRemotePathInEditor(string version)
	{
		string folder = "Assets_";
		if (mTestClient)
		{
			folder += "Test_";
		}
		if (isAndroid())
		{
			folder += "Android";
			if (mGameChannel != GAME_CHANNEL.NONE)
			{
				folder += GAME_CHANNEL_NAME_LIST[mGameChannel];
			}
			folder += "/";
		}
		else if (isWindows())
		{
			folder += "Windows/";
		}
		else if (isIOS())
		{
			folder += "iOS/";
		}
		else if (isMacOS())
		{
			folder += "MacOS/";
		}
		else if (isWebGL())
		{
			folder += "WebGL/";
		}
		else
		{
			Debug.LogError("未知平台");
		}
		if (version.isEmpty())
		{
			return folder;
		}
		return folder + version + "/";
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override string getDefaultPlatformDefine()
	{
		return USE_HYBRID_CLR + ";" + USE_OBFUZ + ";" + PROJECT_2D + ";" + USE_URP + ";" + USE_SQLITE;
	}
	protected override string getBuildTimePlatformDefineInternal()
	{
		string platformDefine = "";
		// 添加宏定义
		// 安卓平台下根据要上架的不同平台添加对应的宏
		if (isAndroid())
		{
			if (mGameChannel == GAME_CHANNEL.TAP_TAP)
			{
				platformDefine += ";TAP_TAP";
			}
		}
		return platformDefine;
	}
}