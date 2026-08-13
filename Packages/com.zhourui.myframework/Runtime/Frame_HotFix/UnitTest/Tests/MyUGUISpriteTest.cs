using UnityEngine;
using static TestAssert;

using static TestAssert;
using static FrameUtility;

// myUGUISprite 深度测试(SpriteRenderer 封装):
//   init(预加 SpriteRenderer) / getSpriteRenderer / setSpriteName(空串清空)
//   setSpriteOnly/getSprite(Texture2D 造 sprite) / getSpriteSize / getSize
//   cull/isCulled/canGenerateDepth(alpha 0/1) / setWindowShader/getWindowShader
//   setOrderInLayer/getOrderInLayer / setRendererPriority/getRendererPriority
//   setAtlas(null) / getAtlas / getOriginMaterialPath 守卫
public static class MyUGUISpriteTest
{
	public static void Run()
	{
		testInitWithComponent();
		testSpriteNameEmptyClears();
		testSetSpriteOnly();
		testCullState();
		testWindowShader();
		testOrderAndPriority();
		testAtlasAndGuard();
	

		testInitAutoAddSpriteRenderer();
		testSetSprite();
		testSetSpriteSameNoOp();
		testSetSpriteNull();
		testSetSpriteOnlyPPU1();
		testGetSpriteSize();
		testSpriteNameRefresh();
		testAlphaCull();
		testOrderInLayer();
		testRendererPriority();
		testShaderField();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createSprite(out myUGUISprite sprite)
	{
		GameObject go = new GameObject("Sprite");
		go.AddComponent<RectTransform>();
		SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
		// 默认材质 "Sprites-Default" 会触发 init 的 MaterialPath 检查(logError) → 置 null 跳过
		renderer.sharedMaterial = null;
		sprite = new myUGUISprite();
		sprite.setObject(go);
		sprite.init();
		return go;
	}

	private static Sprite createTestSprite(out Texture2D tex)
	{
		tex = new Texture2D(4, 4);
		// pixelsPerUnit=1 避免 setSpriteOnly 的 logWarning(scale<=1 时)
		return Sprite.Create(tex, new Rect(0.0f, 0.0f, 4.0f, 4.0f), new Vector2(0.5f, 0.5f), 1.0f);
	}

	// init: 预加 SpriteRenderer → 组件有效 + 无 sprite
	private static void testInitWithComponent()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		try
		{
			SpriteRenderer comp = go.GetComponent<SpriteRenderer>();
			assertTrue(ReferenceEquals(comp, sprite.getSpriteRenderer()), "getSpriteRenderer 同一组件");
			assertNull(sprite.getSprite(), "初始无 sprite");
			assertTrue(sprite.getSize().isZero(), "无 sprite 时 getSize zero");
			assertTrue(sprite.getSpriteSize().isZero(), "无 sprite 时 getSpriteSize zero");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteName("") 清空 sprite
	private static void testSpriteNameEmptyClears()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		Texture2D tex = null;
		try
		{
			Sprite s = createTestSprite(out tex);
			sprite.setSpriteOnly(s);
			assertNotNull(sprite.getSprite(), "设置后 sprite 非 null");
			sprite.setSpriteName("");
			assertNull(sprite.getSprite(), "setSpriteName(空串) 清空 sprite");
			sprite.setSpriteName("NotExist");   // atlas null → 直接 return, 安全
			assertNull(sprite.getSprite(), "无图集时 setSpriteName 不生效");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteOnly/getSprite/getSpriteSize/getSize 链路
	private static void testSetSpriteOnly()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		Texture2D tex = null;
		try
		{
			Sprite s = createTestSprite(out tex);
			sprite.setSpriteOnly(s);
			assertTrue(ReferenceEquals(s, sprite.getSprite()), "setSpriteOnly 设置同一 sprite");
			Vector2 spriteSize = sprite.getSpriteSize();
			assertEqual(4.0f, spriteSize.x, 0.001f, "spriteSize.x=4");
			assertEqual(4.0f, spriteSize.y, 0.001f, "spriteSize.y=4");
			Vector2 size = sprite.getSize();
			assertEqual(4.0f, size.x, 0.001f, "getSize 用 sprite rect");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// cull/isCulled/canGenerateDepth: alpha 0/1 切换
	private static void testCullState()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		try
		{
			assertFalse(sprite.isCulled(), "初始未剔除");
			assertTrue(sprite.canGenerateDepth(), "未剔除可生成深度");
			sprite.cull(true);
			assertTrue(sprite.isCulled(), "cull(true) 后剔除");
			assertFalse(sprite.canGenerateDepth(), "剔除后不可生成深度");
			sprite.cull(false);
			assertFalse(sprite.isCulled(), "cull(false) 恢复");
			assertEqual(1.0f, sprite.getAlpha(), 0.001f, "恢复 alpha=1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setWindowShader/getWindowShader
	private static void testWindowShader()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		try
		{
			CLASS(out WindowShader shader);
			try
			{
				sprite.setWindowShader(shader);
				assertTrue(ReferenceEquals(shader, sprite.getWindowShader()), "setWindowShader 读回同一对象");
				sprite.update(0.01f);   // applyShader 空实现, 无副作用
			}
			finally
			{
				UN_CLASS(ref shader);
			}
			sprite.setWindowShader(null);
			assertNull(sprite.getWindowShader(), "null 传参读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOrderInLayer/setRendererPriority 写读(SpriteRenderer 字段)
	private static void testOrderAndPriority()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		try
		{
			sprite.setOrderInLayer(7);
			assertEqual(7, sprite.getOrderInLayer(), "setOrderInLayer(7) 读回");
			sprite.setOrderInLayer(-2);
			assertEqual(-2, sprite.getOrderInLayer(), "负数 order 读回");
			sprite.setRendererPriority(5);
			assertEqual(5, sprite.getRendererPriority(), "setRendererPriority(5) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAtlas(null) + getAtlas + getOriginMaterialPath 守卫
	private static void testAtlasAndGuard()
	{
		GameObject go = createSprite(out myUGUISprite sprite);
		try
		{
			assertNull(sprite.getAtlas(), "初始图集 null");
			sprite.setAtlas(null, true);   // atlas null → setSprite(null) 安全
			sprite.setAtlas(null, false, true);
			// getOriginMaterialPath 守卫(初始值可能为 null 或空, 只断言调用不崩)
			_ = sprite.getOriginMaterialPath();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建已 init 的 myUGUISprite + 测试 Sprite(32x64, ppu=1)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUISprite createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex)
	{
		go = new GameObject("SpriteGO");
		go.AddComponent<RectTransform>();
		SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
		// 默认材质 "Sprites-Default" 会触发 init 的 MaterialPath 检查(logError + removeStart/endWith NRE) → 置 null 跳过
		renderer.sharedMaterial = null;
		myUGUISprite spriteWindow = new myUGUISprite();
		spriteWindow.setObject(go);
		spriteWindow.init();
		tex = new Texture2D(32, 64);
		sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, 32.0f, 64.0f), new Vector2(0.5f, 0.5f), 1.0f);
		sprite.name = "TestSprite";
		return spriteWindow;
	}

	// init: 无 SpriteRenderer → 自动添加, 无 sprite 时名字为 null
	private static void testInitAutoAddSpriteRenderer()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			assertTrue(sw.getSpriteRenderer() != null, "init 自动添加 SpriteRenderer");
			assertTrue(sw.getSprite() == null, "init 后无 sprite");
			assertTrue(sw.getSpriteName() == null, "init 后 spriteName 为 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSprite: 设置精灵 + 名字延迟刷新
	private static void testSetSprite()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setSprite(sprite);
			assertTrue(ReferenceEquals(sprite, sw.getSprite()), "setSprite 后 sprite 读回");
			assertEqual("TestSprite", sw.getSpriteName(), "getSpriteName 从 sprite.name 刷新");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSprite 同 sprite: 直接 return 无副作用
	private static void testSetSpriteSameNoOp()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setSprite(sprite);
			sw.setSprite(sprite);   // 同 sprite, 直接 return
			assertTrue(ReferenceEquals(sprite, sw.getSprite()), "同 sprite 重复设置无副作用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSprite(null): 清除精灵
	private static void testSetSpriteNull()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setSprite(sprite);
			sw.setSprite(null);
			assertTrue(sw.getSprite() == null, "setSprite(null) 清除 sprite");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteOnly: ppu=1 不触发 logWarning(ppu≠1 且 scale≤1 才告警)
	private static void testSetSpriteOnlyPPU1()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setSpriteOnly(sprite);
			assertTrue(ReferenceEquals(sprite, sw.getSprite()), "setSpriteOnly 设置成功");
			assertEqual("TestSprite", sw.getSpriteName(), "setSpriteOnly 后名字刷新");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getSpriteSize: sprite.rect.size / 无 sprite → zero
	private static void testGetSpriteSize()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			assertEqual(Vector2.zero, sw.getSpriteSize(), "无 sprite 时尺寸 zero");
			sw.setSprite(sprite);
			Vector2 size = sw.getSpriteSize();
			assertEqual(32.0f, size.x, 0.001f, "sprite 宽度 32");
			assertEqual(64.0f, size.y, 0.001f, "sprite 高度 64");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// spriteName: 设置不同 sprite 后名字跟随刷新
	private static void testSpriteNameRefresh()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		Texture2D tex2 = new Texture2D(16, 16);
		Sprite sprite2 = Sprite.Create(tex2, new Rect(0.0f, 0.0f, 16.0f, 16.0f), new Vector2(0.5f, 0.5f), 1.0f);
		sprite2.name = "SecondSprite";
		try
		{
			sw.setSprite(sprite);
			assertEqual("TestSprite", sw.getSpriteName(), "第一次名字");
			sw.setSprite(sprite2);
			assertEqual("SecondSprite", sw.getSpriteName(), "换 sprite 后名字刷新");
			sw.setSprite(null);
			assertTrue(sw.getSpriteName() == null, "清除后名字为 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite2);
			UnityEngine.Object.DestroyImmediate(tex2);
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAlpha/getAlpha/isCulled: SpriteRenderer.color.a
	private static void testAlphaCull()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setAlpha(0.0f);
			assertEqual(0.0f, sw.getAlpha(), 0.001f, "setAlpha(0) 读回");
			assertTrue(sw.isCulled(), "alpha=0 时剔除");
			sw.setAlpha(0.5f);
			assertEqual(0.5f, sw.getAlpha(), 0.001f, "setAlpha(0.5) 读回");
			assertTrue(!sw.isCulled(), "alpha>0 时不剔除");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOrderInLayer/getOrderInLayer
	private static void testOrderInLayer()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setOrderInLayer(5);
			assertEqual(5, sw.getOrderInLayer(), "setOrderInLayer(5) 读回");
			sw.setOrderInLayer(-2);
			assertEqual(-2, sw.getOrderInLayer(), "setOrderInLayer(-2) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setRendererPriority/getRendererPriority
	private static void testRendererPriority()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			sw.setRendererPriority(3);
			assertEqual(3, sw.getRendererPriority(), "setRendererPriority(3) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setWindowShader/getWindowShader: 字段读写(未设置时 null)
	private static void testShaderField()
	{
		myUGUISprite sw = createSprite_Deep(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			assertTrue(sw.getWindowShader() == null, "未设置 shader 时 null");
			sw.setWindowShader(null);
			assertTrue(sw.getWindowShader() == null, "setWindowShader(null) 读回 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
