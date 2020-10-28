using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class myNGUIShapeCircle : myNGUIShape
{
	protected NGUICircle mCircle;
	public void setRaidus(float radius, int detail = 0)
	{
		mCircle.setRadius(radius, detail);
		markChanged();
	}
	public override void cloneFrom(myUIObject obj)
	{
		base.cloneFrom(obj);
		myNGUIShapeCircle ori = obj as myNGUIShapeCircle;
		if (obj != null)
		{
			setRaidus(ori.mCircle.mRadius, ori.mCircle.mDetails);
		}
	}
	protected override void onWorldScaleChanged(Vector2 lastWorldScale)
	{
		// 根据世界缩放值,计算圆形的精细度
		Vector2 worldScale = getWorldScale();
		int detail = ceil(getLength(worldScale) / getLength(Vector2.one) * NGUICircle.DEFAULT_DETAIL);
		clamp(ref detail, 10, 100);
		setRaidus(mCircle.mRadius, detail);
	}
	public float getRadius() { return mCircle.mRadius; }
	//---------------------------------------------------------------------------------------------------------------
	protected override INGUIShape createShape()
	{
		return mCircle = new NGUICircle();
	}
}

#endif