
// 决策树控制节点,用于决定怎么选择子节点
public class DTreeControl : DTreeNode
{
	public override void execute()
	{
		// 找到第一个满足条件的子节点来执行
		if (mChildList.find(node => node.isActive() && node.condition(), out DTreeNode node))
		{
			node.execute();
		}
	}
}