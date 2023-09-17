using System;
using System.Collections.Generic;
using static FrameBase;
using static GBR;
using static FrameUtilityILR;
using static UnityUtility;

public class GameILR
{
	protected static List<FrameSystem> mFrameComponentInit = new List<FrameSystem>();		// 存储框架组件,用于初始化
	public static void start()
	{
		// 创建系统组件
		initFrameSystem();
		mGameFramework.sortList();
		mFrameComponentInit.Sort(FrameSystem.compareInit);

		// 获取系统组件对象
		constructILRDone();

		// 注册对象类型
		registeAll();

		// 初始化所有系统组件
		int count = mFrameComponentInit.Count;
		for (int i = 0; i < count; ++i)
		{
			try
			{
				DateTime start = DateTime.Now;
				mFrameComponentInit[i].init();
				log(mFrameComponentInit[i].getName() + "初始化消耗时间:" + (DateTime.Now - start).TotalMilliseconds, LOG_LEVEL.FORCE);
			}
			catch (Exception e)
			{
				logError("init failed! :" + mFrameComponentInit[i].getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
			}
		}
		for (int i = 0; i < count; ++i)
		{
			try
			{
				DateTime start = DateTime.Now;
				mFrameComponentInit[i].lateInit();
				log(mFrameComponentInit[i].getName() + "late初始化消耗时间:" + (DateTime.Now - start).TotalMilliseconds, LOG_LEVEL.FORCE);
			}
			catch (Exception e)
			{
				logError("late init failed! :" + mFrameComponentInit[i].getName() + ", info:" + e.Message + ", stack:" + e.StackTrace);
			}
		}

		// 进入登录流程
		enterSceneILR<MainScene>();
	}
	//----------------------------------------------------------------------------------------------------------------------------------
	protected static void registeAll()
	{
		LayoutRegisterILR.registeAll();
		SQLiteRegisterILR.registeAll();
	}
	protected static void initFrameSystem()
	{
		registeILRFrameSystem<DemoSystem>();
	}
	protected static void registeILRFrameSystem<T>() where T : FrameSystem
	{
		T frameSystem = mGameFramework.registeFrameSystem(typeof(T)) as T;
		mFrameComponentInit.Add(frameSystem);
	}
}