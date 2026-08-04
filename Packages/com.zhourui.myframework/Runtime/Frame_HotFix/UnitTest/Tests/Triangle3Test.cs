using UnityEngine;
using static TestAssert;

// Triangle3 结构体测试
public static class Triangle3Test
{
    public static void Run()
    {
        testConstructor();
    }

    static void testConstructor()
    {
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(1, 0, 0);
        Vector3 p2 = new Vector3(0, 1, 0);
        Triangle3 tri = new Triangle3(p0, p1, p2);
        assertTrue(tri.mPoint0.isEqual(p0), "point0");
        assertTrue(tri.mPoint1.isEqual(p1), "point1");
        assertTrue(tri.mPoint2.isEqual(p2), "point2");
    }
}
