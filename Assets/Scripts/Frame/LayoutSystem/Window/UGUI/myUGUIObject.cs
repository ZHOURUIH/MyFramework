using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class myUGUIObject : myUIObject
{
	protected RectTransform mRectTransform;
	protected float mDefaultAlpha;
	protected static Comparison<Transform> mCompareDescend = compareZDecending;
	public override void init()
	{
		base.init();
		mRectTransform = mObject.GetComponent<RectTransform>();
		// 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
		if (mRectTransform != null)
		{
			mTransform = mRectTransform;
		}
		if (mBoxCollider != null && mRectTransform != null)
		{
			mBoxCollider.size = mRectTransform.rect.size;
			mBoxCollider.center = multiVector2(mRectTransform.rect.size, new Vector2(0.5f, 0.5f) - mRectTransform.pivot);
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 确保RectTransform和BoxCollider一样大
		if (mRectTransform != null && mBoxCollider != null)
		{
			if (!isFloatEqual(mRectTransform.rect.width, mBoxCollider.size.x) || !isFloatEqual(mRectTransform.rect.height, mBoxCollider.size.y))
			{
				mBoxCollider.size = mRectTransform.rect.size;
				mBoxCollider.center = multiVector2(mRectTransform.rect.size, new Vector2(0.5f, 0.5f) - mRectTransform.pivot);
			}
		}
	}
	public RectTransform getRectTransform() { return mRectTransform; }
	public override bool selfAlphaChild() { return false; }
	public override void setWindowSize(Vector2 size)
	{
		setRectSize(mRectTransform, size, false);
	}
	public override Vector2 getWindowSize(bool transformed = false)
	{
		Vector2 windowSize = mRectTransform.rect.size;
		if (transformed)
		{
			windowSize = multiVector2(windowSize, getWorldScale());
		}
		return windowSize;
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		if(fadeChild)
		{
			setUGUIChildAlpha(mObject, alpha);
		}
	}
	public void refreshChildDepthByPositionZ()
	{
		// z值越大的子节点越靠后
		LIST(out List<Transform> tempList);
		tempList.Clear();
		int childCount = getChildCount();
		for (int i = 0; i < childCount; ++i)
		{
			tempList.Add(mTransform.GetChild(i));
		}
		quickSort(tempList, mCompareDescend);
		int count = tempList.Count;
		for (int i = 0; i < count; ++i)
		{
			tempList[i].SetSiblingIndex(i);
		}
		UN_LIST(tempList);
	}
	//--------------------------------------------------------------------------------------------------------
	protected static int compareZDecending(Transform a, Transform b)
	{
		return (int)sign(b.localPosition.z - a.localPosition.z);
	}
}