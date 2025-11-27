using UnityEngine;
using Obfuz;
using System;
using System.Collections.Generic;
using static FrameBaseUtility;
using static UnityUtility;
using static FrameUtility;
using static MathUtility;
using static TimeUtility;
using static FrameBaseHotFix;

// 最顶层的节点,也是游戏的入口,管理所有框架组件(管理器)
[ObfuzIgnore]
public class GameFrameworkHotFix : IFramework
{
	protected Dictionary<string, FrameSystem> mFrameComponentMap = new(128);		// 存储框架组件,用于查找
	protected Dictionary<string, Action<FrameSystem>> mFrameCallbackList = new(128);// 用于通知框架系统创建或者销毁的回调
	protected List<FrameSystem> mFrameComponentInit = new(128);						// 存储框架组件,用于初始化
	protected List<FrameSystem> mFrameComponentUpdate = new(128);					// 存储框架组件,用于更新
	protected List<FrameSystem> mFrameComponentDestroy = new(128);					// 存储框架组件,用于销毁
	protected ThreadTimeLock mTimeLock = new(15);									// 用于主线程锁帧,与Application.targetFrameRate功能类似
	protected DateTime mStartTime;													// 启动游戏时的时间
	protected DateTime mCurTime;													// 记录当前时间
	protected Action mOnApplicationQuitCallBack;									// 程序退出的回调
	protected BoolCallback mOnApplicationFocusCallBack;								// 程序切换到前台或者切换到后台的回调
	protected float mThisFrameTime;													// 当前这一帧的消耗时间
	protected long mFrameIndex;														// 当前帧下标
	protected int mCurFrameCount;													// 当前已执行的帧数量
	protected int mFrameRate;														// 当前设置的最大帧率
	protected int mFPS;																// 当前帧率
	protected bool mResourceAvailable;												// 资源是否已经可用
	protected bool mIsDestroy;														// 框架是否已经被销毁
	public static Action mOnDestroy;
	public static Action<int, long, long, long, long> mOnMemoryModifiedCheck;
	public static Func<string> mOnPackageName;
	public static void startHotFix(Action callback)
	{
		GameFrameworkHotFix framework = new();
		GameEntry.getInstance().setFrameworkHotFix(framework);
		framework.init(callback);
	}
	public DateTime getStartTime() { return mStartTime; }
	public DateTime getFrameStartTime() { return mTimeLock.getFrameStartTime(); }
	public long getFrameIndex() { return mFrameIndex; }
	public void update(float elapsedTime)
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
		elapsedTime = mThisFrameTime;
		setThisTimeMS(getNowTimeStampMS());
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
	public void fixedUpdate(float elapsedTime)
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
	public void lateUpdate(float elapsedTime)
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
	public void drawGizmos()
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
	public void onApplicationFocus(bool focus)
	{
		mOnApplicationFocusCallBack?.Invoke(focus);
	}
	public void onApplicationQuit()
	{
		mOnApplicationQuitCallBack?.Invoke();
		destroy();
	}
	public void registeOnApplicationQuit(Action action) { mOnApplicationQuitCallBack += action; }
	public void unregisteOnApplicationQuit(Action action) { mOnApplicationQuitCallBack -= action; }
	public void registeOnApplicationFocus(BoolCallback action) { mOnApplicationFocusCallBack += action; }
	public void unregisteOnApplicationFocus(BoolCallback action) { mOnApplicationFocusCallBack -= action; }
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
		if (rate <= 0)
		{
			logError("帧率不能小于等于0");
			return;
		}
		mFrameRate = rate;
		Application.targetFrameRate = mFrameRate;
	}
	public void resetFrameRate()
	{
		setFrameRate(GameEntry.getInstance().mFramworkParam.mDefaultFrameRate);
	}
	public bool isResourceAvailable() { return mResourceAvailable; }
	public bool isDestroy() { return mIsDestroy; }
	public void destroy()
	{
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
	}
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
		mFrameCallbackList.Remove(name, out var callback);
		callback?.Invoke(null);
	}
	public T registeFrameSystem<T>(Action<T> callback, int initOrder = -1, int updateOrder = -1, int destroyOrder = -1) where T : FrameSystem, new()
	{
		Type type = typeof(T);
		if (isDevOrEditor())
		{
			log("注册系统:" + type.ToString() + ", owner:" + GetType());
		}
		T com = new();
		string name = type.Assembly.FullName.rangeToFirst(',') + "_" + type.ToString();
		com.setName(name);
		com.setInitOrder(initOrder == -1 ? mFrameComponentMap.Count : initOrder);
		com.setUpdateOrder(updateOrder == -1 ? mFrameComponentMap.Count : updateOrder);
		com.setDestroyOrder(destroyOrder == -1 ? mFrameComponentMap.Count : destroyOrder);
		mFrameComponentMap.Add(name, com);
		mFrameComponentInit.Add(com);
		mFrameComponentUpdate.Add(com);
		mFrameComponentDestroy.Add(com);
		mFrameCallbackList.Add(name, (com) => { callback?.Invoke(com as T); });
		callback?.Invoke(com);
		return com;
	}
	public void sortList()
	{
		mFrameComponentInit.Sort(FrameSystem.compareInit);
		mFrameComponentUpdate.Sort(FrameSystem.compareUpdate);
		mFrameComponentDestroy.Sort(FrameSystem.compareDestroy);
	}
	public void onMemoryModified(int flag, long param0, long param1, long param2, long param3) 
	{
		mOnMemoryModifiedCheck?.Invoke(flag, param0, param1, param2, param3);
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
	protected void init(Action callback)
	{
		using var a = new ProfilerScope(0);
		// 通过代码添加接受java日志的节点
		getOrAddComponent<UnityAndroidLog>(findOrCreateRootGameObject("UnityLog"));
		mGameFrameworkHotFix = this;
		mIsDestroy = false;
		mStartTime = DateTime.Now;
		setFrameRate(GameEntry.getInstance().mFramworkParam.mDefaultFrameRate);

		if (!isEditor() && isDevelopment())
		{
			// 跟引擎自带的dev的调试控制台功能重合了,所以不再使用
			//getOrAddComponent<ConsoleToScreen>(GameEntry.getInstanceObject());
		}

		// 设置默认的日志等级
		setLogLevel(LOG_LEVEL.FORCE);

		registeFrameSystem<AndroidPluginManager>(null);
		registeFrameSystem<AndroidAssetLoader>(null);
		registeFrameSystem<AndroidMainClass>(null);
		AndroidPluginManager.initAnroidPlugin(mOnPackageName?.Invoke());
		AndroidAssetLoader.initJava(AndroidPluginManager.getPackageName() + ".AssetLoader");
		AndroidMainClass.initJava(AndroidPluginManager.getPackageName() + ".MainClass");
		log("start game hotfix!");
		log("当前平台:" + getPlatformName());
		try
		{
			DateTime startTime = DateTime.Now;
			initFrameSystem();
			recoverCrossParam();
			log("start消耗时间:" + (int)(DateTime.Now - startTime).TotalMilliseconds);
			// 根据设置的顺序对列表进行排序
			sortList();
			foreach (FrameSystem frame in mFrameComponentInit)
			{
				try
				{
					DateTime start = DateTime.Now;
					frame.init();
					if (isDevOrEditor())
					{
						log(frame.getName() + "初始化消耗时间:" + (int)(DateTime.Now - start).TotalMilliseconds);
					}
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
		}
		catch (Exception e)
		{
			logException(e, "init failed! " + (e.InnerException?.Message ?? "empty"));
		}
		mCurTime = DateTime.Now;

		// 先执行所有的preInitAsync
		preInitAsync(() =>
		{
			// 再执行所有的initAsync,因为部分initAsync会依赖于preInitAsync
			int initedCount = 0;
			foreach (FrameSystem item in mFrameComponentInit)
			{
				item.initAsync(() =>
				{
					if (++initedCount == mFrameComponentInit.count())
					{
						resourceAvailable();
						try
						{
							callback?.Invoke();
						}
						catch(Exception e)
						{
							logException(e);
						}
					}
				});
			}
		});
	}
	protected void initFrameSystem()
	{
		registeFrameSystem<ResourceManager>((com) =>		{ mResourceManager = com; },  -1, 3000, 3000);		// 资源管理器的需要最先初始化,并且是最后被销毁,作为最后的资源清理
		registeFrameSystem<TimeManager>((com) =>			{ mTimeManager = com; });
		registeFrameSystem<GlobalCmdReceiver>((com) =>		{ mGlobalCmdReceiver = com; });
		registeFrameSystem<SQLiteManager>((com) =>			{ mSQLiteManager = com; });
		registeFrameSystem<CommandSystem>((com) =>			{ mCommandSystem = com; }, -1, -1, 2001);			// 命令系统在大部分管理器都销毁完毕后再销毁
		registeFrameSystem<InputSystem>((com) =>			{ mInputSystem = com; });                           // 输入系统应该早点更新,需要更新输入的状态,以便后续的系统组件中使用
		registeFrameSystem<KeyMappingSystem>((com) =>		{ mKeyMappingSystem = com; });                      // 输入映射系统需要在输入系统之后
		registeFrameSystem<GlobalTouchSystem>((com) =>		{ mGlobalTouchSystem = com; });
		registeFrameSystem<TweenerManager>((com) =>			{ mTweenerManager = com; });
		registeFrameSystem<CharacterManager>((com) =>		{ mCharacterManager = com; });
		registeFrameSystem<AudioManager>((com) =>			{ mAudioManager = com; });
		registeFrameSystem<GameSceneManager>((com) =>		{ mGameSceneManager = com; }, -1, -1, 0);			// 在退出程序时,需要先执行流程的退出,然后才能执行其他系统的销毁
		registeFrameSystem<KeyFrameManager>((com) =>		{ mKeyFrameManager = com; });
		registeFrameSystem<DllImportSystem>((com) =>		{ mDllImportSystem = com; });
		registeFrameSystem<ShaderManager>((com) =>			{ mShaderManager = com; });
		registeFrameSystem<CameraManager>((com) =>			{ mCameraManager = com; });
		registeFrameSystem<SceneSystem>((com) =>			{ mSceneSystem = com; });
		registeFrameSystem<GamePluginManager>((com) =>		{ mGamePluginManager = com; });
		registeFrameSystem<ClassPool>((com) =>				{ mClassPool = com; }, -1, -1, 3101);
		registeFrameSystem<ClassPoolThread>((com) =>		{ mClassPoolThread = com; }, -1, -1, 3102);
		registeFrameSystem<ListPool>((com) =>				{ mListPool = com; }, -1, -1, 3103);
		registeFrameSystem<ListPoolThread>((com) =>			{ mListPoolThread = com; }, -1, -1, 3104);
		registeFrameSystem<HashSetPool>((com) =>			{ mHashSetPool = com; }, -1, -1, 3104);
		registeFrameSystem<HashSetPoolThread>((com) =>		{ mHashSetPoolThread = com; }, -1, -1, 3105);
		registeFrameSystem<DictionaryPool>((com) =>			{ mDictionaryPool = com; }, -1, -1, 3106);
		registeFrameSystem<DictionaryPoolThread>((com) =>	{ mDictionaryPoolThread = com; }, -1, -1, 3107);
		registeFrameSystem<ArrayPool>((com) =>				{ mArrayPool = com; }, -1, -1, 3108);
		registeFrameSystem<ArrayPoolThread>((com) =>		{ mArrayPoolThread = com; }, -1, -1, 3109);
		registeFrameSystem<ByteArrayPool>((com) =>			{ mByteArrayPool = com; }, -1, -1, 3110);
		registeFrameSystem<ByteArrayPoolThread>((com) =>	{ mByteArrayPoolThread = com; }, -1, -1, 3111);
		registeFrameSystem<MovableObjectManager>((com) =>	{ mMovableObjectManager = com; });
		registeFrameSystem<EffectManager>((com) =>			{ mEffectManager = com; });
		registeFrameSystem<AtlasManager>((com) =>		{ mAtlasManager = com; });
		registeFrameSystem<NetPacketFactory>((com) =>		{ mNetPacketFactory = com; });
		registeFrameSystem<PathKeyframeManager>((com) =>	{ mPathKeyframeManager = com; });
		registeFrameSystem<EventSystem>((com) =>			{ mEventSystem = com; });
		registeFrameSystem<StateManager>((com) =>			{ mStateManager = com; });
		registeFrameSystem<NetPacketTypeManager>((com) =>	{ mNetPacketTypeManager = com; });
		registeFrameSystem<GameObjectPool>((com) =>			{ mGameObjectPool = com; });
		registeFrameSystem<ExcelManager>((com) =>			{ mExcelManager = com; });
		registeFrameSystem<RedPointSystem>((com) =>			{ mRedPointSystem = com; });
		registeFrameSystem<AssetVersionSystem>((com) =>		{ mAssetVersionSystem = com; });
		registeFrameSystem<GlobalKeyProcess>((com) =>		{ mGlobalKeyProcess = com; });
		registeFrameSystem<LocalizationManager>((com) =>	{ mLocalizationManager = com; });
		registeFrameSystem<AsyncTaskGroupManager>((com) =>	{ mAsyncTaskGroupManager = com; });
		registeFrameSystem<GoogleLogin>((com) =>			{ mGoogleLogin = com; });
		registeFrameSystem<AppleLogin>((com) =>				{ mAppleLogin = com; });
		registeFrameSystem<ScreenOrientationSystem>((com) =>{ mScreenOrientationSystem = com; });
		registeFrameSystem<WaitingManager>((com) =>			{ mWaitingManager = com; });
		registeFrameSystem<UndoManager>((com) =>			{ mUndoManager = com; });
		registeFrameSystem<AndroidPurchasing>((com) =>		{ mAndroidPurchasing = com; });
		registeFrameSystem<PurchasingSystem>((com) =>		{ mPurchasingSystem = com; });
		registeFrameSystem<AvatarRenderer>((com) =>			{ mAvatarRenderer = com; });
		registeFrameSystem<LayoutManager>((com) =>			{ mLayoutManager = com; }, 1000, 1000, -1);			// 布局管理器也需要在最后更新,确保所有游戏逻辑都更新完毕后,再更新界面
		registeFrameSystem<PrefabPoolManager>((com) =>		{ mPrefabPoolManager = com; }, 2000, 2000, 2000);	// 物体管理器最后注册,销毁所有缓存的资源对象
	}
}