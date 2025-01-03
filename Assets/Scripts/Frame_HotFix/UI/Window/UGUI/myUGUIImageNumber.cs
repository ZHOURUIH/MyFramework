using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static FrameDefine;
using static FrameBase;

// 可显示数字的窗口,只支持整数,且每个数字图片的大小必须一样,不能显示小数,负数
public class myUGUIImageNumber : myUGUIObject
{
	protected ImageNumber mImageNumber;						// 渲染组件
	protected UGUIAtlasPtr mOriginAtlasPtr = new();			// 初始的图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected UGUIAtlasPtr mAtlasPtr = new();				// 图片图集
	protected Sprite mOriginSprite;                         // 备份加载物体时原始的精灵图片
	protected string mOriginTextureName;                    // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mOriginAtlasInResources;                 // OriginAtlas是否是从Resources中加载的
	protected string mNumberStyle;							// 数字图集名
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		if (!mObject.TryGetComponent(out mImageNumber))
		{
			mImageNumber = mObject.AddComponent<ImageNumber>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}

		mOriginSprite = mImageNumber.sprite;
		// mOriginSprite无法简单使用?.来判断是否为空,需要显式判断
		Texture2D curTexture = mOriginSprite != null ? mOriginSprite.texture : null;
		// 获取初始的精灵所在图集
		if (curTexture != null)
		{
			if (!mObject.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
			{
				logError("找不到图集,请添加ImageAtlasPath组件, window:" + mName + ", layout:" + mLayout.getName());
			}
			UGUIAtlasPtr originAtlas = null;
			string atlasPath = imageAtlasPath.mAtlasPath;
			// unity_builtin_extra是unity内置的资源,不需要再次加载
			if (!atlasPath.EndsWith("/unity_builtin_extra"))
			{
				mOriginAtlasInResources = mLayout.isInResources();
				if (mOriginAtlasInResources)
				{
					removeStartString(ref atlasPath, P_RESOURCES_PATH);
					originAtlas = mTPSpriteManager.getAtlasInResources(atlasPath, false, true);
				}
				else
				{
					removeStartString(ref atlasPath, P_GAME_RESOURCES_PATH);
					originAtlas = mTPSpriteManager.getAtlas(atlasPath, false, true);
				}
				if (originAtlas == null || !originAtlas.isValid())
				{
					logWarning("无法加载初始化的图集:" + atlasPath + ", window:" + mName + ", layout:" + mLayout.getName() +
						",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
				}
				if (originAtlas != null && originAtlas.isValid() && originAtlas.getTexture() != curTexture)
				{
					logError("设置的图集与加载出的图集不一致!可能未添加ImageAtlasPath组件,或者ImageAtlasPath组件中记录的路径错误," +
						"或者是在当前物体在重复使用过程中销毁了原始图集\n图片名:" + mOriginSprite.name + ", 记录的图集路径:" + atlasPath + ", 名字:" + mName +
						"layout:" + mLayout.getName());
				}
			}
			mOriginAtlasPtr = originAtlas;
			mAtlasPtr = mOriginAtlasPtr;
			mOriginTextureName = curTexture.name;
		}

		mNumberStyle = mImageNumber.sprite.name.rangeToLast('_');
		refreshSpriteList();
	}
	public override void notifyAnchorApply()
	{
		base.notifyAnchorApply();
		// 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
		mImageNumber.setInterval((int)(mImageNumber.getInterval() * getScreenScale(ASPECT_BASE.AUTO).x));
	}
	public override void cloneFrom(myUIObject obj)
	{
		base.cloneFrom(obj);
		var source = obj as myUGUIImageNumber;
		mImageNumber.setInterval(source.mImageNumber.getInterval());
		mNumberStyle = source.mNumberStyle;
		mImageNumber.setNumber(source.getNumber());
		mImageNumber.setSpriteList(source.mImageNumber.getSpriteList());
		mImageNumber.setDockingPosition(source.getDockingPosition());
	}
	public void setAtlas(UGUIAtlasPtr atlas)
	{
		mAtlasPtr = atlas;
		refreshSpriteList();
	}
	public void setNumberStyle(string style)
	{
		mNumberStyle = style;
		refreshSpriteList();
	}
	public void setInterval(int interval)					{ mImageNumber.setInterval(interval); }
	public void setDockingPosition(DOCKING_POSITION dock)	{ mImageNumber.setDockingPosition(dock); }
	public void setNumber(int num, int limitLen = 0)		{ mImageNumber.setNumber(IToS(num, limitLen)); }
	public void clearNumber()								{ mImageNumber.clearNumber(); }
	public int getContentWidth()							{ return mImageNumber.getContentWidth(); }
	public string getNumber()								{ return mImageNumber.getNumber(); }
	public int getInterval()								{ return mImageNumber.getInterval(); }
	public string getNumberStyle()							{ return mNumberStyle; }
	public DOCKING_POSITION getDockingPosition()			{ return mImageNumber.getDockingPosition(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshSpriteList()
	{
		using var a = new DicScope<char, Sprite>(out var spriteList);
		for (int i = 0; i < 10; ++i)
		{
			spriteList.add((char)('0' + i), mAtlasPtr.getSprite(mNumberStyle + "_" + IToS(i)));
		}
		mImageNumber.sprite = spriteList.firstValue();
		mImageNumber.setSpriteList(spriteList);
	}
}