using System;
using System.Collections.Generic;
using System.Diagnostics;
using static UnityUtility;

public class FrameHotFixTest
{
    private static readonly Dictionary<string, Action> sTests = new();
    public static void runAll()
    {
        Register("AttributeLabelTest", AttributeLabelTest.Run);
        Register("ArrayExtensionTest", ArrayExtensionTest.Run);
        Register("ArrayScopeTest", ArrayScopeTest.Run);
        Register("AssetVersionSystemTest", AssetVersionSystemTest.Run);
        Register("AtlasManagerTest", AtlasManagerTest.Run);
        Register("AtlasUGUITest", AtlasUGUITest.Run);
        Register("GlobalTouchSystemTest", GlobalTouchSystemTest.Run);
        Register("MouseCastWindowSetTest", MouseCastWindowSetTest.Run);
        Register("MouseCastObjectSetTest", MouseCastObjectSetTest.Run);
        Register("TouchInfoTest", TouchInfoTest.Run);
        Register("BinaryUtilityTest", BinaryUtilityTest.Run);
        Register("COMWindowDragViewTest", COMWindowDragViewTest.Run);
        Register("ClassObjectTest", ClassObjectTest.Run);
        Register("ClassPoolSingleTest", ClassPoolSingleTest.Run);
        Register("CommandTest", CommandTest.Run);
        Register("CommandSystemTest", CommandSystemTest.Run);
        Register("ComplexPointTest", ComplexPointTest.Run);
        Register("ComponentTest", ComponentTest.Run);
        Register("ComponentOwnerDeepTest", ComponentOwnerDeepTest.Run);
        Register("PoolTest", PoolTest.Run);
        Register("PoolDeepTest", PoolDeepTest.Run);
        Register("AssetBundleInfoTest", AssetBundleInfoTest.Run);
        Register("AssetDataBaseLoadInfoTest", AssetDataBaseLoadInfoTest.Run);
        Register("CurveTest", CurveTest.Run);
        Register("DamageNumberDataTest", DamageNumberDataTest.Run);
        Register("DictionaryExtensionTest", DictionaryExtensionTest.Run);
        Register("DistanceSortHelperTest", DistanceSortHelperTest.Run);
        Register("DoubleBufferTest", DoubleBufferTest.Run);
        Register("EditorCurveFactoryTest", EditorCurveFactoryTest.Run);
        Register("EventSystemTest", EventSystemTest.Run);
        Register("FileUtilityTest", FileUtilityTest.Run);
        Register("FrameBaseUtilityTest", FrameBaseUtilityTest.Run);
        Register("FrameSystemTest", FrameSystemTest.Run);
        Register("FrameUtilityTest", FrameUtilityTest.Run);
        Register("GameEventRegisteInfoTest", GameEventRegisteInfoTest.Run);
        Register("GameEffectTest", GameEffectTest.Run);
        Register("QuickEffectTest", QuickEffectTest.Run);
        Register("KeyFrameManagerTest", KeyFrameManagerTest.Run);
        Register("GameKeyframeTest", GameKeyframeTest.Run);
        Register("ListExtensionTest", ListExtensionTest.Run);
        Register("ListScope2Test", ListScope2Test.Run);
        Register("MathUtilityTest", MathUtilityTest.Run);
        Register("MathUtilityDeepTest", MathUtilityDeepTest.Run);
        Register("MostSafeFloatTest", MostSafeFloatTest.Run);
        Register("MostSafeIntTest", MostSafeIntTest.Run);
        Register("MostSafeLongTest", MostSafeLongTest.Run);
        Register("MyStringBuilderTest", MyStringBuilderTest.Run);
        Register("MyTimer1Test", MyTimer1Test.Run);
        Register("MyTimerTest", MyTimerTest.Run);
        Register("NetPacketBitTest", NetPacketBitTest.Run);
        Register("NetPacketByteTest", NetPacketByteTest.Run);
        Register("SafeDeepDictionaryTest", SafeDeepDictionaryTest.Run);
        Register("SafeDictionaryTest", SafeDictionaryTest.Run);
        Register("SafeFloatTest", SafeFloatTest.Run);
        Register("SafeHashSetTest", SafeHashSetTest.Run);
        Register("SafeIntTest", SafeIntTest.Run);
        Register("SafeListTest", SafeListTest.Run);
        Register("SafeLongTest", SafeLongTest.Run);
        Register("SequenceSpritePreviewBaseTest", SequenceSpritePreviewBaseTest.Run);
        Register("SerializableBitTest", SerializableBitTest.Run);
        Register("SerializableTest", SerializableTest.Run);
        Register("SerializeByteUtilityTest", SerializeByteUtilityTest.Run);
        Register("SpringTest", SpringTest.Run);
        Register("SpringDeepTest", SpringDeepTest.Run);
        Register("StateGroupTest", StateGroupTest.Run);
        Register("StateParamTest", StateParamTest.Run);
        Register("StreamBufferTest", StreamBufferTest.Run);
        Register("StringExtensionTest", StringExtensionTest.Run);
        Register("StringUtilityTest", StringUtilityTest.Run);
        Register("ThreadLockTest", ThreadLockTest.Run);
        Register("ThreadLockManagerTest", ThreadLockManagerTest.Run);
        Register("TileRenderDataTest", TileRenderDataTest.Run);
        Register("TimeUtilityTest", TimeUtilityTest.Run);
        Register("TimeUtilityDeepTest", TimeUtilityDeepTest.Run);
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
        Register("SafeFastDeepListTest", SafeFastDeepListTest.Run);
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
        Register("LayoutLoadGroupTest", LayoutLoadGroupTest.Run);
        Register("LongPressDataTest", LongPressDataTest.Run);
        Register("GameLayoutTest", GameLayoutTest.Run);
        Register("GameLayoutDeepTest", GameLayoutDeepTest.Run);
        Register("GameLayoutLifecycleTest", GameLayoutLifecycleTest.Run);
        Register("LayoutInfoTest", LayoutInfoTest.Run);
        Register("LayoutScriptTest", LayoutScriptTest.Run);
        Register("SceneProcedureTest", SceneProcedureTest.Run);
        Register("GameSceneTest", GameSceneTest.Run);
        Register("ScopeFallbackTest", ScopeFallbackTest.Run);
        Register("AsyncOperationAndTaskGroupTest", AsyncOperationAndTaskGroupTest.Run);
        Register("InputDataAndTouchPointTest", InputDataAndTouchPointTest.Run);
        Register("PacketAndPurchaseInfoTest", PacketAndPurchaseInfoTest.Run);
        Register("ResourceInfoBasicTest", ResourceInfoBasicTest.Run);
        Register("UIDepthTest", UIDepthTest.Run);
        Register("LayoutAndLongPressDataTest", LayoutAndLongPressDataTest.Run);
        Register("WaitingTest", WaitingTest.Run);
        Register("MiscDataResetTest", MiscDataResetTest.Run);
        Register("MyCurveTest", MyCurveTest.Run);
        Register("ThreadLockScopeTest", ThreadLockScopeTest.Run);
        Register("ThreadTimeLockTest", ThreadTimeLockTest.Run);
        Register("MyThreadTest", MyThreadTest.Run);
        Register("StructAndFormItemTest", StructAndFormItemTest.Run);
        Register("Triangle2Test", Triangle2Test.Run);
        Register("Triangle3Test", Triangle3Test.Run);
        Register("Vector4IntTest", Vector4IntTest.Run);
        Register("FormItemFileTest", FormItemFileTest.Run);
        Register("FormItemParamTest", FormItemParamTest.Run);
        Register("ParamCopyableTest", ParamCopyableTest.Run);
        Register("StateGroupMutexTest", StateGroupMutexTest.Run);
        Register("NetPacketJsonHttpTest", NetPacketJsonHttpTest.Run);
        Register("DoubleBufferReaderTest", DoubleBufferReaderTest.Run);
        Register("PurchaseAndCurveInfoTest", PurchaseAndCurveInfoTest.Run);
        Register("ResourceManagerTest", ResourceManagerTest.Run);
        Register("Vector2IntMyTest", Vector2IntMyTest.Run);
        Register("AStarMinHeapTest", AStarMinHeapTest.Run);
        Register("AStarMinHeapDeepTest", AStarMinHeapDeepTest.Run);
        Register("PointTest", PointTest.Run);
        Register("Vector2ShortTest", Vector2ShortTest.Run);
        Register("Vector2UIntTest", Vector2UIntTest.Run);
        Register("Vector2UShortTest", Vector2UShortTest.Run);
        Register("Line2Test", Line2Test.Run);
        Register("Line3Test", Line3Test.Run);
        Register("Rect3Test", Rect3Test.Run);
        Register("Circle3Test", Circle3Test.Run);
        Register("PrefsUtilityTest", PrefsUtilityTest.Run);
        Register("SQLUtilityTest", SQLUtilityTest.Run);
        Register("SQLiteTableTest", SQLiteTableTest.Run);
        Register("WidgetUtilityTest", WidgetUtilityTest.Run);
        Register("ConvexPolygonTest", ConvexPolygonTest.Run);
        Register("HttpUtilityTest", HttpUtilityTest.Run);
        Register("RectTransformExtensionTest", RectTransformExtensionTest.Run);
        Register("MathExtensionTest", MathExtensionTest.Run);
        Register("SerializeBitUtilityTest", SerializeBitUtilityTest.Run);
        Register("TweenUtilityTest", TweenUtilityTest.Run);
        Register("ClassObjectExtensionTest", ClassObjectExtensionTest.Run);
        Register("WavSoundTest", WavSoundTest.Run);
        Register("SpriteAtlasExtensionTest", SpriteAtlasExtensionTest.Run);
        Register("LayoutScriptExtensionTest", LayoutScriptExtensionTest.Run);
		Register("UnityUtilityPhysicsTest", UnityUtilityPhysicsTest.Run);
		Register("RedPointTest", RedPointTest.Run);
		Register("DecisionTreeTest", DecisionTreeTest.Run);
		Register("NetCoreTest", NetCoreTest.Run);
		Register("LocalizationTest", LocalizationTest.Run);
		Register("DoubleExtensionTest", DoubleExtensionTest.Run);
		Register("IntExtensionTest", IntExtensionTest.Run);
		Register("LongExtensionTest", LongExtensionTest.Run);
		Register("QuaternionExtensionTest", QuaternionExtensionTest.Run);
		Register("UnityUtilityTest", UnityUtilityTest.Run);
		Register("Vector4ExtensionTest", Vector4ExtensionTest.Run);
		Register("FloatExtensionTest", FloatExtensionTest.Run);
		Register("Vector2ExtensionTest", Vector2ExtensionTest.Run);
		Register("Vector3ExtensionTest", Vector3ExtensionTest.Run);
		Register("Vector3IntExtensionTest", Vector3IntExtensionTest.Run);
		Register("GameEffectPoolTest", GameEffectPoolTest.Run);
		Register("KeyMappingSystemTest", KeyMappingSystemTest.Run);
		Register("ParamParseCollectionTest", ParamParseCollectionTest.Run);
		Register("NetStructBitTest", NetStructBitTest.Run);
		Register("NetStructByteTest", NetStructByteTest.Run);
		Register("AudioInfoTest", AudioInfoTest.Run);
		Register("AtlasRefTest", AtlasRefTest.Run);
		Register("SpriteRefTest", SpriteRefTest.Run);
		Register("AsyncTaskGroupManagerTest", AsyncTaskGroupManagerTest.Run);
		Register("ImageXBR4Test", ImageXBR4Test.Run);
		Register("ExcelDataTTest", ExcelDataTTest.Run);
		Register("ExcelTableTest", ExcelTableTest.Run);
		Register("GameObjectInfoTest", GameObjectInfoTest.Run);
		Register("MyUGUIObjectTest", MyUGUIObjectTest.Run);
		Register("MyUGUIObjectGeometryTest", MyUGUIObjectGeometryTest.Run);
		Register("ParamBaseTest", ParamBaseTest.Run);
		Register("CharacterStateTTest", CharacterStateTTest.Run);
        Register("SafeModifyTest", SafeModifyTest.Run);
        Register("AnimControlTest", AnimControlTest.Run);
        Register("CheckLayerTest", CheckLayerTest.Run);
        Register("ObsSystemTest", ObsSystemTest.Run);
        Register("SceneInstanceTest", SceneInstanceTest.Run);
        Register("InputSystemTest", InputSystemTest.Run);
        Register("TransformableTest", TransformableTest.Run);
        Register("COMMyTweenerFloatTest", COMMyTweenerFloatTest.Run);
        Register("TweenerManagerTest", TweenerManagerTest.Run);
        Register("MovableObjectTest", MovableObjectTest.Run);
        Register("SceneSystemTest", SceneSystemTest.Run);
        Register("SceneSystemDeepTest", SceneSystemDeepTest.Run);
        Register("GeometryStructTest", GeometryStructTest.Run);
        Register("NetPacketFactoryTest", NetPacketFactoryTest.Run);
        Register("ScopeTest", ScopeTest.Run);
        Register("CharacterStateTest", CharacterStateTest.Run);
        Register("AsyncTaskGroupTest", AsyncTaskGroupTest.Run);
        Register("TouchPointTest", TouchPointTest.Run);
        Register("StateManagerTest", StateManagerTest.Run);
        Register("StateManagerDeepTest", StateManagerDeepTest.Run);
        Register("UndoManagerDeepTest", UndoManagerDeepTest.Run);
        Register("COMCharacterStateMachineDeepTest", COMCharacterStateMachineDeepTest.Run);
        Register("WaitingManagerDeepTest", WaitingManagerDeepTest.Run);
        Register("InputSystemDeepTest", InputSystemDeepTest.Run);
        Register("EventSystemDeepTest", EventSystemDeepTest.Run);
        Register("RedPointSystemDeepTest", RedPointSystemDeepTest.Run);
        Register("SerializerByteTest", SerializerByteTest.Run);
        Register("NetPacketTest", NetPacketTest.Run);
        Register("HttpSendInfoTest", HttpSendInfoTest.Run);
        Register("PacketInfoTest", PacketInfoTest.Run);
        Register("NetPacketHttpTTest", NetPacketHttpTTest.Run);
        Register("ByteSerializableTest", ByteSerializableTest.Run);
        Register("ByteSerializableTest2", ByteSerializableTest2.Run);
        Register("ListPoolTest", ListPoolTest.Run);
        Register("ByteArrayPoolTest", ByteArrayPoolTest.Run);
        Register("TweenGroupTest", TweenGroupTest.Run);
        Register("AnimationLayerTest", AnimationLayerTest.Run);
        Register("TweenSequenceTest", TweenSequenceTest.Run);
        Register("TweenTrackTest", TweenTrackTest.Run);
        Register("TweenSequenceDeepTest", TweenSequenceDeepTest.Run);
        Register("CmdGlobalDelayCallTest", CmdGlobalDelayCallTest.Run);
        Register("MemberDataTest", MemberDataTest.Run);
        Register("CurveDeepTest", CurveDeepTest.Run);
        Register("MovableObjectDeepTest", MovableObjectDeepTest.Run);
        Register("GameSceneDeepTest", GameSceneDeepTest.Run);
        Register("SceneProcedureDeepTest", SceneProcedureDeepTest.Run);
        Register("BinaryUtilityDeepTest", BinaryUtilityDeepTest.Run);
        Register("SQLUtilityDeepTest", SQLUtilityDeepTest.Run);
        Register("TransformableDeepTest", TransformableDeepTest.Run);
        Register("SafeIntDeepTest", SafeIntDeepTest.Run);
        Register("SafeFloatDeepTest", SafeFloatDeepTest.Run);
        Register("SafeLongDeepTest", SafeLongDeepTest.Run);
        Register("MostSafeIntDeepTest", MostSafeIntDeepTest.Run);
        Register("MostSafeFloatDeepTest", MostSafeFloatDeepTest.Run);
        Register("MostSafeLongDeepTest", MostSafeLongDeepTest.Run);
        Register("CharacterStateDeepTest", CharacterStateDeepTest.Run);
        Register("CharacterManagerDeepTest", CharacterManagerDeepTest.Run);
        Register("CharacterDeepTest", CharacterDeepTest.Run);
        Register("AudioManagerTest", AudioManagerTest.Run);

        doRunAll(sTests);
    }
    public static void Register(string name, Action run)
    {
        if (sTests.ContainsKey(name))
        {
            logError("[TestRunner] duplicate test: " + name);
            return;
        }
        sTests.Add(name, run);
    }
    public static void doRunAll(Dictionary<string, Action> list)
    {
        int pass = 0;
        int fail = 0;
        foreach (var test in list)
        {
			TestResult result = runOne(test.Key, test.Value);
            if (result.mPassed)
            {
                pass++;
            }
            else
            {
                fail++;
            }
        }

        string info = "[TestRunner] total:" + list.Count + ", pass:" + pass + ", fail:" + fail;
        if (fail > 0)
        {
            logError(info);
        }
        else
        {
            log(info);
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
            logException(ex, "[TestRunner] failed: " + name); 
            return new TestResult(name, false, ex.Message, (float)sw.Elapsed.TotalMilliseconds);
        }
    }
}