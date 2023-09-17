using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static FrameBase;
using static CSharpUtility;
using static MathUtility;
using static StringUtility;
using static FrameUtility;
using static FrameDefine;

// 对UGUI的RawImage的封装
public class myUGUIRawImage : myUGUIObject, IShaderWindow
{
	protected AssetLoadDoneCallback mTextureCallback;	// 避免GC的回调
	protected AssetLoadDoneCallback mMaterialCallback;	// 避免GC的回调
	protected WindowShader mWindowShader;				// shader对象
	protected RawImage mRawImage;                       // UGUI的RawImage组件
	protected CanvasGroup mCanvasGroup;					// 用于是否显示
	protected Material mOriginMaterial;					// 初始的材质,用于重置时恢复材质
	protected Texture mOriginTexture;					// 初始的图片
	protected bool mIsNewMaterial;						// 当前的材质是否是新创建的材质对象
	public myUGUIRawImage()
	{
		mTextureCallback = onTextureLoaded;
		mMaterialCallback = onMaterialLoaded;
	}
	public override void init()
	{
		base.init();
		mRawImage = mObject.GetComponent<RawImage>();
		if (mRawImage == null)
		{
			mRawImage = mObject.AddComponent<RawImage>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mRawImage == null)
		{
			logError(Typeof(this) + " can not find " + typeof(RawImage) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		mOriginMaterial = mRawImage.material;
		mOriginTexture = mRawImage.texture;
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
			destroyGameObject(mRawImage.material);
#endif
		}
		mRawImage.material = mOriginMaterial;
		mRawImage.texture = mOriginTexture;
		if (mCanvasGroup != null)
		{
			mCanvasGroup.alpha = 1.0f;
			mCanvasGroup = null;
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
	}
	public bool isCull() { return mCanvasGroup != null && isFloatZero(mCanvasGroup.alpha); }
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
		if (mRawImage.material != null)
		{
			mWindowShader?.applyShader(mRawImage.material);
		}
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mRawImage.color;
		color.a = alpha;
		mRawImage.color = color;
	}
	public virtual void setTexture(Texture tex, bool useTextureSize = false)
	{
		if (mRawImage == null)
		{
			return;
		}
		mRawImage.texture = tex;
		if (useTextureSize && tex != null)
		{
			setWindowSize(getTextureSize());
		}
	}
	public Texture getTexture()
	{
		if(mRawImage == null)
		{
			return null;
		}
		return mRawImage.texture;
	}
	public Vector2 getTextureSize()
	{
		if (mRawImage.texture == null)
		{
			return Vector2.zero;
		}
		return new Vector2(mRawImage.texture.width, mRawImage.texture.height);
	}
	public string getTextureName()
	{
		if (mRawImage == null || mRawImage.texture == null)
		{
			return null;
		}
		return mRawImage.texture.name;
	}
	public void setTextureName(string name, bool useTextureSize = false, bool loadAsync = false)
	{
		if (isEmpty(name))
		{
			setTexture(null, useTextureSize);
			return;
		}
		// 同步加载
		if (!loadAsync)
		{
			Texture tex = mResourceManager.loadResource<Texture>(name);
			setTexture(tex, useTextureSize);
		}
		// 异步加载
		else
		{
			mResourceManager.loadResourceAsync<Texture>(name, mTextureCallback, useTextureSize);
		}
	}
	public string getMaterialName()
	{
		if (mRawImage == null || mRawImage.material == null)
		{
			return null;
		}
		return mRawImage.material.name;
	}
	public void setMaterialName(string materialName, bool newMaterial, bool loadAsync = false)
	{
		if (mRawImage == null)
		{
			return;
		}
		mIsNewMaterial = newMaterial;
		// 同步加载
		if (!loadAsync)
		{
			Material mat;
			var loadedMaterial = mResourceManager.loadResource<Material>(R_MATERIAL_PATH + materialName);
			if (mIsNewMaterial)
			{
				mat = new Material(loadedMaterial);
				mat.name = materialName + "_" + IToS(mID);
			}
			else
			{
				mat = loadedMaterial;
			}
			mRawImage.material = mat;
		}
		// 异步加载
		else
		{
			CLASS(out LoadMaterialParam param);
			param.mMaterialName = materialName;
			param.mNewMaterial = mIsNewMaterial;
			mResourceManager.loadResourceAsync<Material>(R_MATERIAL_PATH + materialName, mMaterialCallback, param);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onTextureLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		// userData表示是否使用图片尺寸设置窗口大小
		setTexture(res as Texture, (bool)userData);
	}
	protected void onMaterialLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		if (mRawImage == null)
		{
			return;
		}
		var material = res as Material;
		var param = userData as LoadMaterialParam;
		if (param.mNewMaterial)
		{
			Material newMat = new Material(material);
			newMat.name = param.mMaterialName + "_" + IToS(mID);
			mRawImage.material = newMat;
		}
		else
		{
			mRawImage.material = material;
		}
		UN_CLASS(ref param);
	}
}