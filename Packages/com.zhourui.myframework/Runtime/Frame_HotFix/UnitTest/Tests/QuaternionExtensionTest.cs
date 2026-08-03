using UnityEngine;
using static TestAssert;

// QuaternionExtension 中 MathExtensionTest 未覆盖的方法测试
// 已覆盖: getQuaternionYaw, getQuaternionPitch, getQuaternionRoll
public static class QuaternionExtensionTest
{
    public static void Run()
    {
        testIsEqual();
    }

    // ---- isEqual ----
    static void testIsEqual()
    {
        Quaternion q1 = Quaternion.identity;
        Quaternion q2 = Quaternion.identity;
        assertTrue(q1.isEqual(q2), "isEqual identity");

        Quaternion q3 = Quaternion.Euler(45, 30, 60);
        Quaternion q4 = Quaternion.Euler(45, 30, 60);
        assertTrue(q3.isEqual(q4), "isEqual same rotation");

        // 不同旋转
        Quaternion q5 = Quaternion.Euler(0, 90, 0);
        Quaternion q6 = Quaternion.Euler(0, 0, 0);
        assertFalse(q5.isEqual(q6), "isEqual different");

        // 自定义精度：小角度差在宽松精度下相等
        Quaternion q7 = Quaternion.Euler(45, 30, 60);
        Quaternion q8 = Quaternion.Euler(45.001f, 30.001f, 60.001f);
        assertTrue(q7.isEqual(q8, 0.01f), "isEqual within custom precision");
        // 欧拉角差转四元数后分量差异很小，需要更大角度差才能在 0.0001 精度区分
        Quaternion q9 = Quaternion.Euler(0, 0, 0);
        Quaternion q10 = Quaternion.Euler(0, 90, 0); // 90° 差，分量差异明显
        assertFalse(q9.isEqual(q10, 0.0001f), "isEqual different with strict precision");
    }
}
