using UnityEngine;
using static TestAssert;

// UGUICheckbox 深度测试(复选框控件)
// 状态机: setChecked 写勾选态 / isChecked 读勾选态(走 mMark 的真实 GameObject 激活状态)
//   onCheckClick: 点击翻转勾选态 + 触发回调; setInteractable(false) 时点击被拦截
// 测试环境: TestUGUICheckbox(传 null parent, 不调 init/assignWindow, 绕开场景节点查找与全局注册)
//   mMark 用真实 myUGUIObject(setObject+init 的裸节点), 勾选态走真实 Transformable 路径
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class UGUICheckboxTest
{
	public static void Run()
	{
		testSetChecked();
		testOnCheckClickToggle();
		testCheckCallback();
		testInteractableBlock();
		testInteractableGetSet();
		testLabelNullSafe();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建复选框 + 已 init 的勾选节点
	// ═════════════════════════════════════════════════════════════════
	private static TestUGUICheckbox createCheckbox(out GameObject markGo)
	{
		// parent 不能传 null: WindowObjectFixedT 构造里 mScript.addWindowObject(this) 会 NRE
		// 传一个未绑节点的 TestLayoutScriptDeep 满足 mScript 非 null 即可(不调 assignWindow/init)
		TestUGUICheckbox box = new TestUGUICheckbox(new TestLayoutScriptDeep());
		markGo = new GameObject("MarkGO");
		myUGUIObject mark = new myUGUIObject();
		mark.setObject(markGo);
		mark.init();
		box.setMarkForTest(mark);
		return box;
	}

	// setChecked / isChecked 读写
	// 注意: mark 节点默认 active(setObject 同步 mActive=activeSelf) → 初始即"勾选", 不假设初始状态
	private static void testSetChecked()
	{
		TestUGUICheckbox box = createCheckbox(out GameObject markGo);
		try
		{
			box.setChecked(true);
			assertTrue(box.isChecked(), "setChecked(true) 后已勾选");
			box.setChecked(false);
			assertTrue(!box.isChecked(), "setChecked(false) 后未勾选");
			box.setChecked(true);
			assertTrue(box.isChecked(), "再次 setChecked(true) 后已勾选");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(markGo);
		}
	}

	// onCheckClick: 点击翻转勾选态
	private static void testOnCheckClickToggle()
	{
		TestUGUICheckbox box = createCheckbox(out GameObject markGo);
		try
		{
			box.setChecked(true);
			box.onCheckClickForTest();
			assertTrue(!box.isChecked(), "勾选中点击 → 取消勾选");
			box.onCheckClickForTest();
			assertTrue(box.isChecked(), "未勾选中点击 → 重新勾选");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(markGo);
		}
	}

	// setCheckCallback: 点击翻转时触发回调(收到 this)
	private static void testCheckCallback()
	{
		TestUGUICheckbox box = createCheckbox(out GameObject markGo);
		try
		{
			int count = 0;
			UGUICheckbox callbackBox = null;
			box.setCheckCallback((b) => { ++count; callbackBox = b; });
			box.onCheckClickForTest();
			assertEqual(1, count, "点击触发回调");
			assertTrue(ReferenceEquals(box, callbackBox), "回调收到同一复选框对象");
			box.onCheckClickForTest();
			assertEqual(2, count, "再次点击再次触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(markGo);
		}
	}

	// setInteractable(false): 点击被拦截, 不翻转不回调
	private static void testInteractableBlock()
	{
		TestUGUICheckbox box = createCheckbox(out GameObject markGo);
		try
		{
			int count = 0;
			box.setCheckCallback((b) => ++count);
			box.setChecked(true);
			box.setInteractable(false);
			box.onCheckClickForTest();
			assertTrue(box.isChecked(), "不可交互时点击不翻转");
			assertEqual(0, count, "不可交互时点击不触发回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(markGo);
		}
	}

	// setInteractable/isInteractable 读写
	private static void testInteractableGetSet()
	{
		TestUGUICheckbox box = createCheckbox(out GameObject markGo);
		try
		{
			assertTrue(box.isInteractable(), "默认可交互");
			box.setInteractable(false);
			assertTrue(!box.isInteractable(), "setInteractable(false) 读回");
			box.setInteractable(true);
			assertTrue(box.isInteractable(), "setInteractable(true) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(markGo);
		}
	}

	// mLabel 未绑定(null)时 setLabel 安全, getLabelObject 返回 null
	private static void testLabelNullSafe()
	{
		TestUGUICheckbox box = createCheckbox(out GameObject markGo);
		try
		{
			box.setLabel("标签文字");   // mLabel 为 null, 空安全不崩
			assertTrue(box.getLabelObject() == null, "未绑定 Label 节点时返回 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(markGo);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 UGUICheckbox 的 protected 字段与方法
// ═════════════════════════════════════════════════════════════════
public class TestUGUICheckbox : UGUICheckbox
{
	public TestUGUICheckbox(IWindowObjectOwner parent) : base(parent) { }

	public void setMarkForTest(myUGUIObject mark) { mMark = mark; }

	public void onCheckClickForTest() { onCheckClick(); }
}
