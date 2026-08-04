using System;
using System.Collections;
using static TestAssert;

// AsyncTaskGroupManager 单元测试：createGroup/destroyGroup/update自动清理
public static class AsyncTaskGroupManagerTest
{
	public static void Run()
	{
		testCreateGroup();
		testCreateGroupWithCallback();
		testDestroyGroup();
		testDestroyGroupNotInList();
		testUpdateAutoDestroy();
		testUpdateNoAutoDestroyWhileRunning();
		testMultipleGroups();
		testCreateAndManualDestroy();
		testCreateGroupReturnsGroup();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static int sCallbackCount;

	private static IEnumerator createDoneEnumerator()
	{
		// 已经完成的迭代器：MoveNext 返回 false
		yield break;
	}

	private static IEnumerator createRunningEnumerator()
	{
		// 未完成的迭代器：MoveNext 返回 true
		yield return null;
	}

	private static void onGroupDone()
	{
		sCallbackCount++;
	}

	private static void testCreateGroup()
	{
		var manager = new AsyncTaskGroupManager();
		var group = manager.createGroup(null);
		assertNotNull(group, "createGroup 返回非 null");
		assertEqual(1, manager.mGroupList.count(), "mGroupList 应有1个 group");
		// 清理
		manager.destroyGroup(group);
	}

	private static void testCreateGroupWithCallback()
	{
		sCallbackCount = 0;
		var manager = new AsyncTaskGroupManager();
		var group = manager.createGroup(onGroupDone);
		assertNotNull(group, "createGroup 返回非 null");

		// 添加一个已完成的迭代器，checkDone 应返回 true
		group.addTask(createDoneEnumerator());
		assertTrue(group.checkDone(), "已完成迭代器 checkDone=true");
		assertEqual(1, sCallbackCount, "回调被调用1次");

		manager.destroyGroup(group);
	}

	private static void testDestroyGroup()
	{
		var manager = new AsyncTaskGroupManager();
		var group = manager.createGroup(null);
		assertEqual(1, manager.mGroupList.count(), "销毁前有1个");
		manager.destroyGroup(group);
		assertEqual(0, manager.mGroupList.count(), "销毁后0个");
	}

	private static void testDestroyGroupNotInList()
	{
		var manager = new AsyncTaskGroupManager();
		var group = manager.createGroup(null);
		manager.destroyGroup(group);
		assertEqual(0, manager.mGroupList.count(), "第一次销毁后0个");
		// 再次销毁同一个 group，不会崩溃
		manager.destroyGroup(group);
		assertEqual(0, manager.mGroupList.count(), "第二次销毁后仍0个");
	}

	private static void testUpdateAutoDestroy()
	{
		var manager = new AsyncTaskGroupManager();
		sCallbackCount = 0;
		var group = manager.createGroup(onGroupDone);
		// 添加已完成的迭代器
		group.addTask(createDoneEnumerator());
		assertEqual(1, manager.mGroupList.count(), "update前有1个");
		manager.update(0.016f);
		assertEqual(0, manager.mGroupList.count(), "update自动销毁后0个");
		assertEqual(1, sCallbackCount, "回调被调用1次");
	}

	private static void testUpdateNoAutoDestroyWhileRunning()
	{
		var manager = new AsyncTaskGroupManager();
		var group = manager.createGroup(null);
		// 添加未完成的迭代器
		group.addTask(createRunningEnumerator());
		assertEqual(1, manager.mGroupList.count(), "update前有1个");
		manager.update(0.016f);
		assertEqual(1, manager.mGroupList.count(), "update后仍有1个（未完成）");
		manager.destroyGroup(group);
	}

	private static void testMultipleGroups()
	{
		var manager = new AsyncTaskGroupManager();
		var groups = new AsyncTaskGroup[5];
		for (int i = 0; i < 5; i++)
		{
			groups[i] = manager.createGroup(null);
		}
		assertEqual(5, manager.mGroupList.count(), "应有5个 group");

		// 给其中3个添加已完成迭代器，另外2个添加未完成迭代器
		groups[0].addTask(createDoneEnumerator());
		groups[1].addTask(createRunningEnumerator());
		groups[2].addTask(createDoneEnumerator());
		groups[3].addTask(createRunningEnumerator());
		groups[4].addTask(createDoneEnumerator());

		manager.update(0.016f);
		// 3个已完成的被自动销毁，2个未完成的保留
		assertEqual(2, manager.mGroupList.count(), "自动销毁3个后剩2个");

		// 清理剩余
		manager.destroyGroup(groups[1]);
		manager.destroyGroup(groups[3]);
		assertEqual(0, manager.mGroupList.count(), "全部清理后0个");
	}

	private static void testCreateAndManualDestroy()
	{
		var manager = new AsyncTaskGroupManager();
		var group = manager.createGroup(null);
		assertEqual(1, manager.mGroupList.count(), "创建后有1个");
		manager.destroyGroup(group);
		assertEqual(0, manager.mGroupList.count(), "手动销毁后0个");
	}

	private static void testCreateGroupReturnsGroup()
	{
		var manager = new AsyncTaskGroupManager();
		var group1 = manager.createGroup(null);
		var group2 = manager.createGroup(null);
		assertNotNull(group1, "group1 不为 null");
		assertNotNull(group2, "group2 不为 null");
		assertFalse(ReferenceEquals(group1, group2), "两个 group 不同");
		manager.destroyGroup(group1);
		manager.destroyGroup(group2);
	}
}
