using System;
using static TestAssert;

// 补充覆盖 StateGroupMutex 各种互斥策略
public static class StateGroupMutexTest
{
	public static void Run()
	{
		testBaseGetterSetterAndReset();
		testCoexist();
		testRemoveOthers();
		testNoNew();
		testMutexWithMain();
		testMutexWithMainOnly();
		testMutexInverseMain();
	}

	private static void testBaseGetterSetterAndReset()
	{
		StateGroup group = makeGroup(typeof(MainState));
		StateGroupMutexCoexist mutex = new();
		mutex.setGroup(group);
		mutex.setMutexType(GROUP_MUTEX.MUTEX_WITH_MAIN);
		assertEqual(group, mutex.getGroup(), "组对象设置错误");
		assertEqual(GROUP_MUTEX.MUTEX_WITH_MAIN, mutex.getMutexType(), "互斥类型设置错误");
		mutex.resetProperty();
		assertNull(mutex.getGroup(), "reset 后组对象应清空");
		assertEqual(GROUP_MUTEX.COEXIST, mutex.getMutexType(), "reset 后互斥类型应回到 COEXIST");
	}

	private static void testCoexist()
	{
		StateGroupMutexCoexist mutex = attach(new StateGroupMutexCoexist(), typeof(MainState));
		assertTrue(mutex.allowKeepState(typeof(MainState), typeof(OtherState)));
		assertTrue(mutex.allowAddState(typeof(MainState), typeof(OtherState)));
	}

	private static void testRemoveOthers()
	{
		StateGroupMutexRemoveOthers mutex = attach(new StateGroupMutexRemoveOthers(), typeof(MainState));
		assertFalse(mutex.allowKeepState(typeof(MainState), typeof(OtherState)), "添加新状态时应移除已有状态");
		assertTrue(mutex.allowAddState(typeof(MainState), typeof(OtherState)), "允许添加新状态");
	}

	private static void testNoNew()
	{
		StateGroupMutexNoNew mutex = attach(new StateGroupMutexNoNew(), typeof(MainState));
		assertTrue(mutex.allowKeepState(typeof(MainState), typeof(OtherState)), "已有状态可保留");
		assertFalse(mutex.allowAddState(typeof(MainState), typeof(OtherState)), "存在状态时不允许添加新状态");
	}

	private static void testMutexWithMain()
	{
		StateGroupMutexMutexWithMain mutex = attach(new StateGroupMutexMutexWithMain(), typeof(MainState));
		assertFalse(mutex.allowKeepState(typeof(MainState), typeof(OtherState)), "添加主状态时应移除其他状态");
		assertTrue(mutex.allowKeepState(typeof(OtherState), typeof(MainState)), "添加非主状态时不由 keep 阶段移除主状态");
		assertFalse(mutex.allowAddState(typeof(OtherState), typeof(MainState)), "已有主状态时不可添加其他状态");
		assertTrue(mutex.allowAddState(typeof(OtherState), typeof(ThirdState)), "没有主状态时可添加其他状态");
	}

	private static void testMutexWithMainOnly()
	{
		StateGroupMutexMutexWithMainOnly mutex = attach(new StateGroupMutexMutexWithMainOnly(), typeof(MainState));
		assertFalse(mutex.allowKeepState(typeof(MainState), typeof(OtherState)), "添加主状态时应移除其他状态");
		assertTrue(mutex.allowKeepState(typeof(OtherState), typeof(MainState)), "添加非主状态时 keep 阶段不移除主状态");
		assertTrue(mutex.allowAddState(typeof(OtherState), typeof(MainState)), "该策略允许添加其他状态");
	}

	private static void testMutexInverseMain()
	{
		StateGroupMutexMutexInverseMain mutex = attach(new StateGroupMutexMutexInverseMain(), typeof(MainState));
		assertFalse(mutex.allowKeepState(typeof(OtherState), typeof(MainState)), "添加其他状态时应移除已有主状态");
		assertTrue(mutex.allowKeepState(typeof(MainState), typeof(OtherState)), "已有非主状态可保留");
		assertFalse(mutex.allowAddState(typeof(MainState), typeof(OtherState)), "有其他状态时不允许添加主状态");
		assertTrue(mutex.allowAddState(typeof(OtherState), typeof(MainState)), "允许添加非主状态");
	}

	private static T attach<T>(T mutex, Type mainState) where T : StateGroupMutex
	{
		mutex.setGroup(makeGroup(mainState));
		return mutex;
	}

	private static StateGroup makeGroup(Type mainState)
	{
		StateGroup group = new();
		group.setMainState(mainState);
		return group;
	}

	private class MainState { }
	private class OtherState { }
	private class ThirdState { }
}