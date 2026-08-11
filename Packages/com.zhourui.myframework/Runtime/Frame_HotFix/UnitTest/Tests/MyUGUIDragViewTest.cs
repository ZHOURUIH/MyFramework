using System.IO;
using System;
using UnityEngine;
using static FrameBaseHotFix;
using static TestAssert;

// myUGUIDragView 深度测试(拖拽滑动窗口封装, 完整 init 链路):
//   ⚠️ 不继承 myUGUIDragView: BASE001 分析器强制 override init 必须调 base.init(),
//      而 myUGUIDragView.init 有布局依赖(mLayout.getScript().bindPassOnlyParent)
//      → 直接 new myUGUIDragView() + 注入真实 GameLayout + 父链, 让 base.init() 完整跑通
//   节点链: rootGo(Canvas) / Grand / Parent / DragView
//   setLayout(layout) 注入布局 / setParent(parent) 提供父链(父还需有自己的父)
//   setDragDirection/getDragDirection / setClampInner/isClampInner
//   setMaxRelativePos/getMaxRelativePos / setMinRelativePos/getMinRelativePos
//   setAllowDragOnlyOverParentSize/isAllowDragOnlyOverParentSize / setDragView 组合
//   onTouchDown/onTouchMove/onTouchStay/onScreenTouchUp 拖拽链路 / stopMoving
//   autoClampPosition/autoResetPosition/notifyParentSizeChange / getViewport
public static class MyUGUIDragViewTest
{
	private const string TEST_LAYOUT_PATH = "TestDragViewLayout.prefab";
	private const string TEST_LAYOUT_NAME = "TestDragViewLayout";
	private static readonly string sPrefabFile = "Assets/GameResources/" + TEST_LAYOUT_PATH;
	private static bool sRegistered;

