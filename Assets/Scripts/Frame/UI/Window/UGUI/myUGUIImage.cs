using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static StringUtility;
using static FrameBase;
using static CSharpUtility;
using static MathUtility;
using static FrameUtility;
using static FrameDefine;

// 对UGUI的Image的封装,UGUI的静态图片不支持递归变化透明度
public class myUGUIImage : myUGUIObject, IShaderWindow
{
	protected AssetLoadDoneCallback mMaterialLoadCallback;      // 避免GC的回调
	protected WindowShader mWindowShader;   // 图片所使用的shader类,用于动态设置shader参数
	protected CanvasGroup mCanvasGroup;     // 用于是否显示
	protected UGUIAtlasPtr mOriginAtlasPtr;	// 图片图集,用于卸载
	protected UGUIAtlasPtr mAtlasPtr;		// 图片图集
	protected Material mOriginMaterial;     // 初始的材质,用于重置时恢复材质
	protected Sprite mOriginSprite;         // 备份加载物体时原始的精灵图片
	protected Image mImage;                 // 图片组件
	protected string mOriginTextureName;    // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mIsNewMaterial;          // 当前的材质是否是新建的材质对象
	protected bool mCanvasGroupValid;       // 当前CanvasGroup是否有效,在测试中发现mCanvasGroup != null的写法会比较耗时,所以替换为bool判断
	protected bool mOriginAtlasInResources;	// OriginAtlas是否是从Resources中加载的
	public myUGUIImage()
	{
		mMaterialLoadCallback = onMaterialLoaded;
	}
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		mImage = mObject.GetComponent<Image>();
		if (mImage == null)
		{
			mImage = mObject.AddComponent<Image>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mImage == null)
		{
			logError(Typeof(this) + " can not find " + typeof(Image) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		mOriginSprite = mImage.sprite;
		mOriginMaterial = mImage.material;
		// mOriginSprite无法简单使用?.来判断是否为空,需要显式判断
		Texture2D curTexture = mOriginSprite != null ? mOriginSprite.texture : null;
		// 获取初始的精灵所在图集
		if (curTexture != null)
		{
			var imageAtlasPath = mObject.GetComponent<ImageAtlasPath>();
			if (imageAtlasPath == null)
			{
				logError("找不到图集,请添加ImageAtlasPath组件, window:" + mName + ", layout:" + mLayout.getName());
			}
			string atlasPath = imageAtlasPath.mAtlasPath;
			mOriginAtlasInResources = mLayout.isInResources();
			if (mOriginAtlasInResources)
			{
				mOriginAtlasPtr = mTPSpriteManager.getAtlasInResources(atlasPath, false, true);
			}
			else
			{
				mOriginAtlasPtr = mTPSpriteManager.getAtlas(atlasPath, false, true);
			}
			if (!mOriginAtlasPtr.isValid())
			{
				logWarning("无法加载初始化的图集:" + atlasPath + ", window:" + mName + ", layout:" + mLayout.getName() +
					",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
			}
			if (mOriginAtlasPtr.isValid() && mOriginAtlasPtr.getTexture() != curTexture)
			{
				logError("设置的图集与加载出的图集不一致!可能未添加ImageAtlasPath组件,或者ImageAtlasPath组件中记录的路径错误," +
					"或者是在当前物体在重复使用过程中销毁了原始图集\n图集名:" + mOriginSprite.name + ", 记录的图集路径:" + atlasPath + ", 名字:" + mName + 
					"layout:" + mLayout.getName());
			}
			mAtlasPtr = mOriginAtlasPtr;
		}
		mOriginTextureName = getSpriteName();
		string materialName = getMaterialName();
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if (!isEmpty(materialName) &&
			materialName != BUILDIN_UI_MATERIAL &&
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
#if !UNITY_EDITOR
			destroyGameObject(mImage.material);
#endif
		}
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mImage.sprite = mOriginSprite;
		mImage.material = mOriginMaterial;
		if (mCanvasGroup != null)
		{
			mCanvasGroup.alpha = 1.0f;
			mCanvasGroup = null;
		}
		mCanvasGroupValid = false;
		if (mOriginAtlasInResources)
		{
			mTPSpriteManager.unloadAtlasInResourcecs(ref mOriginAtlasPtr);
		}
		else
		{
			mTPSpriteManager.unloadAtlas(ref mOriginAtlasPtr);
		}
		base.destroy();
	}
	// 是否剔除渲染
	public void cull(bool isCull)
	{
		if (mCanvasGroup == null)
		{
			mCanvasGroup = getUnityComponent<CanvasGroup>();
		}
		mCanvasGroup.alpha = isCull ? 0.0f : 1.0f;
		mCanvasGroupValid = true;
	}
	public override bool isCulled() { return mCanvasGroupValid && isFloatZero(mCanvasGroup.alpha); }
	public override bool canUpdate() { return !isCulled() && base.canUpdate(); }
	public override bool canGenerateDepth() { return !isCulled(); }
	public void setRenderQueue(int renderQueue)
	{
		if (mImage == null || mImage.material == null)
		{
			return;
		}
		mImage.material.renderQueue = renderQueue;
	}
	public int getRenderQueue()
	{
		if (mImage == null || mImage.material == null)
		{
			return 0;
		}
		return mImage.material.renderQueue;
	}
	public void setWindowShader(WindowShader shader)
	{
		mWindowShader = shader;
		// 因为shader参数的需要在update中更新,所以需要启用窗口的更新
		mEnable = true;
	}
	public WindowShader getWindowShader() { return mWindowShader; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isCulled())
		{
			return;
		}
		if(mImage.material != null)
		{
			mWindowShader?.applyShader(mImage.material);
		}
	}
	public UGUIAtlasPtr getAtlas() { return mAtlasPtr; }
	public virtual void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false)
	{
		if(mImage == null)
		{
			return;
		}
		mAtlasPtr = atlas;
		if (clearSprite)
		{
			setSprite(null, false);
		}
		else
		{
			setSprite(mAtlasPtr.getSprite(getSpriteName()), false);
		}
	}
	public void setSpriteName(string spriteName, bool useSpriteSize, float sizeScale = 1.0f)
	{
		if (mImage == null || !mAtlasPtr.isValid())
		{
			return;
		}
		if (isEmpty(spriteName))
		{
			mImage.sprite = null;
			return;
		}
		setSprite(mAtlasPtr.getSprite(spriteName), useSpriteSize, sizeScale);
	}
	// 设置图片,需要确保图片在当前图集内
	public void setSprite(Sprite sprite, bool useSpriteSize, float sizeScale = 1.0f)
	{
		if (mImage == null)
		{
			return;
		}
		if (sprite != null && sprite.texture != mAtlasPtr.getTexture())
		{
			logError("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
		}
		setSpriteOnly(sprite, useSpriteSize, sizeScale);
	}
	// 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
	// 并且需要自己保证设置不同图集的图片以后不会有什么问题
	public void setSpriteOnly(Sprite tex, bool useSpriteSize, float sizeScale = 1.0f)
	{
		if (mImage == null)
		{
			return;
		}
		mImage.sprite = tex;
		if (useSpriteSize)
		{
			setWindowSize(getSpriteSize() * sizeScale);
		}
	}
	public Vector2 getSpriteSize()
	{
		if (mImage == null)
		{
			return Vector2.zero;
		}
		if(mImage.sprite != null)
		{
			return mImage.sprite.rect.size;
		}
		return getWindowSize();
	}
	public Image getImage() { return mImage; }
	public Sprite getSprite() { return mImage.sprite; }
	public void setMaterialName(string materialName, bool newMaterial, bool loadAsync = false)
	{
		if(mImage == null)
		{ 
			return; 
		}
		mIsNewMaterial = newMaterial;
		// 同步加载
		if (!loadAsync)
		{
			Material mat;
			Material loadedMaterial = mResourceManager.loadResource<Material>(R_MATERIAL_PATH + materialName + ".mat");
			if (mIsNewMaterial)
			{
				mat = new Material(loadedMaterial);
				mat.name = materialName + "_" + IToS(mID);
			}
			else
			{
				mat = loadedMaterial;
			}
			mImage.material = mat;
		}
		// 异步加载
		else
		{
			CLASS(out LoadMaterialParam param);
			param.mMaterialName = materialName;
			param.mNewMaterial = mIsNewMaterial;
			mResourceManager.loadResourceAsync<Material>(R_MATERIAL_PATH + materialName + ".mat", mMaterialLoadCallback, param);
		}
	}
	public void setMaterial(Material material)
	{
		mImage.material = material;
	}
	public void setShader(Shader shader, bool force)
	{
		if(mImage == null || mImage.material == null)
		{
			return;
		}
		if (force)
		{
			mImage.material.shader = null;
			mImage.material.shader = shader;
		}
	}
	public string getSpriteName()
	{
		if(mImage == null || mImage.sprite == null)
		{
			return null;
		}
		return mImage.sprite.name;
	}
	public Material getMaterial()
	{
		if(mImage == null)
		{
			return null;
		}
		return mImage.material;
	}
	public string getMaterialName()
	{
		if(mImage == null || mImage.material == null)
		{
			return null;
		}
		return mImage.material.name;
	}
	public string getShaderName()
	{
		if(mImage.material == null || mImage.material.shader == null)
		{
			return null;
		}
		return mImage.material.shader.name;
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		if(mImage == null)
		{
			return;
		}
		Color color = mImage.color;
		color.a = alpha;
		mImage.color = color;
	}
	public override float getAlpha() { return mImage.color.a; }
	public override void setFillPercent(float percent) 
	{
		if (mImage == null)
		{
			return;
		}
		mImage.fillAmount = percent; 
	}
	public override float getFillPercent() { return mImage.fillAmount; }
	public override void setColor(Color color) 
	{
		if (mImage == null)
		{
			return;
		}
		mImage.color = color; 
	}
	public void setColor(Vector3 color) 
	{
		if (mImage == null)
		{
			return;
		}
		mImage.color = new Color(color.x, color.y, color.z); 
	}
	public override Color getColor() { return mImage.color; }
	public void setUGUIRaycastTarget(bool enable) 
	{
		if (mImage == null)
		{
			return;
		}
		mImage.raycastTarget = enable; 
	}
	public string getOriginTextureName() { return mOriginTextureName; }
	public void setOriginTextureName(string textureName) { mOriginTextureName = textureName; }
	// 自动计算图片的原始名称,也就是不带后缀的名称,后缀默认以_分隔
	public void generateOriginTextureName(string key = "_")
	{
		int pos = mOriginTextureName.LastIndexOf(key);
		if (pos < 0)
		{
			logError("texture name is not valid!can not generate origin texture name, texture name : " + mOriginTextureName);
			return;
		}
		mOriginTextureName = mOriginTextureName.Substring(0, mOriginTextureName.LastIndexOf(key) + 1);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onMaterialLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		if(mImage == null)
		{
			return;
		}
		Material material = res as Material;
		var param = userData as LoadMaterialParam;
		if (param.mNewMaterial)
		{
			// 当需要复制一个新的材质时,刚加载出来的材质实际上就不会再用到了
			// 只有当下次还加载相同的材质时才会直接返回已加载的材质
			// 如果要卸载最开始加载出来的材质,只能通过卸载整个文件夹的资源来卸载
			Material newMat = new Material(material);
			newMat.name = param.mMaterialName + "_" + IToS(mID);
			mImage.material = newMat;
		}
		else
		{
			mImage.material = material;
		}
		UN_CLASS(ref param);
	}
}