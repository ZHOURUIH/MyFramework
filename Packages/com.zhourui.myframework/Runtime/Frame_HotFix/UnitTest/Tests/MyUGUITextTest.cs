using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIText 深度测试(基于 MicroLegend 真实最高频用法: setText 788 次调用)
// 覆盖全部纯逻辑方法:
//   init(预加 Text 跳过 logError) / setText(string/int/long/null) / 相同文本不重复赋值
//   setText(preferredHeight=true) → applyPreferredHeight
//   applyPreferredWidth / applyPreferredHeight / applyPreferredHeightKeepTop(y 补偿)
//   cull / isCulled / canGenerateDepth(CanvasGroup alpha) / setAlpha/getAlpha / setColor/getColor
//   setFontSize/getFontSize / getTextComponent / getPreferredWidth/Height(相对断言)
// 附带 myUGUIObject.setActive 返回值语义(恒等入参, 文档化):
//   UIAttackTarget 用 setActive 返回值判断"是否变化", 但真实实现返回入参本身(恒等)
public static class MyUGUITextTest
{
	public static void Run()
	{
		testInitPrefabTextSkipsLogError();
		testSetTextString();
		testSetTextSameNoChange();
		testSetTextNullEmpty();
		testSetTextInt();
		testSetTextLong();
		testSetTextPreferredHeight();
		testApplyPreferredWidthHeight();
		testApplyPreferredHeightKeepTop();
		testCull();
		testAlphaColorFont();
		testGetTextComponent();
		testSetActiveReturnValue();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myUGUIText(预加 Text 组件跳过 init 的 logError 分支)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIText createText(out GameObject go)
	{
		go = new GameObject("TestText");
		go.AddComponent<RectTransform>();
		go.AddComponent<Text>();
		myUGUIText text = new myUGUIText();
		text.setObject(go);
		text.init();
		return text;
	}

	// init: 预加 Text → TryGetComponent 命中 → 不 logError, mText 有效
	private static void testInitPrefabTextSkipsLogError()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			assertNotNull(text.getTextComponent(), "init 后 mText 非 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(string): 设置并读回
	private static void testSetTextString()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setText("Hello");
			assertEqual("Hello", text.getText(), "setText 后 getText 一致");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 相同文本: 内部 mText.text != text 判断 → 不重复赋值(读回不变)
	private static void testSetTextSameNoChange()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setText("Same");
			text.setText("Same");
			assertEqual("Same", text.getText(), "重复 setText 相同文本无副作用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(null): 置为 EMPTY 空串
	private static void testSetTextNullEmpty()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setText(null);
			assertEqual("", text.getText(), "setText(null) 置空串");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(int): IToS 转换
	private static void testSetTextInt()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setText(123);
			assertEqual("123", text.getText(), "setText(int) 转字符串");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(long): LToS 转换
	private static void testSetTextLong()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setText(9876543210L);
			assertEqual("9876543210", text.getText(), "setText(long) 转字符串");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setText(text, preferredHeight=true): 触发 applyPreferredHeight
	private static void testSetTextPreferredHeight()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setText("Hello", true);
			assertEqual(text.getPreferredHeight(), text.getSize().y, 0.01f, "setText(preferredHeight) 后高度=preferredHeight(相对断言)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// applyPreferredWidth/Height: 尺寸更新为 preferred(相对断言, 不依赖字体)
	private static void testApplyPreferredWidthHeight()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.applyPreferredWidth();
			assertEqual(text.getPreferredWidth(), text.getSize().x, 0.01f, "applyPreferredWidth 后宽度=preferredWidth");
			text.applyPreferredHeight();
			assertEqual(text.getPreferredHeight(), text.getSize().y, 0.01f, "applyPreferredHeight 后高度=preferredHeight");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// applyPreferredHeightKeepTop: 高度变化后 Y 补偿 (old-new)*0.5
	private static void testApplyPreferredHeightKeepTop()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setSize(new Vector2(200.0f, 50.0f));
			text.setPosition(new Vector3(0.0f, 100.0f, 0.0f));
			float oldHeight = text.getSize().y;
			text.applyPreferredHeightKeepTop();
			float newHeight = text.getSize().y;
			float expectedY = 100.0f + (oldHeight - newHeight) * 0.5f;
			assertEqual(expectedY, text.getPosition().y, 0.01f, "keepTop 后 Y 补偿 (old-new)*0.5");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// cull: CanvasGroup alpha 控制剔除
	private static void testCull()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			assertFalse(text.isCulled(), "初始不剔除");
			assertTrue(text.canGenerateDepth(), "初始可生成深度");
			text.cull(true);
			assertTrue(text.isCulled(), "cull(true) 后剔除");
			assertFalse(text.canGenerateDepth(), "剔除后不可生成深度");
			text.cull(false);
			assertFalse(text.isCulled(), "cull(false) 恢复");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// alpha/color/fontSize 读写
	private static void testAlphaColorFont()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			text.setAlpha(0.5f);
			assertEqual(0.5f, text.getAlpha(), 0.001f, "setAlpha 写入 mText.color.a");
			Color color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			text.setColor(color);
			assertTrue(color == text.getColor(), "setColor 写入 mText.color");
			text.setFontSize(20);
			assertEqual(20, text.getFontSize(), "setFontSize 写入 mText.fontSize");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getTextComponent: 返回底层 Text 组件
	private static void testGetTextComponent()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			Text t = text.getTextComponent();
			assertNotNull(t, "getTextComponent 非 null");
			assertTrue(ReferenceEquals(t, go.GetComponent<Text>()), "getTextComponent 是预加的 Text");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setActive 返回值: 恒等入参(文档化真实行为)
	// UIAttackTarget.update 用 setActive 返回值当"是否变化"判断, 但实现返回入参本身
	private static void testSetActiveReturnValue()
	{
		myUGUIText text = createText(out GameObject go);
		try
		{
			bool r1 = text.setActive(true);
			assertTrue(r1, "setActive(true) 返回 true");
			assertTrue(text.isActive(), "isActive()==true");
			// 已是 true 再次 setActive(true) → 仍返回 true(恒等入参, 非"是否变化")
			bool r2 = text.setActive(true);
			assertTrue(r2, "重复 setActive(true) 仍返回 true(文档化: 返回值恒等入参)");
			bool r3 = text.setActive(false);
			assertFalse(r3, "setActive(false) 返回 false");
			assertFalse(text.isActive(), "isActive()==false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

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
}
