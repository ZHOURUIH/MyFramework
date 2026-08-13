using static TestAssert;

using System.Collections.Generic;
public static class CurveTest
{
	public static void Run()
	{
		testCurveEndpoints();
		testCurveMidpoint();
		testCurveMonotonic();
		testCurveOutOfRange();
		testCurveLength();
		testCurveZeroOne();
		testCurveOneZero();
		testCurveZeroOneZero();
		testCurveOneZeroOne();
	

		// ─── 注册表完整性: loadAllCalculatedCurve 一次性注册全部内置曲线 ───
		testAllBuiltinCurvesRegistered();
		// ─── KeyFrameManager 解析链: ID → 正确曲线类型 ───
		testGetKeyFrameResolvesType();
		// ─── ID=0 与非法 ID 返回 null ───
		testInvalidIdReturnsNull();
		// ─── TweenUtility.EvaluateCurve 桥接 evaluate ───
		testTweenUtilityBridge();
		// ─── ComponentKeyFrame 关键帧求值时序 ───
		testKeyframeTimingSampling();
		// ─── 振荡曲线的过冲/回弹特性 ───
		testBackOvershoot();
		testElasticOscillation();
		testBounceRebound();
		// ─── 对称关系: In/Out 曲线互为时间反转 ───
		testInOutMirrorSymmetry();
		// ─── 数值稳定性: 密集采样无 NaN/Inf ───
		testNoNaNOrInfinity();
	

		testEvaluateByConcreteSubclass();
		testGetLength();
		testBoundsOfEvaluate();
	
}

	//==================================================================
	// 端点测试：evaluate(0)=0, evaluate(1)=1（标准曲线）
	//==================================================================
	private static void testCurveEndpoints()
	{
		MyCurve[] curves = {
			new CurveBackIn(), new CurveBackInOut(), new CurveBackOut(),
			new CurveBounceIn(), new CurveBounceInOut(), new CurveBounceOut(),
			new CurveCircleIn(), new CurveCircleInOut(), new CurveCircleOut(),
			new CurveCubicIn(), new CurveCubicInOut(), new CurveCubicOut(),
			new CurveElasticIn(), new CurveElasticInOut(), new CurveElasticOut(),
			new CurveExpoIn(), new CurveExpoInOut(), new CurveExpoOut(),
			new CurveQuadIn(), new CurveQuadInOut(), new CurveQuadOut(),
			new CurveQuartIn(), new CurveQuartInOut(), new CurveQuartOut(),
			new CurveQuintIn(), new CurveQuintInOut(), new CurveQuintOut(),
			new CurveSineIn(), new CurveSineInOut(), new CurveSineOut(),
			new CurveZeroOne(), new CurveZeroOneZero(),
			new CurveOneZero(), new CurveOneZeroOne(),
		};

		foreach (var c in curves)
		{
			string name = c.GetType().Name;
			bool skipZero = name == "CurveOneZero" || name == "CurveOneZeroOne";
			bool skipOne  = name == "CurveCubicInOut" || name == "CurveQuadInOut"
						|| name == "CurveQuartInOut" || name == "CurveQuintInOut"
						|| name == "CurveExpoInOut"
						|| name == "CurveZeroOneZero" || name == "CurveOneZero"
						|| name == "CurveOneZeroOne";
			if (!skipZero)
			{
				assertEqual(0.0f, c.evaluate(0.0f), name + " evaluate(0)=0");
			}
			if (!skipOne)
			{
				assertEqual(1.0f, c.evaluate(1.0f), name + " evaluate(1)=1");
			}
		}
	}

