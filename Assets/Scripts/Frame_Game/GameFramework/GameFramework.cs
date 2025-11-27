using System;
using System.Collections.Generic;
using static FrameBase;
using static FrameBaseUtility;

// 本来想要实现为在非热更代码中只下载最新的热更dll,启动热更以后再去下载更新资源,这样做到使尽可能多的代码都可以热更
// 但是由于启动热更时需要初始化各个系统,比如KeyFrameManager,AudioManager等,这些系统的初始化中会依赖于资源,也就是会去异步加载一些需要用到的资源
// 而且热更过程中需要显示的界面也是需要加载资源,这就导致逻辑有冲突.就没有办法在资源已经更新完成时去做这些操作.
// 所以不得不实现为非热更代码中去更新整个游戏的资源,只有当资源更新完成后再启动热更.
// 虽然Frame_Game与Frame_HotFix很类似,都是框架层.不过为了尽量减少代码的重复,Frame_Game中去除了非常多的特性,以尽量减少代码量,尽量做到代码的精简

// 非热更部分的最顶层的节点,管理所有框架组件(管理器)
public class GameFramework : IFramework
{
	protected List<FrameSystem> mFrameComponentList = new(128);							// 存储框架组件,用于查找
	protected Dictionary<string, Action<FrameSystem>> mFrameCallbackList = new(128);	// 用于通知框架系统创建或者销毁的回调
	public static Action mOnInitFrameSystem;                                            // 用于通知注册所有的应用层框架组件
	public static Action mOnRegisteStuff;												// 用于通知注册应用层对象
	public static Action mOnDestroy;                                                    // 用于通知应用层销毁
	public static Func<string> mOnPackageName;											// 用于获取安卓包名
	public virtual void init()
	{
		registeFrameSystem<AndroidPluginManager>(null);
		registeFrameSystem<AndroidAssetLoader>(null);
		registeFrameSystem<AndroidMainClass>(null);
		AndroidPluginManager.initAnroidPlugin(mOnPackageName?.Invoke());
		AndroidAssetLoader.initJava(AndroidPluginManager.getPackageName() + ".AssetLoader");
		AndroidMainClass.initJava(AndroidPluginManager.getPackageName() + ".MainClass");
		logBase("start game!");
		try
		{
			DateTime startTime = DateTime.Now;
			initFrameSystem();
			AndroidMainClass.gameStart();
			logBase("start消耗时间:" + (int)(DateTime.Now - startTime).TotalMilliseconds);
			mOnRegisteStuff?.Invoke();
			foreach (FrameSystem frame in mFrameComponentList)
			{
				try
				{
					DateTime start = DateTime.Now;
					frame.init();
					if (isDevOrEditor())
					{
						logBase(frame.getName() + "初始化消耗时间:" + (int)(DateTime.Now - start).TotalMilliseconds);
					}
				}
				catch (Exception e)
				{
					logExceptionBase(e, "init failed! :" + frame.getName());
				}
			}
		}
		catch (Exception e)
		{
			logExceptionBase(e, "init failed! " + (e.InnerException?.Message ?? "empty"));
		}

		// 打印一下当前的版本类型
		if (!isEditor())
		{
			if (isNoHotFixTestClient())
			{
				logBase("无热更测试版");
			}
			else if (isHotFixTestClient())
			{
				logBase("热更测试版");
			}
			else
			{
				logBase("正式版");
			}
			initSDK();
		}
	}
	public void update(float elapsedTime)
	{
		if (mFrameComponentList == null)
		{
			return;
		}
		int count = mFrameComponentList.Count;
		for (int i = 0; i < count; ++i)
		{
			// 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
			if (mFrameComponentList == null)
			{
				return;
			}
			mFrameComponentList[i]?.update(elapsedTime);
		}
	}
	public void fixedUpdate(float elapsedTime){}
	public void lateUpdate(float elapsedTime){}
	public void drawGizmos(){}
	public void onApplicationFocus(bool focus){}
	public void onApplicationQuit()
	{
		destroy();
	}
	public void destroy()
	{
		logBase("destroy GameFramework NoHotFix");
		if (mFrameComponentList == null)
		{
			return;
		}
		mOnDestroy?.Invoke();
		foreach (FrameSystem frame in mFrameComponentList)
		{
			frame?.willDestroy();
		}
		foreach (FrameSystem frame in mFrameComponentList)
		{
			if (frame != null)
			{
				frame.destroy();
				mFrameCallbackList.Remove(frame.getName(), out var callback);
				callback?.Invoke(null);
			}
		}
		mFrameComponentList.Clear();
		mFrameComponentList = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void initSDK(){}
	protected void initFrameSystem()
	{
		registeFrameSystem<GameSceneManager>((com) =>		{ mGameSceneManager = com; });
		registeFrameSystem<LayoutManager>((com) =>			{ mLayoutManager = com; });
		registeFrameSystem<ResourceManager>((com) =>		{ mResourceManager = com; });
		registeFrameSystem<AssetVersionSystem>((com) =>		{ mAssetVersionSystem = com; });
		mOnInitFrameSystem?.Invoke();
	}
	protected T registeFrameSystem<T>(Action<T> callback) where T : FrameSystem, new()
	{
		if (isDevOrEditor())
		{
			logBase("注册系统:" + typeof(T) + ", owner:" + GetType());
		}
		T com = new();
		string name = typeof(T).ToString();
		mFrameComponentList.Add(com);
		mFrameCallbackList.Add(name, (com) => { callback?.Invoke(com as T); });
		callback?.Invoke(com);
		return com;
	}
}