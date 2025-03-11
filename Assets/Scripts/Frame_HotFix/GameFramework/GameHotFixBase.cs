using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static UnityUtility;
using static FrameUtility;
using static FrameEditorUtility;

public abstract class GameHotFixBase
{
	protected static GameHotFixBase mInstance;						// 在子类中创建
	protected List<FrameSystem> mFrameComponentInit = new();        // 存储框架组件,用于初始化
	protected Action mFinishCallback;								// 存储的启动热更完成的回调
	protected bool mCanCallback = true;                             // 是否允许在初始化以后自动调用start传递的callback参数,如果不自动调用,就需要手动调用
	public void start(Action callback)
	{
		mFinishCallback = callback;
		GameFrameworkHotFix.mOnPackageName += getAndroidPluginBundleName;
		GameFrameworkHotFix.startHotFix(() => 
		{
			mGameSetting.getComponent(out mCOMGameSettingAudio);

			// 创建系统组件
			initFrameSystem();
			mGameFrameworkHotFix.sortList();
			mFrameComponentInit.Sort(FrameSystem.compareInit);

			// 注册对象类型
			registerAll();

			DateTime startTime = DateTime.Now;
			mExcelManager.loadAllAsync(() =>
			{
				log("打开所有表格耗时:" + (int)(DateTime.Now - startTime).TotalMilliseconds + "毫秒");
				if (isEditor())
				{
					mExcelManager.checkAll();
				}
				
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
				log("启动游戏耗时:" + (int)(DateTime.Now - mGameFrameworkHotFix.getStartTime()).TotalMilliseconds + "毫秒");
				if (mCanCallback)
				{
					mFinishCallback?.Invoke();
				}
				// 进入主场景
				enterScene(getStartGameSceneType());
			});
		});
	}
	public static void callback() { mInstance.mFinishCallback?.Invoke(); }
	//----------------------------------------------------------------------------------------------------------------------------------
	protected abstract string getAndroidPluginBundleName();
	protected abstract void registerAll();
	protected abstract void initFrameSystem();
	protected virtual void onPreInit() { }
	protected virtual void onPostInit() { }
	protected abstract Type getStartGameSceneType();
	protected void registeFrameSystem<T>(Action<T> callback) where T : FrameSystem, new()
	{
		mFrameComponentInit.Add(mGameFrameworkHotFix.registeFrameSystem(callback));
	}
	// 需要在子类中添加如下函数,来创建热更对象的实例
	//public static GameHotFixBase createHotFixInstance()
	//{
	//	mInstance = createInstance<GameHotFixBase>(MethodBase.GetCurrentMethod().DeclaringType);
	//	return mInstance;
	//}
}