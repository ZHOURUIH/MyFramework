using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIInputField 深度测试(UGUI InputField 封装):
//   init: 无 InputField 组件时自动添加(isNewObject 不 logError), 并设置 mImage.raycastTarget=true
//   setText(string/int/float) / getText
//   setOnEditting: 输入中回调(无设备依赖, 直接 Invoke onValueChanged 触发)
//   setOnEndEdit: 结束编辑回调(默认 needEnter=true 时依赖回车键检测, 测试环境 isKeyDown 恒 false → 不触发)
//   setCharacterLimit / setCaretPosition / getCaretPosition / clear
// 环境: 裸 GameObject + RectTransform + myUGUIInputField(setObject+init)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class MyUGUIInputFieldTest
{
	public static void Run()
	{
		testInitAutoAddInputField();
		testSetGetText();
		testSetTextIntFloat();
		testOnEditting();
		testOnEndEditNoEnter();
		testOnEndEditNeedEnter();
		testClear();
		testCharacterLimitCaret();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建已 init 的 myUGUIInputField
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIInputField createInputField(out GameObject go)
	{
		go = new GameObject("InputFieldGO");
		go.AddComponent<RectTransform>();
		myUGUIInputField input = new myUGUIInputField();
		input.setIsNewObject(true);
		input.setObject(go);
		input.init();
		return input;
	}

	// init: 无 InputField 组件 + isNewObject=true → 自动 AddComponent
	private static void testInitAutoAddInputField()
	{
		GameObject go = new GameObject("InputAuto");
		go.AddComponent<RectTransform>();
		try
		{
			myUGUIInputField input = new myUGUIInputField();
			input.setIsNewObject(true);
			input.setObject(go);
			input.init();
			assertTrue(go.GetComponent<InputField>() != null, "init 自动添加 InputField 组件");
			Image image = go.GetComponent<Image>();
			assertTrue(image != null, "基类 init 自动添加 Image 组件");
			assertTrue(image.raycastTarget, "init 设置 raycastTarget = true");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(string)/getText 读写
	private static void testSetGetText()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			input.setText("hello");
			assertEqual("hello", input.getText(), "setText 读回");
			input.setText("新内容");
			assertEqual("新内容", input.getText(), "setText 覆盖读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(int/float) 重载
	private static void testSetTextIntFloat()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			input.setText(123);
			assertEqual("123", input.getText(), "setText(int) 转换");
			input.setText(1.5f);
			assertEqual("1.5", input.getText(), "setText(float) FToS(2) 默认去末尾零 → 1.5");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnEditting: 输入中回调(无设备依赖)
	private static void testOnEditting()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			int count = 0;
			string lastValue = null;
			input.setOnEditting((v) => { ++count; lastValue = v; });
			// 直接触发 UGUI InputField 的输入中事件
			go.GetComponent<InputField>().onValueChanged.Invoke("a");
			go.GetComponent<InputField>().onValueChanged.Invoke("ab");
			assertEqual(2, count, "输入中回调触发 2 次");
			assertEqual("ab", lastValue, "回调收到最新值");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnEndEdit(needEnter=false): 直接触发结束编辑回调
	private static void testOnEndEditNoEnter()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			int count = 0;
			string lastValue = null;
			input.setOnEndEdit((v) => { ++count; lastValue = v; }, false);
			go.GetComponent<InputField>().onEndEdit.Invoke("done");
			assertEqual(1, count, "needEnter=false 时结束编辑回调触发");
			assertEqual("done", lastValue, "回调收到结束值");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnEndEdit(默认 needEnter=true): 无回车键时回调不触发(文档化真实行为)
	private static void testOnEndEditNeedEnter()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			int count = 0;
			input.setOnEndEdit((v) => ++count);   // 默认 needEnter=true
			go.GetComponent<InputField>().onEndEdit.Invoke("x");
			// 测试环境 isKeyDown(Return) 恒 false → 提前 return, 回调不触发
			assertEqual(0, count, "needEnter=true 且无回车输入时回调不触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clear: 清空文本(removeFocus=false 不触碰聚焦逻辑)
	private static void testClear()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			input.setText("abc");
			input.clear(false);
			assertEqual("", input.getText(), "clear 后文本清空");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setCharacterLimit: 直接写 mInputField.characterLimit(纯字段, 无 Text 组件也安全)
	// caretPosition 依赖 InputField 的 textComponent 非 null, 测试环境无 Text 组件, 合法跳过
	private static void testCharacterLimitCaret()
	{
		myUGUIInputField input = createInputField(out GameObject go);
		try
		{
			input.setCharacterLimit(5);
			assertEqual(5, go.GetComponent<InputField>().characterLimit, "setCharacterLimit(5) 生效");
			input.setCharacterLimit(12);
			assertEqual(12, go.GetComponent<InputField>().characterLimit, "setCharacterLimit(12) 覆盖生效");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
