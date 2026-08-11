using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// UGUIAnimProgress 深度测试
// 覆盖 UGUIControlDeepTest 未测的动画进度条(与 UGUIProgress 同构, 但进度条是 myUGUIImageAnim):
//   assignWindow 绑定 ProgressBar(myUGUIImageAnim + Image)/Thumb 子节点
//   init(记录 origin size/pos + 模式判断 Image.type==Filled → FILL 否则 SIZING)
//   setValue: value.saturate() 夹取[0,1] + SIZING(宽度=value*originW, 位置补偿) / FILL(fillAmount)
//   mThumb.x = (value-0.5)*originW
//   getValue / setSliderMode / getSliderMode / showForeground / getProgressBar
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 节点树: AnimRoot(独立) ├ ProgressBar(带 Image) └ Thumb
// 清理: destroyObject 销毁根节点, rootGo 手动 DestroyImmediate
public static class UGUIAnimProgressTest
{
	public static void Run()
	{
		testAnimProgressAssignWindowAndInit();
		testAnimProgressSetValueSizing();
		testAnimProgressSetValueClamp();
		testAnimProgressThumbPosition();
		testAnimProgressFillMode();
		testAnimProgressShowForeground();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 script + 节点树(AnimRoot ├ ProgressBar(Image) └ Thumb)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot)
	{
		rootGo = new GameObject("TestAnimProgressRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		animRoot = script.createUGUIObject<myUGUIObject>(null, "AnimRoot", true);
		// ProgressBar: 裸节点 + Image 组件(控件内部 newObject 绑定, 不能预注册 layout)
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(animRoot.getGameObject().transform, false);
		// Thumb
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(animRoot.getGameObject().transform, false);
		thumbGo.SetActive(false);
		return script;
	}

	// ═════════════════════════════════════════════════════════════════
	// assignWindow + init: 绑定 ProgressBar/Thumb + 记录 origin + 模式判断
	// ═════════════════════════════════════════════════════════════════
	private static void testAnimProgressAssignWindowAndInit()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot);
		UGUIAnimProgress progress = null;
		try
		{
			progress = new UGUIAnimProgress(script);
			progress.assignWindow(animRoot);
			progress.init();
			assertEqual(100.0f, progress.getOriginProgressSize().x, 0.001f, "origin 宽度=ProgressBar 预设 100");
			assertEqual(10.0f, progress.getOriginProgressSize().y, 0.001f, "origin 高度=10");
			assertEqual(0.0f, progress.getOriginProgressPosition().x, 0.001f, "origin 位置 x=0");
			assertTrue(SLIDER_MODE.SIZING == progress.getSliderMode(), "Image.type=Simple → SIZING 模式");
			assertNotNull(progress.getProgressBar(), "getProgressBar 返回非 null");
		}
		finally
		{
			progress?.destroy();
			destroyUI(ref animRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue SIZING: 宽度=value*originW, 位置=origin.x - originW/2 + newWidth/2
	private static void testAnimProgressSetValueSizing()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot);
		UGUIAnimProgress progress = null;
		try
		{
			progress = new UGUIAnimProgress(script);
			progress.assignWindow(animRoot);
			progress.init();
			progress.setValue(1.0f);
			assertEqual(1.0f, progress.getValue(), 0.001f, "setValue(1) 读回");
			// origin.x=0, originW=100 → 位置 = 0 - 50 + 50 = 0
			myUGUIObject bar = progress.getProgressBar();
			assertEqual(0.0f, bar.getPosition().x, 0.001f, "value=1 时位置 x=0(全宽)");
			progress.setValue(0.5f);
			assertEqual(0.5f, progress.getValue(), 0.001f, "setValue(0.5) 读回");
			// 位置 = 0 - 50 + 25 = -25
			assertEqual(-25.0f, bar.getPosition().x, 0.001f, "value=0.5 时位置 x=-25");
			// 宽度 = 0.5 * 100 = 50
			assertEqual(50.0f, bar.getSize().x, 0.001f, "value=0.5 时宽度=50");
			progress.setValue(0.0f);
			assertEqual(0.0f, bar.getSize().x, 0.001f, "value=0 时宽度=0");
		}
		finally
		{
			progress?.destroy();
			destroyUI(ref animRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue 夹取: 超出 [0,1] 被 saturate
	private static void testAnimProgressSetValueClamp()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot);
		UGUIAnimProgress progress = null;
		try
		{
			progress = new UGUIAnimProgress(script);
			progress.assignWindow(animRoot);
			progress.init();
			progress.setValue(-1.0f);
			assertEqual(0.0f, progress.getValue(), 0.001f, "负值被夹取为 0");
			progress.setValue(2.0f);
			assertEqual(1.0f, progress.getValue(), 0.001f, "超 1 被夹取为 1");
			progress.setValue(0.3f);
			assertEqual(0.3f, progress.getValue(), 0.001f, "正常值不变");
		}
		finally
		{
			progress?.destroy();
			destroyUI(ref animRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// mThumb.x = (value-0.5)*originW
	private static void testAnimProgressThumbPosition()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot);
		UGUIAnimProgress progress = null;
		try
		{
			progress = new UGUIAnimProgress(script);
			progress.assignWindow(animRoot);
			progress.init();
			progress.setValue(0.5f);
			// Thumb 位置 x = (0.5-0.5)*100 = 0
			// 通过布局注册表查找 Thumb 节点验证
			myUGUIObject thumb = script.getLayout().getUIObject(findChildGo(animRoot, "Thumb"));
			assertNotNull(thumb, "Thumb 节点已注册");
			assertEqual(0.0f, thumb.getPosition().x, 0.001f, "value=0.5 时 Thumb.x=0");
			progress.setValue(1.0f);
			assertEqual(50.0f, thumb.getPosition().x, 0.001f, "value=1 时 Thumb.x=50");
			progress.setValue(0.0f);
			assertEqual(-50.0f, thumb.getPosition().x, 0.001f, "value=0 时 Thumb.x=-50");
		}
		finally
		{
			progress?.destroy();
			destroyUI(ref animRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setSliderMode(FILL) + setValue: 走 setFillPercent
	private static void testAnimProgressFillMode()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot);
		UGUIAnimProgress progress = null;
		try
		{
			progress = new UGUIAnimProgress(script);
			progress.assignWindow(animRoot);
			progress.init();
			progress.setSliderMode(SLIDER_MODE.FILL);
			assertTrue(SLIDER_MODE.FILL == progress.getSliderMode(), "setSliderMode(FILL) 读回");
			progress.setValue(0.7f);
			assertEqual(0.7f, progress.getValue(), 0.001f, "FILL 模式 setValue(0.7) 读回");
			// Image.fillAmount 被设置
			Image image = progress.getProgressBar().getGameObject().GetComponent<Image>();
			assertEqual(0.7f, image.fillAmount, 0.001f, "FILL 模式 fillAmount=0.7");
		}
		finally
		{
			progress?.destroy();
			destroyUI(ref animRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// showForeground: 显隐进度条 Image
	private static void testAnimProgressShowForeground()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject animRoot);
		UGUIAnimProgress progress = null;
		try
		{
			progress = new UGUIAnimProgress(script);
			progress.assignWindow(animRoot);
			progress.init();
			progress.showForeground(true);
			Image image = progress.getProgressBar().getGameObject().GetComponent<Image>();
			assertTrue(image.enabled, "showForeground(true) 后 Image 启用");
			progress.showForeground(false);
			assertFalse(image.enabled, "showForeground(false) 后 Image 禁用");
		}
		finally
		{
			progress?.destroy();
			destroyUI(ref animRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static GameObject findChildGo(myUGUIObject parent, string name)
	{
		Transform child = parent.getGameObject().transform.Find(name);
		if (child == null)
		{
			return null;
		}
		return child.gameObject;
	}

	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}
}
