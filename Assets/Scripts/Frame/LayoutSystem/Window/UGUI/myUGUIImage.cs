using UnityEngine;
using UnityEngine.UI;

// UGUI的静态图片不支持递归变化透明度
public class myUGUIImage : myUGUIObject, IShaderWindow
{
	protected AssetLoadDoneCallback mMaterialLoadCallback;
	protected WindowShader mWindowShader;	// 图片所使用的shader类,用于动态设置shader参数
	protected UGUIAtlas mAtlas;				// 图片图集
	protected Sprite mOriginSprite;			// 备份加载物体时原始的精灵图片
	protected Image mImage;					// 图片组件
	protected string mOriginTextureName;    // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mIsNewMaterial;
	public myUGUIImage()
	{
		mMaterialLoadCallback = onMaterialLoaded;
	}
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		mImage = mObject.GetComponent<Image>();
		if(mImage == null)
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
		// 获取初始的精灵所在图集
		if (mOriginSprite != null)
		{
			Texture2D curTexture = mOriginSprite.texture;
			if (curTexture != null)
			{
				ImageAtlasPath imageAtlasPath = mObject.GetComponent<ImageAtlasPath>();
				string atlasPath;
				if(imageAtlasPath != null)
				{
					atlasPath = imageAtlasPath.mAtlasPath;
				}
				else
				{
					atlasPath = FrameDefine.R_ATLAS_GAME_ATLAS_PATH + curTexture.name + "/" + curTexture.name;
				}
				mAtlas = mTPSpriteManager.getAtlas(atlasPath, false, true);
				if(mAtlas != null && mAtlas.mTexture != curTexture)
				{
					logError("设置的图集与加载出的图集不一致!可能未添加ImageAtlasPath组件,或者ImageAtlasPath组件中记录的路径错误,或者是在当前物体在重复使用过程中销毁了原始图集");
				}
			}
		}
		mOriginTextureName = getSpriteName();
		string materialName = getMaterialName();
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if(!isEmpty(materialName) && materialName != FrameDefine.BUILDIN_UI_MATERIAL)
		{
			if (!mShaderManager.isSingleShader(materialName))
			{
				setMaterialName(materialName, true);
			}
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
			mImage.material = null;
		}
		// 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
		// 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
		mImage.sprite = mOriginSprite;
		base.destroy();
	}
	public void setWindowShader(WindowShader shader) { mWindowShader = shader; }
	public WindowShader getWindowShader() { return mWindowShader; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if(mImage.material != null)
		{
			mWindowShader?.applyShader(mImage.material);
		}
	}
	public UGUIAtlas getAtlas() { return mAtlas; }
	public virtual void setAtlas(UGUIAtlas atlas, bool clearSprite = false)
	{
		if(mImage == null)
		{
			return;
		}
		mAtlas = atlas;
		if(clearSprite)
		{
			setSprite(null);
		}
		else
		{
			setSprite(mTPSpriteManager.getSprite(mAtlas, getSpriteName()));
		}
	}
	public void setSpriteName(string spriteName, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null || mAtlas == null)
		{
			return;
		}
		if (isEmpty(spriteName))
		{
			mImage.sprite = null;
			return;
		}
		Sprite sprite = mTPSpriteManager.getSprite(mAtlas, spriteName);
		setSprite(sprite, useSpriteSize, sizeScale);
	}
	// 设置图片,需要确保图片在当前图集内
	public void setSprite(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null)
		{
			return;
		}
		if (sprite != null && sprite.texture != mAtlas.mTexture)
		{
			logWarning("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
		}
		setSpriteOnly(sprite, useSpriteSize, sizeScale);
	}
	// 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
	// 并且需要自己保证设置不同图集的图片以后不会有什么问题
	public void setSpriteOnly(Sprite tex, bool useSpriteSize = false, float sizeScale = 1.0f)
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
	public void setMaterialName(string materialName, bool newMaterial)
	{
		if(mImage == null)
		{ 
			return; 
		}
		mIsNewMaterial = newMaterial;
		// 查看是否允许同步加载
		if (mResourceManager.isSyncLoadAvalaible())
		{
			Material mat;
			Material loadedMaterial = mResourceManager.loadResource<Material>(FrameDefine.R_MATERIAL_PATH + materialName);
			if(mIsNewMaterial)
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
		else
		{
			CLASS_MAIN(out LoadMaterialParam param);
			param.mMaterialName = materialName;
			param.mNewMaterial = mIsNewMaterial;
			mResourceManager.loadResourceAsync<Material>(FrameDefine.R_MATERIAL_PATH + materialName, mMaterialLoadCallback, param);
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
	public override void setFillPercent(float percent) { mImage.fillAmount = percent; }
	public override float getFillPercent() { return mImage.fillAmount; }
	public override void setColor(Color color) { mImage.color = color; }
	public void setColor(Vector3 color) { mImage.color = new Color(color.x, color.y, color.z); }
	public override Color getColor() { return mImage.color; }
	public void setUGUIRaycastTarget(bool enable) { mImage.raycastTarget = enable; }
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
	//----------------------------------------------------------------------------------------------------------------------------------
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
		UN_CLASS(param);
	}
}