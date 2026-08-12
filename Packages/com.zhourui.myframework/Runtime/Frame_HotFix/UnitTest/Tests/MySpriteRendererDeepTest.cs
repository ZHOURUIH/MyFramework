using UnityEngine;
using static TestAssert;

// mySpriteRenderer 底层精灵渲染器封装深度测试(ClassObject, 直接持有 SpriteRenderer)
//   init(SpriteRenderer): 绑定渲染器 + 记录初始 sprite/材质; sprite null 时跳过图集检查(安全)
//   setSprite/setSpriteOnly: 设置精灵(同 sprite 早退; ppu=1 规避 warning)
//   setSpriteName: mAtlasPtr null 时直接 return(无图集环境安全)
//   cull/setAlpha/setColor: 纯渲染状态
// 环境: 裸 GameObject + SpriteRenderer(sprite 默认 null)
// 注意: 不调 destroy(依赖 mAtlasManager.unloadAtlas/mResourceManager, 测试环境跳过)
public static class MySpriteRendererDeepTest
{
	public static void Run()
	{
		testInitBind();
		testInitNoSpriteSafe();
		testSetSprite();
		testSetSpriteSameNoop();
		testSetSpriteOnly();
		testSetSpriteNameNoAtlas();
		testSetSpriteNameEmptyNoEffect();
		testGetSpriteSize();
		testGetSpriteNameDirty();
		testSetAlphaGetAlpha();
		testCull();
		testSetColor();
		testSetOrderInLayer();
		testSetRendererPriority();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static mySpriteRenderer createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex)
	{
		go = new GameObject("SpriteRendererGO");
		renderer = go.AddComponent<SpriteRenderer>();
		// 默认材质 "Sprites-Default" 会触发 init 的材质路径检查(logError+removeStart NRE) → 置 null 跳过
		renderer.sharedMaterial = null;
		mySpriteRenderer spriteWindow = new mySpriteRenderer();
		spriteWindow.init(renderer);
		tex = new Texture2D(32, 64);
		return spriteWindow;
	}

