using static TestAssert;

// Vector4Int 结构体测试
public static class Vector4IntTest
{
    public static void Run()
    {
        testConstructor();
        testEquals();
        testGetHashCode();
        testZero();
    }

    static void testConstructor()
    {
        Vector4Int v = new Vector4Int(1, 2, 3, 4);
        assertEqual(1, v.x, "x=1");
        assertEqual(2, v.y, "y=2");
        assertEqual(3, v.z, "z=3");
        assertEqual(4, v.w, "w=4");
    }

    static void testEquals()
    {
        Vector4Int a = new Vector4Int(1, 2, 3, 4);
        Vector4Int b = new Vector4Int(1, 2, 3, 4);
        Vector4Int c = new Vector4Int(5, 6, 7, 8);
        assertTrue(a.Equals(b), "equals same");
        assertFalse(a.Equals(c), "equals diff");
    }

    static void testGetHashCode()
    {
        Vector4Int a = new Vector4Int(1, 2, 3, 4);
        Vector4Int b = new Vector4Int(1, 2, 3, 4);
        assertEqual(a.GetHashCode(), b.GetHashCode(), "hash same for equal");
    }

    static void testZero()
    {
        assertEqual(0, Vector4Int.zero.x, "zero x=0");
        assertEqual(0, Vector4Int.zero.y, "zero y=0");
        assertEqual(0, Vector4Int.zero.z, "zero z=0");
        assertEqual(0, Vector4Int.zero.w, "zero w=0");
    }
}
