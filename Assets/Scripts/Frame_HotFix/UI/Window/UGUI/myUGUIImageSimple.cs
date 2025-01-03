using UnityEngine;
using UnityEngine.UI;

// 对UGUI的Image的封装,简化版,只有Image组件
public class myUGUIImageSimple : myUGUIObject
{
	protected Image mImage;									// 图片组件
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		if (!mObject.TryGetComponent(out mImage))
		{
			mImage = mObject.AddComponent<Image>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
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
	// 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
	// 并且需要自己保证设置不同图集的图片以后不会有什么问题
	public void setSpriteOnly(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
	{
		if (mImage == null || mImage.sprite == sprite)
		{
			return;
		}
		mImage.sprite = sprite;
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
	public void setMaterial(Material material) { mImage.material = material; }
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
		mImage.color = new(color.x, color.y, color.z); 
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
}