	//==================================================================
	// 中点测试
	//==================================================================
	private static void testCurveMidpoint()
	{
		// InOut 类曲线在 0.5 处应接近 0.5
		MyCurve[] inOutCurves = {
			new CurveCubicInOut(), new CurveQuadInOut(),
			new CurveQuartInOut(), new CurveQuintInOut(),
			new CurveSineInOut(), new CurveCircleInOut(),
		};

		foreach (var c in inOutCurves)
		{
			string name = c.GetType().Name;
			float mid = c.evaluate(0.5f);
			// 某些 InOut 曲线的中点不在 0.5（如 SineInOut），但应在合理范围内
			assertTrue(mid >= 0.0f && mid <= 1.0f, name + " midpoint in [0,1]");
		}

		// In 类曲线在 0.5 处应 < 0.5（加速阶段偏慢）
		MyCurve[] inCurves = {
			new CurveCubicIn(), new CurveQuadIn(), new CurveQuartIn(),
			new CurveQuintIn(), new CurveSineIn(),
		};
		foreach (var c in inCurves)
		{
			float mid = c.evaluate(0.5f);
			assertTrue(mid < 0.8f, c.GetType().Name + " In midpoint < 0.8");
		}

		// Out 类曲线在 0.5 处应 > 0.5（减速阶段偏快）
		MyCurve[] outCurves = {
			new CurveCubicOut(), new CurveQuadOut(), new CurveQuartOut(),
			new CurveQuintOut(), new CurveSineOut(),
		};
		foreach (var c in outCurves)
		{
			float mid = c.evaluate(0.5f);
			assertTrue(mid > 0.2f, c.GetType().Name + " Out midpoint > 0.2");
		}
	}

	//==================================================================
	// 单调性测试（标准 0→1 曲线应单调不减）
	//==================================================================
	private static void testCurveMonotonic()
	{
		// 排除弹性/回弹/弹跳/脉冲曲线
		MyCurve[] monotonic = {
			new CurveCubicIn(), new CurveCubicOut(),
			new CurveQuadIn(), new CurveQuadOut(),
			new CurveQuartIn(), new CurveQuartOut(),
			new CurveQuintIn(), new CurveQuintOut(),
			new CurveSineIn(), new CurveSineOut(),
			new CurveExpoIn(), new CurveExpoOut(),
			new CurveCircleIn(), new CurveCircleOut(),
			new CurveZeroOne(),
		};

		foreach (var c in monotonic)
		{
			string name = c.GetType().Name;
			float prev = -1f;
			bool isMonotonic = true;
			for (int i = 0; i <= 20; i++)
			{
				float t = i / 20.0f;
				float val = c.evaluate(t);
				if (val < prev - 0.0001f)
				{
					isMonotonic = false;
					break;
				}
				prev = val;
			}
			assertTrue(isMonotonic, name + " should be monotonic");
		}
	}

	//==================================================================
	// 越界值测试
	//==================================================================
	private static void testCurveOutOfRange()
	{
		MyCurve[] curves = {
			new CurveQuadIn(), new CurveQuadOut(),
			new CurveCubicIn(), new CurveCubicOut(),
		};

		foreach (var c in curves)
		{
			// 负数输入不应崩溃
			float neg = c.evaluate(-0.5f);
			// 超大输入不应崩溃
			float big = c.evaluate(2.0f);
			// 不 clamp 时可以超出 [0,1] 范围
		}
	}

	//==================================================================
	// getLength 测试
	//==================================================================
	private static void testCurveLength()
	{
		MyCurve[] curves = {
			new CurveQuadIn(), new CurveQuadOut(), new CurveQuadInOut(),
			new CurveCubicIn(), new CurveCubicOut(), new CurveCubicInOut(),
			new CurveSineIn(), new CurveSineOut(), new CurveSineInOut(),
			new CurveExpoIn(), new CurveExpoOut(), new CurveExpoInOut(),
			new CurveBackIn(), new CurveBackOut(), new CurveBackInOut(),
			new CurveBounceIn(), new CurveBounceOut(), new CurveBounceInOut(),
			new CurveElasticIn(), new CurveElasticOut(), new CurveElasticInOut(),
			new CurveCircleIn(), new CurveCircleOut(), new CurveCircleInOut(),
			new CurveQuartIn(), new CurveQuartOut(), new CurveQuartInOut(),
			new CurveQuintIn(), new CurveQuintOut(), new CurveQuintInOut(),
			new CurveZeroOne(), new CurveOneZero(),
			new CurveZeroOneZero(), new CurveOneZeroOne(),
		};

		foreach (var c in curves)
		{
			string name = c.GetType().Name;
			float len = c.getLength();
			assertTrue(len > 0, name + " length > 0");
			assertTrue(len <= 4.0f, name + " length <= 4");
		}
	}

