using System.Collections.Generic;
using static TestAssert;

// KeyFrameManager 关键帧曲线管理器
// 覆盖: KeyFrameManager.loadAllCalculatedCurve(静态, 向字典填充全部内置公式曲线)
//      + 实例方法 getKeyFrame/isLoadDone/destroy(局部 new 安全, 见方法注释)
public static class KeyFrameManagerTest
{
	public static void Run()
	{
		testLoadAllCalculatedCurve();
		testGetKeyFrame_Zero_ReturnsNull();
		testGetKeyFrame_BuiltinCurve_ReturnsInstance();
		testGetKeyFrame_UnknownID_ReturnsNull();
		testIsLoadDone_DefaultFalse();
		testDestroy_ClearsList();
		testGetKeyFrameMultipleBuiltin();
		testLoadAllAfterDestroy();
		testGetKeyFrameAllBuiltin();
		testBuiltinCurveEndpointsFinite();
	}

	// loadAllCalculatedCurve: 静态纯填充, 无副作用, 可重复调用
	private static void testLoadAllCalculatedCurve()
	{
		Dictionary<int, MyCurve> curveList = new();
		KeyFrameManager.loadAllCalculatedCurve(curveList);

		// 内置公式曲线数量应大于 0
		assertTrue(curveList.Count > 0, "loadAllCalculatedCurve 应填充内置曲线");
		assertTrue(curveList.ContainsKey(KEY_CURVE.ZERO_ONE), "应包含 ZERO_ONE");
		assertTrue(curveList.ContainsKey(KEY_CURVE.SINE_OUT), "应包含 SINE_OUT");
		assertTrue(curveList.ContainsKey(KEY_CURVE.QUAD_IN), "应包含 QUAD_IN");

		// 取出 ZERO_ONE 曲线实例并验证其可 evaluate
		MyCurve zeroOne = curveList[KEY_CURVE.ZERO_ONE];
		assertTrue(zeroOne != null, "ZERO_ONE 曲线实例非空");
		float mid = zeroOne.evaluate(0.5f);
		assertTrue(mid > 0.0f && mid < 1.0f, "ZERO_ONE 在 0.5 处取值在 (0,1)");

		// 再次填充到独立字典, 确认不互相污染, 且无异常
		Dictionary<int, MyCurve> curveList2 = new();
		KeyFrameManager.loadAllCalculatedCurve(curveList2);
		assertTrue(curveList2.Count == curveList.Count, "两次填充数量一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// getKeyFrame(0) 恒返回 null(源码: id==0 直接 return null, 零外部依赖)
	// ═════════════════════════════════════════════════════════════════
	private static void testGetKeyFrame_Zero_ReturnsNull()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			assertNull(mgr.getKeyFrame(0), "getKeyFrame(0) 返回 null");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 构造时 loadAllCalculatedCurve 已填充内置曲线, 取 ZERO_ONE 返回实例
	// ═════════════════════════════════════════════════════════════════
	private static void testGetKeyFrame_BuiltinCurve_ReturnsInstance()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve curve = mgr.getKeyFrame(KEY_CURVE.ZERO_ONE);
			assertNotNull(curve, "ZERO_ONE 内置曲线应被找到");
			// 连续两次返回同一缓存实例
			assertTrue(ReferenceEquals(curve, mgr.getKeyFrame(KEY_CURVE.ZERO_ONE)), "同一曲线返回同一引用");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 未注册的曲线ID返回 null(字典 get 未命中)
	// ═════════════════════════════════════════════════════════════════
	private static void testGetKeyFrame_UnknownID_ReturnsNull()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			assertNull(mgr.getKeyFrame(99999), "未注册的曲线ID返回 null");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// isLoadDone 默认 false(未经 initAsync 异步加载, mLoaded 恒为 false)
	// ═════════════════════════════════════════════════════════════════
	private static void testIsLoadDone_DefaultFalse()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			assertFalse(mgr.isLoadDone(), "未加载资源时 isLoadDone 为 false");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// destroy 清空曲线列表后 base.destroy; 可安全重复(幂等)
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroy_ClearsList()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		// 先确认构造后内部有曲线(通过 getKeyFrame 返回实例佐证)
		assertNotNull(mgr.getKeyFrame(KEY_CURVE.ZERO_ONE), "destroy 前内置曲线存在");
		mgr.destroy();
		// destroy 后再次调用 getKeyFrame 不再有曲线(列表已清空)
		assertNull(mgr.getKeyFrame(KEY_CURVE.ZERO_ONE), "destroy 后曲线列表被清空");
		// destroy 幂等: 二次调用不抛异常
		mgr.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合: 多个内置曲线都能取到且可 evaluate
	// ═════════════════════════════════════════════════════════════════
	private static void testGetKeyFrameMultipleBuiltin()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve zeroOne = mgr.getKeyFrame(KEY_CURVE.ZERO_ONE);
			MyCurve sineOut = mgr.getKeyFrame(KEY_CURVE.SINE_OUT);
			MyCurve quadIn = mgr.getKeyFrame(KEY_CURVE.QUAD_IN);
			assertTrue(zeroOne != null && sineOut != null && quadIn != null, "多个内置曲线均非空");
			// 每个曲线在 0.5 处可 evaluate 且结果有限
			float v0 = zeroOne.evaluate(0.5f);
			float v1 = sineOut.evaluate(0.5f);
			float v2 = quadIn.evaluate(0.5f);
			assertTrue(!float.IsNaN(v0) && !float.IsNaN(v1) && !float.IsNaN(v2), "各曲线 evaluate 非 NaN");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合: destroy 后静态 loadAllCalculatedCurve 仍正常(静态函数与实例状态无关)
	// ═════════════════════════════════════════════════════════════════
	private static void testLoadAllAfterDestroy()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		mgr.destroy();
		// destroy 后静态填充仍正常工作
		Dictionary<int, MyCurve> curveList = new();
		KeyFrameManager.loadAllCalculatedCurve(curveList);
		assertTrue(curveList.Count > 0, "destroy 后 loadAllCalculatedCurve 仍填充内置曲线");
		assertTrue(curveList.ContainsKey(KEY_CURVE.ZERO_ONE), "destroy 后仍包含 ZERO_ONE");
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合: 全部内置曲线 ID 都能取到且可 evaluate
	// ═════════════════════════════════════════════════════════════════
	private static void testGetKeyFrameAllBuiltin()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			// 实际内置曲线范围 ZERO_ONE(2)..SINE_IN_OUT(35); MAX_BUILDIN_CURVE=100 是预留上限(含空洞)
			int found = 0;
			for (int id = KEY_CURVE.ZERO_ONE; id <= KEY_CURVE.SINE_IN_OUT; ++id)
			{
				MyCurve curve = mgr.getKeyFrame(id);
				assertTrue(curve != null, "内置曲线 ID " + id + " 应存在");
				float v = curve.evaluate(0.5f);
				assertTrue(!float.IsNaN(v) && !float.IsInfinity(v), "内置曲线 ID " + id + " evaluate(0.5) 有限");
				++found;
			}
			assertTrue(found >= 30, "内置曲线数量应不少于 30, 实际 " + found);
		}
		finally
		{
			mgr.destroy();
		}
	}

	// 内置曲线端点 evaluate 有限(不炸)
	private static void testBuiltinCurveEndpointsFinite()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			for (int id = KEY_CURVE.ZERO_ONE; id <= KEY_CURVE.SINE_IN_OUT; ++id)
			{
				MyCurve curve = mgr.getKeyFrame(id);
				float v0 = curve.evaluate(0.0f);
				float v1 = curve.evaluate(1.0f);
				assertTrue(!float.IsNaN(v0) && !float.IsInfinity(v0), "ID " + id + " evaluate(0) 有限");
				assertTrue(!float.IsNaN(v1) && !float.IsInfinity(v1), "ID " + id + " evaluate(1) 有限");
			}
		}
		finally
		{
			mgr.destroy();
		}
	}
}
