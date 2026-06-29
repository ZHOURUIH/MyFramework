using System;
using System.Collections;
using System.Collections.Generic;

// 用于等待多个协程执行完成,可设置回调,适合在任意时刻使用
public class AsyncTaskGroup : ClassObject
{
	public List<IEnumerator> mEnumerators = new();	// 协程迭代器列表
	public Action mCallback;						// 所有协程完成的回调
	public override void resetProperty()
	{
		base.resetProperty();
		mEnumerators.Clear();
		mCallback = null;
	}
	public void setCallback(Action callback) { mCallback = callback; }
	public void addTask(IEnumerator enumerator)
	{
		mEnumerators.addNotNull(enumerator);
	}
	public bool checkDone()
	{
		foreach (IEnumerator item in mEnumerators)
		{
			if (item.MoveNext())
			{
				return false;
			}
		}
		mCallback?.Invoke();
		return true;
	}
}