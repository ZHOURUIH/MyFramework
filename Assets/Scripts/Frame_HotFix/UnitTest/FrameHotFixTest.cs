using System;
using System.Collections.Generic;
using System.Diagnostics;
using static UnityUtility;

// 运行时集成测试入口
// 在 GameHotFix.onPostInit() 里调用
// 只在 Editor 或 Development Build 下生效
public class FrameHotFixTest
{
	private static readonly Dictionary<string, Action> sTests = new();
	public static void runAll()
	{
		// Frame_HotFix 测试已移动到 Frame_HotFix/UnitTest 目录
		// 通过 FrameHotFixTestRunner.RunAllTests() 统一运行
		
		Register("MostSafeFloatTest", MostSafeFloatTest.Run);
		Register("SafeValueTypeTest", SafeValueTypeTest.Run);
		Register("NetPacketBitTest", NetPacketBitTest.Run);
		Register("MathUtilityTest", MathUtilityTest.Run);
		Register("BinaryUtilityTest", BinaryUtilityTest.Run);
		Register("StructTest", StructTest.Run);
		Register("MyTimerTest", MyTimerTest.Run);
		Register("StreamBufferTest", StreamBufferTest.Run);
		Register("SpringTest", SpringTest.Run);
		Register("SerializeByteUtilityTest", SerializeByteUtilityTest.Run);
		Register("TimeUtilityTest", TimeUtilityTest.Run);
		Register("BulkMathGreaterPow2TableTests", BulkMathGreaterPow2TableTests.Run);
		Register("BulkMathDivideIntTableTests", BulkMathDivideIntTableTests.Run);
		Register("BulkMathGridIndexTableTests", BulkMathGridIndexTableTests.Run);
		Register("BulkMathClampIntTableTests", BulkMathClampIntTableTests.Run);
		Register("BulkMathPowerAndUnitTableTests", BulkMathPowerAndUnitTableTests.Run);
		Register("SerializeBitUtilityRoundtripTests", SerializeBitUtilityRoundtripTests.Run);
		Register("FrameUtilityCoreTest", FrameUtilityCoreTest.Run);
		Register("StringUtilityCoverageTest", StringUtilityCoverageTest.Run);
		Register("FileUtilitySmokeTest", FileUtilitySmokeTest.Run);
		Register("MyStringBuilderCoverageTest", MyStringBuilderCoverageTest.Run);
		Register("SerializeBitUtilityEdgeCasesTest", SerializeBitUtilityEdgeCasesTest.Run);
		Register("MathUtilityAdvancedCoverageTest", MathUtilityAdvancedCoverageTest.Run);
		Register("TimeUtilityAdvancedCoverageTest", TimeUtilityAdvancedCoverageTest.Run);
		Register("SerializeByteUtilityAdvancedCoverageTest", SerializeByteUtilityAdvancedCoverageTest.Run);
		Register("BinaryUtilityAdvancedCoverageTest", BinaryUtilityAdvancedCoverageTest.Run);
		Register("FrameUtilityPureCoverageTest", FrameUtilityPureCoverageTest.Run);
		Register("MathUtilityExtendedTest", MathUtilityExtendedTest.Run);
		Register("BinaryUtilityExtendedTest", BinaryUtilityExtendedTest.Run);
		Register("StringUtilityExtendedTest", StringUtilityExtendedTest.Run);
		Register("FrameUtilityExtendedTest", FrameUtilityExtendedTest.Run);
		Register("TimeUtilityCoverageTest", TimeUtilityCoverageTest.Run);
		Register("SerializeByteUtilityCoverageTest", SerializeByteUtilityCoverageTest.Run);
		Register("SerializeBitUtilityCoverageTest", SerializeBitUtilityCoverageTest.Run);
		Register("FileUtilityCoverageTest", FileUtilityCoverageTest.Run);
		Register("MyStringBuilderExtendedTest", MyStringBuilderExtendedTest.Run);
		Register("FrameUtilityStringStrictTests", FrameUtilityStringStrictTests.Run);
		Register("ArrayExtensionTest", ArrayExtensionTest.Run);
		Register("ClassObjectTest", ClassObjectTest.Run);
		Register("CommandTest", CommandTest.Run);
		Register("DictionaryExtensionTest", DictionaryExtensionTest.Run);
		Register("EventSystemTest", EventSystemTest.Run);
		Register("ListExtensionTest", ListExtensionTest.Run);
		Register("MyStringBuilderTest", MyStringBuilderTest.Run);
		Register("PoolTest", PoolTest.Run);
		Register("SafeCollectionTest", SafeCollectionTest.Run);
		Register("SerializerBitTest", SerializerBitTest.Run);
		Register("SerializeTest", SerializeTest.Run);
		Register("StringUtilityTest", StringUtilityTest.Run);	
		doRunAll(sTests);
	}
	public static void Register(string name, Action run)
	{
		sTests.Add(name, run);
	}
	public static void doRunAll(Dictionary<string, Action> list)
	{
		int pass = 0;
		int fail = 0;
		List<TestResult> results = new();
		foreach (var test in list)
		{
			var result = runOne(test.Key, test.Value);
			results.Add(result);

			if (result.passed)
			{
				pass++;
			}
			else
			{
				fail++;
			}
		}
		if (fail > 0)
		{
			logError($"[TestRunner] {fail} 个测试失败，请查看上方日志！");
		}
	}
	public static TestResult runOne(string name, Action run)
	{
		var sw = Stopwatch.StartNew();
		try
		{
			run();
			sw.Stop();
			return new TestResult(name, true, "", (float)sw.Elapsed.TotalMilliseconds);
		}
		catch (Exception ex)
		{
			sw.Stop();
			logException(ex);
			return new TestResult(name, false, "", (float)sw.Elapsed.TotalMilliseconds);
		}
	}
}
