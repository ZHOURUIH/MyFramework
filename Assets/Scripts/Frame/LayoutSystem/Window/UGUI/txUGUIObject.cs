using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 降序排序
public struct ChildDepthSort
{
	public static int compareZDecending(Transform a, Transform b)
	{
		return (int)MathUtility.sign(b.localPosition.z - a.localPosition.z);
	}
}

public class txUGUIObject : txUIObject
{
	protected float mDefaultAlpha;
	public override bool selfAlphaChild() { return false; }
	public override void setWindowSize(Vector2 size)
	{
		WidgetUtility.setUGUIRectSize(mRectTransform, size, false);
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
			setUIChildAlpha(mObject, alpha);
		}
	}
	public void setDepthInParent(int depth){mTransform.SetSiblingIndex(depth);}
	public int getDepthInParent(){return mTransform.GetSiblingIndex();}
	public void refreshChildDepthByPositionZ()
	{
		// z值越大的子节点越靠后
		List<Transform> tempList = mListPool.newList(out tempList);
		tempList.Clear();
		int childCount = getChildCount();
		for (int i = 0; i < childCount; ++i)
		{
			tempList.Add(mTransform.GetChild(i));
		}
		tempList.Sort(ChildDepthSort.compareZDecending);
		int count = tempList.Count;
		for (int i = 0; i < count; ++i)
		{
			tempList[i].SetSiblingIndex(i);
		}
		mListPool.destroyList(tempList);
	}
}