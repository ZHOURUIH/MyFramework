using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_NGUI

public class myNGUIScrollView : myNGUIObject
{
	protected UIScrollView     mScrollView;
	protected myUIObject       mGrid;
	protected List<myUIObject> mItemList;
	public myNGUIScrollView()
	{
		mItemList = new List<myUIObject>();
	}
	public override void init()
	{
		base.init();
		mScrollView = getUnityComponent<UIScrollView>();
		int childCount = getChildCount();
		for (int i = 0; i < childCount; ++i)
		{
			GameObject gridGo = mTransform.GetChild(i).gameObject;
			if(gridGo.GetComponent<UIGrid>() != null)
			{
				mLayout.getScript().newObject(out mGrid, this, gridGo.name);
				break;
			}
		}
		if (mGrid == null)
		{
			logError("scroll view window must have a child with grid compoent!");
		}
		// 查找grid下已经挂接的物体
		int itemCount = mGrid.getChildCount();
		for(int i = 0; i < itemCount; ++i)
		{
			GameObject child = mGrid.getChild(i);
			myUIObject item = mLayout.getScript().newObject(out item, mGrid, child.name);
			mItemList.Add(item);
		}
	}
	public void addItem<T>(string name) where T : myUIObject, new()
	{
		T item = mLayout.getScript().createObject<T>(mGrid, name, true);
		item.getObject().AddComponent<ScaleAnchor>();
		mItemList.Add(item);
	}
	public void addItem(myUIObject obj)
	{
		mItemList.Add(obj);
	}
	public void removeItem(int index)
	{
		if(index < 0 || index >= mItemList.Count)
		{
			return;
		}
		destroyGameObject(mItemList[index].getObject());
		mItemList.RemoveAt(index);
	}
	public void clearItem()
	{
		int itemCount = mItemList.Count;
		for (int i = 0; i < itemCount; ++i)
		{
			destroyGameObject(mItemList[i].getObject());
		}
		mItemList.Clear();
	}
}

#endif