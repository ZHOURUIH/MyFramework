using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIImageSimple 深度测试(Image 组件封装, 全部纯逻辑方法)
//   init(预加 Image) / setFillPercent(不夹取, 文档化) / getFillPercent
//   setColor/getColor / setAlpha/getAlpha / setSpriteOnly/getSprite / getSpriteSize
//   getSpriteName(null 安全) / setUGUIRaycastTarget / setRenderQueue(material null 安全)
//   getImage / 兄弟排序 setAsFirstSibling/setAsLastSibling(需父节点, 相同位置 return)
public static class MyUGUIImageSimpleDeepTest
{
	public static void Run()
	{
		testInitWithImage();
		testFillPercentNoClamp();
		testColorAlpha();
		testSpriteNullSafety();
		testSetSpriteOnly();
		testRaycastTarget();
		testRenderQueue();
		testSiblingOrder();
		testSiblingOrderSamePositionNoop();
		testCullChain();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myUGUIImageSimple(预加 Image)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIImageSimple createImage(out GameObject go)
	{
		go = new GameObject("TestImage");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		myUGUIImageSimple img = new myUGUIImageSimple();
		img.setObject(go);
		img.init();
		return img;
	}

	// init: 预加 Image → mImage 有效
	private static void testInitWithImage()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			assertNotNull(img.getImage(), "init 后 getImage 非 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setFillPercent: 写 Image.fillAmount, 超界被 Unity 内部 clamp01(文档化真实行为)
	private static void testFillPercentNoClamp()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			img.setFillPercent(0.5f);
			assertEqual(0.5f, img.getFillPercent(), 0.001f, "setFillPercent(0.5) 读回");
			// Unity 的 Image.fillAmount setter 内部 Mathf.Clamp01 → 超界被夹取(文档化)
			img.setFillPercent(1.5f);
			assertEqual(1.0f, img.getFillPercent(), 0.001f, "超 1 被 Unity clamp 到 1(文档化)");
			img.setFillPercent(-0.5f);
			assertEqual(0.0f, img.getFillPercent(), 0.001f, "负值被 Unity clamp 到 0(文档化)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setColor/getColor, setAlpha/getAlpha 读写
	private static void testColorAlpha()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			Color red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			img.setColor(red);
			assertTrue(red == img.getColor(), "setColor 写入 mImage.color");
			img.setAlpha(0.3f);
			assertEqual(0.3f, img.getAlpha(), 0.001f, "setAlpha 写入 mImage.color.a");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 无 sprite 时: getSpriteName 为 null / getSpriteSize 返回窗口自身大小(文档化)
	private static void testSpriteNullSafety()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			assertNull(img.getSprite(), "初始 sprite 为 null");
			// init 时 sprite==null → mSpriteName=null, 返回 null 而非空串(文档化)
			assertNull(img.getSpriteName(), "无 sprite 时 getSpriteName 为 null");
			// sprite==null 时 getSpriteSize 返回窗口自身大小(非 zero, 文档化)
			assertTrue(img.getSpriteSize() == img.getSize(), "无 sprite 时 getSpriteSize 返回窗口大小");
			img.setSpriteOnly(null);   // mImage.sprite==null → 直接 return, 无副作用
			assertNull(img.getSprite(), "setSpriteOnly(null) 无副作用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteOnly: 设置 sprite → getSprite 同一引用
	private static void testSetSpriteOnly()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			Texture2D tex = new Texture2D(2, 2);
			Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, 2.0f, 2.0f), new Vector2(0.5f, 0.5f));
			try
			{
				img.setSpriteOnly(sprite);
				assertTrue(ReferenceEquals(sprite, img.getSprite()), "setSpriteOnly 设置同一 sprite");
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(sprite);
				UnityEngine.Object.DestroyImmediate(tex);
			}
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIRaycastTarget: Image.raycastTarget 切换
	private static void testRaycastTarget()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			img.setUGUIRaycastTarget(false);
			assertFalse(go.GetComponent<Image>().raycastTarget, "setUGUIRaycastTarget(false)");
			img.setUGUIRaycastTarget(true);
			assertTrue(go.GetComponent<Image>().raycastTarget, "setUGUIRaycastTarget(true)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setRenderQueue/getRenderQueue: 写读往返
	// EditMode 下 Image.material 默认非 null(默认材质), 不测 null 分支
	private static void testRenderQueue()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			int originQueue = img.getRenderQueue();
			img.setRenderQueue(5);
			assertEqual(5, img.getRenderQueue(), "setRenderQueue(5) 写读一致");
			img.setRenderQueue(originQueue);   // 恢复, 避免污染共享默认材质
			assertEqual(originQueue, img.getRenderQueue(), "恢复原 renderQueue");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 兄弟排序: setAsFirstSibling/setAsLastSibling(需父节点)
	// ═════════════════════════════════════════════════════════════════
	private static void testSiblingOrder()
	{
		GameObject parentGo = new GameObject("Parent");
		parentGo.AddComponent<RectTransform>();
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		myUGUIObject a = createChild("ChildA", parentGo);
		myUGUIObject b = createChild("ChildB", parentGo);
		myUGUIObject c = createChild("ChildC", parentGo);
		try
		{
			// 初始顺序 a(0) b(1) c(2)
			assertEqual("ChildA", parentGo.transform.GetChild(0).name, "初始第一个是 ChildA");
			// a 移到末尾 → b c a
			a.setAsLastSibling(false);
			assertEqual(2, a.getGameObject().transform.GetSiblingIndex(), "A 移到末尾 index=2");
			assertEqual(0, b.getGameObject().transform.GetSiblingIndex(), "B 变 index=0");
			// c 移到开头 → c b a
			c.setAsFirstSibling(false);
			assertEqual(0, c.getGameObject().transform.GetSiblingIndex(), "C 移到开头 index=0");
			assertEqual(1, b.getGameObject().transform.GetSiblingIndex(), "B 变 index=1");
			assertEqual(2, a.getGameObject().transform.GetSiblingIndex(), "A 保持末尾 index=2");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 相同位置重复调用: 直接 return 无副作用
	private static void testSiblingOrderSamePositionNoop()
	{
		GameObject parentGo = new GameObject("Parent");
		parentGo.AddComponent<RectTransform>();
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		myUGUIObject a = createChild("ChildA", parentGo);
		myUGUIObject b = createChild("ChildB", parentGo);
		try
		{
			// a 已在末尾(唯一 a? a,b → a 不是末尾)。构造 3 个再验证
			myUGUIObject c = createChild("ChildC", parentGo);
			// c 已在末尾(index=2) → setAsLastSibling 直接 return
			c.setAsLastSibling(false);
			assertEqual(2, c.getGameObject().transform.GetSiblingIndex(), "已在末尾不变");
			// a 在开头(index=0) → setAsFirstSibling 直接 return
			a.setAsFirstSibling(false);
			assertEqual(0, a.getGameObject().transform.GetSiblingIndex(), "已在开头不变");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 加深: cull 链(cull → CanvasGroup.alpha + isCulled + canGenerateDepth 联动)
	private static void testCullChain()
	{
		myUGUIImageSimple img = createImage(out GameObject go);
		try
		{
			assertTrue(!img.isCulled(), "初始未剔除");
			assertTrue(img.canGenerateDepth(), "未剔除时可生成深度");
			img.cull(true);
			assertTrue(img.isCulled(), "cull(true) 后 isCulled true");
			assertTrue(!img.canGenerateDepth(), "剔除后不可生成深度");
			CanvasGroup group = go.GetComponent<CanvasGroup>();
			assertNotNull(group, "cull 自动添加 CanvasGroup");
			assertEqual(0.0f, group.alpha, 0.001f, "cull(true) CanvasGroup.alpha 0");
			img.cull(false);
			assertTrue(!img.isCulled(), "cull(false) 后 isCulled false");
			assertEqual(1.0f, group.alpha, 0.001f, "cull(false) CanvasGroup.alpha 1");
			img.cull(true);
			img.cull(false);
			assertTrue(!img.isCulled(), "连续 cull 切换链最终未剔除");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	private static myUGUIObject createChild(string name, GameObject parentGo)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		go.transform.SetParent(parentGo.transform);
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}
}
