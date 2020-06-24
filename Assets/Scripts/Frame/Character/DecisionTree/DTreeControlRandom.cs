using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 决策树控制节点,执行第一个满足条件的子节点
public class DTreeControlRandom : DTreeControl
{
	private List<DTreeNode> mTempAvailableChildList = new List<DTreeNode>();
	private List<float> mTempOddsList = new List<float>();
	public override void execute()
	{
		mTempAvailableChildList.Clear();
		mTempOddsList.Clear();
		// 按子节点顺序查看子节点是否满足条件
		foreach (var item in mChildList)
		{
			// 找出可以执行的节点
			if (item.isActive() && item.condition())
			{
				mTempAvailableChildList.Add(item);
				mTempOddsList.Add(item.getRandomWeight());
			}
		}
		// 按照权重随机选择其中一个节点
		int index = randomHit(mTempOddsList);
		if(isInRange(index, 0, mTempAvailableChildList.Count - 1, true))
		{
			mTempAvailableChildList[index].execute();
		}
	}
	//--------------------------------------------------------------------------------------------------------------
}