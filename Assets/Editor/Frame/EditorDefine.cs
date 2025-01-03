using System;
using System.Collections.Generic;
using static FrameDefine;

public class EditorDefine
{
	// 不开启mipmaps的目录
	public static List<string> getNoMipmapsPath()
	{
		List<string> list = new()
		{
			P_ATLAS_PATH,
			P_TEXTURE_PATH,
			P_UI_PATH,
			P_RESOURCES_ATLAS_PATH,
			P_RESOURCES_TEXTURE_PATH,
			P_RESOURCES_UI_PATH,
			P_UNUSED_PATH,
		};
		list.AddRange(new GameEditorDefine().getNoMipmapsPath_Extension());
		return list;
	}
	// 清理时需要保留的目录和目录的meta
	public static List<string> getKeepFolder()
	{
		List<string> list = new()
		{
			"Version",
			HOTFIX_FILE + ".bytes",
			"Video",
			"SQLite",
		};
		list.AddRange(new GameEditorDefine().getKeepFolder_Extension());
		return list;
	}
	// 不需要打包AssetBundle的目录,GameResources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录
	// 已经打包图集,并且没有被引用的图片也不打包AB
	public static List<string> getUnpackFolder() 
	{
		List<string> list = new()
		{
			"Unused/",
		};
		list.AddRange(new GameEditorDefine().getUnpackFolder_Extension());
		return list; 
	}
	// 强制为每个文件单独打AssetBundle的目录,GameResources下的目录,带相对路径
	public static List<string> getForceSingleFolder()
	{
		List<string> list = new()
		{
			"Atlas/GameAtlas/",
		};
		list.AddRange(new GameEditorDefine().getForceSingleFolder_Extension());
		return list; 
	}
	// 检测所有代码时需要忽略的目录
	public static List<string> getIgnoreScriptCheck() 
	{
		List<string> list = new() { };
		list.AddRange(new GameEditorDefine().getIgnoreScriptCheck_Extension());
		return list;
	}
	// 检查代码注释时需要忽略的文件
	public static List<string> getIgnoreComment() 
	{
		List<string> list = new()
		{
			"/UI/",
			"/NetHttp/",
			"/EventSystem/",
		};
		list.AddRange(new GameEditorDefine().getIgnoreComment_Extension());
		return list; 
	}
	// 检查成员变量的初始化时需要忽略的文件
	public static List<string> getIgnoreConstructValue()
	{
		List<string> list = new()
		{
			"/Socket/",
			"/SQLite/",
			"/NetHttp/",
		};
		list.AddRange(new GameEditorDefine().getIgnoreConstructValue_Extension());
		return list; 
	}
	// 检测resetProperty时额外忽略的基类
	public static List<Type> getIgnoreResetPropertyClass() 
	{
		List<Type> list = new() { };
		list.AddRange(new GameEditorDefine().getIgnoreResetPropertyClass_Extension());
		return list; 
	}
	// 检测布局脚本的代码时需要忽略的脚本
	public static List<string> getIgnoreLayoutScript() 
	{
		List<string> list = new() { };
		list.AddRange(new GameEditorDefine().getIgnoreLayoutScript_Extension());
		return list;
	}
	// 检测代码行长度时额外忽略的文件
	public static List<string> getIgnoreCodeWidth() 
	{
		List<string> list = new()
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
		list.AddRange(new GameEditorDefine().getIgnoreCodeWidth_Extension());
		return list; 
	}
	// 检测命名规范时额外忽略的文件
	public static List<string> getIgnoreVariableCheck()
	{
		List<string> list = new()
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
		list.AddRange(new GameEditorDefine().getIgnoreVariableCheck_Extension());
		return list;
	}
	// 检查命名规范时需要忽略的类名包含如下字符串的类
	public static List<string> getIgnoreCheckClass() 
	{
		List<string> list = new() { };
		list.AddRange(new GameEditorDefine().getIgnoreCheckClass_Extension());
		return list; 
	}
	// 检查函数命名规范需要忽略的文件
	public static List<string> getIgnoreFileCheckFunction()
	{
		List<string> list = new()
		{
			"/StringUtility.cs",
			"/MathUtility.cs",
			"Utility/Struct/",
			"/GameFramework.cs",
			"/Serialize/",
			"/MyEmptyDictionary.cs",
			"/PurchasingSystem.cs",
			"/UIGradient.cs",
		};
		list.AddRange(new GameEditorDefine().getIgnoreFileCheckFunction_Extension());
		return list; 
	}
	// 检查函数命名规范需要忽略的函数名
	public static List<string> getIgnoreCheckFunction() 
	{
		List<string> list = new()
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
		list.AddRange(new GameEditorDefine().getIgnoreCheckFunction_Extension());
		return list;
	}
	// 检查内置函数调用时需要忽略的文件
	public static List<string> getIgnoreSystemFunctionCheck() 
	{
		List<string> list = new() { };
		list.AddRange(new GameEditorDefine().getIgnoreSystemFunctionCheck_Extension());
		return list; 
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	protected virtual List<string> getNoMipmapsPath_Extension() { return new(); }
	protected virtual List<string> getKeepFolder_Extension() { return new(); }
	protected virtual List<string> getUnpackFolder_Extension() { return new(); }
	protected virtual List<string> getForceSingleFolder_Extension() { return new(); }
	protected virtual List<string> getIgnoreScriptCheck_Extension() { return new(); }
	protected virtual List<string> getIgnoreComment_Extension() { return new(); }
	protected virtual List<string> getIgnoreConstructValue_Extension() { return new(); }
	protected virtual List<Type> getIgnoreResetPropertyClass_Extension() { return new(); }
	protected virtual List<string> getIgnoreLayoutScript_Extension() { return new(); }
	protected virtual List<string> getIgnoreCodeWidth_Extension() 
	{
		return new()
		{ 
			"/Description/",
			"/BattleScene.cs",
		};
	}
	protected virtual List<string> getIgnoreVariableCheck_Extension() { return new(); }
	protected virtual List<string> getIgnoreCheckClass_Extension() { return new(); }
	protected virtual List<string> getIgnoreFileCheckFunction_Extension() 
	{
		return new()
		{ 
			"/Http/",
		};
	}
	protected virtual List<string> getIgnoreCheckFunction_Extension() { return new(); }
	protected virtual List<string> getIgnoreSystemFunctionCheck_Extension() { return new(); }
}