#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TestAssert;

// StateGroup 单元测试
// addState / hasState / mStateList.Count
public static class StateGroupTest
{
	public static void Run()
	{
		testAddState();
		testHasState();
		testCount();
	}

	// ─── addState ────────────────────────────────────────────────────────

	private static void testAddState()
	{
		var group = new StateGroup();
		assertEqual(0, group.mStateList.Count);
		group.addState(typeof(int));
		assertEqual(1, group.mStateList.Count);
		assertTrue(group.hasState(typeof(int)));
	}

	// ─── hasState ────────────────────────────────────────────────────────

	private static void testHasState()
	{
		var group = new StateGroup();
		group.addState(typeof(int));
		assertTrue(group.hasState(typeof(int)));
		assertFalse(group.hasState(typeof(string)));
		assertEqual(1, group.mStateList.Count);
	}

	// ─── Count ───────────────────────────────────────────────────────────

	private static void testCount()
	{
		var group = new StateGroup();
		group.addState(typeof(int));
		group.addState(typeof(float));
		assertEqual(2, group.mStateList.Count);
		group.mStateList.Clear();
		assertEqual(0, group.mStateList.Count);
	}
}
#endif
