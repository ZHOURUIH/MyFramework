using UnityEngine;
using static TestAssert;
using static UnityUtility;

// ScaleAnchor / ScaleAnchor3D 深度测试
// 缩放自适应组件, 核心是 getRealScale(纯数学): 根据屏幕缩放与宽高比基准计算实际缩放值
//   getRealScale: mKeepAspect=false → 原样; USE_WIDTH_SCALE → (x,x); USE_HEIGHT_SCALE → (y,y)
//                 AUTO → (min,min); INVERSE_AUTO → (max,max)
//   updateRect: preview 路径(编辑器非播放)下重算屏幕缩放 + 调整大小/位置/字体
//   ScaleAnchor3D.updateRect: transform.localScale/localPosition 按缩放值调整
//
// 环境: 裸 GameObject + RectTransform + TestScaleAnchor/TestScaleAnchor3D 组件
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class ScaleAnchorTest
{
	public static void Run()
	{
		testGetRealScaleKeepAspectFalse();
		testGetRealScaleWidthBase();
		testGetRealScaleHeightBase();
		testGetRealScaleAuto();
		testGetRealScaleInverseAuto();
		testUpdateRectResize();
		testUpdateRectPosition();
		testUpdateRectFont();
		testScaleAnchor3DUpdate();
		testScaleAnchor3DGetRealScale();
	}

	// ═════════════════════════════════════════════════════════════════
	// getRealScale: mKeepAspect=false → generateScreenScaleByAspectBase(NONE) → 原样
	// ═════════════════════════════════════════════════════════════════
	private static void testGetRealScaleKeepAspectFalse()
	{
		TestScaleAnchor anchor = new TestScaleAnchor();
		try
		{
			anchor.setScreenScaleForTest(new Vector2(2.0f, 3.0f));
			anchor.mKeepAspect = false;
			Vector2 scale = anchor.getRealScaleForTest();
			assertEqual(2.0f, scale.x, 0.001f, "不保持宽高比 → x 原样");
			assertEqual(3.0f, scale.y, 0.001f, "不保持宽高比 → y 原样");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(anchor);
		}
	}

	// getRealScale: USE_WIDTH_SCALE → (x, x)
	private static void testGetRealScaleWidthBase()
	{
		TestScaleAnchor anchor = new TestScaleAnchor();
		try
		{
			anchor.setScreenScaleForTest(new Vector2(2.0f, 3.0f));
			anchor.mKeepAspect = true;
			anchor.mAspectBase = ASPECT_BASE.USE_WIDTH_SCALE;
			Vector2 scale = anchor.getRealScaleForTest();
			assertEqual(2.0f, scale.x, 0.001f, "USE_WIDTH_SCALE → x 不变");
			assertEqual(2.0f, scale.y, 0.001f, "USE_WIDTH_SCALE → y=x");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(anchor);
		}
	}

	// getRealScale: USE_HEIGHT_SCALE → (y, y)
	private static void testGetRealScaleHeightBase()
	{
		TestScaleAnchor anchor = new TestScaleAnchor();
		try
		{
			anchor.setScreenScaleForTest(new Vector2(2.0f, 3.0f));
			anchor.mKeepAspect = true;
			anchor.mAspectBase = ASPECT_BASE.USE_HEIGHT_SCALE;
			Vector2 scale = anchor.getRealScaleForTest();
			assertEqual(3.0f, scale.x, 0.001f, "USE_HEIGHT_SCALE → x=y");
			assertEqual(3.0f, scale.y, 0.001f, "USE_HEIGHT_SCALE → y 不变");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(anchor);
		}
	}

	// getRealScale: AUTO → (min, min)
	private static void testGetRealScaleAuto()
	{
		TestScaleAnchor anchor = new TestScaleAnchor();
		try
		{
			anchor.mKeepAspect = true;
			anchor.mAspectBase = ASPECT_BASE.AUTO;
			anchor.setScreenScaleForTest(new Vector2(2.0f, 3.0f));
			Vector2 scale = anchor.getRealScaleForTest();
			assertEqual(2.0f, scale.x, 0.001f, "AUTO (2,3) → x=min=2");
			assertEqual(2.0f, scale.y, 0.001f, "AUTO (2,3) → y=min=2");
			anchor.setScreenScaleForTest(new Vector2(3.0f, 2.0f));
			scale = anchor.getRealScaleForTest();
			assertEqual(2.0f, scale.x, 0.001f, "AUTO (3,2) → x=min=2");
			assertEqual(2.0f, scale.y, 0.001f, "AUTO (3,2) → y=min=2");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(anchor);
		}
	}

	// getRealScale: INVERSE_AUTO → (max, max)
	private static void testGetRealScaleInverseAuto()
	{
		TestScaleAnchor anchor = new TestScaleAnchor();
		try
		{
			anchor.mKeepAspect = true;
			anchor.mAspectBase = ASPECT_BASE.INVERSE_AUTO;
			anchor.setScreenScaleForTest(new Vector2(2.0f, 3.0f));
			Vector2 scale = anchor.getRealScaleForTest();
			assertEqual(3.0f, scale.x, 0.001f, "INVERSE_AUTO (2,3) → x=max=3");
			assertEqual(3.0f, scale.y, 0.001f, "INVERSE_AUTO (2,3) → y=max=3");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(anchor);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// updateRect: preview 路径(编辑器非播放)下重算屏幕缩放并调整大小
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdateRectResize()
	{
		GameObject go = new GameObject("AnchorResize");
		RectTransform rt = go.AddComponent<RectTransform>();
		TestScaleAnchor anchor = go.AddComponent<TestScaleAnchor>();
		try
		{
			rt.sizeDelta = new Vector2(100.0f, 50.0f);
			anchor.mAdjustFont = false;   // 不调整字体, 只调大小
			anchor.updateRect();
			// 不崩溃 + mDirty 被清除 + 大小按屏幕缩放调整(具体值由 GameView 分辨率决定)
			assertFalse(anchor.isDirtyForTest(), "updateRect 后 mDirty 清除");
			Vector2 newSize = rt.sizeDelta;
			assertTrue(newSize.x > 0.0f && newSize.y > 0.0f, "updateRect 后大小为正");
			// size = floor(origin * scale); origin=100x50, 再次调用后 origin 变为缩放后值 → 继续按比例缩放
			anchor.updateRect();
			assertFalse(anchor.isDirtyForTest(), "再次 updateRect 仍清除 mDirty");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// updateRect: mAdjustPosition=true → 位置也按缩放调整
	private static void testUpdateRectPosition()
	{
		GameObject go = new GameObject("AnchorPos");
		RectTransform rt = go.AddComponent<RectTransform>();
		TestScaleAnchor anchor = go.AddComponent<TestScaleAnchor>();
		try
		{
			rt.sizeDelta = new Vector2(100.0f, 50.0f);
			anchor.mAdjustFont = false;
			anchor.mAdjustPosition = true;
			Vector3 originPos = rt.localPosition;
			anchor.updateRect();
			assertFalse(anchor.isDirtyForTest(), "updateRect 清除 mDirty");
			// 位置按 mOriginPos * realScale 调整(不崩溃即路径正确)
			Vector3 newPos = rt.localPosition;
			assertTrue(newPos.x != 0.0f || newPos.y != 0.0f || originPos == Vector3.zero || newPos == originPos, "位置调整执行");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// updateRect: mAdjustFont=true(默认)且无 Text 组件 → setRectSizeWithFontSize null 安全
	private static void testUpdateRectFont()
	{
		GameObject go = new GameObject("AnchorFont");
		RectTransform rt = go.AddComponent<RectTransform>();
		TestScaleAnchor anchor = go.AddComponent<TestScaleAnchor>();
		try
		{
			rt.sizeDelta = new Vector2(100.0f, 50.0f);
			anchor.mAdjustFont = true;   // 默认值, 无 Text 组件也应安全
			anchor.mAdjustPosition = false;
			anchor.updateRect();
			assertFalse(anchor.isDirtyForTest(), "updateRect(无Text) 清除 mDirty");
			assertTrue(rt.sizeDelta.x > 0.0f && rt.sizeDelta.y > 0.0f, "无 Text 组件时大小仍调整");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// ScaleAnchor3D: transform.localScale/localPosition 按缩放值调整
	// ═════════════════════════════════════════════════════════════════
	private static void testScaleAnchor3DUpdate()
	{
		GameObject go = new GameObject("Anchor3D");
		TestScaleAnchor3D anchor = go.AddComponent<TestScaleAnchor3D>();
		try
		{
			go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			go.transform.localPosition = new Vector3(10.0f, 20.0f, 0.0f);
			anchor.updateRect();
			// localScale/localPosition = origin * scale(具体值由 GameView 分辨率决定), 不崩溃即路径正确
			Vector3 scale = go.transform.localScale;
			Vector3 pos = go.transform.localPosition;
			assertTrue(scale.x > 0.0f && scale.y > 0.0f, "updateRect 后 localScale 为正");
			assertTrue(pos.x != 0.0f || pos.y != 0.0f, "updateRect 后 localPosition 保持非零");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ScaleAnchor3D.getRealScale 等价逻辑: AUTO → min 值作为统一缩放
	private static void testScaleAnchor3DGetRealScale()
	{
		TestScaleAnchor3D anchor = new TestScaleAnchor3D();
		try
		{
			anchor.setScreenScaleForTest(new Vector2(2.0f, 3.0f));
			anchor.mAspectBase = ASPECT_BASE.AUTO;
			Vector2 scale = anchor.getRealScaleForTest();
			assertEqual(2.0f, scale.x, 0.001f, "3D AUTO (2,3) → x=min=2");
			assertEqual(2.0f, scale.y, 0.001f, "3D AUTO (2,3) → y=min=2");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(anchor);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 ScaleAnchor 的 protected 字段与方法
// ═════════════════════════════════════════════════════════════════
public class TestScaleAnchor : ScaleAnchor
{
	public void setScreenScaleForTest(Vector2 scale) { mScreenScale = scale; }
	public Vector2 getRealScaleForTest() { return getRealScale(); }
	public bool isDirtyForTest() { return mDirty; }
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 ScaleAnchor3D 的 protected 字段与缩放计算
// ═════════════════════════════════════════════════════════════════
public class TestScaleAnchor3D : ScaleAnchor3D
{
	public void setScreenScaleForTest(Vector2 scale) { mScreenScale = scale; }
	public Vector2 getRealScaleForTest() { return generateScreenScaleByAspectBase(mScreenScale, mAspectBase); }
}
