#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;
using UnityEngine;
using static MathUtility;

// 几何/向量结构体测试
// 覆盖：Complex / Point / Vector2IntMy / Vector2Short / Vector2UInt / Vector2UShort / Vector4Int /
//        Line2 / Line3 / Triangle2 / Triangle3 / Circle3 / Rect3
public static class StructTest
{
    public static void Run()
    {
        testComplex();
        testPoint();
        testVector2IntMy();
        testVector2Short();
        testVector2UInt();
        testVector2UShort();
        testVector4Int();
        testLine2();
        testLine3();
        testTriangle2();
        testTriangle3();
        testCircle3();
        testRect3();
    }

    // ─── Complex ────────────────────────────────────────────────────────────
    private static void testComplex()
    {
        var c1 = new Complex(1.0f, 2.0f);
        var c2 = new Complex(3.0f, 4.0f);

        // 字段
        assert(isFloatEqual(c1.mReal, 1.0f), "Complex mReal");
        assert(isFloatEqual(c1.mImg,  2.0f), "Complex mImg");

        // Equals
        var c1Copy = new Complex(1.0f, 2.0f);
        assert(c1.Equals(c1Copy),  "Complex Equals 相等");
        assert(!c1.Equals(c2),     "Complex Equals 不等");

        // 加法
        var add = c1 + c2;
        assert(isFloatEqual(add.mReal, 4.0f), "Complex + real");
        assert(isFloatEqual(add.mImg,  6.0f), "Complex + img");

        // 减法
        var sub = c2 - c1;
        assert(isFloatEqual(sub.mReal, 2.0f), "Complex - real");
        assert(isFloatEqual(sub.mImg,  2.0f), "Complex - img");
    }

    // ─── Point ──────────────────────────────────────────────────────────────
    private static void testPoint()
    {
        var p = new Point(3, 4);
        assertEqual(3, p.x, "Point x");
        assertEqual(4, p.y, "Point y");

        // toIndex / fromIndex 互逆
        int width = 10;
        int idx = p.toIndex(width);
        assertEqual(43, idx, "Point toIndex 3+4*10=43");
        var p2 = Point.fromIndex(idx, width);
        assertEqual(p.x, p2.x, "Point fromIndex x");
        assertEqual(p.y, p2.y, "Point fromIndex y");

        // 从 Vector2Int 构造
        var pv = new Point(new Vector2Int(5, 6));
        assertEqual(5, pv.x, "Point fromVec2Int x");
        assertEqual(6, pv.y, "Point fromVec2Int y");

        // Equals
        var pa = new Point(3, 4);
        var pb = new Point(3, 5);
        assert(p.Equals(pa),  "Point Equals 相等");
        assert(!p.Equals(pb), "Point Equals 不等");
    }

    // ─── Vector2IntMy ───────────────────────────────────────────────────────
    private static void testVector2IntMy()
    {
        var v = new Vector2IntMy(7, 8);
        assertEqual(7, v.x, "Vector2IntMy x");
        assertEqual(8, v.y, "Vector2IntMy y");

        // toVec2
        Vector2 v2 = v.toVec2();
        assert(isFloatEqual(v2.x, 7f), "Vector2IntMy toVec2 x");
        assert(isFloatEqual(v2.y, 8f), "Vector2IntMy toVec2 y");

        // toVec2Int
        Vector2Int vi = v.toVec2Int();
        assertEqual(7, vi.x, "Vector2IntMy toVec2Int x");
        assertEqual(8, vi.y, "Vector2IntMy toVec2Int y");

        // Equals
        var va = new Vector2IntMy(7, 8);
        var vb = new Vector2IntMy(7, 9);
        assert(v.Equals(va),  "Vector2IntMy Equals 相等");
        assert(!v.Equals(vb), "Vector2IntMy Equals 不等");

        // GetHashCode 相同值相同 hash
        assertEqual(v.GetHashCode(), va.GetHashCode(), "Vector2IntMy hash 一致");
    }

    // ─── Vector2Short ───────────────────────────────────────────────────────
    private static void testVector2Short()
    {
        var v = new Vector2Short(10, 20);
        assertEqual((short)10, v.x, "Vector2Short x");
        assertEqual((short)20, v.y, "Vector2Short y");

        Vector2 v2 = v.toVec2();
        assert(isFloatEqual(v2.x, 10f), "Vector2Short toVec2 x");
        assert(isFloatEqual(v2.y, 20f), "Vector2Short toVec2 y");

        Vector2Int vi = v.toVec2Int();
        assertEqual(10, vi.x, "Vector2Short toVec2Int x");
        assertEqual(20, vi.y, "Vector2Short toVec2Int y");

        var va = new Vector2Short(10, 20);
        var vb = new Vector2Short(10, 21);
        assert(v.Equals(va),  "Vector2Short Equals 相等");
        assert(!v.Equals(vb), "Vector2Short Equals 不等");
        assertEqual(v.GetHashCode(), va.GetHashCode(), "Vector2Short hash 一致");
    }

