using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static FrameDefine;
using static FrameBaseHotFix;

// 可显示数字的窗口,只支持整数,且每个数字图片的大小必须一样,不能显示小数,负数
// 因为使用了自定义的组件,所以性能上比myUGUINumber更高,只是相比之下myUGUINumber更加灵活一点
public class myUGUIImageNumber : myUGUIObject
{
	protected ImageNumber mRenderer;					// 渲染组件
	protected AtlasRef mOriginAtlasPtr = new();			// 初始的图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected AtlasRef mAtlasPtr = new();				// 当前正在使用的图片图集
	protected Sprite mOriginSprite;                     // 备份加载物体时原始的精灵图片
	protected string mNumberStyle;						// 数字图集名
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		if (!mObject.TryGetComponent(out mRenderer))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个ImageNumber组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mRenderer = mObject.AddComponent<ImageNumber>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}

		mOriginSprite = mRenderer.sprite;
		// 获取初始的精灵所在图集
		if (mOriginSprite != null)
		{
			if (!mObject.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
			{
				logError("找不到图集,请添加ImageAtlasPath组件, window:" + mName + ", layout:" + mLayout.getName());
			}
			string atlasPath = imageAtlasPath.mAtlasPath;
			if (atlasPath.isEmpty())
			{
				logError("ImageAtlasPath中记录的路径为空,GameObject:" + getGameObjectPath(mObject));
			}
			// unity_builtin_extra是unity内置的资源,不需要再次加载
			if (!atlasPath.endWith("/unity_builtin_extra"))
			{
				atlasPath = atlasPath.removeStartString(P_GAME_RESOURCES_PATH);
				mOriginAtlasPtr = mAtlasManager.getAtlas(atlasPath, false);
				if (mOriginAtlasPtr == null || !mOriginAtlasPtr.isValid())
				{
					logWarning("无法加载初始化的图集:" + atlasPath + ", window:" + mName + ", layout:" + mLayout.getName() +
						",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
				}
			}
			mAtlasPtr = mOriginAtlasPtr;
			mNumberStyle = mRenderer.sprite.name.rangeToLast('_');
			refreshSpriteList();
		}
	}
	public override void destroy()
	{
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mRenderer.sprite = mOriginSprite;
		setAlpha(1.0f, false);
		mAtlasManager.unloadAtlas(ref mOriginAtlasPtr);
		mAtlasPtr = null;
		base.destroy();
	}
	public override void notifyAnchorApply()
	{
		base.notifyAnchorApply();
		// 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
		mRenderer.setInterval((int)(mRenderer.getInterval() * getScreenScale(ASPECT_BASE.AUTO).x));
	}
	public override void cloneFrom(myUGUIObject obj)
	{
		base.cloneFrom(obj);
		var source = obj as myUGUIImageNumber;
		mRenderer.setInterval(source.mRenderer.getInterval());
		mNumberStyle = source.mNumberStyle;
		mRenderer.setNumber(source.getNumber());
		mRenderer.setSpriteList(source.mRenderer.getSpriteList());
		mRenderer.setDocking(source.getDocking());
	}
	public void setAtlas(AtlasRef atlas)
	{
		mAtlasPtr = atlas;
		refreshSpriteList();
	}
	public void setNumberStyle(string style)
	{
		mNumberStyle = style;
		refreshSpriteList();
	}
	public void setInterval(int interval)					{ mRenderer.setInterval(interval); }
	public void setDocking(DOCKING_POSITION dock)			{ mRenderer.setDocking(dock); }
	public void setNumber(int num, int limitLen = 0)		{ mRenderer.setNumber(IToS(num, limitLen)); }
	public void setNumber(long num, int limitLen = 0)		{ mRenderer.setNumber(LToS(num, limitLen)); }
	public void clearNumber()								{ mRenderer.clearNumber(); }
	public int getContentWidth()							{ return mRenderer.getContentWidth(); }
	public string getNumber()								{ return mRenderer.getNumber(); }
	public int getInterval()								{ return mRenderer.getInterval(); }
	public string getNumberStyle()							{ return mNumberStyle; }
	public DOCKING_POSITION getDocking()					{ return mRenderer.getDocking(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshSpriteList()
	{
		using var a = new DicScope<char, Sprite>(out var spriteList);
		for (int i = 0; i < 10; ++i)
		{
			spriteList.add((char)('0' + i), mAtlasPtr.getSprite(mNumberStyle + "_" + IToS(i)));
		}
		mRenderer.sprite = spriteList.firstValue();
		mRenderer.setSpriteList(spriteList);
	}
}