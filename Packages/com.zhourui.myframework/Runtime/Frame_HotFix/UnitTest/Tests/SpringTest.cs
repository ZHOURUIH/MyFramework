using static TestAssert;

// Spring 弹簧物理测试
// 覆盖：resetProperty / calculateElasticForce / setters / getters /
//        update 压缩 / 拉伸 / 回弹 / 最小长度限制
public static class SpringTest
{
    public static void Run()
    {
        testResetProperty();
        testSettersAndGetters();
        testCalculateElasticForce();
        testUpdateStretch();
        testUpdateCompress();
        testMinLengthClamp();
		testForceSettles();
		testDamping();
		testExternalForce();
		testZeroMass();
		testZeroSpringK();
	

		testStretchSettlesToNormal();
		testCompressSettlesToNormal();
		testOvershootAcrossNormal();
		testMonotonicApproachPhase();
		testExactEquilibriumWithExternalForce();
		testForceImbalanceBreaksEquilibrium();
		testMassInverseResponsiveness();
		testSpringConstantResponsiveness();
		testMinLengthPinUnderStrongCompression();
		testMinLengthChainsOverManyFrames();
		testSignFlipSettleTiming();
		testParameterSweepStability();
	}

    // ─── resetProperty ──────────────────────────────────────────────────
    private static void testResetProperty()
    {
        var s = new Spring();
        s.setNormaLength(5.0f);
        s.setMass(2.0f);
        s.setSpringK(3.0f);
        s.setForce(1.0f);
        s.setCurLength(6.0f);

        s.resetProperty();

        assert(s.getNormalLength().isEqual(0.0f), "resetProperty normalLength=0");
        assert(s.getLength().isEqual(0.0f), "resetProperty curLength=0");
        assert(s.getSpeed().isEqual(0.0f), "resetProperty speed=0");
        // 检查 mObjectMass mSpringK mMinLength 是否回到默认值（通过弹力间接验证）
        // mNormalLength=0, mCurLength=0 → elasticForce = (0-0)*k = 0
        assert(s.calculateElasticForce().isEqual(0.0f), "resetProperty elasticForce=0");
    }

    // ─── setters / getters ──────────────────────────────────────────────
    private static void testSettersAndGetters()
    {
        var s = new Spring();
        s.setNormaLength(3.0f);
        s.setCurLength(5.0f);
        s.setMass(2.0f);
        s.setSpringK(4.0f);
        s.setSpeed(1.5f);
        s.setForce(0.5f);

        assert(s.getNormalLength().isEqual(3.0f), "getNormalLength=3");
        assert(s.getLength().isEqual(5.0f), "getLength=5");
        assert(s.getSpeed().isEqual(1.5f), "getSpeed=1.5");
    }

    // ─── calculateElasticForce ──────────────────────────────────────────
    private static void testCalculateElasticForce()
    {
        var s = new Spring();
        // normalLength=2, curLength=5, k=1 → force=(5-2)*1=3（压缩弹簧方向为正）
        s.setNormaLength(2.0f);
        s.setCurLength(5.0f);
        s.setSpringK(1.0f);
        assert(s.calculateElasticForce().isEqual(3.0f, 0.001f),
            "elasticForce 拉伸=(5-2)*1=3");

        // normalLength=5, curLength=3, k=2 → force=(3-5)*2=-4（负值→压缩）
        s.setNormaLength(5.0f);
        s.setCurLength(3.0f);
        s.setSpringK(2.0f);
        assert(s.calculateElasticForce().isEqual(-4.0f, 0.001f),
            "elasticForce 压缩=(3-5)*2=-4");

        // 处于自然长度
        s.setNormaLength(4.0f);
        s.setCurLength(4.0f);
        assert(s.calculateElasticForce().isEqual(0.0f, 0.001f),
            "elasticForce 自然长度=0");
    }

