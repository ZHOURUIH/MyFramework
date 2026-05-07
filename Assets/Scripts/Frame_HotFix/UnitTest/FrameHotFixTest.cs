using System;
using System.Collections.Generic;
using System.Diagnostics;
using static UnityUtility;

// 自动生成 - 76 个测试类
public class FrameHotFixTest
{
	private static readonly Dictionary<string, Action> sTests = new();
	public static void runAll()
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Register("ArrayExtensionTest", ArrayExtensionTest.Run);
		Register("BinaryTimeFileComprehensiveTest", BinaryTimeFileComprehensiveTest.Run);
		Register("BinaryTimeFileUtilityEdgeTest", BinaryTimeFileUtilityEdgeTest.Run);
		Register("BinaryUtilityAdvancedCoverageTest", BinaryUtilityAdvancedCoverageTest.Run);
		Register("BinaryUtilityExtendedTest", BinaryUtilityExtendedTest.Run);
		Register("BinaryUtilityRemainingTest", BinaryUtilityRemainingTest.Run);
		Register("BinaryUtilityTest", BinaryUtilityTest.Run);
		Register("BitTypeFullTest", BitTypeFullTest.Run);
		Register("BulkMathClampIntTableTests", BulkMathClampIntTableTests.Run);
		Register("BulkMathDivideIntTableTests", BulkMathDivideIntTableTests.Run);
		Register("BulkMathGreaterPow2TableTests", BulkMathGreaterPow2TableTests.Run);
		Register("BulkMathGridIndexTableTests", BulkMathGridIndexTableTests.Run);
		Register("BulkMathPowerAndUnitTableTests", BulkMathPowerAndUnitTableTests.Run);
		Register("ByteTypeFullTest", ByteTypeFullTest.Run);
		Register("CharacterDecisionStateTest", CharacterDecisionStateTest.Run);
		Register("ClassObjectTest", ClassObjectTest.Run);
		Register("CommandEventSystemTest2", CommandEventSystemTest2.Run);
		Register("CommandEventThreadTest", CommandEventThreadTest.Run);
		Register("CommandTest", CommandTest.Run);
		Register("DataClassesConsistencyTest", DataClassesConsistencyTest.Run);
		Register("DictionaryExtensionTest", DictionaryExtensionTest.Run);
		Register("DoubleBufferTest", DoubleBufferTest.Run);
		Register("EventSystemTest", EventSystemTest.Run);
		Register("FileUtilityCoverageTest", FileUtilityCoverageTest.Run);
		Register("FileUtilitySmokeTest", FileUtilitySmokeTest.Run);
		Register("FrameBaseUtilityTest", FrameBaseUtilityTest.Run);
		Register("FrameUtilityCoreTest", FrameUtilityCoreTest.Run);
		Register("FrameUtilityExpandedTest", FrameUtilityExpandedTest.Run);
		Register("FrameUtilityExtendedTest", FrameUtilityExtendedTest.Run);
		Register("FrameUtilityPureCoverageTest", FrameUtilityPureCoverageTest.Run);
		Register("FrameUtilityRemainingTest", FrameUtilityRemainingTest.Run);
		Register("FrameUtilityStringStrictTests", FrameUtilityStringStrictTests.Run);
		Register("HotFixBusinessTest", HotFixBusinessTest.Run);
		Register("HotFixExtensionDeepTest", HotFixExtensionDeepTest.Run);
		Register("ListExtensionTest", ListExtensionTest.Run);
		Register("MathUtilityAdvancedCoverageTest", MathUtilityAdvancedCoverageTest.Run);
		Register("MathUtilityExtendedTest", MathUtilityExtendedTest.Run);
		Register("MathUtilityTest", MathUtilityTest.Run);
		Register("MostSafeFloatTest", MostSafeFloatTest.Run);
		Register("MyStringBuilderCoverageTest", MyStringBuilderCoverageTest.Run);
		Register("MyStringBuilderExtendedTest", MyStringBuilderExtendedTest.Run);
		Register("MyStringBuilderExtendedTest2", MyStringBuilderExtendedTest2.Run);
		Register("MyStringBuilderTest", MyStringBuilderTest.Run);
		Register("MyTimerTest", MyTimerTest.Run);
		Register("NetPacketBitTest", NetPacketBitTest.Run);
		Register("PoolTest", PoolTest.Run);
		Register("SafeCollectionTest", SafeCollectionTest.Run);
		Register("SafeDictionaryHashSetDetailedTest", SafeDictionaryHashSetDetailedTest.Run);
		Register("SafeListDetailedTest", SafeListDetailedTest.Run);
		Register("SafeTypeStandaloneTest", SafeTypeStandaloneTest.Run);
		Register("SafeValueTypeTest", SafeValueTypeTest.Run);
		Register("SerializeBitUtilityCoverageTest", SerializeBitUtilityCoverageTest.Run);
		Register("SerializeBitUtilityEdgeCasesTest", SerializeBitUtilityEdgeCasesTest.Run);
		Register("SerializeBitUtilityRoundtripTests", SerializeBitUtilityRoundtripTests.Run);
		Register("SerializeByteUtilityAdvancedCoverageTest", SerializeByteUtilityAdvancedCoverageTest.Run);
		Register("SerializeByteUtilityCoverageTest", SerializeByteUtilityCoverageTest.Run);
		Register("SerializeByteUtilityTest", SerializeByteUtilityTest.Run);
		Register("SerializeTest", SerializeTest.Run);
		Register("SerializerBitCoverageTestDummy", SerializerBitCoverageTest.Run);
		Register("SerializerBitIndividualTest", SerializerBitIndividualTest.Run);
		Register("SerializerBitTest", SerializerBitTest.Run);
		Register("SerializerByteIndividualTest", SerializerByteIndividualTest.Run);
		Register("SerializerByteTestDummy", SerializerByteTest.Run);
		Register("SerializerDeepTypeTest", SerializerDeepTypeTest.Run);
		Register("SpringTest", SpringTest.Run);
		Register("StreamBufferTest", StreamBufferTest.Run);
		Register("StringUtilityCoverageTest", StringUtilityCoverageTest.Run);
		Register("StringUtilityExtendedTest", StringUtilityExtendedTest.Run);
		Register("StringUtilityTest", StringUtilityTest.Run);
		Register("StructTest", StructTest.Run);
		Register("TimeUtilityAdvancedCoverageTest", TimeUtilityAdvancedCoverageTest.Run);
		Register("TimeUtilityCoverageTest", TimeUtilityCoverageTest.Run);
		Register("TimeUtilityPureTest", TimeUtilityPureTest.Run);
		Register("TimeUtilityTest", TimeUtilityTest.Run);
		Register("UndoWaitingAsyncTest", UndoWaitingAsyncTest.Run);
		Register("UtilComprehensiveEdgeTest", UtilComprehensiveEdgeTest.Run);
#endif
		doRunAll(sTests);
}
	public static void Register(string name, Action run)
	{
		sTests.Add(name, run);
}
	public static void doRunAll(Dictionary<string, Action> list)
	{
		int pass = 0; int fail = 0;
		List<TestResult> results = new List<TestResult>();
		foreach (var test in list)
		{
			var result = runOne(test.Key, test.Value);
			results.Add(result);
			if (result.passed) pass++; else fail++;
		}
		if (fail > 0) logError("[TestRunner] "+fail+" tests failed");
	}
	public static TestResult runOne(string name, Action run)
	{var sw = Stopwatch.StartNew();try{run();sw.Stop();return new TestResult(name,true,"",(float)sw.Elapsed.TotalMilliseconds);}catch(Exception ex){sw.Stop();logException(ex);return new TestResult(name,false,"",(float)sw.Elapsed.TotalMilliseconds);}}
}