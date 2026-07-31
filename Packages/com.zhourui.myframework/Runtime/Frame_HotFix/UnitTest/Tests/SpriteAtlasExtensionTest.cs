using UnityEngine;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor.U2D;
#endif
using static TestAssert;

public static class SpriteAtlasExtensionTest
{
	public static void Run()
	{
		testNullAtlas();
		testNullSprite();
		testBothNull();
		testEmptyAtlas();
		testSpriteInAtlas();
	}

	// ─── null atlas: 直接返回 false ────────────────────────────────
	private static void testNullAtlas()
	{
		Sprite sprite = Sprite.Create(new Texture2D(4, 4), new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
		bool result = ((SpriteAtlas)null).isSpriteInAtlas(sprite);
		assertFalse(result, "null atlas returns false");
		Object.DestroyImmediate(sprite);
	}

	// ─── null sprite: 直接返回 false ───────────────────────────────
	private static void testNullSprite()
	{
		SpriteAtlas atlas = new();
		bool result = atlas.isSpriteInAtlas(null);
		assertFalse(result, "null sprite returns false");
		Object.DestroyImmediate(atlas);
	}

	// ─── 两个都为 null ─────────────────────────────────────────────
	private static void testBothNull()
	{
		bool result = ((SpriteAtlas)null).isSpriteInAtlas(null);
		assertFalse(result, "both null returns false");
	}

	// ─── 空 atlas (无 packables) ────────────────────────────────────
	private static void testEmptyAtlas()
	{
		SpriteAtlas atlas = new();
		Sprite sprite = Sprite.Create(new Texture2D(2, 2), new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
		bool result = atlas.isSpriteInAtlas(sprite);
		assertFalse(result, "empty atlas returns false");
		Object.DestroyImmediate(sprite);
		Object.DestroyImmediate(atlas);
	}

	// ─── Sprite 在 atlas 中: 返回 true ─────────────────────────────
	private static void testSpriteInAtlas()
	{
#if UNITY_EDITOR
		Texture2D tex = new(4, 4);
		Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
		Sprite otherSprite = Sprite.Create(new Texture2D(4, 4), new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

		SpriteAtlas atlas = new();
		// 使用 Editor API 将 sprite 添加为 packable
		SpriteAtlasExtensions.Add(atlas, new Object[] { sprite });

		// sprite 在 atlas 中 → true
		bool result = atlas.isSpriteInAtlas(sprite);
		assertTrue(result, "sprite in atlas returns true");

		// otherSprite 不在 atlas 中 → false
		bool otherResult = atlas.isSpriteInAtlas(otherSprite);
		assertFalse(otherResult, "other sprite not in atlas");

		Object.DestroyImmediate(sprite);
		Object.DestroyImmediate(otherSprite);
		Object.DestroyImmediate(atlas);
		Object.DestroyImmediate(tex);
#endif
	}
}
