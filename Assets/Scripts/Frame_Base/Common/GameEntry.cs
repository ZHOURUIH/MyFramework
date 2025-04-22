using System;
using System.Net;
using System.Threading;
using UnityEngine;
using static FrameBaseUtility;
using static FrameBaseDefine;

// 游戏的入口
public class GameEntry : MonoBehaviour
{
	protected static GameEntry mInstance;
	public FramworkParam mFramworkParam;
	protected IFramework mFrameworkAOT;
	protected IFramework mFrameworkHotFix;
	public virtual void Awake()
	{
		mInstance = this;
		ServicePointManager.DefaultConnectionLimit = 200;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		// 每当Transform组件更改时是否自动将变换更改与物理系统同步
		Physics.simulationMode = SimulationMode.Script;
		Physics.autoSyncTransforms = true;
		AppDomain.CurrentDomain.UnhandledException += unhandledException;
		setMainThreadID(Thread.CurrentThread.ManagedThreadId);
		dumpSystem();

		WINDOW_MODE fullScreen = mFramworkParam.mWindowMode;
		if (isEditor())
		{
			// 编辑器下固定全屏
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		else if (isWindows())
		{
			// windows下读取配置
			//// 设置为无边框窗口,只在Windows平台使用,由于无边框的需求非常少,所以此处不再实现,如有需求
			//if (fullScreen == WINDOW_MODE.NO_BOARD_WINDOW)
			//{
			//	// 无边框的设置有时候会失效,并且同样的设置,如果上一次设置失效后,即便恢复设置也同样会失效,也就是说本次的是否生效与上一次的结果有关
			//	// 当设置失效后,可以使用添加启动参数-popupwindow来实现无边框
			//	long curStyle = User32.GetWindowLong(User32.GetForegroundWindow(), GWL_STYLE);
			//	curStyle &= ~WS_BORDER;
			//	curStyle &= ~WS_DLGFRAME;
			//	User32.SetWindowLong(User32.GetForegroundWindow(), GWL_STYLE, curStyle);
			//}
		}
		else if (isAndroid() || isIOS())
		{
			// 移动平台下固定为全屏
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		else if (isWebGL())
		{
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		else if (isWeiXin())
		{
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		Vector2 windowSize;
		if (fullScreen == WINDOW_MODE.FULL_SCREEN)
		{
			windowSize.x = Screen.width;
			windowSize.y = Screen.height;
		}
		else
		{
			windowSize.x = mFramworkParam.mScreenWidth;
			windowSize.y = mFramworkParam.mScreenHeight;
		}
		bool fullMode = fullScreen == WINDOW_MODE.FULL_SCREEN || fullScreen == WINDOW_MODE.FULL_SCREEN_CUSTOM_RESOLUTION;
		setScreenSizeBase(new((int)windowSize.x, (int)windowSize.y), fullMode);
	}
	public void Update()
	{
		try
		{
			mFrameworkAOT?.update(Time.deltaTime);
			mFrameworkHotFix?.update(Time.deltaTime);
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
	public void FixedUpdate()
	{
		try
		{
			mFrameworkAOT?.fixedUpdate(Time.fixedDeltaTime);
			mFrameworkHotFix?.fixedUpdate(Time.fixedDeltaTime);
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
	public void LateUpdate()
	{
		try
		{
			mFrameworkAOT?.lateUpdate(Time.deltaTime);
			mFrameworkHotFix?.lateUpdate(Time.deltaTime);
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
	public void OnDrawGizmos()
	{
		try
		{
			mFrameworkAOT?.drawGizmos();
			mFrameworkHotFix?.drawGizmos();
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
	public void OnApplicationFocus(bool focus)
	{
		mFrameworkAOT?.onApplicationFocus(focus);
		mFrameworkHotFix?.onApplicationFocus(focus);
	}
	public void OnApplicationQuit()
	{
		mFrameworkAOT?.onApplicationQuit();
		mFrameworkHotFix?.onApplicationQuit();
		mFrameworkAOT = null;
		mFrameworkHotFix = null;
		logBase("程序退出完毕!");
	}
	protected void OnDestroy()
	{
		AppDomain.CurrentDomain.UnhandledException -= unhandledException;
	}
	public static GameEntry getInstance() { return mInstance; }
	public static GameObject getInstanceObject() { return mInstance.gameObject; }
	public void setFrameworkAOT(IFramework framework) { mFrameworkAOT = framework; }
	public void setFrameworkHotFix(IFramework framework) { mFrameworkHotFix = framework; }
	public IFramework getFrameworkAOT() { return mFrameworkAOT; }
	public IFramework getFrameworkHotFix() { return mFrameworkHotFix; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void unhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Debug.LogError(e.ExceptionObject.ToString());
	}
	protected void dumpSystem()
	{
		logBase("QualitySettings.currentLevel:" + QualitySettings.GetQualityLevel());
		logBase("QualitySettings.activeColorSpace:" + QualitySettings.activeColorSpace);
		logBase("Graphics.activeTier:" + Graphics.activeTier);
		logBase("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType);
		logBase("SystemInfo.maxTextureSize:" + SystemInfo.maxTextureSize);
		logBase("SystemInfo.supportsInstancing:" + SystemInfo.supportsInstancing);
		logBase("SystemInfo.graphicsShaderLevel:" + SystemInfo.graphicsShaderLevel);
		logBase("PersistentDataPath:" + F_PERSISTENT_DATA_PATH);
		logBase("StreamingAssetPath:" + F_STREAMING_ASSETS_PATH);
		logBase("AssetPath:" + F_ASSETS_PATH);
	}
}