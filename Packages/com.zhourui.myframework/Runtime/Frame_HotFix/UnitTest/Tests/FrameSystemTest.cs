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
		testPreInitAsyncCallsCallback();
		testPreInitAsyncNullCallbackSafe();
		testInitAsyncCallsCallback();
		testInitAsyncNullCallbackSafe();
		testGetObjectDefaultNull();
		testCompareInitTie();
		testSetOrdersAfterCompare();
		testEmptyVirtualsSafe();
		testSetCreateObjectNoThrow();
		testDestroyThenEmptyVirtualsSafe();
		testSetOrdersIndependentOfDestroy();
		testDestroyTwiceSafe();
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

	// ─── preInitAsync: 非 null callback 会被调用 ──────────────────
	//     源码: public virtual void preInitAsync(Action callback) { callback?.Invoke(); }
	//     纯逻辑, 不依赖全局单例/资源。
	private static void testPreInitAsyncCallsCallback()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			bool called = false;
			sys.preInitAsync(() => called = true);
			assertTrue(called, "preInitAsync 应调用非 null callback");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── preInitAsync: null callback 不崩溃 ───────────────────────
	private static void testPreInitAsyncNullCallbackSafe()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			sys.preInitAsync(null);   // ?.Invoke() 空安全, 无异常即通过
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── initAsync: 非 null callback 会被调用 ────────────────────
	private static void testInitAsyncCallsCallback()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			bool called = false;
			sys.initAsync(() => called = true);
			assertTrue(called, "initAsync 应调用非 null callback");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── initAsync: null callback 不崩溃 ──────────────────────────
	private static void testInitAsyncNullCallbackSafe()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			sys.initAsync(null);   // ?.Invoke() 空安全, 无异常即通过
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getObject: 未 init 时(默认 mCreateObject=false)返回 null ─
	//     getObject() 是只读 getter 返回 mObject; 未调用 init() 时 mObject=null。
	private static void testGetObjectDefaultNull()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			assertNull(sys.getGameObject(), "未 init 时 getObject 应返回 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合场景
	// ═════════════════════════════════════════════════════════════════

	// 空虚方法调用安全(lateInit/willDestroy/resourceAvailable/onDrawGizmos)
	private static void testEmptyVirtualsSafe()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			sys.lateInit();
			sys.willDestroy();
			sys.resourceAvailable();
			sys.onDrawGizmos();
			// 无异常即通过
		}
		finally
		{
			sys.destroy();
		}
	}

	// setCreateObject 切换不炸
	private static void testSetCreateObjectNoThrow()
	{
		FrameSystem sys = new FrameSystem();
		try
		{
			sys.setCreateObject(true);
			sys.setCreateObject(false);
			// 无异常即通过
		}
		finally
		{
			sys.destroy();
		}
	}

	// destroy 后空虚方法仍可调用
	private static void testDestroyThenEmptyVirtualsSafe()
	{
		FrameSystem sys = new FrameSystem();
		sys.destroy();
		sys.lateInit();
		sys.willDestroy();
		sys.resourceAvailable();
		// 无异常即通过
	}

	// 顺序 setter 与 destroy 互不影响
	private static void testSetOrdersIndependentOfDestroy()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		a.setInitOrder(5);
		a.destroy();
		b.setInitOrder(5);
		assertEqual(0, FrameSystem.compareInit(a, b), "destroy 后比较仍正常");
		a.setInitOrder(9);
		assertTrue(FrameSystem.compareInit(a, b) > 0, "destroy 后 set 仍生效");
	}

	// destroy 幂等
	private static void testDestroyTwiceSafe()
	{
		FrameSystem sys = new FrameSystem();
		sys.destroy();
		sys.destroy();
		// 无异常即通过
	}

	// compareInit 同序返回 0
	private static void testCompareInitTie()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		try
		{
			a.setInitOrder(7);
			b.setInitOrder(7);
			assertEqual(0, FrameSystem.compareInit(a, b), "同 initOrder 返回 0");
		}
		finally
		{
			a.destroy();
			b.destroy();
		}
	}

	// 组合: 设置顺序后比较结果变化
	private static void testSetOrdersAfterCompare()
	{
		FrameSystem a = new FrameSystem();
		FrameSystem b = new FrameSystem();
		try
		{
			a.setInitOrder(30);
			b.setInitOrder(10);
			assertTrue(FrameSystem.compareInit(a, b) > 0, "大 order 排后");
			a.setInitOrder(5);
			assertTrue(FrameSystem.compareInit(a, b) < 0, "改小后排前");
			a.setDestroyOrder(2);
			b.setDestroyOrder(4);
			assertTrue(FrameSystem.compareDestroy(a, b) < 0, "destroyOrder 比较生效");
		}
		finally
		{
			a.destroy();
			b.destroy();
		}
	}
}
