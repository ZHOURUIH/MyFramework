using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 决策树控制节点,执行第一个满足条件的子节点
public class DTreeControlRandom : DTreeControl
{
	public override void execute()
	{
		List<DTreeNode> availableChildList = newList(out availableChildList);
		List<float> oddsList = newList(out oddsList);
		// 按子节点顺序查看子节点是否满足条件
		int count = mChildList.Count;
		for (int i = 0; i < count; ++i)
		{
			DTreeNode node = mChildList[i];
			// 找出可以执行的节点
			if (node.isActive() && node.condition())
			{
				availableChildList.Add(node);
				oddsList.Add(node.getRandomWeight());
			}
		}
		// 按照权重随机选择其中一个节点
		int index = randomHit(oddsList);
		if (inRange(index, 0, availableChildList.Count - 1, true))
		{
			availableChildList[index].execute();
		}
		destroyList(availableChildList);
		destroyList(oddsList);
	}
	//--------------------------------------------------------------------------------------------------------------
}