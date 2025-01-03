using UnityEngine;

// 监听鼠标事件,然后自动修改显示图片的button
public class myUGUIImageButton : myUGUIImage
{
	protected string mNormalSprite;		// 正常时的图片
	protected string mPressSprite;		// 按下时的图片
	protected string mHoverSprite;		// 悬停时的图片
	protected string mSelectedSprite;	// 选中时的图片
	protected bool mUseStateSprite;		// 状态改变时是否切换图片
	protected bool mSelected;			// 是否选中
	public override void init()
	{
		base.init();
		mNormalSprite = getSpriteName();
		mPressSprite = mNormalSprite;
		mHoverSprite = mNormalSprite;
		mSelectedSprite = mNormalSprite;
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
	public override void onMouseEnter(Vector3 mousePos, int touchID)
	{
		base.onMouseEnter(mousePos, touchID);
		if (mUseStateSprite)
		{
			setSpriteName(mSelected ? mSelectedSprite : mHoverSprite);
		}
	}
	public override void onMouseLeave(Vector3 mousePos, int touchID)
	{
		base.onMouseLeave(mousePos, touchID);
		if (mUseStateSprite)
		{
			setSpriteName(mSelected ? mSelectedSprite : mNormalSprite);
		}
	}
	public override void onMouseDown(Vector3 mousePos, int touchID)
	{
		base.onMouseDown(mousePos, touchID);
		if (mUseStateSprite)
		{
			setSpriteName(mSelected ? mSelectedSprite : mPressSprite);
		}
	}
	public override void onMouseUp(Vector3 mousePos, int touchID)
	{
		base.onMouseUp(mousePos, touchID);
		// 一般都会再mouseUp时触发点击消息,跳转界面,所以基类中可能会将当前窗口销毁,需要注意
		if (mUseStateSprite)
		{
			setSpriteName(mSelected ? mSelectedSprite : mNormalSprite);
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
			setSpriteName(mSelected ? mSelectedSprite : mNormalSprite);
		}
	}
}