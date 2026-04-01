#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static FrameUtility;
using static TestAssert;

// FrameUtility core helper tests that do not depend on scene state.
public static class FrameUtilityCoreTest
{
	public static void Run()
	{
		testTickTimerLoop();
		testTickTimerOnce();
		testArrayHelpers();
		testIdHelpers();
		testEnumAndColorHelpers();
	}

	private static void testTickTimerLoop()
	{
		float timer = 1.0f;
		bool fired = tickTimerLoop(ref timer, 0.25f, 1.0f);
		assertFalse(fired, "tickTimerLoop should not fire before interval");
		assertEqual(0.75f, timer, "tickTimerLoop subtract elapsed time");

		fired = tickTimerLoop(ref timer, 0.8f, 1.0f);
		assertTrue(fired, "tickTimerLoop should fire when timer reaches zero");
		assert(timer > 0.0f && timer < 1.0f, "tickTimerLoop should carry leftover time");

		float ensureTimer = 0.1f;
		fired = tickTimerLoop(ref ensureTimer, 2.0f, 1.5f, true);
		assertTrue(fired, "tickTimerLoop ensureInterval should fire");
		assertEqual(1.5f, ensureTimer, "tickTimerLoop ensureInterval resets to interval");

		float stoppedTimer = -1.0f;
		fired = tickTimerLoop(ref stoppedTimer, 0.5f, 1.0f);
		assertFalse(fired, "tickTimerLoop should ignore stopped timer");
		assertEqual(-1.0f, stoppedTimer, "tickTimerLoop should not change stopped timer");
	}

	private static void testTickTimerOnce()
	{
		float timer = 1.0f;
		bool fired = tickTimerOnce(ref timer, 0.25f);
		assertFalse(fired, "tickTimerOnce should not fire before interval");
		assertEqual(0.75f, timer, "tickTimerOnce subtract elapsed time");

		fired = tickTimerOnce(ref timer, 0.75f);
		assertTrue(fired, "tickTimerOnce should fire when timer reaches zero");
		assertEqual(-1.0f, timer, "tickTimerOnce should stop after firing");

		float stoppedTimer = -1.0f;
		fired = tickTimerOnce(ref stoppedTimer, 0.1f);
		assertFalse(fired, "tickTimerOnce should ignore stopped timer");
		assertEqual(-1.0f, stoppedTimer, "tickTimerOnce should not change stopped timer");
	}

	private static void testArrayHelpers()
	{
		int[] values = { 1, 2, 3, 4 };
		removeElement(values, values.Length, 1);
		assertEqual(3, values[1], "removeElement should shift left");
		assertEqual(4, values[2], "removeElement should keep trailing order");

		string[] classValues = { "a", "b", "c", "b" };
		int classCount = removeClassElement(classValues, classValues.Length, "b");
		assertEqual(2, classCount, "removeClassElement should remove all matches");
		assertEqual("a", classValues[0], "removeClassElement first item");
		assertEqual("c", classValues[1], "removeClassElement second item");

		int[] valueValues = { 1, 2, 3, 2, 4 };
		int valueCount = removeValueElement(valueValues, valueValues.Length, 2);
		assertEqual(3, valueCount, "removeValueElement should remove all matches");
		assertEqual(1, valueValues[0], "removeValueElement first item");
		assertEqual(3, valueValues[1], "removeValueElement second item");
		assertEqual(4, valueValues[2], "removeValueElement third item");

		assertTrue(arrayContains(values, 3), "arrayContains should find existing item");
		assertFalse(arrayContains(values, 99), "arrayContains should reject missing item");
		assertTrue(arrayContains(values, 3, 2), "arrayContains should honor explicit length");
		assertFalse(arrayContains(values, 4, 2), "arrayContains should not scan past length");
	}

	private static void testIdHelpers()
	{
		int first = makeID();
		int second = makeID();
		assertEqual(first + 1, second, "makeID should increment");

		notifyIDUsed(second + 20);
		int next = makeID();
		assertEqual(second + 21, next, "notifyIDUsed should advance the seed");
	}

	private static void testEnumAndColorHelpers()
	{
		assertTrue(isEnumValid(CoreTestEnum.First), "isEnumValid should accept defined value");
		assertFalse(isEnumValid((CoreTestEnum)99), "isEnumValid should reject undefined value");

		string enoughColor = "#112233";
		assertEqual(enoughColor, getCountColor(true, enoughColor), "getCountColor should honor custom enough color");
		string notEnoughColor = getCountColor(false, enoughColor);
		assertFalse(string.IsNullOrEmpty(notEnoughColor), "getCountColor should return a value for not enough");
		assertFalse(notEnoughColor == enoughColor, "getCountColor false branch should not use enough color");
	}

	private enum CoreTestEnum
	{
		First = 1,
		Second = 2,
	}
}
#endif
