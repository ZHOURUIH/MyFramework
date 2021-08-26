using System;
using System.Collections.Generic;
using UnityEngine;

// 用于延迟调用指定的函数
public class CmdGlobalDelayCall : Command
{
	public Action mFunction;		// 延迟调用的函数
	public override void resetProperty()
	{
		base.resetProperty();
		mFunction = null;
	}
	public override void execute()
	{
		mFunction?.Invoke();
	}
}