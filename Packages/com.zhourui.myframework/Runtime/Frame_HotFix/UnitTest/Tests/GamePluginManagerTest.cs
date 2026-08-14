using static TestAssert;

// GamePluginManager 空插件列表行为测试
//   init/update/destroy 遍历 mPluginList(空字典时安全)
//   注意: init() 在 Windows 下会 loadAllPlugin(加载真实插件, 测试环境不调用)
//   只测 update/destroy 空安全与幂等
public static class GamePluginManagerTest
{
	public static void Run()
	{
		testEmptyUpdateSafe();
		testEmptyDestroySafe();
		testDestroyTwiceSafe();
		testUpdateAfterDestroySafe();
		testUpdateNegativeTimeSafe();
		testDestroyThenInitNotCalled();
	}

	// 空插件列表 update 不炸
	private static void testEmptyUpdateSafe()
	{
		GamePluginManager mgr = new GamePluginManager();
		mgr.update(0.016f);
		mgr.update(1.0f);
		// 无异常即通过
	}

	// 空插件列表 destroy 不炸且清空
	private static void testEmptyDestroySafe()
	{
		GamePluginManager mgr = new GamePluginManager();
		mgr.destroy();
		// 无异常即通过
	}

	// destroy 幂等
	private static void testDestroyTwiceSafe()
	{
		GamePluginManager mgr = new GamePluginManager();
		mgr.destroy();
		mgr.destroy();
		// 无异常即通过
	}

	// destroy 后 update 安全(列表已清空)
	private static void testUpdateAfterDestroySafe()
	{
		GamePluginManager mgr = new GamePluginManager();
		mgr.destroy();
		mgr.update(0.5f);
		// 无异常即通过
	}

	// 负时间 update 不炸
	private static void testUpdateNegativeTimeSafe()
	{
		GamePluginManager mgr = new GamePluginManager();
		mgr.update(-1.0f);
		// 无异常即通过
	}

	// destroy 后不调 init(避免 Windows 下 loadAllPlugin 副作用)
	private static void testDestroyThenInitNotCalled()
	{
		GamePluginManager mgr = new GamePluginManager();
		mgr.destroy();
		// 已 destroy 的对象不调 init; 只验证 destroy 后对象仍可安全丢弃
		// 无异常即通过
	}
}
