using static TestAssert;

// FrameSystem 静态排序比较方法单测(compareInit / compareUpdate / compareDestroy)
//
// 设计要点:
//   - 三个 compare 方法都是 static, 直接读取两个实例的 protected 顺序字段
//     (mInitOrder / mUpdateOrder / mDestroyOrder), 用 MathUtility.sign(差) 返回 -1/0/1。
//   - 顺序字段仅通过公有 setter(setInitOrder / setUpdateOrder / setDestroyOrder) 写入,
//     无需调用 init()(mCreateObject 默认 false, 不会创建 GameObject)。
//   - 每个用例在 finally 中 destroy() 两个局部实例, 避免残留。
public static class FrameSystemTest
{
	public static void Run()
	{
		testCompareInit();
		testCompareUpdate();
		testCompareDestroy();
		testOrderIndependentBetweenComparers();
	}

	// ─── compareInit: 按 mInitOrder 升序比较 ────────────────────────
	private static void testCompareInit()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		try
		{
			a.setInitOrder(1);
			b.setInitOrder(2);
			assertEqual(-1, FrameSystem.compareInit(a, b), "a.init<b.init 应返回-1");
			assertEqual(1, FrameSystem.compareInit(b, a), "b.init>a.init 应返回1");
			b.setInitOrder(1);
			assertEqual(0, FrameSystem.compareInit(a, b), "init 相等应返回0");
			b.setInitOrder(0);
			a.setInitOrder(10);
			assertEqual(1, FrameSystem.compareInit(a, b), "a.init>b.init 应返回1");
		}
		finally
		{
			a.destroy();
			b.destroy();
		}
	}

	// ─── compareUpdate: 按 mUpdateOrder 升序比较 ────────────────────
	private static void testCompareUpdate()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		try
		{
			a.setUpdateOrder(5);
			b.setUpdateOrder(3);
			assertEqual(-1, FrameSystem.compareUpdate(b, a), "b.update<a.update 应返回-1");
			assertEqual(1, FrameSystem.compareUpdate(a, b), "a.update>b.update 应返回1");
			b.setUpdateOrder(5);
			assertEqual(0, FrameSystem.compareUpdate(a, b), "update 相等应返回0");
		}
		finally
		{
			a.destroy();
			b.destroy();
		}
	}

	// ─── compareDestroy: 按 mDestroyOrder 升序比较 ──────────────────
	private static void testCompareDestroy()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		try
		{
			a.setDestroyOrder(8);
			b.setDestroyOrder(8);
			assertEqual(0, FrameSystem.compareDestroy(a, b), "destroy 相等应返回0");
			b.setDestroyOrder(9);
			assertEqual(-1, FrameSystem.compareDestroy(a, b), "a.destroy<b.destroy 应返回-1");
			assertEqual(1, FrameSystem.compareDestroy(b, a), "b.destroy>a.destroy 应返回1");
		}
		finally
		{
			a.destroy();
			b.destroy();
		}
	}

	// ─── 不同 comparer 各自只读自己对应的顺序字段 ──────────────────
	// 验证: compareX 只比较对应顺序字段, 不受其他顺序字段干扰。
	private static void testOrderIndependentBetweenComparers()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		try
		{
			a.setInitOrder(1);
			a.setUpdateOrder(100);
			a.setDestroyOrder(1000);
			b.setInitOrder(1);      // init 相同 → compareInit == 0
			b.setUpdateOrder(50);   // update 不同 → compareUpdate == 1(a>b)
			b.setDestroyOrder(2000);// destroy 不同 → compareDestroy == -1(a<b)
			assertEqual(0, FrameSystem.compareInit(a, b), "init 相同与无关字段无关 → 0");
			assertEqual(1, FrameSystem.compareUpdate(a, b), "update 只读 update 字段 → 1");
			assertEqual(-1, FrameSystem.compareDestroy(a, b), "destroy 只读 destroy 字段 → -1");
		}
		finally
		{
			a.destroy();
			b.destroy();
		}
	}
}
