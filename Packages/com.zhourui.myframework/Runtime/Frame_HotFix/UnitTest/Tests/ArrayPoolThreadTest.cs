using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// ArrayPoolThread 深度测试(2 的 n 次方数组线程池)
//   newArray: 从池中取(未使用列表有则复用, 无则新建); 非 2 次幂返回 null(固定 logError 不测)
//   destroyArray: 归还池中复用(destroyReally=false)
//   clearUnused: 清空未使用列表
// 环境: new ArrayPoolThread()(FrameSystem 子类, 直接 new)
public static class ArrayPoolThreadTest
{
	public static void Run()
	{
		testNewArrayPow2();
		testDestroyArrayReuse();
		testDestroyArrayNullSafe();
		testClearUnused();
		testGetUnusedListEmpty();
	}

	// newArray: 2 次幂大小创建成功
	private static void testNewArrayPow2()
	{
		ArrayPoolThread pool = new ArrayPoolThread();
		int[] array = pool.newArray<int>(8);
		assertTrue(array != null, "newArray(8) 返回非 null");
		assertEqual(8, array.Length, "数组长度 8");
		pool.destroyArray(ref array, true);
	}

	// destroyArray 后复用: 归还池中再取同一实例
	private static void testDestroyArrayReuse()
	{
		ArrayPoolThread pool = new ArrayPoolThread();
		int[] array = pool.newArray<int>(4);
		assertTrue(array != null, "newArray(4) 非 null");
		pool.destroyArray(ref array, false);   // 归还池中
		// 已使用列表有该数组(编辑器下)
		int[] reused = pool.newArray<int>(4);
		assertTrue(reused != null, "再次 newArray(4) 非 null");
		pool.destroyArray(ref reused, true);
	}

	// destroyArray null 安全
	private static void testDestroyArrayNullSafe()
	{
		ArrayPoolThread pool = new ArrayPoolThread();
		int[] array = null;
		pool.destroyArray(ref array, true);   // null 不崩
		pool.destroyArray(ref array, false);
	}

	// clearUnused: 只清空各队列内容, 字典 key 分组保留(Count 不变, 队列为空)
	private static void testClearUnused()
	{
		ArrayPoolThread pool = new ArrayPoolThread();
		int[] array = pool.newArray<int>(16);
		pool.destroyArray(ref array, false);   // 放入未使用
		pool.clearUnused();
		int totalUnused = 0;
		foreach (Dictionary<int, Queue<Array>> sizeMap in pool.getUnusedList().Values)
		{
			foreach (Queue<Array> queue in sizeMap.Values)
			{
				totalUnused += queue.Count;
			}
		}
		assertTrue(totalUnused == 0, "clearUnused 后所有未使用队列为空");
	}

	// 未使用列表初始为空
	private static void testGetUnusedListEmpty()
	{
		ArrayPoolThread pool = new ArrayPoolThread();
		assertTrue(pool.getUnusedList().Count == 0, "初始未使用列表为空");
	}
}
