using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 决策树控制节点,用于决定怎么选择子节点
public class DTreeControl : DTreeNode
{
	public override void execute()
	{
		// 按子节点顺序查看子节点是否满足条件
		foreach (var item in mChildList)
		{
			// 找到一个满足条件的子节点就不再继续遍历
			if(item.isActive() && item.condition())
			{
				item.execute();
				break;
			}
		}
	}
}