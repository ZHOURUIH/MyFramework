using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_NGUI

public class txNGUIScrollView : txNGUIObject
{
	protected UIScrollView     mScrollView;
	protected txUIObject       mGrid;
	protected List<txUIObject> mItemList;
	public txNGUIScrollView()
	{
		mItemList = new List<txUIObject>();
	}
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
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
			txUIObject item = mLayout.getScript().newObject(out item, mGrid, child.name);
			mItemList.Add(item);
		}
	}
	public void addItem<T>(string name) where T : txUIObject, new()
	{
		T item = mLayout.getScript().createObject<T>(mGrid, name, true);
		item.getObject().AddComponent<ScaleAnchor>();
		mItemList.Add(item);
	}
	public void addItem(txUIObject obj)
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