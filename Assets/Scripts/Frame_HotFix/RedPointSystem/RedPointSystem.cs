using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;

// 红点系统,用于处理红点相关的逻辑

// 红点的使用方式主要关注4个点
// 1.红点的创建,红点应该尽早创建,以便在事件通知时能够被刷新状态
// 2.红点所关联的UI节点的绑定,用于显示的红点UI节点创建时,就应该与对应的红点对象绑定起来,以便正常更新是否显示
// 3.红点所关联的UI节点的取消绑定,当UI节点需要被销毁时,应该被取消绑定,比如在recycle,destroy中
// 4.红点的销毁,红点如果有所属对象的,当所属对象被销毁时,红点应该被销毁,没有所属对象的全局生命周期的红点可以不用销毁

// 也不是所有的看似红点的需求都可以通过此红点系统实现,部分情况下不适合
// 一个红点UI节点对应多个同类数据时,比如翻页显示时,当前页的奖励领取按钮的红点
// 红点的关联数据频繁变动时,比如显示当前页的前面所有页是否有奖励可领取的红点
public class RedPointSystem : FrameSystem
{
	protected List<RedPoint> mRedPointList = new();				// 所有的根节点的红点列表
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (RedPoint point in mRedPointList)
		{
			// 由于只有叶节点才会监听事件,改变刷新状态,所以这里只会去刷新叶节点
			if (point.isDirty())
			{
				point.setDirty(false);
				point.refresh();
				notifyRedPointChanged(point);
			}
		}
	}
	public T createRedPoint<T>() where T : RedPoint
	{
		return createRedPoint(typeof(T), null) as T;
	}
	public RedPoint createRedPoint()
	{
		return createRedPoint(null, null);
	}
	public T createRedPoint<T>(out T point) where T : RedPoint
	{
		point = createRedPoint(typeof(T), null) as T;
		return point;
	}
	public RedPoint createRedPoint(Type type)
	{
		return createRedPoint(type, null);
	}
	public T createRedPoint<T>(RedPoint parent) where T : RedPoint
	{
		return createRedPoint(typeof(T), parent) as T;
	}
	public RedPoint createRedPoint(RedPoint parent)
	{
		return createRedPoint(null, parent);
	}
	public T createRedPoint<T>(out T point, RedPoint parent) where T : RedPoint
	{
		point = createRedPoint(typeof(T), parent) as T;
		return point;
	}
	protected RedPoint createRedPoint(Type type, RedPoint parent)
	{
		type ??= typeof(RedPoint);
		// 创建红点
		var node = CLASS<RedPoint>(type);
		node.setParent(parent);
		node.init();
		mRedPointList.add(node);
		return node;
	}
	// 移除一个红点,以及其所有的子节点
	public void destroyRedPoint(RedPoint point)
	{
		destroyRedPointInternal(point);
	}
	// 移除列表中的红点,以及其所有的子节点
	public void destroyRedPoint<T>(ICollection<T> pointList) where T : RedPoint
	{
		foreach (RedPoint point in pointList)
		{
			destroyRedPointInternal(point);
		}
		if (pointList is List<T>)
		{
			pointList.Clear();
		}
	}
	// 刷新所有节点的状态
	public void refresh()
	{
		foreach (RedPoint point in mRedPointList)
		{
			// 只遍历根节点,里面会递归刷新所有子节点
			if (point.getParent() == null)
			{
				refreshRedPoint(point);
			}
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
	protected void destroyRedPointInternal(RedPoint node)
	{
		if (node == null)
		{
			return;
		}
		// 先销毁所有子节点,倒序遍历,因为里面会从列表中移除元素
		var children = node.getChildren();
		for(int i = children.Count - 1; i >= 0; --i)
		{
			destroyRedPointInternal(children[i]);
		}
		node.setEnable(false);
		RedPoint parent = node.getParent();
		// 先将节点从父节点上取下,然后再销毁
		node.setParent(null);
		mRedPointList.Remove(node);
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