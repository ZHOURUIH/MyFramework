using UnityEngine;
using System;

// 框架层设置参数
public partial class GameFramework : MonoBehaviour
{
	public float mFixedTime;				// 每帧的固定时间,单位秒
	public int mScreenHeight;				// 窗口高度,当mWindowMode为FULL_SCREEN时无效
	public int mScreenWidth;				// 窗口宽度,当mWindowMode为FULL_SCREEN时无效
	public bool mEnablePoolStackTrace;		// 是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,可使用F4动态开启
	public bool mEnableScriptDebug;			// 是否启用调试脚本,也就是挂接在GameObject上用于显示调试信息的脚本,可使用F3动态开启
	public bool mUseFixedTime;				// 是否将每帧的时间固定下来
	public bool mForceTop;					// 窗口是否始终显示在顶层
	public LOAD_SOURCE mLoadSource;			// 加载源,从AssetBundle加载还是从Resources加载
	public WINDOW_MODE mWindowMode;			// 窗口类型
	public LOG_LEVEL mLogLevel;				// 日志等级
}