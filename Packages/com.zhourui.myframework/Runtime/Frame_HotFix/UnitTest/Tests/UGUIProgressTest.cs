using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// UGUIProgress 深度测试(进度条控件)
// setValue: mProgressValue = value.saturate() 夹到 [0,1]
//   SIZING 模式: newWidth = value*originW, bar.x = originX - originW*0.5 + newWidth*0.5, bar.size=(newWidth, originH)
//   FILL 模式: bar.setFillPercent(value)(Image.fillAmount)
//   mThumb?.setPositionX((value-0.5)*originW)
// 测试环境: TestUGUIProgress(传 null parent, 不调 init, 手动注入 bar/thumb/origin/mode)
//   bar = 预加 Image 的 myUGUIImageSimple(setObject+init), thumb = myUGUIObject(setObject+init)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class UGUIProgressTest
{
	public static void Run()
	{
		testSizingMode();
		testSizingBoundary();
		testSaturate();
		testFillMode();
		testThumbNullSafe();
		testModeGetSet();
		testOriginProgressGetSet();
		testSetValueSaturateNegative();
		testSetValueSaturateOverOne();
		testSetValueZero();
		testFillModePercent();
		testThumbPositionRange();
		testModeSwitchAfterSetValue();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建进度条 + bar + thumb
	// ═════════════════════════════════════════════════════════════════
	private static TestUGUIProgress createProgress(out GameObject barGo, out GameObject thumbGo)
	{
		// parent 不能传 null: WindowObjectFixedT 构造里 mScript.addWindowObject(this) 会 NRE
		TestUGUIProgress progress = new TestUGUIProgress(new TestLayoutScriptDeep());

		barGo = new GameObject("ProgressBarGO");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		myUGUIImageSimple bar = new myUGUIImageSimple();
		bar.setObject(barGo);
		bar.init();
		progress.setBarForTest(bar);

		thumbGo = new GameObject("ThumbGO");
		myUGUIObject thumb = new myUGUIObject();
		thumb.setObject(thumbGo);
		thumb.init();
		progress.setThumbForTest(thumb);

		progress.setOriginForTest(new Vector2(100.0f, 20.0f), new Vector3(50.0f, 0.0f, 0.0f));
		progress.setModeForTest(SLIDER_MODE.SIZING);
		return progress;
	}

	// SIZING: value=0.5 → bar.x=25, bar.size=(50,20), thumb.x=0
	private static void testSizingMode()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(0.5f);
			assertEqual(0.5f, progress.getValue(), 0.001f, "getValue 读回 0.5");
			assertEqual(25.0f, progress.getBarPositionX(), 0.001f, "bar.x = 50-50+25 = 25");
			assertEqual(50.0f, progress.getBarSizeX(), 0.001f, "bar 宽度 = 0.5*100 = 50");
			assertEqual(20.0f, progress.getBarSizeY(), 0.001f, "bar 高度不变 = 20");
			assertEqual(0.0f, progress.getThumbPositionX(), 0.001f, "thumb.x = (0.5-0.5)*100 = 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// SIZING: value=1 与 value=0 边界
	private static void testSizingBoundary()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(1.0f);
			assertEqual(50.0f, progress.getBarPositionX(), 0.001f, "value=1 bar.x = 50-50+50 = 50");
			assertEqual(100.0f, progress.getBarSizeX(), 0.001f, "value=1 bar 宽度 = 100");
			assertEqual(50.0f, progress.getThumbPositionX(), 0.001f, "value=1 thumb.x = 50");
			progress.setValue(0.0f);
			assertEqual(0.0f, progress.getBarPositionX(), 0.001f, "value=0 bar.x = 50-50+0 = 0");
			assertEqual(0.0f, progress.getBarSizeX(), 0.001f, "value=0 bar 宽度 = 0");
			assertEqual(-50.0f, progress.getThumbPositionX(), 0.001f, "value=0 thumb.x = -50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// saturate: 超界值夹到 [0,1]
	private static void testSaturate()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(1.5f);
			assertEqual(1.0f, progress.getValue(), 0.001f, "value=1.5 夹到 1");
			progress.setValue(-0.5f);
			assertEqual(0.0f, progress.getValue(), 0.001f, "value=-0.5 夹到 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// FILL: bar 的 Image.fillAmount 跟随进度
	private static void testFillMode()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setModeForTest(SLIDER_MODE.FILL);
			progress.setValue(0.7f);
			assertEqual(0.7f, progress.getBarFillPercent(), 0.001f, "FILL 模式 fillAmount = 0.7");
			progress.setValue(0.0f);
			assertEqual(0.0f, progress.getBarFillPercent(), 0.001f, "FILL 模式 fillAmount = 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// mThumb 为 null 时 setValue 不崩溃(mThumb?. 空安全)
	private static void testThumbNullSafe()
	{
		// parent 不能传 null: WindowObjectFixedT 构造里 mScript.addWindowObject(this) 会 NRE
		TestUGUIProgress progress = new TestUGUIProgress(new TestLayoutScriptDeep());
		GameObject barGo = new GameObject("ProgressBarOnly");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		myUGUIImageSimple bar = new myUGUIImageSimple();
		bar.setObject(barGo);
		bar.init();
		progress.setBarForTest(bar);
		progress.setOriginForTest(new Vector2(100.0f, 20.0f), new Vector3(50.0f, 0.0f, 0.0f));
		progress.setModeForTest(SLIDER_MODE.SIZING);
		try
		{
			progress.setValue(0.5f);   // 未设 thumb, 空安全不崩
			assertEqual(0.5f, progress.getValue(), 0.001f, "无 thumb 时 setValue 正常");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
		}
	}

	// setSliderMode/getSliderMode 读写
	private static void testModeGetSet()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setSliderMode(SLIDER_MODE.FILL);
			assertTrue(progress.getSliderMode() == SLIDER_MODE.FILL, "setSliderMode(FILL) 读回");
			progress.setSliderMode(SLIDER_MODE.SIZING);
			assertTrue(progress.getSliderMode() == SLIDER_MODE.SIZING, "setSliderMode(SIZING) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// getOriginProgressSize/Position: 纯字段读回(createProgress 注入 origin 值)
	private static void testOriginProgressGetSet()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			Vector2 size = progress.getOriginProgressSize();
			assertEqual(100.0f, size.x, 0.0001f, "originSize.x 读回 100");
			assertEqual(20.0f, size.y, 0.0001f, "originSize.y 读回 20");
			Vector3 pos = progress.getOriginProgressPosition();
			assertEqual(50.0f, pos.x, 0.0001f, "originPosition.x 读回 50");
			assertEqual(0.0f, pos.z, 0.0001f, "originPosition.z 读回 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// setValue 负数: saturate 到 0, SIZING 模式下 bar 尺寸为 0, 位置回到最左
	private static void testSetValueSaturateNegative()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(-5.0f);
			assertEqual(0.0f, progress.getValue(), 0.0001f, "负值被 saturate 到 0");
			assertEqual(0.0f, progress.getBarSizeX(), 0.0001f, "bar 尺寸为 0");
			// bar.x = originX - originW*0.5 + 0 = 50 - 50 = 0
			assertEqual(0.0f, progress.getBarPositionX(), 0.0001f, "bar 位置回到最左");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// setValue 超 1: saturate 到 1, bar 尺寸等于 originW
	private static void testSetValueSaturateOverOne()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(2.0f);
			assertEqual(1.0f, progress.getValue(), 0.0001f, "超 1 被 saturate 到 1");
			assertEqual(100.0f, progress.getBarSizeX(), 0.0001f, "bar 尺寸为 originW(100)");
			assertEqual(50.0f, progress.getBarPositionX(), 0.0001f, "bar 位置回到原点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// setValue(0): 边界值
	private static void testSetValueZero()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(0.0f);
			assertEqual(0.0f, progress.getValue(), 0.0001f, "setValue(0) 读回 0");
			assertEqual(0.0f, progress.getBarSizeX(), 0.0001f, "bar 尺寸 0");
			assertEqual(0.0f, progress.getBarPositionX(), 0.0001f, "bar 位置最左");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// FILL 模式: setValue 写 fillPercent
	private static void testFillModePercent()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setModeForTest(SLIDER_MODE.FILL);
			progress.setValue(0.5f);
			assertEqual(0.5f, progress.getValue(), 0.0001f, "getValue 0.5");
			assertEqual(0.5f, progress.getBarFillPercent(), 0.0001f, "fillPercent 0.5");
			progress.setValue(0.75f);
			assertEqual(0.75f, progress.getBarFillPercent(), 0.0001f, "fillPercent 0.75");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// thumb 位置: (value - 0.5) * originW, value=1 → +50, value=0 → -50
	private static void testThumbPositionRange()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(1.0f);
			assertEqual(50.0f, progress.getThumbPositionX(), 0.0001f, "value=1 thumb.x=+50");
			progress.setValue(0.0f);
			assertEqual(-50.0f, progress.getThumbPositionX(), 0.0001f, "value=0 thumb.x=-50");
			progress.setValue(0.5f);
			assertEqual(0.0f, progress.getThumbPositionX(), 0.0001f, "value=0.5 thumb.x=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}

	// 模式切换: SIZING setValue 后切 FILL 再 setValue, fillPercent 生效
	private static void testModeSwitchAfterSetValue()
	{
		TestUGUIProgress progress = createProgress(out GameObject barGo, out GameObject thumbGo);
		try
		{
			progress.setValue(0.3f);   // SIZING 模式
			assertEqual(30.0f, progress.getBarSizeX(), 0.0001f, "SIZING value=0.3 bar 宽 30");
			progress.setModeForTest(SLIDER_MODE.FILL);
			progress.setValue(0.6f);
			assertEqual(0.6f, progress.getBarFillPercent(), 0.0001f, "切 FILL 后 fillPercent 0.6");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 UGUIProgress 的 protected 字段 + 提供断言读取
// ═════════════════════════════════════════════════════════════════
public class TestUGUIProgress : UGUIProgress
{
	public TestUGUIProgress(IWindowObjectOwner parent) : base(parent) { }

	public void setBarForTest(myUGUIImageSimple bar) { mProgressBar = bar; }

	public void setThumbForTest(myUGUIObject thumb) { mThumb = thumb; }

	public void setOriginForTest(Vector2 size, Vector3 pos)
	{
		mOriginProgressSize = size;
		mOriginProgressPosition = pos;
	}

	public void setModeForTest(SLIDER_MODE mode) { mMode = mode; }

	public float getBarPositionX() { return mProgressBar.getPosition().x; }

	public float getBarSizeX() { return mProgressBar.getSize().x; }

	public float getBarSizeY() { return mProgressBar.getSize().y; }

	public float getThumbPositionX() { return mThumb.getPosition().x; }

	public float getBarFillPercent() { return mProgressBar.getFillPercent(); }
}
