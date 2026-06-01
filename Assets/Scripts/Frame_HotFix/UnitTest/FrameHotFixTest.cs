using System;
using System.Collections.Generic;
using System.Diagnostics;
using static UnityUtility;

public class FrameHotFixTest
{
    private static readonly Dictionary<string, Action> sTests = new();
    public static void runAll()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Register("ArrayExtensionTest", ArrayExtensionTest.Run);
        Register("ArrayScopeTest", ArrayScopeTest.Run);
        Register("BinaryUtilityTest", BinaryUtilityTest.Run);
        Register("ClassObjectTest", ClassObjectTest.Run);
        Register("ClassPoolSingleTest", ClassPoolSingleTest.Run);
        Register("CommandTest", CommandTest.Run);
        Register("CurveTest", CurveTest.Run);
        Register("DictionaryExtensionTest", DictionaryExtensionTest.Run);
        Register("DistanceSortHelperTest", DistanceSortHelperTest.Run);
        Register("DoubleBufferTest", DoubleBufferTest.Run);
        Register("EventSystemTest", EventSystemTest.Run);
        Register("FileUtilityTest", FileUtilityTest.Run);
        Register("FrameBaseUtilityTest", FrameBaseUtilityTest.Run);
        Register("FrameUtilityTest", FrameUtilityTest.Run);
        Register("GameEventRegisteInfoTest", GameEventRegisteInfoTest.Run);
        Register("ListExtensionTest", ListExtensionTest.Run);
        Register("ListScope2Test", ListScope2Test.Run);
        Register("MathUtilityTest", MathUtilityTest.Run);
        Register("MostSafeFloatTest", MostSafeFloatTest.Run);
        Register("MostSafeIntTest", MostSafeIntTest.Run);
        Register("MostSafeLongTest", MostSafeLongTest.Run);
        Register("MyStringBuilderTest", MyStringBuilderTest.Run);
        Register("MyTimer1Test", MyTimer1Test.Run);
        Register("MyTimerTest", MyTimerTest.Run);
        Register("NetPacketBitTest", NetPacketBitTest.Run);
        Register("NetPacketByteTest", NetPacketByteTest.Run);
        Register("SafeDeepDictionaryReaderTest", SafeDeepDictionaryReaderTest.Run);
        Register("SafeDeepDictionaryTest", SafeDeepDictionaryTest.Run);
        Register("SafeDeepListReaderTest", SafeDeepListReaderTest.Run);
        Register("SafeDictionaryTest", SafeDictionaryTest.Run);
        Register("SafeFloatTest", SafeFloatTest.Run);
        Register("SafeHashSetTest", SafeHashSetTest.Run);
        Register("SafeIntTest", SafeIntTest.Run);
        Register("SafeListTest", SafeListTest.Run);
        Register("SafeLongTest", SafeLongTest.Run);
        Register("SerializableBitTest", SerializableBitTest.Run);
        Register("SerializableTest", SerializableTest.Run);
        Register("SerializeByteUtilityTest", SerializeByteUtilityTest.Run);
        Register("SpringTest", SpringTest.Run);
        Register("StateGroupTest", StateGroupTest.Run);
        Register("StateParamTest", StateParamTest.Run);
        Register("StreamBufferTest", StreamBufferTest.Run);
        Register("StringExtensionTest", StringExtensionTest.Run);
        Register("StringUtilityTest", StringUtilityTest.Run);
        Register("ThreadLockTest", ThreadLockTest.Run);
        Register("TimeUtilityTest", TimeUtilityTest.Run);
        Register("TypeIDTest", TypeIDTest.Run);
        Register("UndoManagerTest", UndoManagerTest.Run);
        Register("UndoTest", UndoTest.Run);
        Register("UnityCurveTest", UnityCurveTest.Run);
        Register("Vector2IntExtensionTest", Vector2IntExtensionTest.Run);
        Register("GameEventTest", GameEventTest.Run);
        Register("ParamSetTest", ParamSetTest.Run);
        Register("AStarNodeTest", AStarNodeTest.Run);
        Register("ComplexTest", ComplexTest.Run);
        Register("HashSetExtensionTest", HashSetExtensionTest.Run);
        Register("SpanExtensionTest", SpanExtensionTest.Run);
        Register("DictionaryTypeTest", DictionaryTypeTest.Run);
        Register("SafeDeepListTest", SafeDeepListTest.Run);
        Register("SafeList0Test", SafeList0Test.Run);
        Register("SafeListReaderTest", SafeListReaderTest.Run);
        Register("SafeDictionaryReaderTest", SafeDictionaryReaderTest.Run);
        Register("SafeHashSetReaderTest", SafeHashSetReaderTest.Run);
        Register("BIT_BOOLTest", BIT_BOOLTest.Run);
        Register("BIT_BYTESTest", BIT_BYTESTest.Run);
        Register("BIT_BYTETest", BIT_BYTETest.Run);
        Register("BIT_FLOATSTest", BIT_FLOATSTest.Run);
        Register("BIT_FLOATTest", BIT_FLOATTest.Run);
        Register("BIT_INTSTest", BIT_INTSTest.Run);
        Register("BIT_INTTest", BIT_INTTest.Run);
        Register("BIT_LONGSTest", BIT_LONGSTest.Run);
        Register("BIT_LONGTest", BIT_LONGTest.Run);
        Register("BIT_SBYTESTest", BIT_SBYTESTest.Run);
        Register("BIT_SBYTETest", BIT_SBYTETest.Run);
        Register("BIT_SHORTSTest", BIT_SHORTSTest.Run);
        Register("BIT_SHORTTest", BIT_SHORTTest.Run);
        Register("BIT_STRINGSTest", BIT_STRINGSTest.Run);
        Register("BIT_STRINGTest", BIT_STRINGTest.Run);
        Register("BIT_UINTSTest", BIT_UINTSTest.Run);
        Register("BIT_UINTTest", BIT_UINTTest.Run);
        Register("BIT_ULONGSTest", BIT_ULONGSTest.Run);
        Register("BIT_ULONGTest", BIT_ULONGTest.Run);
        Register("BIT_USHORTSTest", BIT_USHORTSTest.Run);
        Register("BIT_USHORTTest", BIT_USHORTTest.Run);
        Register("BIT_VECTOR2Test", BIT_VECTOR2Test.Run);
        Register("BIT_VECTOR2_INTTest", BIT_VECTOR2_INTTest.Run);
        Register("BIT_VECTOR2_SHORTTest", BIT_VECTOR2_SHORTTest.Run);
        Register("BIT_VECTOR2_UINTTest", BIT_VECTOR2_UINTTest.Run);
        Register("BIT_VECTOR2_USHORTTest", BIT_VECTOR2_USHORTTest.Run);
        Register("BIT_VECTOR3Test", BIT_VECTOR3Test.Run);
        Register("BIT_VECTOR4Test", BIT_VECTOR4Test.Run);
        Register("SerializerBitReadTest", SerializerBitReadTest.Run);
        Register("SerializerBitWriteTest", SerializerBitWriteTest.Run);
        Register("LayoutManagerTest", LayoutManagerTest.Run);
        Register("GameLayoutTest", GameLayoutTest.Run);
        Register("LayoutScriptTest", LayoutScriptTest.Run);
        Register("SceneProcedureTest", SceneProcedureTest.Run);
        Register("GameSceneTest", GameSceneTest.Run);
#endif
        doRunAll(sTests);
    }
    public static void Register(string name, Action run) { sTests.Add(name, run); }
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
        if (fail > 0) logError("[TestRunner] "+fail+" tests failed");
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
        catch(Exception ex)
        {
            sw.Stop();
            logException(ex);
            return new TestResult(name, false, "", (float)sw.Elapsed.TotalMilliseconds);
        }
    }
}
