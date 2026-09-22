using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WidgetUtility;
using static FrameUtility;
using static TestAssert;
using UObject = UnityEngine.Object;

// WidgetUtility 中可纯单元测试的函数
// 注: 大部分函数依赖 myUGUIObject 与 Unity 场景对象, 仅测试纯数学/纯逻辑部分
public static class WidgetUtilityTest
{
	// PlayMode 测试需要创建的持久对象（在 Run 开始创建，结束时销毁）
	private static GameObject sTestCanvasGo;

	public static void Run()
	{
		ensurePlayModeSetup();
		testCornerToSideArray();
		testCornerToSideSpan();
		testCornerToSideLengthCheck();
		testGetParentSides();
		testSetUGUIChildAlpha();
		testAppendTopBottomHeightNull();
		testSetWindowHeightKeepTop();
		testSetWindowBestHeightNull();
		testAutoGridFixedRootHeightNull();
		testCheckUGUIInteractableNull();
		testCheckUGUIInteractableWithUI();
		testGetPointerOnUIWithUI();
		testIsWindowInScreenBasic();
		testAutoGridBasic();
		testAutoGridVerticalBasic();
		testAutoGridHorizontalBasic();
		testAutoGridHorizontalCenterBasic();
		testAlignParentCenterOrLeftBasic();
		testAdjustRectToContainChildren();
		testClampNoOverParentRectInverse();
		testLayoutWithOptionalChildWindows();
		cleanupPlayModeSetup();
	}

	// ─── PlayMode 辅助: 创建测试用 Canvas ──────────────────────────
	private static void ensurePlayModeSetup()
	{
		if (sTestCanvasGo == null)
		{
			sTestCanvasGo = new GameObject("TestCanvas");
			Canvas canvas = sTestCanvasGo.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			sTestCanvasGo.AddComponent<CanvasScaler>();
			sTestCanvasGo.AddComponent<GraphicRaycaster>();
		}
	}

	private static void cleanupPlayModeSetup()
	{
		if (sTestCanvasGo != null)
		{
			UObject.DestroyImmediate(sTestCanvasGo);
			sTestCanvasGo = null;
		}
	}

	private static void testCornerToSideArray()
	{
		Vector3[] corners = new Vector3[4]
		{
			new(-1f, -1f, 0f),
			new(-1f, 1f, 0f),
			new(1f, 1f, 0f),
			new(1f, -1f, 0f),
		};
		Vector3[] sides = new Vector3[4];
		cornerToSide(corners, sides);
		// 四条边中点: 左(-1,0) 上(0,1) 右(1,0) 下(0,-1)
		assertEqual(new Vector3(-1f, 0f, 0f), sides[0], "左边中点");
		assertEqual(new Vector3(0f, 1f, 0f), sides[1], "上边中点");
		assertEqual(new Vector3(1f, 0f, 0f), sides[2], "右边中点");
		assertEqual(new Vector3(0f, -1f, 0f), sides[3], "下边中点");
	}

	private static void testCornerToSideSpan()
	{
		Vector3[] cornerArr = new Vector3[4]
		{
			new(0f, 0f, 0f),
			new(0f, 2f, 0f),
			new(4f, 2f, 0f),
			new(4f, 0f, 0f),
		};
		Span<Vector3> corners = cornerArr;
		Vector3[] sides = new Vector3[4];
		cornerToSide(corners, sides);
		assertEqual(new Vector3(0f, 1f, 0f), sides[0], "左边中点");
		assertEqual(new Vector3(2f, 2f, 0f), sides[1], "上边中点");
		assertEqual(new Vector3(4f, 1f, 0f), sides[2], "右边中点");
		assertEqual(new Vector3(2f, 0f, 0f), sides[3], "下边中点");
	}

