using UnityEngine;
using static TestAssert;

// Triangle2 结构体测试
public static class Triangle2Test
{
    public static void Run()
    {
        testConstructor();
        testToTriangle3();
    }

    static void testConstructor()
    {
        Vector2 p0 = new Vector2(0, 0);
        Vector2 p1 = new Vector2(1, 0);
        Vector2 p2 = new Vector2(0, 1);
        Triangle2 tri = new Triangle2(p0, p1, p2);
        assertTrue(tri.mPoint0.isEqual(p0), "point0");
        assertTrue(tri.mPoint1.isEqual(p1), "point1");
        assertTrue(tri.mPoint2.isEqual(p2), "point2");
    }

    static void testToTriangle3()
    {
        Vector2 p0 = new Vector2(1, 2);
        Vector2 p1 = new Vector2(3, 4);
        Vector2 p2 = new Vector2(5, 6);
        Triangle2 tri2 = new Triangle2(p0, p1, p2);
        Triangle3 tri3 = tri2.toTriangle3();
        assertTrue(tri3.mPoint0.isEqual(new Vector3(1, 2, 0)), "toTriangle3 point0");
        assertTrue(tri3.mPoint1.isEqual(new Vector3(3, 4, 0)), "toTriangle3 point1");
        assertTrue(tri3.mPoint2.isEqual(new Vector3(5, 6, 0)), "toTriangle3 point2");
    }
}
