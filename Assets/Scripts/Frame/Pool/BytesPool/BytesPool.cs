using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BytesPool : FrameSystem
{
	protected Dictionary<int, List<byte[]>> mInusedList;
	protected Dictionary<int, Stack<byte[]>> mUnusedList;
	protected ThreadLock mListLock;
	public BytesPool()
	{
		mInusedList = new Dictionary<int, List<byte[]>>();
		mUnusedList = new Dictionary<int, Stack<byte[]>>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<BytesPoolDebug>();
#endif
	}
	public Dictionary<int, List<byte[]>> getInusedList() { return mInusedList; }
	public Dictionary<int, Stack<byte[]>> getUnusedList() { return mUnusedList; }
	public byte[] newBytes(int size)
	{
		if (!isPow2(size))
		{
			logError("只有长度为2的n次方的数组才能使用BytesPool");
			return null;
		}
		byte[] bytes = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.ContainsKey(size) && mUnusedList[size].Count > 0)
			{
				bytes = mUnusedList[size].Pop();
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				bytes = new byte[size];
			}
			// 标记为已使用
			addInuse(bytes);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return bytes;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyBytes(byte[] bytes, bool destroyReally = false)
	{
		if (bytes == null)
		{
			return;
		}
		mListLock.waitForUnlock();
		try
		{
			if(!destroyReally)
			{
				addUnuse(bytes);
			}
			removeInuse(bytes);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(byte[] bytes)
	{
		int length = bytes.Length;
		if (mInusedList.ContainsKey(length))
		{
			if (mInusedList[length].Contains(bytes))
			{
				logError("bytes is in inuse list!");
				return;
			}
		}
		else
		{
			mInusedList.Add(length, new List<byte[]>());
		}
		// 加入使用列表
		mInusedList[length].Add(bytes);
	}
	protected void removeInuse(byte[] bytes)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		int length = bytes.Length;
		if (!mInusedList.ContainsKey(length))
		{
			logError("can not find size in Inused List! size : " + length);
			return;
		}
		if (!mInusedList[length].Remove(bytes))
		{
			logError("Inused List not contains size!");
			return;
		}
	}
	protected void addUnuse(byte[] bytes)
	{
		// 加入未使用列表
		int length = bytes.Length;
		if (mUnusedList.ContainsKey(length))
		{
			if (mUnusedList[length].Contains(bytes))
			{
				logError("bytes is in Unused list! can not add again!");
				return;
			}
		}
		else
		{
			mUnusedList.Add(length, new Stack<byte[]>());
		}
		mUnusedList[length].Push(bytes);
	}
}