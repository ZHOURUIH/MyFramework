using UnityEngine;
using static TestAssert;

// Vector2UInt 结构体测试
public static class Vector2UIntTest
{
    public static void Run()
    {
        testConstructor();
        testEquals();
        testGetHashCode();
        testToVec2();
        testToVec2Int();
    }

    static void testConstructor()
    {
        Vector2UInt v = new Vector2UInt(5, 10);
        assertEqual(5u, v.x, "x=5");
        assertEqual(10u, v.y, "y=10");
    }

    static void testEquals()
    {
        Vector2UInt a = new Vector2UInt(1, 2);
        Vector2UInt b = new Vector2UInt(1, 2);
        Vector2UInt c = new Vector2UInt(3, 4);
        assertTrue(a.Equals(b), "equals same");
        assertFalse(a.Equals(c), "equals diff");
    }

    static void testGetHashCode()
    {
        Vector2UInt a = new Vector2UInt(1, 2);
        Vector2UInt b = new Vector2UInt(1, 2);
        assertEqual(a.GetHashCode(), b.GetHashCode(), "hash same for equal");
    }

    static void testToVec2()
    {
        Vector2UInt v = new Vector2UInt(3, 7);
        Vector2 result = v.toVec2();
        assertTrue(result.x.isEqual(3.0f), "toVec2 x=3");
        assertTrue(result.y.isEqual(7.0f), "toVec2 y=7");
    }

    static void testToVec2Int()
    {
        Vector2UInt v = new Vector2UInt(3, 7);
        Vector2Int result = v.toVec2Int();
        assertEqual(3, result.x, "toVec2Int x=3");
        assertEqual(7, result.y, "toVec2Int y=7");
    }
}
