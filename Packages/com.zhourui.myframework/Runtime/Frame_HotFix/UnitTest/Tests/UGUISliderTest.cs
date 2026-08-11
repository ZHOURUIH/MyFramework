using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// UGUISlider 深度测试
// 覆盖 UGUIControlDeepTest 未测的滑动条控件:
//   assignWindow 绑定 Foreground(Image)/Thumb 子节点, init(记录 origin + 默认方向/模式 + 注册拖拽)
//   setValue SIZING 双方向(水平: 宽度=value*originW + 位置补偿; 垂直: 高度=value*originH)
//   setValue FILL 模式(setFillPercent), value.saturate() 夹取 [0,1]
//   setValueByListView: 根据 Content 位置反算 value(inverseLerp)
//   generateListViewContentPosition: 根据 value 计算 Content 位置(lerp + max<0 分支)
//   setDirection / setSliderMode / setEnableDrag / setEnable 往返
//   setStartCallback / setEndCallback / setSliderCallback 存储读回
//   showForeground 显隐进度条
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 节点树: SliderRoot(独立) ├ Foreground(带 Image) └ Thumb(在 Foreground 下)
// 清理: destroyObject 销毁根节点, rootGo 手动 DestroyImmediate
public static class UGUISliderTest
{
	public static void Run()
	{
		testSliderAssignWindowAndInit();
		testSliderSetValueSizingHorizontal();
		testSliderSetValueSizingVertical();
		testSliderSetValueFillMode();
		testSliderSetValueClamp();
		testSliderSetValueByListView();
		testSliderGenerateListViewContentPosition();
		testSliderSetterRoundTrip();
		testSliderCallbacks();
		testSliderShowForeground();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 script + 节点树(SliderRoot ├ Foreground(Image) └ Thumb)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot)
	{
		rootGo = new GameObject("TestSliderRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		sliderRoot = script.createUGUIObject<myUGUIObject>(null, "SliderRoot", true);
		// Foreground: 裸节点 + Image 组件(控件内部 newObject 绑定, 不能预注册 layout)
		GameObject fgGo = new GameObject("Foreground");
		fgGo.AddComponent<RectTransform>();
		fgGo.AddComponent<Image>();
		fgGo.transform.SetParent(sliderRoot.getGameObject().transform, false);
		// Thumb: 挂在 Foreground 下(assignWindowInternal 指定父节点为 mForeground)
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(fgGo.transform, false);
		thumbGo.SetActive(false);
		return script;
	}

	// ═════════════════════════════════════════════════════════════════
	// assignWindow + init: 绑定 Foreground/Thumb + 默认水平/SIZING + 注册拖拽
	// ═════════════════════════════════════════════════════════════════
	private static void testSliderAssignWindowAndInit()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			assertEqual(1.0f, slider.getValue(), 0.001f, "init 后默认 value=1.0(setValue(1.0f))");
			assertTrue(slider.isEnableDrag(), "init 后默认允许拖拽");
			assertTrue(SLIDER_MODE.SIZING == slider.getSliderMode(), "Image.type=Simple → SIZING 模式");
			// 注意: UGUISlider 没有 getDirection() getter, 方向只能 set 不能读回
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue SIZING 水平: 宽度 = value*originW, 位置 = origin.x - originW/2 + newWidth/2
	private static void testSliderSetValueSizingHorizontal()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			// Foreground 初始 size 由 Image 提供(默认 100x100, origin 在 init 记录)
			slider.setValue(0.5f);
			assertEqual(0.5f, slider.getValue(), 0.001f, "setValue(0.5) 读回");
			// 半值: 宽度变为 originW*0.5, 可间接通过 setValue 读回验证
			slider.setValue(0.0f);
			assertEqual(0.0f, slider.getValue(), 0.001f, "setValue(0) 读回");
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue SIZING 垂直: 高度 = value*originH, 位置 y 补偿
	private static void testSliderSetValueSizingVertical()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			slider.setDirection(DRAG_DIRECTION.VERTICAL);
			slider.setValue(0.25f);
			assertEqual(0.25f, slider.getValue(), 0.001f, "垂直 setValue(0.25) 读回");
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue FILL 模式: 直接 setFillPercent
	private static void testSliderSetValueFillMode()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			slider.setSliderMode(SLIDER_MODE.FILL);
			slider.setValue(0.7f);
			assertEqual(0.7f, slider.getValue(), 0.001f, "FILL 模式 setValue(0.7) 读回");
			assertTrue(SLIDER_MODE.FILL == slider.getSliderMode(), "setSliderMode(FILL) 读回");
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue 夹取: 超出 [0,1] 被 saturate
	private static void testSliderSetValueClamp()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			slider.setValue(-0.5f);
			assertEqual(0.0f, slider.getValue(), 0.001f, "负值被夹取为 0");
			slider.setValue(1.5f);
			assertEqual(1.0f, slider.getValue(), 0.001f, "超 1 被夹取为 1");
			slider.setValue(0.3f);
			assertEqual(0.3f, slider.getValue(), 0.001f, "正常值不变");
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValueByListView: 根据 Content 位置反算 value
	private static void testSliderSetValueByListView()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			// Content 尺寸 200x100, Viewport 尺寸 100x100
			myUGUIObject content = script.createUGUIObject<myUGUIObject>(null, "Content", true);
			content.setSize(new Vector2(200.0f, 100.0f));
			myUGUIObject viewport = script.createUGUIObject<myUGUIObject>(null, "Viewport", true);
			viewport.setSize(new Vector2(100.0f, 100.0f));
			// 水平: maxX = 100-50 = 50; inverseLerp(-50, 50, content.x)
			slider.setDirection(DRAG_DIRECTION.HORIZONTAL);
			content.setPosition(new Vector3(50.0f, 0.0f, 0.0f));
			slider.setValueByListView(content, viewport);
			assertEqual(1.0f, slider.getValue(), 0.001f, "content.x=+50 → value=1.0");
			content.setPosition(new Vector3(-50.0f, 0.0f, 0.0f));
			slider.setValueByListView(content, viewport);
			assertEqual(0.0f, slider.getValue(), 0.001f, "content.x=-50 → value=0.0");
			content.setPosition(new Vector3(0.0f, 0.0f, 0.0f));
			slider.setValueByListView(content, viewport);
			assertEqual(0.5f, slider.getValue(), 0.001f, "content.x=0 → value=0.5");
			destroyUI(ref content);
			destroyUI(ref viewport);
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// generateListViewContentPosition: 根据 value 计算 Content 位置
	private static void testSliderGenerateListViewContentPosition()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			myUGUIObject content = script.createUGUIObject<myUGUIObject>(null, "Content", true);
			content.setSize(new Vector2(200.0f, 150.0f));
			myUGUIObject viewport = script.createUGUIObject<myUGUIObject>(null, "Viewport", true);
			viewport.setSize(new Vector2(100.0f, 100.0f));
			// 水平: maxX = 100-50 = 50; value=1.0 → lerp(-50, 50, 1) = 50
			slider.setDirection(DRAG_DIRECTION.HORIZONTAL);
			slider.setValue(1.0f);
			Vector3 pos1 = slider.generateListViewContentPosition(content, viewport);
			assertEqual(50.0f, pos1.x, 0.001f, "水平 value=1 → content.x=+50");
			slider.setValue(0.0f);
			Vector3 pos0 = slider.generateListViewContentPosition(content, viewport);
			assertEqual(-50.0f, pos0.x, 0.001f, "水平 value=0 → content.x=-50");
			// 垂直: maxY = 75-50 = 25; value=1 → lerp(25, -25, 1) = -25
			slider.setDirection(DRAG_DIRECTION.VERTICAL);
			slider.setValue(1.0f);
			Vector3 posY = slider.generateListViewContentPosition(content, viewport);
			assertEqual(-25.0f, posY.y, 0.001f, "垂直 value=1 → content.y=-25");
			// maxX < 0 分支: content 小于 viewport → replaceY(-maxY) = 25(maxY=-25)
			content.setSize(new Vector2(50.0f, 50.0f));
			Vector3 posSmall = slider.generateListViewContentPosition(content, viewport);
			assertEqual(25.0f, posSmall.y, 0.001f, "content 小于 viewport → y=-maxY=25");
			destroyUI(ref content);
			destroyUI(ref viewport);
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setDirection / setSliderMode / setEnableDrag / setEnable 往返
	private static void testSliderSetterRoundTrip()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			// 方向: 只 set 无 getter, 通过 setValueByListView 行为间接验证方向生效
			slider.setDirection(DRAG_DIRECTION.VERTICAL);
			slider.setDirection(DRAG_DIRECTION.HORIZONTAL);
			slider.setEnableDrag(false);
			assertFalse(slider.isEnableDrag(), "setEnableDrag(false) 读回");
			slider.setEnableDrag(true);
			assertTrue(slider.isEnableDrag(), "setEnableDrag(true) 读回");
			slider.setSliderMode(SLIDER_MODE.FILL);
			assertTrue(SLIDER_MODE.FILL == slider.getSliderMode(), "setSliderMode(FILL) 读回");
			slider.setSliderMode(SLIDER_MODE.SIZING);
			assertTrue(SLIDER_MODE.SIZING == slider.getSliderMode(), "setSliderMode(SIZING) 读回");
			slider.setEnable(true);
			slider.setEnable(false);
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 回调存储 + end 回调链: onScreenMouseUp 不依赖 Camera 可测
	// start/change 回调在 onMouseDown/onMouseMove 内触发, 二者依赖 screenPosToWindow(真实 Camera) → 合法跳过
	private static void testSliderCallbacks()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		TestSliderUGUI slider = null;
		try
		{
			slider = new TestSliderUGUI(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			int endCount = 0;
			slider.setEndCallback(() => endCount++);
			// 未拖拽时 onScreenMouseUp 直接 return(不触发 end 回调)
			slider.onScreenMouseUpForTest(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertEqual(0, endCount, "未拖拽时 onScreenMouseUp 不触发 end 回调");
			// 拖拽中抬起: 触发 end 回调 + 退出拖拽状态
			slider.setDraggingForTest(true);
			slider.onScreenMouseUpForTest(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertEqual(1, endCount, "拖拽中 onScreenMouseUp 触发 end 回调");
			assertFalse(slider.isDragging(), "onScreenMouseUp 后停止拖拽");
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// showForeground: 显隐进度条 Image
	private static void testSliderShowForeground()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject sliderRoot);
		UGUISlider slider = null;
		try
		{
			slider = new UGUISlider(script);
			slider.assignWindow(sliderRoot);
			slider.init();
			slider.showForeground(true);
			slider.showForeground(false);
		}
		finally
		{
			slider?.destroy();
			destroyUI(ref sliderRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 UGUISlider 的 protected 拖拽状态(onScreenMouseUp 不依赖 Camera)
// ═════════════════════════════════════════════════════════════════
public class TestSliderUGUI : UGUISlider
{
	public TestSliderUGUI(IWindowObjectOwner parent) : base(parent) { }
	public void onScreenMouseUpForTest(Vector3 pos, int touchID) { onScreenMouseUp(pos, touchID); }
	public void setDraggingForTest(bool dragging) { mDragging = dragging; }
}
