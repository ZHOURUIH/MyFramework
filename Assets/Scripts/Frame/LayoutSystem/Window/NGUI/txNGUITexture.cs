using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUITexture : txNGUIObject, IShaderWindow
{
	protected UITexture mTexture;
	protected WindowShader mWindowShader;
    protected string mOriginTextureName;    // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected bool mIsNewMaterial;
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		mTexture = getUnityComponent<UITexture>();
		mTexture.setOnRender(onWidgetRender);
		string materialName = getMaterialName();
		if(materialName.Length != 0)
		{
			bool newMaterial = !mShaderManager.isSingleShader(materialName);
			if(newMaterial)
			{
				setMaterial(getMaterialName(), newMaterial);
			}
		}
        mOriginTextureName = getTextureName();
    }
	public virtual void setWindowShader<T>() where T : WindowShader, new()
	{
		mWindowShader = new T();
	}
	public virtual T getWindowShader<T>() where T : WindowShader
	{
		return mWindowShader as T;
	}
	public override void destroy()
	{
		// 卸载创建出的材质
		if(mIsNewMaterial)
		{
#if !UNITY_EDITOR
			destroyGameObject(mTexture.material);
#endif
			mTexture.material = null;
		}
		base.destroy();
	}
	public virtual void setTexture(Texture tex, bool useTextureSize = false)
	{
		if (mTexture == null)
		{
			return;
		}
		mTexture.mainTexture = tex;
		if (useTextureSize && tex != null)
		{
			setWindowSize(getTextureSize());
		}
	}
	public Texture getTexture()
	{
		if (mTexture == null)
		{
			return null;
		}
		return mTexture.mainTexture;
	}
	public void setMaterial(string materialName, bool newMaterial)
	{
		if (mTexture == null)
		{
			return;
		}
		if(materialName.IndexOf('_') != -1)
		{
			logError("非法的材质名: " + materialName + ", 可能是重复加载引起的");
			return;
		}
		mIsNewMaterial = newMaterial;
		// 查看是否允许同步加载
		if (mResourceManager.syncLoadAvalaible())
		{	
			Material mat = null;
			Material loadedMaterial = mResourceManager.loadResource<Material>(CommonDefine.R_MATERIAL_PATH + materialName, true);
			if (mIsNewMaterial)
			{
				mat = new Material(loadedMaterial);
				mat.name = materialName + "_" + mID;
			}
			else
			{
				mat = loadedMaterial;
			}
			mTexture.material = mat;
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
	public void setShader(Shader shader, bool force)
	{
		if (mTexture == null)
		{
			return;
		}
		if(force)
		{
			mTexture.shader = null;
			mTexture.shader = shader;
		}
	}
	public void setTextureName(string name, bool useTextureSize = false)
	{
		if (name.Length != 0)
		{
			// 允许同步加载时,使用同步加载
			if (mResourceManager.syncLoadAvalaible())
			{
				Texture tex = mResourceManager.loadResource<Texture>(name, true);
				setTexture(tex, useTextureSize);
			}
			// 否则只能使用异步加载
			else
			{
				TextureLoadParam param = new TextureLoadParam();
				param.mUseTextureSize = useTextureSize;
				mResourceManager.loadResourceAsync<Texture>(name, onTextureLoaded, param, true);
			}
		}
		else
		{
			setTexture(null, useTextureSize);
		}
	}
	public string getTextureName()
	{
		if(mTexture == null || mTexture.mainTexture == null)
		{
			return EMPTY_STRING;
		}
		return mTexture.mainTexture.name;
	}
	public Vector2 getTextureSize()
	{
		if (mTexture.mainTexture == null)
		{
			return Vector2.zero;
		}
		return new Vector2(mTexture.mainTexture.width, mTexture.mainTexture.height);
	}
	public string getMaterialName()
	{
		if (mTexture == null || mTexture.material == null)
		{
			return EMPTY_STRING;
		}
		return mTexture.material.name;
	}
	public string getShaderName()
	{
		if (mTexture == null || mTexture.material == null || mTexture.material.shader == null)
		{
			return EMPTY_STRING;
		}
		return mTexture.material.shader.name;
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		if (mTexture == null)
		{
			return;
		}
		mTexture.alpha = alpha;
	}
	public override float getAlpha()
	{
		if (mTexture == null)
		{
			return 0.0f;
		}
		return mTexture.alpha;
	}
	public override void setFillPercent(float percent)
	{
		if (mTexture == null)
		{
			return;
		}
		mTexture.fillAmount = percent;
	}
	public override float getFillPercent()
	{
		if (mTexture == null)
		{
			return 0.0f;
		}
		return mTexture.fillAmount;
	}
	public override Vector2 getWindowSize(bool transformed = false)
	{
		if (mTexture == null)
		{
			return Vector2.zero;
		}
		Vector2 textureSize = new Vector2(mTexture.width, mTexture.height);
		if(transformed)
		{
			Vector2 scale = getWorldScale();
			textureSize.x *= scale.x;
			textureSize.y *= scale.y;
		}
		return textureSize;
	}
	public override void setWindowSize(Vector2 size)
	{
		mTexture.width = (int)size.x;
		mTexture.height = (int)size.y;
	}
	public override int getDepth()
	{
		if(mTexture == null)
		{
			return 0;
		}
		return mTexture.depth;
	}
	public override void setDepth(int depth)
	{
		if(mTexture == null)
		{
			return;
		}
		mTexture.depth = depth;
		base.setDepth(depth);
	}
	public virtual void setColor(Color color){mTexture.color = color;}
	public virtual Color getColor() { return mTexture.color; }
	public UITexture getUITexture(){return mTexture;}
	public string getOriginTextureName() { return mOriginTextureName; }
    public void setOriginTextureName(string textureName) { mOriginTextureName = textureName; }
	// 自动计算图片的原始名称,也就是不带后缀的名称,后缀默认以_分隔
	public void generateOriginTextureName(string key = "_")
	{
		int pos = mOriginTextureName.LastIndexOf(key);
		if (pos >= 0)
		{
			mOriginTextureName = mOriginTextureName.Substring(0, mOriginTextureName.LastIndexOf(key) + 1);
		}
		else
		{
			logError("texture name is not valid!can not generate origin texture name, texture name : " + mOriginTextureName);
		}
	}
	//---------------------------------------------------------------------------------------------------
	protected void onWidgetRender(Material mat)
	{
		mWindowShader?.applyShader(mat);
	}
	protected void onMaterialLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		Material material = res as Material;
		var param = userData as LoadMaterialParam;
		if (param.mNewMaterial)
		{
			Material newMat = new Material(material);
			newMat.name = param.mMaterialName + "_" + mID;
			mTexture.material = newMat;
		}
		else
		{
			mTexture.material = material;
		}
		mClassPool.destroyClass(param);
	}
	protected void onTextureLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		TextureLoadParam param = (TextureLoadParam)userData;
		setTexture(res as Texture, param.mUseTextureSize);
	}
}

#endif