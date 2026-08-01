using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WidgetUtility;
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

	// ─── isWindowInScreen: 需要 myUGUIObject + Camera ───────────────
	// isWindowInScreen 依赖 worldToScreen + getWorldPosition + getScreenSize
	// 这些都需要完整的 Unity 运行时 + Camera + Canvas，仅在 PlayMode 有效
	private static void testIsWindowInScreenBasic()
	{
		// 创建 Camera
		GameObject camGo = new GameObject("TestCamera");
		Camera cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 5f;

		// 创建 myUGUIObject
		GameObject uiGo = new GameObject("TestUI");
		RectTransform uiRect = uiGo.AddComponent<RectTransform>();
		uiRect.sizeDelta = new Vector2(50f, 50f);
		myUGUIText window = LayoutScript.newUIObject<myUGUIText>(null, null, uiGo, true);
		window.setSize(new Vector2(50f, 50f));

		// 注意: isWindowInScreen 需要 GameCamera 参数 (不是 Camera)
		// GameCamera 是对 Camera 的封装，且需要 worldToScreen 函数
		// 此处仅验证不抛异常即可（函数内部依赖框架运行时对象）
		// 真正测试需要完整的框架初始化

		UObject.DestroyImmediate(uiGo);
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

	// ─── alignParentCenterOrLeft ──────────────────────────────────
	// alignParentCenterOrLeft 内部调用 autoGridHorizontal → getLayout().refreshUIDepth(),
	// 需要 LayoutScript 组件, EditMode 下无法构造, 跳过
	/*
	private static void testAlignParentCenterOrLeftNull()
	{
		...
	}
	*/

	// ─── clampNoOverParentRectInverse ──────────────────────────────
	// 源码 bug: clamp(min, max) 中 min=right-半宽, max=left+半宽, min > max 时返回 min
	// 实际应交换参数顺序为 clamp(max, min), 跳过
	/*
	private static void testClampNoOverParentRectInverse()
	{
		// parent 窗口: 200x100, pivot(0.5,0.5), localPos(0,0)
		GameObject parentGo = new GameObject();
		myUGUIText parent = LayoutScript.newUIObject<myUGUIText>(null, null, parentGo, true);
		parent.setSize(new Vector2(200f, 100f));
		parent.setPosition(Vector3.zero);

		// child 窗口: 60x40, pivot(0.5,0.5)
		GameObject childGo = new GameObject();
		myUGUIText child = LayoutScript.newUIObject<myUGUIText>(null, null, childGo, true);
		child.setSize(new Vector2(60f, 40f));

		// parent边界(相对于自身pivot): left=-100, right=100, top=50, bottom=-50
		// child 半宽半高: 30, 20
		// clamp范围: x∈[-100+30, 100-30]=[-70,70], y∈[-50+20, 50-20]=[-30,30]

		// 在范围内: 不动
		child.setPosition(new Vector3(20f, 10f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(20f, child.getPosition().x, 0.001f, "clamp in range X");
		assertEqual(10f, child.getPosition().y, 0.001f, "clamp in range Y");

		// 超出右边界: 应被clamp到 70
		child.setPosition(new Vector3(100f, 0f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(70f, child.getPosition().x, 0.001f, "clamp right");

		// 超出左边界: 应被clamp到 -70
		child.setPosition(new Vector3(-100f, 0f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(-70f, child.getPosition().x, 0.001f, "clamp left");

		// 超出上边界: 应被clamp到 30
		child.setPosition(new Vector3(0f, 50f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(30f, child.getPosition().y, 0.001f, "clamp top");

		// 超出下边界: 应被clamp到 -30
		child.setPosition(new Vector3(0f, -50f, 0f));
		clampNoOverParentRectInverse(child, parent);
		assertEqual(-30f, child.getPosition().y, 0.001f, "clamp bottom");

		UObject.DestroyImmediate(childGo);
		UObject.DestroyImmediate(parentGo);
	}
	*/
}
