using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System.Threading;
using System.Net;

public partial class GameFramework : MonoBehaviour
{
	public static GameFramework mGameFramework;                     // 框架单例
	protected Dictionary<string, FrameSystem> mFrameComponentMap;   // 存储框架组件,用于查找
	protected List<FrameSystem> mFrameComponentInit;                // 存储框架组件,用于初始化
	protected List<FrameSystem> mFrameComponentUpdate;              // 存储框架组件,用于更新
	protected List<FrameSystem> mFrameComponentDestroy;             // 存储框架组件,用于销毁
	protected ThreadTimeLock mTimeLock;                             // 用于主线程锁帧,与Application.targetFrameRate功能类似
	protected GameObject mGameFrameObject;                          // 游戏框架根节点
	protected DateTime mCurTime;                                    // 记录当前时间
	protected float mThisFrameTime;                                 // 当前这一帧的消耗时间
	protected int mCurFrameCount;                                   // 当前已执行的帧数量
	protected int mFPS;                                             // 当前帧率
	protected bool mResourceAvailable;								// 资源是否已经可用
	public void Start()
	{
		Profiler.BeginSample("Start");
		CSharpUtility.setMainThreadID(Thread.CurrentThread.ManagedThreadId);
		mTimeLock = new ThreadTimeLock(15);
		Application.targetFrameRate = 60;
		ServicePointManager.DefaultConnectionLimit = 200;
		AppDomain.CurrentDomain.UnhandledException += UnhandledException;
		mFrameComponentMap = new Dictionary<string, FrameSystem>();
		mFrameComponentInit = new List<FrameSystem>();
		mFrameComponentUpdate = new List<FrameSystem>();
		mFrameComponentDestroy = new List<FrameSystem>();

		BinaryUtility.initUtility();
		StringUtility.initUtility();
		MathUtility.initUtility();
		FileUtility.initUtility();
		UnityUtility.initUtility();
		WidgetUtility.initUtility();

		// 设置默认的日志等级
		UnityUtility.setLogLevel(mLogLevel);

		// 本地日志的初始化在移动平台上依赖于插件,所以在本地日志系统之前注册插件
		registeFrameSystem(typeof(AndroidPluginManager));
		registeFrameSystem(typeof(AndroidAssetLoader));
#if !UNITY_EDITOR
		// 由于本地日志系统的特殊性,必须在最开始就初始化,由于会出现问题,暂时禁用
		//FrameBase.mLocalLog = new LocalLog();
		//FrameBase.mLocalLog.init();
#endif
		UnityUtility.setLogCallback(getLogCallback());
		UnityUtility.logForce("start game!");
		UnityUtility.logForce("QualitySettings.currentLevel:" + QualitySettings.GetQualityLevel());
		UnityUtility.logForce("QualitySettings.activeColorSpace:" + QualitySettings.activeColorSpace);
		UnityUtility.logForce("Graphics.activeTier:" + Graphics.activeTier);
		UnityUtility.logForce("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType);
		UnityUtility.logForce("SystemInfo.maxTextureSize:" + SystemInfo.maxTextureSize);
		UnityUtility.logForce("SystemInfo.supportsInstancing:" + SystemInfo.supportsInstancing);
		UnityUtility.logForce("SystemInfo.graphicsShaderLevel:" + SystemInfo.graphicsShaderLevel);
		try
		{
			DateTime startTime = DateTime.Now;
			start();
			UnityUtility.logForce("start消耗时间:" + (DateTime.Now - startTime).TotalMilliseconds);
			// 根据设置的顺序对列表进行排序
			sortList();
			notifyBase();
			registe();
			init();
		}
		catch (Exception e)
		{
			string innerMessage = e.InnerException != null ? e.InnerException.Message : "empty";
			UnityUtility.logError("init failed! " + e.Message + ", inner exception:" + innerMessage + "\nstack:" + e.StackTrace);
		}
		// 初始化完毕后启动游戏
		launch();
		mCurTime = DateTime.Now;
		Profiler.EndSample();
	}
	public void Update()
	{
		try
		{
			// 每帧刷新一次远端时间
			TimeUtility.generateRemoteTimeStampMS();
			++mCurFrameCount;
			DateTime now = DateTime.Now;
			if ((now - mCurTime).TotalMilliseconds >= 1000.0f)
			{
				mFPS = mCurFrameCount;
				mCurFrameCount = 0;
				mCurTime = now;
			}
			mThisFrameTime = MathUtility.clampMax((float)(mTimeLock.update() * 0.001) * Time.timeScale, 0.3f);
			update(mThisFrameTime);
			keyProcess();
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
			//FrameBase.mLocalLog.update(mThisFrameTime);
#endif
		}
		catch (Exception e)
		{
			UnityUtility.logError(e.Message + ", stack:" + e.StackTrace);
			if (e.InnerException != null)
			{
				UnityUtility.logError("inner exception:" + e.InnerException.Message + ", stack:" + e.InnerException.StackTrace);
			}
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
			UnityUtility.logError(e.Message + ", stack:" + e.StackTrace);
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
			UnityUtility.logError(e.Message + ", stack:" + e.StackTrace);
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
			UnityUtility.logError(e.Message + ", stack:" + e.StackTrace);
		}
	}
	public void OnApplicationQuit()
	{
		destroy();
		UnityUtility.logForce("程序退出完毕!");
#if !UNITY_EDITOR
		//FrameBase.mLocalLog?.destroy();
		//FrameBase.mLocalLog = null;
#endif
	}
	// 当资源更新完毕后,由外部进行调用
	public void resourceAvailable()
	{
		mResourceAvailable = true;
		int count = mFrameComponentInit.Count;
		for (int i = 0; i < count; ++i)
		{
			mFrameComponentInit[i].resourceAvailable();
		}
	}
	public bool isResourceAvailable() { return mResourceAvailable; }
	public void hotFixInited()
	{
		int count = mFrameComponentInit.Count;
		for (int i = 0; i < count; ++i)
		{
			mFrameComponentInit[i].hotFixInited();
		}
	}
	public virtual void destroy()
	{
		if (mFrameComponentDestroy == null)
		{
			return;
		}
		int count = mFrameComponentDestroy.Count;
		for (int i = 0; i < count; ++i)
		{
			FrameSystem frameSystem = mFrameComponentDestroy[i];
			if (frameSystem == null)
			{
				continue;
			}
			UnityUtility.logForce("start destroy:" + frameSystem.getName());
			frameSystem.destroy();
		}
		mFrameComponentInit.Clear();
		mFrameComponentUpdate.Clear();
		mFrameComponentDestroy.Clear();
		mFrameComponentMap.Clear();
		mFrameComponentInit = null;
		mFrameComponentUpdate = null;
		mFrameComponentDestroy = null;
		mFrameComponentMap = null;
		// 所有系统组件都销毁完毕后,刷新GameBase和FrameBase中记录的变量
		notifyBase();
	}
	public void stop()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
	public virtual void keyProcess()
	{
		// F1切换日志等级
		if (FrameUtility.isKeyCurrentDown(KeyCode.F1))
		{
			LOG_LEVEL newLevel = (LOG_LEVEL)(((int)UnityUtility.getLogLevel() + 1) % (int)(LOG_LEVEL.FORCE + 1));
			UnityUtility.setLogLevel(newLevel);
			UnityUtility.logForce("当前日志等级:" + newLevel);
		}
		// F2检测当前鼠标坐标下有哪些窗口
		if (FrameUtility.isKeyCurrentDown(KeyCode.F2))
		{
			Vector3 mousePos = FrameUtility.getMousePosition();
			FrameUtility.LIST(out List<IMouseEventCollect> hoverList);
			FrameBase.mGlobalTouchSystem.getAllHoverWindow(hoverList, mousePos, null, true);
			int resultCount = hoverList.Count;
			for (int i = 0; i < resultCount; ++i)
			{
				UIDepth depth = hoverList[i].getDepth();
				UnityUtility.logForce("窗口:" + hoverList[i].getName() +
									", 深度:layout:" + depth.toDepthString() +
									", priority:" + depth.getPriority());
			}
			FrameUtility.UN_LIST(hoverList);
		}
		// F3启用或禁用用作调试的脚本的更新
		if (FrameUtility.isKeyCurrentDown(KeyCode.F3))
		{
			mEnableScriptDebug = !mEnableScriptDebug;
			UnityUtility.logForce(mEnableScriptDebug ? "已开启调试脚本" : "已关闭调试脚本", mEnableScriptDebug ? Color.green : Color.red);
		}
		// F4启用或禁用
		if (FrameUtility.isKeyCurrentDown(KeyCode.F4))
		{
			mEnablePoolStackTrace = !mEnablePoolStackTrace;
			UnityUtility.logForce(mEnablePoolStackTrace ? "已开启对象池分配堆栈追踪" : "已关闭对象池分配堆栈追踪", mEnablePoolStackTrace ? Color.green : Color.red);
		}
	}
	public FrameSystem getSystem(Type type)
	{
		if (mFrameComponentMap != null && mFrameComponentMap.TryGetValue(type.ToString(), out FrameSystem frameSystem))
		{
			return frameSystem;
		}
		return null;
	}
	public GameObject getGameFrameObject() { return mGameFrameObject; }
	public int getFPS() { return mFPS; }
	public void destroyComponent<T>(ref T com) where T : FrameSystem
	{
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			if (mFrameComponentInit[i] == com)
			{
				mFrameComponentInit[i] = null;
			}
			if (mFrameComponentUpdate[i] == com)
			{
				mFrameComponentUpdate[i] = null;
			}
			if (mFrameComponentDestroy[i] == com)
			{
				mFrameComponentDestroy[i] = null;
			}
		}
		string name = com.getName();
		mFrameComponentMap.Remove(name);
		com.destroy();
		com = null;
		notifyBase();
	}
	// 注册时可以指定组件的初始化顺序,更新顺序,销毁顺序
	public FrameSystem registeFrameSystem(Type type, int initOrder = -1, int updateOrder = -1, int destroyOrder = -1)
	{
		string name = type.ToString();
		var com = CSharpUtility.createInstance<FrameSystem>(type);
		com.setName(name);
		com.setInitOrder(initOrder == -1 ? mFrameComponentMap.Count : initOrder);
		com.setUpdateOrder(updateOrder == -1 ? mFrameComponentMap.Count : updateOrder);
		com.setDestroyOrder(destroyOrder == -1 ? mFrameComponentMap.Count : destroyOrder);
		mFrameComponentMap.Add(name, com);
		mFrameComponentInit.Add(com);
		mFrameComponentUpdate.Add(com);
		mFrameComponentDestroy.Add(com);
		return com;
	}
	public void sortList()
	{
		mFrameComponentInit.Sort(FrameSystem.compareInit);
		mFrameComponentUpdate.Sort(FrameSystem.compareUpdate);
		mFrameComponentDestroy.Sort(FrameSystem.compareDestroy);
	}
	//--------------------------------------------------------------------------------------------------------------------------------------------
	protected virtual void update(float elapsedTime)
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
			if (com != null && !com.isDestroy())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Profiler.BeginSample(com.getName());
#endif
				com.update(elapsedTime);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Profiler.EndSample();
#endif
			}
		}
	}
	protected virtual void fixedUpdate(float elapsedTime)
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
			if (com != null && !com.isDestroy())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Profiler.BeginSample(com.getName());
