using System;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static FrameDefine;

// 对UGUI的Image的封装,普通版本,提供替换图片的功能,UGUI的静态图片不支持递归变化透明度
public class myUGUIImage : myUGUIImageSimple, IUGUIImage
{
	protected AtlasRef mOriginAtlasPtr;			// 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected AtlasRef mAtlasPtr;				// 当前正在使用的图集
	protected Sprite mOriginSprite;             // 备份加载物体时原始的精灵图片,此图片的卸载是在prefab卸载后,没有任何地方对其有引用,在Resources.UnloadUnusedAssets中被卸载
	protected string mOriginSpriteName;         // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected string mWillSetSpriteName;		// 存储图集还未初始化完成时就要设置的图片名字,用于在初始化完成后设置正确的图片显示
	protected bool mWillSetUseSpriteSize;		// 存储图集还未初始化完成时就要设置的参数,用于在初始化完成后设置正确的图片显示
	protected float mWillSetSizeScale;			// 存储图集还未初始化完成时就要设置的参数,用于在初始化完成后设置正确的图片显示
	protected bool mInitDone;					// 异步初始化是否完成
	public override void init()
	{
		base.init();
		mOriginSprite = mImage.sprite;
		// 获取初始的精灵所在图集
		if (mOriginSprite != null)
		{
			if (!mObject.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
			{
				logError("找不到图集,请添加ImageAtlasPath组件, GameObject:" + getGameObjectPath());
			}
			string atlasPath = imageAtlasPath.mAtlasPath;
			if (atlasPath.isEmpty())
			{
				logError("ImageAtlasPath中记录的路径为空,GameObject:" + getGameObjectPath());
			}
			// unity_builtin_extra是unity内置的资源,不需要再次加载
			if (!atlasPath.endWith("/unity_builtin_extra"))
			{
				atlasPath = atlasPath.removeStart(P_GAME_RESOURCES_PATH);
				// webgl中还不支持同步加载,但是异步又可能会出现很多执行时序问题.所以分开写,能同步则同步,不能才异步
                mAtlasManager.getAtlasAsyncSafe(this, atlasPath, (AtlasRef atlas) =>
                {
                    mOriginAtlasPtr = atlas;
                    mAtlasPtr = mOriginAtlasPtr;
                    if (mOriginAtlasPtr == null || !mOriginAtlasPtr.isValid())
                    {
                        logWarning("无法加载初始化的图集:" + atlasPath + ", GameObject:" + getGameObjectPath() +
                            ",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
                    }
                    mInitDone = true;
                    onInitAsyncDone();
					if (!mWillSetSpriteName.isEmpty())
					{
						setSpriteNamePro(mWillSetSpriteName, mWillSetUseSpriteSize, mWillSetSizeScale);
					}
                }, false);
            }
			else
			{
				logError("需要切换图片的节点上不要使用引擎内置的图片, GameObject:" + getGameObjectPath());
			}
		}
		mOriginSpriteName = getSpriteName();
	}
	public override void destroy()
	{
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mImage.sprite = mOriginSprite;
		if (!mInitDone)
		{
			logWarning("图集未初始化完成,无法卸载此图集,sprite:" + mOriginSpriteName + ",name:" + mName);
		}
		mAtlasManager.unloadAtlas(ref mOriginAtlasPtr);
		base.destroy();
	}
	public void getAtlasAsync(Action<AtlasRef> callback) 
	{
		if (mInitDone)
		{
			callback?.Invoke(mAtlasPtr);
			return;
		}
		mWaitingManager.wait(() => { return mInitDone; }, ()=> { callback?.Invoke(mAtlasPtr); });
	}
	public AtlasRef getAtlas() { return mAtlasPtr; }
	public virtual void setAtlas(AtlasRef atlas, bool clearSprite = false, bool force = false)
	{
		if (mImage == null)
		{
			return;
		}
		if (!mInitDone)
		{
			logError("图集未初始化完成,还不能去设置图集,atlas name:" + atlas?.getAtlasSingleName() + ",name:" + mName);
			return;
		}
		mAtlasPtr = atlas;
		setSprite(clearSprite ? null : atlas?.getSprite(getSpriteName()));
	}
	public void setSpriteName(string spriteName)
	{
		setSpriteNamePro(spriteName, false, 1.0f);
	}
	public void setSpriteNamePro(string spriteName, bool useSpriteSize, float sizeScale)
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
		// 还未初始化完成,就记录下要设置的参数,等到初始化完成再恢复参数设置
		if (!mInitDone)
		{
			mWillSetSpriteName = spriteName;
			mWillSetUseSpriteSize = useSpriteSize;
			mWillSetSizeScale = sizeScale;
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
		if (!mInitDone)
		{
			logError("图集还未初始化完成,还不能去设置Sprite对象");
			return;
		}
		if (sprite != null && !mAtlasPtr.hasSprite(sprite))
		{
			logError("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
		}
		setSpriteOnly(sprite, useSpriteSize, sizeScale);
	}
	// 设置可自动本地化的文本内容,collection是myUGUIImage对象所属的布局对象或者布局结构体对象,如LayoutScript或WindowObjectBase
	public void setLocalizationImage(string chineseSpriteName, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, chineseSpriteName);
		collection.addLocalizationObject(this);
	}
	public bool isOriginAtlas(AtlasRef atlas) { return mOriginAtlasPtr == atlas; }
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
		if (!mInitDone)
		{
			logError("图集还未初始化完成,无法获取其中的图片");
			return null;
		}
		Sprite sprite = mAtlasPtr?.getSprite(spriteName);
		if (sprite != null)
		{
			return sprite;
		}
		return null;
	}
    protected virtual void onInitAsyncDone() { }
}