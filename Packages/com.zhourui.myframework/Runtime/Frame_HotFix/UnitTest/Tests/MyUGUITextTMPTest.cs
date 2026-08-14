using static TestAssert;
using UnityEngine;
using TMPro;
using UObject = UnityEngine.Object;

// myUGUITextTMP: TextMeshPro 文本封装——文本/字体/颜色/透明度/对齐读写
// (本地化重载 setText(mainText, ILocalizationCollection) 依赖全局注册, 有残留风险, 不测)
public static class MyUGUITextTMPTest
{
	public static void Run()
	{
		testInitAddsTextMeshProUGUI();
		testSetGetText();
		testSetTextInt();
		testSetTextLong();
		testSetTextSameValue();
		testFontSize();
		testColor();
		testAlpha();
		testAlignment();
		testGetTextComponent();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// init: 无 TextMeshProUGUI 时 setIsNewObject(true) 自动 AddComponent
	private static void testInitAddsTextMeshProUGUI()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			assertNotNull(text.getTextComponent(), "init 后 getTextComponent 非 null");
			assertNotNull(go.GetComponent<TextMeshProUGUI>(), "GameObject 上已添加 TextMeshProUGUI");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setText(string)/getText 读写
	private static void testSetGetText()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setText("hello");
			assertEqual("hello", text.getText(), "setText(hello) 读回");
			text.setText("世界");
			assertEqual("世界", text.getText(), "setText(中文) 读回");
			text.setText("");
			assertEqual("", text.getText(), "setText(空) 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setText(int): IToS 转换
	private static void testSetTextInt()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setText(123);
			assertEqual("123", text.getText(), "setText(123) → 123");
			text.setText(-45);
			assertEqual("-45", text.getText(), "setText(-45) → -45");
			text.setText(0);
			assertEqual("0", text.getText(), "setText(0) → 0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setText(long): LToS 转换
	private static void testSetTextLong()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setText(1234567890123L);
			assertEqual("1234567890123", text.getText(), "setText(long) 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// 相同文本重复设置: 结果一致(内部有 mText.text != text 跳过)
	private static void testSetTextSameValue()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setText("same");
			text.setText("same");
			assertEqual("same", text.getText(), "重复设置相同文本结果一致");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// getFontSize/setFontSize
	private static void testFontSize()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setFontSize(30.0f);
			assertEqual(30.0f, text.getFontSize(), 0.001f, "setFontSize(30) 读回");
			text.setFontSize(12.5f);
			assertEqual(12.5f, text.getFontSize(), 0.001f, "setFontSize(12.5) 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setColor/getColor
	private static void testColor()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			Color red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			text.setColor(red);
			assertEqual(1.0f, text.getColor().r, 0.001f, "R 读回");
			assertEqual(0.0f, text.getColor().g, 0.001f, "G 读回");
			assertEqual(0.0f, text.getColor().b, 0.001f, "B 读回");
			assertEqual(1.0f, text.getColor().a, 0.001f, "A 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setAlpha/getAlpha: 透明度写入 color.a
	private static void testAlpha()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setAlpha(0.5f);
			assertEqual(0.5f, text.getAlpha(), 0.001f, "setAlpha(0.5) 读回");
			text.setAlpha(0.0f);
			assertEqual(0.0f, text.getAlpha(), 0.001f, "setAlpha(0) 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setAlignment: 写入 TMP alignment
	private static void testAlignment()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			text.setAlignment(TextAlignmentOptions.Center);
			assertEqual(TextAlignmentOptions.Center, text.getTextComponent().alignment, "Center 读回");
			text.setAlignment(TextAlignmentOptions.TopRight);
			assertEqual(TextAlignmentOptions.TopRight, text.getTextComponent().alignment, "TopRight 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// getTextComponent: 返回同一引用
	private static void testGetTextComponent()
	{
		myUGUITextTMP text = createTextTMP(out GameObject go);
		try
		{
			TextMeshProUGUI c = go.GetComponent<TextMeshProUGUI>();
			assertTrue(ReferenceEquals(c, text.getTextComponent()), "getTextComponent 返回 GameObject 上的组件");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static myUGUITextTMP createTextTMP(out GameObject go)
	{
		go = new GameObject("TextTMP");
		myUGUITextTMP text = new myUGUITextTMP();
		// 无 TextMeshProUGUI 组件时自动补组件, 避免 init 的 logError 分支
		text.setIsNewObject(true);
		text.setObject(go);
		text.init();
		return text;
	}
}
