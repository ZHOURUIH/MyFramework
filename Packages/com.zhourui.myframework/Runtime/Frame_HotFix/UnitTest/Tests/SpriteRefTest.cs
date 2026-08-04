using UnityEngine;
using static TestAssert;
using static FrameBaseHotFix;

// SpriteRef 单元测试：setSprite/destroy/名称缓存/isValid/resetProperty
public static class SpriteRefTest
{
	public static void Run()
	{
		testSetSpriteValid();
		testSetSpriteCachesName();
		testIsValidNullSprite();
		testIsValidValidSprite();
		testGetSprite();
		testGetSpriteName();
		testResetProperty();
		testResetPropertyAfterSetSprite();
		testSetSpriteWithNullAtlas();
		testSetSpriteWithAtlasRef();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static Sprite createSprite(string name)
	{
		var tex = new Texture2D(4, 4);
		var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
		sprite.name = name;
		return sprite;
	}

	private static void testSetSpriteValid()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("test_sprite");
		sr.setSprite(sprite, null);
		assertNotNull(sr.getSprite(), "setSprite 后 sprite 不为 null");
		assertTrue(sr.isValid(), "isValid 返回 true");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testSetSpriteCachesName()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("cached_name");
		sr.setSprite(sprite, null);
		assertEqual("cached_name", sr.getSpriteName(), "spriteName 缓存了 sprite.name");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testIsValidNullSprite()
	{
		var sr = new SpriteRef();
		assertFalse(sr.isValid(), "未设置 sprite 时 isValid=false");
	}

	private static void testIsValidValidSprite()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("test");
		sr.setSprite(sprite, null);
		assertTrue(sr.isValid(), "设置 sprite 后 isValid=true");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testGetSprite()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("get_sprite_test");
		sr.setSprite(sprite, null);
		assertEqual(sprite, sr.getSprite(), "getSprite 返回设置的 sprite");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testGetSpriteName()
	{
		var sr = new SpriteRef();
		assertNull(sr.getSpriteName(), "未设置 sprite 时 spriteName=null");
		var sprite = createSprite("my_sprite");
		sr.setSprite(sprite, null);
		assertEqual("my_sprite", sr.getSpriteName(), "getSpriteName 返回缓存的名称");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testResetProperty()
	{
		var sr = new SpriteRef();
		sr.resetProperty();
		assertNull(sr.getSprite(), "resetProperty 后 sprite=null");
		assertNull(sr.getSpriteName(), "resetProperty 后 spriteName=null");
		assertFalse(sr.isValid(), "resetProperty 后 isValid=false");
	}

	private static void testResetPropertyAfterSetSprite()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("reset_test");
		sr.setSprite(sprite, null);
		sr.resetProperty();
		assertNull(sr.getSprite(), "resetProperty 后 sprite=null");
		assertNull(sr.getSpriteName(), "resetProperty 后 spriteName=null");
		assertFalse(sr.isValid(), "resetProperty 后 isValid=false");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testSetSpriteWithNullAtlas()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("no_atlas");
		sr.setSprite(sprite, null);
		assertEqual(sprite, sr.getSprite(), "sprite 设置成功");
		assertEqual("no_atlas", sr.getSpriteName(), "名称缓存");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}

	private static void testSetSpriteWithAtlasRef()
	{
		var sr = new SpriteRef();
		var sprite = createSprite("with_atlas");
		// 创建 mock AtlasRef（不需要真实 AtlasBase）
		var atlasRef = new AtlasRef();
		sr.setSprite(sprite, atlasRef);
		assertEqual(sprite, sr.getSprite(), "sprite 设置成功");
		assertEqual("with_atlas", sr.getSpriteName(), "名称缓存");

		// resetProperty 清除引用
		sr.resetProperty();
		assertNull(sr.getSprite(), "reset 后 sprite=null");
		Object.DestroyImmediate(sprite.texture);
		Object.DestroyImmediate(sprite);
	}
}
