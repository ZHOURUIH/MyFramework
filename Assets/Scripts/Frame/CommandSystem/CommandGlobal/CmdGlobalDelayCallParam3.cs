using System;
using System.Collections.Generic;
using UnityEngine;

// 用于延迟调用指定的函数
public class CmdGlobalDelayCallParam3<T0, T1, T2> : Command
{
	public Action<T0, T1, T2> mFunction;
	public T0 mParam0;
	public T1 mParam1;
	public T2 mParam2;
	public override void resetProperty()
	{
		base.resetProperty();
		mFunction = null;
		mParam0 = default;
		mParam1 = default;
		mParam2 = default;
	}
	public override void execute()
	{
		mFunction?.Invoke(mParam0, mParam1, mParam2);
	}
}