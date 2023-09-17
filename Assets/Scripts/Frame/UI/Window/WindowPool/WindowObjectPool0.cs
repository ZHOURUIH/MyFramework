using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;

// 负责窗口对象池,效率稍微高一些,但是功能会比普通的WindowObjectPool少一点,UsedList是无序的
public class WindowObjectPool0<T> where T : PooledWindow
{
	protected HashSet<T> mUsedItemList;		// 正在使用的列表
	protected List<T> mUnusedItemList;		// 未使用列表
	protected LayoutScript mScript;			// 所属的布局脚本
	protected myUIObject mItemParent;		// 创建节点时默认的父节点
	protected myUIObject mTemplate;			// 创建节点的模板
	protected Type mObjectType;				// 物体类型
	protected string mPreName;				// 创建物体的名字前缀
	protected long mAssignIDSeed;			// 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	protected bool mNewItemMoveToLast;		// 新创建的物体是否需要放到父节点的最后,也就是是否在意其渲染顺序
	public WindowObjectPool0(LayoutScript script)
	{
		mScript = script;
		mUsedItemList = new HashSet<T>();
		mUnusedItemList = new List<T>();
		mNewItemMoveToLast = true;
	}
	public void destroy()
	{
		unuseAll();
		foreach(var item in mUnusedItemList)
		{
			item.destroy();
		}
		mUnusedItemList.Clear();
	}
	// 当使用ILRuntime热更时,T类型很可能就是一个热更工程中的类,在主工程中无法根据T获得真正的类型,所以只能由外部传进来类型
	public void init(myUIObject parent, myUIObject template, Type objectType, bool newItemToLast = true)
	{
		mItemParent = parent;
		mTemplate = template;
		mNewItemMoveToLast = newItemToLast;
		mPreName = template?.getName();
		mObjectType = objectType;
#if UNITY_EDITOR
		if(mObjectType != null)
		{
			ConstructorInfo[] info = mObjectType.GetConstructors();
			if (info != null)
			{
				bool hasNoneParamConstructor = false;
				int count = info.Length;
				for(int i = 0; i < count; ++i)
				{
					if (info[i].GetParameters().Length == 0)
					{
						hasNoneParamConstructor = true;
					}
				}
				if (!hasNoneParamConstructor && count > 0)
				{
					logError("WindowObjectPool0需要有无参构造的类作为节点类型, Type:" + mObjectType.Name);
				}
			}
		}
#endif
	}
	public myUIObject getInUseParent() { return mItemParent; }
	public void setItemPreName(string preName) { mPreName = preName; }
	public HashSet<T> getUsedList() { return mUsedItemList; }
	public void checkCapacity(int capacity)
	{
		int needCount = capacity - mUsedItemList.Count;
		for (int i = 0; i < needCount; ++i)
		{
			newItem();
		}
	}
	public T newItem(out T item)
	{
		return item = newItem(mItemParent);
	}
	public T newItem()
	{
		return newItem(mItemParent);
	}
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
	public T newItem(myUIObject parent)
	{
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList[mUnusedItemList.Count - 1];
			mUnusedItemList.RemoveAt(mUnusedItemList.Count - 1);
			item.setParent(parent, false);
		}
		else
		{
			item = createInstance<T>(mObjectType);
			item.setScript(mScript);
#if UNITY_EDITOR
			string name = mPreName + makeID();
#else
			string name = mPreName;
#endif
			item.assignWindow(parent, mTemplate, name);
			item.init();
		}
		item.setAssignID(++mAssignIDSeed);
		item.reset();
		item.setVisible(true);
		if (mNewItemMoveToLast)
		{
			item.setAsLastSibling(false);
		}
		mUsedItemList.Add(item);
		return item;
	}
	public void unuseAll()
	{
		foreach (var item in mUsedItemList)
		{
			item.recycle();
			item.setVisible(false);
			item.setAssignID(-1);
			mUnusedItemList.Add(item);
		}
		mUsedItemList.Clear();
	}
	public void unuseItem(T item)
	{
		if (Typeof(item) != mObjectType)
		{
			logError("物体类型与池类型不一致,无法回收");
			return;
		}
		if (item.getAssignID() <= 0)
		{
			logError("物体已经回收过了,无法重复回收");
			return;
		}
		if (!mUsedItemList.Remove(item))
		{
			logError("此窗口物体不属于当前窗口物体池,无法回收");
			return;
		}
		item.recycle();
		item.setVisible(false);
		item.setAssignID(-1);
		mUnusedItemList.Add(item);
	}
}