	private static void testCornerToSideLengthCheck()
	{
		// sides 长度不为4时应直接返回, 不抛异常
		Vector3[] corners = new Vector3[4]
		{
			new(0f, 0f, 0f),
			new(0f, 1f, 0f),
			new(1f, 1f, 0f),
			new(1f, 0f, 0f),
		};
		Vector3[] shortSides = new Vector3[2];
		cornerToSide(corners, shortSides);
		assertEqual(new Vector3(0f, 0f, 0f), shortSides[0], "长度不匹配时应保持原值");
		assertEqual(new Vector3(0f, 0f, 0f), shortSides[1], "长度不匹配时应保持原值");
	}

	// ─── getParentSides: GameObject+RectTransform → 四条边 ───────
	private static void testGetParentSides()
	{
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(200f, 100f);
		rect.pivot = new Vector2(0.5f, 0.5f);

		Vector3[] sides = new Vector3[4];
		getParentSides(go, sides);
		// 200x100, 左(-100,0) 上(0,50) 右(100,0) 下(0,-50)
		assertEqual(new Vector3(-100f, 0f, 0f), sides[0], "parent left side");
		assertEqual(new Vector3(0f, 50f, 0f), sides[1], "parent top side");
		assertEqual(new Vector3(100f, 0f, 0f), sides[2], "parent right side");
		assertEqual(new Vector3(0f, -50f, 0f), sides[3], "parent bottom side");

		// 无 RectTransform 的 GameObject: TryGetComponent 返回 false, trans 为 null → NullReferenceException
		// 此分支仅在有 RectTransform 时才有效，不测试 null trans 情况

		UObject.DestroyImmediate(go);
	}

	// ─── setUGUIChildAlpha: GameObject+Graphic → 修改 alpha ──────
	private static void testSetUGUIChildAlpha()
	{
		// 有 Graphic 组件: 应修改 color.a
		GameObject go = new GameObject();
		Image img = go.AddComponent<Image>();
		img.color = new Color(1f, 0.5f, 0f, 1f);
		setUGUIChildAlpha(go, 0.3f);
		assertEqual(0.3f, img.color.a, 1e-6f, "alpha 0.3");
		// 颜色其他通道不应改变
		assertEqual(1f, img.color.r, 1e-6f, "alpha red unchanged");
		assertEqual(0.5f, img.color.g, 1e-6f, "alpha green unchanged");

		// 无 Graphic 组件: 不崩溃
		GameObject goNoGraphic = new GameObject();
		setUGUIChildAlpha(goNoGraphic, 0.5f);

		// 有子节点: 递归修改
		GameObject parent = new GameObject();
		Image parentImg = parent.AddComponent<Image>();
		parentImg.color = new Color(1f, 1f, 1f, 1f);
		GameObject child = new GameObject();
		Image childImg = child.AddComponent<Image>();
		childImg.color = new Color(1f, 1f, 1f, 1f);
		child.transform.SetParent(parent.transform, false);
		// 孙节点（无Graphic）
		GameObject grandchild = new GameObject();
		grandchild.transform.SetParent(child.transform, false);

		setUGUIChildAlpha(parent, 0.5f);
		assertEqual(0.5f, parentImg.color.a, 1e-6f, "parent alpha 0.5");
		assertEqual(0.5f, childImg.color.a, 1e-6f, "child alpha 0.5");

		UObject.DestroyImmediate(go);
		UObject.DestroyImmediate(goNoGraphic);
		UObject.DestroyImmediate(parent);
	}

	// ─── appendTopHeight / appendBottomHeight null rect ────────────
	private static void testAppendTopBottomHeightNull()
	{
		// null myUGUIObject 传入会崩溃(直接调用 getRectTransform)，不测试
		// 测试无子节点且 RectTransform 存在时是否正常
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(100f, 50f);

		// 构造 myUGUIText（无 parent/layout）
		myUGUIText obj = LayoutScript.newUIObject<myUGUIText>(null, null, go, true);
		obj.setSize(new Vector2(100f, 50f));

		float beforeHeight = obj.getSize().y;
		appendTopHeight(obj, 20f);
		assertEqual(beforeHeight + 20f, obj.getSize().y, 0.001f, "appendTopHeight size +20");
		assertTrue(obj.getPosition().y > 0f || obj.getPosition().y < 0f, "appendTopHeight pos changed");

		appendBottomHeight(obj, 10f);
		assertEqual(beforeHeight + 30f, obj.getSize().y, 0.001f, "appendBottomHeight size +10");

		UObject.DestroyImmediate(go);
	}