#endif
				com.fixedUpdate(elapsedTime);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Profiler.EndSample();
#endif
			}
		}
	}
	protected virtual void lateUpdate(float elapsedTime)
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
			if (com != null && !com.isDestroy())
			{
				com.lateUpdate(elapsedTime);
			}
		}
	}
	protected virtual void drawGizmos()
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
			if (com != null && !com.isDestroy())
			{
				com.onDrawGizmos();
			}
		}
	}
	protected virtual OnLog getLogCallback() { return null; }
	protected virtual void notifyBase()
	{
		// 所有类都构造完成后通知FrameBase
		FrameBase frameBase = new FrameBase();
		frameBase.notifyConstructDone();
	}
	protected virtual void start()
	{
		mGameFramework = this;
		mGameFrameObject = gameObject;
		initFrameSystem();
	}
	protected virtual void init()
	{
		// 必须先初始化配置文件
		int count = mFrameComponentInit.Count;
		for (int i = 0; i < count; ++i)
		{
			try
			{
				DateTime start = DateTime.Now;
				mFrameComponentInit[i].init();
				UnityUtility.logForce(mFrameComponentInit[i].getName() + "初始化消耗时间:" + (DateTime.Now - start).TotalMilliseconds);
			}
			catch (Exception e)
			{
				UnityUtility.logError("init failed! :" + mFrameComponentInit[i].getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
			}
		}

		WINDOW_MODE fullScreen = mWindowMode;
#if UNITY_EDITOR
		// 编辑器下固定全屏
		fullScreen = WINDOW_MODE.FULL_SCREEN;
#elif UNITY_STANDALONE_WIN
		// 设置为无边框窗口,只在Windows平台使用
		if (fullScreen == WINDOW_MODE.NO_BOARD_WINDOW)
		{
			// 无边框的设置有时候会失效,并且同样的设置,如果上一次设置失效后,即便恢复设置也同样会失效,也就是说本次的是否生效与上一次的结果有关
			// 当设置失效后,可以使用添加启动参数-popupwindow来实现无边框
			long curStyle = User32.GetWindowLong(User32.GetForegroundWindow(), FrameDefine.GWL_STYLE);
			curStyle &= ~FrameDefine.WS_BORDER;
			curStyle &= ~FrameDefine.WS_DLGFRAME;
			User32.SetWindowLong(User32.GetForegroundWindow(), FrameDefine.GWL_STYLE, curStyle);
		}
#elif UNITY_ANDROID || UNITY_IOS
		// 移动平台下固定为全屏
		fullScreen = WINDOW_MODE.FULL_SCREEN;
#endif
		Vector2 windowSize;
		if (fullScreen == WINDOW_MODE.FULL_SCREEN)
		{
			windowSize = UnityUtility.getScreenSize();
		}
		else
		{
			windowSize.x = mScreenWidth;
			windowSize.y = mScreenHeight;
		}
		Screen.SetResolution((int)windowSize.x, (int)windowSize.y, fullScreen == WINDOW_MODE.FULL_SCREEN || fullScreen == WINDOW_MODE.FULL_SCREEN_CUSTOM_RESOLUTION);
		// UGUI
		GameObject uguiRootObj = UnityUtility.getGameObject(FrameDefine.UGUI_ROOT);
		RectTransform uguiRectTransform = uguiRootObj.GetComponent<RectTransform>();
		uguiRectTransform.offsetMin = -windowSize * 0.5f;
		uguiRectTransform.offsetMax = windowSize * 0.5f;
		uguiRectTransform.anchorMax = Vector2.one * 0.5f;
		uguiRectTransform.anchorMin = Vector2.one * 0.5f;
		GameCamera camera = FrameBase.mCameraManager.getUICamera();
		if (camera != null)
		{
			FT.MOVE(camera, new Vector3(0.0f, 0.0f, -windowSize.y * 0.5f / MathUtility.tan(camera.getFOVY(true) * 0.5f)));
		}
		GameCamera blurCamera = FrameBase.mCameraManager.getUIBlurCamera();
		if (blurCamera != null)
		{
			FT.MOVE(blurCamera, new Vector3(0.0f, 0.0f, -windowSize.y * 0.5f / MathUtility.tan(blurCamera.getFOVY(true) * 0.5f)));
		}
	}
	protected virtual void registe() { }
	protected virtual void launch() { }
	protected virtual void initFrameSystem()
	{
		registeFrameSystem(typeof(ResourceManager), -1, 3000, 3000);    // 资源管理器的需要最先初始化,并且是最后被销毁,作为最后的资源清理
		registeFrameSystem(typeof(TimeManager));
		registeFrameSystem(typeof(GlobalCmdReceiver));
		registeFrameSystem(typeof(HttpUtility));
#if !UNITY_IOS && !NO_SQLITE
		registeFrameSystem(typeof(SQLiteManager));
#endif
		registeFrameSystem(typeof(CommandSystem), -1, -1, 2001);		// 命令系统在大部分管理器都销毁完毕后再销毁
		registeFrameSystem(typeof(InputSystem));						// 输入系统应该早点更新,需要更新输入的状态,以便后续的系统组件中使用
		registeFrameSystem(typeof(GlobalTouchSystem));
		registeFrameSystem(typeof(TweenerManager));
		registeFrameSystem(typeof(CharacterManager));
		registeFrameSystem(typeof(AudioManager));
		registeFrameSystem(typeof(GameSceneManager));
		registeFrameSystem(typeof(KeyFrameManager));
		registeFrameSystem(typeof(DllImportSystem));
		registeFrameSystem(typeof(ShaderManager));
		registeFrameSystem(typeof(CameraManager));
		registeFrameSystem(typeof(SceneSystem));
		registeFrameSystem(typeof(GamePluginManager));
		registeFrameSystem(typeof(ClassPool), -1, -1, 3101);
		registeFrameSystem(typeof(ClassPoolThread), -1, -1, 3102);
		registeFrameSystem(typeof(ListPool), -1, -1, 3103);
		registeFrameSystem(typeof(ListPoolThread), -1, -1, 3104);
		registeFrameSystem(typeof(HashSetPool), -1, -1, 3104);
		registeFrameSystem(typeof(HashSetPoolThread), -1, -1, 3105);
		registeFrameSystem(typeof(DictionaryPool), -1, -1, 3106);
		registeFrameSystem(typeof(DictionaryPoolThread), -1, -1, 3107);
		registeFrameSystem(typeof(ArrayPool), -1, -1, 3108);
		registeFrameSystem(typeof(ArrayPoolThread), -1, -1, 3109);
		registeFrameSystem(typeof(HeadTextureManager));
		registeFrameSystem(typeof(MovableObjectManager));
		registeFrameSystem(typeof(EffectManager));
		registeFrameSystem(typeof(TPSpriteManager));
		registeFrameSystem(typeof(SocketFactory));
		registeFrameSystem(typeof(SocketFactoryThread));
		registeFrameSystem(typeof(PathKeyframeManager));
		registeFrameSystem(typeof(EventSystem));
		registeFrameSystem(typeof(StateManager));
		registeFrameSystem(typeof(SocketTypeManager));
		registeFrameSystem(typeof(GameObjectPool));
		registeFrameSystem(typeof(ExcelManager));
#if USE_ILRUNTIME
		// ILRSystem需要在很多系统后面销毁,因为很多Game层的系统位于ILRuntime中,需要等到所有其他系统销毁后,ILRSystem才能销毁
		registeFrameSystem(typeof(ILRSystem), -1, -1, 3999);
#endif
		registeFrameSystem(typeof(LayoutManager), 1000, 1000, -1);      // 布局管理器也需要在最后更新,确保所有游戏逻辑都更新完毕后,再更新界面
		registeFrameSystem(typeof(ObjectPool), 2000, 2000, 2000);       // 物体管理器最后注册,销毁所有缓存的资源对象
	}
	protected void UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		UnityUtility.logError(e.ExceptionObject.ToString());
	}
}