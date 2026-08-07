using UnityEngine;
using static TestAssert;

// Vector2IntExtension 纯数学扩展方法测试
public static class Vector2IntExtensionTest
{
    public static void Run()
    {
        testToVec3();
        testAbs();
        testClampMin();
        testClampMax();
        testIntPosToIndex();
        testGetAngle();
        testGetAngleDegree();
    }

    static void testToVec3()
    {
        Vector2Int v = new(3, 4);
        Vector3 v3 = v.toVec3();
        assertEqual(new Vector3(3, 4), v3);
    }

    // ---- abs ----
    static void testAbs()
    {
        assertEqual(new Vector2Int(3, 4), new Vector2Int(-3, 4).abs(), "abs(-3,4)");
        assertEqual(new Vector2Int(3, 4), new Vector2Int(3, -4).abs(), "abs(3,-4)");
        assertEqual(new Vector2Int(0, 5), new Vector2Int(0, -5).abs(), "abs(0,-5)");
        assertEqual(new Vector2Int(7, 0), new Vector2Int(-7, 0).abs(), "abs(-7,0)");
        assertEqual(new Vector2Int(0, 0), new Vector2Int(0, 0).abs(), "abs(0,0)");
    }

    // ---- clampMin ----
    static void testClampMin()
    {
        // 默认 min=0
        assertEqual(new Vector2Int(3, 4), new Vector2Int(3, 4).clampMin(), "clampMin 默认 3,4");
        assertEqual(new Vector2Int(3, 0), new Vector2Int(3, -2).clampMin(), "clampMin 默认 -2->0");
        assertEqual(new Vector2Int(5, 6), new Vector2Int(5, 6).clampMin(5), "clampMin 5 边界");
        assertEqual(new Vector2Int(5, 5), new Vector2Int(-100, 5).clampMin(5), "clampMin 5 负值");
    }

    // ---- clampMax ----
    static void testClampMax()
    {
        // clampMax(int max=0): 各分量 > max 时钳到 max, 否则保持
        assertEqual(new Vector2Int(3, 4), new Vector2Int(3, 4).clampMax(10), "clampMax 未超");
        assertEqual(new Vector2Int(10, 10), new Vector2Int(100, 10).clampMax(10), "clampMax 超上限");
        assertEqual(new Vector2Int(-5, 10), new Vector2Int(-5, 99).clampMax(10), "clampMax 仅 y 超");
        assertEqual(new Vector2Int(-3, 5), new Vector2Int(-3, 5).clampMax(5), "clampMax 负数保持");
        // 默认 max=0: 正数全部钳到0
        assertEqual(new Vector2Int(0, 0), new Vector2Int(3, 5).clampMax(), "clampMax 默认正数->0");
        assertEqual(new Vector2Int(-2, 0), new Vector2Int(-2, 3).clampMax(), "clampMax 默认 x 负保持");
    }

    // ---- intPosToIndex: pos.x + pos.y * width ----
    static void testIntPosToIndex()
    {
        assertEqual(0, new Vector2Int(0, 0).intPosToIndex(4), "(0,0) idx");
        assertEqual(1, new Vector2Int(1, 0).intPosToIndex(4), "(1,0) idx");
        assertEqual(4, new Vector2Int(0, 1).intPosToIndex(4), "(0,1) idx");
        assertEqual(13, new Vector2Int(1, 3).intPosToIndex(4), "(1,3) idx = 1+12");
        assertEqual(35, new Vector2Int(5, 6).intPosToIndex(5), "(5,6) idx = 5+30");
        assertEqual(-3, new Vector2Int(-3, 0).intPosToIndex(5), "负x idx");
    }

    // ---- getAngle: 弧度制方位角 ----
    static void testGetAngle()
    {
        // 与 Vector3Extension.getAngle 语义一致: (x,0,z)
        // 纯数值关系断言(避免浮点敏感): 四个方向应产生不同角度
        float east = new Vector2Int(1, 0).getAngle();
        float west = new Vector2Int(-1, 0).getAngle();
        float north = new Vector2Int(0, 1).getAngle();
        float south = new Vector2Int(0, -1).getAngle();
        assertTrue(east != west, "东西角度不同");
        assertTrue(north != south, "南北角度不同");
        assertTrue(east != north, "东西南北两两不同");
    }

    // ---- getAngle: 角度制可换算回弧度 ----
    static void testGetAngleDegree()
    {
        // 30° 方向 (0.866, 0.5) 只需验证 DEGREE 与 RADIAN 圆周关系
        float rad = new Vector2Int(1, 1).getAngle(ANGLE.RADIAN);
        float deg = new Vector2Int(1, 1).getAngle(ANGLE.DEGREE);
        // deg ≈ rad * 180/π
        assertEqual(rad * Mathf.Rad2Deg, deg, 0.5f, "DEGREE 与 RADIAN 换算一致");
    }
}
