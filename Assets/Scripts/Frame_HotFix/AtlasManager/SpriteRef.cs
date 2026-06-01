using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;

// 开放给外部以持有Sprite,可以进行引用计数
// 不过这里实际上只会去对图集进行引用计数,因为Sprite本身不需要进行引用计数,只要图集不被卸载了,Sprite就不会被卸载
// 减少一套引用计数的流程
// 但是实际上使用频率很低
public class SpriteRef : ClassObject
{
	private Sprite mSprite;             // 引用的图片
	private AtlasRef mAtlas;			// 所属的图集,主要用于在销毁时减少图集的引用计数,如果没有图集,则不需要减少图集的引用计数
	private string mSpriteName;         // 图片的名字,避免访问name而产生GC
	public override void resetProperty()
	{
		base.resetProperty();
		mSprite = null;
		mAtlas = null;
		mSpriteName = null;
	}
	public void setSprite(Sprite sprite, AtlasRef atlas)
	{
		mSprite = sprite;
		mSpriteName = null;
		if (mSprite == null)
		{
			logError("sprite is null");
			return;
		}
		mSpriteName = sprite.name;
		mAtlas = atlas;
	}
	public bool isValid()								{ return mSprite != null; }
	public Sprite getSprite()							{ return mSprite; }
	public string getSpriteName()						{ return mSpriteName; }
	public override void destroy()
	{
		base.destroy();
		if (mSprite == null)
		{
			logError("sprite is null");
			return;
		}
		
		mAtlasManager.unloadAtlas(ref mAtlas);
	}
}