using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FrameDefineExtension : FrameDefine
{
	// UI的制作标准,所有UI都是按1280*960标准分辨率制作的
	public const int STANDARD_WIDTH = 1920;
	public const int STANDARD_HEIGHT = 1080;
	public static byte[] SQLITE_ENCRYPT_KEY = BinaryUtility.stringToBytes("ASLDIHQWILDjadiuahrfiqwdo!@##*^%ishduhasf#*$^(][][dajfgsdf奥斯杜埃松恩哎u的物品*(%&#$");
	// 清理时需要保留的目录和目录的meta
	public static string[] mKeepFolder = new string[]
	{
		ILR_FILE,
		ILR_PDB_FILE,
		"Video", 
		"PathKeyFrame", 
		"DataBase",
		"Map", 
	};
	// GameResources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录
	// 已经打包图集,并且没有被引用的图片也不打包AB
	public static string[] mUnPackFolder = new string[]
	{
		"Texture/GameTexture/Unpack/",
		"Texture/NumberStyle/",
	};

	// 检测resetProperty时额外忽略的基类
	public static List<Type> IGNORE_RESETPROPERTY_CLASS = new List<Type>(){};

	// 检测空行时需要忽略的目录
	public static List<string> IGNORE_EMPTY_LINE_DIR = new List<string>()
	{ 
		"Game/ILRuntime/GeneratedCLRBinding/",
		"Game/ILRuntime/GeneratedCrossBinding/",
		"Game/ILRuntime/ValueTypeBind/",
	};

	// 检测布局脚本的代码时需要忽略的脚本
	public static List<string> IGNORE_LAYOUT_SCRIPT = new List<string>(){};
	// 检查类内函数,变量,枚举等命名规范时需要忽略的类名包含如下字符串的类
	public static string[] mCheckClassIgnore = new string[]{};
}