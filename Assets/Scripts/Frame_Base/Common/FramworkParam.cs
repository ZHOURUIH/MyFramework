using System;
using UnityEngine;

[Serializable]
public class FramworkParam
{
	[Tooltip("每帧的固定时间,单位秒")]
	public float mFixedTime;                                        // 每帧的固定时间,单位秒
	[Tooltip("窗口高度,当mWindowMode为FULL_SCREEN时无效")]
	public int mScreenHeight;                                       // 窗口高度,当mWindowMode为FULL_SCREEN时无效
	[Tooltip("窗口宽度,当mWindowMode为FULL_SCREEN时无效")]
	public int mScreenWidth;                                        // 窗口宽度,当mWindowMode为FULL_SCREEN时无效
	[Tooltip("默认的帧率")]
	public int mDefaultFrameRate = 60;                              // 默认的帧率
	[Tooltip("是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,快捷键F4")]
	public bool mEnablePoolStackTrace;                              // 是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,快捷键F4
	[Tooltip("是否启用调试脚本,用于显示调试信息的脚本,快捷键F3")]
	public bool mEnableScriptDebug;                                 // 是否启用调试脚本,用于显示调试信息的脚本,快捷键F3
	[Tooltip("是否将每帧的时间固定下来")]
	public bool mUseFixedTime;                                      // 是否将每帧的时间固定下来
	[Tooltip("窗口是否始终显示在顶层")]
	public bool mForceTop;                                          // 窗口是否始终显示在顶层
	[Tooltip("加载源,从AssetBundle加载还是从Resources加载")]
	public LOAD_SOURCE mLoadSource;                                 // 加载源,从AssetBundle加载还是从Resources加载
	[Tooltip("窗口类型")]
	public WINDOW_MODE mWindowMode;                                 // 窗口类型
	[Tooltip("日志等级,快捷键F1")]
	public LOG_LEVEL mLogLevel;                                     // 日志等级,快捷键F1
}