using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 游戏枚举定义-----------------------------------------------------------------------------------------------

// 委托定于

// 游戏常量定义-------------------------------------------------------------------------------------------------------------
public class GameDefine : FrameDefine
{
	// 路径定义
	// Frame需要的常量,因为Frame中需要该变量,但是每个项目的值都可能不一致,所以放到GameDefine中
	//-----------------------------------------------------------------------------------------------------------------
	// UI的制作标准,所有UI都是按1920*1080标准分辨率制作的
	public const int STANDARD_WIDTH = 1920;
	public const int STANDARD_HEIGHT = 1080;
	public static string SQLITE_ENCRYPT_KEY = "ASLDIHQWILDjadiuahrfiqwdo!@##*^%ishduhasf#*$^(][][dajfgsdf奥斯杜埃松恩哎u的物品*(%&#$";
	// 常量定义
	//-----------------------------------------------------------------------------------------------------------------
	// 标签
	// 关键帧
	// 层
	public const string LAYER_UGUI = "UGUI";
	// 清理时需要保留的目录和目录的meta
	public static string[] mKeepFolder = new string[] 
	{
		"Config", "GameDataFile", "Video", "DataTemplate", "Version", "PathKeyFrame", "DataBase", "Map" 
	};
	// Resources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录
	// 已经打包图集,并且没有被引用的图片也不打包AB
	public static string[] mUnPackFolder = new string[]
	{
		"Texture/GameTexture/Unpack/",
		"Texture/NumberStyle/",
	};
}