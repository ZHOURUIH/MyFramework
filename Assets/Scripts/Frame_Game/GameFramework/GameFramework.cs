﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using static UnityUtility;
using static FrameDefineBase;
using static CSharpUtility;
using static FrameDefine;
using static MathUtility;
using static TimeUtility;
using static FrameBase;
using static FrameEditorUtility;

// 最顶层的节点,也是游戏的入口,管理所有框架组件(管理器)
public class GameFramework : MonoBehaviour
{
	public static GameFramework mGameFramework;											// 框架单例
	protected Dictionary<string, FrameSystem> mFrameComponentMap = new(128);			// 存储框架组件,用于查找
	protected Dictionary<string, Action<FrameSystem>> mFrameCallbackList = new(128);	// 用于通知框架系统创建或者销毁的回调
	protected List<FrameSystem> mFrameComponentInit = new(128);							// 存储框架组件,用于初始化
	protected List<FrameSystem> mFrameComponentUpdate = new(128);						// 存储框架组件,用于更新
	protected List<FrameSystem> mFrameComponentDestroy = new(128);						// 存储框架组件,用于销毁
	protected ThreadTimeLock mTimeLock = new(30);					// 用于主线程锁帧,与Application.targetFrameRate功能类似
	protected GameObject mGameFrameObject;                          // 游戏框架根节点
	protected DateTime mStartTime;                                  // 启动游戏时的时间
	protected DateTime mCurTime;                                    // 记录当前时间
	protected Action mOnApplicationQuitCallBack;					// 程序退出的回调
	protected BoolCallback mOnApplicationFocusCallBack;				// 程序切换到前台或者切换到后台的回调
	protected float mThisFrameTime;                                 // 当前这一帧的消耗时间
	protected long mFrameIndex;                                     // 当前帧下标
	protected int mCurFrameCount;                                   // 当前已执行的帧数量
	protected int mFrameRate;										// 当前设置的最大帧率
	protected int mFPS;                                             // 当前帧率
	protected bool mResourceAvailable;                              // 资源是否已经可用
	protected bool mIsDestroy;                                      // 框架是否已经被销毁
	public FramworkParam mParam = new();                            // 框架层设置参数
	public static Action mOnInitFrameSystem;
	public static Action mOnRegisteStuff;
	public static Action mOnDestroy;
	public static Action<int, long, long, long, long> mOnMemoryModifiedCheck;
	public static Func<string> mOnPackageName;
	public virtual void Awake()
	{
		using var a = new ProfilerScope("Start");
		// 通过代码添加接受java日志的节点
		GameObject unityLog = getRootGameObject("UnityLog");
		if (unityLog == null)
		{
			unityLog = createGameObject("UnityLog");
		}
		if (unityLog.GetComponent<ResUnityAndroidLog>() == null)
		{
			unityLog.AddComponent<ResUnityAndroidLog>();
		}
		mIsDestroy = false;
		mStartTime = DateTime.Now;
		FrameBase.mGameFramework = this;
		mGameFramework = this;
		mGameFrameObject = gameObject;
		setMainThreadID(Thread.CurrentThread.ManagedThreadId);
		setFrameRate(mParam.mDefaultFrameRate);
		ServicePointManager.DefaultConnectionLimit = 200;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		// 每当Transform组件更改时是否自动将变换更改与物理系统同步
		Physics.simulationMode = SimulationMode.Script;
		Physics.autoSyncTransforms = true;
		AppDomain.CurrentDomain.UnhandledException += unhandledException;

		if (!isEditor() && isDevelopment())
		{
			gameObject.AddComponent<ResConsoleToScreen>();
		}

		// 设置默认的日志等级
		setLogLevel(mParam.mLogLevel);

		registeFrameSystem<AndroidPluginManager>((com) =>	{ mAndroidPluginManager = com; });
		registeFrameSystem<AndroidAssetLoader>((com) =>		{ mAndroidAssetLoader = com; });
		registeFrameSystem<AndroidMainClass>((com) =>		{ mAndroidMainClass = com; });
		AndroidPluginManager.initAnroidPlugin(mOnPackageName?.Invoke());
		AndroidAssetLoader.initJava(AndroidPluginManager.getPackageName() + ".AssetLoader");
		AndroidMainClass.initJava(AndroidPluginManager.getPackageName() + ".MainClass");
		log("start game!");
		dumpSystem();
		try
		{
			DateTime startTime = DateTime.Now;
			initFrameSystem();
			AndroidMainClass.gameStart();
			log("start消耗时间:" + (int)(DateTime.Now - startTime).TotalMilliseconds);
			// 根据设置的顺序对列表进行排序
			mFrameComponentInit.Sort(FrameSystem.compareInit);
			mFrameComponentUpdate.Sort(FrameSystem.compareUpdate);
			mFrameComponentDestroy.Sort(FrameSystem.compareDestroy);
			mOnRegisteStuff?.Invoke();
			init();
		}
		catch (Exception e)
		{
			logException(e, "init failed! " + (e.InnerException?.Message ?? "empty"));
		}
		mCurTime = DateTime.Now;
	}
	public DateTime getStartTime() { return mStartTime; }
	public DateTime getFrameStartTime() { return mTimeLock.getFrameStartTime(); }
	public long getFrameIndex() { return mFrameIndex; }
	public void Update()
	{
		try
		{
			++mFrameIndex;
			++mCurFrameCount;
			DateTime now = DateTime.Now;
			if ((now - mCurTime).TotalMilliseconds >= 1000.0f)
			{
				mFPS = mCurFrameCount;
				mCurFrameCount = 0;
				mCurTime = now;
			}
			mThisFrameTime = clampMax((float)(mTimeLock.update() * 0.001) * Time.timeScale, 0.3f);
			setThisTimeMS(getNowTimeStampMS());
			update(mThisFrameTime);
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public void FixedUpdate()
	{
		try
		{
			fixedUpdate(Time.fixedDeltaTime);
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public void LateUpdate()
	{
		try
		{
			lateUpdate(mThisFrameTime);
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public void OnDrawGizmos()
	{
		try
		{
			drawGizmos();
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public void OnDestroy()
	{
		destroy();
	}
	public void OnApplicationFocus(bool focus)
	{
		mOnApplicationFocusCallBack?.Invoke(focus);
	}
	public void OnApplicationQuit()
	{
		mOnApplicationQuitCallBack?.Invoke();
		destroy();
		log("程序退出完毕!");
	}
	// 当资源更新完毕后,由外部进行调用
	public void resourceAvailable()
	{
		mResourceAvailable = true;
		foreach (FrameSystem frame in mFrameComponentInit)
		{
			frame.resourceAvailable();
		}
	}
	public void setFrameRate(int rate)
	{
		mFrameRate = rate;
		Application.targetFrameRate = mFrameRate;
	}
	public bool isDestroy() { return mIsDestroy; }
	public void destroy()
	{
		log("destroy GameFramework NoHotFix");
		if (!isEditor() && isDevelopment())
		{
			destroyUnityObject(gameObject.GetComponent<ResConsoleToScreen>());
		}
		GameObject unityLog = getRootGameObject("UnityLog");
		if (unityLog != null && unityLog.TryGetComponent<ResUnityAndroidLog>(out var comUnityLog))
		{
			destroyUnityObject(comUnityLog);
		}
		mIsDestroy = true;
		if (mFrameComponentDestroy == null)
		{
			return;
		}
		mOnDestroy?.Invoke();
		foreach (FrameSystem frame in mFrameComponentInit)
		{
			frame?.willDestroy();
		}
		foreach (FrameSystem frame in mFrameComponentInit)
		{
			if (frame != null)
			{
				frame.destroy();
				mFrameCallbackList.Remove(frame.getName(), out var callback);
				callback?.Invoke(null);
			}
		}
		mFrameComponentInit.Clear();
		mFrameComponentUpdate.Clear();
		mFrameComponentDestroy.Clear();
		mFrameComponentMap.Clear();
		mFrameComponentInit = null;
		mFrameComponentUpdate = null;
		mFrameComponentDestroy = null;
		mFrameComponentMap = null;
		AppDomain.CurrentDomain.UnhandledException -= unhandledException;
	}
	public void stop()
	{
		stopApplication();
	}
	public GameObject getGameFrameObject() { return mGameFrameObject; }
	public void onMemoryModified(int flag, long param0, long param1, long param2, long param3) 
	{
		mOnMemoryModifiedCheck?.Invoke(flag, param0, param1, param2, param3);
	}
	// 实际上不需要调用,在非热更执行期间加载的全部都是Resource中的资源,不需要AssetBundle
	public void initAsync(Action callback)
	{
		// 先执行所有的preInit
		preInitAsync(() =>
		{
			// 再执行所有的init,因为部分init会依赖于preInit
			int initedCount = 0;
			foreach (FrameSystem item in mFrameComponentInit)
			{
				item.initAsync(() =>
				{
					if (++initedCount == mFrameComponentInit.count())
					{
						resourceAvailable();
						callback?.Invoke();
					}
				});
			}
		});
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void preInitAsync(Action callback)
	{
		int initedCount = 0;
		foreach (FrameSystem item in mFrameComponentInit)
		{
			item.preInitAsync(() =>
			{
				if (++initedCount == mFrameComponentInit.count())
				{
					callback?.Invoke();
				}
			});
		}
	}
	protected void update(float elapsedTime)
	{
		if (mFrameComponentUpdate == null)
		{
			return;
		}
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			// 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
			if (mFrameComponentUpdate == null)
			{
				return;
			}
			FrameSystem com = mFrameComponentUpdate[i];
			if (com.isValid())
			{
				using var a = new ProfilerScope(com.getName());
				com.update(elapsedTime);
			}
		}
	}
	protected void fixedUpdate(float elapsedTime)
	{
		if (mFrameComponentUpdate == null)
		{
			return;
		}
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			// 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
			if (mFrameComponentUpdate == null)
			{
				return;
			}
			FrameSystem com = mFrameComponentUpdate[i];
			if (com.isValid())
			{
				using var a = new ProfilerScope(com.getName());
				com.fixedUpdate(elapsedTime);
			}
		}
	}
	protected void lateUpdate(float elapsedTime)
	{
		if (mFrameComponentUpdate == null)
		{
			return;
		}
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			// 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
			if (mFrameComponentUpdate == null)
			{
				return;
			}
			FrameSystem com = mFrameComponentUpdate[i];
			if (com.isValid())
			{
				using var a = new ProfilerScope(com.getName());
				com.lateUpdate(elapsedTime);
			}
		}
	}
	protected void drawGizmos()
	{
		if (mFrameComponentUpdate == null)
		{
			return;
		}
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			// 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
			if (mFrameComponentUpdate == null)
			{
				return;
			}
			FrameSystem com = mFrameComponentUpdate[i];
			if (com.isValid())
			{
				com.onDrawGizmos();
			}
		}
	}
	protected void init()
	{
		foreach (FrameSystem frame in mFrameComponentInit)
		{
			try
			{
				DateTime start = DateTime.Now;
				frame.init();
				log(frame.getName() + "初始化消耗时间:" + (int)(DateTime.Now - start).TotalMilliseconds);
			}
			catch (Exception e)
			{
				logException(e, "init failed! :" + frame.getName());
			}
		}

		foreach (FrameSystem frame in mFrameComponentInit)
		{
			try
			{
				frame.lateInit();
			}
			catch (Exception e)
			{
				logException(e, "lateInit failed! :" + frame.getName());
			}
		}

		WINDOW_MODE fullScreen = mParam.mWindowMode;
		if (isEditor())
		{
			// 编辑器下固定全屏
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		else if (isWindows())
		{
			// windows下读取配置
		}
		else if (isMobile())
		{
			// 移动平台下固定为全屏
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		else if (isWebGL())
		{
			fullScreen = WINDOW_MODE.FULL_SCREEN;
		}
		Vector2 windowSize;
		if (fullScreen == WINDOW_MODE.FULL_SCREEN)
		{
			windowSize = getScreenSize();
		}
		else
		{
			windowSize.x = mParam.mScreenWidth;
			windowSize.y = mParam.mScreenHeight;
		}
		bool fullMode = fullScreen == WINDOW_MODE.FULL_SCREEN || fullScreen == WINDOW_MODE.FULL_SCREEN_CUSTOM_RESOLUTION;
		setScreenSize(new((int)windowSize.x, (int)windowSize.y), fullMode);
	}
	protected void initFrameSystem()
	{
		registeFrameSystem<ResourceManager>((com) =>		{ mResourceManager = com; },  -1, 3000, 3000);    // 资源管理器的需要最先初始化,并且是最后被销毁,作为最后的资源清理
		registeFrameSystem<GlobalCmdReceiver>((com) =>		{ mGlobalCmdReceiver = com; });
		registeFrameSystem<CommandSystem>((com) =>			{ mCommandSystem = com; }, -1, -1, 2001);      // 命令系统在大部分管理器都销毁完毕后再销毁
		registeFrameSystem<InputSystem>((com) =>			{ mInputSystem = com; });                      // 输入系统应该早点更新,需要更新输入的状态,以便后续的系统组件中使用
		registeFrameSystem<GlobalTouchSystem>((com) =>		{ mGlobalTouchSystem = com; });
		registeFrameSystem<AudioManager>((com) =>			{ mAudioManager = com; });
		registeFrameSystem<GameSceneManager>((com) =>		{ mGameSceneManager = com; });
		registeFrameSystem<KeyFrameManager>((com) =>		{ mKeyFrameManager = com; });
		registeFrameSystem<CameraManager>((com) =>			{ mCameraManager = com; });
		registeFrameSystem<ClassPool>((com) =>				{ mClassPool = com; }, -1, -1, 3101);
		registeFrameSystem<ClassPoolThread>((com) =>		{ mClassPoolThread = com; }, -1, -1, 3102);
		registeFrameSystem<ListPool>((com) =>				{ mListPool = com; }, -1, -1, 3103);
		registeFrameSystem<HashSetPool>((com) =>			{ mHashSetPool = com; }, -1, -1, 3104);
		registeFrameSystem<DictionaryPool>((com) =>			{ mDictionaryPool = com; }, -1, -1, 3106);
		registeFrameSystem<ArrayPoolThread>((com) =>		{ mArrayPoolThread = com; }, -1, -1, 3109);
		registeFrameSystem<TPSpriteManager>((com) =>		{ mTPSpriteManager = com; });
		registeFrameSystem<GameObjectPool>((com) =>			{ mGameObjectPool = com; });
		registeFrameSystem<AssetVersionSystem>((com) =>		{ mAssetVersionSystem = com; });
		registeFrameSystem<AsyncTaskGroupManager>((com) =>	{ mAsyncTaskGroupManager = com; });
		registeFrameSystem<ScreenOrientationSystem>((com) =>{ mScreenOrientationSystem = com; });
		registeFrameSystem<WaitingManager>((com) =>			{ mWaitingManager = com; });
		registeFrameSystem<LayoutManager>((com) =>			{ mLayoutManager = com; }, 1000, 1000, -1);      // 布局管理器也需要在最后更新,确保所有游戏逻辑都更新完毕后,再更新界面
		registeFrameSystem<PrefabPoolManager>((com) =>		{ mPrefabPoolManager = com; }, 2000, 2000, 2000);// 物体管理器最后注册,销毁所有缓存的资源对象
		mOnInitFrameSystem?.Invoke();
	}
	protected void unhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		logError(e.ExceptionObject.ToString());
	}
	protected void dumpSystem()
	{
		log("QualitySettings.currentLevel:" + QualitySettings.GetQualityLevel());
		log("QualitySettings.activeColorSpace:" + QualitySettings.activeColorSpace);
		log("Graphics.activeTier:" + Graphics.activeTier);
		log("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType);
		log("SystemInfo.maxTextureSize:" + SystemInfo.maxTextureSize);
		log("SystemInfo.supportsInstancing:" + SystemInfo.supportsInstancing);
		log("SystemInfo.graphicsShaderLevel:" + SystemInfo.graphicsShaderLevel);
		log("PersistentDataPath:" + F_PERSISTENT_DATA_PATH);
		log("StreamingAssetPath:" + F_STREAMING_ASSETS_PATH);
		log("AssetPath:" + F_ASSETS_PATH);
	}
	protected T registeFrameSystem<T>(Action<T> callback, int initOrder = -1, int updateOrder = -1, int destroyOrder = -1) where T : FrameSystem, new()
	{
		return registeFrameSystem<T>((com) => { callback?.Invoke(com as T); }, initOrder, updateOrder, destroyOrder);
	}
	// 注册时可以指定组件的初始化顺序,更新顺序,销毁顺序
	protected T registeFrameSystem<T>(Action<FrameSystem> callback, int initOrder = -1, int updateOrder = -1, int destroyOrder = -1) where T : FrameSystem, new()
	{
		log("注册系统:" + typeof(T) + ", owner:" + GetType());
		T com = new();
		string name = typeof(T).ToString();
		com.setName(name);
		com.setInitOrder(initOrder == -1 ? mFrameComponentMap.Count : initOrder);
		com.setUpdateOrder(updateOrder == -1 ? mFrameComponentMap.Count : updateOrder);
		com.setDestroyOrder(destroyOrder == -1 ? mFrameComponentMap.Count : destroyOrder);
		mFrameComponentMap.Add(name, com);
		mFrameComponentInit.Add(com);
		mFrameComponentUpdate.Add(com);
		mFrameComponentDestroy.Add(com);
		mFrameCallbackList.Add(name, callback);
		callback?.Invoke(com);
		return com;
	}
}