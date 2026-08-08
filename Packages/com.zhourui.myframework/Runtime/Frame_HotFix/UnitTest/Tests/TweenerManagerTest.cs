using static TestAssert;
using static FrameUtility;

// TweenerManager 入口层测试
// 覆盖核心"正常使用入口"方法:
//   createTweenerFloat: 创建并登记到 mTweenerList
//   destroyTweener:     移除并还池(null 守卫)
//   update:             遍历所有 tweener 的 update(空列表安全)
// 不测: 基类 FrameSystem 的框架生命周期(由框架测试负责), 只测本类自有逻辑
public static class TweenerManagerTest
{
    public static void Run()
    {
        testCreateTweenerFloatNotNull();
        testCreateTweenerFloatRegistered();
        testCreateTwoTweenersDistinctID();
        testDestroyTweenerRemovesFromList();
        testDestroyTweenerNull();
        testUpdateEmptyList();
        testUpdateDrivesTweenerUpdate();
        // ── 语义深测(纯逻辑) ──
        testTweenUtilityEvaluate();
        testTweenUtilityEvaluateUnclamped();
        testTweenUtilityEvaluateCurve();
        testMyTweenerBaseIsDoing();
        testMyTweenerFloatIsDoingNotPlaying();
    }

    // ═══════════════════════════════════════════════════════════════════
    // createTweenerFloat
    // ═══════════════════════════════════════════════════════════════════

    private static void testCreateTweenerFloatNotNull()
    {
        var mgr = new TestTweenerManager();
        try
        {
            MyTweenerFloat tweener = mgr.createTweenerFloat();
            assertNotNull(tweener, "createTweenerFloat 返回非 null");
            assertTrue(tweener is MyTweenerFloat, "返回类型为 MyTweenerFloat");
            mgr.destroyTweener(tweener);
        }
        finally
        {
            mgr.destroy();
        }
    }

    private static void testCreateTweenerFloatRegistered()
    {
        var mgr = new TestTweenerManager();
        try
        {
            MyTweenerFloat tweener = mgr.createTweenerFloat();
            long id = tweener.getAssignID();
            // 登记到 mTweenerList 后, 通过 id 能取到同一个对象
            MyTweener fetched = null;
            foreach (var item in mgr.getTweenerList())
            {
                if (item.Key == id)
                {
                    fetched = item.Value;
                    break;
                }
            }
            assertNotNull(fetched, "createTweenerFloat 后对象应登记进 mTweenerList");
            assertTrue(fetched == tweener, "登记的实例与返回的实例一致");
            mgr.destroyTweener(tweener);
        }
        finally
        {
            mgr.destroy();
        }
    }

