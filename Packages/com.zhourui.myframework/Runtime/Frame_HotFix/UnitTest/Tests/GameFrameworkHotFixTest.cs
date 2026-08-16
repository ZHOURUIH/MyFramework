using static TestAssert;
using static FrameBaseHotFix;

// GameFrameworkHotFix 纯 getter / setter / 事件注册测试
//   框架环境已完全初始化, 使用全局 mGameFrameworkHotFix 单例
//   ⚠️ 不测 update/fixedUpdate/lateUpdate(依赖内部组件列表状态)
//   ⚠️ 绝不调 onApplicationQuit/onApplicationFocus——onApplicationQuit 内部会 destroy() 销毁整个框架!
//   ⚠️ 事件注册测试必须 finally 注销, 不留全局状态
public static class GameFrameworkHotFixTest
{
	public static void Run()
	{
		testGetStartTimeValid();
		testGetFrameIndexNonNegative();
		testGetUnscaledTimeNonNegative();
		testSetAllInitedToggle();
		testRegisteUnregisteQuit();
		testRegisteUnregisteFocus();
		testGetFrameStartTimeValid();
		testGetFPSSafe();
		testIsDestroySafe();
		testIsResourceAvailableSafe();
		testResetFrameRateSafe();
	}

	// getStartTime 有效(非默认时间)
	private static void testGetStartTimeValid()
	{
		assertTrue(mGameFrameworkHotFix.getStartTime() > System.DateTime.MinValue, "getStartTime 已设置");
	}

	// getFrameIndex 非负
	private static void testGetFrameIndexNonNegative()
	{
		assertTrue(mGameFrameworkHotFix.getFrameIndex() >= 0, "getFrameIndex >= 0");
	}

	// getUnscaledTime 非负
	private static void testGetUnscaledTimeNonNegative()
	{
		assertTrue(mGameFrameworkHotFix.getUnscaledTime() >= 0.0f, "getUnscaledTime >= 0");
	}

	// setAllInited 切换
	private static void testSetAllInitedToggle()
	{
		bool original = mGameFrameworkHotFix.isAllInited();
		try
		{
			mGameFrameworkHotFix.setAllInited(true);
			assertTrue(mGameFrameworkHotFix.isAllInited(), "set true 后 isAllInited true");
			mGameFrameworkHotFix.setAllInited(false);
			assertFalse(mGameFrameworkHotFix.isAllInited(), "set false 后 isAllInited false");
		}
		finally
		{
			mGameFrameworkHotFix.setAllInited(original);
		}
	}

	// onApplicationQuit 注册/注销(不触发——onApplicationQuit 内部会 destroy 框架!)
	private static void testRegisteUnregisteQuit()
	{
		bool called = false;
		System.Action action = () => { called = true; };
		mGameFrameworkHotFix.registeOnApplicationQuit(action);
		mGameFrameworkHotFix.unregisteOnApplicationQuit(action);
		// 只验证注册/注销不炸(触发会销毁框架, 绝不调用)
	}

	// onApplicationFocus 注册/注销(同一 delegate 实例, 不触发——onApplicationFocus 内部可能依赖全局)
	private static void testRegisteUnregisteFocus()
	{
		bool called = false;
		BoolCallback cb = (focus) => { called = true; };
		mGameFrameworkHotFix.registeOnApplicationFocus(cb);
		mGameFrameworkHotFix.unregisteOnApplicationFocus(cb);
		// 只验证注册/注销不炸
	}

	// getFrameStartTime 有效
	private static void testGetFrameStartTimeValid()
	{
		assertTrue(mGameFrameworkHotFix.getFrameStartTime() > System.DateTime.MinValue, "getFrameStartTime 已设置");
	}

	// getFPS 非负
	private static void testGetFPSSafe()
	{
		assertTrue(mGameFrameworkHotFix.getFPS() >= 0, "getFPS >= 0");
	}

	// isDestroy 读值安全
	private static void testIsDestroySafe()
	{
		// 测试环境框架未销毁
		assertFalse(mGameFrameworkHotFix.isDestroy(), "测试环境框架未销毁");
	}

	// isResourceAvailable 读值安全
	private static void testIsResourceAvailableSafe()
	{
		mGameFrameworkHotFix.isResourceAvailable();
		// 无异常即通过(不修改状态)
	}

	// resetFrameRate 恢复默认帧率(不改坏状态)
	private static void testResetFrameRateSafe()
	{
		mGameFrameworkHotFix.resetFrameRate();
		// 无异常即通过
	}
}