    // ─── Vector2UInt ────────────────────────────────────────────────────────
    private static void testVector2UInt()
    {
        var v = new Vector2UInt(100u, 200u);
        assertEqual(100u, v.x, "Vector2UInt x");
        assertEqual(200u, v.y, "Vector2UInt y");

        Vector2 v2 = v.toVec2();
        assert(isFloatEqual(v2.x, 100f), "Vector2UInt toVec2 x");
        assert(isFloatEqual(v2.y, 200f), "Vector2UInt toVec2 y");

        var va = new Vector2UInt(100u, 200u);
        var vb = new Vector2UInt(100u, 201u);
        assert(v.Equals(va),  "Vector2UInt Equals 相等");
        assert(!v.Equals(vb), "Vector2UInt Equals 不等");
        assertEqual(v.GetHashCode(), va.GetHashCode(), "Vector2UInt hash 一致");
    }

    // ─── Vector2UShort ──────────────────────────────────────────────────────
    private static void testVector2UShort()
    {
        var v = new Vector2UShort(5, 15);
        assertEqual((ushort)5,  v.x, "Vector2UShort x");
        assertEqual((ushort)15, v.y, "Vector2UShort y");

        Vector2 v2 = v.toVec2();
        assert(isFloatEqual(v2.x, 5f),  "Vector2UShort toVec2 x");
        assert(isFloatEqual(v2.y, 15f), "Vector2UShort toVec2 y");

        var va = new Vector2UShort(5, 15);
        var vb = new Vector2UShort(5, 16);
        assert(v.Equals(va),  "Vector2UShort Equals 相等");
        assert(!v.Equals(vb), "Vector2UShort Equals 不等");
        assertEqual(v.GetHashCode(), va.GetHashCode(), "Vector2UShort hash 一致");
    }

    // ─── Vector4Int ─────────────────────────────────────────────────────────
    private static void testVector4Int()
    {
        var v = new Vector4Int(1, 2, 3, 4);
        assertEqual(1, v.x, "Vector4Int x");
        assertEqual(2, v.y, "Vector4Int y");
        assertEqual(3, v.z, "Vector4Int z");
        assertEqual(4, v.w, "Vector4Int w");

        var va = new Vector4Int(1, 2, 3, 4);
        var vb = new Vector4Int(1, 2, 3, 5);
        assert(v.Equals(va),  "Vector4Int Equals 相等");
        assert(!v.Equals(vb), "Vector4Int Equals 不等");
        assertEqual(v.GetHashCode(), va.GetHashCode(), "Vector4Int hash 一致");

        // zero 静态字段
        assertEqual(0, Vector4Int.zero.x, "Vector4Int.zero x=0");
        assertEqual(0, Vector4Int.zero.w, "Vector4Int.zero w=0");
    }

    // ─── Line2 ──────────────────────────────────────────────────────────────
    private static void testLine2()
    {
        // 斜率存在的线段: (0,0)→(2,4)  k=2 b=0
        var line = new Line2(new Vector2(0, 0), new Vector2(2, 4));
        assert(line.mHasK,                    "Line2 hasK");
        assert(isFloatEqual(line.mK, 2.0f),   "Line2 k=2");
        assert(isFloatEqual(line.mB, 0.0f),   "Line2 b=0");

        // getPointYOnLine
        assert(line.getPointYOnLine(1.0f, out float y), "Line2 getY 有斜率");
        assert(isFloatEqual(y, 2.0f),                    "Line2 getY x=1→y=2");

        // getPointXOnLine
        assert(line.getPointXOnLine(4.0f, out float x), "Line2 getX 有斜率");
        assert(isFloatEqual(x, 2.0f),                    "Line2 getX y=4→x=2");

        // getDirection / length
        Vector2 dir = line.getDirection();
        assert(isFloatEqual(dir.x, 2.0f) && isFloatEqual(dir.y, 4.0f), "Line2 direction");
        assert(line.length() > 0f, "Line2 length>0");

        // toLine3 
        Line3 l3 = line.toLine3();
        assert(isFloatEqual(l3.mStart.x, 0f) && isFloatEqual(l3.mEnd.x, 2f), "Line2 toLine3");

        // 垂直线段（斜率不存在）: (3,0)→(3,5)
        var vLine = new Line2(new Vector2(3, 0), new Vector2(3, 5));
        assert(!vLine.mHasK, "Line2 垂直线段 hasK=false");
        assert(!vLine.getPointYOnLine(3f, out float _), "Line2 垂直线段 getY=false");
        assert(vLine.getPointXOnLine(2.5f, out float vx), "Line2 垂直线段 getX=true");
        assert(isFloatEqual(vx, 3.0f), "Line2 垂直线段 getX=3");

        // 水平线段（斜率=0）: (0,5)→(4,5) k=0 → getX应返回false
        var hLine = new Line2(new Vector2(0, 5), new Vector2(4, 5));
        assert(hLine.mHasK,                      "Line2 水平线段 hasK=true");
        assert(isFloatEqual(hLine.mK, 0.0f),     "Line2 水平线段 k=0");
        assert(!hLine.getPointXOnLine(5f, out float _), "Line2 水平线段 getX=false(k=0)");
    }

