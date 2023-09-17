using System;
using UnityEngine;
using static UnityUtility;

// 用于接收Java中的信息传输
public class UnityAndroidLog : MonoBehaviour
{
	public static OnAndroidError mOnAndroidError;	// Java中错误日志的回调函数
	public static OnAndroidLog mOnAndroidLog;		// Java中普通日志的回调函数,或者是其他由Android主动传到Unity的数据
	public void log(string info)
	{
		logForce("android : " + info);
		mOnAndroidLog?.Invoke(info);
	}
	public void logError(string info)
	{
		logError("android : " + info);
		mOnAndroidError?.Invoke(info);
	}
}