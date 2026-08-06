using System;
using static TestAssert;

// InputSystem 单元测试
// 覆盖可脱离真实输入设备/框架环境的纯逻辑:
//   setMask / hasMask 焦点掩码判断
//   registeInputField / unregisteInputField 输入框登记
//   KeyListenInfo.resetProperty (ClassObject)
// 注: 依赖真实按键/触点的 update 逻辑需运行时, 不在此覆盖
public static class InputSystemTest
{
	public static void Run()
	{
		// ─── FOCUS_MASK 掩码 ───
		testDefaultMask();
		testSetMaskScene();
		testSetMaskUI();
		testSetMaskBoth();
		testMaskNoneAlwaysTrue();
		testMaskNoneWhenMaskZero();
		testMaskNonOverlap();
		// ─── 输入框登记 ───
		testRegisteUnregisteInputField();
		// ─── KeyListenInfo ───
		testKeyListenInfoDefault();
		testKeyListenInfoReset();
	}

	// ═════════════════════════════════════════════════════════════════
	// FOCUS_MASK
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultMask()
	{
		InputSystem input = new();
		// hasMask: mask==NONE || mFocusMask==0 || (mFocusMask&mask)!=0
		// 默认 mFocusMask=0, 因 mFocusMask==0 分支, 任何掩码都返回 true
		assertTrue(input.hasMask(FOCUS_MASK.NONE), "NONE 掩码始终 true");
		assertTrue(input.hasMask(FOCUS_MASK.SCENE), "mask=0 时 SCENE 为 true (mFocusMask==0 分支)");
		assertTrue(input.hasMask(FOCUS_MASK.UI), "mask=0 时 UI 为 true (mFocusMask==0 分支)");
	}
	private static void testSetMaskScene()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.SCENE);
		assertTrue(input.hasMask(FOCUS_MASK.SCENE), "set SCENE 后 hasMask(SCENE) true");
		assertFalse(input.hasMask(FOCUS_MASK.UI), "仅 SCENE 时 UI false");
	}
	private static void testSetMaskUI()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.UI);
		assertTrue(input.hasMask(FOCUS_MASK.UI), "set UI 后 hasMask(UI) true");
		assertFalse(input.hasMask(FOCUS_MASK.SCENE), "仅 UI 时 SCENE false");
	}
	private static void testSetMaskBoth()
	{
		InputSystem input = new();
		input.setMask((FOCUS_MASK)((int)FOCUS_MASK.SCENE | (int)FOCUS_MASK.UI));
		assertTrue(input.hasMask(FOCUS_MASK.SCENE));
		assertTrue(input.hasMask(FOCUS_MASK.UI));
	}
	private static void testMaskNoneAlwaysTrue()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.SCENE);
		assertTrue(input.hasMask(FOCUS_MASK.NONE), "NONE 掩码始终 true, 与当前 mask 无关");
	}
	private static void testMaskNoneWhenMaskZero()
	{
		InputSystem input = new();
		// mask 不为 NONE, 但当前掩码为0 → 因 mFocusMask==0 分支返回 true
		assertTrue(input.hasMask(FOCUS_MASK.UI), "mFocusMask==0 时非 NONE 掩码 true");
	}
	private static void testMaskNonOverlap()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.SCENE);
		// UI(1<<2) 与 SCENE(1<<1) 不重叠
		assertFalse(input.hasMask(FOCUS_MASK.UI));
	}

	// ═════════════════════════════════════════════════════════════════
	// 输入框登记
	// ═════════════════════════════════════════════════════════════════
	private static void testRegisteUnregisteInputField()
	{
		InputSystem input = new();
		TestInputField field = new();
		input.registeInputField(field);
		// 无 getter 直接验证, 但 unregiste 不应抛异常(存在即登记成功)
		input.unregisteInputField(field);
		// 重复移除不应抛异常
		input.unregisteInputField(field);
	}

	// ═════════════════════════════════════════════════════════════════
	// KeyListenInfo
	// ═════════════════════════════════════════════════════════════════
	private static void testKeyListenInfoDefault()
	{
		KeyListenInfo info = new();
		assertNull(info.mCallback);
		assertNull(info.mListener);
		assertEqual(COMBINATION_KEY.NONE, info.mCombinationKey);
		assertEqual(UnityEngine.KeyCode.None, info.mKey);
	}
	private static void testKeyListenInfoReset()
	{
		KeyListenInfo info = new();
		info.mCallback = () => { };
		info.mListener = new TestEventListener();
		info.mCombinationKey = COMBINATION_KEY.CTRL;
		info.mKey = UnityEngine.KeyCode.A;
		info.resetProperty();
		assertNull(info.mCallback, "reset 后回调清空");
		assertNull(info.mListener, "reset 后监听者清空");
		assertEqual(COMBINATION_KEY.NONE, info.mCombinationKey, "reset 后组合键 NONE");
		assertEqual(UnityEngine.KeyCode.None, info.mKey, "reset 后按键 None");
	}
}

// 测试用 IInputField 实现
public class TestInputField : IInputField
{
	public bool isFocused() { return false; }
	public bool isVisible() { return true; }
}

// 测试用 IEventListener 实现
public class TestEventListener : IEventListener
{
	public void resetProperty() { }
}
