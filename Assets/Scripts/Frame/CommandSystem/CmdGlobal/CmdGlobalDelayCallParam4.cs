using System;
using System.Collections.Generic;
using UnityEngine;

// 用于延迟调用指定的函数
public class CmdGlobalDelayCallParam4<T0, T1, T2, T3> : Command
{
	public Action<T0, T1, T2, T3> mFunction;	// 延迟调用的函数
	public T0 mParam0;							// 函数的参数0
	public T1 mParam1;							// 函数的参数1
	public T2 mParam2;							// 函数的参数2
	public T3 mParam3;							// 函数的参数3
	public override void resetProperty()
	{
		base.resetProperty();
		mFunction = null;
		mParam0 = default;
		mParam1 = default;
		mParam2 = default;
		mParam3 = default;
	}
	public override void execute()
	{
		mFunction?.Invoke(mParam0, mParam1, mParam2, mParam3);
	}
}