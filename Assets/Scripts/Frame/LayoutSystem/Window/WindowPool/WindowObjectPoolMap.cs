using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WindowObjectPoolMap<Key, T> : FrameBase where T : PooledWindow
{
	protected Dictionary<Key, T> mUsedItemList;
	protected List<T> mUnusedItemList;
	protected LayoutScript mScript;
	protected myUIObject mItemParentInuse;
	protected myUIObject mItemParentUnuse;
	protected myUIObject mTemplate;
	protected Type mValueType;			// Value的类型,用于创建Value实例
	protected string mPreName;
	protected int mAssignIDSeed;        // 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	public WindowObjectPoolMap(LayoutScript script)
	{
		mScript = script;
		mUsedItemList = new Dictionary<Key, T>();
		mUnusedItemList = new List<T>();
	}
	public void destroy()
	{
		unuseAll();
		int count = mUnusedItemList.Count;
		for(int i = 0; i < count; ++i)
		{
			mUnusedItemList[i].destroy();
		}
		mUnusedItemList.Clear();
	}
	public void setNode(myUIObject parent, myUIObject template, Type valueType)
	{
		setNode(parent, parent, template, valueType);
	}
	public void setNode(myUIObject parentInuse, myUIObject parentUnuse, myUIObject template, Type valueType)
	{
		mItemParentInuse = parentInuse;
		mItemParentUnuse = parentUnuse;
		mTemplate = template;
		mValueType = valueType;
		mPreName = template.getName();
#if UNITY_EDITOR
		if(mValueType != null)
		{
			ConstructorInfo[] info = mValueType.GetConstructors();
			if (info != null)
			{
				bool hasNoneParamConstructor = false;
				int count = info.Length;
				for(int i= 0; i < count; ++i)
				{
					if (info[i].GetParameters().Length == 0)
					{
						hasNoneParamConstructor = true;
					}
				}
				if (!hasNoneParamConstructor && count > 0)
				{
					logError("WindowObjectPoolMap需要有无参构造的类作为节点类型, Type:" + mValueType.Name);
				}
			}
		}
#endif
	}
	public bool hasKey(Key key) { return mUsedItemList.ContainsKey(key); }
	public T getItem(Key key) 
	{
		if (mUsedItemList.TryGetValue(key, out T item))
		{
			return item;
		}
		return default;
	}
	public myUIObject getInUseParent() { return mItemParentInuse; }
	public Dictionary<Key, T> getUseList() { return mUsedItemList; }
	public void setItemPreName(string preName) { mPreName = preName; }
	public T newItem(Key key, bool asLastSibling = true)
	{
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList[mUnusedItemList.Count - 1];
			mUnusedItemList.RemoveAt(mUnusedItemList.Count - 1);
		}
		else
		{
			item = createInstance<T>(mValueType);
			item.setScript(mScript);
#if UNITY_EDITOR
			string name = mPreName + makeID();
#else
			string name = mPreName;
#endif
			item.assignWindow(mItemParentInuse, mTemplate, name);
			item.init();
		}
		item.setAssignID(++mAssignIDSeed);
		item.reset();
		item.setVisible(true);
		// 将asLastSibling作为是否排序子节点的标记,因为当渲染顺序敏感时,也会对子节点在列表中的顺序敏感
		item.setParent(mItemParentInuse, asLastSibling);
		if (asLastSibling)
		{
			item.setAsLastSibling();
		}
		mUsedItemList.Add(key, item);
		return item;
	}
	public void unuseAll()
	{
		foreach (var item in mUsedItemList)
		{
			item.Value.recycle();
			item.Value.setVisible(false);
			item.Value.setParent(mItemParentUnuse, false);
			item.Value.setAssignID(-1);
			mUnusedItemList.Add(item.Value);
		}
		mUsedItemList.Clear();
	}
	public void unuseItem(Key key, bool showError = true)
	{
		if (key == null)
		{
			return;
		}
		if (!mUsedItemList.TryGetValue(key, out T item))
		{
			if(showError)
			{
				logError("此窗口物体不属于当前窗口物体池,无法回收");
			}
			return;
		}
		item.recycle();
		item.setVisible(false);
		item.setParent(mItemParentUnuse, false);
		item.setAssignID(-1);
		mUnusedItemList.Add(item);
		mUsedItemList.Remove(key);
	}
}