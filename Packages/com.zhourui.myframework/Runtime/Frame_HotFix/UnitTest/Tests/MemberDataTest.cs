using UnityEngine;
using static TestAssert;

// MemberData 单元测试 — UGUI 生成器成员变量数据的纯逻辑方法
// [Serializable] 纯 C# 类, 覆盖 setType/getTypeName/setWindowType/setArrayType/checkRegisterCollider/isValid
// (getParentObject/getGameObjectName/autoSetArrayLength/setObject 依赖 GameObject, 仅部分用 dummy GameObject 覆盖)
public static class MemberDataTest
{
	public static void Run()
	{
		test_SetType();
		test_GetTypeName_NormalWindow();
		test_GetTypeName_ScrollList();
		test_GetTypeName_Pool();
		test_GetTypeName_PoolMap();
		test_SetWindowType_ToScrollList();
		test_SetWindowType_ToPool();
		test_SetWindowType_ToNormal();
		test_SetArrayType_Array();
		test_SetArrayType_None();
		test_CheckRegisterCollider_Button();
		test_CheckRegisterCollider_Other();
		test_IsValid();
	}

	// ═════════════════════════════════════════════════════════════════
	// setType — 设置类型
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetType()
	{
		MemberData data = new MemberData();
		data.setType("MyWindow");
		// mType 为私有, 通过 getTypeName 验证(NORMAL_WINDOW 直接返回 mType)
		data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
		assertEqual("MyWindow", data.getTypeName(), "setType 后 getTypeName 返回类型名");
	}

	// ═════════════════════════════════════════════════════════════════
	// getTypeName — 普通窗口/通用控件/子页面直接返回类型
	// ═════════════════════════════════════════════════════════════════
	private static void test_GetTypeName_NormalWindow()
	{
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
		data.setType("MyWindow");
		assertEqual("MyWindow", data.getTypeName(), "普通窗口返回类型名");
		data.mWindowType = WINDOW_TYPE.COMMON_CONTROL;
		assertEqual("MyWindow", data.getTypeName(), "通用控件返回类型名");
		data.mWindowType = WINDOW_TYPE.SUB_UI;
		assertEqual("MyWindow", data.getTypeName(), "子页面返回类型名");
	}

	// ═════════════════════════════════════════════════════════════════
	// getTypeName — 滚动列表拼参数
	// ═════════════════════════════════════════════════════════════════
	private static void test_GetTypeName_ScrollList()
	{
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.SCROLL_LIST;
		data.setType("UGUIDragViewLoop");
		data.mParam0 = "Item";
		assertEqual("UGUIDragViewLoop<Item, Item.Data>", data.getTypeName(), "滚动列表拼接类型");
	}

	// ═════════════════════════════════════════════════════════════════
	// getTypeName — 对象池单参数
	// ═════════════════════════════════════════════════════════════════
	private static void test_GetTypeName_Pool()
	{
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.POOL;
		data.setType("WindowPool");
		data.mParam0 = "ItemWindow";
		assertEqual("WindowPool<ItemWindow>", data.getTypeName(), "对象池单参数类型");
	}

	// ═════════════════════════════════════════════════════════════════
	// getTypeName — 对象池双参数(WindowStructPoolMap)
	// ═════════════════════════════════════════════════════════════════
	private static void test_GetTypeName_PoolMap()
	{
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.POOL;
		data.setType("WindowStructPoolMap");
		data.mParam0 = "K";
		data.mParam1 = "V";
		assertEqual("WindowStructPoolMap<K, V>", data.getTypeName(), "对象池双参数类型");
	}

