using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 游戏常量定义
public class GameDefine : FrameDefine
{
	// 路径定义
	// 常量定义
	//-----------------------------------------------------------------------------------------------------------------
	// 标签
	// 层
	// 清理时需要保留的目录和目录的meta
	public static string[] mKeepFolder = new string[] 
	{
		"Config", "Video", "Version", "PathKeyFrame", "DataBase"
	};
	// Resources下的目录,带相对路径,且如果前缀符合,也会认为是不打包的目录
	// 已经打包图集,并且没有被引用的图片也不打包AB
	public static string[] mUnPackFolder = new string[]
	{
		"Texture/GameTexture/Unpack/",
		"Texture/NumberStyle/",
	};
}