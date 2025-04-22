using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 对SpriteRenderer的封装
public class myUISprite : myUIObject, IShaderWindow
{
	protected SpriteRenderer mSpriteRenderer;   // 图片组件
	protected WindowShader mWindowShader;       // 图片所使用的shader类,用于动态设置shader参数
	protected UGUIAtlasPtr mOriginAtlasPtr;     // 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected UGUIAtlasPtr mAtlasPtr;           // 图片图集
	protected Material mOriginMaterial;         // 初始的材质,用于重置时恢复材质
	protected Sprite mOriginSprite;             // 备份加载物体时原始的精灵图片
	protected string mOriginMaterialPath;       // 原始材质的文件路径
	protected string mOriginSpriteName;         // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mIsNewMaterial;              // 当前的材质是否是新建的材质对象
	protected bool mOriginAtlasInResources;		// OriginAtlas是否是从Resources中加载的
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		mSpriteRenderer = getOrAddUnityComponent<SpriteRenderer>();
		mOriginSprite = mSpriteRenderer.sprite;
		mOriginMaterial = mSpriteRenderer.sharedMaterial;
		mOriginSpriteName = getSpriteName();
		// mOriginSprite无法简单使用?.来判断是否为空,需要显式判断
		Texture2D curTexture = mOriginSprite != null ? mOriginSprite.texture : null;
		// 获取初始的精灵所在图集
		if (curTexture != null)
		{
			string atlasPath;
			if (mObject.TryGetComponent<ImageAtlasPath>(out var comImageAtlasPath))
			{
				if (mOriginAtlasInResources)
				{
					atlasPath = comImageAtlasPath.mAtlasPath.removeStartString(P_RESOURCES_PATH);
				}
				else
				{
					atlasPath = comImageAtlasPath.mAtlasPath.removeStartString(P_GAME_RESOURCES_PATH);
				}
			}
			else
			{
				atlasPath = R_ATLAS_GAME_ATLAS_PATH + curTexture.name + ".png";
			}
			mOriginAtlasInResources = mLayout.isInResources();
			if (mOriginAtlasInResources)
			{
				mOriginAtlasPtr = mTPSpriteManager.getAtlasInResources(atlasPath, false, true);
			}
			else
			{
				mOriginAtlasPtr = mTPSpriteManager.getAtlas(atlasPath, false, true);
			}
			if (mOriginAtlasPtr == null || !mOriginAtlasPtr.isValid())
			{
				logError("无法加载初始化的图集:" + atlasPath + ",Texture:" + curTexture.name + ",GameObject:" + getGameObjectPath(mObject) +
					",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (comImageAtlasPath != null ? comImageAtlasPath.mAtlasPath : EMPTY));
			}
			if (mOriginAtlasPtr != null && mOriginAtlasPtr.isValid() && mOriginAtlasPtr.getTexture() != curTexture)
			{
				logError("设置的图集与加载出的图集不一致!可能未添加ImageAtlasPath组件,或者ImageAtlasPath组件中记录的路径错误,或者是在当前物体在重复使用过程中销毁了原始图集\n图集名:" + mOriginSprite.name + ", 记录的图集路径:" + atlasPath);
			}
			mAtlasPtr = mOriginAtlasPtr;
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
				logError("没有找到MaterialPath组件,name:" + getName());
			}
			mOriginMaterialPath = mOriginMaterialPath.removeStartString(P_GAME_RESOURCES_PATH);
			if (!mOriginMaterialPath.endWith("/unity_builtin_extra"))
			{
				if (!mOriginMaterialPath.Contains('.'))
				{
					logError("材质文件需要带后缀:" + mOriginMaterialPath + ",GameObject:" + getName() + ",parent:" + getParent()?.getName());
				}
				setMaterialName(mOriginMaterialPath, !mShaderManager.isSingleShader(mOriginMaterial.shader.name));
			}
		}
	}
	public override void destroy()
	{
		// 卸载创建出的材质
		if (mIsNewMaterial)
		{
			if (!isEditor())
			{
				destroyUnityObject(mSpriteRenderer.sharedMaterial);
			}
		}
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mSpriteRenderer.sprite = mOriginSprite;
		setMaterial(mOriginMaterial);
		setAlpha(1.0f);
		if (mOriginAtlasInResources)
		{
			mTPSpriteManager.unloadAtlasInResourcecs(ref mOriginAtlasPtr);
		}
		else
		{
			mTPSpriteManager.unloadAtlas(ref mOriginAtlasPtr);
		}
		mAtlasPtr = null;
		base.destroy();
	}
	// 是否剔除渲染
	public void cull(bool isCull)
	{
		setAlpha(isCull ? 0.0f : 1.0f);
	}
	public override bool isCulled() { return isFloatZero(getAlpha()); }
	public override bool canUpdate() { return !isCulled() && base.canUpdate(); }
	public override bool canGenerateDepth() { return !isCulled(); }
	public void setWindowShader(WindowShader shader)
	{
		mWindowShader = shader;
		// 因为shader参数的需要在update中更新,所以需要启用窗口的更新
		mNeedUpdate = true;
	}
	public WindowShader getWindowShader() { return mWindowShader; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mWindowShader != null && !isCulled() && mSpriteRenderer.sharedMaterial != null)
		{
			mWindowShader.applyShader(mSpriteRenderer.sharedMaterial);
		}
	}
	// 谨慎使用设置RendererQueue,尤其是操作material而非sharedMaterial
	// 操作material会复制出一个材质实例,从而导致drawcall增加
	public void setRenderQueue(int renderQueue, bool shareMaterial = false) 
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		if (shareMaterial)
		{
			if (mSpriteRenderer.sharedMaterial == null)
			{
				return;
			}
			mSpriteRenderer.sharedMaterial.renderQueue = renderQueue;
		}
		else
		{
			if (mSpriteRenderer.material == null)
			{
				return;
			}
			mSpriteRenderer.material.renderQueue = renderQueue;
		}
	}
	public int getRenderQueue()
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sharedMaterial == null)
		{
			return 0;
		}
		return mSpriteRenderer.sharedMaterial.renderQueue;
	}
	public override Vector2 getWindowSize(bool transformed = false)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == null)
		{
			return Vector2.zero;
		}
		if (transformed)
		{
			return getSpriteSize() * getScale();
		}
		else
		{
			return getSpriteSize();
		}
	}
	public UGUIAtlasPtr getAtlas() { return mAtlasPtr; }
	public virtual void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
	{
		if (mSpriteRenderer == null)
		{
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
		setSprite(mAtlasPtr.getSprite(spriteName));
	}
	// 设置图片,需要确保图片在当前图集内
	public void setSprite(Sprite sprite)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == sprite)
		{
			return;
		}
		if (sprite != null && mAtlasPtr != null && sprite.texture != mAtlasPtr.getTexture())
		{
			logWarning("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
		}
		setSpriteOnly(sprite);
	}
	// 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
	// 并且需要自己保证设置不同图集的图片以后不会有什么问题
	public void setSpriteOnly(Sprite sprite)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == sprite)
		{
			return;
		}
		if (sprite != null && !isFloatEqual(sprite.pixelsPerUnit, 1.0f))
		{
			logError("sprite的pixelsPerUnit需要为1, name:" + sprite.texture.name);
		}
		mSpriteRenderer.sprite = sprite;
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
	public void setOrderInLayer(int order)			{ mSpriteRenderer.sortingOrder = order; }
	public int getOrderInLayer()					{ return mSpriteRenderer.sortingOrder; }
	public void setRendererPrority(int priority)	{ mSpriteRenderer.rendererPriority = priority; }
	public int getRendererPrority()					{ return mSpriteRenderer.rendererPriority; }
	public string getOriginMaterialPath()			{ return mOriginMaterialPath; }
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
			mResourceManager.loadGameResourceAsync(materialPath, (Material mat) =>
			{
				if (mSpriteRenderer == null)
				{
					return;
				}
				if (mIsNewMaterial)
				{
					// 当需要复制一个新的材质时,刚加载出来的材质实际上就不会再用到了
					// 只有当下次还加载相同的材质时才会直接返回已加载的材质
					// 如果要卸载最开始加载出来的材质,只能通过卸载整个文件夹的资源来卸载
					Material newMat = new(mat);
					newMat.name = getFileNameNoSuffixNoDir(materialPath) + "_" + IToS(mID);
					setMaterial(newMat);
				}
				else
				{
					setMaterial(mat);
				}
			});
		}
		// 同步加载
		else
		{
			var loadedMaterial = mResourceManager.loadGameResource<Material>(materialPath);
			if (mIsNewMaterial)
			{
				Material mat = new(loadedMaterial);
				mat.name = getFileNameNoSuffixNoDir(materialPath) + "_" + IToS(mID);
				setMaterial(mat);
			}
			else
			{
				setMaterial(loadedMaterial);
			}
		}
	}
	public void setMaterial(Material mat)  { mSpriteRenderer.material = mat; }
	public void setShader(Shader shader, bool force)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sharedMaterial == null)
		{
			return;
		}
		if (force)
		{
			mSpriteRenderer.sharedMaterial.shader = null;
			mSpriteRenderer.sharedMaterial.shader = shader;
		}
	}
	public string getSpriteName()
	{
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == null)
		{
			return null;
		}
		return mSpriteRenderer.sprite.name;
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
		if (mSpriteRenderer == null || mSpriteRenderer.sharedMaterial == null)
		{
			return null;
		}
		return mSpriteRenderer.sharedMaterial.name;
	}
	public string getShaderName()
	{
		if (mSpriteRenderer.sharedMaterial == null || mSpriteRenderer.sharedMaterial.shader == null)
		{
			return null;
		}
		return mSpriteRenderer.sharedMaterial.shader.name;
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		if (mSpriteRenderer == null)
		{
			return;
		}
		Color color = mSpriteRenderer.color;
		color.a = alpha;
		mSpriteRenderer.color = color;
	}
	public override float getAlpha() { return mSpriteRenderer.color.a; }
	public override void setColor(Color color)
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
	public override Color getColor() { return mSpriteRenderer.color; }
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
	protected override void ensureColliderSize()
	{
		// 确保RectTransform和BoxCollider一样大
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == null)
		{
			return;
		}
		mCOMWindowCollider?.setColliderSize(mSpriteRenderer.sprite.rect.size);
	}
}