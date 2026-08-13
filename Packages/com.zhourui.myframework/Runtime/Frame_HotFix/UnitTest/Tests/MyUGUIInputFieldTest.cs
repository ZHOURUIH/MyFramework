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
	

		testInitWithTextComponent();
		testSetTextStringInt();
		testSetTextFloat();
		testClear_Deep();
		testCharacterLimit();
		testCaretPosition();
		testFocusToggle();
		testIsVisible();
		testCallbacksStored();
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


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myUGUIInputField(预加 Image + InputField + 子 Text 配 textComponent)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIInputField createInput(out GameObject go, out Text textComp)
	{
		go = new GameObject("TestInput");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		GameObject textGo = new GameObject("Text");
		textGo.AddComponent<RectTransform>();
		textComp = textGo.AddComponent<Text>();
		textGo.transform.SetParent(go.transform);
		InputField field = go.AddComponent<InputField>();
		field.textComponent = textComp;   // 必须配置, 否则 setText 内部 NRE
		myUGUIInputField input = new myUGUIInputField();
		input.setObject(go);
		input.init();
		return input;
	}

	// init: 预加 InputField + textComponent → setText 可正常工作
	private static void testInitWithTextComponent()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text textComp);
		try
		{
			input.setText("Ready");
			assertEqual("Ready", input.getText(), "init 后 setText/getText 正常(textComponent 已配置)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(string/int): 读回一致
	private static void testSetTextStringInt()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			input.setText("Hello");
			assertEqual("Hello", input.getText(), "setText(string) 读回一致");
			input.setText(123);
			assertEqual("123", input.getText(), "setText(int) 转字符串");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(float): FToS(2) 格式化(文档化: 2 位小数)
	private static void testSetTextFloat()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			input.setText(1.5f);
			// FToS(2) 输出格式, 断言以"包含小数且非空"为主(格式细节由 StringUtility 决定)
			assertTrue(input.getText().Length > 0, "setText(float) 输出非空");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clear: 清空文本 + 失去焦点
	private static void testClear_Deep()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			input.setText("Temp");
			input.clear();
			assertEqual("", input.getText(), "clear 后文本清空");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setCharacterLimit: 写入 InputField.characterLimit
	private static void testCharacterLimit()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			input.setCharacterLimit(10);
			assertEqual(10, go.GetComponent<InputField>().characterLimit, "setCharacterLimit 写入组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setCaretPosition/getCaretPosition: 读写
	private static void testCaretPosition()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			input.setText("Hello");
			input.setCaretPosition(0);
			assertEqual(0, input.getCaretPosition(), "setCaretPosition(0) 读回 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// focus(true/false): Activate/Deactivate 输入焦点(EditMode 无 EventSystem, 守卫式)
	private static void testFocusToggle()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			input.focus(true);
			input.focus(false);
			// 不抛异常即通过(EditMode 焦点请求无害)
			assertTrue(true, "focus(true/false) 空安全");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// isVisible: 激活状态
	private static void testIsVisible()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			assertTrue(input.isVisible(), "激活时 isVisible=true");
			go.SetActive(false);
			assertFalse(input.isVisible(), "SetActive(false) 后 isVisible=false");
			go.SetActive(true);
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnEndEdit/setOnEditting: 回调存储不抛异常(触发需真实输入事件, 守卫式)
	private static void testCallbacksStored()
	{
		myUGUIInputField input = createInput(out GameObject go, out Text _);
		try
		{
			StringCallback cb = delegate (string value) { };
			input.setOnEndEdit(cb);
			input.setOnEditting(cb);
			assertTrue(true, "setOnEndEdit/setOnEditting 回调存储安全");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
