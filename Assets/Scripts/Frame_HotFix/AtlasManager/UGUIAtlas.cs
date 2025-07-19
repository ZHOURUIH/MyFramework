using UnityEngine.U2D;
using static StringUtility;

// 对SpriteAtlas的封装,主要是实现对Sprite的缓存
public class UGUIAtlas : AtlasBase
{
	public SpriteAtlas mSpriteAtlas;			// 图集对象,尽量不要直接访问公有变量
	public override bool isValid()				{ return mSpriteAtlas != null; }
	public override string getName()			{ return mSpriteAtlas != null ? mSpriteAtlas.name : EMPTY; }
	public SpriteAtlas getAtlas()				{ return mSpriteAtlas; }
	public void setAtlas(SpriteAtlas atlas)		{ mSpriteAtlas = atlas; }
}