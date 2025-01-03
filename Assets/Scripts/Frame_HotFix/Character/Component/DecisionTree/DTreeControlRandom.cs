using System;
using static MathUtility;

// 决策树控制节点,执行第一个满足条件的子节点
public class DTreeControlRandom : DTreeControl
{
	public override void execute()
	{
		using var a = new ListScope<DTreeNode>(out var availableChildList);
		Span<float> oddsList = stackalloc float[mChildList.Count];
		int count = 0;
		// 按子节点顺序查看子节点是否满足条件
		foreach (DTreeNode node in mChildList)
		{
			// 找出可以执行的节点
			if (node.isActive() && node.condition())
			{
				availableChildList.Add(node);
				oddsList[count++] = node.getRandomWeight();
			}
		}
		// 按照权重随机选择其中一个节点
		availableChildList.getSafe(randomHit(oddsList, count))?.execute();
	}
}