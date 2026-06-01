using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// 对UGUI的Image的封装,简化版,只有Image组件,不能在运行时切换图片
public class myUGUIImageSimple : myUGUIObject
{
	protected CanvasGroup mCanvasGroup;         // 用于是否显示
	protected Image mImage;                     // 图片组件
	protected string mSpriteName;				// 当前图片的名字,避免GC
	protected string mMaterialName;             // 当前材质的名字,避免GC
	protected string mShaderName;               // 当前shader的名字,避免GC
	protected bool mSpriteNameDirty;			// 图片名字是否需要更新
	protected bool mMaterialNameDirty;			// 材质名字是否需要更新
	protected bool mShaderNameDirty;			// shader名字是否需要更新
	protected bool mCanvasGroupValid;           // 当前CanvasGroup是否有效,在测试中发现判断mCanvasGroup是否为空的写法会比较耗时,所以替换为bool判断
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		if (!mObject.TryGetComponent(out mImage))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个Image组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mImage = mObject.AddComponent<Image>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
		mSpriteName = mImage.sprite != null ? mImage.sprite.name : null;
		mMaterialName = mImage.material != null ? mImage.material.name : null;
		mShaderName = mImage.material != null && mImage.material.shader != null ? mImage.material.shader.name : null;
	}
	public override void destroy()
	{
		if (mCanvasGroup != null)
		{
			mCanvasGroup.alpha = 1.0f;
			mCanvasGroup = null;
		}
		mCanvasGroupValid = false;
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
		mCanvasGroupValid = true;
	}
	public override bool isCulled() { return mCanvasGroupValid && isFloatZero(mCanvasGroup.alpha); }
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
	// 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
	// 并且需要自己保证设置不同图集的图片以后不会有什么问题
	public void setSpriteOnly(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null || mImage.sprite == sprite)
		{
			return;
		}
		mImage.sprite = sprite;
		mSpriteNameDirty = true;
		if (useSpriteSize)
		{
			setSize(getSpriteSize() * sizeScale);
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
		return getSize();
	}
	public Image getImage() { return mImage; }
	public Sprite getSprite() { return mImage.sprite; }
	public void setMaterial(Material material) 
	{
		mImage.material = material;
		mMaterialNameDirty = true;
	}
	public void setShader(Shader shader)
	{
		if(mImage == null || mImage.material == null || mImage.material.shader == shader)
		{
			return;
		}
		mImage.material.shader = null;
		mImage.material.shader = shader;
		mShaderNameDirty = true;
	}
	public string getSpriteName()
	{
		if (mSpriteNameDirty)
		{
			mSpriteNameDirty = false;
			mSpriteName = mImage.sprite != null ? mImage.sprite.name : null;
		}
		return mSpriteName;
	}
	public Material getMaterial()
	{
		if (mImage == null)
		{
			return null;
		}
		return mImage.material;
	}
	public string getMaterialName()
	{
		if (mMaterialNameDirty)
		{
			mMaterialNameDirty = false;
			mMaterialName = mImage.material != null ? mImage.material.name : null;
		}
		return mMaterialName;
	}
	public string getShaderName()
	{
		if (mShaderNameDirty)
		{
			mShaderNameDirty = false;
			mShaderName = mImage.material != null && mImage.material.shader != null ? mImage.material.shader.name : null;
		}
		return mShaderName;
	}
	public override void setAlpha(float alpha)
	{
		base.setAlpha(alpha);
		if (mImage == null)
		{
			return;
		}
		Color color = mImage.color;
		color.a = alpha;
		mImage.color = color;
	}
	public override float getAlpha() { return mImage.color.a; }
	public void setFillPercent(float percent) 
	{
		if (mImage == null)
		{
			return;
		}
		mImage.fillAmount = percent; 
	}
	public float getFillPercent() { return mImage.fillAmount; }
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
		setColor(new(color.x, color.y, color.z)); 
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
	public void registeColliderImage(Action clickCallback)
	{
		registeCollider(clickCallback, mDefaultClickSound);
		getOrAddComponent<COMWindowInteractiveFade>();
		mReceiveLayoutHide = true;
	}
	public void registeColliderImage(Action clickCallback, int clickSound)
	{
		registeCollider(clickCallback, clickSound);
		getOrAddComponent<COMWindowInteractiveFade>();
		mReceiveLayoutHide = true;
	}
}