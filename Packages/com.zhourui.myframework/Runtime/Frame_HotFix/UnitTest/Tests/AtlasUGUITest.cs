using UnityEngine;
using UnityEngine.U2D;
using UObject = UnityEngine.Object;
using static TestAssert;

// AtlasBase 具体子类测试: AtlasUGUI(SpriteAtlas 封装) / AtlasTP(Texture2D 封装)
// 纯逻辑: 构造存 base asset(可传 null), setAtlas 缓存对象+名字, isValid/getName/getAtlas 读取
// 不调 destroy(AtlasBase.destroy 依赖 mResourceManager 单例, 与 AtlasManagerTest.MockAtlas 一致规避)
// 测试自 new 的 SpriteAtlas/Texture2D 在 finally 中 DestroyImmediate 清理
public static class AtlasUGUITest
{
	public static void Run()
	{
		testUGUIConstructNull();
		testUGUISetAtlas();
		testUGUISetAtlasNullName();
		testTPConstructNull();
		testTPSetAtlas();
		testTPSetAtlasNullName();
	}

	// ═════════════════════════════════════════════════════════════════
	// AtlasUGUI — 构造 null asset, 未 setAtlas: 无效/无名
	// ═════════════════════════════════════════════════════════════════
	private static void testUGUIConstructNull()
	{
		AtlasUGUI atlas = new AtlasUGUI(null);
		assertFalse(atlas.isValid(), "未 setAtlas 时 isValid 应为 false");
		assertNull(atlas.getName(), "未 setAtlas 时 getName 应为 null");
		assertNull(atlas.getAtlas(), "未 setAtlas 时 getAtlas 应为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// AtlasUGUI — setAtlas 真实 SpriteAtlas
	// ═════════════════════════════════════════════════════════════════
	private static void testUGUISetAtlas()
	{
		SpriteAtlas spriteAtlas = new SpriteAtlas();
		try
		{
			AtlasUGUI atlas = new AtlasUGUI(null);
			atlas.setAtlas(spriteAtlas);
			assertTrue(atlas.isValid(), "setAtlas 后 isValid 应为 true");
			assertTrue(ReferenceEquals(spriteAtlas, atlas.getAtlas()), "getAtlas 应返回 setAtlas 传入的同一对象");
			assertEqual(spriteAtlas.name, atlas.getName(), "getName 应缓存 SpriteAtlas.name");
		}
		finally
		{
			UObject.DestroyImmediate(spriteAtlas);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// AtlasUGUI — setAtlas(null) 触发 NRE(源码行为), 不测;
	// 改测: 二次 setAtlas 覆盖缓存
	// ═════════════════════════════════════════════════════════════════
	private static void testUGUISetAtlasNullName()
	{
		SpriteAtlas atlas1 = new SpriteAtlas();
		SpriteAtlas atlas2 = new SpriteAtlas();
		try
		{
			AtlasUGUI atlas = new AtlasUGUI(null);
			atlas.setAtlas(atlas1);
			atlas.setAtlas(atlas2);
			assertTrue(ReferenceEquals(atlas2, atlas.getAtlas()), "二次 setAtlas 后 getAtlas 应返回新对象");
			assertEqual(atlas2.name, atlas.getName(), "二次 setAtlas 后 getName 应更新为新名字");
		}
		finally
		{
			UObject.DestroyImmediate(atlas1);
			UObject.DestroyImmediate(atlas2);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// AtlasTP — 构造 null asset, 未 setAtlas: 无效/无名
	// ═════════════════════════════════════════════════════════════════
	private static void testTPConstructNull()
	{
		AtlasTP atlas = new AtlasTP(null);
		assertFalse(atlas.isValid(), "未 setAtlas 时 isValid 应为 false");
		assertNull(atlas.getName(), "未 setAtlas 时 getName 应为 null");
		assertNull(atlas.getAtlas(), "未 setAtlas 时 getAtlas 应为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// AtlasTP — setAtlas 真实 Texture2D
	// ═════════════════════════════════════════════════════════════════
	private static void testTPSetAtlas()
	{
		Texture2D texture = new Texture2D(4, 4);
		try
		{
			AtlasTP atlas = new AtlasTP(null);
			atlas.setAtlas(texture);
			assertTrue(atlas.isValid(), "setAtlas 后 isValid 应为 true");
			assertTrue(ReferenceEquals(texture, atlas.getAtlas()), "getAtlas 应返回 setAtlas 传入的同一对象");
			assertEqual(texture.name, atlas.getName(), "getName 应缓存 Texture2D.name");
		}
		finally
		{
			UObject.DestroyImmediate(texture);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// AtlasTP — 二次 setAtlas 覆盖缓存
	// ═════════════════════════════════════════════════════════════════
	private static void testTPSetAtlasNullName()
	{
		Texture2D tex1 = new Texture2D(2, 2);
		Texture2D tex2 = new Texture2D(2, 2);
		try
		{
			AtlasTP atlas = new AtlasTP(null);
			atlas.setAtlas(tex1);
			atlas.setAtlas(tex2);
			assertTrue(ReferenceEquals(tex2, atlas.getAtlas()), "二次 setAtlas 后 getAtlas 应返回新对象");
			assertEqual(tex2.name, atlas.getName(), "二次 setAtlas 后 getName 应更新为新名字");
		}
		finally
		{
			UObject.DestroyImmediate(tex1);
			UObject.DestroyImmediate(tex2);
		}
	}
}
