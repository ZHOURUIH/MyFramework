using System;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;
using static FrameUtility;

// 红点系统,用于处理红点相关的逻辑
public class RedPointSystem : FrameSystem
{
	protected Dictionary<int, RedPoint> mPointDictionary = new();	// 用于根据ID查找红点的列表
	protected List<RedPoint> mRootNodeList = new();					// 所有的根节点的红点列表
	protected int mRedPointIDSeed;									// 用于生成动态红点的ID,确保ID不冲突
	// 添加红点
	public RedPoint addRedPoint()
	{
		return addRedPoint(null, 0, null);
	}
	public T addRedPoint<T>() where T : RedPoint
	{
		return addRedPoint(typeof(T), 0, null) as T;
	}
	public T addRedPoint<T>(int id) where T : RedPoint
	{
		return addRedPoint(typeof(T), id, null) as T;
	}
	public T addRedPoint<T>(int id, int parentID) where T : RedPoint
	{
		return addRedPoint(typeof(T), id, getRedPoint(parentID)) as T;
	}
	public RedPoint addRedPoint(Type type)
	{
		return addRedPoint(type, 0, null);
	}
	public RedPoint addRedPoint(Type type, int id)
	{
		return addRedPoint(type, id, null);
	}
	public RedPoint addRedPoint(Type type, int id, int parentID)
	{
		return addRedPoint(type, id, getRedPoint(parentID));
	}
	public RedPoint addRedPoint(Type type, int id, RedPoint parent)
	{
		type ??= typeof(RedPoint);
		if (id == 0)
		{
			id = ++mRedPointIDSeed;
		}
		else
		{
			clampMin(ref mRedPointIDSeed, id);
		}
		if (mPointDictionary.ContainsKey(id))
		{
			logError("已经存在相同ID的红点了, ID:" + id);
			return null;
		}

		// 创建红点
		var node = CLASS<RedPoint>(type);
		node.setID(id);
		node.setParent(parent);
		node.init();
		mRootNodeList.addIf(node, parent == null);
		return mPointDictionary.add(id, node);
	}
	// 移除一个红点,以及其所有的子节点
	public void removeRedPoint(RedPoint point)
	{
		destroyRedPoint(point);
	}
	// 根据名字获取一个红点
	public RedPoint getRedPoint(int id) { return mPointDictionary.get(id); }
	// 刷新所有节点的状态
	public void refresh()
	{
		foreach (RedPoint point in mRootNodeList)
		{
			refreshRedPoint(point);
		}
	}
	// 由叶节点发起的状态改变的通知
	public void notifyRedPointChanged(RedPoint node)
	{
		if (node.getChildCount() > 0)
		{
			logError("只能由叶节点通知改变");
		}
		onRedPointChanged(node);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 当node发生改变时,通知node的父节点刷新
	protected void onRedPointChanged(RedPoint node)
	{
		RedPoint parent = node.getParent();
		if (parent == null)
		{
			return;
		}
		// 叶节点重写refresh，根节点refresh根据子节点刷新
		parent.refresh();
		// 递归向上通知
		onRedPointChanged(parent);
	}
	protected void refreshRedPoint(RedPoint node)
	{
		// 先递归刷新所有子节点的状态
		foreach (RedPoint point in node.getChildren())
		{
			refreshRedPoint(point);
		}
		// 再刷新当前节点的状态
		node.refresh();
	}
	// 销毁一个红点
	protected void destroyRedPoint(RedPoint node)
	{
		if (node == null)
		{
			return;
		}
		// 先销毁所有子节点
		var children = node.getChildren();
		for(int i = 0; i < children.Count; ++i)
		{
			destroyRedPoint(children[i]);
		}
		mPointDictionary.Remove(node.getID());
		node.setEnable(false);
		RedPoint parent = node.getParent();
		// 先将节点从父节点上取下,然后再销毁
		node.setParent(null);
		UN_CLASS(ref node);

		// 当前节点销毁时,需要通知父节点刷新
		if (parent != null)
		{
			// 叶节点重写refresh，根节点refresh根据子节点刷新
			parent.refresh();
			// 递归向上通知
			onRedPointChanged(parent);
		}
	}
}