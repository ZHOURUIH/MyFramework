using System;
using System.Collections.Generic;
using UnityEngine;

// 用于延迟调用指定的函数
public class CmdGlobalDelayCallParam1<T> : Command
{
	public Action<T> mFunction;		// 延迟调用的函数
	public T mParam;				// 函数的参数
	public override void resetProperty()
	{
		base.resetProperty();
		mFunction = null;
		mParam = default;
	}
	public override void execute()
	{
		mFunction?.Invoke(mParam);
	}
}