    private static void testCreateTwoTweenersDistinctID()
    {
        var mgr = new TestTweenerManager();
        try
        {
            MyTweenerFloat a = mgr.createTweenerFloat();
            MyTweenerFloat b = mgr.createTweenerFloat();
            assertTrue(a.getAssignID() != b.getAssignID(), "两次创建应分配不同 assignID");
            int count = 0;
            foreach (var item in mgr.getTweenerList())
            {
                count++;
            }
            assertEqual(2, count, "mTweenerList 应登记 2 个 tweener");
            mgr.destroyTweener(a);
            mgr.destroyTweener(b);
        }
        finally
        {
            mgr.destroy();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // destroyTweener
    // ═══════════════════════════════════════════════════════════════════

    private static void testDestroyTweenerRemovesFromList()
    {
        var mgr = new TestTweenerManager();
        try
        {
            MyTweenerFloat tweener = mgr.createTweenerFloat();
            long id = tweener.getAssignID();
            mgr.destroyTweener(tweener);
            // 销毁后不应再能取到
            bool found = false;
            foreach (var item in mgr.getTweenerList())
            {
                if (item.Key == id)
                {
                    found = true;
                    break;
                }
            }
            assertFalse(found, "destroyTweener 后对象应从 mTweenerList 移除");
        }
        finally
        {
            mgr.destroy();
        }
    }

    private static void testDestroyTweenerNull()
    {
        var mgr = new TestTweenerManager();
        try
        {
            // null 守卫: 不抛异常
            mgr.destroyTweener(null);
        }
        finally
        {
            mgr.destroy();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // update
    // ═══════════════════════════════════════════════════════════════════

    private static void testUpdateEmptyList()
    {
        var mgr = new TestTweenerManager();
        try
        {
            // 空列表 update 不抛异常
            mgr.update(0.016f);
        }
        finally
        {
            mgr.destroy();
        }
    }

    private static void testUpdateDrivesTweenerUpdate()
    {
        var mgr = new TestTweenerManager();
        try
        {
            CountingTweener tweener = new CountingTweener();
            // 手动登记(绕过 createTweenerFloat 以使用自定义子类, 验证 update 遍历逻辑)
            mgr.getTweenerList().add(tweener.getAssignID(), tweener);
            assertEqual(0, tweener.updateCount, "update 前计数为0");
            mgr.update(0.016f);
            assertEqual(1, tweener.updateCount, "update 应驱动 tweener.update 一次");
            mgr.update(0.016f);
            assertEqual(2, tweener.updateCount, "第二次 update 再次驱动");
            mgr.getTweenerList().remove(tweener.getAssignID());
        }
        finally
        {
            mgr.destroy();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // TweenUtility 纯逻辑
    // ═══════════════════════════════════════════════════════════════════

    private static void testTweenUtilityEvaluate()
    {
        // Evaluate = Vector3.LerpUnclamped(start, target, value)
        var result = TweenUtility.Evaluate(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Vector3(10, 20, 30), 0.5f);
        assertEqual(5.0f,  result.x, 0.0001f, "value=0.5 → x=5");
        assertEqual(10.0f, result.y, 0.0001f, "value=0.5 → y=10");
        assertEqual(15.0f, result.z, 0.0001f, "value=0.5 → z=15");

        // value=0 → start; value=1 → target
        var atStart = TweenUtility.Evaluate(new UnityEngine.Vector3(1, 2, 3), new UnityEngine.Vector3(7, 8, 9), 0f);
        assertEqual(1.0f, atStart.x, 0.0001f, "value=0 → x=start");
        assertEqual(2.0f, atStart.y, 0.0001f, "value=0 → y=start");
        assertEqual(3.0f, atStart.z, 0.0001f, "value=0 → z=start");
        var atEnd = TweenUtility.Evaluate(new UnityEngine.Vector3(1, 2, 3), new UnityEngine.Vector3(7, 8, 9), 1f);
        assertEqual(7.0f, atEnd.x, 0.0001f, "value=1 → x=target");
        assertEqual(8.0f, atEnd.y, 0.0001f, "value=1 → y=target");
        assertEqual(9.0f, atEnd.z, 0.0001f, "value=1 → z=target");
    }

    private static void testTweenUtilityEvaluateUnclamped()
    {
        // LerpUnclamped: value>1 会超出 target 范围(不做钳制)
        var result = TweenUtility.Evaluate(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Vector3(10, 0, 0), 2f);
        assertEqual(20.0f, result.x, 0.0001f, "value=2 未钳制 → x=20");
        var below = TweenUtility.Evaluate(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Vector3(10, 0, 0), -1f);
        assertEqual(-10.0f, below.x, 0.0001f, "value=-1 未钳制 → x=-10");
    }

    private static void testTweenUtilityEvaluateCurve()
    {
        // 自定义线性曲线 evaluate(time)=time
        var curve = new LinearTestCurve();
        assertEqual(0.0f,  TweenUtility.EvaluateCurve(curve, 0.0f),  0.0001f, "evaluate(0)=0");
        assertEqual(0.5f,  TweenUtility.EvaluateCurve(curve, 0.5f),  0.0001f, "evaluate(0.5)=0.5");
        assertEqual(1.0f,  TweenUtility.EvaluateCurve(curve, 1.0f),  0.0001f, "evaluate(1)=1");
        assertEqual(0.25f, TweenUtility.EvaluateCurve(curve, 0.25f), 0.0001f, "evaluate(0.25)=0.25");
    }

    // ═══════════════════════════════════════════════════════════════════
    // MyTweener.isDoing 语义
    // ═══════════════════════════════════════════════════════════════════

    private static void testMyTweenerBaseIsDoing()
    {
        // 基类默认返回 false
        var tweener = new MyTweener();
        assertFalse(tweener.isDoing(), "MyTweener 基类 isDoing() 默认 false");
    }

    private static void testMyTweenerFloatIsDoingNotPlaying()
    {
        var mgr = new TestTweenerManager();
        try
        {
            // createTweenerFloat 内部已 init(), mComponentFloat 初始化后状态为 STOP
            MyTweenerFloat tweener = mgr.createTweenerFloat();
            assertFalse(tweener.isDoing(), "刚创建未播放的 MyTweenerFloat isDoing()=false (state=STOP)");
            mgr.destroyTweener(tweener);
        }
        finally
        {
            mgr.destroy();
        }
    }
}

// 测试子类, 暴露 protected 字段 mTweenerList 供外部断言访问
// (C# 中 protected 成员只能从继承类内部访问, 故通过公有 getter 暴露)
public class TestTweenerManager : TweenerManager
{
    public SafeDictionary<long, MyTweener> getTweenerList()
    {
        return mTweenerList;
    }
}

// 用于验证 update 遍历是否调用 tweener.update 的计数子类
public class CountingTweener : MyTweenerFloat
{
    public int updateCount;
    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        updateCount++;
    }
    public override void resetProperty()
    {
        base.resetProperty();
        updateCount = 0;
    }
}
