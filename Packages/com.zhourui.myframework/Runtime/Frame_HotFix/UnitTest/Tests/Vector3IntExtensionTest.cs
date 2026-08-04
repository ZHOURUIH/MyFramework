using UnityEngine;
using static TestAssert;

// Vector3IntExtension 纯数学扩展方法测试
public static class Vector3IntExtensionTest
{
    public static void Run()
    {
        testAbs();
        testClampMin();
        testClampMax();
    }

    // ---- abs ----
    static void testAbs()
    {
        Vector3Int v = new Vector3Int(-3, 4, -5).abs();
        assertEqual(3, v.x, "abs x -3->3");
        assertEqual(4, v.y, "abs y 4->4");
        assertEqual(5, v.z, "abs z -5->5");
    }

    // ---- clampMin ----
    static void testClampMin()
    {
        Vector3Int v = new Vector3Int(-1, 5, -3).clampMin(0);
        assertEqual(0, v.x, "clampMin x -1->0");
        assertEqual(5, v.y, "clampMin y 5 stays");
        assertEqual(0, v.z, "clampMin z -3->0");

        // default min=0
        Vector3Int v2 = new Vector3Int(-1, 5, -3).clampMin();
        assertEqual(0, v2.x, "clampMin default x -1->0");
        assertEqual(5, v2.y, "clampMin default y stays");
        assertEqual(0, v2.z, "clampMin default z -3->0");
    }

    // ---- clampMax ----
    static void testClampMax()
    {
        Vector3Int v = new Vector3Int(7, 3, 9).clampMax(5);
        assertEqual(5, v.x, "clampMax x 7->5");
        assertEqual(3, v.y, "clampMax y 3 stays");
        assertEqual(5, v.z, "clampMax z 9->5");

        // default max=0
        Vector3Int v2 = new Vector3Int(7, -3, 9).clampMax();
        assertEqual(0, v2.x, "clampMax default x 7->0");
        assertEqual(-3, v2.y, "clampMax default y stays");
        assertEqual(0, v2.z, "clampMax default z 9->0");
    }
}
