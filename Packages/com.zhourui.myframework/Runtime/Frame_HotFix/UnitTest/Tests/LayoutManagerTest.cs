using UnityEngine;
using System.Collections.Generic;
using static TestAssert;
using static FrameDefine;
using static FrameBaseHotFix;

// LayoutManager 布局管理器单元测试
// 每个测试函数验证一个独立场景
//
// 不可测试方法（需 prefab/异步/有效 GameLayout）:
//   createLayout / createLayoutAsync / createScript / newLayout
//   notifyLayoutVisible / notifyLayoutChanged / testAllLayout / update/lateUpdate/willDestroy/onDrawGizmos
public static class LayoutManagerTest
{
	public static void Run()
	{
		if (mLayoutManager == null) return;

		// === generateRenderOrder: ALWAYS_TOP ===
		testGenerateRenderOrderAlwaysTopZero();
		testGenerateRenderOrderAlwaysTopSmall();
		testGenerateRenderOrderAlwaysTopAtBoundary();
		testGenerateRenderOrderAlwaysTopAboveBoundary();
		testGenerateRenderOrderAlwaysTopHighValue();
		testGenerateRenderOrderAlwaysTopMaxValue();
		testGenerateRenderOrderAlwaysTopNegative();

		// === generateRenderOrder: FIXED ===
		testGenerateRenderOrderFixedZero();
		testGenerateRenderOrderFixedNormal();
		testGenerateRenderOrderFixedHigh();
		testGenerateRenderOrderFixedMaxValue();
		testGenerateRenderOrderFixedMinValue();

		// === generateRenderOrder: ALWAYS_TOP_AUTO ===
		testGenerateRenderOrderAlwaysTopAutoZero();
		testGenerateRenderOrderAlwaysTopAutoHighValue();
		testGenerateRenderOrderAlwaysTopAutoAtAlwaysTopOrder();
		testGenerateRenderOrderAlwaysTopAutoMaxValue();

		// === generateRenderOrder: AUTO ===
		testGenerateRenderOrderAutoZero();
		testGenerateRenderOrderAutoHighValue();
		testGenerateRenderOrderAutoMaxValue();
		testGenerateRenderOrderAutoMinValue();

		// === generateRenderOrder: exceptLayout ===
		testGenerateRenderOrderExceptLayoutNoEffectOnEmpty();

		// === getTopLayoutOrder ===
		testGetTopLayoutOrderAlwaysTopEmpty();
		testGetTopLayoutOrderNormalEmpty();

		// === 字典查询: 未注册返回默认值 ===
		testGetLayoutPathByTypeUnregistered();
		testGetLayoutTypeByPathUnregistered();
		testGetLayoutTypeByPathEmptyString();

		// === 字典查询: 未加载返回默认值 ===
		testGetLayoutUnloaded();
		testGetScriptUnloaded();
		testGetLayoutListReturnsNonNull();
		testGetLayoutListInitialCount();

		// === 空状态安全调用 ===
		testDestroyLayoutNonExistent();
		testUnloadAllPartLayoutEmpty();
		testGetAllLayoutBoxColliderEmpty();

		// === COM 组件 ===
		testNotifyLayoutRenderOrderEmpty();

		// === getter/setter ===
		testSetUseAnchorToggle();
		testSetUseAnchorRestore();
		testSetUseAnchorDuplicateFalse();
		testSetUseAnchorDuplicateTrue();

		// === UI 根节点 ===
		testGetRootObjectExists();
		testGetRootObjectNameNonEmpty();
		testGetUIRootExists();
		testGetUGUIRootComponentExists();

		// === 属性一致性 ===
		testRootObjectMatchesGetUIRoot();
		testCanvasMatchesGetUIRoot();
		testGetLayoutCountNonNegative();
	}
	// ================================================================
	//  generateRenderOrder: ALWAYS_TOP
	// ================================================================
	// renderOrder=0 → ALWAYS_TOP_ORDER(1000)
	private static void testGenerateRenderOrderAlwaysTopZero()
	{
		assertEqual(ALWAYS_TOP_ORDER, mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// renderOrder=1 → 1001
	private static void testGenerateRenderOrderAlwaysTopSmall()
	{
		assertEqual(ALWAYS_TOP_ORDER + 1, mLayoutManager.generateRenderOrder(null, 1, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// renderOrder=ALWAYS_TOP_ORDER-1 → 1000+999=1999
	private static void testGenerateRenderOrderAlwaysTopAtBoundary()
	{
		assertEqual(ALWAYS_TOP_ORDER * 2 - 1, mLayoutManager.generateRenderOrder(null, ALWAYS_TOP_ORDER - 1, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// renderOrder=ALWAYS_TOP_ORDER → 不变(1000)
	private static void testGenerateRenderOrderAlwaysTopAboveBoundary()
	{
		assertEqual(ALWAYS_TOP_ORDER, mLayoutManager.generateRenderOrder(null, ALWAYS_TOP_ORDER, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// renderOrder=1500 → 不变(1500)
	private static void testGenerateRenderOrderAlwaysTopHighValue()
	{
		assertEqual(ALWAYS_TOP_ORDER + 500, mLayoutManager.generateRenderOrder(null, ALWAYS_TOP_ORDER + 500, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// renderOrder=int.MaxValue → 不变
	private static void testGenerateRenderOrderAlwaysTopMaxValue()
	{
		assertEqual(int.MaxValue, mLayoutManager.generateRenderOrder(null, int.MaxValue, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// renderOrder=-100 → -100+1000=900
	private static void testGenerateRenderOrderAlwaysTopNegative()
	{
		assertEqual(ALWAYS_TOP_ORDER - 100, mLayoutManager.generateRenderOrder(null, -100, LAYOUT_ORDER.ALWAYS_TOP));
	}
	// ================================================================
	//  generateRenderOrder: FIXED
	// ================================================================
	private static void testGenerateRenderOrderFixedZero()
	{
		assertEqual(0, mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.FIXED));
	}
	private static void testGenerateRenderOrderFixedNormal()
	{
		assertEqual(42, mLayoutManager.generateRenderOrder(null, 42, LAYOUT_ORDER.FIXED));
	}
	private static void testGenerateRenderOrderFixedHigh()
	{
		assertEqual(9999, mLayoutManager.generateRenderOrder(null, 9999, LAYOUT_ORDER.FIXED));
	}
	private static void testGenerateRenderOrderFixedMaxValue()
	{
		assertEqual(int.MaxValue, mLayoutManager.generateRenderOrder(null, int.MaxValue, LAYOUT_ORDER.FIXED));
	}
	private static void testGenerateRenderOrderFixedMinValue()
	{
		assertEqual(int.MinValue, mLayoutManager.generateRenderOrder(null, int.MinValue, LAYOUT_ORDER.FIXED));
	}
	// ================================================================
	//  generateRenderOrder: ALWAYS_TOP_AUTO
	//  空列表时 getTopLayoutOrder(null,true)=1000 → +1=1001
	//  传入的 renderOrder 被忽略
	// ================================================================
	private static void testGenerateRenderOrderAlwaysTopAutoZero()
	{
		assertEqual(ALWAYS_TOP_ORDER + 1, mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO));
	}
	private static void testGenerateRenderOrderAlwaysTopAutoHighValue()
	{
		assertEqual(ALWAYS_TOP_ORDER + 1, mLayoutManager.generateRenderOrder(null, 9999, LAYOUT_ORDER.ALWAYS_TOP_AUTO));
	}
	private static void testGenerateRenderOrderAlwaysTopAutoAtAlwaysTopOrder()
	{
		assertEqual(ALWAYS_TOP_ORDER + 1, mLayoutManager.generateRenderOrder(null, ALWAYS_TOP_ORDER, LAYOUT_ORDER.ALWAYS_TOP_AUTO));
	}
	private static void testGenerateRenderOrderAlwaysTopAutoMaxValue()
	{
		assertEqual(ALWAYS_TOP_ORDER + 1, mLayoutManager.generateRenderOrder(null, int.MaxValue, LAYOUT_ORDER.ALWAYS_TOP_AUTO));
	}
	// ================================================================
	//  generateRenderOrder: AUTO
	//  空列表时 getTopLayoutOrder(null,false)=0 → +1=1
	//  传入的 renderOrder 被忽略
	// ================================================================
	private static void testGenerateRenderOrderAutoZero()
	{
		assertEqual(1, mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.AUTO));
	}
	private static void testGenerateRenderOrderAutoHighValue()
	{
		assertEqual(1, mLayoutManager.generateRenderOrder(null, 9999, LAYOUT_ORDER.AUTO));
	}
	private static void testGenerateRenderOrderAutoMaxValue()
	{
		assertEqual(1, mLayoutManager.generateRenderOrder(null, int.MaxValue, LAYOUT_ORDER.AUTO));
	}
	private static void testGenerateRenderOrderAutoMinValue()
	{
		assertEqual(1, mLayoutManager.generateRenderOrder(null, int.MinValue, LAYOUT_ORDER.AUTO));
	}
	// ================================================================
	//  generateRenderOrder: exceptLayout 参数
	// ================================================================
	private static void testGenerateRenderOrderExceptLayoutNoEffectOnEmpty()
	{
		// 列表为空时 exceptLayout 无影响
		assertEqual(1, mLayoutManager.generateRenderOrder(default(GameLayout), 0, LAYOUT_ORDER.AUTO));
	}
	// ================================================================
	//  getTopLayoutOrder
	// ================================================================
	private static void testGetTopLayoutOrderAlwaysTopEmpty()
	{
		// 空列表 + alwaysTop=true → 返回 ALWAYS_TOP_ORDER(1000) 保底
		assertEqual(ALWAYS_TOP_ORDER, mLayoutManager.getTopLayoutOrder(null, true));
	}
	private static void testGetTopLayoutOrderNormalEmpty()
	{
		// 空列表 + alwaysTop=false → 返回 0
		assertEqual(0, mLayoutManager.getTopLayoutOrder(null, false));
	}
	// ================================================================
	//  字典查询: 未注册
	// ================================================================
	private static void testGetLayoutPathByTypeUnregistered()
	{
		var path = mLayoutManager.getLayoutPathByType(typeof(string));
		assertTrue(path == null || path == "");
	}
	private static void testGetLayoutTypeByPathUnregistered()
	{
		assertNull(mLayoutManager.getLayoutTypeByPath("__NONEXISTENT_PATH__"));
	}
	private static void testGetLayoutTypeByPathEmptyString()
	{
		assertNull(mLayoutManager.getLayoutTypeByPath(""));
	}
	// ================================================================
	//  字典查询: 未加载
	// ================================================================
	private static void testGetLayoutUnloaded()
	{
		assertNull(mLayoutManager.getLayout(typeof(string)));
	}
	private static void testGetScriptUnloaded()
	{
		assertNull(mLayoutManager.getScript(typeof(string)));
	}
	private static void testGetLayoutListReturnsNonNull()
	{
		assertNotNull(mLayoutManager.getLayoutList());
	}
	private static void testGetLayoutListInitialCount()
	{
		assertEqual(0, mLayoutManager.getLayoutList().count());
	}
	// ================================================================
	//  空状态安全调用
	// ================================================================
	private static void testDestroyLayoutNonExistent()
	{
		mLayoutManager.destroyLayout(typeof(string));
	}
	private static void testUnloadAllPartLayoutEmpty()
	{
		mLayoutManager.unloadAllPartLayout();
	}
	private static void testGetAllLayoutBoxColliderEmpty()
	{
		var colliders = new List<Collider>();
		mLayoutManager.getAllLayoutBoxCollider(colliders);
		assertEqual(0, colliders.Count);
	}
	// ================================================================
	//  COM 组件
	// ================================================================
	private static void testNotifyLayoutRenderOrderEmpty()
	{
		mLayoutManager.notifyLayoutRenderOrder();
	}
	// ================================================================
	//  getter/setter: isUseAnchor
	// ================================================================
	private static void testSetUseAnchorToggle()
	{
		bool origin = mLayoutManager.isUseAnchor();
		mLayoutManager.setUseAnchor(!origin);
		assertEqual(!origin, mLayoutManager.isUseAnchor());
		mLayoutManager.setUseAnchor(origin);
	}
	private static void testSetUseAnchorRestore()
	{
		bool origin = mLayoutManager.isUseAnchor();
		mLayoutManager.setUseAnchor(false);
		mLayoutManager.setUseAnchor(true);
		assertTrue(mLayoutManager.isUseAnchor());
		mLayoutManager.setUseAnchor(origin);
	}
	private static void testSetUseAnchorDuplicateFalse()
	{
		bool origin = mLayoutManager.isUseAnchor();
		mLayoutManager.setUseAnchor(false);
		mLayoutManager.setUseAnchor(false);
		assertFalse(mLayoutManager.isUseAnchor());
		mLayoutManager.setUseAnchor(origin);
	}
	private static void testSetUseAnchorDuplicateTrue()
	{
		bool origin = mLayoutManager.isUseAnchor();
		mLayoutManager.setUseAnchor(true);
		mLayoutManager.setUseAnchor(true);
		assertTrue(mLayoutManager.isUseAnchor());
		mLayoutManager.setUseAnchor(origin);
	}
	// ================================================================
	//  UI 根节点
	// ================================================================
	private static void testGetRootObjectExists()
	{
		assertNotNull(mLayoutManager.getRootObject());
	}
	private static void testGetRootObjectNameNonEmpty()
	{
		var root = mLayoutManager.getRootObject();
		assertTrue(root.name.Length > 0);
	}
	private static void testGetUIRootExists()
	{
		assertNotNull(mLayoutManager.getUIRoot());
	}
	private static void testGetUGUIRootComponentExists()
	{
		assertNotNull(mLayoutManager.getUGUIRootComponent());
	}
	// ================================================================
	//  属性一致性
	// ================================================================
	private static void testRootObjectMatchesGetUIRoot()
	{
		var rootObj = mLayoutManager.getRootObject();
		var uiRoot = mLayoutManager.getUIRoot();
		if (rootObj != null && uiRoot != null)
		{
			assertEqual(rootObj, uiRoot.getGameObject());
		}
	}
	private static void testCanvasMatchesGetUIRoot()
	{
		var canvas = mLayoutManager.getUGUIRootComponent();
		var uiRoot = mLayoutManager.getUIRoot();
		if (canvas != null && uiRoot != null)
		{
			assertEqual(canvas, uiRoot.getCanvas());
		}
	}
	private static void testGetLayoutCountNonNegative()
	{
		assertTrue(mLayoutManager.getLayoutCount() >= 0);
	}
}