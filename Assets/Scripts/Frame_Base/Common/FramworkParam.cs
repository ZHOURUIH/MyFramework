using System;
using UnityEngine;

[Serializable]
public class FramworkParam
{
	[Tooltip("窗口高度,当mWindowMode为FULL_SCREEN时无效")]
	public int mScreenHeight = 0;                                   // 窗口高度,当mWindowMode为FULL_SCREEN时无效
	[Tooltip("窗口宽度,当mWindowMode为FULL_SCREEN时无效")]
	public int mScreenWidth = 0;                                    // 窗口宽度,当mWindowMode为FULL_SCREEN时无效
	[Tooltip("默认的帧率")]
	public int mDefaultFrameRate = 60;                              // 默认的帧率
	[Tooltip("是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,快捷键F4")]
	public bool mEnablePoolStackTrace;                              // 是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,快捷键F4
	[Tooltip("是否启用调试脚本,用于显示调试信息的脚本,快捷键F3")]
	public bool mEnableScriptDebug;                                 // 是否启用调试脚本,用于显示调试信息的脚本,快捷键F3
	[Tooltip("加载源,从AssetBundle加载还是从Resources加载")]
	public LOAD_SOURCE mLoadSource = LOAD_SOURCE.ASSET_DATABASE;    // 加载源,从AssetBundle加载还是从Resources加载
	[Tooltip("窗口类型")]
	public WINDOW_MODE mWindowMode = WINDOW_MODE.FULL_SCREEN;       // 窗口类型
}