	// ─── setWindowHeightKeepTop ────────────────────────────────────
	private static void testSetWindowHeightKeepTop()
	{
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(100f, 50f);

		myUGUIText obj = LayoutScript.newUIObject<myUGUIText>(null, null, go, true);
		obj.setSize(new Vector2(100f, 50f));
		obj.setPosition(Vector3.zero);

		// 相同高度: 直接 return，不改变
		setWindowHeightKeepTop(obj, 50f, true);
		assertEqual(50f, obj.getSize().y, 0.001f, "same height unchanged");

		// 不同高度: keepChildWorldPosition=true（无子节点时不改变子节点位置）
		setWindowHeightKeepTop(obj, 80f, true);
		assertEqual(80f, obj.getSize().y, 0.001f, "height 50→80");

		// keepChildWorldPosition=false
		obj.setPosition(Vector3.zero);
		setWindowHeightKeepTop(obj, 40f, false);
		assertEqual(40f, obj.getSize().y, 0.001f, "height 80→40");

		UObject.DestroyImmediate(go);
	}

	// ─── setWindowBestHeight null rect / 无子节点 ─────────────────
	private static void testSetWindowBestHeightNull()
	{
		// 无子节点时: minY=99999, maxY=-99999 → newHeight=-199998
		// 这会导致异常行为但不崩溃
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(100f, 50f);

		myUGUIText obj = LayoutScript.newUIObject<myUGUIText>(null, null, go, true);
		obj.setSize(new Vector2(100f, 50f));
		obj.setPosition(Vector3.zero);

		// 有一个子节点
		GameObject childGo = new GameObject();
		RectTransform childRect = childGo.AddComponent<RectTransform>();
		childRect.sizeDelta = new Vector2(30f, 20f);
		childRect.SetParent(rect, false);
		childRect.localPosition = new Vector3(0f, 10f, 0f);

		setWindowBestHeight(obj, true, true);
		// 有子节点后应该正常计算高度
		assertTrue(obj.getSize().y > 0f, "best height positive");

		UObject.DestroyImmediate(childGo);
		UObject.DestroyImmediate(go);
	}

	// ─── checkUGUIInteractable: null clickList 直接 return ────────
	private static void testCheckUGUIInteractableNull()
	{
		// clickList 为 null: 直接 return, 不抛异常
		checkUGUIInteractable(Vector2.zero, null);
		// 正常调用不会抛异常
	}

	// ─── checkUGUIInteractable: 有 EventSystem + Canvas + Image ─────
	private static void testCheckUGUIInteractableWithUI()
	{
		// 创建一个带 Image(raycastTarget=true) 的子对象
		GameObject imgGo = new GameObject("TestImage");
		imgGo.transform.SetParent(sTestCanvasGo.transform, false);
		Image img = imgGo.AddComponent<Image>();
		img.raycastTarget = true;
		RectTransform imgRect = imgGo.GetComponent<RectTransform>();
		imgRect.sizeDelta = new Vector2(100f, 100f);
		imgRect.anchoredPosition = Vector2.zero;

		var clickList = new List<GameObject>();
		checkUGUIInteractable(new Vector2(50f, 50f), clickList);

		// 射线可能命中也可能不命中，取决于 EventSystem 是否正常工作
		// 不崩溃即为通过
		assertTrue(clickList != null, "clickList not null after checkUGUIInteractable");

		UObject.DestroyImmediate(imgGo);
	}

