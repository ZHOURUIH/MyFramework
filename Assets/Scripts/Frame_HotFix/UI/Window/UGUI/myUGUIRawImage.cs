using UnityEngine;
using UnityEngine.UI;
using static FrameBaseHotFix;
using static MathUtility;
using static StringUtility;
using static FrameDefine;
using static UnityUtility;
using static FrameEditorUtility;

// 对UGUI的RawImage的封装
public class myUGUIRawImage : myUGUIObject, IShaderWindow
{
	protected WindowShader mWindowShader;				// shader对象
	protected RawImage mRawImage;                       // UGUI的RawImage组件
	protected CanvasGroup mCanvasGroup;					// 用于是否显示
	protected Material mOriginMaterial;					// 初始的材质,用于重置时恢复材质
	protected Texture mOriginTexture;                   // 初始的图片
	protected string mOriginMaterialPath;				// 初始材质的文件路径
	protected bool mIsNewMaterial;						// 当前的材质是否是新创建的材质对象
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mRawImage))
		{
			mRawImage = mObject.AddComponent<RawImage>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
		mOriginMaterial = mRawImage.material;
		mOriginTexture = mRawImage.texture;
		string materialName = getMaterialName().removeAll(" (Instance)");
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if (!materialName.isEmpty() && 
			materialName != BUILDIN_UI_MATERIAL)
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
				destroyUnityObject(mRawImage.material);
			}
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
			mCanvasGroup = getOrAddUnityComponent<CanvasGroup>();
		}
		mCanvasGroup.alpha = isCull ? 0.0f : 1.0f;
	}
	public bool isCull() { return mCanvasGroup != null && isFloatZero(mCanvasGroup.alpha); }
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
		if (mRawImage == null)
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
		return new(mRawImage.texture.width, mRawImage.texture.height);
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
		if (name.isEmpty())
		{
			setTexture(null, useTextureSize);
			return;
		}
		// 同步加载
		if (!loadAsync)
		{
			setTexture(mResourceManager.loadGameResource<Texture>(name), useTextureSize);
		}
		// 异步加载
		else
		{
			mResourceManager.loadGameResourceAsync(name, (Texture tex) => { setTexture(tex, useTextureSize); });
		}
	}
	public Material getMaterial() 
	{
		if (mRawImage == null)
		{
			return null;
		}
		return mRawImage.material;
	}
	public string getMaterialName()
	{
		if (mRawImage == null || mRawImage.material == null)
		{
			return null;
		}
		return mRawImage.material.name;
	}
	public void setMaterialName(string materialPath, bool newMaterial, bool loadAsync = false)
	{
		if (mRawImage == null)
		{
			return;
		}
		mIsNewMaterial = newMaterial;
		// 异步加载
		if (loadAsync)
		{
			mResourceManager.loadGameResourceAsync(materialPath, (Material mat) =>
			{
				if (mRawImage == null)
				{
					return;
				}
				if (mIsNewMaterial)
				{
					Material newMat = new(mat);
					newMat.name = getFileNameNoSuffixNoDir(materialPath) + "_" + IToS(mID);
					mRawImage.material = newMat;
				}
				else
				{
					mRawImage.material = mat;
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
				mRawImage.material = mat;
			}
			else
			{
				mRawImage.material = loadedMaterial;
			}
		}
	}
}