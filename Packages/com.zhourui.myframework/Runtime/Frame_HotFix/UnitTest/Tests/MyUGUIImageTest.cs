using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIImage 深度测试(Image 封装, 补 MyUGUIButtonTest 未直接覆盖的 myUGUIImage 本体)
//   init: 无 Image 组件自动添加(isNewObject 不 logError)
//   setSpriteOnly: 不检查图集(Sprite.Create + ppu=1 规避 logWarning)
//   setColor/getColor/setAlpha/getAlpha / getAtlas(null)
// 环境: 裸 GameObject + RectTransform + myUGUIImage(setObject+init)
// 清理: 测试自己 new 的裸 GameObject + Sprite/Texture2D, 手动 DestroyImmediate
public static class MyUGUIImageTest
{
	public static void Run()
	{
		testInitAutoAddImage();
		testSetSpriteOnly();
		testColor();
		testAlpha();
		testAtlasNullByDefault();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIImage createImage(out GameObject go, out Sprite sprite, out Texture2D tex)
	{
		go = new GameObject("ImageGO");
		go.AddComponent<RectTransform>();
		myUGUIImage img = new myUGUIImage();
		img.setIsNewObject(true);
		img.setObject(go);
		img.init();
		tex = new Texture2D(16, 16);
		sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, 16.0f, 16.0f), new Vector2(0.5f, 0.5f), 1.0f);
		sprite.name = "TestImageSprite";
		return img;
	}

	// init: 无 Image 组件 → 自动添加
	private static void testInitAutoAddImage()
	{
		myUGUIImage img = createImage(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			assertTrue(go.GetComponent<Image>() != null, "init 自动添加 Image");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSpriteOnly: 设置精灵(不检查图集)
	private static void testSetSpriteOnly()
	{
		myUGUIImage img = createImage(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			img.setSpriteOnly(sprite);
			assertTrue(ReferenceEquals(sprite, go.GetComponent<Image>().sprite), "setSpriteOnly 设置到 Image.sprite");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setColor/getColor
	private static void testColor()
	{
		myUGUIImage img = createImage(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			Color color = new Color(0.5f, 0.25f, 1.0f, 0.8f);
			img.setColor(color);
			Color got = img.getColor();
			assertEqual(color.r, got.r, 0.001f, "颜色 R");
			assertEqual(color.g, got.g, 0.001f, "颜色 G");
			assertEqual(color.b, got.b, 0.001f, "颜色 B");
			assertEqual(color.a, got.a, 0.001f, "颜色 A");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAlpha/getAlpha
	private static void testAlpha()
	{
		myUGUIImage img = createImage(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			img.setAlpha(0.4f);
			assertEqual(0.4f, img.getAlpha(), 0.001f, "setAlpha(0.4) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 默认无图集
	private static void testAtlasNullByDefault()
	{
		myUGUIImage img = createImage(out GameObject go, out Sprite sprite, out Texture2D tex);
		try
		{
			assertTrue(img.getAtlas() == null, "未设置图集时 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
