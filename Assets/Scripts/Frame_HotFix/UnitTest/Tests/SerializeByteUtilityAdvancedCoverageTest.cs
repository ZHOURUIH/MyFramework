#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;
using static SerializeByteUtility;
using static TestAssert;

public static class SerializeByteUtilityAdvancedCoverageTest
{
	public static void Run()
	{
		testPrimitiveByteLayout();
		testSpanByteLayout();
		testArrayRoundTrips();
		testVectorRoundTrips();
		testGuardPaths();
	}

	private static void testPrimitiveByteLayout()
	{
		byte[] bytes2 = new byte[2];
		ushortToBytes(0x1234, bytes2);
		assertEqual((byte)0x34, bytes2[0], "ushortToBytes little endian byte0");
		assertEqual((byte)0x12, bytes2[1], "ushortToBytes little endian byte1");

		shortToBytesBigEndian(0x1234, bytes2);
		assertEqual((byte)0x12, bytes2[0], "shortToBytesBigEndian byte0");
		assertEqual((byte)0x34, bytes2[1], "shortToBytesBigEndian byte1");

		byte[] bytes4 = new byte[4];
		intToBytes(0x01020304, bytes4);
		assertEqual((byte)0x04, bytes4[0], "intToBytes byte0");
		assertEqual((byte)0x01, bytes4[3], "intToBytes byte3");
		intToBytesBigEndian(0x01020304, bytes4);
		assertEqual((byte)0x01, bytes4[0], "intToBytesBigEndian byte0");
		assertEqual((byte)0x04, bytes4[3], "intToBytesBigEndian byte3");
	}

	private static void testSpanByteLayout()
	{
		Span<byte> shortBytes = stackalloc byte[] { 0x34, 0x12 };
		assertEqual((short)0x1234, bytesToShort(shortBytes), "bytesToShort(Span<byte>) should decode little endian");
		assertEqual((short)0x3412, bytesToShortBigEndian(shortBytes), "bytesToShortBigEndian(Span<byte>) should decode big endian");

		Span<byte> intBytes = stackalloc byte[] { 0x78, 0x56, 0x34, 0x12 };
		assertEqual(0x12345678, bytesToInt(intBytes), "bytesToInt(Span<byte>) should decode little endian");
		assertEqual(0x78563412, bytesToIntBigEndian(intBytes), "bytesToIntBigEndian(Span<byte>) should decode big endian");
	}

