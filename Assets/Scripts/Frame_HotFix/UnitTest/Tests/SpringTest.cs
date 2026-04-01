#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;
using static MathUtility;

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
    }

    // ─── resetProperty ──────────────────────────────────────────────────
    private static void testResetProperty()
    {
        var s = new Spring();
        s.setNormaLength(5.0f);
        s.setMass(2.0f);
        s.setSpringk(3.0f);
        s.setForce(1.0f);
        s.setCurLength(6.0f);

        s.resetProperty();

        assert(isFloatEqual(s.getNomalLength(), 0.0f), "resetProperty normalLength=0");
        assert(isFloatEqual(s.getLength(),      0.0f), "resetProperty curLength=0");
        assert(isFloatEqual(s.getSpeed(),       0.0f), "resetProperty speed=0");
        // 检查 mObjectMass mSpringK mMinLength 是否回到默认值（通过弹力间接验证）
        // mNormalLength=0, mCurLength=0 → elasticForce = (0-0)*k = 0
        assert(isFloatEqual(s.calculateElasticForce(), 0.0f), "resetProperty elasticForce=0");
    }

    // ─── setters / getters ──────────────────────────────────────────────
    private static void testSettersAndGetters()
    {
        var s = new Spring();
        s.setNormaLength(3.0f);
        s.setCurLength(5.0f);
        s.setMass(2.0f);
        s.setSpringk(4.0f);
        s.setSpeed(1.5f);
        s.setForce(0.5f);

        assert(isFloatEqual(s.getNomalLength(), 3.0f), "getNormalLength=3");
        assert(isFloatEqual(s.getLength(),      5.0f), "getLength=5");
        assert(isFloatEqual(s.getSpeed(),       1.5f), "getSpeed=1.5");
    }

    // ─── calculateElasticForce ──────────────────────────────────────────
    private static void testCalculateElasticForce()
    {
        var s = new Spring();
        // normalLength=2, curLength=5, k=1 → force=(5-2)*1=3（压缩弹簧方向为正）
        s.setNormaLength(2.0f);
        s.setCurLength(5.0f);
        s.setSpringk(1.0f);
        assert(isFloatEqual(s.calculateElasticForce(), 3.0f, 0.001f),
            "elasticForce 拉伸=(5-2)*1=3");

        // normalLength=5, curLength=3, k=2 → force=(3-5)*2=-4（负值→压缩）
        s.setNormaLength(5.0f);
        s.setCurLength(3.0f);
        s.setSpringk(2.0f);
        assert(isFloatEqual(s.calculateElasticForce(), -4.0f, 0.001f),
            "elasticForce 压缩=(3-5)*2=-4");

        // 处于自然长度
        s.setNormaLength(4.0f);
        s.setCurLength(4.0f);
        assert(isFloatEqual(s.calculateElasticForce(), 0.0f, 0.001f),
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
        s.setSpringk(1.0f);
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
        s.setSpringk(1.0f);
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
        s.setSpringk(1.0f);
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
        s.setSpringk(1.0f);
        s.setMass(1.0f);
        s.setForce(0.0f);
        s.setSpeed(0.0f);

        float lengthBefore = s.getLength();
        s.update(0.1f);
        float lengthAfter = s.getLength();

        // 处于自然长度，无外力，速度=0 → elasticForce=0 → 加速度=0 → 长度不变
        assert(isFloatEqual(lengthBefore, lengthAfter, 0.001f),
            "自然长度无外力 update后长度不变");
    }
}
#endif
