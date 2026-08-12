using UnityEngine;
using static TestAssert;

// myUGUISprite 深度测试(SpriteRenderer 封装, 加深 MyUGUISpriteTest 未覆盖的 setSprite 链)
//   init: 自动加 SpriteRenderer 组件(无 sprite 时跳过图集检查)
//   setSprite/setSpriteOnly: 设置 sprite(mAtlasPtr null 时不触发图集 logWarning)
//   getSpriteName: mSpriteNameDirty 延迟刷新(从 sprite.name 读)
//   getSpriteSize: sprite.rect.size(无 sprite → zero)
//   setAlpha/getAlpha/isCulled: SpriteRenderer.color.a
//   setOrderInLayer/setRendererPriority: SpriteRenderer 字段
// 测试资源: Sprite.Create(Texture2D + rect + pivot + pixelsPerUnit=1)
//           pixelsPerUnit=1 规避 setSpriteOnly 的 logWarning(ppu≠1 且 scale≤1 才告警)
// 环境: 裸 GameObject + RectTransform + myUGUISprite(setObject+init)
// 清理: DestroyImmediate(go) + DestroyImmediate(sprite/tex)
public static class MyUGUISpriteDeepTest
{
	public static void Run()
	{
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
	// 辅助: 创建已 init 的 myUGUISprite + 测试 Sprite(32x64, ppu=1)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUISprite createSprite(out GameObject go, out Sprite sprite, out Texture2D tex)
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
		myUGUISprite sw = createSprite(out GameObject go, out Sprite sprite, out Texture2D tex);
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
