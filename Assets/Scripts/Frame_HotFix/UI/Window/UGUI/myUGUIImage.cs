using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static FrameDefine;

// 对UGUI的Image的封装,普通版本,提供替换图片的功能,UGUI的静态图片不支持递归变化透明度
public class myUGUIImage : myUGUIImageSimple
{
	protected List<UGUIAtlasPtr> mOriginAtlasPtr = new();	// 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected List<UGUIAtlasPtr> mAtlasPtrList = new();		// 图片图集,为了支持多图集,也就是原本属于同一个图集里的图片,但是由于一个图集装不下而不得不拆分到多个图集中,所以使用List类型存储
	protected Sprite mOriginSprite;							// 备份加载物体时原始的精灵图片
	protected string mOriginSpriteName;						// 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mOriginAtlasInResources;					// OriginAtlas是否是从Resources中加载的
	public override void init()
	{
		base.init();
		mOriginSprite = mImage.sprite;
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
			if (!atlasPath.endWith("/unity_builtin_extra"))
			{
				mOriginAtlasInResources = mLayout.isInResources();
				if (mOriginAtlasInResources)
				{
					atlasPath = atlasPath.removeStartString(P_RESOURCES_PATH);
					originAtlas = mTPSpriteManager.getAtlasInResources(atlasPath, false, true);
				}
				else
				{
					atlasPath = atlasPath.removeStartString(P_GAME_RESOURCES_PATH);
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
			mOriginAtlasPtr.addNotNull(originAtlas);
			string singleFileName = getFileNameNoSuffixNoDir(atlasPath);
			// 一般不会拆分到太多图集,所以只是简单判断0,1,2,3即可
			if (singleFileName != null && 
				(singleFileName.EndsWith("_0") || singleFileName.EndsWith("_1") || singleFileName.EndsWith("_2") || singleFileName.EndsWith("_3")))
			{
				mOriginAtlasPtr.Clear();
				// 找到所有序列图集
				string suffix = getFileSuffix(atlasPath);
				string cleanName = singleFileName.rangeToLast('_');
				string filePath = getFilePath(atlasPath, true);
				int index = 0;
				while (true)
				{
					UGUIAtlasPtr atlas;
					string pathInGameRes = strcat(filePath, cleanName, "_", IToS(index++), suffix);
					if (mOriginAtlasInResources)
					{
						atlas = mTPSpriteManager.getAtlasInResources(pathInGameRes, false, true);
					}
					else
					{
						atlas = mTPSpriteManager.getAtlas(pathInGameRes, false, true);
					}
					if (atlas == null || !atlas.isValid())
					{
						break;
					}
					mOriginAtlasPtr.Add(atlas);
				}
			}
			mAtlasPtrList.AddRange(mOriginAtlasPtr);
		}
		mOriginSpriteName = getSpriteName();
	}
	public override void destroy()
	{
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mImage.sprite = mOriginSprite;
		if (mOriginAtlasInResources)
		{
			mTPSpriteManager.unloadAtlasInResourcecs(mOriginAtlasPtr);
		}
		else
		{
			mTPSpriteManager.unloadAtlas(mOriginAtlasPtr);
		}
		base.destroy();
	}
	public UGUIAtlasPtr getAtlas() { return mAtlasPtrList.getSafe(0); }
	public virtual void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
	{
		if (mImage == null)
		{
			return;
		}
		mAtlasPtrList.Clear();
		mAtlasPtrList.addNotNull(atlas);
		setSprite(clearSprite ? null : atlas?.getSprite(getSpriteName()));
	}
	public void setSpriteName(string spriteName, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null)
		{
			return;
		}
		if (spriteName.isEmpty())
		{
			mImage.sprite = null;
			return;
		}
		setSprite(getSpriteInAtlas(spriteName), useSpriteSize, sizeScale);
	}
	// 设置图片,需要确保图片在当前图集内
	public void setSprite(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null || mImage.sprite == sprite)
		{
			return;
		}
		if (sprite != null && !hasAtlas(sprite.texture))
		{
			logError("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
		}
		setSpriteOnly(sprite, useSpriteSize, sizeScale);
	}
	// 设置可自动本地化的文本内容,collection是myUGUIText对象所属的布局对象或者布局结构体对象,如LayoutScript或WindowObjectUGUI
	public void setLocalizationImage(string chineseSpriteName, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, chineseSpriteName);
		collection.addLocalizationObject(this);
	}
	public string getOriginSpriteName() { return mOriginSpriteName; }
	public void setOriginSpriteName(string textureName) { mOriginSpriteName = textureName; }
	// 自动计算图片的原始名称,也就是不带后缀的名称,后缀默认以_分隔
	public void generateOriginSpriteName(char key = '_')
	{
		if (!mOriginSpriteName.Contains(key))
		{
			logError("texture name is not valid!can not generate origin texture name, texture name : " + mOriginSpriteName);
			return;
		}
		mOriginSpriteName = mOriginSpriteName.rangeToLastInclude(key);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected Sprite getSpriteInAtlas(string spriteName)
	{
		foreach (UGUIAtlasPtr atlas in mAtlasPtrList)
		{
			Sprite sprite = atlas?.getSprite(spriteName);
			if (sprite != null)
			{
				return sprite;
			}
		}
		return null;
	}
	protected bool hasAtlas(Texture2D tex2D)
	{
		foreach (UGUIAtlasPtr atlas in mAtlasPtrList)
		{
			if (atlas != null && tex2D == atlas.getTexture())
			{
				return true;
			}
		}
		return false;
	}
}