    // ─── update 拉伸收缩过程 ─────────────────────────────────────────────
    private static void testUpdateStretch()
    {
        // 弹簧被拉伸：normalLength=1, curLength=3, k=1, mass=1, 无外力
        // elasticForce = (3-1)*1 = 2 → update 施加力 -2（回弹）
        // 加速度 = (0 + (-2)) / 1 = -2 → 速度增加 -2*dt
        // curLength 减少
        var s = new Spring();
        s.setNormaLength(1.0f);
        s.setCurLength(3.0f);
        s.setSpringK(1.0f);
        s.setMass(1.0f);
        s.setForce(0.0f);

        float lengthBefore = s.getLength();
        s.update(0.1f);
        float lengthAfter = s.getLength();

        assert(lengthAfter < lengthBefore, "update 拉伸后长度减小（回弹）");
    }

    // ─── update 压缩弹簧 ─────────────────────────────────────────────────
    private static void testUpdateCompress()
    {
        // 弹簧被压缩到比自然长度短：normalLength=3, curLength=1, k=1, mass=1
        // elasticForce = (1-3)*1 = -2 → update 施加力 +2（回弹拉伸）
        var s = new Spring();
        s.setNormaLength(3.0f);
        s.setCurLength(1.0f);  // 大于 minLength(0.5)
        s.setSpringK(1.0f);
        s.setMass(1.0f);
        s.setForce(0.0f);

        float lengthBefore = s.getLength();
        s.update(0.1f);
        float lengthAfter = s.getLength();

        assert(lengthAfter > lengthBefore, "update 压缩后长度增大（回弹）");
    }

    // ─── 最小长度限制 ─────────────────────────────────────────────────────
    private static void testMinLengthClamp()
    {
        // 设置 minLength=2, 当前长度=2，施加巨大压缩力，update后长度不应小于minLength
        // 用间接方式：设置 curLength 接近 minLength，给负速度，确认不突破
        var s = new Spring();
        s.setNormaLength(10.0f);
        s.setCurLength(0.6f);   // 刚好略大于默认 minLength=0.5
        s.setSpringK(1.0f);
        s.setMass(1.0f);
        s.setForce(-100.0f);    // 强压缩力
        s.setSpeed(-50.0f);

        // 多帧更新
        for (int i = 0; i < 10; ++i)
        {
            s.update(0.1f);
        }

        assert(s.getLength() >= 0.5f - 0.001f, "minLength 限制：长度不低于0.5");
    }

