using UnityEngine;

public struct DamageNumberSpriteData
{
	public Texture mTexture;
	public Sprite mSprite;
	public int mWidth;
	public int mHeight;
	public Vector2[] mUVs;
	public void init(Sprite sprite)
	{
		mSprite = sprite;
		mTexture = sprite.texture;
		mWidth = (int)sprite.rect.width;
		mHeight = (int)sprite.rect.height;
		mUVs = sprite.uv;
	}
}