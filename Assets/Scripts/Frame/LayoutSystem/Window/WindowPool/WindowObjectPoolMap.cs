using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowObjectPoolMap<Key, Value> : GameBase where Value : PooledWindow, new()
{
	protected Dictionary<Key, Value> mUsedItemList;
	protected List<Value> mUnusedItemList;
	protected LayoutScript mScript;
	protected txUIObject mItemParentInuse;
	protected txUIObject mItemParentUnuse;
	protected txUIObject mTemplate;
	protected string mPreName;
	protected int mAssignIDSeed;        // 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	public WindowObjectPoolMap(LayoutScript script)
	{
		mScript = script;
		mUsedItemList = new Dictionary<Key, Value>();
		mUnusedItemList = new List<Value>();
	}
	public void destroy()
	{
		unuseAll();
		foreach (var item in mUnusedItemList)
		{
			item.destroy();
		}
		mUnusedItemList.Clear();
	}
	public void setNode(txUIObject parent, txUIObject template)
	{
		setNode(parent, parent, template);
	}
	public void setNode(txUIObject parentInuse, txUIObject parentUnuse, txUIObject template)
	{
		mItemParentInuse = parentInuse;
		mItemParentUnuse = parentUnuse;
		mTemplate = template;
		mPreName = template.getName();
	}
	public bool hasKey(Key key) { return mUsedItemList.ContainsKey(key); }
	public Value getItem(Key key) 
	{
		if(mUsedItemList.ContainsKey(key))
		{
			return mUsedItemList[key];
		}
		return default;
	}
	public Dictionary<Key, Value> getUseList() { return mUsedItemList; }
	public void setItemPreName(string preName) { mPreName = preName; }
	public Value getOneUnusedItem(Key key)
	{
		Value item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList[mUnusedItemList.Count - 1];
			mUnusedItemList.RemoveAt(mUnusedItemList.Count - 1);
		}
		else
		{
			item = new Value();
			item.setScript(mScript);
			item.assignWindow(mItemParentInuse, mTemplate, mPreName + UnityUtility.makeID());
			item.init();
		}
		item.setAssignID(++mAssignIDSeed);
		item.reset();
		item.setVisible(true);
		item.setParent(mItemParentInuse);
		mUsedItemList.Add(key, item);
		return item;
	}
	public void unuseAll()
	{
		foreach (var item in mUsedItemList)
		{
			item.Value.setVisible(false);
			item.Value.setParent(mItemParentUnuse);
			item.Value.setAssignID(-1);
			mUnusedItemList.Add(item.Value);
		}
		mUsedItemList.Clear();
	}
	public void unuseItem(Key key)
	{
		if (key != null && mUsedItemList.ContainsKey(key))
		{
			Value item = mUsedItemList[key];
			item.setVisible(false);
			item.setParent(mItemParentUnuse);
			item.setAssignID(-1);
			mUnusedItemList.Add(item);
			mUsedItemList.Remove(key);
		}
	}
}