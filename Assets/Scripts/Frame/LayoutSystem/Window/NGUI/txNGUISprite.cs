using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUISprite : txNGUIObject
{
	protected UISprite mSprite;
	protected string mOriginTextureName;    // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
	protected string mNormalSprite;
	protected string mPressSprite;
	protected string mHoverSprite;
	protected string mSelectedSprite;
	protected bool mSelected;
	protected bool mUseStateSprite;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		mSprite = getUnityComponent<UISprite>();
		mNormalSprite = getSpriteName();
		mPressSprite = mNormalSprite;
		mHoverSprite = mNormalSprite;
		mSelectedSprite = mNormalSprite;
		mOriginTextureName = mNormalSprite;
	}
	public UIAtlas getAtlas() { return mSprite.atlas; }
	public string getSpriteName() { return mSprite.spriteName; }
	public void setNormalSprite(string normalSprite, bool apply = true, bool resetStateSprites = true)
	{
		mUseStateSprite = true;
		mNormalSprite = normalSprite;
		if(apply)
		{
			setSpriteName(mNormalSprite);
		}
		if(resetStateSprites)
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
	public virtual void setAtlas(UIAtlas atlas, bool clearSprite = false)
	{
		if (mSprite.atlas != atlas)
		{
			mSprite.atlas = atlas;
		}
		if(clearSprite)
		{
			mSprite.spriteName = EMPTY_STRING;
		}
	}
	public void setSpriteName(string name, bool useSize = false)
	{
		if (name == mSprite.spriteName)
		{
			return;
		}
		mSprite.spriteName = name;
		if (useSize && !isEmpty(name))
		{
			setWindowSize(getSpriteSize());
		}
	}
	public Vector2 getSpriteSize()
	{
		UISpriteData spriteData = mSprite.GetAtlasSprite();
		if (spriteData != null)
		{
			return new Vector2(spriteData.width, spriteData.height);
		}
		return Vector2.zero;
	}
	public override Vector2 getWindowSize(bool transformed = false)
	{
		if (mSprite == null)
		{
			return Vector2.zero;
		}
		Vector2 textureSize = new Vector2(mSprite.width, mSprite.height);
		if (transformed)
		{
			Vector2 scale = getWorldScale();
			textureSize.x *= scale.x;
			textureSize.y *= scale.y;
		}
		return textureSize;
	}
	public override void setWindowSize(Vector2 size)
	{
		mSprite.SetDimensions((int)size.x, (int)size.y);
	}
	public override void setDepth(int depth)
	{
		mSprite.depth = depth;
		base.setDepth(depth);
	}
	public override int getDepth() { return mSprite.depth; }
	public void setColor(Color color) { mSprite.color = color; }
	public void setColor(Vector3 color) { mSprite.color = new Color(color.x, color.y, color.z); }
	public Color getColor() { return mSprite.color; }
	public override void setAlpha(float alpha, bool fadeChild) { mSprite.alpha = alpha; }
	public override float getAlpha() { return mSprite.alpha; }
	public override void setFillPercent(float percent) { mSprite.fillAmount = percent; }
	public override float getFillPercent() { return mSprite.fillAmount; }
	public UISprite getSprite() { return mSprite; }
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
	public override void onMouseEnter()
	{
		base.onMouseEnter();
		if(mUseStateSprite)
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
		if(mUseStateSprite)
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
		if(mUseStateSprite)
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
		if(mUseStateSprite)
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
		if(mUseStateSprite)
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
}

#endif