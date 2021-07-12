using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnAndroidLog(string info);
public delegate void OnAndroidError(string info);

public class UnityAndroidLog : MonoBehaviour
{
	public static OnAndroidLog mOnAndroidLog;
	public static OnAndroidError mOnAndroidError;
	public void log(string info)
	{
		UnityUtility.logForce("android : " + info);
		mOnAndroidLog?.Invoke(info);
	}
	public void logError(string info)
	{
		UnityUtility.logError("android : " + info);
		mOnAndroidError?.Invoke(info);
	}
}