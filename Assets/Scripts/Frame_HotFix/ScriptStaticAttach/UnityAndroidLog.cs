using UnityEngine;

// 用于接收Java中的信息传输
public class UnityAndroidLog : MonoBehaviour
{
	public static StringCallback mOnAndroidError;	// Java中错误日志的回调函数
	public static StringCallback mOnAndroidLog;		// Java中普通日志的回调函数,或者是其他由Android主动传到Unity的数据
	public void log(string info)
	{
		// 这里需要加上UnityUtility,否则会无限递归
		UnityUtility.log("android : " + info);
		mOnAndroidLog?.Invoke(info);
	}
	public void logError(string info)
	{
		// 这里需要加上UnityUtility,否则会无限递归
		UnityUtility.logError("android : " + info);
		mOnAndroidError?.Invoke(info);
	}
}