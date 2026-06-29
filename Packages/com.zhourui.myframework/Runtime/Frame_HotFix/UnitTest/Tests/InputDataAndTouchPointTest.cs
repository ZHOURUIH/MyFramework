using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

public static class InputDataAndTouchPointTest
{
	public static void Run()
	{
		testKeyListenInfoReset();
		testKeyMappingReset();
		testTouchPointDownMoveUpLateUpdate();
		testTouchPointDoubleClick();
		testTouchPointResetStateAndResetProperty();
		testDeadClickConstructor();
	}
	private static void testKeyListenInfoReset()
	{
		bool called = false;
		KeyListenInfo info = new();
		info.mCallback = () => called = true;
		info.mCombinationKey = COMBINATION_KEY.CTRL;
		info.mKey = KeyCode.A;
		info.resetProperty();
		assertNull(info.mCallback);
		assertNull(info.mListener);
		assertEqual(COMBINATION_KEY.NONE, info.mCombinationKey);
		assertEqual(KeyCode.None, info.mKey);
		assertFalse(called);
	}
	private static void testKeyMappingReset()
	{
		KeyMapping mapping = new();
		mapping.mMappingID = 100;
		mapping.mDefaultKey = KeyCode.Space;
		mapping.mKey = KeyCode.Return;
		mapping.mMappingName = "Jump";
		mapping.resetProperty();
		assertEqual(0, mapping.mMappingID);
		assertEqual(KeyCode.None, mapping.mDefaultKey);
		assertEqual(KeyCode.None, mapping.mKey);
		assertNull(mapping.mMappingName);
	}
	private static void testTouchPointDownMoveUpLateUpdate()
	{
		TouchPoint point = new();
		point.setTouchID(5);
		point.setMouse(true);
		point.pointDown(new Vector3(10, 20, 0));
		assertEqual(5, point.getTouchID());
		assertTrue(point.isMouse());
		assertTrue(point.isDown());
		assertTrue(point.isCurrentDown());
		assertEqual(new Vector3(10, 20, 0), point.getDownPosition());
		point.update(new Vector3(13, 25, 0));
		assertEqual(new Vector3(10, 20, 0), point.getLastPosition());
		assertEqual(new Vector3(13, 25, 0), point.getCurPosition());
		assertEqual(new Vector3(3, 5, 0), point.getMoveDelta());
		point.pointUp(new Vector3(13, 25, 0), new List<DeadClick>());
		assertTrue(point.isCurrentUp());
		assertFalse(point.isDown());
		assertTrue(point.isClick(), "移动距离小于 CLICK_LENGTH 时应认为是点击");
		assertFalse(point.isDoubleClick());
		point.lateUpdate();
		assertFalse(point.isCurrentDown());
		assertFalse(point.isCurrentUp());
		assertFalse(point.isClick());
		assertFalse(point.isDoubleClick());
	}
	private static void testTouchPointDoubleClick()
	{
		TouchPoint point = new();
		Vector3 pos = new(100, 100, 0);
		point.pointDown(pos);
		point.pointUp(pos, new List<DeadClick>{ new(pos) });
		assertTrue(point.isClick());
		assertTrue(point.isDoubleClick(), "与最近点击位置/时间接近时应识别为双击");
	}
	private static void testTouchPointResetStateAndResetProperty()
	{
		TouchPoint point = new();
		point.pointDown(Vector3.one);
		point.pointUp(Vector3.one, new List<DeadClick>());
		point.resetState();
		assertFalse(point.isCurrentUp());
		assertFalse(point.isDown());
		assertFalse(point.isClick());
		assertFalse(point.isDoubleClick());
		point.setTouchID(9);
		point.setMouse(true);
		point.resetProperty();
		assertEqual(0, point.getTouchID());
		assertFalse(point.isMouse());
		assertFalse(point.isDown());
		assertEqual(Vector3.zero, point.getCurPosition());
	}
	private static void testDeadClickConstructor()
	{
		Vector3 pos = new(1, 2, 3);
		DeadClick click = new(pos);
		assertEqual(pos, click.mClickPosition);
	}
}