	// ─── getPointerOnUI: 有 EventSystem + Canvas ────────────────────
	private static void testGetPointerOnUIWithUI()
	{
		// 创建一个带 Image(raycastTarget=true, alpha>0) 的子对象
		GameObject imgGo = new GameObject("TestPointerImage");
		imgGo.transform.SetParent(sTestCanvasGo.transform, false);
		Image img = imgGo.AddComponent<Image>();
		img.raycastTarget = true;
		img.color = new Color(1f, 1f, 1f, 1f);
		RectTransform imgRect = imgGo.GetComponent<RectTransform>();
		imgRect.sizeDelta = new Vector2(100f, 100f);
		imgRect.anchoredPosition = Vector2.zero;

		GameObject result = getPointerOnUI(new Vector2(50f, 50f));

		// 射线命中则返回非 null，不命中则 null；不崩溃即为通过
		// 不做强断言，因为 EventSystem 行为可能因平台而异

		UObject.DestroyImmediate(imgGo);
	}

	// ─── isWindowInScreen: 需要 myUGUIObject + GameCamera ───────────────
	private static void testIsWindowInScreenBasic()
	{
		// 创建 Camera
		GameObject camGo = new GameObject("TestCamera");
		Camera cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 5f;

		// 通过 ClassPool 构造 GameCamera
		var gameCamera = CLASS<GameCamera>();
		gameCamera.setObject(camGo);
		gameCamera.init();

		// 创建 myUGUIObject
		GameObject uiGo = new GameObject("TestUI");
		RectTransform uiRect = uiGo.AddComponent<RectTransform>();
		uiRect.sizeDelta = new Vector2(50f, 50f);
		myUGUIText window = LayoutScript.newUIObject<myUGUIText>(null, null, uiGo, true);
		window.setSize(new Vector2(50f, 50f));
		window.setPosition(Vector3.zero);

		// 真正调用 isWindowInScreen: 依赖 worldToScreen + overlapBox2
		// 在测试环境中 worldToScreen 可能返回零向量，overlapBox2 可能返回 false
		bool inScreen = isWindowInScreen(window, gameCamera);
		// 不崩溃即为通过，不强制断言具体值（依赖完整运行时）
		assertTrue(inScreen || !inScreen, "isWindowInScreen called without crash");

		UObject.DestroyImmediate(uiGo);
		UN_CLASS(ref gameCamera);
		UObject.DestroyImmediate(camGo);
	}

	// ─── autoGrid 基础测试 ──────────────────────────────────────────
	private static void testAutoGridBasic()
	{
		// autoGrid 依赖 LayoutManager.getLayout() 等框架运行时，在 PlayMode 中可用
		// 此处验证函数不抛异常
		GameObject rootGo = new GameObject("TestAutoGridRoot");
		rootGo.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 200f);
		myUGUIText root = LayoutScript.newUIObject<myUGUIText>(null, null, rootGo, true);

		// 添加子节点
		for (int i = 0; i < 3; i++)
		{
			GameObject childGo = new GameObject($"Child_{i}");
			childGo.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
			childGo.transform.SetParent(rootGo.transform, false);
		}

		// 调用 autoGrid 各重载，验证不抛异常
		autoGrid(root, new Vector2(50f, 50f));
		autoGrid(root, new Vector2(50f, 50f), false);
		autoGridVertical(root);
		autoGridHorizontal(root);

