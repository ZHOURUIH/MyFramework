using UnityEngine;
using static TestAssert;

// TileRenderData 平铺渲染数据单元测试(纯逻辑, 继承 ClassObject 无池依赖)
// cloneTo: mSpriteData(struct 值复制)/mPosition/mSize 复制
// resetProperty: SpriteData 字段置零 + mPosition/mSize 归零
// init 依赖真实 Sprite(sprite.texture/rect/uv), 不测
public static class TileRenderDataTest
{
	public static void Run()
	{
		testCloneTo();
		testCloneToSpriteData();
		testResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// cloneTo — mPosition/mSize 复制
	// ═════════════════════════════════════════════════════════════════
	private static void testCloneTo()
	{
		TileRenderData src = new TileRenderData();
		src.mPosition = new Vector3(1.0f, 2.0f, 3.0f);
		src.mSize = new Vector2(4.0f, 5.0f);
		TileRenderData dst = new TileRenderData();
		dst.mPosition = new Vector3(9.0f, 9.0f, 9.0f);
		dst.mSize = new Vector2(8.0f, 8.0f);
		src.cloneTo(dst);
		assertEqual(new Vector3(1.0f, 2.0f, 3.0f), dst.mPosition, "cloneTo 复制 mPosition");
		assertEqual(new Vector2(4.0f, 5.0f), dst.mSize, "cloneTo 复制 mSize");
	}

	// ═════════════════════════════════════════════════════════════════
	// cloneTo — mSpriteData(struct 值复制, 含内部字段)
	// ═════════════════════════════════════════════════════════════════
	private static void testCloneToSpriteData()
	{
		TileRenderData src = new TileRenderData();
		// 手工设置 SpriteData 字段(不调 init, 避免真实 Sprite 依赖)
		src.mSpriteData.mWidth = 100;
		src.mSpriteData.mHeight = 200;
		Vector2[] uvs = { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f) };
		src.mSpriteData.mUVs = uvs;
		TileRenderData dst = new TileRenderData();
		src.cloneTo(dst);
		assertEqual(100, dst.mSpriteData.mWidth, "cloneTo 复制 mSpriteData.mWidth");
		assertEqual(200, dst.mSpriteData.mHeight, "cloneTo 复制 mSpriteData.mHeight");
		// struct 值复制: mUVs 数组引用共享
		assertTrue(ReferenceEquals(uvs, dst.mSpriteData.mUVs), "cloneTo 后 mSpriteData.mUVs 引用共享");
		assertEqual(2, dst.mSpriteData.mUVs.Length, "cloneTo 后 mSpriteData.mUVs 长度正确");
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty — SpriteData 字段置零 + mPosition/mSize 归零
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty()
	{
		TileRenderData data = new TileRenderData();
		data.mPosition = new Vector3(1.0f, 2.0f, 3.0f);
		data.mSize = new Vector2(4.0f, 5.0f);
		data.mSpriteData.mWidth = 100;
		data.mSpriteData.mUVs = new Vector2[2];
		data.resetProperty();
		assertEqual(Vector3.zero, data.mPosition, "resetProperty 后 mPosition 归零");
		assertEqual(Vector2.zero, data.mSize, "resetProperty 后 mSize 归零");
		assertEqual(0, data.mSpriteData.mWidth, "resetProperty 后 mSpriteData.mWidth 归 0");
		assertEqual(0, data.mSpriteData.mHeight, "resetProperty 后 mSpriteData.mHeight 归 0");
		assertNull(data.mSpriteData.mUVs, "resetProperty 后 mSpriteData.mUVs 为 null");
		assertNull(data.mSpriteData.mSprite, "resetProperty 后 mSpriteData.mSprite 为 null");
		assertNull(data.mSpriteData.mTexture, "resetProperty 后 mSpriteData.mTexture 为 null");
	}
}
