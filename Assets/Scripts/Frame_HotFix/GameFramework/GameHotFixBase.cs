using System;
using System.Collections.Generic;
using static FrameBase;
using static UnityUtility;
using static FrameUtility;

public abstract class GameHotFixBase
{
	protected static GameHotFixBase mInstance;						// 在子类中创建
	protected List<FrameSystem> mFrameComponentInit = new();		// 存储框架组件,用于初始化
	public void start(Action callback)
	{
		GameFramework.mOnPackageName += getAndroidPluginBundleName;
		GameFramework.startHotFix(() => 
		{
			// 创建系统组件
			initFrameSystem();
			mGameFramework.sortList();
			mFrameComponentInit.Sort(FrameSystem.compareInit);

			// 注册对象类型
			registerAll();

			DateTime startTime = DateTime.Now;
			mExcelManager.loadAllAsync(() =>
			{
				log("打开所有表格耗时:" + (int)(DateTime.Now - startTime).TotalMilliseconds + "毫秒");
				mExcelManager.checkAll();
				onPreInit();
				// 初始化所有系统组件
				foreach (FrameSystem frame in mFrameComponentInit)
				{
					try
					{
						DateTime start = DateTime.Now;
						frame.init();
						log(frame.getName() + "初始化消耗时间:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒");
					}
					catch (Exception e)
					{
						logError("init failed! :" + frame.getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
					}
				}
				foreach (FrameSystem frame in mFrameComponentInit)
				{
					try
					{
						DateTime start = DateTime.Now;
						frame.lateInit();
						log(frame.getName() + " late初始化消耗时间:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒");
					}
					catch (Exception e)
					{
						logError("late init failed! :" + frame.getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
					}
				}
				onPostInit();
				log("启动游戏耗时:" + (int)(DateTime.Now - mGameFramework.getStartTime()).TotalMilliseconds + "毫秒");
				callback?.Invoke();
				// 进入主场景
				enterScene(getStartGameSceneType());
			});
		});
	}
	//----------------------------------------------------------------------------------------------------------------------------------
	protected abstract string getAndroidPluginBundleName();
	protected abstract void registerAll();
	protected abstract void initFrameSystem();
	protected virtual void onPreInit() { }
	protected virtual void onPostInit() { }
	protected abstract Type getStartGameSceneType();
	protected void registeFrameSystem<T>(Action<T> callback) where T : FrameSystem, new()
	{
		mFrameComponentInit.Add(mGameFramework.registeFrameSystem(callback));
	}
	// 需要在子类中添加如下函数,来创建热更对象的实例
	//public static GameHotFixBase createHotFixInstance()
	//{
	//	mInstance = createInstance<GameHotFixBase>(MethodBase.GetCurrentMethod().DeclaringType);
	//	return mInstance;
	//}
}