#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;
using static SerializeByteUtility;
using static TestAssert;

public static class SerializeByteUtilityCoverageTest
{
    public static void Run()
    {
        testPrimitiveLayout();
        testBigEndianLayout();
        testVectorAndGuardPaths();
    }

    private static void testPrimitiveLayout()
    {
        byte[] b2 = new byte[2];
        ushortToBytes((ushort)0x1234, b2);
        assertEqual((ushort)0x1234, bytesToUShort(b2), "ushort round trip");

        shortToBytes((short)-12345, b2);
        assertEqual((short)-12345, bytesToShort(b2), "short round trip");

        byte[] b4 = new byte[4];
        intToBytes(0x01020304, b4);
        assertEqual(0x01020304, bytesToInt(b4), "int round trip");

        uintToBytes(0x89ABCDEF, b4);
        assertEqual(0x89ABCDEFu, bytesToUInt(b4), "uint round trip");

        float f = 3.1415926f;
        double d = -2.718281828;
        assertTrue(Mathf.Abs(bytesToFloat(toBytes(f)) - f) < 0.0001f, "float round trip");
        assertTrue(Math.Abs(bytesToDouble(toBytes(d)) - d) < 0.000001, "double round trip");
    }

    private static void testBigEndianLayout()
    {
        byte[] b4 = new byte[4];
        intToBytesBigEndian(0x01020304, b4);
        assertEqual((byte)0x01, b4[0], "big endian first byte");
        assertEqual(0x01020304, bytesToIntBigEndian(b4), "big endian int round trip");

        byte[] b2 = new byte[2];
        shortToBytesBigEndian((short)0x1234, b2);
        assertEqual((short)0x1234, bytesToShortBigEndian(b2), "big endian short round trip");

        uintToBytesBigEndian(0x89ABCDEFu, b4);
        assertEqual(0x89ABCDEFu, bytesToUIntBigEndian(b4), "big endian uint round trip");
    }

    private static void testVectorAndGuardPaths()
    {
        byte[] buffer = new byte[128];
        int index = 0;
        Vector2 v2 = new(1.25f, -2.5f);
        Vector3 v3 = new(3.5f, 4.5f, -5.5f);
        Vector4 v4 = new(6.25f, -7.5f, 8.75f, -9.125f);

        assert(writeVector2(buffer, buffer.Length, ref index, v2), "writeVector2");
        assert(writeVector3(buffer, buffer.Length, ref index, v3), "writeVector3");
        assert(writeVector4(buffer, buffer.Length, ref index, v4), "writeVector4");

        int readIndex = 0;
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v2.x) < 0.0001f, "vector2 x");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v2.y) < 0.0001f, "vector2 y");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v3.x) < 0.0001f, "vector3 x");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v3.y) < 0.0001f, "vector3 y");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v3.z) < 0.0001f, "vector3 z");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.x) < 0.0001f, "vector4 x");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.y) < 0.0001f, "vector4 y");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.z) < 0.0001f, "vector4 z");
        assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.w) < 0.0001f, "vector4 w");

        byte[] tiny = new byte[1];
        index = 0;
        assertFalse(writeInt(tiny, tiny.Length, ref index, 1), "writeInt guard");
        assertFalse(writeDouble(tiny, tiny.Length, ref index, 1.0), "writeDouble guard");
    }
}
#endif