	// ═════════════════════════════════════════════════════════════════
	// setWindowType — 切到滚动列表清 mObject 并设模板类型
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetWindowType_ToScrollList()
	{
		MemberData data = new MemberData();
		GameObject go = new GameObject("DummyWindow");
		try
		{
			// 先设为普通窗口并关联对象
			data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
			data.setType("MyWindow");
			data.mObject = go;
			data.mPoolTemplate = go;
			data.mParam0 = "Old0";
			data.mParam1 = "Old1";
			// 切到滚动列表
			data.setWindowType(WINDOW_TYPE.SCROLL_LIST);
			assertTrue(data.mWindowType == WINDOW_TYPE.SCROLL_LIST, "windowType 已切换为 SCROLL_LIST");
			assertNull(data.mObject, "切到滚动列表清 mObject");
			assertEqual(typeof(myUGUIObject).ToString(), data.mTemplateWindowType, "模板类型设为 myUGUIObject");
			assertNull(data.mViewportObject, "切到滚动列表清 viewport");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setWindowType — 切到对象池
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetWindowType_ToPool()
	{
		MemberData data = new MemberData();
		GameObject go = new GameObject("Dummy");
		try
		{
			data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
			data.mObject = go;
			data.setWindowType(WINDOW_TYPE.POOL);
			assertTrue(data.mWindowType == WINDOW_TYPE.POOL, "windowType 已切换为 POOL");
			assertNull(data.mObject, "切到对象池清 mObject");
			assertEqual(typeof(myUGUIObject).ToString(), data.mTemplateWindowType, "模板类型设为 myUGUIObject");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setWindowType — 切回普通窗口清对象池相关字段
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetWindowType_ToNormal()
	{
		MemberData data = new MemberData();
		GameObject go = new GameObject("Dummy");
		try
		{
			data.mWindowType = WINDOW_TYPE.POOL;
			data.mPoolTemplate = go;
			data.mParam0 = "P0";
			data.mParam1 = "P1";
			data.mViewportObject = go;
			data.setWindowType(WINDOW_TYPE.NORMAL_WINDOW);
			assertTrue(data.mWindowType == WINDOW_TYPE.NORMAL_WINDOW, "windowType 已切换为 NORMAL_WINDOW");
			assertNull(data.mPoolTemplate, "切回普通窗口清 poolTemplate");
			assertNull(data.mParam0, "切回普通窗口清 param0");
			assertNull(data.mParam1, "切回普通窗口清 param1");
			assertNull(data.mViewportObject, "非滚动列表清 viewport");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setArrayType — 数组类型关闭自动注册事件
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetArrayType_Array()
	{
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
		data.mRegisterCollider = true;
		data.mHasClickEvent = true;
		// DYNAMIC_ARRAY 不触发 autoSetArrayLength(mObject 为 null 时跳过)
		data.setArrayType(ARRAY_TYPE.DYNAMIC_ARRAY);
		assertTrue(data.mArrayType == ARRAY_TYPE.DYNAMIC_ARRAY, "arrayType 已设置");
		assertFalse(data.mRegisterCollider, "数组类型关闭自动注册碰撞");
		assertFalse(data.mHasClickEvent, "数组类型关闭点击事件");
	}

	// ═════════════════════════════════════════════════════════════════
	// setArrayType — 非数组类型
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetArrayType_None()
	{
		MemberData data = new MemberData();
		data.setArrayType(ARRAY_TYPE.NONE);
		assertTrue(data.mArrayType == ARRAY_TYPE.NONE, "arrayType 为 NONE");
	}

	// ═════════════════════════════════════════════════════════════════
	// checkRegisterCollider — 通用控件 + Button + 非数组 → 自动注册
	// ═════════════════════════════════════════════════════════════════
	private static void test_CheckRegisterCollider_Button()
	{
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.COMMON_CONTROL;
		data.mArrayType = ARRAY_TYPE.NONE;
		data.setType(typeof(LegendButton).ToString());
		assertTrue(data.mRegisterCollider, "LegendButton 自动注册碰撞");
		assertTrue(data.mHasClickEvent, "LegendButton 自动注册点击");
	}

	// ═════════════════════════════════════════════════════════════════
	// checkRegisterCollider — 其他类型不自动注册
	// ═════════════════════════════════════════════════════════════════
	private static void test_CheckRegisterCollider_Other()
	{
		// NORMAL_WINDOW 不触发自动注册逻辑, 字段保持不变
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
		data.mRegisterCollider = true;
		data.mHasClickEvent = true;
		data.setType(typeof(LegendButton).ToString());
		assertTrue(data.mRegisterCollider, "普通窗口不触发自动注册, 字段保持原值");
		assertTrue(data.mHasClickEvent, "普通窗口不触发自动注册, 字段保持原值");
		// COMMON_CONTROL 但类型不是按钮/标签/复选框 → 关闭
		MemberData data2 = new MemberData();
		data2.mWindowType = WINDOW_TYPE.COMMON_CONTROL;
		data2.mRegisterCollider = true;
		data2.mHasClickEvent = true;
		data2.setType("OtherType");
		assertFalse(data2.mRegisterCollider, "非按钮类型不自动注册碰撞");
		assertFalse(data2.mHasClickEvent, "非按钮类型不自动注册点击");
	}

	// ═════════════════════════════════════════════════════════════════
	// isValid — 不同类型是否需要关联 GameObject
	// ═════════════════════════════════════════════════════════════════
	private static void test_IsValid()
	{
		// 普通窗口无对象 → 无效
		MemberData data = new MemberData();
		data.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
		data.mObject = null;
		assertFalse(data.isValid(), "普通窗口无对象应无效");
		// 对象池不需对象 → 有效
		MemberData data2 = new MemberData();
		data2.mWindowType = WINDOW_TYPE.POOL;
		data2.mObject = null;
		assertTrue(data2.isValid(), "对象池不需对象应有效");
		// 普通窗口有对象 → 有效
		MemberData data3 = new MemberData();
		data3.mWindowType = WINDOW_TYPE.NORMAL_WINDOW;
		GameObject go = new GameObject("Valid");
		try
		{
			data3.mObject = go;
			assertTrue(data3.isValid(), "普通窗口有对象应有效");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
