using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Frame需要的枚举值,每个项目的值都不同,但是Frame中需要使用
// 界面布局定义
public class LAYOUT
{
	public const int NONE = 0;
	public const int DEMO = 1;
	public const int DEMO_START = 2;
};

// 关键帧ID定义,对应关键帧预设中的曲线ID
public enum KEY_FRAME : byte
{
	NONE,
	ONE_ZERO,
	ONE_ZERO_ONE,
	ONE_ZERO_ONE_CURVE,
	QUADRATIC_CURVE,
	ZERO_ONE,
	ZERO_ONE_ZERO,
	ZERO_ONE_ZERO_CURVE,
	SIN_CURVE,
	SINE_IN_OUT,
}

// 音效定义
public enum SOUND_DEFINE : byte
{
	MIN = 0,
};

// 游戏中的公共变量定义
public enum GAME_FLOAT : byte
{
	NONE,
	// 应用程序配置参数
	APPLICATION_MIN,
	FULL_SCREEN,                // 是否全屏,0为窗口模式,1为全屏,2为无边框窗口
	SCREEN_WIDTH,               // 分辨率的宽
	SCREEN_HEIGHT,              // 分辨率的高
	USE_FIXED_TIME,             // 是否将每帧的时间固定下来
	FIXED_TIME,                 // 每帧的固定时间,单位秒
	VSYNC,                      // 垂直同步,0为关闭垂直同步,1为开启垂直同步
	FORCE_TOP,					// 是否将窗口置顶
	APPLICATION_MAX,

	// 框架配置参数
	FRAME_MIN,
	LOAD_RESOURCES,             // 游戏加载资源的路径,0代表在Resources中读取,1代表从AssetBundle中读取
	LOG_LEVEL,                  // 日志输出等级
	ENABLE_KEYBOARD,            // 是否响应键盘按键
	PERSISTENT_DATA_FIRST,      // 当从AssetBundle加载资源时,是否先去persistentDataPath中查找资源
	FRAME_MAX,

	// 游戏配置参数
	GAME_MIN,
	GAME_MAX,
};
public enum GAME_STRING : byte
{
	NONE,
	// 应用程序配置参数
	APPLICATION_MIN,
	APPLICATION_MAX,

	// 框架配置参数
	FRAME_MIN,
	FRAME_MAX,

	// 游戏配置参数
	GAME_MIN,
	GAME_MAX,
};