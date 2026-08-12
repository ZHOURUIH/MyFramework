using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// ByteArrayPoolThread 深度测试(2 的 n 次方字节数组线程池)
//   newArray: 取字节数组(非 2 次幂固定 logError 不测)
//   destroyArray: 归还池中 / destroyReally=true 交给 GC
//   clearUnused: 清空未使用列表
// 环境: new ByteArrayPoolThread()(FrameSystem 子类, 直接 new)
public static class ByteArrayPoolThreadTest
{
	public static void Run()
	{
		testNewArrayPow2();
		testDestroyArrayReuse();
		testDestroyArrayNullSafe();
		testClearUnused();
	}

	// newArray: 2 次幂大小创建成功
	private static void testNewArrayPow2()
	{
		ByteArrayPoolThread pool = new ByteArrayPoolThread();
		byte[] array = pool.newArray(32);
		assertTrue(array != null, "newArray(32) 返回非 null");
		assertEqual(32, array.Length, "数组长度 32");
		pool.destroyArray(ref array, true);
	}

	// destroyArray 后复用
	private static void testDestroyArrayReuse()
	{
		ByteArrayPoolThread pool = new ByteArrayPoolThread();
		byte[] array = pool.newArray(16);
		assertTrue(array != null, "newArray(16) 非 null");
		pool.destroyArray(ref array, false);   // 归还池中
		byte[] reused = pool.newArray(16);
		assertTrue(reused != null, "再次 newArray(16) 非 null");
		pool.destroyArray(ref reused, true);
	}

	// destroyArray null 安全
	private static void testDestroyArrayNullSafe()
	{
		ByteArrayPoolThread pool = new ByteArrayPoolThread();
		byte[] array = null;
		pool.destroyArray(ref array, true);   // null 不崩
		pool.destroyArray(ref array, false);
	}

	// clearUnused: 只清空各队列内容, 字典 key 分组保留(Count 不变, 队列为空)
	private static void testClearUnused()
	{
		ByteArrayPoolThread pool = new ByteArrayPoolThread();
		byte[] array = pool.newArray(64);
		pool.destroyArray(ref array, false);
		pool.clearUnused();
		int totalUnused = 0;
		foreach (Queue<byte[]> queue in pool.getUnusedList().Values)
		{
			totalUnused += queue.Count;
		}
		assertTrue(totalUnused == 0, "clearUnused 后所有未使用队列为空");
	}
}
