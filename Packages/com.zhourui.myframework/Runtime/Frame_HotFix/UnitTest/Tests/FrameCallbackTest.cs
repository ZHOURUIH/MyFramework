using System;
using System.Reflection;
using UnityEngine;
using static TestAssert;

// FrameCallback delegate 定义检查(反射)
// 验证框架回调 delegate 的类型存在性 + 委托基类 + 参数数量
// 方法名匹配的变体类(同参数模式的 delegate)不逐个测, 覆盖代表性全集
public static class FrameCallbackTest
{
	public static void Run()
	{
		testIntCallback();
		testBoolFunction();
		testString2Callback();
		testVector3Callback();
		testIntBoolCallback();
		testRefBoolCallback();
		testFloat3Callback();
		testPredicate2();
		testLongCallback();
		testBoolBoolCallback();
		testFloatCallback();
		testStringIntCallback();
		testKeyFrameCallback();
		testGameEventCallback();
		testHttpCallback();
		testAssetLoadCallback();
		testLogCallback();
		testVector3IntCallback();
		testDragCallback();
		testScrollItemCallback();
		testStringArrayCallback();
		testBoolIntCallback();
		testBoolLongCallback();
		testSequenceCallback();
		testAssetBundleCallback();
		testAssetBundleBytesCallback();
		testAssetRefLoadCallback();
		testSceneScriptCallback();
		testAtlasPtrCallback();
		testUGUIObjectCallback();
		testBytesStringCallback();
		testClassObjectCallback();
		testLerpCallback();
		testCommandCallback();
		testDragEndCallback();
		testDragStartCallback();
		testCharacterCallback();
		testGameObjectCallback();
		testCheckCallback();
		testKeyCodeCallback();
		testNetStateCallback();
		testAudioInfoCallback();
		testFloat2Callback();
		testFloatStringParam();
		testStringBoolCallback();
		testFloatBoolCallback();
		testGameLayoutCallback();
		testDragHoverCallback();
		testHeadDownloadCallback();
		testDownloadingCallback();
		testLayoutScriptCallback();
		testCreateObjectGroupCallback();
		testGameEffectCallback();
		testNetConnectTCPCallback();
		testImageCallback();
		testImageAnimCallback();
		testEncryptPacket();
		testDecryptPacket();
		testCharacterStateCallback();
		testLocalizationCallback();
		testString3Callback();
		testRecordCallback();
		testReceiveDragCallback();
		testStateLeaveCallback();
		testReloadLanguageCallback();
	}

	// 辅助: 断言 delegate 类型存在且是委托
	static void checkDelegate(string name, int paramCount)
	{
		Type t = Type.GetType(name + ", Assembly-CSharp");
		if (t == null)
		{
			// 热更程序集名不同时用程序集扫描
			t = findTypeByName(name);
		}
		assertNotNull(t, name + " 类型存在");
		assertTrue(typeof(Delegate).IsAssignableFrom(t), name + " 是委托类型");
		MethodInfo invoke = t.GetMethod("Invoke");
		assertNotNull(invoke, name + " 有 Invoke 方法");
		assertEqual(paramCount, invoke.GetParameters().Length, name + " 参数数量 " + paramCount);
	}