	private static Sprite createTestSprite(Texture2D tex, string name)
	{
		Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, 32.0f, 64.0f), new Vector2(0.5f, 0.5f), 1.0f);
		sprite.name = name;
		return sprite;
	}

	// init 绑定: renderer 同一实例 + 初始无 sprite + alpha 1
	private static void testInitBind()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			assertTrue(ReferenceEquals(renderer, spriteWindow.getSpriteRenderer()), "init 绑定同一渲染器");
			assertTrue(spriteWindow.getSprite() == null, "初始无 sprite");
			assertEqual(1.0f, spriteWindow.getAlpha(), 0.001f, "初始 alpha 1");
			assertEqual(Vector2.zero, spriteWindow.getSpriteSize(), "无 sprite 时尺寸 zero");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// init 无 sprite: 跳过图集检查分支(不依赖 ImageAtlasPath)
	private static void testInitNoSpriteSafe()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			assertTrue(spriteWindow.getSprite() == null, "无 sprite 安全初始化");
			assertTrue(spriteWindow.getOriginMaterialPath() == null, "初始材质路径 null(未记录)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSprite: 设置 + 读回
	private static void testSetSprite()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			Sprite sprite = createTestSprite(tex, "TestSpriteA");
			spriteWindow.setSprite(sprite);
			assertTrue(ReferenceEquals(sprite, spriteWindow.getSprite()), "setSprite 后读回同一 sprite");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSprite 同 sprite 早退(不重复设置, 无副作用)
	private static void testSetSpriteSameNoop()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			Sprite sprite = createTestSprite(tex, "TestSpriteB");
			spriteWindow.setSprite(sprite);
			spriteWindow.setSprite(sprite);
			assertTrue(ReferenceEquals(sprite, spriteWindow.getSprite()), "同 sprite 重复设置保持不变");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteOnly: ppu=1 规避 logWarning, 读回
	private static void testSetSpriteOnly()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			Sprite sprite = createTestSprite(tex, "TestSpriteC");
			spriteWindow.setSpriteOnly(sprite);
			assertTrue(ReferenceEquals(sprite, spriteWindow.getSprite()), "setSpriteOnly 后读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteName: 无图集(mAtlasPtr null)直接 return, sprite 不变
	private static void testSetSpriteNameNoAtlas()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			Sprite sprite = createTestSprite(tex, "TestSpriteD");
			spriteWindow.setSprite(sprite);
			spriteWindow.setSpriteName("SomeName");   // mAtlasPtr null → return
			assertTrue(ReferenceEquals(sprite, spriteWindow.getSprite()), "无图集时 setSpriteName 不改变 sprite");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteName 空串: 无图集时同样被早退拦截, sprite 不变
	private static void testSetSpriteNameEmptyNoEffect()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			Sprite sprite = createTestSprite(tex, "TestSpriteE");
			spriteWindow.setSprite(sprite);
			spriteWindow.setSpriteName("");
			assertTrue(ReferenceEquals(sprite, spriteWindow.getSprite()), "无图集时空串也不生效");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getSpriteSize: 无 sprite zero / 有 sprite rect.size
	private static void testGetSpriteSize()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			assertEqual(Vector2.zero, spriteWindow.getSpriteSize(), "无 sprite 尺寸 zero");
			Sprite sprite = createTestSprite(tex, "TestSpriteF");
			spriteWindow.setSprite(sprite);
			assertEqual(new Vector2(32.0f, 64.0f), spriteWindow.getSpriteSize(), "有 sprite 尺寸 = rect.size");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getSpriteName 延迟刷新: 初始 null → setSprite 后跟随 sprite.name
	private static void testGetSpriteNameDirty()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			assertTrue(spriteWindow.getSpriteName() == null, "初始 spriteName null");
			Sprite sprite = createTestSprite(tex, "RefreshSprite");
			spriteWindow.setSprite(sprite);
			assertEqual("RefreshSprite", spriteWindow.getSpriteName(), "setSprite 后名字延迟刷新");
			spriteWindow.setSprite(null);
			assertTrue(spriteWindow.getSpriteName() == null, "清除 sprite 后名字 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAlpha/getAlpha 往返
	private static void testSetAlphaGetAlpha()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			spriteWindow.setAlpha(0.5f);
			assertEqual(0.5f, spriteWindow.getAlpha(), 0.001f, "setAlpha(0.5) 读回");
			spriteWindow.setAlpha(0.0f);
			assertEqual(0.0f, spriteWindow.getAlpha(), 0.001f, "setAlpha(0)");
			spriteWindow.setAlpha(1.0f);
			assertEqual(1.0f, spriteWindow.getAlpha(), 0.001f, "setAlpha(1)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// cull: true → alpha 0; false → alpha 1
	private static void testCull()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			spriteWindow.cull(true);
			assertEqual(0.0f, spriteWindow.getAlpha(), 0.001f, "cull(true) alpha 0");
			spriteWindow.cull(false);
			assertEqual(1.0f, spriteWindow.getAlpha(), 0.001f, "cull(false) alpha 1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setColor: Color 重载 + Vector3 重载
	private static void testSetColor()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			spriteWindow.setColor(new Color(1.0f, 0.5f, 0.25f, 0.75f));
			Color color = spriteWindow.getSpriteRenderer().color;
			assertEqual(1.0f, color.r, 0.001f, "Color 重载 r");
			assertEqual(0.5f, color.g, 0.001f, "Color 重载 g");
			assertEqual(0.25f, color.b, 0.001f, "Color 重载 b");
			spriteWindow.setColor(new Vector3(0.1f, 0.2f, 0.3f));
			color = spriteWindow.getSpriteRenderer().color;
			assertEqual(0.1f, color.r, 0.001f, "Vector3 重载 r");
			assertEqual(0.2f, color.g, 0.001f, "Vector3 重载 g");
			assertEqual(0.3f, color.b, 0.001f, "Vector3 重载 b");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOrderInLayer: sortingOrder 读回
	private static void testSetOrderInLayer()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			spriteWindow.setOrderInLayer(5);
			assertEqual(5, spriteWindow.getOrderInLayer(), "setOrderInLayer(5)");
			spriteWindow.setOrderInLayer(-3);
			assertEqual(-3, spriteWindow.getOrderInLayer(), "setOrderInLayer(-3)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setRendererPriority: rendererPriority 读回
	private static void testSetRendererPriority()
	{
		mySpriteRenderer spriteWindow = createRenderer(out GameObject go, out SpriteRenderer renderer, out Texture2D tex);
		try
		{
			spriteWindow.setRendererPriority(100);
			assertEqual(100, spriteWindow.getRendererPriority(), "setRendererPriority(100)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