    // ─── 弹簧最终趋于稳定 ────────────────────────────────────────────────
    private static void testForceSettles()
    {
        // normalLength=5, 从 curLength=5 出发，无外力，update若干帧应保持稳定
        var s = new Spring();
        s.setNormaLength(5.0f);
        s.setCurLength(5.0f);
        s.setSpringK(1.0f);
        s.setMass(1.0f);
        s.setForce(0.0f);
        s.setSpeed(0.0f);

        float lengthBefore = s.getLength();
        s.update(0.1f);
        float lengthAfter = s.getLength();

        // 处于自然长度，无外力，速度=0 → elasticForce=0 → 加速度=0 → 长度不变
        assert(lengthBefore.isEqual(lengthAfter, 0.001f), "自然长度无外力 update后长度不变");
	}
	private static void testDamping()
	{
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(5.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(0.0f);
		// 多次 update 后速度应趋近 0（有阻尼）
		float prevLength = s.getLength();
		for (int i = 0; i < 50; ++i)
		{
			s.update(0.05f);
		}
		float finalLength = s.getLength();
		assertTrue(finalLength < prevLength, "damping 后长度应收敛");
	}
	private static void testExternalForce()
	{
		// 外力可以抵消弹力，保持长度不变
		var s = new Spring();
		s.setNormaLength(1.0f);
		s.setCurLength(3.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		// elasticForce = (3-1)*1 = 2, 施加外力 -2 抵消
		s.setForce(-2.0f);
		s.setSpeed(0.0f);

		float before = s.getLength();
		s.update(0.1f);
		float after = s.getLength();
		// 外力抵消弹力，长度应不变（忽略阻尼）
		assertTrue(before.isEqual(after, 0.1f), "外力抵消弹力后长度近似不变");
	}
	private static void testZeroMass()
	{
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(5.0f);
		s.setSpringK(1.0f);
		s.setMass(0.0f);
		// mass=0 不应崩溃
		s.update(0.1f);
		assertTrue(s.getLength() > 0, "mass=0 不崩溃");
	}
	private static void testZeroSpringK()
	{
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(5.0f);
		s.setSpringK(0.0f);
		// springK=0 时弹力为 0，长度不变
		float before = s.getLength();
		s.update(0.1f);
		assertTrue(before.isEqual(s.getLength(), 0.001f), "springK=0 长度不变");
	}


	

	// ─── 拉伸释放收敛到自然长度 ──────────────────────────────────────────
	// 弹簧从拉长状态释放, 无外力, 经多次 update 应收敛于 normalLength
	private static void testStretchSettlesToNormal()
	{
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(5.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		// 持续更新直到收敛
		float last = s.getLength();
		for (int i = 0; i < 200; ++i)
		{
			s.update(0.1f);
			last = s.getLength();
		}
		assertTrue(last.isEqual(s.getNormalLength(), 0.02f),
			"拉伸释放最终收敛于自然长度, 实测=" + last);
		// 收敛方向: 从拉长(5)收缩回到自然长度(2), 最终长度必须小于初始长度
		assertTrue(last < 5.0f, "拉伸释放长度单调回落, 最终=" + last);
	}

	// ─── 压缩释放收敛到自然长度 ──────────────────────────────────────────
	private static void testCompressSettlesToNormal()
	{
		var s = new Spring();
		s.setNormaLength(3.0f);
		s.setCurLength(1.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		float last = s.getLength();
		for (int i = 0; i < 200; ++i)
		{
			s.update(0.1f);
			last = s.getLength();
		}
		assertTrue(last.isEqual(s.getNormalLength(), 0.02f),
			"压缩释放最终收敛于自然长度, 实测=" + last);
		assertTrue(last > 1.0f, "压缩释放长度单调回升, 最终=" + last);
	}

	// ─── 过冲: 释放后越过自然长度再吸附 ─────────────────────────────────
	// 离散积分在符号翻转时会吸附(速度清零), 但在吸附前弹簧会越过自然长度,
	// 证明存在"过冲"这一涌现行为(拉伸释放会短暂低于 normal, 压缩释放会短暂高于 normal)
	private static void testOvershootAcrossNormal()
	{
		// 拉伸释放: normal=2, 释放后长度先降, 会越过 2 下降到 2 以下
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(5.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		bool overshotBelow = false;
		for (int i = 0; i < 50; ++i)
		{
			s.update(0.3f);   // 大步长更易越过平衡点
			if (s.getLength() < 2.0f)
			{
				overshotBelow = true;
			}
		}
		assertTrue(overshotBelow, "拉伸释放应存在越过自然长度(向下过冲)的时刻");
		// 同理由压缩释放: normal=2, 释放后长度先升, 会越过 2 升到 2 以上
		var c = new Spring();
		c.setNormaLength(2.0f);
		c.setCurLength(0.6f);
		c.setSpringK(1.0f);
		c.setMass(1.0f);
		c.setForce(0.0f);
		c.setSpeed(0.0f);
		bool overshotAbove = false;
		for (int i = 0; i < 50; ++i)
		{
			c.update(0.3f);
			if (c.getLength() > 2.0f)
			{
				overshotAbove = true;
			}
		}
		assertTrue(overshotAbove, "压缩释放应存在越过自然长度(向上过冲)的时刻");
	}

	// ─── 逼近阶段单调性 ─────────────────────────────────────────────────
	// 拉伸释放: 在越过自然长度前, 长度应严格单调递减(每一步都比上一步小)
	private static void testMonotonicApproachPhase()
	{
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(5.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		bool monotonic = true;
		float prev = s.getLength();
		for (int i = 0; i < 14; ++i)   // 选在达到/越过 normal 前的步数
		{
			s.update(0.1f);
			float cur = s.getLength();
			if (cur > prev + 0.00001f)
			{
				monotonic = false;
			}
			prev = cur;
		}
		// 逼近后仍高于 normal, 保证这段是"逼近"而非"过冲回摆"
		assertTrue(s.getLength() > 2.0f, "逼近阶段尚未越过自然长度, len=" + s.getLength());
		assertTrue(monotonic, "拉伸释放逼近阶段长度应单调递减");
	}

	// ─── 外力精确抵消弹力 → 静止平衡 ────────────────────────────────────
	// 弹力 = (len - normal)*k, update 内部取 *-1 施加, 因此令 mForce = (len-normal)*k
	// 则合力 = mForce + 弹力*-1 = 0 → 加速度恒 0 → 长度恒定(静止平衡)
	private static void testExactEquilibriumWithExternalForce()
	{
		var s = new Spring();
		s.setNormaLength(5.0f);
		s.setCurLength(7.0f);
		s.setSpringK(2.0f);
		s.setMass(1.0f);
		s.setSpeed(0.0f);
		s.setForce((7.0f - 5.0f) * 2.0f);   // = +4, 抵消弹力
		float before = s.getLength();
		for (int i = 0; i < 20; ++i)
		{
			s.update(0.1f);
		}
		float after = s.getLength();
		assertTrue(before.isEqual(after, 0.0005f),
			"外力精确抵消弹力时长度应恒定, before=" + before);
		assertTrue(s.getSpeed().isEqual(0.0f, 0.0005f),
			"静止平衡下速度应保持 0, speed=" + s.getSpeed());
	}

	// ─── 微小的力不平衡会破坏平衡并朝预测方向移动 ──────────────────────
	// 弹力被外力部分抵消但未完全抵消时, 剩余合力会把长度拉回 normal
	private static void testForceImbalanceBreaksEquilibrium()
	{
		// normal=5 len=7: 弹力 = 4, 完全抵消需 force=4; 用 force=1 只抵消一部分
		var s = new Spring();
		s.setNormaLength(5.0f);
		s.setCurLength(7.0f);
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setSpeed(0.0f);
		s.setForce(1.0f);
		float before = s.getLength();
		for (int i = 0; i < 60; ++i)
		{
			s.update(0.1f);
		}
		float after = s.getLength();
		// 弹力占优(2 → 回拉), 长度应显著小于 7
		assertTrue(after < before - 0.5f,
			"力不平衡应产生向自然长度的净运动, before=" + before);
		// 用过大/过小的力方向分别验证移动方向
		// 过大力(>抵消): force=10 会把弹簧进一步拉长
		var s2 = new Spring();
		s2.setNormaLength(5.0f);
		s2.setCurLength(7.0f);
		s2.setSpringK(1.0f);
		s2.setMass(1.0f);
		s2.setSpeed(0.0f);
		s2.setForce(10.0f);
		for (int i = 0; i < 60; ++i)
		{
			s2.update(0.1f);
		}
		assertTrue(s2.getLength() > 7.0f + 0.5f,
			"外力大于弹力应将弹簧进一步拉长, len=" + s2.getLength());
	}

	// ─── 质量对响应快慢的反比例影响 ───────────────────────────────────
	// 相同初始条件, 质量越大加速度越小 → 单帧位移越小
	private static void testMassInverseResponsiveness()
	{
		int frames = 3;
		float dispMass1 = measureInitialDisplacement(1.0f, frames);
		float dispMass2 = measureInitialDisplacement(2.0f, frames);
		float dispMass10 = measureInitialDisplacement(10.0f, frames);
		// 质量越大位移越小
		assertTrue(dispMass1 > dispMass2, "mass=1 位移应大于 mass=2");
		assertTrue(dispMass2 > dispMass10, "mass=2 位移应大于 mass=10");
		// 位移近似反比于质量(前几帧欧拉积分近似线性)
		assertTrue((dispMass1 / dispMass2).isEqual(2.0f, 0.06f),
			"位移应与质量成反比, ratio=" + (dispMass1 / dispMass2));
	}

	// ─── 弹簧系数对初始响应强弱的影响 ─────────────────────────────────
	// 相同初始条件, k 越大弹力越强 → 单帧位移越大(响应更敏捷)
	private static void testSpringConstantResponsiveness()
	{
		int frames = 3;
		float dispK1 = measureInitialDisplacementK(1.0f, frames);
		float dispK2 = measureInitialDisplacementK(2.0f, frames);
		float dispK4 = measureInitialDisplacementK(4.0f, frames);
		assertTrue(dispK4 > dispK2, "k=4 位移应大于 k=2");
		assertTrue(dispK2 > dispK1, "k=2 位移应大于 k=1");
		// k 越大收敛到自然长度越快(更少帧达到阈值)
		int framesToSettleK1 = framesToApproachNormal(1.0f);
		int framesToSettleK4 = framesToApproachNormal(4.0f);
		assertTrue(framesToSettleK4 <= framesToSettleK1,
			"k 越大逼近自然长度所需帧越少, k1=" + framesToSettleK1 + " k4=" + framesToSettleK4);
	}

	// ─── 最小长度钳制: 强压缩 → 贴合且速度清零 ─────────────────────────
	private static void testMinLengthPinUnderStrongCompression()
	{
		var s = new Spring();
		s.setNormaLength(10.0f);
		s.setCurLength(0.51f);      // 略高于默认 minLength=0.5
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(-100.0f);
		s.setSpeed(-50.0f);         // 强负速度
		s.update(0.1f);
		// 一旦 <= 0.5 就被钳制到 0.5 且速度清零
		assertTrue(s.getLength() >= 0.5f - 0.001f,
			"长度不得低于最小长度, len=" + s.getLength());
		assertTrue(s.getSpeed().isEqual(0.0f, 0.0001f),
			"钳制后速度应清零, speed=" + s.getSpeed());
	}

	// ─── 最小长度跨多帧保持贴合 ─────────────────────────────────────────
	// 被钳制到 minLength 后, 即使弹力/外力仍试图压缩, 长度不再下降
	private static void testMinLengthChainsOverManyFrames()
	{
		var s = new Spring();
		s.setNormaLength(8.0f);
		s.setCurLength(0.5f);      // 已处于 minLength
		s.setSpringK(2.0f);
		s.setMass(1.0f);
		s.setForce(-50.0f);
		s.setSpeed(-30.0f);
		float min = s.getLength();
		for (int i = 0; i < 40; ++i)
		{
			s.update(0.1f);
			if (s.getLength() < min)
			{
				min = s.getLength();
			}
		}
		// 全程不低于 0.5 - 容差(FloatExtension.isZero 精度)
		assertTrue(min >= 0.5f - 0.001f,
			"多帧强压缩下长度不得低于最小长度, 最低=" + min);
		assertTrue(s.getLength().isEqual(0.5f, 0.001f),
			"持续压缩最终贴合在最小长度, len=" + s.getLength());
	}

	// ─── 符号翻转吸附的精确定时 ─────────────────────────────────────────
	// 压缩释放: 长度上升越过 normal 后, 弹力方向翻转为回拉, 下一次 update 由于
	// 加速度符号翻转(acc 与 mPreAcce 异号) → 速度清零吸附。验证吸附发生且
	// 吸附后速度从 0 重新起跳(朝反方向微小移动)。
	private static void testSignFlipSettleTiming()
	{
		var s = new Spring();
		s.setNormaLength(2.0f);
		s.setCurLength(0.6f);      // 压缩: normal=2, 长度较小
		s.setSpringK(1.0f);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		// 找到第一次速度变为 0 的帧(吸附点)
		int settleFrame = -1;
		float speedBeforeSettle = -1.0f;
		for (int i = 0; i < 60; ++i)
		{
			float spdBefore = s.getSpeed();
			s.update(0.2f);
			if (s.getSpeed().isEqual(0.0f, 0.0001f) && !spdBefore.isEqual(0.0f, 0.0001f))
			{
				settleFrame = i;
				speedBeforeSettle = spdBefore;
				break;
			}
		}
		// 吸附必然发生: 压缩释放越过 normal 后速度会被清零
		assertTrue(settleFrame >= 0, "符号翻转吸附应发生");
		// 吸附只能出现在长度已越过 natural(>2) 之后, 即吸附时刻长度应接近自然长度
		assertTrue(s.getLength() >= 2.0f - 0.3f,
			"吸附发生在越过自然长度后, len=" + s.getLength());
		// 吸附前速度非零(说明确实经历了有速度的逼近过程)
		assertTrue(!speedBeforeSettle.isEqual(0.0f, 0.0001f),
			"吸附前速度应非零");
	}

	// ─── 多参数联合扫掠稳定性 ─────────────────────────────────────────
	// mass/k/force/speed 全部变化, 长时间 update 不应崩溃、输出应有界。
	// 注: Spring 的最小长度 mMinLength 无 setter, 恒为 0.5, 故不作为扫掠维度。
	private static void testParameterSweepStability()
	{
		float[] masses = { 0.1f, 0.5f, 1.0f, 2.0f, 10.0f };
		float[] ks = { 0.2f, 0.5f, 1.0f, 3.0f, 8.0f };
		float[] forces = { -5.0f, -1.0f, 0.0f, 1.0f, 5.0f };
		float[] speeds = { -4.0f, -1.0f, 0.0f, 2.0f };
		float absMax = 0.0f;
		long combos = 0;
		foreach (float mass in masses)
		{
			foreach (float k in ks)
			{
				foreach (float force in forces)
				{
					foreach (float speed in speeds)
					{
						var s = new Spring();
						s.setNormaLength(6.0f);
						s.setCurLength(2.0f);
						s.setSpringK(k);
						s.setMass(mass);
						s.setForce(force);
						s.setSpeed(speed);
						for (int i = 0; i < 80; ++i)
						{
							s.update(0.1f);
							// 长度不出现负值(最小长度钳制), 并有一个有限上界
							assertTrue(s.getLength() >= -0.001f,
								"长度不得为负, mass=" + mass + " k=" + k);
							float l = s.getLength().abs();
							if (l > absMax)
							{
								absMax = l;
							}
						}
						++combos;
					}
				}
			}
		}
		// 有界性: 长度峰值都不超过一个保守大上界
		assertTrue(absMax < 1000.0f, "长时扫掠下长度应有限, 峰值=" + absMax);
		assertTrue(combos == 500, "组合总数应为 5*5*5*4=500, 实际=" + combos);
	}

	// ─── 工具: 测量前 N 帧的初始位移(mass 影响) ────────────────────────
	private static float measureInitialDisplacement(float mass, int frames)
	{
		var s = new Spring();
		s.setNormaLength(1.0f);
		s.setCurLength(3.0f);
		s.setSpringK(1.0f);
		s.setMass(mass);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		float start = s.getLength();
		for (int i = 0; i < frames; ++i)
		{
			s.update(0.1f);
		}
		// 位移(拉伸回缩的距离)
		return (start - s.getLength()).abs();
	}

	// ─── 工具: 测量前 N 帧的初始位移(k 影响) ──────────────────────────
	private static float measureInitialDisplacementK(float k, int frames)
	{
		var s = new Spring();
		s.setNormaLength(1.0f);
		s.setCurLength(3.0f);
		s.setSpringK(k);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		float start = s.getLength();
		for (int i = 0; i < frames; ++i)
		{
			s.update(0.1f);
		}
		return (start - s.getLength()).abs();
	}

	// ─── 工具: k固定时, 逼近到距 normal 0.5 以内所需的帧数 ──────────────
	private static int framesToApproachNormal(float k)
	{
		var s = new Spring();
		s.setNormaLength(1.0f);
		s.setCurLength(3.0f);
		s.setSpringK(k);
		s.setMass(1.0f);
		s.setForce(0.0f);
		s.setSpeed(0.0f);
		for (int i = 0; i < 1000; ++i)
		{
			s.update(0.1f);
			if (s.getLength() <= 1.0f + 0.5f)
			{
				return i + 1;
			}
		}
		return 1000;
	}
}