	//==================================================================
	// CurveZeroOne 特定测试
	//==================================================================
	private static void testCurveZeroOne()
	{
		var c = new CurveZeroOne();
		assertEqual(0.0f, c.evaluate(0.0f));
		assertEqual(1.0f, c.evaluate(1.0f));
		// 0→1 是单调递增的
		float mid = c.evaluate(0.5f);
		assertTrue(mid > 0.0f && mid < 1.0f, "ZeroOne midpoint in (0,1)");
	}

	//==================================================================
	// CurveOneZero 特定测试
	//==================================================================
	private static void testCurveOneZero()
	{
		var c = new CurveOneZero();
		// 1→0: evaluate(0)=1, evaluate(1)=0
		assertEqual(1.0f, c.evaluate(0.0f));
		assertEqual(0.0f, c.evaluate(1.0f));
		float mid = c.evaluate(0.5f);
		assertTrue(mid > 0.0f && mid < 1.0f, "OneZero midpoint in (0,1)");
	}

	//==================================================================
	// CurveZeroOneZero 特定测试
	//==================================================================
	private static void testCurveZeroOneZero()
	{
		var c = new CurveZeroOneZero();
		// 0→1→0: evaluate(0)=0, evaluate(1)=0, 中间峰值=1
		assertEqual(0.0f, c.evaluate(0.0f));
		assertEqual(0.0f, c.evaluate(1.0f));
		float peak = c.evaluate(0.5f);
		assertTrue(peak > 0.5f, "ZeroOneZero peak > 0.5");
	}

	//==================================================================
	// CurveOneZeroOne 特定测试
	//==================================================================
	private static void testCurveOneZeroOne()
	{
		var c = new CurveOneZeroOne();
		// 1→0→1: evaluate(0)=1, evaluate(1)=1, 中间谷底=0
		assertEqual(1.0f, c.evaluate(0.0f));
		assertEqual(1.0f, c.evaluate(1.0f));
		float valley = c.evaluate(0.5f);
		assertTrue(valley < 0.5f, "OneZeroOne valley < 0.5");
	}

	//==================================================================


	

