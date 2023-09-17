using System;
using static UnityUtility;

// 决策树决策节点,用于发送具体的行为命令
public abstract class DTreeDecision : DTreeNode
{
	public override void execute(){}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void notifyAttachParent(DTreeNode parent)
	{
		if(parent is DTreeDecision)
		{
			logError("决策节点不能挂接到决策节点类型下");
		}
	}
}