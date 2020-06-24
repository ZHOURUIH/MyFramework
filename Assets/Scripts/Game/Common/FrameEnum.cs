using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Frame需要的枚举值,每个项目的值都不同,但是Frame中需要使用
// 界面布局定义
public enum LAYOUT
{
	L_DEMO,
	L_DEMO_START,
	L_MAX,
};
// 音效定义
public enum SOUND_DEFINE
{
	SD_MIN = 0,
	SD_MAX,
};
// 游戏中的公共变量定义
public enum GAME_DEFINE_FLOAT
{
	GDF_NONE,
	// 应用程序配置参数
	GDF_APPLICATION_MIN,
	GDF_FULL_SCREEN,                // 是否全屏,0为窗口模式,1为全屏,2为无边框窗口
	GDF_SCREEN_WIDTH,               // 分辨率的宽
	GDF_SCREEN_HEIGHT,              // 分辨率的高
	GDF_USE_FIXED_TIME,             // 是否将每帧的时间固定下来
	GDF_FIXED_TIME,                 // 每帧的固定时间,单位秒
	GDF_VSYNC,                      // 垂直同步,0为关闭垂直同步,1为开启垂直同步
	GDF_FORCE_TOP,					// 是否将窗口置顶
	GDF_APPLICATION_MAX,

	// 框架配置参数
	GDF_FRAME_MIN,

	GDF_LOAD_RESOURCES,             // 游戏加载资源的路径,0代表在Resources中读取,1代表从AssetBundle中读取
	GDF_LOG_LEVEL,                  // 日志输出等级
	GDF_ENABLE_KEYBOARD,            // 是否响应键盘按键
	GDF_PERSISTENT_DATA_FIRST,      // 当从AssetBundle加载资源时,是否先去persistentDataPath中查找资源
	GDF_FRAME_MAX,

	// 游戏配置参数
	GDF_GAME_MIN,
	GDF_GAME_MAX,
};
public enum GAME_DEFINE_STRING
{
	GDS_NONE,
	// 应用程序配置参数
	GDS_APPLICATION_MIN,
	GDS_APPLICATION_MAX,

	// 框架配置参数
	GDS_FRAME_MIN,
	GDS_FRAME_MAX,

	// 游戏配置参数
	GDS_GAME_MIN,
	GDS_GAME_MAX,
};