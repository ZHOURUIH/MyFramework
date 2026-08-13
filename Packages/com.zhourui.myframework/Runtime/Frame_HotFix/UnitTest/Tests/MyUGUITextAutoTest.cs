using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUITextAuto 深度测试(自适应文本封装):
//   init: TryGetComponent<TextMeshProUGUI>/<Text>, 不自动补组件 → 双 null 时全部方法空安全
//   setText: mTextPro 优先, 其次 mText, 都 null 时静默不设置
//   applyPreferredWidth/Height: 读 preferred 尺寸 + extra 补偿 → setSize
//   getAlpha/getColor/getFontSize/getPreferredWidth/Height: 无组件时返回默认值
//   cull: mText 路径走 CanvasGroup.alpha(TMP 路径走 mTextPro.color, 测试不测)
// 环境: 裸 GameObject + RectTransform + myUGUITextAuto(setObject+init), 可选预加 Text 组件
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class MyUGUITextAutoTest
{
	public static void Run()
	{
		testInitNoComponents();
		testSetTextNoComponentSafe();
		testSetGetTextWithText();
		testSetTextIntLong();
		testSetTextWithPreferredWidth();
		testSetTextWithPreferredHeight();
		testApplyPreferredWidthNoComponent();
		testApplyPreferredHeightNoComponent();
		testColorAlphaWithText();
		testFontSizeWithText();
		testAlignmentWithText();
		testCullWithText();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建已 init 的 myUGUITextAuto(可选预加 Text 组件)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUITextAuto createText(out GameObject go, bool withTextComponent)
	{
		go = new GameObject("TextAutoGO");
		go.AddComponent<RectTransform>();
		if (withTextComponent)
		{
			go.AddComponent<Text>();
		}
		myUGUITextAuto text = new myUGUITextAuto();
		text.setObject(go);
		text.init();
		return text;
	}

	// init: 无组件 → 全部 getter 返回默认值
	private static void testInitNoComponents()
	{
		myUGUITextAuto text = createText(out GameObject go, false);
		try
		{
			assertEqual("", text.getText(), "无组件时 getText 返回空串");
			assertEqual(1.0f, text.getAlpha(), 0.001f, "无组件时 alpha 默认 1");
			assertEqual(Color.white, text.getColor(), "无组件时颜色默认白色");
			assertEqual(20.0f, text.getFontSize(), 0.001f, "无组件时字号默认 20");
			assertEqual(0.0f, text.getPreferredWidth(), 0.001f, "无组件时 preferredWidth 0");
			assertEqual(0.0f, text.getPreferredHeight(), 0.001f, "无组件时 preferredHeight 0");
			assertTrue(text.getTextComponent() == null, "无组件时 textComponent null");
			assertTrue(text.getTextProComponent() == null, "无组件时 textProComponent null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText 无组件: 静默不设置, 不崩溃
	private static void testSetTextNoComponentSafe()
	{
		myUGUITextAuto text = createText(out GameObject go, false);
		try
		{
			text.setText("hello");   // 双 null, 静默跳过
			assertEqual("", text.getText(), "无组件时 setText 静默无效");
			text.setAlpha(0.5f);     // 空安全
			text.setColor(Color.red);
			text.setFontSize(30.0f);
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 预加 Text: setText/getText 读写
	private static void testSetGetTextWithText()
	{
		myUGUITextAuto text = createText(out GameObject go, true);
		try
		{
			text.setText("hello");
			assertEqual("hello", text.getText(), "setText 读回");
			text.setText("新文本");
			assertEqual("新文本", text.getText(), "setText 覆盖读回");
			assertTrue(text.getTextComponent() != null, "Text 组件已绑定");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(int/long) 重载
	private static void testSetTextIntLong()
	{
		myUGUITextAuto text = createText(out GameObject go, true);
		try
		{
			text.setText(123);
			assertEqual("123", text.getText(), "setText(int) 转换");
			text.setText(123456789L);
			assertEqual("123456789", text.getText(), "setText(long) 转换");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setTextWithPreferredWidth: 设置文本 + 自适应宽度(无组件时 preferred=0, 只加 extra)
	private static void testSetTextWithPreferredWidth()
	{
		myUGUITextAuto text = createText(out GameObject go, false);
		try
		{
			text.setTextWithPreferredWidth("abc", 10.0f);
			// height = getSize().y = 100(RectTransform 默认 sizeDelta 100x100); preferredWidth = 0(无组件)
			// setSize((0+10, 100))
			assertEqual(10.0f, text.getSize().x, 0.001f, "宽度 = preferred(0) + extra(10)");
			assertEqual(100.0f, text.getSize().y, 0.001f, "高度保持原值(RectTransform 默认 100)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setTextWithPreferredHeight: 设置文本 + 自适应高度
	private static void testSetTextWithPreferredHeight()
	{
		myUGUITextAuto text = createText(out GameObject go, false);
		try
		{
			text.setTextWithPreferredHeight("abc", 10.0f);
			// width = getSize().x = 100(RectTransform 默认 100x100); preferredHeight = 0(无组件)
			// setSize((100, 0+10))
			assertEqual(100.0f, text.getSize().x, 0.001f, "宽度保持原值(RectTransform 默认 100)");
			assertEqual(10.0f, text.getSize().y, 0.001f, "高度 = preferred(0) + extra(10)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// applyPreferredWidth: 直接调用(无组件 preferred=0)
	private static void testApplyPreferredWidthNoComponent()
	{
		myUGUITextAuto text = createText(out GameObject go, false);
		try
		{
			text.applyPreferredWidth(0.0f, 5.0f);
			assertEqual(5.0f, text.getSize().x, 0.001f, "宽度 = 0 + extra(5)");
			text.applyPreferredWidth(0.0f, 8.0f);
			assertEqual(8.0f, text.getSize().x, 0.001f, "再次设置覆盖");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// applyPreferredHeight: 直接调用(无组件 preferred=0)
	private static void testApplyPreferredHeightNoComponent()
	{
		myUGUITextAuto text = createText(out GameObject go, false);
		try
		{
			text.applyPreferredHeight(0.0f, 5.0f);
			assertEqual(5.0f, text.getSize().y, 0.001f, "高度 = 0 + extra(5)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 预加 Text: setColor/getColor + setAlpha/getAlpha
	private static void testColorAlphaWithText()
	{
		myUGUITextAuto text = createText(out GameObject go, true);
		try
		{
			Color color = new Color(0.5f, 0.25f, 1.0f, 0.8f);
			text.setColor(color);
			Color got = text.getColor();
			assertEqual(color.r, got.r, 0.001f, "颜色 R");
			assertEqual(color.g, got.g, 0.001f, "颜色 G");
			assertEqual(color.b, got.b, 0.001f, "颜色 B");
			assertEqual(color.a, got.a, 0.001f, "颜色 A");
			text.setAlpha(0.4f);
			assertEqual(0.4f, text.getAlpha(), 0.001f, "setAlpha(0.4) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 预加 Text: setFontSize/getFontSize(Text 路径转 int)
	private static void testFontSizeWithText()
	{
		myUGUITextAuto text = createText(out GameObject go, true);
		try
		{
			text.setFontSize(18.0f);
			assertEqual(18.0f, text.getFontSize(), 0.001f, "setFontSize(18) 读回");
			text.setFontSize(12.5f);
			assertEqual(12.0f, text.getFontSize(), 0.001f, "Text 路径转 int 截断");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 预加 Text: setAlignment 写 Text.alignment
	private static void testAlignmentWithText()
	{
		myUGUITextAuto text = createText(out GameObject go, true);
		try
		{
			text.setAlignment(TextAnchor.MiddleCenter);
			assertTrue(go.GetComponent<Text>().alignment == TextAnchor.MiddleCenter, "对齐 MiddleCenter");
			text.setAlignment(TextAnchor.UpperRight);
			assertTrue(go.GetComponent<Text>().alignment == TextAnchor.UpperRight, "对齐 UpperRight");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 预加 Text: cull 走 CanvasGroup.alpha(Text 路径)
	private static void testCullWithText()
	{
		myUGUITextAuto text = createText(out GameObject go, true);
		try
		{
			text.cull(true);
			CanvasGroup group = go.GetComponent<CanvasGroup>();
			assertTrue(group != null, "cull 自动添加 CanvasGroup");
			assertEqual(0.0f, group.alpha, 0.001f, "cull(true) → alpha 0");
			text.cull(false);
			assertEqual(1.0f, group.alpha, 0.001f, "cull(false) → alpha 1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

private static void testTextAutoGetFont()
	{
		GameObject go = createTextAuto(out myUGUITextAuto text);
		try
		{
			Text comp = go.GetComponent<Text>();
			assertTrue(ReferenceEquals(comp.font, text.getFont()), "getFont 返回 Text 字体");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

private static void testTextAutoTmpNull()
	{
		GameObject go = createTextAuto(out myUGUITextAuto text);
		try
		{
			assertNull(text.getTMPFont(), "无 TMP 时 getTMPFont null");
			assertNull(text.getTextProComponent(), "无 TMP 时 getTextProComponent null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

private static void testTextAutoSetAlignment()
	{
		GameObject go = createTextAuto(out myUGUITextAuto text);
		try
		{
			Text comp = go.GetComponent<Text>();
			text.setAlignment(TextAnchor.LowerLeft);
			assertTrue(TextAnchor.LowerLeft == comp.alignment, "setAlignment(LowerLeft) 写入");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

private static void testTextAutoPreferredSize()
	{
		GameObject go = createTextAuto(out myUGUITextAuto text);
		try
		{
			text.setTextWithPreferredWidth("Hello World", 0.0f);
			assertEqual("Hello World", text.getText(), "setTextWithPreferredWidth 设置文本");
			assertTrue(text.getSize().x >= 0.0f, "preferredWidth 后宽度非负");
			text.setTextWithPreferredHeight("Hello", 0.0f);
			assertEqual("Hello", text.getText(), "setTextWithPreferredHeight 设置文本");
			assertTrue(text.getSize().y >= 0.0f, "preferredHeight 后高度非负");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

private static GameObject createTextAuto(out myUGUITextAuto text)
	{
		GameObject go = new GameObject("TextAuto");
		go.AddComponent<RectTransform>();
		go.AddComponent<Text>();
		text = new myUGUITextAuto();
		text.setObject(go);
		text.init();
		return go;
	}
}