    // ─── Line3 ──────────────────────────────────────────────────────────────
    private static void testLine3()
    {
        var l3 = new Line3(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
        assert(isFloatEqual(l3.mStart.x, 1f), "Line3 start.x");
        assert(isFloatEqual(l3.mEnd.z,   6f), "Line3 end.z");

        // toLine2IgnoreY → 使用 x 和 z
        Line2 l2y = l3.toLine2IgnoreY();
        assert(isFloatEqual(l2y.mStart.x, 1f), "Line3 toLine2IgnoreY start.x");
        assert(isFloatEqual(l2y.mStart.y, 3f), "Line3 toLine2IgnoreY start.y(z)");

        // toLine2IgnoreX → 使用 z 和 y
        Line2 l2x = l3.toLine2IgnoreX();
        assert(isFloatEqual(l2x.mStart.x, 3f), "Line3 toLine2IgnoreX start.x(z)");
        assert(isFloatEqual(l2x.mStart.y, 2f), "Line3 toLine2IgnoreX start.y");
    }

    // ─── Triangle2 ──────────────────────────────────────────────────────────
    private static void testTriangle2()
    {
        var t = new Triangle2(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1));
        assert(isFloatEqual(t.mPoint0.x, 0f), "Triangle2 p0.x");
        assert(isFloatEqual(t.mPoint1.x, 1f), "Triangle2 p1.x");
        assert(isFloatEqual(t.mPoint2.y, 1f), "Triangle2 p2.y");

        // toTriangle3
        Triangle3 t3 = t.toTriangle3();
        assert(isFloatEqual(t3.mPoint0.x, 0f), "Triangle2 toTriangle3 p0.x");
        assert(isFloatEqual(t3.mPoint1.x, 1f), "Triangle2 toTriangle3 p1.x");
        assert(isFloatEqual(t3.mPoint2.y, 1f), "Triangle2 toTriangle3 p2.y");
        // Z应为0
        assert(isFloatEqual(t3.mPoint0.z, 0f), "Triangle2 toTriangle3 z=0");
    }

    // ─── Triangle3 ──────────────────────────────────────────────────────────
    private static void testTriangle3()
    {
        var t = new Triangle3(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));
        assert(isFloatEqual(t.mPoint0.x, 1f), "Triangle3 p0.x");
        assert(isFloatEqual(t.mPoint1.y, 1f), "Triangle3 p1.y");
        assert(isFloatEqual(t.mPoint2.z, 1f), "Triangle3 p2.z");
    }

    // ─── Circle3 ────────────────────────────────────────────────────────────
    private static void testCircle3()
    {
        var c = new Circle3(new Vector3(1, 2, 3), 5.0f);
        assert(isFloatEqual(c.mCenter.x, 1f), "Circle3 center.x");
        assert(isFloatEqual(c.mCenter.y, 2f), "Circle3 center.y");
        assert(isFloatEqual(c.mCenter.z, 3f), "Circle3 center.z");
        assert(isFloatEqual(c.mRadius,   5f), "Circle3 radius");
    }

    // ─── Rect3 ──────────────────────────────────────────────────────────────
    private static void testRect3()
    {
        var r = new Rect3(
            new Vector3(0, 0, 0),
            Vector3.up,
            Vector3.forward,
            4.0f,
            2.0f
        );
        assert(isFloatEqual(r.mWidth,  4.0f), "Rect3 width");
        assert(isFloatEqual(r.mHeight, 2.0f), "Rect3 height");

        // toRect: x = center.x - width/2 = -2, y = center.z - height/2 = -1
        Rect rect = r.toRect();
        assert(isFloatEqual(rect.x,      -2.0f), "Rect3 toRect x");
        assert(isFloatEqual(rect.y,      -1.0f), "Rect3 toRect y");
        assert(isFloatEqual(rect.width,   4.0f), "Rect3 toRect width");
        assert(isFloatEqual(rect.height,  2.0f), "Rect3 toRect height");
    }
}
#endif
