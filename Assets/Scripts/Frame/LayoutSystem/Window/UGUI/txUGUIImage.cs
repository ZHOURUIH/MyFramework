using UnityEngine;
using UnityEngine.UI;

// UGUI的静态图片不支持递归变化透明度
public class txUGUIImage : txUGUIObject, IShaderWindow
{
	protected Image mImage;
	protected Sprite mOriginSprite;	// 备份加载物体时原始的精灵图片
	protected Texture2D mAtlas;
	protected WindowShader mWindowShader;
	protected string mOriginTextureName;    // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected string mNormalSprite;
	protected string mPressSprite;
	protected string mHoverSprite;
	protected string mSelectedSprite;
	protected bool mIsNewMaterial;
	protected bool mSelected;
	protected bool mUseStateSprite;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
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
			logError(GetType() + " can not find " + typeof(Image) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		mOriginSprite = mImage.sprite;
		// 获取初始的精灵所在图集
		if (mOriginSprite != null)
		{
			mAtlas = mOriginSprite.texture;
			if(mAtlas != null)
			{
				ImageAtlasPath imageAtlasPath = mObject.GetComponent<ImageAtlasPath>();
				string atlasPath;
				if(imageAtlasPath != null)
				{
					atlasPath = imageAtlasPath.mAtlasPath;
				}
				else
				{
					atlasPath = CommonDefine.R_ATLAS_GAME_ATLAS_PATH + mAtlas.name + "/" + mAtlas.name;
				}
				Texture2D atlas = mTPSpriteManager.getAtlas(atlasPath, false, true);
				if(atlas != null && atlas != mAtlas)
				{
					logError("设置的图集与加载出的图集不一致!可能未添加ImageAtlasPath组件,或者ImageAtlasPath组件中记录的路径错误,或者是在当前物体在重复使用过程中销毁了原始图集");
				}
			}
		}
		mNormalSprite = getSpriteName();
		mPressSprite = mNormalSprite;
		mHoverSprite = mNormalSprite;
		mSelectedSprite = mNormalSprite;
		mOriginTextureName = mNormalSprite;
		string materialName = getMaterialName();
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if(!isEmpty(materialName) && materialName != CommonDefine.BUILDIN_UI_MATERIAL)
		{
			bool newMaterial = !mShaderManager.isSingleShader(materialName);
			if(newMaterial)
			{
				setMaterialName(materialName, newMaterial);
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
	public void setWindowShader<T>() where T : WindowShader, new()
	{
		mWindowShader = new T();
	}
	public T getWindowShader<T>() where T : WindowShader
	{
		return mWindowShader as T;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if(mImage.material != null)
		{
			mWindowShader?.applyShader(mImage.material);
		}
	}
	public void setNormalSprite(string normalSprite, bool apply = true, bool resetStateSprites = true)
	{
		mUseStateSprite = true;
		mNormalSprite = normalSprite;
		if (apply)
		{
			setSpriteName(mNormalSprite);
		}
		if (resetStateSprites)
		{
			mPressSprite = mNormalSprite;
			mHoverSprite = mNormalSprite;
			mSelectedSprite = mNormalSprite;
		}
	}
	public void setPressSprite(string pressSprite)
	{
		mUseStateSprite = true;
		mPressSprite = pressSprite;
	}
	public void setHoverSprite(string hoverSprite)
	{
		mUseStateSprite = true;
		mHoverSprite = hoverSprite;
	}
	public void setSelectedSprite(string selectedSprite)
	{
		mUseStateSprite = true;
		mSelectedSprite = selectedSprite;
	}
	public void setSpriteNames(string pressSprite, string hoverSprite)
	{
		mUseStateSprite = true;
		mPressSprite = pressSprite;
		mHoverSprite = hoverSprite;
	}
	public void setSpriteNames(string pressSprite, string hoverSprite, string selectedSprite)
	{
		mUseStateSprite = true;
		mPressSprite = pressSprite;
		mHoverSprite = hoverSprite;
		mSelectedSprite = selectedSprite;
	}
	public Texture2D getAtlas() { return mAtlas; }
	public virtual void setAtlas(Texture2D atlas, bool clearSprite = false)
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
		}
		else
		{
			Sprite sprite = mTPSpriteManager.getSprite(mAtlas, spriteName);
			setSprite(sprite, useSpriteSize, sizeScale);
		}
	}
	// 设置图片,需要确保图片在当前图集内
	public void setSprite(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null)
		{
			return;
		}
		if(sprite != null && sprite.texture != mAtlas)
		{
			logWarning("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
		}
		mImage.sprite = sprite;
		if(useSpriteSize)
		{
			setWindowSize(getSpriteSize() * sizeScale);
		}
	}
	// 只设置图片,不关心所在图集
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
		if (mResourceManager.syncLoadAvalaible())
		{
			Material mat = null;
			Material loadedMaterial = mResourceManager.loadResource<Material>(CommonDefine.R_MATERIAL_PATH + materialName, true);
			if(mIsNewMaterial)
			{
				mat = new Material(loadedMaterial);
				mat.name = materialName + "_" + mID;
			}
			else
			{
				mat = loadedMaterial;
			}
			mImage.material = mat;
		}
		else
		{
			LoadMaterialParam param;
			mClassPool.newClass(out param);
			param.mMaterialName = materialName;
			param.mNewMaterial = mIsNewMaterial;
			mResourceManager.loadResourceAsync<Material>(CommonDefine.R_MATERIAL_PATH + materialName, onMaterialLoaded, param, true);
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
	public override void onMouseEnter()
	{
		base.onMouseEnter();
		if (mUseStateSprite)
		{
			if (mSelected)
			{
				setSpriteName(mSelectedSprite);
			}
			else
			{
				setSpriteName(mHoverSprite);
			}
		}
	}
	public override void onMouseLeave()
	{
		base.onMouseLeave();
		if (mUseStateSprite)
		{
			if (mSelected)
			{
				setSpriteName(mSelectedSprite);
			}
			else
			{
				setSpriteName(mNormalSprite);
			}
		}
	}
	public override void onMouseDown(Vector3 mousePos)
	{
		base.onMouseDown(mousePos);
		if (mUseStateSprite)
		{
			if (mSelected)
			{
				setSpriteName(mSelectedSprite);
			}
			else
			{
				setSpriteName(mPressSprite);
			}
		}
	}
	public override void onMouseUp(Vector3 mousePos)
	{
		base.onMouseUp(mousePos);
		// 一般都会再mouseUp时触发点击消息,跳转界面,所以基类中可能会将当前窗口销毁,需要注意
		if (mUseStateSprite)
		{
			if (mSelected)
			{
				setSpriteName(mSelectedSprite);
			}
			else
			{
				setSpriteName(mNormalSprite);
			}
		}
	}
	public void setSelected(bool select)
	{
		if (mSelected == select)
		{
			return;
		}
		mSelected = select;
		if (mUseStateSprite)
		{
			if (mSelected)
			{
				setSpriteName(mSelectedSprite);
			}
			else
			{
				setSpriteName(mNormalSprite);
			}
		}
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
		if(param.mNewMaterial)
		{
			Material newMat = new Material(material);
			newMat.name = param.mMaterialName + "_" + mID;
			mImage.material = newMat;
		}
		else
		{
			mImage.material = material;
		}
		mClassPool.destroyClass(param);
	}
}