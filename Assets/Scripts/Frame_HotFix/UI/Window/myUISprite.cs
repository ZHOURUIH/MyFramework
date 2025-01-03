using UnityEngine;
using static UnityUtility;
using static FrameBase;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static FrameEditorUtility;

// 对SpriteRenderer的封装
public class myUISprite : myUIObject, IShaderWindow
{
	protected SpriteRenderer mSpriteRenderer;   // 图片组件
	protected WindowShader mWindowShader;       // 图片所使用的shader类,用于动态设置shader参数
	protected UGUIAtlasPtr mOriginAtlasPtr;     // 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected UGUIAtlasPtr mAtlasPtr;			// 图片图集
	protected Material mOriginMaterial;         // 初始的材质,用于重置时恢复材质
	protected Sprite mOriginSprite;             // 备份加载物体时原始的精灵图片
	protected string mOriginTextureName;        // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mIsNewMaterial;              // 当前的材质是否是新建的材质对象
	protected bool mOriginAtlasInResources;		// OriginAtlas是否是从Resources中加载的
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		mSpriteRenderer = getOrAddUnityComponent<SpriteRenderer>();
		mOriginSprite = mSpriteRenderer.sprite;
		mOriginMaterial = mSpriteRenderer.material;
		// mOriginSprite无法简单使用?.来判断是否为空,需要显式判断
		Texture2D curTexture = mOriginSprite != null ? mOriginSprite.texture : null;
		// 获取初始的精灵所在图集
		if (curTexture != null)
		{
			string atlasPath;
			if (mObject.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
			{
				if (mOriginAtlasInResources)
				{
					atlasPath = removeStartString(imageAtlasPath.mAtlasPath, P_RESOURCES_PATH);
				}
				else
				{
					atlasPath = removeStartString(imageAtlasPath.mAtlasPath, P_GAME_RESOURCES_PATH);
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
					",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
			}
			if (mOriginAtlasPtr != null && mOriginAtlasPtr.isValid() && mOriginAtlasPtr.getTexture() != curTexture)
			{
				logError("设置的图集与加载出的图集不一致!可能未添加ImageAtlasPath组件,或者ImageAtlasPath组件中记录的路径错误,或者是在当前物体在重复使用过程中销毁了原始图集\n图集名:" + mOriginSprite.name + ", 记录的图集路径:" + atlasPath);
			}
			mAtlasPtr = mOriginAtlasPtr;
		}
		mOriginTextureName = getSpriteName();
		string materialName = getMaterialName();
		materialName = removeAll(materialName, " (Instance)");
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if (!materialName.isEmpty() &&
			materialName != DEFAULT_MATERIAL &&
			materialName != SPRITE_DEFAULT_MATERIAL &&
			!mShaderManager.isSingleShader(materialName))
		{
			setMaterialName(materialName, true);
		}
	}
	public override void destroy()
	{
		// 卸载创建出的材质
		if (mIsNewMaterial)
		{
			if (!isEditor())
			{
				destroyUnityObject(mSpriteRenderer.material);
			}
		}
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mSpriteRenderer.sprite = mOriginSprite;
		mSpriteRenderer.material = mOriginMaterial;
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
	public void setRenderQueue(int renderQueue) 
	{
		if (mSpriteRenderer == null || mSpriteRenderer.material == null)
		{
			return;
		}
		mSpriteRenderer.material.renderQueue = renderQueue;
	}
	public int getRenderQueue()
	{
		if (mSpriteRenderer == null || mSpriteRenderer.material == null)
		{
			return 0;
		}
		return mSpriteRenderer.material.renderQueue;
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
		if (isCulled())
		{
			return;
		}
		if (mSpriteRenderer.material != null)
		{
			mWindowShader?.applyShader(mSpriteRenderer.material);
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
	public SpriteRenderer getImage() { return mSpriteRenderer; }
	public Sprite getSprite() { return mSpriteRenderer.sprite; }
	public void setOrderInLayer(int order) { mSpriteRenderer.sortingOrder = order; }
	public int getOrderInLayer() { return mSpriteRenderer.sortingOrder; }
	public void setRendererPrority(int priority) { mSpriteRenderer.rendererPriority = priority; }
	public int getRendererPrority() { return mSpriteRenderer.rendererPriority; }
	public void setMaterialName(string materialName, bool newMaterial, bool loadAsync = false)
	{
		if (mSpriteRenderer == null)
		{
			return;
		}
		mIsNewMaterial = newMaterial;
		string materialPath = R_MATERIAL_PATH + materialName + ".mat";
		// 同步加载
		if (!loadAsync)
		{
			Material mat;
			var loadedMaterial = mResourceManager.loadGameResource<Material>(materialPath);
			if (mIsNewMaterial)
			{
				mat = new(loadedMaterial);
				mat.name = materialName + "_" + IToS(mID);
			}
			else
			{
				mat = loadedMaterial;
			}
			mSpriteRenderer.material = mat;
		}
		// 异步加载
		else
		{
			mResourceManager.loadGameResourceAsync<Material>(materialPath, (Object res, Object[] objs, byte[] bytes, string path) =>
			{
				if (mSpriteRenderer == null)
				{
					return;
				}
				var material = res as Material;
				if (mIsNewMaterial)
				{
					// 当需要复制一个新的材质时,刚加载出来的材质实际上就不会再用到了
					// 只有当下次还加载相同的材质时才会直接返回已加载的材质
					// 如果要卸载最开始加载出来的材质,只能通过卸载整个文件夹的资源来卸载
					Material newMat = new(material);
					newMat.name = materialName + "_" + IToS(mID);
					mSpriteRenderer.material = newMat;
				}
				else
				{
					mSpriteRenderer.material = material;
				}
			});
		}
	}
	public void setMaterial(Material material)
	{
		mSpriteRenderer.material = material;
	}
	public void setShader(Shader shader, bool force)
	{
		if (mSpriteRenderer == null || mSpriteRenderer.material == null)
		{
			return;
		}
		if (force)
		{
			mSpriteRenderer.material.shader = null;
			mSpriteRenderer.material.shader = shader;
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
		return mSpriteRenderer.material;
	}
	public string getMaterialName()
	{
		if (mSpriteRenderer == null || mSpriteRenderer.material == null)
		{
			return null;
		}
		return mSpriteRenderer.material.name;
	}
	public string getShaderName()
	{
		if (mSpriteRenderer.material == null || mSpriteRenderer.material.shader == null)
		{
			return null;
		}
		return mSpriteRenderer.material.shader.name;
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
	public string getOriginTextureName() { return mOriginTextureName; }
	public void setOriginTextureName(string textureName) { mOriginTextureName = textureName; }
	// 自动计算图片的原始名称,也就是不带后缀的名称,后缀默认以_分隔
	public void generateOriginTextureName(char key = '_')
	{
		int pos = mOriginTextureName.LastIndexOf(key);
		if (pos < 0)
		{
			logError("texture name is not valid!can not generate origin texture name, texture name : " + mOriginTextureName);
			return;
		}
		mOriginTextureName = mOriginTextureName.rangeToLastInclude(key);
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