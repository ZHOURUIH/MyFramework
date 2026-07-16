using UnityEngine;
using static FrameBaseUtility;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static MathUtility;
using static FrameDefine;

// 对SpriteRenderer的封装,在3D空间中使用的
public class mySpriteRenderer : ClassObject
{
	protected GameObject mObject;					// 节点
	protected SpriteRenderer mSpriteRenderer;		// 图片组件
	protected AtlasRef mOriginAtlasPtr;				// 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected AtlasRef mAtlasPtr;					// 图片图集
	protected Material mOriginMaterial;				// 初始的材质,用于重置时恢复材质
	protected ResourceRef<Material> mCurMaterial;   // 当前引用的材质,用于卸载
	protected Sprite mOriginSprite;					// 备份加载物体时原始的精灵图片
	protected string mName;							// 节点名字的缓存
	protected string mOriginMaterialPath;			// 原始材质的文件路径
	protected string mOriginSpriteName;             // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected string mSpriteName;                   // 当前图片的名字,避免GC
	protected string mMaterialName;                 // 当前材质的名字,避免GC
	protected string mWillSetSpriteName;			// 存储图集还未初始化完成时就要设置的图片名字,用于在初始化完成后设置正确的图片显示
	protected bool mSpriteNameDirty;                // 图片名字是否需要更新
	protected bool mMaterialNameDirty;              // 材质名字是否需要更新
	protected bool mIsNewMaterial;                  // 当前的材质是否是新建的材质对象
	protected bool mInitDone;						// 图集是否已经初始化完毕
	public void init(SpriteRenderer renderer)
	{
		mSpriteRenderer = renderer;
		mObject = mSpriteRenderer.gameObject;
        mName = mObject.name;
        mOriginSprite = mSpriteRenderer.sprite;
		mOriginMaterial = mSpriteRenderer.sharedMaterial;
		mSpriteName = mOriginSprite != null ? mOriginSprite.name : null;
		mOriginSpriteName = mSpriteName;
		mMaterialName = mOriginMaterial != null ? mOriginMaterial.name : null;
		// 获取初始的精灵所在图集
		if (mOriginSprite != null)
		{
			if (!mObject.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
			{
				logError("需要切换图片的SpriteRenderer组件上找不到ImageAtlasPath组件, GameObject:" + getGameObjectPath(mObject));
				return;
			}
			string atlasPath = imageAtlasPath.mAtlasPath;
			if (atlasPath.isEmpty())
			{
				logError("ImageAtlasPath中记录的路径为空,GameObject:" + getGameObjectPath(mObject));
			}
			atlasPath = atlasPath.removeStartString(P_GAME_RESOURCES_PATH);
            mAtlasManager.getAtlasAsyncSafe(this, atlasPath, (AtlasRef ptr) =>
            {
                mOriginAtlasPtr = ptr;
                if (mOriginAtlasPtr == null || !mOriginAtlasPtr.isValid())
                {
                    logError("无法加载初始化的图集:" + atlasPath + ",GameObject:" + getGameObjectPath(mObject) +
                        ",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
                }
                mAtlasPtr = mOriginAtlasPtr;
				mInitDone = true;
				if (!mWillSetSpriteName.isEmpty())
				{
					setSpriteName(mWillSetSpriteName);
				}
			}, false);
		}
		string materialName = getMaterialName().removeAll(" (Instance)");
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if (!materialName.isEmpty() &&
			materialName != DEFAULT_MATERIAL &&
			materialName != SPRITE_DEFAULT_MATERIAL)
		{
			if (mOriginMaterial != null && mObject.TryGetComponent<MaterialPath>(out var comMaterialPath))
			{
				mOriginMaterialPath = comMaterialPath.mMaterialPath;
			}
			if (mOriginMaterialPath.isEmpty())
			{
				logError("没有找到MaterialPath组件,name:" + mName);
			}
			mOriginMaterialPath = mOriginMaterialPath.removeStartString(P_GAME_RESOURCES_PATH);
			if (!mOriginMaterialPath.endWith("/unity_builtin_extra"))
			{
				if (!mOriginMaterialPath.Contains('.'))
				{
					logError("材质文件需要带后缀:" + mOriginMaterialPath + ",GameObject:" + mName);
				}
				setMaterialName(mOriginMaterialPath, !mShaderManager.isSingleShader(mOriginMaterial.shader.name), true);
			}
		}
	}
    public override void resetProperty()
    {
        base.resetProperty();
		mObject = null;
        mSpriteRenderer = null;
        mOriginAtlasPtr = null;
        mAtlasPtr = null;
        mOriginMaterial = null;
        mCurMaterial = null;
        mOriginSprite = null;
        mName = null;
        mOriginMaterialPath = null;
        mOriginSpriteName = null;
        mSpriteName = null;
        mMaterialName = null;
		mWillSetSpriteName = null;
		mSpriteNameDirty = false;
        mMaterialNameDirty = false;
        mIsNewMaterial = false;
		mInitDone = false;
	}
	public override void destroy()
	{
		base.destroy();
		// 卸载创建出的材质
		if (mIsNewMaterial && !isEditor())
		{
			destroyUnityObject(mSpriteRenderer.sharedMaterial);
		}
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mSpriteRenderer.sprite = mOriginSprite;
		setMaterial(mOriginMaterial);
		setAlpha(1.0f);
		mAtlasPtr = null;
		if (!mInitDone)
		{
			logWarning("图集还未初始化完毕,无法正常卸载图集,sprite:" + mOriginSpriteName);
		}
		mAtlasManager.unloadAtlas(ref mOriginAtlasPtr);
		mResourceManager.unload(ref mCurMaterial);
	}
	// 是否剔除渲染
	public void cull(bool isCull)
	{
		setAlpha(isCull ? 0.0f : 1.0f);
	}
	public Vector2 getSize(bool transformed = false)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == null)
		{
			return Vector2.zero;
		}
		Vector2 size = Vector2.zero;
		if (mSpriteRenderer.drawMode == SpriteDrawMode.Simple)
		{
			size = mSpriteRenderer.sprite.rect.size;
		}
		else if (mSpriteRenderer.drawMode == SpriteDrawMode.Sliced || mSpriteRenderer.drawMode == SpriteDrawMode.Tiled)
		{
            size = mSpriteRenderer.size;
        }
		if (transformed)
		{
			return size * mObject.transform.localScale;
		}
		else
		{
			return size;
		}
	}
	public AtlasRef getAtlas() { return mAtlasPtr; }
	public virtual void setAtlas(AtlasRef atlas, bool clearSprite = false, bool force = false)
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		if (!mInitDone)
		{
			logError("图集未初始化完成,还不能去设置图集,atlas name:" + atlas?.getAtlasSingleName());
			return;
		}
		mAtlasPtr = atlas;
		setSprite(clearSprite ? null : mAtlasPtr?.getSprite(getSpriteName()));
	}
	public void setSpriteName(string spriteName)
	{
		if (mSpriteRenderer == null || mAtlasPtr == null || !mAtlasPtr.isValid())
		{
			return;
		}
		if (spriteName.isEmpty())
		{
			mSpriteRenderer.sprite = null;
			return;
		}
		// 还未初始化完成,就记录下要设置的参数,等到初始化完成再恢复参数设置
		if (!mInitDone)
		{
			mWillSetSpriteName = spriteName;
			return;
		}
		setSprite(mAtlasPtr.getSprite(spriteName));
	}
	// 设置图片,需要确保图片在当前图集内
	public void setSprite(Sprite sprite)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == sprite)
		{
			return;
		}
		if (!mInitDone)
		{
			logError("图集还未初始化完成,还不能去设置Sprite对象");
			return;
		}
		if (sprite != null && mAtlasPtr != null && !mAtlasPtr.hasSprite(sprite))
		{
			logWarning("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly, sprite:" + sprite.name + 
					   ", atlas:" + mAtlasPtr.getAtlasSingleName() + ", token:" + mAtlasPtr.getToken() + ", hash:" + mAtlasPtr.GetHashCode() +
                       ", GameObject:" + getGameObjectPath(mObject) + ", GameObject hash:" + GetHashCode());
		}
		mSpriteRenderer.sprite = sprite;
		mSpriteNameDirty = true;
	}
	// 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
	// 并且需要自己保证设置不同图集的图片以后不会有什么问题
	public void setSpriteOnly(Sprite sprite)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == sprite)
		{
			return;
		}
		if (sprite != null && !isFloatEqual(sprite.pixelsPerUnit, 1.0f) && mObject.transform.localScale.x <= 1.0f)
		{
			logWarning("sprite的pixelsPerUnit为1,且Transform缩放为1, 会使最终渲染结果缩小100倍,如果需要显示正常,请调整pixelsPerUnit或者Transform缩放, sprite:" + 
					   sprite.name + ", GameObject:" + getGameObjectPath(mObject));
		}
		mSpriteRenderer.sprite = sprite;
		mSpriteNameDirty = true;
	}
	public Vector2 getSpriteSize()
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == null)
		{
			return Vector2.zero;
		}
		return mSpriteRenderer.sprite.rect.size;
	}
	public SpriteRenderer getSpriteRenderer()		{ return mSpriteRenderer; }
	public Sprite getSprite()						{ return mSpriteRenderer.sprite; }
	public int getOrderInLayer()					{ return mSpriteRenderer.sortingOrder; }
	public int getRendererPriority()				{ return mSpriteRenderer.rendererPriority; }
	public string getOriginMaterialPath()			{ return mOriginMaterialPath; }
	public void setOrderInLayer(int order)			
	{
		if (mSpriteRenderer.sortingOrder != order)
		{
			mSpriteRenderer.sortingOrder = order;
		}
	}
	public void setRendererPriority(int priority)	
	{
		if (mSpriteRenderer.rendererPriority != priority)
		{
			mSpriteRenderer.rendererPriority = priority;
		}
	}
	// materialPath是GameResources下的相对路径,带后缀
	public void setMaterialName(string materialPath, bool newMaterial, bool loadAsync = false)
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		mIsNewMaterial = newMaterial;
		// 异步加载
		if (loadAsync)
		{
			mResourceManager.loadGameResourceAsync<Material>(materialPath, (mat) =>
			{
				mCurMaterial = mat;
				if (mSpriteRenderer == null)
				{
					return;
				}
				if (mIsNewMaterial)
				{
					// 当需要复制一个新的材质时,刚加载出来的材质实际上就不会再用到了
					// 只有当下次还加载相同的材质时才会直接返回已加载的材质
					// 如果要卸载最开始加载出来的材质,只能通过卸载整个文件夹的资源来卸载
					Material newMat = new(mCurMaterial.get());
					newMat.name = getFileNameNoSuffixNoDir(materialPath);
					setMaterial(newMat);
				}
				else
				{
					setMaterial(mCurMaterial.get());
				}
			});
		}
		// 同步加载
		else
		{
			mCurMaterial = mResourceManager.loadGameResource<Material>(materialPath);
			if (mIsNewMaterial)
			{
				Material mat = new(mCurMaterial.get());
				mat.name = getFileNameNoSuffixNoDir(materialPath);
				setMaterial(mat);
			}
			else
			{
				setMaterial(mCurMaterial.get());
			}
		}
	}
	public void setMaterial(Material mat) 
	{
		mSpriteRenderer.material = mat;
		mMaterialNameDirty = true;
	}
	public string getSpriteName()
	{
		if (mSpriteNameDirty)
		{
			mSpriteNameDirty = false;
			mSpriteName = mSpriteRenderer.sprite != null ? mSpriteRenderer.sprite.name : null;
		}
		return mSpriteName; 
	}
	public Material getMaterial()
	{
		if (mSpriteRenderer == null)
		{
			return null;
		}
		return mSpriteRenderer.sharedMaterial;
	}
	public string getMaterialName() 
	{
		if (mMaterialNameDirty)
		{
			mMaterialNameDirty = false;
			mMaterialName = mSpriteRenderer.material != null ? mSpriteRenderer.material.name : null;
		}
		return mMaterialName; 
	}
	public void setAlpha(float alpha)
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		Color color = mSpriteRenderer.color;
		color.a = alpha;
		mSpriteRenderer.color = color;
	}
	public float getAlpha() { return mSpriteRenderer.color.a; }
	public void setColor(Color color)
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		mSpriteRenderer.color = color;
	}
	public void setColor(Vector3 color)
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		mSpriteRenderer.color = new(color.x, color.y, color.z);
	}
	public AtlasRef getOriginAtlas() { return mOriginAtlasPtr; }
	public bool isOriginAtlas(AtlasRef atlas) { return mOriginAtlasPtr == atlas; }
	public Color getColor() { return mSpriteRenderer.color; }
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
}