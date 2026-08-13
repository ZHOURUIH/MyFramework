using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIScrollRect 深度测试(ScrollRect 封装)
//   init(预加组件) / initScrollRect(视口+内容绑定+pivot 初始化+raycastTarget)
//   getContent/getViewport/getScrollRect / setScrollEnable(StopMovement+enabled)
//   setContentPivotVertical/Horizontal / get/setNormalizedPosition/X/Y
//   alignContentPivotVertical/Horizontal / alignContentTop/Bottom/Left/Right
//   autoAdjustContent(空 content 对齐分支, 全 4 路径) / setContentTopPos/getContentTopPos
//   update(makeSizeEven 偶数化 + content 位置取整)
public static class MyUGUIScrollRectTest
{
	public static void Run()
	{
		testInitWithComponent();
		testScrollEnable();
		testInitScrollRectBind();
		testContentPivot();
		testNormalizedPosition();
		testAlignContent();
		testAutoAdjustContent();
		testAutoAdjustContentEmpty();
		testContentTopPos();
		testUpdate();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 root(RectTransform+Image+ScrollRect) / viewport / content
	// 层级: root > viewport > content
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createScrollRect(out myUGUIScrollRect sr)
	{
		GameObject go = new GameObject("ScrollRectRoot");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		go.AddComponent<ScrollRect>();
		sr = new myUGUIScrollRect();
		sr.setObject(go);
		sr.init();
		return go;
	}

	private static myUGUIObject createChildUI(string name, Transform parent)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		go.transform.SetParent(parent);
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// init: 预加 ScrollRect → getScrollRect 返回同一组件
	private static void testInitWithComponent()
	{
		GameObject go = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			ScrollRect comp = go.GetComponent<ScrollRect>();
			assertNotNull(sr.getScrollRect(), "init 后 getScrollRect 非 null");
			assertTrue(ReferenceEquals(comp, sr.getScrollRect()), "getScrollRect 返回同一组件");
			assertTrue(sr.getScrollRect().enabled, "ScrollRect 默认 enabled");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setScrollEnable: StopMovement + enabled 切换
	private static void testScrollEnable()
	{
		GameObject go = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			sr.setScrollEnable(false);
			assertFalse(sr.getScrollRect().enabled, "setScrollEnable(false) 禁用组件");
			sr.setScrollEnable(true);
			assertTrue(sr.getScrollRect().enabled, "setScrollEnable(true) 启用组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// initScrollRect: content/viewport 绑定 + pivot 初始化 + Image raycastTarget 置 true
	private static void testInitScrollRectBind()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			// content/viewport 先给确定大小, 便于后续滚动
			content.setSize(new Vector2(300.0f, 300.0f));
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			ScrollRect comp = sr.getScrollRect();
			// 绑定
			assertTrue(ReferenceEquals(content.getRectTransform(), comp.content), "content 绑定到 ScrollRect.content");
			assertTrue(ReferenceEquals(viewport.getRectTransform(), comp.viewport), "viewport 绑定到 ScrollRect.viewport");
			assertTrue(ReferenceEquals(content, sr.getContent()), "getContent 返回同一 myUGUIObject");
			assertTrue(ReferenceEquals(viewport, sr.getViewport()), "getViewport 返回同一 myUGUIObject");
			// initScrollRect 内部调 setContentPivotVertical(1.0)/setContentPivotHorizontal(0.5)
			assertEqual(1.0f, content.getPivot().y, 0.001f, "verticalPivot=1.0 写入 content pivot.y");
			assertEqual(0.5f, content.getPivot().x, 0.001f, "horizontalPivot=0.5 写入 content pivot.x");
			// initScrollRect 内部调 alignContentPivotVertical/Horizontal
			assertEqual(1.0f, comp.verticalNormalizedPosition, 0.001f, "alignContentPivotVertical 生效");
			assertEqual(0.5f, comp.horizontalNormalizedPosition, 0.001f, "alignContentPivotHorizontal 生效");
			// Image 射线检测被置 true
			assertTrue(rootGo.GetComponent<Image>().raycastTarget, "root Image.raycastTarget 置 true");
			assertTrue(viewport.getGameObject().GetComponent<Image>().raycastTarget, "viewport Image.raycastTarget 置 true");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setContentPivotVertical/Horizontal: 只改 pivot, 不影响 normalizedPosition
	private static void testContentPivot()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			content.setSize(new Vector2(300.0f, 300.0f));
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			sr.setContentPivotVertical(0.2f);
			assertEqual(0.2f, content.getPivot().y, 0.001f, "setContentPivotVertical(0.2) 写入 pivot.y");
			sr.setContentPivotHorizontal(0.8f);
			assertEqual(0.8f, content.getPivot().x, 0.001f, "setContentPivotHorizontal(0.8) 写入 pivot.x");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// get/setNormalizedPosition + X/Y 单轴设置
	private static void testNormalizedPosition()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			content.setSize(new Vector2(300.0f, 300.0f));
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			// 双轴写入读回
			sr.setNormalizedPosition(new Vector2(0.25f, 0.75f));
			Vector2 readBack = sr.getNormalizedPosition();
			assertEqual(0.25f, readBack.x, 0.01f, "setNormalizedPosition x 读回");
			assertEqual(0.75f, readBack.y, 0.01f, "setNormalizedPosition y 读回");
			// 单轴写入
			sr.setNormalizedPositionX(0.4f);
			assertEqual(0.4f, sr.getScrollRect().horizontalNormalizedPosition, 0.01f, "setNormalizedPositionX(0.4)");
			sr.setNormalizedPositionY(0.6f);
			assertEqual(0.6f, sr.getScrollRect().verticalNormalizedPosition, 0.01f, "setNormalizedPositionY(0.6)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// alignContent 系列: 直接改 normalizedPosition(0/1/pivot)
	private static void testAlignContent()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			content.setSize(new Vector2(300.0f, 300.0f));
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			ScrollRect comp = sr.getScrollRect();
			// 竖直
			sr.alignContentTop();
			assertEqual(1.0f, comp.verticalNormalizedPosition, 0.001f, "alignContentTop → vnp=1");
			sr.alignContentBottom();
			assertEqual(0.0f, comp.verticalNormalizedPosition, 0.001f, "alignContentBottom → vnp=0");
			sr.setContentPivotVertical(0.3f);
			sr.alignContentPivotVertical();
			assertEqual(0.3f, comp.verticalNormalizedPosition, 0.001f, "alignContentPivotVertical → vnp=pivot.y");
			// 水平
			sr.alignContentRight();
			assertEqual(1.0f, comp.horizontalNormalizedPosition, 0.001f, "alignContentRight → hnp=1");
			sr.alignContentLeft();
			assertEqual(0.0f, comp.horizontalNormalizedPosition, 0.001f, "alignContentLeft → hnp=0");
			sr.setContentPivotHorizontal(0.7f);
			sr.alignContentPivotHorizontal();
			assertEqual(0.7f, comp.horizontalNormalizedPosition, 0.001f, "alignContentPivotHorizontal → hnp=pivot.x");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// autoAdjustContent:
	//   content 有子节点撑大(> viewport) → "content 大于 viewport"对齐分支
	//     垂直: alignContentY(1.0 - pivot.y) / 水平: alignContentRight() → hnp=1
	private static void testAutoAdjustContent()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 0.3f, 0.5f);   // pivot.y=0.3
			ScrollRect comp = sr.getScrollRect();
			// ══ 垂直 + content 有 300 高子节点(> viewport 100) → alignContentY(1-0.3) ══
			myUGUIObject itemV = createChildUI("ItemV", content.getGameObject().transform);
			itemV.setSize(new Vector2(50.0f, 300.0f));
			sr.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(0.7f, comp.verticalNormalizedPosition, 0.01f, "垂直 content>viewport → vnp=1-pivot.y");
			// ══ 垂直 FIXED(itemSize 150 大格子 → content 150 > 100) ══
			sr.autoAdjustContent(CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT, new Vector2(150.0f, 150.0f));
			assertEqual(0.7f, comp.verticalNormalizedPosition, 0.01f, "垂直 FIXED → vnp=1-pivot.y");
			// ══ 切水平 + content 有 300 宽子节点 → alignContentRight ══
			comp.vertical = false;
			comp.horizontal = true;
			myUGUIObject itemH = createChildUI("ItemH", content.getGameObject().transform);
			itemH.setSize(new Vector2(300.0f, 50.0f));
			sr.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(1.0f, comp.horizontalNormalizedPosition, 0.01f, "水平 content>viewport → hnp=1(alignContentRight)");
			// ══ 水平 FIXED → 依然 alignContentRight ══
			sr.autoAdjustContent(CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT, new Vector2(150.0f, 150.0f));
			assertEqual(1.0f, comp.horizontalNormalizedPosition, 0.01f, "水平 FIXED → hnp=1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// autoAdjustContent 空 content: 被 autoGrid* 改小(< viewport) → ScrollRect 不可滚动, normalizedPosition 归零(文档化)
	private static void testAutoAdjustContentEmpty()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			content.setSize(new Vector2(300.0f, 300.0f));
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			ScrollRect comp = sr.getScrollRect();
			// 空 content: autoGridVertical 把 content 高改 0 → 0 < 100 → ScrollRect 归零(文档化)
			sr.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(0.0f, comp.verticalNormalizedPosition, 0.001f, "空 content 垂直 → vnp 归零(不可滚动)");
			// 空 content: autoGrid 把 content 高改 0 → 同样归零
			sr.autoAdjustContent(CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT, new Vector2(10.0f, 10.0f));
			assertEqual(0.0f, comp.verticalNormalizedPosition, 0.001f, "空 content 垂直 FIXED → vnp 归零");
			// 水平空 content: autoGridHorizontal/autoGridFixedRootHeight 把 content 宽改小 → hnp 归零
			comp.vertical = false;
			comp.horizontal = true;
			sr.autoAdjustContent(CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT, new Vector2(10.0f, 10.0f));
			assertEqual(0.0f, comp.horizontalNormalizedPosition, 0.001f, "空 content 水平 → hnp 归零");
			sr.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(0.0f, comp.horizontalNormalizedPosition, 0.001f, "空 content 水平 SINGLE → hnp 归零");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setContentTopPos/getContentTopPos: 保持 content 顶部在指定 y
	private static void testContentTopPos()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			content.setSize(new Vector2(300.0f, 40.0f));
			viewport.setSize(new Vector2(100.0f, 100.0f));
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			sr.setContentTopPos(37.0f);
			assertEqual(37.0f, sr.getContentTopPos(), 0.001f, "setContentTopPos(37) 后 getContentTopPos 读回");
			sr.setContentTopPos(-15.5f);
			assertEqual(-15.5f, sr.getContentTopPos(), 0.001f, "setContentTopPos(-15.5) 后读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// update: makeSizeEven 把奇数尺寸补成偶数 + content 位置取整
	private static void testUpdate()
	{
		GameObject rootGo = createScrollRect(out myUGUIScrollRect sr);
		try
		{
			myUGUIObject viewport = createChildUI("Viewport", rootGo.transform);
			myUGUIObject content = createChildUI("Content", viewport.getGameObject().transform);
			content.setSize(new Vector2(300.0f, 300.0f));
			viewport.setSize(new Vector2(101.0f, 103.0f));   // 奇数
			sr.setSize(new Vector2(201.0f, 203.0f));         // 奇数
			sr.initScrollRect(viewport, content, 1.0f, 0.5f);
			content.setPosition(new Vector3(1.4f, 2.6f, 0.0f));   // 非整数, update 后会取整
			sr.update(0.01f);
			Vector2 srSize = sr.getSize();
			assertEqual(202.0f, srSize.x, 0.001f, "update 后自身宽补成偶数");
			assertEqual(204.0f, srSize.y, 0.001f, "update 后自身高补成偶数");
			Vector2 vpSize = viewport.getSize();
			assertEqual(102.0f, vpSize.x, 0.001f, "update 后 viewport 宽补成偶数");
			assertEqual(104.0f, vpSize.y, 0.001f, "update 后 viewport 高补成偶数");
			// content 位置被 round(velocity 为 0 时)
			Vector3 contentPos = content.getPosition();
			assertEqual(0.0f, contentPos.x - Mathf.Round(contentPos.x), 0.001f, "content.x 取整");
			assertEqual(0.0f, contentPos.y - Mathf.Round(contentPos.y), 0.001f, "content.y 取整");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}
