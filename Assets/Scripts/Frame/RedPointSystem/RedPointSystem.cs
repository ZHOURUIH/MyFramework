using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RedPointSystem : FrameSystem
{
	public Dictionary<string, RedPoint> mPointDictionary;	// 用于根据名字查找红点的列表
	public List<RedPoint> mRootNodeList;					// 所有的根节点的红点列表
	public RedPointSystem()
	{
		mPointDictionary = new Dictionary<string, RedPoint>();
		mRootNodeList = new List<RedPoint>();
	}
	public override void init()
	{
		base.init();
	}
	// 添加非叶节点的红点
	public RedPoint addRedPoint(string name, RedPoint parent = null)
	{
		return addRedPoint<RedPoint>(name, parent);
	}
	// 添加叶节点的红点
	public T addRedPoint<T>(string name, RedPoint parent = null) where T : RedPoint
	{
		CLASS(out T node);
		node.setName(name);
		node.setParent(parent);
		node.init();
		mPointDictionary.Add(name, node);
		if (parent == null)
		{
			mRootNodeList.Add(node);
		}
		return node;
	}
	// 移除一个红点,以及其所有的子节点
	public void removeNode(string name)
	{
		destroyNode(getNode(name));
	}
	// 根据名字获取一个红点
	public RedPoint getNode(string name)
	{
		mPointDictionary.TryGetValue(name, out RedPoint point);
		return point;
	}
	// 刷新所有节点的状态
	public void refresh()
	{
		for(int i = 0; i < mRootNodeList.Count; ++i)
		{
			refreshNode(mRootNodeList[i]);
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
	protected void onRedPointChanged(RedPoint node)
	{
		if (node.getParent() == null)
		{
			return;
		}
		// 叶节点重写refresh，根节点refresh根据子节点刷新
		node.getParent().refresh();
		// 递归向上通知
		onRedPointChanged(node.getParent());
	}
	protected void refreshNode(RedPoint node)
	{
		// 先递归刷新所有子节点的状态
		var children = node.getChildren();
		for(int i = 0; i < children.Count; ++i)
		{
			refreshNode(children[i]);
		}
		// 再刷新当前节点的状态
		node.refresh();
	}
	// 销毁一个红点
	protected void destroyNode(RedPoint node)
	{
		// 先销毁所有子节点
		var children = node.getChildren();
		for(int i = 0; i < children.Count; ++i)
		{
			destroyNode(children[i]);
		}
		mPointDictionary.Remove(node.getName());
		node.setEnable(false);
		// 先将节点从父节点上取下,然后再销毁
		node.setParent(null);
		node.destroy();
		UN_CLASS(node);
	}
}