	static Type findTypeByName(string name)
	{
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			Type t = asm.GetType(name, false);
			if (t != null)
			{
				return t;
			}
		}
		return null;
	}

	// ─── 各 delegate 签名 ────────────────────────────────────────────

	static void testIntCallback()
	{
		checkDelegate("IntCallback", 1);
	}

	static void testBoolFunction()
	{
		checkDelegate("BoolFunction", 0);
	}

	static void testString2Callback()
	{
		checkDelegate("String2Callback", 2);
	}

	static void testVector3Callback()
	{
		checkDelegate("Vector3Callback", 1);
	}

	static void testIntBoolCallback()
	{
		checkDelegate("IntBoolCallback", 2);
	}

	static void testRefBoolCallback()
	{
		checkDelegate("RefBoolCallback", 1);
	}

	static void testFloat3Callback()
	{
		checkDelegate("Float3Callback", 3);
	}

	static void testPredicate2()
	{
		checkDelegate("Predicate2`2", 2);
	}

	static void testLongCallback()
	{
		checkDelegate("LongCallback", 1);
	}

	// ═════════════════════════════════════════════════════════════════
	// Batch 2
	// ═════════════════════════════════════════════════════════════════

	static void testBoolBoolCallback()
	{
		checkDelegate("BoolBoolCallback", 2);
	}

	static void testFloatCallback()
	{
		checkDelegate("FloatCallback", 1);
	}

	static void testStringIntCallback()
	{
		checkDelegate("StringIntCallback", 2);
	}

	static void testKeyFrameCallback()
	{
		checkDelegate("KeyFrameCallback", 2);
	}

	static void testGameEventCallback()
	{
		checkDelegate("GameEventCallback", 1);
	}

	static void testHttpCallback()
	{
		checkDelegate("HttpCallback", 3);   // (string result, WebExceptionStatus status, HttpStatusCode code)
	}

	static void testAssetLoadCallback()
	{
		checkDelegate("AssetLoadCallback", 4);   // (UObject asset, UObject[] assets, byte[] bytes, string loadPath)
	}

	static void testLogCallback()
	{
		checkDelegate("LogCallback", 4);   // (string time, string info, LOG_LEVEL level, bool isError)
	}

	static void testVector3IntCallback()
	{
		checkDelegate("Vector3IntCallback", 2);
	}

	static void testDragCallback()
	{
		checkDelegate("DragCallback", 2);   // (ComponentOwner dragObj, Vector3 pos)
	}

	static void testScrollItemCallback()
	{
		checkDelegate("ScrollItemCallback", 2);   // (IScrollItem item, int index)
	}

	// ═════════════════════════════════════════════════════════════════
	// Batch 5
	// ═════════════════════════════════════════════════════════════════

	static void testStringArrayCallback()
	{
		checkDelegate("StringArrayCallback", 1);   // (string[] lines)
	}

	static void testBoolIntCallback()
	{
		checkDelegate("BoolIntCallback", 2);   // (bool value0, int value1)
	}

	static void testBoolLongCallback()
	{
		checkDelegate("BoolLongCallback", 2);   // (bool value0, long value1)
	}

	static void testSequenceCallback()
	{
		checkDelegate("SequenceCallback", 2);   // (COMTransformableSequence com, bool isBreak)
	}

	static void testAssetBundleCallback()
	{
		checkDelegate("AssetBundleCallback", 1);   // (AssetBundleInfo assetBundle)
	}

	static void testAssetBundleBytesCallback()
	{
		checkDelegate("AssetBundleBytesCallback", 2);   // (AssetBundleInfo assetBundle, byte[] bytes)
	}

	static void testAssetRefLoadCallback()
	{
		checkDelegate("AssetRefLoadCallback`1", 4);   // (ResourceRef<T>, UObject[], byte[], string)
	}

	static void testSceneScriptCallback()
	{
		checkDelegate("SceneScriptCallback", 1);   // (SceneInstance instance)
	}

	static void testAtlasPtrCallback()
	{
		checkDelegate("AtlasPtrCallback", 1);   // (AtlasRef atlas)
	}

	static void testUGUIObjectCallback()
	{
		checkDelegate("UGUIObjectCallback", 1);   // (myUGUIObject window)
	}

	// ═════════════════════════════════════════════════════════════════
	// Batch 6
	// ═════════════════════════════════════════════════════════════════

	static void testBytesStringCallback()
	{
		checkDelegate("BytesStringCallback", 2);   // (byte[] bytes, string value)
	}

	static void testClassObjectCallback()
	{
		checkDelegate("ClassObjectCallback", 1);   // (ClassObject owner)
	}

	static void testLerpCallback()
	{
		checkDelegate("LerpCallback", 2);   // (ComponentLerp com, bool breakLerp)
	}

	static void testCommandCallback()
	{
		checkDelegate("CommandCallback", 1);   // (Command cmd)
	}

	static void testDragEndCallback()
	{
		checkDelegate("DragEndCallback", 3);   // (ComponentOwner dragObj, Vector3 pos, bool cancel)
	}

	static void testDragStartCallback()
	{
		checkDelegate("DragStartCallback", 3);   // (ComponentOwner dragObj, TouchPoint touchPoint, ref bool allowDrag)
	}

	static void testCharacterCallback()
	{
		checkDelegate("CharacterCallback", 1);   // (Character character)
	}

	static void testGameObjectCallback()
	{
		checkDelegate("GameObjectCallback", 1);   // (GameObject go)
	}

	static void testCheckCallback()
	{
		checkDelegate("CheckCallback", 1);   // (UGUICheckbox checkbox)
	}

	static void testKeyCodeCallback()
	{
		checkDelegate("KeyCodeCallback", 1);   // (KeyCode key)
	}

	static void testNetStateCallback()
	{
		checkDelegate("NetStateCallback", 2);   // (NET_STATE state, NET_STATE lastState)
	}

	static void testAudioInfoCallback()
	{
		checkDelegate("AudioInfoCallback", 1);   // (AudioInfo info)
	}

	static void testFloat2Callback()
	{
		checkDelegate("Float2Callback", 2);   // (float value0, float value1)
	}

	static void testFloatStringParam()
	{
		checkDelegate("FloatStringParam", 2);   // (float floatParam, string stringParam)
	}

	static void testStringBoolCallback()
	{
		checkDelegate("StringBoolCallback", 2);   // (string str, bool value)
	}

	// ═════════════════════════════════════════════════════════════════
	// Batch 8
	// ═════════════════════════════════════════════════════════════════

	static void testFloatBoolCallback()
	{
		checkDelegate("FloatBoolCallback", 2);   // (float progress, bool done)
	}

	static void testGameLayoutCallback()
	{
		checkDelegate("GameLayoutCallback", 1);   // (GameLayout layout)
	}

	static void testDragHoverCallback()
	{
		checkDelegate("DragHoverCallback", 3);   // (IMouseEventCollect dragObj, Vector3 touchPos, bool hover)
	}

	static void testHeadDownloadCallback()
	{
		checkDelegate("HeadDownloadCallback", 2);   // (Texture head, string openID)
	}

	static void testDownloadingCallback()
	{
		checkDelegate("DownloadingCallback", 3);   // (string fileName, long fileSize, long downloadedSize)
	}

	static void testLayoutScriptCallback()
	{
		checkDelegate("LayoutScriptCallback", 1);   // (LayoutScript script)
	}

	static void testCreateObjectGroupCallback()
	{
		checkDelegate("CreateObjectGroupCallback", 1);   // (Dictionary<string, GameObject> go)
	}

	static void testGameEffectCallback()
	{
		checkDelegate("GameEffectCallback", 1);   // (GameEffect effect)
	}

	static void testNetConnectTCPCallback()
	{
		checkDelegate("NetConnectTCPCallback", 1);   // (NetConnectTCP client)
	}

	static void testImageCallback()
	{
		checkDelegate("ImageCallback", 1);   // (myUGUIImage image)
	}

	static void testImageAnimCallback()
	{
		checkDelegate("ImageAnimCallback", 1);   // (myUGUIImageAnim imageAnim)
	}

	static void testEncryptPacket()
	{
		checkDelegate("EncryptPacket", 4);   // (byte[] data, int offset, int length, byte param)
	}

	static void testDecryptPacket()
	{
		checkDelegate("DecryptPacket", 4);   // (byte[] data, int offset, int length, byte param)
	}

	static void testCharacterStateCallback()
	{
		checkDelegate("CharacterStateCallback", 1);   // (CharacterState state)
	}

	static void testLocalizationCallback()
	{
		checkDelegate("LocalizationCallback", 3);   // (IUGUIText textObj, string localizedText, List<string> localizedParams)
	}

	static void testString3Callback()
	{
		checkDelegate("String3Callback", 3);   // (string str0, string str1, string str2)
	}

	static void testRecordCallback()
	{
		checkDelegate("RecordCallback", 2);   // (short[] data, int dataCount)
	}

	static void testReceiveDragCallback()
	{
		checkDelegate("ReceiveDragCallback", 3);   // (IMouseEventCollect dragObj, Vector3 touchPos, ref bool continueEvent)
	}

	static void testStateLeaveCallback()
	{
		checkDelegate("StateLeaveCallback", 4);   // (CharacterState state, bool isBreak, bool willDestroy, string param)
	}

	static void testReloadLanguageCallback()
	{
		checkDelegate("ReloadLanguageCallback", 3);   // (string languageType, Dictionary<string,string>, Dictionary<int,string>)
	}
}
