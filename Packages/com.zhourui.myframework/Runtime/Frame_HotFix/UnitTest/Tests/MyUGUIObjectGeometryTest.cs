using UnityEngine;
using UObject = UnityEngine.Object;
using static TestAssert;

// myUGUIObject 几何实例方法单元测试(EditMode, 纯 RectTransform 操作)
// 模式: new GameObject + AddComponent<RectTransform> + setObject + init
//       (init 设置 mRectTransform; initComponents 走 ComponentOwner 空实现; 依赖链全部空安全)
// 关键: 裸 RectTransform 无父节点 → getParentSize=zero → setRectSize 的 sizeDelta=size, 确定性
// 清理: DestroyImmediate 测试自 new 的 GameObject
public static class MyUGUIObjectGeometryTest
{
	public static void Run()
	{
		testInitSetsRectTransform();
		testSetSize();
		testSetWidthHeight();
		testPivot();
		testSelfBounds();
		testPosition();
		testParentBounds();
		testAlignToOther();
		testInParentCenter();
		testSetInParentRoundTrip();
		testCloneFrom();
	}

	// ═════════════════════════════════════════════════════════════════
	// init 后 mRectTransform 非 null, getRectTransform 返回同一对象
	// ═════════════════════════════════════════════════════════════════
	private static void testInitSetsRectTransform()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			RectTransform rt = go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			assertNotNull(ui.getRectTransform(), "init 后 getRectTransform 非 null");
			assertTrue(ReferenceEquals(rt, ui.getRectTransform()), "getRectTransform 返回 Go 上的 RectTransform");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setSize → getSize 一致(无父节点: sizeDelta=size)
	// ═════════════════════════════════════════════════════════════════
	private static void testSetSize()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			Vector2 size = ui.getSize();
			assertEqual(100.0f, size.x, 0.001f, "setSize 后 getSize.x=100");
			assertEqual(50.0f, size.y, 0.001f, "setSize 后 getSize.y=50");
			// 再设一次不同大小
			ui.setSize(new Vector2(200.0f, 80.0f));
			size = ui.getSize();
			assertEqual(200.0f, size.x, 0.001f, "二次 setSize 后 getSize.x=200");
			assertEqual(80.0f, size.y, 0.001f, "二次 setSize 后 getSize.y=80");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setWidth/setHeight — 只改单轴
	// ═════════════════════════════════════════════════════════════════
	private static void testSetWidthHeight()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			ui.setWidth(150.0f);
			Vector2 size = ui.getSize();
			assertEqual(150.0f, size.x, 0.001f, "setWidth 后 getSize.x=150");
			assertEqual(50.0f, size.y, 0.001f, "setWidth 不影响 getSize.y");
			ui.setHeight(75.0f);
			size = ui.getSize();
			assertEqual(150.0f, size.x, 0.001f, "setHeight 不影响 getSize.x");
			assertEqual(75.0f, size.y, 0.001f, "setHeight 后 getSize.y=75");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setPivot/getPivot — pivot 读写
	// ═════════════════════════════════════════════════════════════════
	private static void testPivot()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			// RectTransform 默认 pivot 是 (0.5, 0.5)
			Vector2 def = ui.getPivot();
			assertEqual(0.5f, def.x, 0.001f, "默认 pivot.x=0.5");
			assertEqual(0.5f, def.y, 0.001f, "默认 pivot.y=0.5");
			ui.setPivot(new Vector2(0.0f, 0.0f));
			Vector2 p = ui.getPivot();
			assertEqual(0.0f, p.x, 0.001f, "setPivot(0,0) 后 pivot.x=0");
			assertEqual(0.0f, p.y, 0.001f, "setPivot(0,0) 后 pivot.y=0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getLeftInSelf/getRightInSelf/getTopInSelf/getBottomInSelf
	// 基于 size × pivot 的边界计算
	// ═════════════════════════════════════════════════════════════════
	private static void testSelfBounds()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			// pivot 默认 (0.5, 0.5): left=-50, right=50, top=25, bottom=-25
			assertEqual(-50.0f, ui.getLeftInSelf(), 0.001f, "pivot=0.5, size=100 → left=-50");
			assertEqual(50.0f, ui.getRightInSelf(), 0.001f, "pivot=0.5, size=100 → right=50");
			assertEqual(25.0f, ui.getTopInSelf(), 0.001f, "pivot=0.5, size=50 → top=25");
			assertEqual(-25.0f, ui.getBottomInSelf(), 0.001f, "pivot=0.5, size=50 → bottom=-25");
			// pivot=(0,0): left=0, right=100, top=50, bottom=0
			ui.setPivot(new Vector2(0.0f, 0.0f));
			assertEqual(0.0f, ui.getLeftInSelf(), 0.001f, "pivot=0 → left=0");
			assertEqual(100.0f, ui.getRightInSelf(), 0.001f, "pivot=0 → right=100");
			assertEqual(50.0f, ui.getTopInSelf(), 0.001f, "pivot=0 → top=50");
			assertEqual(0.0f, ui.getBottomInSelf(), 0.001f, "pivot=0 → bottom=0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setPosition/getPosition — localPosition 读写
	// ═════════════════════════════════════════════════════════════════
	private static void testPosition()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			Vector3 pos = ui.getPosition();
			assertEqual(10.0f, pos.x, 0.001f, "setPosition 后 x=10");
			assertEqual(20.0f, pos.y, 0.001f, "setPosition 后 y=20");
			// setPositionX 只改 x
			ui.setPositionX(30.0f);
			pos = ui.getPosition();
			assertEqual(30.0f, pos.x, 0.001f, "setPositionX 后 x=30");
			assertEqual(20.0f, pos.y, 0.001f, "setPositionX 不影响 y");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getLeftInParent/getRightInParent/getTopInParent/getBottomInParent
	// = getPosition + self 边界(无父也成立, 纯公式)
	// ═════════════════════════════════════════════════════════════════
	private static void testParentBounds()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			ui.setPosition(new Vector3(20.0f, 10.0f, 0.0f));
			// pivot=0.5: leftInSelf=-50, rightInSelf=50 → leftInParent=20-50=-30, rightInParent=20+50=70
			assertEqual(-30.0f, ui.getLeftInParent(), 0.001f, "leftInParent = pos.x + leftInSelf = -30");
			assertEqual(70.0f, ui.getRightInParent(), 0.001f, "rightInParent = pos.x + rightInSelf = 70");
			assertEqual(35.0f, ui.getTopInParent(), 0.001f, "topInParent = pos.y + topInSelf = 10+25=35");
			assertEqual(-15.0f, ui.getBottomInParent(), 0.001f, "bottomInParent = pos.y + bottomInSelf = 10-25=-15");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setLeftToOtherLeft 等对齐方法 — 无父时 getParent 都 null → 走对齐公式
	// setPositionX(other.x - other.size.x/2 + self.size.x/2 + interval)
	// ═════════════════════════════════════════════════════════════════
	private static void testAlignToOther()
	{
		GameObject goA = new GameObject("TestUIA");
		GameObject goB = new GameObject("TestUIB");
		try
		{
			goA.AddComponent<RectTransform>();
			goB.AddComponent<RectTransform>();
			myUGUIObject uiA = new myUGUIObject();
			uiA.setObject(goA);
			uiA.init();
			myUGUIObject uiB = new myUGUIObject();
			uiB.setObject(goB);
			uiB.init();
			// A: size=100x50, pos=(10,0); B: size=60x30, interval=5
			uiA.setSize(new Vector2(100.0f, 50.0f));
			uiA.setPosition(new Vector3(10.0f, 0.0f, 0.0f));
			uiB.setSize(new Vector2(60.0f, 30.0f));
			uiB.setLeftToOtherLeft(uiA, 5.0f);
			// B 新 x = 10 - 50 + 30 + 5 = -5; B 左边界 = -5 - 30 = -35 = A 左边界(-40)+5
			Vector3 posB = uiB.getPosition();
			assertEqual(-5.0f, posB.x, 0.001f, "setLeftToOtherLeft 后 B.x=-5");
			assertEqual(-35.0f, uiB.getLeftInParent(), 0.001f, "B 左边界对齐 A 左边界+interval");
			// setLeftToOtherRight: B 左边界 = A 右边界 + interval
			// B 新 x = 10 + 50 + 30 + 5 = 95 → B 左边界 = 95-30 = 65 = A 右边界(60)+5
			uiB.setLeftToOtherRight(uiA, 5.0f);
			posB = uiB.getPosition();
			assertEqual(95.0f, posB.x, 0.001f, "setLeftToOtherRight 后 B.x=95");
			assertEqual(65.0f, uiB.getLeftInParent(), 0.001f, "B 左边界对齐 A 右边界+interval");
			// setRightToOtherLeft: B 右边界 = A 左边界 - interval
			// B 新 x = 10 - 50 - 30 - 5 = -75 → B 右边界 = -75+30 = -45 = A 左边界(-40)-5
			uiB.setRightToOtherLeft(uiA, 5.0f);
			posB = uiB.getPosition();
			assertEqual(-75.0f, posB.x, 0.001f, "setRightToOtherLeft 后 B.x=-75");
			assertEqual(-45.0f, uiB.getRightInParent(), 0.001f, "B 右边界对齐 A 左边界-interval");
		}
		finally
		{
			UObject.DestroyImmediate(goA);
			UObject.DestroyImmediate(goB);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setInParentCenterX/Y — 位置归零(无父时即 localPosition=0)
	// ═════════════════════════════════════════════════════════════════
	private static void testInParentCenter()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setPosition(new Vector3(30.0f, 40.0f, 0.0f));
			ui.setInParentCenterX();
			assertEqual(0.0f, ui.getPosition().x, 0.001f, "setInParentCenterX 后 x=0");
			assertEqual(40.0f, ui.getPosition().y, 0.001f, "setInParentCenterX 不影响 y");
			ui.setInParentCenterY();
			assertEqual(0.0f, ui.getPosition().y, 0.001f, "setInParentCenterY 后 y=0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setLeftInParent/setRightInParent/setTopInParent/setBottomInParent
	// 与 getXxxInParent 互为逆运算: set 后 get 还原
	// ═════════════════════════════════════════════════════════════════
	private static void testSetInParentRoundTrip()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			// 目标边界值
			ui.setLeftInParent(-30.0f);
			assertEqual(-30.0f, ui.getLeftInParent(), 0.001f, "setLeftInParent 后 getLeftInParent 还原");
			ui.setRightInParent(70.0f);
			assertEqual(70.0f, ui.getRightInParent(), 0.001f, "setRightInParent 后 getRightInParent 还原");
			ui.setTopInParent(35.0f);
			assertEqual(35.0f, ui.getTopInParent(), 0.001f, "setTopInParent 后 getTopInParent 还原");
			ui.setBottomInParent(-15.0f);
			assertEqual(-15.0f, ui.getBottomInParent(), 0.001f, "setBottomInParent 后 getBottomInParent 还原");
			// 逆运算一致性: 设左边界-30 → position.x = -30 - leftInSelf = -30+50 = 20
			assertEqual(20.0f, ui.getPosition().x, 0.001f, "setLeftInParent(-30) → pos.x=20");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// cloneFrom — 同类型克隆 position/rotation/scale
	// ═════════════════════════════════════════════════════════════════
	private static void testCloneFrom()
	{
		GameObject goA = new GameObject("TestUIA");
		GameObject goB = new GameObject("TestUIB");
		try
		{
			goA.AddComponent<RectTransform>();
			goB.AddComponent<RectTransform>();
			myUGUIObject src = new myUGUIObject();
			src.setObject(goA);
			src.init();
			myUGUIObject dst = new myUGUIObject();
			dst.setObject(goB);
			dst.init();
			src.setPosition(new Vector3(15.0f, 25.0f, 5.0f));
			src.setScale(new Vector3(2.0f, 2.0f, 2.0f));
			dst.cloneFrom(src);
			Vector3 pos = dst.getPosition();
			assertEqual(15.0f, pos.x, 0.001f, "cloneFrom 复制 position.x");
			assertEqual(25.0f, pos.y, 0.001f, "cloneFrom 复制 position.y");
			Vector3 scale = dst.getScale();
			assertEqual(2.0f, scale.x, 0.001f, "cloneFrom 复制 scale.x");
		}
		finally
		{
			UObject.DestroyImmediate(goA);
			UObject.DestroyImmediate(goB);
		}
	}
}
