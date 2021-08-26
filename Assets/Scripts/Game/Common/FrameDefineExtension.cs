using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Frame需要的常量,因为Frame中需要该变量,但是每个项目的值都可能不一致,所以放到GameDefine中
public class FrameDefineExtension : FrameDefine
{
	// UI的制作标准,所有UI都是按1920*1080标准分辨率制作的
	public const int STANDARD_WIDTH = 1920;
	public const int STANDARD_HEIGHT = 1080;
	public static byte[] SQLITE_ENCRYPT_KEY = BinaryUtility.stringToBytes("ASLDIHQWILDjadiuahrfiqwdo!@##*^%ishduhasf#*$^(][][dajfgsdf奥斯杜埃松恩哎u的物品*(%&#$");
	// 清理时需要保留的目录和目录的meta
	public static string[] mKeepFolder = new string[]
	{
		ILR_FILE,
		ILR_PDB_FILE,
		"Config",
		"Video",
		"PathKeyFrame",
		"DataBase",
		"Map",
	};
	// Resources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录
	// 已经打包图集,并且没有被引用的图片也不打包AB
	public static string[] mUnPackFolder = new string[]
	{
		"Texture/GameTexture/Unpack/",
		"Texture/NumberStyle/",
	};
	// 检测所有代码时需要忽略的目录
	public static string[] IGNORE_SCRIPTS_CHECK = new string[]
	{ 
		"Game/ILRuntime/GeneratedCLRBinding/",
		"Game/ILRuntime/GeneratedCrossBinding/",
		"Game/ILRuntime/ValueTypeBind/",
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
	public static List<string> IGNORE_LAYOUT_SCRIPT = new List<string>(){};
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
	public static string[] IGNORE_CHECK_CLASS = new string[]{};
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