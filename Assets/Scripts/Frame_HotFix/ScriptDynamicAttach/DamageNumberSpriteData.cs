using UnityEngine;

public struct DamageNumberSpriteData
{
	public char mSearchKey;			// 索引标记,比如数字0到9的索引标记就是字符0到9,可以自定义其他图片的索引标记
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