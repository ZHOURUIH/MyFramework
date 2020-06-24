using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Profiling;

public class GameFramework : MonoBehaviour
{
	public static GameFramework		mGameFramework;
	protected Dictionary<string, FrameComponent> mFrameComponentMap;	// 存储框架组件,用于查找
	protected List<FrameComponent>	mFrameComponentInit;				// 存储框架组件,用于初始化
	protected List<FrameComponent>	mFrameComponentUpdate;				// 存储框架组件,用于更新
	protected List<FrameComponent>	mFrameComponentDestroy;				// 存储框架组件,用于销毁
	protected GameObject			mGameFrameObject;
	protected bool					mPauseFrame;
	protected bool					mEnableKeyboard;
	protected int					mFPS;
	protected DateTime				mCurTime;
	protected int					mCurFrameCount;
	protected ThreadTimeLock		mTimeLock;
	protected float					mThisFrameTime;
	public void Start()
	{
		Application.targetFrameRate = 120;
		AppDomain app = AppDomain.CurrentDomain;
		app.UnhandledException += UnhandledException;
		mFrameComponentMap = new Dictionary<string, FrameComponent>();
		mFrameComponentInit = new List<FrameComponent>();
		mFrameComponentUpdate = new List<FrameComponent>();
		mFrameComponentDestroy = new List<FrameComponent>();
		mTimeLock = new ThreadTimeLock(15);
		// 本地日志的初始化在移动平台上依赖于插件,所以在本地日志系统之前注册插件
		registeComponent<AndroidPluginManager>();
		registeComponent<AndroidAssetLoader>();
#if !UNITY_EDITOR
		// 由于本地日志系统的特殊性,必须在最开始就初始化
		FrameBase.mLocalLog = new LocalLog();
		FrameBase.mLocalLog.init();
#endif
		UnityUtility.mOnLog = getLogCallback();
		UnityUtility.logInfo("start game!", LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("QualitySettings.currentLevel:" + QualitySettings.GetQualityLevel(), LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("QualitySettings.activeColorSpace:" + QualitySettings.activeColorSpace, LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("Graphics.activeTier:" + Graphics.activeTier, LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType, LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("SystemInfo.maxTextureSize:" + SystemInfo.maxTextureSize, LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("SystemInfo.supportsInstancing:" + SystemInfo.supportsInstancing, LOG_LEVEL.LL_FORCE);
		UnityUtility.logInfo("SystemInfo.graphicsShaderLevel:" + SystemInfo.graphicsShaderLevel, LOG_LEVEL.LL_FORCE);
		try
		{
			DateTime startTime = DateTime.Now;
			start();
			UnityUtility.logInfo("start消耗时间:" + (DateTime.Now - startTime).TotalMilliseconds);
			// 根据设置的顺序对列表进行排序
			mFrameComponentInit.Sort(FrameComponent.compareInit);
			mFrameComponentUpdate.Sort(FrameComponent.compareUpdate);
			mFrameComponentDestroy.Sort(FrameComponent.compareDestroy);
			notifyBase();
			registe();
			init();
		}
		catch(Exception e)
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
		catch(Exception e)
		{
			UnityUtility.logError(e.Message + ", stack:" + e.StackTrace);
		}
	}
	public void OnApplicationQuit()
	{
		destroy();
		UnityUtility.logInfo("程序退出完毕!", LOG_LEVEL.LL_FORCE);
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
		if(FrameBase.getKeyCurrentDown(KeyCode.F1))
		{
			int newLevel = ((int)UnityUtility.getLogLevel() + 1) % (int)LOG_LEVEL.LL_MAX;
			UnityUtility.setLogLevel((LOG_LEVEL)newLevel);
		}
	}
	public void getSystem<T>(out T component) where T : FrameComponent
	{
		component = null;
		string name = typeof(T).ToString();
		if(mFrameComponentMap != null && mFrameComponentMap.ContainsKey(name))
		{
			component = mFrameComponentMap[name] as T;
		}
	}
	public void setPasueFrame(bool value) { mPauseFrame = value; }
	public bool isPasueFrame() { return mPauseFrame; }
	public GameObject getGameFrameObject() { return mGameFrameObject; }
	public bool isEnableKeyboard() { return mEnableKeyboard; }
	public int getFPS() { return mFPS; }
	public void destroyComponent<T>(ref T component) where T : FrameComponent
	{
		int count = mFrameComponentUpdate.Count;
		for (int i = 0; i < count; ++i)
		{
			if(mFrameComponentInit[i] == component)
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
			FrameComponent component = mFrameComponentUpdate[i];
			if(component != null && !component.mDestroy)
			{
				Profiler.BeginSample(component.getName());
				component.update(elapsedTime);
				Profiler.EndSample();
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
			FrameComponent component = mFrameComponentUpdate[i];
			if (component != null && !component.mDestroy)
			{
				component.fixedUpdate(elapsedTime);
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
			FrameComponent component = mFrameComponentUpdate[i];
			if (component != null && !component.mDestroy)
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
			FrameComponent component = mFrameComponentUpdate[i];
			if (component != null && !component.mDestroy)
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
		initComponent();
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
				UnityUtility.logInfo(mFrameComponentInit[i].getName() + "初始化消耗时间:" + (DateTime.Now - start).TotalMilliseconds);
			}
			catch(Exception e)
			{
				UnityUtility.logError("init failed! :" + mFrameComponentInit[i].getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
			}
		}
		System.Net.ServicePointManager.DefaultConnectionLimit = 200;
		QualitySettings.vSyncCount = (int)FrameBase.mApplicationConfig.getFloatParam(GAME_DEFINE_FLOAT.GDF_VSYNC);
		mEnableKeyboard = (int)FrameBase.mFrameConfig.getFloatParam(GAME_DEFINE_FLOAT.GDF_ENABLE_KEYBOARD) > 0;
		int width = (int)FrameBase.mApplicationConfig.getFloatParam(GAME_DEFINE_FLOAT.GDF_SCREEN_WIDTH);
		int height = (int)FrameBase.mApplicationConfig.getFloatParam(GAME_DEFINE_FLOAT.GDF_SCREEN_HEIGHT);
		int fullScreen = (int)FrameBase.mApplicationConfig.getFloatParam(GAME_DEFINE_FLOAT.GDF_FULL_SCREEN);
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
			long curStyle = User32.GetWindowLong(User32.GetForegroundWindow(), CommonDefine.GWL_STYLE);
			curStyle &= ~CommonDefine.WS_BORDER;
			curStyle &= ~CommonDefine.WS_DLGFRAME;
			User32.SetWindowLong(User32.GetForegroundWindow(), CommonDefine.GWL_STYLE, curStyle);
		}
#endif
		// NGUI
#if USE_NGUI
		GameObject nguiRootObj = UnityUtility.getGameObject(null, CommonDefine.NGUI_ROOT);
		UIRoot nguiRoot = nguiRootObj.GetComponent<UIRoot>();
		nguiRoot.scalingStyle = UIRoot.Scaling.Constrained;
		nguiRoot.manualWidth = width;
		nguiRoot.manualHeight = height;
		GameCamera camera = FrameBase.mCameraManager.getUICamera(false);
		OT.MOVE(camera, new Vector3(0.0f, 0.0f, -height * 0.5f / MathUtility.tan(camera.getFOVY(true)* 0.5f)));
		GameCamera blurCamera = FrameBase.mCameraManager.getUIBlurCamera(false);
		OT.MOVE(blurCamera, new Vector3(0.0f, 0.0f, -height * 0.5f / MathUtility.tan(blurCamera.getFOVY(true) * 0.5f)));
#endif
		// UGUI
		GameObject uguiRootObj = UnityUtility.getGameObject(null, CommonDefine.UGUI_ROOT);
		RectTransform uguiRectTransform = uguiRootObj.GetComponent<RectTransform>();
		uguiRectTransform.offsetMin = -screenSize * 0.5f;
		uguiRectTransform.offsetMax = screenSize * 0.5f;
		uguiRectTransform.anchorMax = Vector2.zero;
		uguiRectTransform.anchorMin = Vector2.zero;
		GameCamera camera = FrameBase.mCameraManager.getUICamera(false);
		if(camera != null)
		{
			OT.MOVE(camera, new Vector3(0.0f, 0.0f, -height * 0.5f / MathUtility.tan(camera.getFOVY(true) * 0.5f)));
		}
		GameCamera blurCamera = FrameBase.mCameraManager.getUIBlurCamera(false);
		if(blurCamera != null)
		{
			OT.MOVE(blurCamera, new Vector3(0.0f, 0.0f, -height * 0.5f / MathUtility.tan(blurCamera.getFOVY(true) * 0.5f)));
		}
		// 设置默认的日志等级
#if UNITY_EDITOR
		UnityUtility.setLogLevel(LOG_LEVEL.LL_NORMAL);
#else
		UnityUtility.setLogLevel((LOG_LEVEL)(int)FrameBase.mFrameConfig.getFloatParam(GAME_DEFINE_FLOAT.GDF_LOG_LEVEL));
#endif
	}
	protected virtual void registe() { }
	protected virtual void launch() { }
	protected virtual void initComponent()
	{
		registeComponent<TimeManager>();
		registeComponent<ApplicationConfig>();
		registeComponent<FrameConfig>();
		registeComponent<HttpUtility>();
#if !UNITY_IOS && !NO_SQLITE
		registeComponent<SQLite>();
#endif
		registeComponent<DataBase>();
		registeComponent<CommandSystem>(-1, -1, 2001);  // 命令系统在大部分管理器都销毁完毕后再销毁
		registeComponent<GlobalTouchSystem>();
		registeComponent<CharacterManager>();
		registeComponent<AudioManager>();
		registeComponent<GameSceneManager>();
		registeComponent<KeyFrameManager>();
		registeComponent<DllImportExtern>();
		registeComponent<ShaderManager>();
		registeComponent<CameraManager>();
		registeComponent<InputManager>();
		registeComponent<SceneSystem>();
		registeComponent<GamePluginManager>();
		registeComponent<ClassPool>();
		registeComponent<HeadTextureManager>();
		registeComponent<MovableObjectManager>();
		registeComponent<EffectManager>();
		registeComponent<TPSpriteManager>();
		registeComponent<SocketFactory>();
		registeComponent<PathKeyframeManager>();
		// 布局管理器也需要在最后更新,确保所有游戏逻辑都更新完毕后,再更新界面
		registeComponent<GameLayoutManager>(1000, 1000, 1000);
		// 物体管理器和资源管理器必须最后注册,以便最后销毁,作为最后的资源清理
		registeComponent<ObjectPool>(2000, 2000, 2000);
		registeComponent<ResourceManager>(3000, 3000, 3000);
	}
	// 注册时可以指定组件的初始化顺序,更新顺序,销毁顺序
	protected void registeComponent<T>(int initOrder = -1, int updateOrder = -1, int destroyOrder = -1) where T : FrameComponent
	{
		string name = typeof(T).ToString();
		T component = UnityUtility.createInstance<T>(typeof(T), name);
		component.mInitOrder = initOrder == -1 ? mFrameComponentMap.Count : initOrder;
		component.mUpdateOrder = updateOrder == -1 ? mFrameComponentMap.Count : updateOrder;
		component.mDestroyOrder = destroyOrder == -1 ? mFrameComponentMap.Count : destroyOrder;
		mFrameComponentMap.Add(name, component);
		mFrameComponentInit.Add(component);
		mFrameComponentUpdate.Add(component);
		mFrameComponentDestroy.Add(component);
	}
}