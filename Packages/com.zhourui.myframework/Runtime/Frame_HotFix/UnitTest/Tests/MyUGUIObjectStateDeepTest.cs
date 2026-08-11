using System;
using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIObject 状态 setter/getter 深度测试(纯逻辑守卫式)
//   setDestroyImmediately / setIsNewObject / setReceiveLayoutHide+isReceiveLayoutHide
//   setClickSound+getClickSound / setLongPressLengthThreshold+getLongPressLengthThreshold
//   setColliderForClick+isColliderForClick / setDepthOverAllChild+isDepthOverAllChild
//   setAllowGenerateDepth+isAllowGenerateDepth / setPassDragEvent(无 drag 组件恒 true 文档化)
//   setDepth+getDepth / setPressCallback+getPressCallback / setOnScreenTouchUp+getOnScreenTouchUp
//   setAlphaWithChild(递归 Graphic 组件色) / setSibling+getSibling / sortChild(内部列表按 index 排序)
//   addLongPress+clearLongPress+removeLongPress / notifyAnchorApply / refreshChildDepthByPositionZ
public static class MyUGUIObjectStateDeepTest
{
	public static void Run()
	{
		testStateFlags();
		testClickSound();
		testLongPressThreshold();
		testColliderForClick();
		testDepthFlags();
		testPassDragEvent();
		testDepth();
		testCallbackStorage();
		testAlphaWithChild();
		testSibling();
		testSortChild();
		testAddLongPress();
		testNotifyAnchorApply();
		testRefreshChildDepthByPositionZ();
	}