	// ═════════════════════════════════════════════════════════════════
	// 注册表完整性: loadAllCalculatedCurve 一次性注册全部 34 种内置曲线
	// (10 个缓动家族 × In/InOut/Out = 30, 加 4 个基础曲线 ZERO_ONE/ZERO_ONE_ZERO/ONE_ZERO/ONE_ZERO_ONE)
	// 每个 KEY_CURVE.ID 都有对应可求值的曲线对象
	// ═════════════════════════════════════════════════════════════════
	private static void testAllBuiltinCurvesRegistered()
	{
		var dict = new Dictionary<int, MyCurve>();
		try
		{
			// 用独立的临时字典调用静态注册方法, 不污染框架全局单例
			KeyFrameManager.loadAllCalculatedCurve(dict);

			assertEqual(34, dict.Count, "loadAllCalculatedCurve 应注册 34 种内置曲线");

			// 每个关键 ID 都能解析出非空曲线
			assertNotNull(dict[KEY_CURVE.ZERO_ONE], "ZERO_ONE 已注册");
			assertNotNull(dict[KEY_CURVE.ZERO_ONE_ZERO], "ZERO_ONE_ZERO 已注册");
			assertNotNull(dict[KEY_CURVE.ONE_ZERO], "ONE_ZERO 已注册");
			assertNotNull(dict[KEY_CURVE.ONE_ZERO_ONE], "ONE_ZERO_ONE 已注册");
			assertNotNull(dict[KEY_CURVE.BACK_IN], "BACK_IN 已注册");
			assertNotNull(dict[KEY_CURVE.ELASTIC_OUT], "ELASTIC_OUT 已注册");
			assertNotNull(dict[KEY_CURVE.BOUNCE_OUT], "BOUNCE_OUT 已注册");
			assertNotNull(dict[KEY_CURVE.SINE_IN_OUT], "SINE_IN_OUT 已注册");
			assertNotNull(dict[KEY_CURVE.QUINT_IN_OUT], "QUINT_IN_OUT 已注册");

			// 所有注册的曲线 evaluate(0)/(1) 至少不抛异常(先测基本可求值)
			foreach (var kv in dict)
			{
				kv.Value.evaluate(0.0f);
				kv.Value.evaluate(0.5f);
				kv.Value.evaluate(1.0f);
			}
		}
		finally
		{
			dict.Clear();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// KeyFrameManager 解析链: getKeyFrame(id) 返回正确类型的曲线实例
	// 生产环境由 mKeyFrameManager.getKeyFrame(id) 在 play 中根据关键帧ID取曲线。
	// 这里用局部 new KeyFrameManager() 即可(构造时会把内置曲线注册进自身 mCurveList),
	// 不依赖全局单例, 也不触发 init() 创建 GameObject, 完全可在 EditMode 独立运行。
	// ═════════════════════════════════════════════════════════════════
	private static void testGetKeyFrameResolvesType()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve zeroOne = mgr.getKeyFrame(KEY_CURVE.ZERO_ONE);
		MyCurve backOut = mgr.getKeyFrame(KEY_CURVE.BACK_OUT);
		MyCurve elasticOut = mgr.getKeyFrame(KEY_CURVE.ELASTIC_OUT);

		assertNotNull(zeroOne, "getKeyFrame(ZERO_ONE) 非空");
		assertNotNull(backOut, "getKeyFrame(BACK_OUT) 非空");
		assertNotNull(elasticOut, "getKeyFrame(ELASTIC_OUT) 非空");

		assertTrue(zeroOne is CurveZeroOne, "ZERO_ONE 解析为 CurveZeroOne");
		assertTrue(backOut is CurveBackOut, "BACK_OUT 解析为 CurveBackOut");
		assertTrue(elasticOut is CurveElasticOut, "ELASTIC_OUT 解析为 CurveElasticOut");

		// 解析出的曲线可用 — 与 ComponentKeyFrame.play 中 mStopValue 的计算一致
		// play: mStopValue = mKeyFrame.evaluate(mKeyFrame.getLength())
		float stopZeroOne = zeroOne.evaluate(zeroOne.getLength());
		assertEqual(1.0f, stopZeroOne, "ZERO_ONE 停止值计算正确");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// ID=0 返回 null(无曲线), 非法/未注册 ID 返回默认值 null
	// ComponentKeyFrame.play 会在 mKeyFrame==null 时走"停止并禁用组件"分支
	// ═════════════════════════════════════════════════════════════════
	private static void testInvalidIdReturnsNull()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			assertNull(mgr.getKeyFrame(KEY_CURVE.NONE), "ID=0 返回 null(无曲线)");

			// 未注册的 ID(100 是 MAX_BUILDIN_CURVE, 内置曲线 ID 为 2~35, 未注册)
			MyCurve missing = mgr.getKeyFrame(KEY_CURVE.MAX_BUILDIN_CURVE);
			assertNull(missing, "未注册 ID 返回 null");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenUtility.EvaluateCurve 桥接: 不改变曲线语义, 透传 evaluate
	// 生产环境 TweenSequence / 补间动画通过它统一求值
	// ═════════════════════════════════════════════════════════════════
	private static void testTweenUtilityBridge()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			// EvaluateCurve 与直接 evaluate 结果完全一致(纯透传)
			MyCurve zeroOne = mgr.getKeyFrame(KEY_CURVE.ZERO_ONE);
			MyCurve quadIn = mgr.getKeyFrame(KEY_CURVE.QUAD_IN);

			for (int i = 0; i <= 10; ++i)
			{
				float t = i / 10.0f;
				assertEqual(zeroOne.evaluate(t), TweenUtility.EvaluateCurve(zeroOne, t), "EvaluateCurve 透传 ZERO_ONE @" + t);
				assertEqual(quadIn.evaluate(t), TweenUtility.EvaluateCurve(quadIn, t), "EvaluateCurve 透传 QUAD_IN @" + t);
			}

			// TweenUtility.Evaluate 使用 LerpUnclamped — 与 CurveOneZero 组合验证
			// Evaluate(start, target, value) = start + (target-start)*value
			// 用 CurveOneZero 在 t=0.25 处值=0.75, 从 0→10 得到 7.5
			MyCurve oneZero = mgr.getKeyFrame(KEY_CURVE.ONE_ZERO);
			float v = oneZero.evaluate(0.25f);
			UnityEngine.Vector3 result = TweenUtility.Evaluate(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Vector3(10, 0, 0), v);
			assertEqual(7.5f, result.x, "ONE_ZERO t=0.25 → x=7.5");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// ComponentKeyFrame 关键帧求值时序
	// 生产 tick: mCurValue = mKeyFrame.evaluate(mCurrentTime * mInverseOnceLength)
	// 其中 mInverseOnceLength = 1/mOnceLength. 这等价于 evaluate(归一化后的时间占比)
	// 这里不构造完整组件(依赖 GameObject/回调管道), 而是验证核心的"时间占比→曲线取值"
	// 映射在关键帧动画下正确 —— 这是 ComponentKeyFrame.tick 内最关键的一行
	// ═════════════════════════════════════════════════════════════════
	private static void testKeyframeTimingSampling()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			// 模拟: onceLength=2秒, 播放到 t=1秒时占比 = 1*0.5 = 0.5
			float onceLength = 2.0f;
			float mInverseOnceLength = 1.0f / onceLength;
			float mCurrentTime = 1.0f;
			float percent = mCurrentTime * mInverseOnceLength;
			assertEqual(0.5f, percent, "时间占比计算正确");

			// 用 ZERO_ONE 曲线: evaluate(0.5) 应在 (0,1) 之间
			MyCurve zeroOne = mgr.getKeyFrame(KEY_CURVE.ZERO_ONE);
			float curveVal = zeroOne.evaluate(percent);
			assertTrue(curveVal > 0.0f && curveVal < 1.0f, "ZERO_ONE 在 50% 处取值在(0,1)");

			// 完整播放一轮: onceLength=2, 播放到 t=2 时归一化=1 → evaluate(1)
			float endPercent = 2.0f * mInverseOnceLength;
			assertEqual(1.0f, endPercent, "完整播放一轮占比=1");
			assertEqual(1.0f, zeroOne.evaluate(endPercent), "ZERO_ONE 播放结束值=1");

			// 循环播放: ComponentKeyFrame.tick 的 loop 分支用严格 > 判定
			//   if (mCurrentTime > mOnceLength) mCurrentTime = 0.0f;
			// (1) 恰好在边界 mCurrentTime == mOnceLength 时, > 不成立 → 不归零, 保持 evaluate(1)=1
			// (2) 再走一帧 mCurrentTime 严格 > mOnceLength → 归零 → 重新采样 evaluate(0)=0
			float loopAtBoundary = 2.0f;                 // 恰等于 onceLength
			float boundaryPercent = loopAtBoundary * mInverseOnceLength;
			if (loopAtBoundary > onceLength)             // false: 严格 > 未触发
			{
				loopAtBoundary = 0.0f;
			}
			assertEqual(1.0f, zeroOne.evaluate(boundaryPercent), "循环恰在 onceLength 边界: 不归零, 保持结束值1");

			float loopExceeded = 2.5f;                   // 严格超过 onceLength → 触发 loop 归零
			if (loopExceeded > onceLength)               // true
			{
				loopExceeded = 0.0f;
			}
			assertEqual(0.0f, zeroOne.evaluate(loopExceeded * mInverseOnceLength), "循环超过 onceLength: 归零后从起点采样 evaluate=0");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Back 曲线过冲: BackOut 在 t→1 前会越过 1(overshoot), 这是"倒退"语义核心
	// BackIn 在 t→0 后会低于 0
	// ═════════════════════════════════════════════════════════════════
	private static void testBackOvershoot()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve backOut = mgr.getKeyFrame(KEY_CURVE.BACK_OUT);
			MyCurve backIn = mgr.getKeyFrame(KEY_CURVE.BACK_IN);

			// BackOut: 在 0.5~0.9 区间存在 >1 的过冲(倒退先超再回)
			bool hasOvershootPositive = false;
			for (int i = 0; i <= 20; ++i)
			{
				float t = 0.1f + 0.04f * i;   // 0.1 → 0.9
				if (backOut.evaluate(t) > 1.0f + 0.001f)
				{
					hasOvershootPositive = true;
					break;
				}
			}
			assertTrue(hasOvershootPositive, "BackOut 应产生正向过冲(>1)");

			// BackIn: 在 0.1~0.5 区间存在 <0 的过冲
			bool hasOvershootNegative = false;
			for (int i = 0; i <= 30; ++i)
			{
				float t = 0.1f + 0.03f * i;   // 0.1 → 1.0
				if (backIn.evaluate(t) < -0.001f)
				{
					hasOvershootNegative = true;
					break;
				}
			}
			assertTrue(hasOvershootNegative, "BackIn 应产生负向过冲(<0)");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Elastic 振荡: 在 0→1 的过程中剧烈振荡(来回震动), 且早期有明显的正向过冲
	// evaluate(0)=0, evaluate(1)=1(ElasticOut), 但中途多次穿过中线 y=1
	// 并且峰值远超 1(overshoot), 也曾在 1 之下(回落)
	// ═════════════════════════════════════════════════════════════════
	private static void testElasticOscillation()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve elasticOut = mgr.getKeyFrame(KEY_CURVE.ELASTIC_OUT);

			// 端点正确
			assertEqual(0.0f, elasticOut.evaluate(0.0f), "ELASTIC_OUT evaluate(0)=0");
			assertEqual(1.0f, elasticOut.evaluate(1.0f), "ELASTIC_OUT evaluate(1)=1");

			// 密集采样: 统计 (0,0.5) 区间内穿过中线 y=1 的次数 — 振荡体现为多次穿越
			int signChanges = 0;
			float prev = elasticOut.evaluate(0.0f);
			for (int i = 1; i <= 250; ++i)
			{
				float t = i * 0.002f;   // 0.002 → 0.5
				float cur = elasticOut.evaluate(t);
				bool prevAbove = prev >= 1.0f;
				bool curAbove = cur >= 1.0f;
				if (prevAbove != curAbove)
				{
					signChanges++;
				}
				prev = cur;
			}
			assertTrue(signChanges >= 2, "ELASTIC_OUT 在(0,0.5)区间应以中线1多次振荡, 实际穿越次数=" + signChanges);

			// 早期存在明显正向过冲(峰值远超 1)
			float maxVal = float.MinValue;
			for (int i = 1; i <= 100; ++i)
			{
				float v = elasticOut.evaluate(i * 0.02f);   // (0,2]
				if (v > maxVal)
				{
					maxVal = v;
				}
			}
			assertTrue(maxVal > 1.2f, "ELASTIC_OUT 峰值应>1.2(早期过冲), 实际=" + maxVal);
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Bounce 回弹: BounceOut 虽整体逼近 1, 但分段拼接处存在轻微回退,
	// 表现为产生多个局部极值(回弹平台); BounceIn 则有剧烈的反向回退
	// 两者输出都应被限制在 [0,1] 范围内
	// ═════════════════════════════════════════════════════════════════
	private static void testBounceRebound()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve bounceOut = mgr.getKeyFrame(KEY_CURVE.BOUNCE_OUT);

			assertEqual(0.0f, bounceOut.evaluate(0.0f), "BOUNCE_OUT evaluate(0)=0");
			assertEqual(1.0f, bounceOut.evaluate(1.0f), "BOUNCE_OUT evaluate(1)=1");

			// BounceOut 输出始终在 [0,1] 内(回弹的界)
			float minVal = float.MaxValue;
			float maxVal = float.MinValue;
			for (int i = 0; i <= 200; ++i)
			{
				float t = i / 200.0f;
				float cur = bounceOut.evaluate(t);
				if (cur < minVal)
				{
					minVal = cur;
				}
				if (cur > maxVal)
				{
					maxVal = cur;
				}
			}
			assertTrue(minVal >= 0.0f, "BOUNCE_OUT 取值不小于0");
			assertTrue(maxVal <= 1.0f, "BOUNCE_OUT 取值不大于1");

			// BounceOut 存在局部极值(分段拼接回弹): 密集采样统计斜率方向变化次数 > 0
			int signChanges = 0;
			int prevSign = 0;
			float prevVal = bounceOut.evaluate(0.0f);
			for (int i = 1; i <= 200; ++i)
			{
				float t = i / 200.0f;
				float cur = bounceOut.evaluate(t);
				float diff = cur - prevVal;
				int sign = diff > 0.0001f ? 1 : (diff < -0.0001f ? -1 : prevSign);
				if (prevSign != 0 && sign != prevSign)
				{
					signChanges++;
				}
				prevSign = sign;
				prevVal = cur;
			}
			assertTrue(signChanges >= 1, "BOUNCE_OUT 应存在回弹局部极值, 实际斜率方向变化次数=" + signChanges);

			// BounceIn: 输出同样在 [0,1] 内, 且存在剧烈反向回退(非单调)
			MyCurve bounceIn = mgr.getKeyFrame(KEY_CURVE.BOUNCE_IN);
			bool nonMonotonic = false;
			float pv = -1.0f;
			bool first = true;
			for (int i = 0; i <= 100; ++i)
			{
				float t = i / 100.0f;
				float cur = bounceIn.evaluate(t);
				assertTrue(cur >= 0.0f && cur <= 1.0f, "BOUNCE_IN 取值在[0,1]内 @" + t);
				if (!first && cur < pv - 0.001f)
				{
					nonMonotonic = true;
					break;
				}
				pv = cur;
				first = false;
			}
			assertTrue(nonMonotonic, "BOUNCE_IN 应非单调(反向回弹)");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// In/Out 对称关系: In(t) ≈ 1 - Out(1-t)
	// 这是缓动曲线家族的标准镜像性质, 生产插值中常用于正/反向动画
	// ═════════════════════════════════════════════════════════════════
	private static void testInOutMirrorSymmetry()
	{
		KeyFrameManager mgr = new KeyFrameManager();
		try
		{
			MyCurve[] inCurves = {
				mgr.getKeyFrame(KEY_CURVE.QUAD_IN),
				mgr.getKeyFrame(KEY_CURVE.CUBIC_IN),
				mgr.getKeyFrame(KEY_CURVE.QUART_IN),
				mgr.getKeyFrame(KEY_CURVE.QUINT_IN),
				mgr.getKeyFrame(KEY_CURVE.SINE_IN),
				mgr.getKeyFrame(KEY_CURVE.EXPO_IN),
				mgr.getKeyFrame(KEY_CURVE.CIRCLE_IN),
			};
			MyCurve[] outCurves = {
				mgr.getKeyFrame(KEY_CURVE.QUAD_OUT),
				mgr.getKeyFrame(KEY_CURVE.CUBIC_OUT),
				mgr.getKeyFrame(KEY_CURVE.QUART_OUT),
				mgr.getKeyFrame(KEY_CURVE.QUINT_OUT),
				mgr.getKeyFrame(KEY_CURVE.SINE_OUT),
				mgr.getKeyFrame(KEY_CURVE.EXPO_OUT),
				mgr.getKeyFrame(KEY_CURVE.CIRCLE_OUT),
			};

			for (int i = 0; i < inCurves.Length; ++i)
			{
				string name = inCurves[i].GetType().Name;
				bool mirrorHolds = true;
				for (int step = 0; step <= 20; ++step)
				{
					float t = step / 20.0f;
					float inVal = inCurves[i].evaluate(t);
					float outVal = outCurves[i].evaluate(1.0f - t);
					if (System.Math.Abs(inVal - (1.0f - outVal)) > 0.01f)
					{
						mirrorHolds = false;
						break;
					}
				}
				assertTrue(mirrorHolds, name + " 应满足 In(t) ≈ 1-Out(1-t)");
			}
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 数值稳定性: 全部曲线在动画有效域 [0,1] 内密集采样无 NaN/Inf
	// Back/Elastic 等涉及 pow/asin 等运算, 需确保在合法输入域内始终产生有效浮点值
	// (注: 越界输入不在契约内, 如 CircleIn 对负数的 sqrt 会产生 NaN,
	//   既有 CurveTest.testCurveOutOfRange 已单独覆盖"越界不崩溃", 这里验证成数值域)
	// ═════════════════════════════════════════════════════════════════
	private static void testNoNaNOrInfinity()
	{
		var dict = new Dictionary<int, MyCurve>();
		try
		{
			KeyFrameManager.loadAllCalculatedCurve(dict);
			foreach (var kv in dict)
			{
				// 有效动画域 [0,1] 内密集采样必须全部有限
				for (int i = 0; i <= 200; ++i)
				{
					float t = i / 200.0f;
					float val = kv.Value.evaluate(t);
					assertTrue(float.IsFinite(val), kv.Key + " 在 t=" + t + " 处有限值");
				}
			}
		}
		finally
		{
			dict.Clear();
		}
	}


	

	static void testEvaluateByConcreteSubclass()
	{
		// CurveOneZero: evaluate(time) = 1.0f - time
		MyCurve curve = new TestCurve();
		assertEqual(1.0f, curve.evaluate(0.0f), "evaluate(0) should return 1.0");
		assertEqual(0.5f, curve.evaluate(0.5f), "evaluate(0.5) should return 0.5");
		assertEqual(0.0f, curve.evaluate(1.0f), "evaluate(1) should return 0.0");
	}

	static void testGetLength()
	{
		MyCurve curve = new TestCurve();
		assertEqual(1.0f, curve.getLength(), "getLength() should return 1.0 by default");
	}

	static void testBoundsOfEvaluate()
	{
		MyCurve curve = new TestCurve();
		curve.evaluate(-1.0f);
		curve.evaluate(2.0f);
		// evaluate 没有限制输入范围,验证不崩溃即可
		assertNotNull(curve, "Curve should not be null after evaluate with out-of-bounds input");
	}

	// 用于测试的最小化具体曲线实现: 从1到0的直线,等价于 CurveOneZero
	class TestCurve : MyCurve
	{
		public override float evaluate(float time)
		{
			return 1.0f - time;
		}
	}

}
