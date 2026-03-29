using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;

// 专用于显示大量简单Sprite的窗口,仅限在同一图集中的图片
public class myUGUITileImage : myUGUIObject
{
	protected TileImageRenderer mRenderer;		// 渲染组件
	protected AtlasRef mAtlasPtr;				// 当前正在使用的图片图集
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		if (!mObject.TryGetComponent(out mRenderer))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个TileImageRenderer组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mRenderer = mObject.AddComponent<TileImageRenderer>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public override void destroy()
	{
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		setAlpha(1.0f, false);
		mAtlasPtr = null;
		base.destroy();
	}
	public override void cloneFrom(myUGUIObject obj)
	{
		base.cloneFrom(obj);
		var source = obj as myUGUITileImage;
		mRenderer.setTexture(source.mRenderer.getTexture());
	}
	public void setAtlas(AtlasRef atlas)
	{
		mAtlasPtr = atlas;
		clearTile();
	}
	public void addTile(Vector3 pos, Vector2 size, string spriteName) 
	{
		addTile(pos, size, mAtlasPtr.getSprite(spriteName));
	}
	// pos是屏幕上的坐标,size是显示的大小,sprite是要显示的图片
	public void addTile(Vector3 pos, Vector2 size, Sprite sprite)
	{
		CLASS(out TileData data);
		data.mPosition = pos;
		data.mSize = size;
		data.mSpriteData.init(sprite);
		mRenderer.getTiles().add(data);
	}
	public void setTexture(Texture texture) { mRenderer.setTexture(texture); }
	public void setTileList(List<TileData> list) { mRenderer.setTileList(list); }
	public void setTileMap(Dictionary<object, TileData> list) { mRenderer.setTileMap(list); }
	public void clearTile() { mRenderer.clearTile(); }
	public Sprite getSprite(string name) { return mAtlasPtr.getSprite(name); }
}