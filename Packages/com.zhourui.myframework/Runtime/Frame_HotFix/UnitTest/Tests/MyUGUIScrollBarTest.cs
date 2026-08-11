using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIScrollBar 深度测试(Scrollbar 封装):
//   init(预加 Scrollbar) / setValue/getValue 读写
//   setCallBack → setValue 触发 onValueChanged → 回调链路(收到新值+this)
public static class MyUGUIScrollBarTest
{
	public static void Run()
	{
		testInitValue();
		testSetGetValue();
		testCallbackChain();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createScrollBar(out myUGUIScrollBar sb)
	{
		GameObject go = new GameObject("ScrollBar");
		go.AddComponent<RectTransform>();
		go.AddComponent<Scrollbar>();
		sb = new myUGUIScrollBar();
		sb.setObject(go);
		sb.init();
		return go;
	}

	// init 后默认值 0
	private static void testInitValue()
	{
		GameObject go = createScrollBar(out myUGUIScrollBar sb);
		try
		{
			assertEqual(0.0f, sb.getValue(), 0.001f, "init 后 value 默认 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setValue/getValue 读写
	private static void testSetGetValue()
	{
		GameObject go = createScrollBar(out myUGUIScrollBar sb);
		try
		{
			sb.setValue(0.7f);
			assertEqual(0.7f, sb.getValue(), 0.001f, "setValue(0.7) 读回");
			sb.setValue(0.0f);
			assertEqual(0.0f, sb.getValue(), 0.001f, "setValue(0) 读回");
			sb.setValue(1.0f);
			assertEqual(1.0f, sb.getValue(), 0.001f, "setValue(1) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 组合链路: setCallBack → setValue 触发 Scrollbar.onValueChanged → 回调(新值+this)
	private static void testCallbackChain()
	{
		GameObject go = createScrollBar(out myUGUIScrollBar sb);
		try
		{
			int count = 0;
			float lastValue = -1.0f;
			myUGUIScrollBar lastSb = null;
			sb.setCallBack((value, bar) => { ++count; lastValue = value; lastSb = bar; });
			sb.setValue(0.3f);
			assertEqual(1, count, "setValue 变化触发回调");
			assertEqual(0.3f, lastValue, 0.001f, "回调收到新值");
			assertTrue(ReferenceEquals(sb, lastSb), "回调收到同一 ScrollBar 对象");
			sb.setValue(0.3f);   // 值未变, 不触发
			assertEqual(1, count, "值未变不重复触发");
			sb.setValue(0.8f);
			assertEqual(2, count, "再次变化触发");
			assertEqual(0.8f, lastValue, 0.001f, "第二次回调值");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
