using System;
using UnityEngine;

[Serializable]
public sealed class FastUIInventoryBenchmarkAtlas
{
	public Texture2D mTexture;
	public Sprite mSlotBackground;
	public Sprite mQualityFrame;
	public Sprite mBindIcon;
	public Sprite mLockIcon;
	public Sprite mSelected;
	public Sprite mHeaderBackground;
	public Sprite mTabBackground;
	public Sprite mDetailBackground;
	public Sprite[] mItemIcons;
	public Sprite getItemIcon(int index)
	{
		if (mItemIcons == null || mItemIcons.Length == 0)
		{
			return null;
		}
		int normalized = index % mItemIcons.Length;
		if (normalized < 0)
		{
			normalized += mItemIcons.Length;
		}
		return mItemIcons[normalized];
	}
}
