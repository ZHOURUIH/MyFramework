using UnityEngine;
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
}