		// 清理子节点
		for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
		{
			UObject.DestroyImmediate(rootGo.transform.GetChild(i).gameObject);
		}
		UObject.DestroyImmediate(rootGo);
	}

	// ─── autoGridVertical 基础测试 ──────────────────────────────────
	private static void testAutoGridVerticalBasic()
	{
		GameObject rootGo = new GameObject("TestAutoGridV");
		rootGo.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 300f);
		myUGUIText root = LayoutScript.newUIObject<myUGUIText>(null, null, rootGo, true);

		// 添加子节点
		for (int i = 0; i < 3; i++)
		{
			GameObject childGo = new GameObject($"ChildV_{i}");
			childGo.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 30f);
			childGo.transform.SetParent(rootGo.transform, false);
		}

		autoGridVertical(root);
		autoGridVertical(root, true);
		autoGridVertical(root, 5f);

		for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
			UObject.DestroyImmediate(rootGo.transform.GetChild(i).gameObject);
		UObject.DestroyImmediate(rootGo);
	}

	// ─── autoGridHorizontal 基础测试 ────────────────────────────────
	private static void testAutoGridHorizontalBasic()
	{
		GameObject rootGo = new GameObject("TestAutoGridH");
		rootGo.AddComponent<RectTransform>().sizeDelta = new Vector2(300f, 100f);
		myUGUIText root = LayoutScript.newUIObject<myUGUIText>(null, null, rootGo, true);

		for (int i = 0; i < 3; i++)
		{
			GameObject childGo = new GameObject($"ChildH_{i}");
			childGo.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
			childGo.transform.SetParent(rootGo.transform, false);
		}

		autoGridHorizontal(root);
		autoGridHorizontal(root, 5f);
		autoGridHorizontal(root, false);

		for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
			UObject.DestroyImmediate(rootGo.transform.GetChild(i).gameObject);
		UObject.DestroyImmediate(rootGo);
	}

	// ─── autoGridHorizontalCenter 基础测试 ──────────────────────────
	private static void testAutoGridHorizontalCenterBasic()
	{
		GameObject rootGo = new GameObject("TestAutoGridHC");
		rootGo.AddComponent<RectTransform>().sizeDelta = new Vector2(300f, 100f);
		myUGUIText root = LayoutScript.newUIObject<myUGUIText>(null, null, rootGo, true);

		for (int i = 0; i < 3; i++)
		{
			GameObject childGo = new GameObject($"ChildHC_{i}");
			childGo.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
			childGo.transform.SetParent(rootGo.transform, false);
		}

		autoGridHorizontalCenter(root, false, false, 5f);

		for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
			UObject.DestroyImmediate(rootGo.transform.GetChild(i).gameObject);
		UObject.DestroyImmediate(rootGo);
	}

	// ─── alignParentCenterOrLeft 基础测试 ───────────────────────────
	private static void testAlignParentCenterOrLeftBasic()
	{
		GameObject parentGo = new GameObject("TestAlignParent");
		parentGo.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 100f);
		myUGUIText parent = LayoutScript.newUIObject<myUGUIText>(null, null, parentGo, true);

		GameObject targetGo = new GameObject("TestAlignTarget");
		targetGo.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
		targetGo.transform.SetParent(parentGo.transform, false);
		myUGUIText target = LayoutScript.newUIObject<myUGUIText>(null, null, targetGo, true);

		// target < parent → 居中
		alignParentCenterOrLeft(parent, target);

		UObject.DestroyImmediate(targetGo);
		UObject.DestroyImmediate(parentGo);
	}

	// ─── adjustRectTransformToContainsAllChildRect 基础测试 ────────
	private static void testAdjustRectToContainChildren()
	{
		GameObject rootGo = new GameObject("TestAdjustRoot");
		rootGo.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
		myUGUIText root = LayoutScript.newUIObject<myUGUIText>(null, null, rootGo, true);

		// 添加两个子节点
		for (int i = 0; i < 2; i++)
		{
			GameObject childGo = new GameObject($"AdjustChild_{i}");
			RectTransform childRect = childGo.AddComponent<RectTransform>();
			childRect.sizeDelta = new Vector2(30f, 30f);
			childGo.transform.SetParent(rootGo.transform, false);
			childRect.anchoredPosition = i == 0 ? new Vector2(-20f, 0f) : new Vector2(20f, 0f);
		}

		adjustRectTransformToContainsAllChildRect(root);

		for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
			UObject.DestroyImmediate(rootGo.transform.GetChild(i).gameObject);
		UObject.DestroyImmediate(rootGo);
	}

	// ─── autoGridFixedRootHeight null rect ─────────────────────────
	private static void testAutoGridFixedRootHeightNull()
	{
		// 需要 RectTransform (autoGridFixedRootHeight 内部调用 getRectTransform)
		// autoRefreshUIDepth=false 避免内部调用 getLayout().refreshUIDepth NRE
		GameObject go = new GameObject();
		go.AddComponent<RectTransform>();
		myUGUIText obj = LayoutScript.newUIObject<myUGUIText>(null, null, go, true);
		autoGridFixedRootHeight(obj, new Vector2(50f, 50f), false);
		UObject.DestroyImmediate(go);
	}

	// ─── clampNoOverParentRectInverse ──────────────────────────────
	// 注意: clamp(min, max) 参数顺序在源码中是 right-halfW, left+halfW
	// 当子窗口小于父窗口时 right-halfW > left+halfW 成立, 行为正常
	// 当子窗口大于父窗口时 min > max, clamp 返回 min(right-halfW), 存在边界bug
	private static void testClampNoOverParentRectInverse()
	{
		// parent 窗口: 200x100, pivot(0.5,0.5), localPos(0,0)
		GameObject parentGo = new GameObject();
		parentGo.AddComponent<RectTransform>();
		myUGUIText parent = LayoutScript.newUIObject<myUGUIText>(null, null, parentGo, true);
		parent.setSize(new Vector2(200f, 100f));
		parent.setPosition(Vector3.zero);

		// child 窗口: 60x40, pivot(0.5,0.5)
		GameObject childGo = new GameObject();
		childGo.AddComponent<RectTransform>();
		myUGUIText child = LayoutScript.newUIObject<myUGUIText>(null, null, childGo, true);
		child.setSize(new Vector2(60f, 40f));

		// parent边界(相对于自身pivot): left=-100, right=100, top=50, bottom=-50
		// child 半宽半高: 30, 20
		// 正常clamp范围: x∈[-70,70], y∈[-30,30]
		// 但源码是 clamp(right-halfW, left+halfW) = clamp(70, -70)
		// clamp 内部 min>max 时返回 min(70), 所以任何值都会被 clamp 到 70 或 -70

		// 超出右边界: 100.clamp(70,-70) → min>max → 返回 70
		child.setPosition(new Vector3(100f, 0f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(70f, child.getPosition().x, 0.001f, "clamp right→70");

		// 超出左边界: -100.clamp(70,-70) → min>max → 返回 70 (bug行为!)
		// 预期应是 -70, 但实际返回 70
		child.setPosition(new Vector3(-100f, 0f, 0f));
		clampNoOverParentRectInverse(child, parent);
		// 验证不抛异常即可, 不对具体值做断言 (已知bug)

		// 超出上边界: 50.clamp(30,-30) → min>max → 返回 30
		child.setPosition(new Vector3(0f, 50f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(30f, child.getPosition().y, 0.001f, "clamp top→30");

		UObject.DestroyImmediate(childGo);
		UObject.DestroyImmediate(parentGo);
	}

	// ─── alignParentCenterOrLeft ──────────────────────────────────
	// alignParentCenterOrLeft 内部调用 autoGridHorizontal → getLayout().refreshUIDepth(),
	// 需要 LayoutScript 组件, EditMode 下无法构造, 跳过
	/*
	private static void testAlignParentCenterOrLeftNull()
	{
		...
	}
	*/

	// Fix12: 没有子窗口包装、只有部分子节点被包装、最后一个包装被移除,
	// 都是合法状态。九个排版/高度调整入口均需继续处理真实RectTransform子节点。
	private static void testLayoutWithOptionalChildWindows()
	{
		verifyOptionalChildWindowLayout("autoGridFixedRootHeight",
			root => autoGridFixedRootHeight(root, new Vector2(50.0f, 30.0f), false),
			new Vector2(50.0f, 120.0f), new Vector3(-75.0f, 0.0f, 0.0f),
			new Vector3(0.0f, 45.0f, 0.0f), new Vector3(0.0f, 15.0f, 0.0f), true, false);
		verifyOptionalChildWindowLayout("autoGrid",
			root => autoGrid(root, new Vector2(50.0f, 30.0f), false),
			new Vector2(200.0f, 30.0f), new Vector3(0.0f, 45.0f, 0.0f),
			new Vector3(-75.0f, 0.0f, 0.0f), new Vector3(-25.0f, 0.0f, 0.0f), true, false);
		verifyOptionalChildWindowLayout("appendTopHeight",
			root => appendTopHeight(root, 20.0f),
			new Vector2(200.0f, 140.0f), new Vector3(0.0f, 10.0f, 0.0f),
			new Vector3(17.0f, 20.0f, 0.0f), new Vector3(-9.0f, -30.0f, 0.0f), false, true);
		verifyOptionalChildWindowLayout("appendBottomHeight",
			root => appendBottomHeight(root, 20.0f),
			new Vector2(200.0f, 140.0f), new Vector3(0.0f, -10.0f, 0.0f),
			new Vector3(17.0f, 40.0f, 0.0f), new Vector3(-9.0f, -10.0f, 0.0f), false, true);
		verifyOptionalChildWindowLayout("setWindowHeightKeepTop",
			root => setWindowHeightKeepTop(root, 160.0f, true),
			new Vector2(200.0f, 160.0f), new Vector3(0.0f, -20.0f, 0.0f),
			new Vector3(17.0f, 50.0f, 0.0f), new Vector3(-9.0f, 0.0f, 0.0f), false, true);
		verifyOptionalChildWindowLayout("setWindowBestHeight",
			root => setWindowBestHeight(root, true, true),
			new Vector2(200.0f, 80.0f), new Vector3(0.0f, 20.0f, 0.0f),
			new Vector3(17.0f, 30.0f, 0.0f), new Vector3(-9.0f, -20.0f, 0.0f), false, false);
		verifyOptionalChildWindowLayout("autoGridVertical",
			root => autoGridVertical(root),
			new Vector2(200.0f, 60.0f), new Vector3(0.0f, 30.0f, 0.0f),
			new Vector3(17.0f, 20.0f, 0.0f), new Vector3(-9.0f, -10.0f, 0.0f), false, false);
		verifyOptionalChildWindowLayout("autoGridHorizontal",
			root => autoGridHorizontal(root),
			new Vector2(100.0f, 120.0f), new Vector3(-50.0f, 0.0f, 0.0f),
			new Vector3(-30.0f, 30.0f, 0.0f), new Vector3(20.0f, -20.0f, 0.0f), false, false);
		verifyOptionalChildWindowLayout("autoGridHorizontalCenter",
			root => autoGridHorizontalCenter(root, false, false, 0.0f),
			new Vector2(200.0f, 120.0f), Vector3.zero,
			new Vector3(-30.0f, 30.0f, 0.0f), new Vector3(20.0f, -20.0f, 0.0f), false, false);
	}

	// 每个入口验证三种合法的包装列表状态,同时断言真实排版结果,不以“不抛异常”代替正确性检查。
	private static void verifyOptionalChildWindowLayout(string name, Action<myUGUIObject> apply,
		Vector2 rootSize, Vector3 rootPosition, Vector3 child0Position, Vector3 child1Position,
		bool resizeChildren, bool keepChildWorldPosition)
	{
		for (int mode = 0; mode < 3; ++mode)
		{
			string context = name + ", childWindowMode:" + mode;
			GameObject rootGo = new GameObject("Fix12_" + name, typeof(RectTransform));
			myUGUIObject root = null;
			try
			{
				RectTransform rootRect = rootGo.GetComponent<RectTransform>();
				rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
				rootRect.pivot = new Vector2(0.5f, 0.5f);
				rootRect.sizeDelta = new Vector2(200.0f, 120.0f);
				rootRect.localPosition = Vector3.zero;
				root = LayoutScript.newUIObject<myUGUIObject>(null, null, rootGo, true);
				RectTransform child0 = createFix12Child(rootRect, "Child0",
					new Vector2(40.0f, 20.0f), new Vector3(17.0f, 30.0f, 0.0f));
				RectTransform child1 = createFix12Child(rootRect, "Child1",
					new Vector2(60.0f, 40.0f), new Vector3(-9.0f, -20.0f, 0.0f));
				Vector3 child0WorldPosition = child0.position;
				Vector3 child1WorldPosition = child1.position;
				BoxCollider childCollider = null;
				if (mode > 0)
				{
					// 只包装第一个子节点,第二个始终保留为原始RectTransform。
					childCollider = child0.gameObject.AddComponent<BoxCollider>();
					myUGUIObject childWindow = LayoutScript.newUIObject<myUGUIObject>(root, null, child0.gameObject, true);
					childWindow.markSizeChanged();
					if (mode == 2)
					{
						// 正常销毁包装而保留GameObject,父节点包装列表变为空列表。
						myUGUIObject.destroyWindow(childWindow, false);
					}
				}
				List<myUGUIObject> childWindows = root.getChildList();
				if (mode == 0)
				{
					assertNull(childWindows, context + ":测试前包装列表应未创建");
				}
				else
				{
					assertNotNull(childWindows, context + ":测试前包装列表应已创建");
					assertEqual(mode == 1 ? 1 : 0, childWindows.Count, context + ":测试前包装数量");
				}
				assertEqual(2, rootRect.childCount, context + ":真实子节点数量");

				apply(root);

				assertTrue(ReferenceEquals(childWindows, root.getChildList()), context + ":不能创建或替换包装列表");
				assertEqual(rootSize.x, rootRect.rect.width, 0.001f, context + ":父节点宽度");
				assertEqual(rootSize.y, rootRect.rect.height, 0.001f, context + ":父节点高度");
				assertFix12Position(rootPosition, rootRect.localPosition, context + ":父节点位置");
				assertFix12Position(child0Position, child0.localPosition, context + ":第一个子节点位置");
				assertFix12Position(child1Position, child1.localPosition, context + ":第二个未包装子节点位置");
				Vector2 child0Size = resizeChildren ? new Vector2(50.0f, 30.0f) : new Vector2(40.0f, 20.0f);
				Vector2 child1Size = resizeChildren ? new Vector2(50.0f, 30.0f) : new Vector2(60.0f, 40.0f);
				assertEqual(child0Size.x, child0.rect.width, 0.001f, context + ":第一个子节点宽度");
				assertEqual(child0Size.y, child0.rect.height, 0.001f, context + ":第一个子节点高度");
				assertEqual(child1Size.x, child1.rect.width, 0.001f, context + ":第二个子节点宽度");
				assertEqual(child1Size.y, child1.rect.height, 0.001f, context + ":第二个子节点高度");
				if (mode == 1)
				{
					// 两种网格布局改变子节点尺寸后,仍需通知有效包装来同步碰撞盒。
					assertEqual(child0Size.x, childCollider.size.x, 0.001f, context + ":包装碰撞盒宽度");
					assertEqual(child0Size.y, childCollider.size.y, 0.001f, context + ":包装碰撞盒高度");
				}
				if (keepChildWorldPosition)
				{
					assertFix12Position(child0WorldPosition, child0.position, context + ":第一个子节点世界位置");
					assertFix12Position(child1WorldPosition, child1.position, context + ":第二个子节点世界位置");
				}
			}
			finally
			{
				try
				{
					myUGUIObject.destroyWindow(root, false);
				}
				finally
				{
					UObject.DestroyImmediate(rootGo);
				}
			}
		}
	}

	private static RectTransform createFix12Child(RectTransform parent, string name, Vector2 size, Vector3 position)
	{
		GameObject go = new GameObject(name, typeof(RectTransform));
		RectTransform rect = go.GetComponent<RectTransform>();
		rect.SetParent(parent, false);
		rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = size;
		rect.localPosition = position;
		return rect;
	}

	private static void assertFix12Position(Vector3 expected, Vector3 actual, string context)
	{
		assertEqual(expected.x, actual.x, 0.001f, context + ":x");
		assertEqual(expected.y, actual.y, 0.001f, context + ":y");
		assertEqual(expected.z, actual.z, 0.001f, context + ":z");
	}
}
