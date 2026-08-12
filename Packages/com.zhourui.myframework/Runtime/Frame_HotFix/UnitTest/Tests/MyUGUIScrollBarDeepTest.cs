using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIScrollBar 深度测试(UGUI Scrollbar 封装行为链):
//   init: 无 Scrollbar 组件时自动添加(isNewObject 不 logError), 有则直接绑定
//   setValue → UGUI Scrollbar.value(内部 clamp 到 [0,1]) → onValueChanged → 回调链
//   setCallBack: 回调替换(旧回调不再触发) / 未注册回调安全 / 同值不触发
// 环境: 裸 GameObject + RectTransform + myUGUIScrollBar(setObject+init)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class MyUGUIScrollBarDeepTest
{
	public static void Run()
	{
		testInitAutoAddScrollbar();
		testInitBindExisting();
		testSetGetValue();
		testCallbackChain();
		testCallbackReplace();
		testNoCallbackSafe();
		testSameValueNoCallback();
		testValueChangeSequence();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建已 init 的 myUGUIScrollBar
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIScrollBar createScrollBar(out GameObject go)
	{
		go = new GameObject("ScrollBarGO");
		go.AddComponent<RectTransform>();
		myUGUIScrollBar sb = new myUGUIScrollBar();
		sb.setIsNewObject(true);
		sb.setObject(go);
		sb.init();
		return sb;
	}

	// init: 无 Scrollbar 组件 + isNewObject=true → 自动 AddComponent
	private static void testInitAutoAddScrollbar()
	{
		GameObject go = new GameObject("ScrollBarAuto");
		go.AddComponent<RectTransform>();
		try
		{
			myUGUIScrollBar sb = new myUGUIScrollBar();
			sb.setIsNewObject(true);
			sb.setObject(go);
			sb.init();
			assertTrue(go.GetComponent<Scrollbar>() != null, "init 自动添加 Scrollbar 组件");
			assertEqual(0.0f, sb.getValue(), 0.001f, "init 后默认值 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// init: 预加 Scrollbar 组件 → 直接绑定同一组件
	private static void testInitBindExisting()
	{
		GameObject go = new GameObject("ScrollBarBind");
		go.AddComponent<RectTransform>();
		go.AddComponent<Scrollbar>();
		try
		{
			myUGUIScrollBar sb = new myUGUIScrollBar();
			sb.setObject(go);
			sb.init();
			bool called = false;
			sb.setCallBack((v, bar) => called = true);
			sb.setValue(0.5f);
			assertTrue(called, "绑定已有组件后回调链可用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setValue/getValue 读写(0/0.5/1 边界)
	private static void testSetGetValue()
	{
		myUGUIScrollBar sb = createScrollBar(out GameObject go);
		try
		{
			sb.setValue(0.5f);
			assertEqual(0.5f, sb.getValue(), 0.001f, "setValue(0.5) 读回");
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

	// 回调链: setCallBack → setValue 变化 → onValueChanged → 回调(新值 + this)
	private static void testCallbackChain()
	{
		myUGUIScrollBar sb = createScrollBar(out GameObject go);
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
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setCallBack 替换: 新回调生效, 旧回调不再触发
	private static void testCallbackReplace()
	{
		myUGUIScrollBar sb = createScrollBar(out GameObject go);
		try
		{
			int countA = 0;
			int countB = 0;
			sb.setCallBack((v, bar) => ++countA);
			sb.setValue(0.2f);
			assertEqual(1, countA, "旧回调首次触发");
			sb.setCallBack((v, bar) => ++countB);
			sb.setValue(0.6f);
			assertEqual(1, countA, "替换后旧回调不再触发");
			assertEqual(1, countB, "新回调触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 未注册回调时 setValue 安全
	private static void testNoCallbackSafe()
	{
		myUGUIScrollBar sb = createScrollBar(out GameObject go);
		try
		{
			sb.setValue(0.5f);   // 无回调, 空安全不崩
			sb.setValue(0.8f);
			assertEqual(0.8f, sb.getValue(), 0.001f, "无回调时值仍正常写入");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 同值不触发回调(Unity Scrollbar 语义: 值未变不派发 onValueChanged)
	private static void testSameValueNoCallback()
	{
		myUGUIScrollBar sb = createScrollBar(out GameObject go);
		try
		{
			int count = 0;
			sb.setCallBack((v, bar) => ++count);
			sb.setValue(0.3f);
			assertEqual(1, count, "首次设置触发");
			sb.setValue(0.3f);
			assertEqual(1, count, "同值不重复触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 多次变化: 每次触发, 最后一次值正确
	private static void testValueChangeSequence()
	{
		myUGUIScrollBar sb = createScrollBar(out GameObject go);
		try
		{
			int count = 0;
			float lastValue = -1.0f;
			sb.setCallBack((v, bar) => { ++count; lastValue = v; });
			sb.setValue(0.1f);
			sb.setValue(0.4f);
			sb.setValue(0.9f);
			assertEqual(3, count, "三次变化触发三次");
			assertEqual(0.9f, lastValue, 0.001f, "最后一次回调值 0.9");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
