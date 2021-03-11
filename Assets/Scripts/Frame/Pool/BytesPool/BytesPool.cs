using System.Collections.Generic;

// 只能在主线程中使用的字节数组池
public class BytesPool : FrameSystem
{
	protected Dictionary<int, HashSet<byte[]>> mInusedList;
	protected Dictionary<int, HashSet<byte[]>> mUnusedList;
	public BytesPool()
	{
		mInusedList = new Dictionary<int, HashSet<byte[]>>();
		mUnusedList = new Dictionary<int, HashSet<byte[]>>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<BytesPoolDebug>();
#endif
	}
	public Dictionary<int, HashSet<byte[]>> getInusedList() { return mInusedList; }
	public Dictionary<int, HashSet<byte[]>> getUnusedList() { return mUnusedList; }
	public byte[] newBytes(int size)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用BytesPoolMain");
			return null;
		}
		if (!isPow2(size))
		{
			logError("只有长度为2的n次方的数组才能使用BytesPoolMain");
			return null;
		}
		byte[] bytes = null;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(size, out HashSet<byte[]> bytesLis) && bytesLis.Count > 0)
		{
			foreach(var item in bytesLis)
			{
				bytes = item;
				break;
			}
			bytesLis.Remove(bytes);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			bytes = new byte[size];
		}
		// 标记为已使用
		addInuse(bytes);
		return bytes;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyBytes(byte[] bytes, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用BytesPoolMain");
			return;
		}
		if (bytes == null)
		{
			return;
		}
		if(!destroyReally)
		{
			addUnuse(bytes);
		}
		removeInuse(bytes);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(byte[] bytes)
	{
		int length = bytes.Length;
		if (mInusedList.TryGetValue(length, out HashSet<byte[]> bytesList))
		{
			if (bytesList.Contains(bytes))
			{
				logError("bytes is in inuse list!");
				return;
			}
		}
		else
		{
			bytesList = new HashSet<byte[]>();
			mInusedList.Add(length, bytesList);
		}
		// 加入使用列表
		bytesList.Add(bytes);
	}
	protected void removeInuse(byte[] bytes)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		int length = bytes.Length;
		if (!mInusedList.TryGetValue(length, out HashSet<byte[]> bytesList))
		{
			logError("can not find size in Inused List! size : " + length);
			return;
		}
		if (!bytesList.Remove(bytes))
		{
			logError("Inused List not contains size!");
			return;
		}
	}
	protected void addUnuse(byte[] bytes)
	{
		// 加入未使用列表
		int length = bytes.Length;
		if (mUnusedList.TryGetValue(length, out HashSet<byte[]> bytesList))
		{
			if (bytesList.Contains(bytes))
			{
				logError("bytes is in Unused list! can not add again!");
				return;
			}
		}
		else
		{
			bytesList = new HashSet<byte[]>();
			mUnusedList.Add(length, bytesList);
		}
		bytesList.Add(bytes);
	}
}