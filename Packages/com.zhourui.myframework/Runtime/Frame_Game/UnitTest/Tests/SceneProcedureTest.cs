using static TestAssert;

// Frame_Game 精简层 SceneProcedure 生命周期测试
// 抽象类虚方法调用不炸(测试子类)
public static class SceneProcedureTest
{
	public static void Run()
	{
		testLifecycleNoThrow();
		testLifecycleOrder();
		testWillDestroy();
	}

	// 测试子类
	private class TestProcedure : SceneProcedure
	{
		public int mOrder;
		public override void init()
		{
			mOrder = 1;
			base.init();
		}
		public override void update(float elapsedTime)
		{
			mOrder = 2;
			base.update(elapsedTime);
		}
		public override void exit()
		{
			mOrder = 3;
			base.exit();
		}
		public override void willDestroy()
		{
			mOrder = 4;
			base.willDestroy();
		}
	}

	// 生命周期方法调用不炸
	static void testLifecycleNoThrow()
	{
		TestProcedure proc = new TestProcedure();
		proc.init();
		proc.update(0.1f);
		proc.exit();
		// 无异常即通过
	}

	// 调用顺序
	static void testLifecycleOrder()
	{
		TestProcedure proc = new TestProcedure();
		proc.init();
		assertEqual(1, proc.mOrder, "init 先");
		proc.update(0f);
		assertEqual(2, proc.mOrder, "update 次");
		proc.exit();
		assertEqual(3, proc.mOrder, "exit 后");
	}

	// willDestroy
	static void testWillDestroy()
	{
		TestProcedure proc = new TestProcedure();
		proc.willDestroy();
		assertEqual(4, proc.mOrder, "willDestroy 执行");
	}
}
