using UnityEngine;
using static FrameBaseUtility;

// 用于加载Android平台下的资源
public class AndroidMainClass : FrameSystem
{
	protected static AndroidJavaClass mMainClass;    // Java中加载类的实例
	public static void initJava(string classPath)
	{
		if (!isEditor() && isAndroid())
		{
			if (classPath.isEmpty())
			{
				logErrorBase("initJava failed! classPath not valid");
				return;
			}
			mMainClass = new(classPath);
		}
	}
	public override void destroy()
	{
		mMainClass?.Dispose();
		mMainClass = null;
		base.destroy();
	}
	public static AndroidJavaClass getMainClass() { return mMainClass; }
	public static void gameStart()
	{
		if (isEditor() || !isAndroid())
		{
			return;
		}
		if (mMainClass == null)
		{
			logErrorBase("MainClass is null");
			return;
		}
		mMainClass.CallStatic("gameStart", AndroidPluginManager.getMainActivity(), AndroidPluginManager.getApplication());
	}
}