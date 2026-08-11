using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// UGUI 控件深度测试(基于 MicroLegend 真实用法):
//   UIAttackTarget 里 `mHPBar.setValue(target.getHP()...)` → UGUIProgress
//   MicroLegend 高频: setChecked 43 次 / isChecked 52 次 → UGUICheckbox
//
// UGUIProgress(SIZING/FILL 双模式):
//   assignWindow 绑定 HPBar/ProgressBar/Thumb → init(记录 origin size/pos + 模式)
//   setValue: value.saturate() 夹取[0,1] + SIZING(宽度=value*originW, 位置补偿) / FILL(fillAmount)
//   mThumb.x = (value-0.5)*originW
// UGUICheckbox:
//   assignWindow 绑定 Mark/Label → init 注册 onCheckClick 碰撞
//   setChecked/isChecked / onCheckClick 切换 + 回调 / 不可交互不切换 / setLabel
public static class UGUIControlDeepTest
{
	public static void Run()
	{
		testProgressAssignWindowAndInit();
		testProgressSetValueSizing();
		testProgressSetValueClamp();
		testProgressThumbPosition();
		testProgressFillMode();
		testProgressShowForeground();
		testCheckboxAssignWindow();
		testCheckboxSetCheckedToggle();
		testCheckboxClickToggle();
		testCheckboxNonInteractable();
		testCheckboxSetLabel();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 TestLayoutScriptDeep + 父节点 GameObject
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScriptAndParent(out GameObject parentGo, out GameObject rootGo)
	{
		rootGo = new GameObject("TestDeepRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		parentGo = new GameObject("ParentUI");
		parentGo.AddComponent<RectTransform>();
		parentGo.transform.SetParent(rootGo.transform);
		return script;
	}

	// ═════════════════════════════════════════════════════════════════
	// UGUIProgress: assignWindow + init(origin size/pos + SIZING 模式)
	// ═════════════════════════════════════════════════════════════════
	private static void testProgressAssignWindowAndInit()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject hpGo = new GameObject("HPBar");
		hpGo.AddComponent<RectTransform>();
		hpGo.transform.SetParent(parentGo.transform);
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(hpGo.transform);
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(hpGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			UGUIProgress hp = new UGUIProgress(script);
			hp.assignWindow(parent, "HPBar");
			hp.init();
			assertEqual(100.0f, hp.getOriginProgressSize().x, 0.001f, "origin 宽度=ProgressBar 预设 100");
			assertEqual(10.0f, hp.getOriginProgressSize().y, 0.001f, "origin 高度=10");
			assertEqual(0.0f, hp.getOriginProgressPosition().x, 0.001f, "origin 位置 x=0");
			assertTrue(hp.getSliderMode() == SLIDER_MODE.SIZING, "Image.type=Sliced → SIZING 模式");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(hpGo);
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue(SIZING): 宽度=value*originW + 位置补偿
	private static void testProgressSetValueSizing()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject hpGo = new GameObject("HPBar");
		hpGo.AddComponent<RectTransform>();
		hpGo.transform.SetParent(parentGo.transform);
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(hpGo.transform);
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(hpGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			UGUIProgress hp = new UGUIProgress(script);
			hp.assignWindow(parent, "HPBar");
			hp.init();
			// setValue(0.5): 宽度=50, positionX = 0-50+25 = -25
			hp.setValue(0.5f);
			assertEqual(0.5f, hp.getValue(), 0.001f, "getValue 返回 0.5");
			assertEqual(50.0f, barGo.GetComponent<RectTransform>().sizeDelta.x, 0.001f, "宽度=0.5*100=50");
			assertEqual(-25.0f, barGo.transform.localPosition.x, 0.001f, "positionX = -originW/2+newW/2 = -25");
			// setValue(0.25): 宽度=25, positionX = -50+12.5 = -37.5
			hp.setValue(0.25f);
			assertEqual(25.0f, barGo.GetComponent<RectTransform>().sizeDelta.x, 0.001f, "宽度=0.25*100=25");
			assertEqual(-37.5f, barGo.transform.localPosition.x, 0.001f, "positionX = -37.5");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(hpGo);
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setValue 夹取: saturate → [0,1]
	private static void testProgressSetValueClamp()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject hpGo = new GameObject("HPBar");
		hpGo.AddComponent<RectTransform>();
		hpGo.transform.SetParent(parentGo.transform);
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(hpGo.transform);
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(hpGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			UGUIProgress hp = new UGUIProgress(script);
			hp.assignWindow(parent, "HPBar");
			hp.init();
			hp.setValue(-0.3f);
			assertEqual(0.0f, hp.getValue(), 0.001f, "负数夹取到 0");
			hp.setValue(2.0f);
			assertEqual(1.0f, hp.getValue(), 0.001f, "超 1 夹取到 1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(hpGo);
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// Thumb 位置: (value-0.5)*originW
	private static void testProgressThumbPosition()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject hpGo = new GameObject("HPBar");
		hpGo.AddComponent<RectTransform>();
		hpGo.transform.SetParent(parentGo.transform);
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(hpGo.transform);
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(hpGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			UGUIProgress hp = new UGUIProgress(script);
			hp.assignWindow(parent, "HPBar");
			hp.init();
			hp.setValue(0.25f);
			assertEqual(-25.0f, thumbGo.transform.localPosition.x, 0.001f, "Thumb.x = (0.25-0.5)*100 = -25");
			hp.setValue(1.0f);
			assertEqual(50.0f, thumbGo.transform.localPosition.x, 0.001f, "Thumb.x = (1-0.5)*100 = 50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(hpGo);
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// FILL 模式: setFillPercent → Image.fillAmount
	private static void testProgressFillMode()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject hpGo = new GameObject("HPBar");
		hpGo.AddComponent<RectTransform>();
		hpGo.transform.SetParent(parentGo.transform);
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(hpGo.transform);
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(hpGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			UGUIProgress hp = new UGUIProgress(script);
			hp.assignWindow(parent, "HPBar");
			hp.init();
			hp.setSliderMode(SLIDER_MODE.FILL);
			assertTrue(hp.getSliderMode() == SLIDER_MODE.FILL, "setSliderMode 生效");
			hp.setValue(0.6f);
			assertEqual(0.6f, barGo.GetComponent<Image>().fillAmount, 0.001f, "FILL 模式 fillAmount=0.6");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(hpGo);
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// showForeground: ProgressBar Image.enabled 切换
	private static void testProgressShowForeground()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject hpGo = new GameObject("HPBar");
		hpGo.AddComponent<RectTransform>();
		hpGo.transform.SetParent(parentGo.transform);
		GameObject barGo = new GameObject("ProgressBar");
		barGo.AddComponent<RectTransform>();
		barGo.AddComponent<Image>();
		barGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100.0f, 10.0f);
		barGo.transform.SetParent(hpGo.transform);
		GameObject thumbGo = new GameObject("Thumb");
		thumbGo.AddComponent<RectTransform>();
		thumbGo.transform.SetParent(hpGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			UGUIProgress hp = new UGUIProgress(script);
			hp.assignWindow(parent, "HPBar");
			hp.init();
			hp.showForeground(false);
			assertFalse(barGo.GetComponent<Image>().enabled, "showForeground(false) 隐藏前景");
			hp.showForeground(true);
			assertTrue(barGo.GetComponent<Image>().enabled, "showForeground(true) 显示前景");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(hpGo);
			UnityEngine.Object.DestroyImmediate(barGo);
			UnityEngine.Object.DestroyImmediate(thumbGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// UGUICheckbox: assignWindow 绑定 Mark/Label
	// ═════════════════════════════════════════════════════════════════
	private static void testCheckboxAssignWindow()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject cbGo = new GameObject("Checkbox");
		cbGo.AddComponent<RectTransform>();
		cbGo.transform.SetParent(parentGo.transform);
		GameObject markGo = new GameObject("Mark");
		markGo.AddComponent<RectTransform>();
		markGo.transform.SetParent(cbGo.transform);
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		GameObject labelGo = new GameObject("Label");
		labelGo.AddComponent<RectTransform>();
		labelGo.AddComponent<Text>();
		labelGo.transform.SetParent(cbGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			TestCheckbox cb = new TestCheckbox(script);
			cb.assignWindow(parent, "Checkbox");
			cb.init();
			assertFalse(cb.isChecked(), "初始未勾选(Mark 默认不激活)");
			assertNotNull(cb.getLabelObject(), "Label 绑定 myUGUITextAuto");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(cbGo);
			UnityEngine.Object.DestroyImmediate(markGo);
			UnityEngine.Object.DestroyImmediate(labelGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setChecked/isChecked 切换
	private static void testCheckboxSetCheckedToggle()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject cbGo = new GameObject("Checkbox");
		cbGo.AddComponent<RectTransform>();
		cbGo.transform.SetParent(parentGo.transform);
		GameObject markGo = new GameObject("Mark");
		markGo.AddComponent<RectTransform>();
		markGo.transform.SetParent(cbGo.transform);
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		GameObject labelGo = new GameObject("Label");
		labelGo.AddComponent<RectTransform>();
		labelGo.AddComponent<Text>();
		labelGo.transform.SetParent(cbGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			TestCheckbox cb = new TestCheckbox(script);
			cb.assignWindow(parent, "Checkbox");
			cb.init();
			cb.setChecked(true);
			assertTrue(cb.isChecked(), "setChecked(true) 后勾选");
			cb.setChecked(false);
			assertFalse(cb.isChecked(), "setChecked(false) 后取消");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(cbGo);
			UnityEngine.Object.DestroyImmediate(markGo);
			UnityEngine.Object.DestroyImmediate(labelGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onCheckClick: 点击切换 + 回调触发
	private static void testCheckboxClickToggle()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject cbGo = new GameObject("Checkbox");
		cbGo.AddComponent<RectTransform>();
		cbGo.transform.SetParent(parentGo.transform);
		GameObject markGo = new GameObject("Mark");
		markGo.AddComponent<RectTransform>();
		markGo.transform.SetParent(cbGo.transform);
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		GameObject labelGo = new GameObject("Label");
		labelGo.AddComponent<RectTransform>();
		labelGo.AddComponent<Text>();
		labelGo.transform.SetParent(cbGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			TestCheckbox cb = new TestCheckbox(script);
			cb.assignWindow(parent, "Checkbox");
			cb.init();
			int callbackCount = 0;
			cb.setCheckCallback(delegate (UGUICheckbox checkbox) { callbackCount++; });
			cb.clickForTest();
			assertTrue(cb.isChecked(), "点击后勾选");
			assertEqual(1, callbackCount, "勾选回调触发 1 次");
			cb.clickForTest();
			assertFalse(cb.isChecked(), "再次点击取消");
			assertEqual(2, callbackCount, "回调触发 2 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(cbGo);
			UnityEngine.Object.DestroyImmediate(markGo);
			UnityEngine.Object.DestroyImmediate(labelGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 不可交互: clickForTest 不切换
	private static void testCheckboxNonInteractable()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject cbGo = new GameObject("Checkbox");
		cbGo.AddComponent<RectTransform>();
		cbGo.transform.SetParent(parentGo.transform);
		GameObject markGo = new GameObject("Mark");
		markGo.AddComponent<RectTransform>();
		markGo.transform.SetParent(cbGo.transform);
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		GameObject labelGo = new GameObject("Label");
		labelGo.AddComponent<RectTransform>();
		labelGo.AddComponent<Text>();
		labelGo.transform.SetParent(cbGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			TestCheckbox cb = new TestCheckbox(script);
			cb.assignWindow(parent, "Checkbox");
			cb.init();
			cb.setInteractable(false);
			cb.clickForTest();
			assertFalse(cb.isChecked(), "不可交互时点击不切换");
			cb.setInteractable(true);
			cb.clickForTest();
			assertTrue(cb.isChecked(), "恢复交互后点击切换");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(cbGo);
			UnityEngine.Object.DestroyImmediate(markGo);
			UnityEngine.Object.DestroyImmediate(labelGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setLabel: 写入 Label 文本
	private static void testCheckboxSetLabel()
	{
		TestLayoutScriptDeep script = createScriptAndParent(out GameObject parentGo, out GameObject rootGo);
		GameObject cbGo = new GameObject("Checkbox");
		cbGo.AddComponent<RectTransform>();
		cbGo.transform.SetParent(parentGo.transform);
		GameObject markGo = new GameObject("Mark");
		markGo.AddComponent<RectTransform>();
		markGo.transform.SetParent(cbGo.transform);
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		markGo.SetActive(false);   // 模拟真实 prefab: Mark 默认隐藏, 勾选才显示
		GameObject labelGo = new GameObject("Label");
		labelGo.AddComponent<RectTransform>();
		labelGo.AddComponent<Text>();
		labelGo.transform.SetParent(cbGo.transform);
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			TestCheckbox cb = new TestCheckbox(script);
			cb.assignWindow(parent, "Checkbox");
			cb.init();
			cb.setLabel("OK");
			assertEqual("OK", cb.getLabelObject().getText(), "setLabel 写入 Label 文本");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(cbGo);
			UnityEngine.Object.DestroyImmediate(markGo);
			UnityEngine.Object.DestroyImmediate(labelGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// 暴露 protected onCheckClick 的 UGUICheckbox 测试子类
public class TestCheckbox : UGUICheckbox
{
	public TestCheckbox(IWindowObjectOwner parent) : base(parent) { }
	public void clickForTest() { onCheckClick(); }
}
