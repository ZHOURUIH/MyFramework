using System.Collections.Generic;
using static UnityUtility;

// 专用于显示大量简单Sprite的窗口,仅限在同一图集中的图片
public class myUGUITileImage : myUGUIObject
{
	protected TileImageRenderer mRenderer;		// 渲染组件
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
		setAlpha(1.0f, false);
		base.destroy();
	}
	public override void cloneFrom(myUGUIObject obj)
	{
		base.cloneFrom(obj);
	}
	public void setTileList(List<TileRenderData> list) { mRenderer.setTileList(list); }
	public void setTileMap(Dictionary<object, TileRenderData> list) { mRenderer.setTileMap(list); }
	public int getTileCount() { return mRenderer.getTileCount(); }
	public void clearTile() { mRenderer.clearTile(); }
	public void setSortingLayerName(string name) { mRenderer.setSortingLayerName(name); }
	public void setSortingOrder(int order) { mRenderer.setSortingOrder(order); }
}