	private static void testArrayRoundTrips()
	{
		{
			byte[] buffer = new byte[2];
			int index = 0;
			assert(writeShort(buffer, buffer.Length, ref index, -12345), "writeShort should succeed");
			int readIndex = 0;
			assertEqual((short)-12345, readShort(buffer, buffer.Length, ref readIndex, out _), "readShort roundtrip");
		}
		{
			byte[] buffer = new byte[2];
			int index = 0;
			assert(writeShortBigEndian(buffer, buffer.Length, ref index, 12345), "writeShortBigEndian should succeed");
			int readIndex = 0;
			assertEqual((short)12345, readShortBigEndian(buffer, buffer.Length, ref readIndex, out _), "readShortBigEndian roundtrip");
		}
		{
			byte[] buffer = new byte[4];
			int index = 0;
			assert(writeInt(buffer, buffer.Length, ref index, -123456789), "writeInt should succeed");
			int readIndex = 0;
			assertEqual(-123456789, readInt(buffer, buffer.Length, ref readIndex, out _), "readInt roundtrip");
		}
		{
			byte[] buffer = new byte[4];
			int index = 0;
			assert(writeIntBigEndian(buffer, buffer.Length, ref index, 123456789), "writeIntBigEndian should succeed");
			int readIndex = 0;
			assertEqual(123456789, readIntBigEndian(buffer, buffer.Length, ref readIndex, out _), "readIntBigEndian roundtrip");
		}
		{
			byte[] buffer = new byte[4];
			int index = 0;
			assert(writeUInt(buffer, buffer.Length, ref index, 4000000000u), "writeUInt should succeed");
			int readIndex = 0;
			assertEqual(4000000000u, readUInt(buffer, buffer.Length, ref readIndex, out _), "readUInt roundtrip");
		}
		{
			byte[] buffer = new byte[4];
			int index = 0;
			assert(writeUIntBigEndian(buffer, buffer.Length, ref index, 4000000000u), "writeUIntBigEndian should succeed");
			int readIndex = 0;
			assertEqual(4000000000u, readUIntBigEndian(buffer, buffer.Length, ref readIndex, out _), "readUIntBigEndian roundtrip");
		}
		{
			byte[] buffer = new byte[8];
			int index = 0;
			assert(writeLong(buffer, buffer.Length, ref index, 1234567890123456789L), "writeLong should succeed");
			int readIndex = 0;
			assertEqual(1234567890123456789L, readLong(buffer, buffer.Length, ref readIndex, out _), "readLong roundtrip");
		}
		{
			byte[] buffer = new byte[8];
			int index = 0;
			assert(writeLongBigEndian(buffer, buffer.Length, ref index, -1234567890123456789L), "writeLongBigEndian should succeed");
			int readIndex = 0;
			assertEqual(-1234567890123456789L, readLongBigEndian(buffer, buffer.Length, ref readIndex, out _), "readLongBigEndian roundtrip");
		}
		{
			byte[] buffer = new byte[4];
			int index = 0;
			assert(writeFloat(buffer, buffer.Length, ref index, 3.1415926f), "writeFloat should succeed");
			int readIndex = 0;
			assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - 3.1415926f) < 0.0001f, "readFloat roundtrip");
		}
		{
			byte[] buffer = new byte[4];
			int index = 0;
			assert(writeFloatBigEndian(buffer, buffer.Length, ref index, -2.5f), "writeFloatBigEndian should succeed");
			int readIndex = 0;
			assertTrue(Mathf.Abs(readFloatBigEndian(buffer, buffer.Length, ref readIndex, out _) - (-2.5f)) < 0.0001f, "readFloatBigEndian roundtrip");
		}
		{
			byte[] buffer = new byte[8];
			int index = 0;
			assert(writeDouble(buffer, buffer.Length, ref index, 2.718281828459045), "writeDouble should succeed");
			int readIndex = 0;
			assertTrue(Math.Abs(readDouble(buffer, buffer.Length, ref readIndex, out _) - 2.718281828459045) < 0.0000001, "readDouble roundtrip");
		}
		{
			byte[] buffer = new byte[8];
			int index = 0;
			assert(writeDoubleBigEndian(buffer, buffer.Length, ref index, -0.125), "writeDoubleBigEndian should succeed");
			int readIndex = 0;
			assertTrue(Math.Abs(readDoubleBigEndian(buffer, buffer.Length, ref readIndex, out _) - (-0.125)) < 0.0000001, "readDoubleBigEndian roundtrip");
		}
	}

	private static void testVectorRoundTrips()
	{
		byte[] buffer = new byte[128];
		int index = 0;

		Vector2 v2 = new(1.25f, -2.5f);
		Vector3 v3 = new(3.5f, 4.5f, -5.5f);
		Vector4 v4 = new(6.25f, -7.5f, 8.75f, -9.125f);
		Vector2Int v2i = new(11, -12);
		Vector3Int v3i = new(13, -14, 15);

		assert(writeVector2(buffer, buffer.Length, ref index, v2), "writeVector2 should succeed");
		assert(writeVector3(buffer, buffer.Length, ref index, v3), "writeVector3 should succeed");
		assert(writeVector4(buffer, buffer.Length, ref index, v4), "writeVector4 should succeed");
		assert(writeVector2Int(buffer, buffer.Length, ref index, v2i), "writeVector2Int should succeed");
		assert(writeInt(buffer, buffer.Length, ref index, v3i.x), "write int x for Vector3Int");
		assert(writeInt(buffer, buffer.Length, ref index, v3i.y), "write int y for Vector3Int");
		assert(writeInt(buffer, buffer.Length, ref index, v3i.z), "write int z for Vector3Int");

		int readIndex = 0;
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v2.x) < 0.0001f, "Vector2 x roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v2.y) < 0.0001f, "Vector2 y roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v3.x) < 0.0001f, "Vector3 x roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v3.y) < 0.0001f, "Vector3 y roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v3.z) < 0.0001f, "Vector3 z roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.x) < 0.0001f, "Vector4 x roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.y) < 0.0001f, "Vector4 y roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.z) < 0.0001f, "Vector4 z roundtrip");
		assertTrue(Mathf.Abs(readFloat(buffer, buffer.Length, ref readIndex, out _) - v4.w) < 0.0001f, "Vector4 w roundtrip");
		assertEqual(v2i.x, readInt(buffer, buffer.Length, ref readIndex, out _), "Vector2Int x roundtrip");
		assertEqual(v2i.y, readInt(buffer, buffer.Length, ref readIndex, out _), "Vector2Int y roundtrip");
		assertEqual(v3i.x, readInt(buffer, buffer.Length, ref readIndex, out _), "Vector3Int x roundtrip");
		assertEqual(v3i.y, readInt(buffer, buffer.Length, ref readIndex, out _), "Vector3Int y roundtrip");
		assertEqual(v3i.z, readInt(buffer, buffer.Length, ref readIndex, out _), "Vector3Int z roundtrip");
	}

	private static void testGuardPaths()
	{
		byte[] tiny = new byte[1];
		int index = 0;
		assertFalse(writeShort(tiny, tiny.Length, ref index, 1), "writeShort should fail on tiny buffer");
		assertFalse(writeInt(tiny, tiny.Length, ref index, 1), "writeInt should fail on tiny buffer");
		assertFalse(writeDouble(tiny, tiny.Length, ref index, 1.0), "writeDouble should fail on tiny buffer");

		byte[] source = { 1, 2, 3, 4 };
		byte[] dest = { 0, 0 };
		index = 0;
		assertFalse(readBytes(source, ref index, dest, source.Length, dest.Length, 3), "readBytes should reject oversized read");
	}
}
#endif
