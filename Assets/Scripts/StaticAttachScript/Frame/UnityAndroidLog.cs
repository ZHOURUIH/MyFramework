using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public delegate void OnAndroidLog(string info);
public delegate void OnAndroidError(string info);
public class UnityAndroidLog : MonoBehaviour
{
	public static OnAndroidLog mOnAndroidLog;
	public static OnAndroidError mOnAndroidError;
	public void log(string info)
	{
		ReflectionUtility.logInfo("android : " + info, 0);
		mOnAndroidLog?.Invoke(info);
	}
	public void logError(string info)
	{
		ReflectionUtility.logError("android : " + info);
		mOnAndroidError?.Invoke(info);
	}
}