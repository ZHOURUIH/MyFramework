using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIText + myUGUITextAuto 补充测试:
//   myUGUIText: getFont / setAlignment(TextAnchor)
//   myUGUITextAuto: getFont / getTMPFont(无 TMP 时 null) / getTextProComponent(null)
//                   setAlignment(TextAnchor) / setTextWithPreferredWidth/Height(设置文本+自适应)
public static class MyUGUITextComplementTest
{
	public static void Run()
	{
		testTextGetFont();
		testTextSetAlignment();
		testTextAutoGetFont();
		testTextAutoTmpNull();
		testTextAutoSetAlignment();
		testTextAutoPreferredSize();
	}

	// ═════════════════════════════════════════════════════════════════
	// myUGUIText
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createText(out myUGUIText text)
	{
		GameObject go = new GameObject("Text");
		go.AddComponent<RectTransform>();
		go.AddComponent<Text>();
		text = new myUGUIText();
		text.setObject(go);
		text.init();
		return go;
	}

	// getFont: 返回 Text 组件的 font(默认字体非 null)
	private static void testTextGetFont()
	{
		GameObject go = createText(out myUGUIText text);
		try
		{
			Text comp = go.GetComponent<Text>();
			assertTrue(ReferenceEquals(comp.font, text.getFont()), "getFont 返回 Text 组件的字体");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAlignment: 写入 Text.alignment
	private static void testTextSetAlignment()
	{
		GameObject go = createText(out myUGUIText text);
		try
		{
			Text comp = go.GetComponent<Text>();
			text.setAlignment(TextAnchor.UpperRight);
			assertTrue(TextAnchor.UpperRight == comp.alignment, "setAlignment(UpperRight) 写入");
			text.setAlignment(TextAnchor.MiddleCenter);
			assertTrue(TextAnchor.MiddleCenter == comp.alignment, "setAlignment(MiddleCenter) 写入");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// myUGUITextAuto(预加 Text, 无 TMP)
	// ═════════════════════════════════════════════════════════════════
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

	// getFont: 有 Text 组件时返回其字体
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

	// 无 TMP 组件时: getTMPFont/getTextProComponent 返回 null
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

	// setAlignment(TextAnchor): 写入 Text.alignment
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

	// setTextWithPreferredWidth/Height: 设置文本 + 按 preferred 自适应(守卫: 文本生效+尺寸非负)
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
}
