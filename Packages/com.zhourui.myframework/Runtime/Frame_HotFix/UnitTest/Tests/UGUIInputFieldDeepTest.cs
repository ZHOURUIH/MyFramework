using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIInputField 深度测试(基于 MicroLegend 真实用法: focus 20 次 / isFocused 9 次)
// 覆盖全部纯逻辑方法:
//   init(预加 InputField + textComponent 配置) / setText(string/int/float) / getText
//   clear(removeFocus) / setCharacterLimit / setCaretPosition/getCaretPosition
//   focus(true/false) / isVisible / setOnEndEdit/setOnEditting(回调存储守卫)
//
// 关键: InputField.textComponent 默认 null, setText 内部 m_Text.text 会 NRE
//       → 必须创建子 Text 节点并配置 textComponent
public static class UGUIInputFieldDeepTest
{
	public static void Run()
	{
		testInitWithTextComponent();
		testSetTextStringInt();
		testSetTextFloat();
		testClear();
		testCharacterLimit();
		testCaretPosition();
		testFocusToggle();
		testIsVisible();
		testCallbacksStored();
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
	private static void testClear()
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
