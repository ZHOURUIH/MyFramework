using UnityEngine;
using static FrameBaseUtility;

public class BuglyForwarder
{
	protected static bool mHasReportedError;
	public static void init()
    {
        if (isEditor())
        {
			return;
		}
		Application.logMessageReceived += reportError;
		Application.logMessageReceivedThreaded += reportError;
    }
	public static void setVersion(AndroidJavaObject mainActivity, string version)
	{
		// 初始化 Bugly 前设置版本
		if (isAndroid())
		{
			using AndroidJavaClass crashReport = new("com.tencent.bugly.crashreport.CrashReport");
			crashReport.CallStatic("setAppVersion", mainActivity, version);
		}
		else if (isIOS())
		{
			iOSDllImportFrameBase.setUserData("unity_version", version);
		}
	}
    public static void reportError(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
			if (mHasReportedError)
			{
				return;
			}
			mHasReportedError = true;
			string name = condition;
			// 需要去掉一开始的时间,使同样的报错能够合并
			if (name.Contains("error:"))
			{
				name = name[name.IndexOf("error:")..];
			}
			if (isAndroid())
			{
				using AndroidJavaClass crashReport = new("com.tencent.bugly.crashreport.CrashReport");
				crashReport.CallStatic("postException", 4, name, condition, stackTrace, null);
			}
			else if (isIOS())
			{
				iOSDllImportFrameBase.reportException(name, condition, stackTrace);
			}
        }
    }
}
