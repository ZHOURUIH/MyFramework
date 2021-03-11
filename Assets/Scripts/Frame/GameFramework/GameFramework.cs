using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System.Threading;

public class GameFramework : MonoBehaviour
{
	public static GameFramework mGameFramework;
	protected Dictionary<string, FrameSystem> mFrameComponentMap;   // 存储框架组件,用于查找
	protected List<FrameSystem> mFrameComponentInit;                // 存储框架组件,用于初始化
	protected List<FrameSystem> mFrameComponentUpdate;              // 存储框架组件,用于更新
	protected List<FrameSystem> mFrameComponentDestroy;             // 存储框架组件,用于销毁
	protected ThreadTimeLock mTimeLock;
	protected GameObject mGameFrameObject;
	protected DateTime mCurTime;
	protected float mThisFrameTime;
	protected bool mEnableScriptDebug;
	protected bool mEnableKeyboard;
	protected bool mPauseFrame;
	protected int mCurFrameCount;
	protected int mFPS;
	public void Start()
	{
		UnityUtility.setMainThreadID(Thread.CurrentThread.ManagedThreadId);
		Application.targetFrameRate = 60;
		AppDomain app = AppDomain.CurrentDomain;
		app.UnhandledException += UnhandledException;
		mFrameComponentMap = new Dictionary<string, FrameSystem>();
		mFrameComponentInit = new List<FrameSystem>();
		mFrameComponentUpdate = new List<FrameSystem>();
		mFrameComponentDestroy = new List<FrameSystem>();
		mTimeLock = new ThreadTimeLock(15);
		// 本地日志的初始化在移动平台上依赖于插件,所以在本地日志系统之前注册插件
		registeFrameSystem(UnityUtility.Typeof<AndroidPluginManager>());
		registeFrameSystem(UnityUtility.Typeof<AndroidAssetLoader>());
#if !UNITY_EDITOR
		// 由于本地日志系统的特殊性,必须在最开始就初始化
		FrameBase.mLocalLog = new LocalLog();
		FrameBase.mLocalLog.init();
#endif
		UnityUtility.setLogCallback(getLogCallback());
		UnityUtility.log("start game!", LOG_LEVEL.FORCE);
		UnityUtility.log("QualitySettings.currentLevel:" + QualitySettings.GetQualityLevel(), LOG_LEVEL.FORCE);
		UnityUtility.log("QualitySettings.activeColorSpace:" + QualitySettings.activeColorSpace, LOG_LEVEL.FORCE);
		UnityUtility.log("Graphics.activeTier:" + Graphics.activeTier, LOG_LEVEL.FORCE);
		UnityUtility.log("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType, LOG_LEVEL.FORCE);
		UnityUtility.log("SystemInfo.maxTextureSize:" + SystemInfo.maxTextureSize, LOG_LEVEL.FORCE);
		UnityUtility.log("SystemInfo.supportsInstancing:" + SystemInfo.supportsInstancing, LOG_LEVEL.FORCE);
		UnityUtility.log("SystemInfo.graphicsShaderLevel:" + SystemInfo.graphicsShaderLevel, LOG_LEVEL.FORCE);
		try
		{
			DateTime startTime = DateTime.Now;
			start();
			UnityUtility.log("start消耗时间:" + (DateTime.Now - startTime).TotalMilliseconds);
			// 根据设置的顺序对列表进行排序
			mFrameComponentInit.Sort(FrameSystem.compareInit);
			mFrameComponentUpdate.Sort(FrameSystem.compareUpdate);
			mFrameComponentDestroy.Sort(FrameSystem.compareDestroy);
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
	}
	void UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		UnityUtility.logError(e.ExceptionObject.ToString());
	}
	public void Update()
	{
		try
		{
			++mCurFrameCount;
			DateTime now = DateTime.Now;
			if ((now - mCurTime).TotalMilliseconds >= 1000.0f)
			{
				mFPS = mCurFrameCount;
				mCurFrameCount = 0;
				mCurTime = now;
			}
			if (mPauseFrame)
			{
				return;
			}
			mThisFrameTime = (float)(mTimeLock.update() * 0.001f) * Time.timeScale;
#if UNITY_EDITOR
			MathUtility.clampMax(ref mThisFrameTime, 0.05f);
			mThisFrameTime = Time.deltaTime;
#endif
			update(mThisFrameTime);
			keyProcess();
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
			FrameBase.mLocalLog.update(mThisFrameTime);
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
			if (mPauseFrame)
			{
				return;
			}
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
			if (mPauseFrame)
			{
				return;
			}
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
		UnityUtility.log("程序退出完毕!", LOG_LEVEL.FORCE);
#if !UNITY_EDITOR
		FrameBase.mLocalLog?.destroy();
		FrameBase.mLocalLog = null;
#endif
	}
	public virtual void destroy()
	{
		int count = mFrameComponentDestroy.Count;
		for (int i = 0; i < count; ++i)
		{
			mFrameComponentDestroy[i]?.destroy();
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
		if (FrameBase.getKeyCurrentDown(KeyCode.F1))
		{
			int newLevel = ((int)UnityUtility.getLogLevel() + 1) % (int)LOG_LEVEL.MAX;
			UnityUtility.setLogLevel((LOG_LEVEL)newLevel);
		}
		// F2检测当前鼠标坐标下有哪些窗口
		if (FrameBase.getKeyCurrentDown(KeyCode.F2))
		{
			Vector3 mousePos = FrameBase.getMousePosition();
			var resultList = FrameBase.mGlobalTouchSystem.getAllHoverWindow(ref mousePos, null, true);
			int resultCount = resultList.Count;
			for (int i = 0; i < resultCount; ++i)
			{
				UIDepth depth = resultList[i].getDepth();
				UnityUtility.log("窗口:" + resultList[i].getName() +
										", 深度:layout:" + depth.toDepthString() +
										", priority:" + depth.getPriority(),
										LOG_LEVEL.FORCE);
			}
		}
		// F3启用或禁用用作调试的脚本的更新
		if (FrameBase.getKeyCurrentDown(KeyCode.F3))
		{
			mEnableScriptDebug = !mEnableScriptDebug;
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
	public bool isEnableScriptDebug() { return mEnableScriptDebug; }
	public void setPasueFrame(bool value) { mPauseFrame = value; }
	public bool isPasueFrame() { return mPauseFrame; }
	public GameObject getGameFrameObject() { return mGameFrameObject; }
	public bool isEnableKeyboard() { return mEnableKeyboard; }
	public int getFPS() { return mFPS; }
	public void destroyComponent<T>(ref T component) where T : FrameSystem
	{
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			if (mFrameComponentInit[i] == component)
			{
				mFrameComponentInit[i] = null;
			}
			if (mFrameComponentUpdate[i] == component)
			{
				mFrameComponentUpdate[i] = null;
			}
			if (mFrameComponentDestroy[i] == component)
			{
				mFrameComponentDestroy[i] = null;
			}
		}
		string name = component.getName();
		mFrameComponentMap.Remove(name);
		component.destroy();
		component = null;
		notifyBase();
	}
	//------------------------------------------------------------------------------------------------------
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
			FrameSystem component = mFrameComponentUpdate[i];
			if (component != null && !component.isDestroy())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Profiler.BeginSample(component.getName());
#endif
				component.update(elapsedTime);
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
			FrameSystem component = mFrameComponentUpdate[i];
			if (component != null && !component.isDestroy())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Profiler.BeginSample(component.getName());
#endif
				component.fixedUpdate(elapsedTime);
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
			FrameSystem component = mFrameComponentUpdate[i];
			if (component != null && !component.isDestroy())
			{
				component.lateUpdate(elapsedTime);
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
			FrameSystem component = mFrameComponentUpdate[i];
			if (component != null && !component.isDestroy())
			{
				component.onDrawGizmos();
			}
		}
	}
	protected virtual onLog getLogCallback() { return null; }
	protected virtual void notifyBase()
	{
		// 所有类都构造完成后通知FrameBase
		FrameBase frameBase = new FrameBase();
		frameBase.notifyConstructDone();
	}
	protected virtual void start()
	{
		mPauseFrame = false;
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
				UnityUtility.log(mFrameComponentInit[i].getName() + "初始化消耗时间:" + (DateTime.Now - start).TotalMilliseconds);
			}
			catch (Exception e)
			{
				UnityUtility.logError("init failed! :" + mFrameComponentInit[i].getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
			}
		}
		System.Net.ServicePointManager.DefaultConnectionLimit = 200;
		// 默认不启用调试脚本
		mEnableScriptDebug = false;
		mEnableKeyboard = (int)FrameBase.mFrameConfig.getFloat(GAME_FLOAT.ENABLE_KEYBOARD) > 0;
		ApplicationConfig appConfig = FrameBase.mApplicationConfig;
		QualitySettings.vSyncCount = (int)appConfig.getFloat(GAME_FLOAT.VSYNC);
		int width = (int)appConfig.getFloat(GAME_FLOAT.SCREEN_WIDTH);
		int height = (int)appConfig.getFloat(GAME_FLOAT.SCREEN_HEIGHT);
		int fullScreen = (int)appConfig.getFloat(GAME_FLOAT.FULL_SCREEN);
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		// 移动平台下固定为全屏
		fullScreen = 1;
#endif
		Vector2 screenSize = UnityUtility.getScreenSize();
		if (fullScreen == 1)
		{
			width = (int)screenSize.x;
			height = (int)screenSize.y;
		}
		Screen.SetResolution(width, height, fullScreen == 1 || fullScreen == 3);
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
		// 设置为无边框窗口,只在Windows平台使用
		if (fullScreen == 2)
		{
			// 无边框的设置有时候会失效,并且同样的设置,如果上一次设置失效后,即便恢复设置也同样会失效,也就是说本次的是否生效与上一次的结果有关
			// 当设置失效后,可以使用添加启动参数-popupwindow来实现无边框
			long curStyle = User32.GetWindowLong(User32.GetForegroundWindow(), FrameDefine.GWL_STYLE);
			curStyle &= ~FrameDefine.WS_BORDER;
			curStyle &= ~FrameDefine.WS_DLGFRAME;
			User32.SetWindowLong(User32.GetForegroundWindow(), FrameDefine.GWL_STYLE, curStyle);
		}
#endif
		// UGUI
		GameObject uguiRootObj = UnityUtility.getGameObject(FrameDefine.UGUI_ROOT);
		RectTransform uguiRectTransform = uguiRootObj.GetComponent<RectTransform>();
		uguiRectTransform.offsetMin = -screenSize * 0.5f;
		uguiRectTransform.offsetMax = screenSize * 0.5f;
		uguiRectTransform.anchorMax = Vector2.one * 0.5f;
		uguiRectTransform.anchorMin = Vector2.one * 0.5f;
		GameCamera camera = FrameBase.mCameraManager.getUICamera();
		if (camera != null)
		{
			FT.MOVE(camera, new Vector3(0.0f, 0.0f, -height * 0.5f / MathUtility.tan(camera.getFOVY(true) * 0.5f)));
		}
		GameCamera blurCamera = FrameBase.mCameraManager.getUIBlurCamera();
		if (blurCamera != null)
		{
			FT.MOVE(blurCamera, new Vector3(0.0f, 0.0f, -height * 0.5f / MathUtility.tan(blurCamera.getFOVY(true) * 0.5f)));
		}
		// 设置默认的日志等级
#if UNITY_EDITOR
		UnityUtility.setLogLevel(LOG_LEVEL.NORMAL);
#else
		UnityUtility.setLogLevel((LOG_LEVEL)(int)FrameBase.mFrameConfig.getFloat(GAME_FLOAT.LOG_LEVEL));
#endif
	}
	protected virtual void registe() { }
	protected virtual void launch() { }
	protected virtual void initFrameSystem()
	{
		registeFrameSystem(UnityUtility.Typeof<TimeManager>());
		registeFrameSystem(UnityUtility.Typeof<ApplicationConfig>());
		registeFrameSystem(UnityUtility.Typeof<FrameConfig>());
		registeFrameSystem(UnityUtility.Typeof<HttpUtility>());
#if !UNITY_IOS && !NO_SQLITE
		registeFrameSystem(UnityUtility.Typeof<SQLite>());
#endif
		registeFrameSystem(UnityUtility.Typeof<DataBase>());
		registeFrameSystem(UnityUtility.Typeof<CommandSystem>(), -1, -1, 2001);  // 命令系统在大部分管理器都销毁完毕后再销毁
		registeFrameSystem(UnityUtility.Typeof<GlobalTouchSystem>());
		registeFrameSystem(UnityUtility.Typeof<CharacterManager>());
		registeFrameSystem(UnityUtility.Typeof<AudioManager>());
		registeFrameSystem(UnityUtility.Typeof<GameSceneManager>());
		registeFrameSystem(UnityUtility.Typeof<KeyFrameManager>());
		registeFrameSystem(UnityUtility.Typeof<DllImportExtern>());
		registeFrameSystem(UnityUtility.Typeof<ShaderManager>());
		registeFrameSystem(UnityUtility.Typeof<CameraManager>());
		registeFrameSystem(UnityUtility.Typeof<InputManager>());
		registeFrameSystem(UnityUtility.Typeof<SceneSystem>());
		registeFrameSystem(UnityUtility.Typeof<GamePluginManager>());
		registeFrameSystem(UnityUtility.Typeof<ClassPool>(), -1, -1, 3101);
		registeFrameSystem(UnityUtility.Typeof<ClassPoolThread>(), -1, -1, 3102);
		registeFrameSystem(UnityUtility.Typeof<ListPool>(), -1, -1, 3103);
		registeFrameSystem(UnityUtility.Typeof<ListPoolThread>(), -1, -1, 3104);
		registeFrameSystem(UnityUtility.Typeof<DictionaryPool>(), -1, -1, 3105);
		registeFrameSystem(UnityUtility.Typeof<DictionaryPoolThread>(), -1, -1, 3106);
		registeFrameSystem(UnityUtility.Typeof<BytesPool>(), -1, -1, 3107);
		registeFrameSystem(UnityUtility.Typeof<BytesPoolThread>(), -1, -1, 3108);
		registeFrameSystem(UnityUtility.Typeof<HeadTextureManager>());
		registeFrameSystem(UnityUtility.Typeof<MovableObjectManager>());
		registeFrameSystem(UnityUtility.Typeof<EffectManager>());
		registeFrameSystem(UnityUtility.Typeof<TPSpriteManager>());
		registeFrameSystem(UnityUtility.Typeof<SocketFactory>());
		registeFrameSystem(UnityUtility.Typeof<SocketFactoryThread>());
		registeFrameSystem(UnityUtility.Typeof<PathKeyframeManager>());
		registeFrameSystem(UnityUtility.Typeof<EventSystem>());
		registeFrameSystem(UnityUtility.Typeof<StringBuilderPool>());
		registeFrameSystem(UnityUtility.Typeof<StringBuilderPoolThread>());
#if USE_ILRUNTIME
		registeFrameSystem(UnityUtility.Typeof<ILRSystem>());
#endif
		// 布局管理器也需要在最后更新,确保所有游戏逻辑都更新完毕后,再更新界面
		registeFrameSystem(UnityUtility.Typeof<LayoutManager>(), 1000, 1000, 1000);
		// 物体管理器和资源管理器必须最后注册,以便最后销毁,作为最后的资源清理
		registeFrameSystem(UnityUtility.Typeof<ObjectPool>(), 2000, 2000, 2000);
		registeFrameSystem(UnityUtility.Typeof<ResourceManager>(), 3000, 3000, 3000);
	}
	// 注册时可以指定组件的初始化顺序,更新顺序,销毁顺序
	protected void registeFrameSystem(Type type, int initOrder = -1, int updateOrder = -1, int destroyOrder = -1)
	{
		string name = type.ToString();
		var component = UnityUtility.createInstance<FrameSystem>(type);
		component.setName(name);
		component.setInitOrder(initOrder == -1 ? mFrameComponentMap.Count : initOrder);
		component.setUpdateOrder(updateOrder == -1 ? mFrameComponentMap.Count : updateOrder);
		component.setDestroyOrder(destroyOrder == -1 ? mFrameComponentMap.Count : destroyOrder);
		mFrameComponentMap.Add(name, component);
		mFrameComponentInit.Add(component);
		mFrameComponentUpdate.Add(component);
		mFrameComponentDestroy.Add(component);
	}
}