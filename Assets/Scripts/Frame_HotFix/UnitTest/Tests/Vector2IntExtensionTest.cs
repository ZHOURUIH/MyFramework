using UnityEngine;
using static TestAssert;

public static class Vector2IntExtensionTest
{
    public static void Run()
    {
        testToVec3();
    }

    static void testToVec3()
    {
        Vector2Int v = new(3, 4);
        Vector3 v3 = v.toVec3();
        assertEqual(new Vector3(3, 4), v3);
    }
}