	public static void Run()
	{
		try
		{
			if (!File.Exists(sPrefabFile))
			{
				File.WriteAllText(sPrefabFile, "");
			}
			if (!sRegistered)
			{
				mLayoutManager.registeLayout(typeof(TestDragViewLayout), TEST_LAYOUT_PATH, LAYOUT_LIFE_CYCLE.PERSIST, null);
				sRegistered = true;
			}
			testInitCreatesComponent();
			testDragDirection();
			testClampInner();
			testClampAndAllowFlags();
			testRelativePos();
			testSetDragViewCombo();
			testCallbackAndConfigSetters();
			testOnTouchChain();
			testStopMoving();
			testAutoClampReset();
			testNotifyParentSizeChange();
			testViewportAndReceive();
		}
		finally
		{
			sRegistered = false;
			if (File.Exists(sPrefabFile))
			{
				File.Delete(sPrefabFile);
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 完整布局环境 + 节点链(Grand → Parent → DragView)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIDragView createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo)
	{
		rootGo = new GameObject(TEST_LAYOUT_NAME);
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		layout = new GameLayout();
		layout.setName(TEST_LAYOUT_NAME);
		layout.setType(typeof(TestDragViewLayout));
		layout.setParent(null);
		layout.init();

		// Grand 节点(父的父, bindPassOnlyParent 需要 parent.getParent().getDepth())
		grandGo = new GameObject("Grand");
		grandGo.AddComponent<RectTransform>();
		grandGo.transform.SetParent(rootGo.transform);   // 挂到布局根下, 便于统一销毁
		myUGUIObject grand = new myUGUIObject();
		grand.setObject(grandGo);
		grand.init();

		parentGo = new GameObject("Parent");
		parentGo.AddComponent<RectTransform>();
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		parentGo.transform.SetParent(grandGo.transform);
		parent.setParent(grand, false);

		GameObject go = new GameObject("DragView");
		myUGUIDragView drag = new myUGUIDragView();
		drag.setObject(go);
		drag.setLayout(layout);          // 注入布局: base.init 的 mLayout.getScript() 依赖
		go.transform.SetParent(parentGo.transform);
		drag.setParent(parent, false);   // mParent 非 null
		drag.init();                     // 完整 init: 布局依赖全部满足
		return drag;
	}

	// 清理: 注销碰撞体 + 布局销毁 + 节点销毁
	private static void cleanupDragView(GameLayout layout, myUGUIDragView drag, GameObject rootGo)
	{
		mGlobalTouchSystem.unregisteCollider(drag);
		if (drag.getViewport() != null)
		{
			mGlobalTouchSystem.unregisteCollider(drag.getViewport());
		}
		layout.destroy();
		UnityEngine.Object.DestroyImmediate(rootGo);
	}

	// init(完整链路) 后组件已创建
	private static void testInitCreatesComponent()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			assertNotNull(drag.getDragViewComponent(), "init 后拖拽组件已创建");
			assertFalse(drag.isDragging(), "初始未在拖拽");
			assertTrue(ReferenceEquals(parentGo.transform, drag.getGameObject().transform.parent), "DragView 挂到 Parent 下");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setDragDirection/getDragDirection 往返
	private static void testDragDirection()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setDragDirection(DRAG_DIRECTION.HORIZONTAL);
			assertTrue(DRAG_DIRECTION.HORIZONTAL == drag.getDragDirection(), "横向读回");
			drag.setDragDirection(DRAG_DIRECTION.VERTICAL);
			assertTrue(DRAG_DIRECTION.VERTICAL == drag.getDragDirection(), "纵向读回");
			drag.setDragDirection(DRAG_DIRECTION.FREE);
			assertTrue(DRAG_DIRECTION.FREE == drag.getDragDirection(), "自由读回");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setClampInner/isClampInner 往返
	private static void testClampInner()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setClampInner(true);
			assertTrue(drag.isClampInner(), "setClampInner(true) 读回");
			drag.setClampInner(false);
			assertFalse(drag.isClampInner(), "setClampInner(false) 读回");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setAllowDragOnlyOverParentSize 往返 + setClampInRange/setClampType 守卫
	private static void testClampAndAllowFlags()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setAllowDragOnlyOverParentSize(true);
			assertTrue(drag.isAllowDragOnlyOverParentSize(), "allowDragOnly=true 读回");
			drag.setAllowDragOnlyOverParentSize(false);
			assertFalse(drag.isAllowDragOnlyOverParentSize(), "allowDragOnly=false 读回");
			drag.setClampInRange(true);
			drag.setClampType(CLAMP_TYPE.CENTER_IN_RECT);
			drag.setClampType(CLAMP_TYPE.EDGE_IN_RECT);
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setMaxRelativePos/setMinRelativePos 读写
	private static void testRelativePos()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			Vector3 max = new Vector3(100.0f, 50.0f, 0.0f);
			drag.setMaxRelativePos(max);
			assertTrue(max == drag.getMaxRelativePos(), "maxRelativePos 读回");
			Vector3 min = new Vector3(-10.0f, 0.0f, 0.0f);
			drag.setMinRelativePos(min);
			assertTrue(min == drag.getMinRelativePos(), "minRelativePos 读回");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setDragView 组合: 一次设置 5 个参数
	private static void testSetDragViewCombo()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setDragView(DRAG_DIRECTION.HORIZONTAL, 0.5f, true, false, true);
			assertTrue(DRAG_DIRECTION.HORIZONTAL == drag.getDragDirection(), "组合: 方向");
			assertTrue(drag.isClampInner(), "组合: clampInner");
			assertFalse(drag.isAllowDragOnlyOverParentSize(), "组合: allowDragOnly");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// 回调与配置 setter 守卫式
	private static void testCallbackAndConfigSetters()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setMoveSpeedScale(2.0f);
			drag.setDragViewStartCallback((ref bool b) => { });
			drag.setDraggingCallback(() => { });
			drag.setReleaseDragCallback(() => { });
			drag.setPositionChangeCallback(() => { });
			drag.setAutoMoveToEdge(true);
			drag.setDragLengthThreshold(5.0f);
			drag.setDragAngleThreshold(0.5f);
			drag.setAttenuateFactor(0.8f);
			drag.setAutoClampSpeed(3.0f);
			drag.setAlignTopOrLeft(true);
			// 全部守卫式调用不崩
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// 拖拽链路: onTouchDown → onTouchMove(同 id 生效, 不同 id 忽略) → onScreenTouchUp
	private static void testOnTouchChain()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.onTouchDown(new Vector3(10.0f, 20.0f, 0.0f), 1);
			drag.onTouchMove(new Vector3(11.0f, 20.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), 0.1f, 1);
			drag.onTouchMove(new Vector3(12.0f, 20.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), 0.1f, 2);   // 不同触点忽略
			drag.onTouchStay(new Vector3(12.0f, 20.0f, 0.0f), 1);
			drag.onScreenTouchUp(new Vector3(12.0f, 20.0f, 0.0f), 1);
			// 全链路不崩
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// stopMoving 调用安全
	private static void testStopMoving()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.stopMoving();
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// autoClampPosition/autoResetPosition: 有父子链环境调用不崩
	private static void testAutoClampReset()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setSize(new Vector2(200.0f, 200.0f));
			drag.autoClampPosition();
			drag.autoResetPosition();
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// notifyParentSizeChange + setSize 组合(触发 onWindowSizeChange)
	private static void testNotifyParentSizeChange()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.notifyParentSizeChange();
			drag.setSize(new Vector2(100.0f, 100.0f));
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// getViewport 返回父节点 + isReceiveScreenTouch 恒 true
	private static void testViewportAndReceive()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			assertTrue(drag.isReceiveScreenTouch(), "isReceiveScreenTouch 恒 true");
			assertTrue(ReferenceEquals(parentGo.transform, drag.getViewport().getGameObject().transform), "getViewport 返回父节点");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}
}

// 空布局脚本(为 myUGUIDragView.init 的 mLayout.getScript() 提供脚本实例)
public class TestDragViewLayout : LayoutScript
{
	public override void assignWindow()
	{
	}

	public new void resetProperty()
	{
		base.resetProperty();
	}
}
