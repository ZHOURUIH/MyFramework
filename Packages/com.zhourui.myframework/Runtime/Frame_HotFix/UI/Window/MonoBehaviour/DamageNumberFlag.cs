using UnityEngine;

public class DamageNumberFlag : ClassObject
{
	public Sprite mSprite;          // 显示的图片
	public Vector2[] mUVs;          // 缓存的uv
	public float mSpriteWidth;      // 缓存的图片宽度
	public float mSpriteHeight;     // 缓存的图片高度
	public float mOffsetX;          // 渲染的位置偏移X
	public float mOffsetY;          // 渲染的位置偏移Y
	public float mScale = 1.0f;     // 渲染的缩放,因为图片一般都是保持宽高比显示的,所以只需要设置缩放
	public override void resetProperty()
	{
		base.resetProperty();
		mSprite = null;
		mUVs = null;
		mSpriteWidth = 0;
		mSpriteHeight = 0;
		mOffsetX = 0.0f;
		mOffsetY = 0.0f;
		mScale = 1.0f;
	}
	public void setSprite(Sprite sprite)
	{
		mSprite = sprite;
		mUVs = sprite.uv;
		mSpriteWidth = mSprite.rect.width * mScale;
		mSpriteHeight = mSprite.rect.height * mScale;
	}
	public void setScale(float scale)
	{
		mScale = scale;
		mSpriteWidth = mSprite.rect.width * mScale;
		mSpriteHeight = mSprite.rect.height * mScale;
	}
	public void setOffset(float x, float y)
	{
		mOffsetX = x;
		mOffsetY = y;
	}
}