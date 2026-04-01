#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static BinaryUtility;
using static MathUtility;
using static FrameUtility;
using static TestAssert;
using static SerializeByteUtility;

public static class FrameUtilityPureCoverageTest
{
	public static void Run()
	{
		testEnumConversion();
		testCrcHelpers();
		testFindMaxHelpers();
		testFindMaxAbsHelpers();
		testPathIgnoreAndParsing();
		testLineAndStackHelpers();
	}

	private enum SampleEnum
	{
		Zero = 0,
		One = 1,
		Two = 2,
	}

	private static void testEnumConversion()
	{
		assertEqual(SampleEnum.Two, intToEnum<SampleEnum, int>(2), "intToEnum should map integers to enum values");
		assertEqual(1, enumToInt(SampleEnum.One), "enumToInt should map enum values to ints");
		assertTrue(isEnumValid(SampleEnum.Zero), "isEnumValid should accept defined values");
		assertFalse(isEnumValid((SampleEnum)9), "isEnumValid should reject undefined values");
	}

	private static void testCrcHelpers()
	{
		byte[] data = { 1, 2, 3, 4 };
		ushort fromBytes = generateCRC16(data, data.Length);
		assertEqual((ushort)(crc16(0x1F, data, data.Length) ^ 0x123F), fromBytes, "generateCRC16(byte[]) should wrap crc16 consistently");

		ushortToBytes((ushort)0x1234, out byte short0, out byte short1);
		ushort fromShort = generateCRC16((ushort)0x1234);
		assertEqual(generateCRC16(new byte[] { short0, short1 }, 2), fromShort, "generateCRC16(ushort) should match byte-array form");

		intToBytes(0x12345678, out byte int0, out byte int1, out byte int2, out byte int3);
		ushort fromInt = generateCRC16(0x12345678);
		assertEqual(generateCRC16(new byte[] { int0, int1, int2, int3 }, 4), fromInt, "generateCRC16(int) should match byte-array form");
	}

	private static void testFindMaxHelpers()
	{
		Span<sbyte> sbytes = stackalloc sbyte[] { -3, -1, 0, 2, 1 };
		assertEqual((sbyte)2, findMax(sbytes), "findMax(span sbyte) should find maximum");
		List<sbyte> sbyteList = new() { -3, -2, 4, 1 };
		assertEqual((sbyte)4, findMax(sbyteList), "findMax(list sbyte) should find maximum");

		Span<int> ints = stackalloc int[] { -5, -1, 0, 3, 8 };
		assertEqual(8, findMax(ints), "findMax(span int) should find maximum");
		List<int> intList = new() { -5, -1, 0, 3, 8 };
		assertEqual(8, findMax(intList), "findMax(list int) should find maximum");

		Span<float> floats = stackalloc float[] { -1.5f, 0.25f, 9.75f, 2.0f };
		assertTrue(isFloatEqual(findMax(floats), 9.75f, 0.0001f), "findMax(span float) should find maximum");
		List<float> floatList = new() { -1.5f, 0.25f, 9.75f, 2.0f };
		assertTrue(isFloatEqual(findMax(floatList), 9.75f, 0.0001f), "findMax(list float) should find maximum");

		Span<double> doubles = stackalloc double[] { -1.5, 0.25, 9.75, 2.0 };
		assertTrue(Math.Abs(findMax(doubles) - 9.75) < 0.0000001, "findMax(span double) should find maximum");
		List<double> doubleList = new() { -1.5, 0.25, 9.75, 2.0 };
		assertTrue(Math.Abs(findMax(doubleList) - 9.75) < 0.0000001, "findMax(list double) should find maximum");
	}

	private static void testFindMaxAbsHelpers()
	{
		Span<int> ints = stackalloc int[] { -5, -1, 0, 3, 8 };
		assertEqual(8, findMaxAbs(ints), "findMaxAbs(span int) should find maximum absolute value");
		List<int> intList = new() { -5, -1, 0, 3, 8 };
		assertEqual(8, findMaxAbs(intList), "findMaxAbs(list int) should find maximum absolute value");

		Span<float> floats = stackalloc float[] { -1.5f, 0.25f, -9.75f, 2.0f };
		assertTrue(isFloatEqual(findMaxAbs(floats), 9.75f, 0.0001f), "findMaxAbs(span float) should find maximum absolute value");
		List<float> floatList = new() { -1.5f, 0.25f, -9.75f, 2.0f };
		assertTrue(isFloatEqual(findMaxAbs(floatList), 9.75f, 0.0001f), "findMaxAbs(list float) should find maximum absolute value");

		Span<double> doubles = stackalloc double[] { -1.5, 0.25, -9.75, 2.0 };
		assertTrue(Math.Abs(findMaxAbs(doubles) - 9.75) < 0.0000001, "findMaxAbs(span double) should find maximum absolute value");
		List<double> doubleList = new() { -1.5, 0.25, -9.75, 2.0 };
		assertTrue(Math.Abs(findMaxAbs(doubleList) - 9.75) < 0.0000001, "findMaxAbs(list double) should find maximum absolute value");
	}

	private static void testPathIgnoreAndParsing()
	{
		List<string> ignores = new() { "/Temp/", "/Cache/" };
		assertTrue(isIgnorePath("/root/Temp/file.txt", ignores), "isIgnorePath should detect ignored path fragments");
		assertFalse(isIgnorePath("/root/Assets/file.txt", ignores), "isIgnorePath should allow non-matching paths");

		Dictionary<string, GameFileInfo> fileMap = new();
		parseFileList("", fileMap);
		assertEqual(0, fileMap.Count, "parseFileList should ignore empty content");
	}

	private static void testLineAndStackHelpers()
	{
		int line = getLineNum();
		assertTrue(line > 0, "getLineNum should return a positive line number");

		string sourceFile = getCurSourceFileName();
		assertFalse(string.IsNullOrEmpty(sourceFile), "getCurSourceFileName should return a file name");

		string stack = getStackTrace(2);
		assertFalse(string.IsNullOrEmpty(stack), "getStackTrace should return a non-empty trace");
	}
}
#endif