	// 暴露 protected addChild, 用于构造 mChildList 测试 sortChild
	public class TestUIObjectAccessor : myUGUIObject
	{
		public void exposeAddChild(myUGUIObject child, bool refreshDepth)
		{
			addChild(child, refreshDepth);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI(out GameObject go)
	{
		go = new GameObject("UIObject");
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// setDestroyImmediately/setIsNewObject 守卫式 + setReceiveLayoutHide 读回
	private static void testStateFlags()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setDestroyImmediately(true);   // 守卫: 无 getter, 调用不崩
			ui.setIsNewObject(true);          // 守卫: 无 getter, 调用不崩
			ui.setReceiveLayoutHide(true);
			assertTrue(ui.isReceiveLayoutHide(), "setReceiveLayoutHide(true) 读回");
			ui.setReceiveLayoutHide(false);
			assertFalse(ui.isReceiveLayoutHide(), "setReceiveLayoutHide(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setClickSound/getClickSound 写读
	private static void testClickSound()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setClickSound(7);
			assertEqual(7, ui.getClickSound(), "setClickSound(7) 读回");
			ui.setClickSound(0);
			assertEqual(0, ui.getClickSound(), "setClickSound(0) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setLongPressLengthThreshold/getLongPressLengthThreshold 写读
	private static void testLongPressThreshold()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setLongPressLengthThreshold(0.8f);
			assertEqual(0.8f, ui.getLongPressLengthThreshold(), 0.001f, "setLongPressLengthThreshold(0.8) 读回");
			ui.setLongPressLengthThreshold(0.0f);
			assertEqual(0.0f, ui.getLongPressLengthThreshold(), 0.001f, "重置 0 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setColliderForClick/isColliderForClick 往返
	private static void testColliderForClick()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setColliderForClick(true);
			assertTrue(ui.isColliderForClick(), "setColliderForClick(true) 读回");
			ui.setColliderForClick(false);
			assertFalse(ui.isColliderForClick(), "setColliderForClick(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setDepthOverAllChild/setAllowGenerateDepth 读回
	private static void testDepthFlags()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setDepthOverAllChild(true);
			assertTrue(ui.isDepthOverAllChild(), "setDepthOverAllChild(true) 读回");
			ui.setDepthOverAllChild(false);
			assertFalse(ui.isDepthOverAllChild(), "setDepthOverAllChild(false) 读回");
			ui.setAllowGenerateDepth(true);
			assertTrue(ui.isAllowGenerateDepth(), "setAllowGenerateDepth(true) 读回");
			ui.setAllowGenerateDepth(false);
			assertFalse(ui.isAllowGenerateDepth(), "setAllowGenerateDepth(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPassDragEvent: 无 COMWindowDrag 组件时 isPassDragEvent 恒 true(短路, 文档化)
	private static void testPassDragEvent()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setPassDragEvent(true);
			assertTrue(ui.isPassDragEvent(), "setPassDragEvent(true) → true");
			ui.setPassDragEvent(false);
			assertTrue(ui.isPassDragEvent(), "无 drag 组件时恒 true(!isDraggable 短路, 文档化)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setDepth/getDepth: orderInParent 写读
	private static void testDepth()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setDepth(new UIDepth(), 3);
			assertEqual(3, ui.getDepth().getOrderInParent(), "setDepth orderInParent=3 读回");
			ui.setDepth(new UIDepth(), 0);
			assertEqual(0, ui.getDepth().getOrderInParent(), "setDepth orderInParent=0 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPressCallback/setOnScreenTouchUp 回调存储读回(同一引用)
	private static void testCallbackStorage()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			BoolCallback press = (hover) => { };
			ui.setPressCallback(press);
			assertTrue(ReferenceEquals(press, ui.getPressCallback()), "setPressCallback 读回同一回调");
			Vector3IntCallback touchUp = (pos, id) => { };
			ui.setOnScreenTouchUp(touchUp);
			ComponentInteractive com = ui.getComponent<ComponentInteractive>();
			assertTrue(ReferenceEquals(touchUp, com.getOnScreenTouchUp()), "setOnScreenTouchUp 读回同一回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAlphaWithChild: 自身 getAlpha 恒 1.0(空实现), 子节点 Graphic 组件色被递归设置
	private static void testAlphaWithChild()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			GameObject childGo = new GameObject("Child");
			childGo.AddComponent<RectTransform>();
			childGo.AddComponent<Image>();
			childGo.transform.SetParent(go.transform);
			ui.setAlphaWithChild(0.3f);
			Image childImage = childGo.GetComponent<Image>();
			assertEqual(0.3f, childImage.color.a, 0.001f, "子节点 Image alpha 被递归设置");
			assertEqual(1.0f, ui.getAlpha(), 0.001f, "自身 getAlpha 恒 1.0(空实现文档化)");
			// 恢复 alpha 不影响
			ui.setAlphaWithChild(1.0f);
			assertEqual(1.0f, childImage.color.a, 0.001f, "恢复 1.0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSibling/getSibling: 移动兄弟索引 + 相同位置返回 false
	private static void testSibling()
	{
		GameObject parentGo = new GameObject("Parent");
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			myUGUIObject a = createChildUI("A", parentGo);
			myUGUIObject b = createChildUI("B", parentGo);
			myUGUIObject c = createChildUI("C", parentGo);
			// 初始 a(0) b(1) c(2)
			assertEqual(0, a.getSibling(), "初始 A index=0");
			assertEqual(1, b.getSibling(), "初始 B index=1");
			assertEqual(2, c.getSibling(), "初始 C index=2");
			assertTrue(a.setSibling(2, false), "setSibling(2) 返回 true");
			assertEqual(2, a.getSibling(), "A 移到 index=2");
			assertEqual(0, b.getSibling(), "B 变 index=0");
			assertFalse(a.setSibling(2, false), "相同位置返回 false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// sortChild: 按 sibling index 排序内部 mChildList
	private static void testSortChild()
	{
		GameObject parentGo = new GameObject("Parent");
		TestUIObjectAccessor parent = new TestUIObjectAccessor();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			myUGUIObject a = createChildUI("A", parentGo);
			myUGUIObject b = createChildUI("B", parentGo);
			myUGUIObject c = createChildUI("C", parentGo);
			// 乱序加入内部列表: a(0) c(2) b(1)
			parent.exposeAddChild(a, false);
			parent.exposeAddChild(c, false);
			parent.exposeAddChild(b, false);
			parent.sortChild();
			System.Collections.Generic.List<myUGUIObject> list = parent.getChildList();
			assertTrue(ReferenceEquals(a, list[0]), "排序后第 0 个是 A");
			assertTrue(ReferenceEquals(b, list[1]), "排序后第 1 个是 B");
			assertTrue(ReferenceEquals(c, list[2]), "排序后第 2 个是 C");
			// 已排序后再次调用直接返回(无副作用)
			parent.sortChild();
			assertTrue(ReferenceEquals(a, list[0]), "重复 sortChild 无副作用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// addLongPress/clearLongPress/removeLongPress 守卫式(不崩, 重复 add 不重复)
	// addLongPress/removeLongPress/clearLongPress 守卫式
	// 注意: removeLongPress 置 null 不移除元素(框架行为), 之后任何遍历(add/remove)会 NRE,
	//       必须先 clearLongPress 清掉残留 null 再继续
	private static void testAddLongPress()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			Action callback = () => { };
			ui.addLongPress(callback, 0.5f, null);   // 首次添加
			ui.addLongPress(callback, 0.5f, null);   // 重复添加不重复
			ui.clearLongPress();                     // 清空
			ui.addLongPress(callback, 0.5f, null);
			ui.removeLongPress(callback);            // 移除指定 → 列表残留 null(框架行为)
			ui.clearLongPress();                     // 必须先清掉残留 null
			ui.removeLongPress(callback);            // 空列表 remove 安全
			ui.clearLongPress();                     // 空清空安全
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// notifyAnchorApply 空实现, 调用安全
	private static void testNotifyAnchorApply()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.notifyAnchorApply();   // 空实现, 不崩
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// refreshChildDepthByPositionZ: 按 z 降序重排兄弟(z 大 index 靠前)
	private static void testRefreshChildDepthByPositionZ()
	{
		GameObject parentGo = new GameObject("Parent");
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			myUGUIObject high = createChildUI("High", parentGo);
			myUGUIObject low = createChildUI("Low", parentGo);
			high.setPosition(new Vector3(0.0f, 0.0f, 1.0f));    // z=1
			low.setPosition(new Vector3(0.0f, 0.0f, -1.0f));    // z=-1
			parent.refreshChildDepthByPositionZ();
			// z 大(1)的排到 index=0
			assertEqual(0, high.getSibling(), "z 大的排前面 index=0");
			assertEqual(1, low.getSibling(), "z 小的排后面 index=1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	private static myUGUIObject createChildUI(string name, GameObject parentGo)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		go.transform.SetParent(parentGo.transform);
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}
}
