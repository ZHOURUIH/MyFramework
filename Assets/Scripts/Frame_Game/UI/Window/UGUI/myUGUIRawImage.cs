using UnityEngine;
using UnityEngine.UI;
using static FrameBase;
using static UnityUtility;
using static FrameEditorUtility;

// 对UGUI的RawImage的封装
public class myUGUIRawImage : myUGUIObject
{
	protected RawImage mRawImage;                       // UGUI的RawImage组件
	protected CanvasGroup mCanvasGroup;					// 用于是否显示
	protected Material mOriginMaterial;					// 初始的材质,用于重置时恢复材质
	protected Texture mOriginTexture;					// 初始的图片
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
			Texture tex = mResourceManager.loadGameResource<Texture>(name);
			setTexture(tex, useTextureSize);
		}
		// 异步加载
		else
		{
			mResourceManager.loadGameResourceAsync<Texture>(name, (Object res, Object[] _, byte[] _, string _)=>
			{
				setTexture(res as Texture, useTextureSize);
			});
		}
	}
}