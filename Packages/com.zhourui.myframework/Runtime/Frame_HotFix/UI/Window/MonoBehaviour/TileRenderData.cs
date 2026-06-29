using UnityEngine;

public class TileRenderData : ClassObject
{
	public SpriteData mSpriteData;		// 显示的图片信息
	public Vector3 mPosition;			// 图片中心的位置
	public Vector2 mSize;				// 图片的渲染大小
	public void init(Sprite sprite, Vector3 pos, Vector2 size)
	{
		mSpriteData.init(sprite);
		mPosition = pos;
		mSize = size;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mSpriteData.resetProperty();
		mPosition = Vector3.zero;
		mSize = Vector2.zero;
	}
	public void cloneTo(TileRenderData other)
	{
		other.mSpriteData = mSpriteData;
		other.mPosition = mPosition;
		other.mSize = mSize;
	}
}