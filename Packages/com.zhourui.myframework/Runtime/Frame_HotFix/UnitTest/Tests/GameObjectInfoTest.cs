using System.Collections.Generic;
using static TestAssert;

// GameObjectInfo 测试：resetProperty / setter/getter / isUsing / destroy
public static class GameObjectInfoTest
{
	public static void Run()
	{
		testDefaultState();
		testSetPool();
		testSetObject();
		testSetTag();
		testSetUsing();
		testSetMoveToHide();
		testResetProperty();
		testSetFileWithPath();
		testGetFileWithPathDefault();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testDefaultState()
	{
		var info = new GameObjectInfo();
		assertNull(info.getPool(), "默认 pool=null");
		assertNull(info.getObject(), "默认 object=null");
		assertEqual(0, info.getTag(), "默认 tag=0");
		assertFalse(info.isUsing(), "默认 using=false");
		assertFalse(info.isMoveToHide(), "默认 moveToHide=false");
		assertNull(info.getFileWithPath(), "默认 fileWithPath=null");
	}

	private static void testSetPool()
	{
		var info = new GameObjectInfo();
		var pool = new PrefabPool();
		info.setPool(pool);
		assertEqual(pool, info.getPool(), "setPool 后 getPool 返回相同对象");
	}

	private static void testSetObject()
	{
		var info = new GameObjectInfo();
		var go = new UnityEngine.GameObject("TestGO");
		info.setObject(go);
		assertEqual(go, info.getObject(), "setObject 后 getObject 返回相同对象");
		UnityEngine.Object.DestroyImmediate(go);
	}

	private static void testSetTag()
	{
		var info = new GameObjectInfo();
		info.setTag(42);
		assertEqual(42, info.getTag(), "setTag(42) 后 getTag=42");
		info.setTag(0);
		assertEqual(0, info.getTag(), "setTag(0) 后 getTag=0");
		info.setTag(-1);
		assertEqual(-1, info.getTag(), "负 tag 也可设置");
	}

	private static void testSetUsing()
	{
		var info = new GameObjectInfo();
		assertFalse(info.isUsing(), "默认 not using");
		info.setUsing(true);
		assertTrue(info.isUsing(), "setUsing(true)");
		info.setUsing(false);
		assertFalse(info.isUsing(), "setUsing(false)");
	}

	private static void testSetMoveToHide()
	{
		var info = new GameObjectInfo();
		assertFalse(info.isMoveToHide(), "默认 moveToHide=false");
		info.setMoveToHide(true);
		assertTrue(info.isMoveToHide(), "setMoveToHide(true)");
		info.setMoveToHide(false);
		assertFalse(info.isMoveToHide(), "setMoveToHide(false)");
	}

	private static void testResetProperty()
	{
		var info = new GameObjectInfo();
		var pool = new PrefabPool();
		var go = new UnityEngine.GameObject("ResetTest");
		info.setPool(pool);
		info.setObject(go);
		info.setTag(100);
		info.setUsing(true);
		info.setMoveToHide(true);

		info.resetProperty();
		assertNull(info.getPool(), "reset 后 pool=null");
		assertNull(info.getObject(), "reset 后 object=null");
		assertEqual(0, info.getTag(), "reset 后 tag=0");
		assertFalse(info.isUsing(), "reset 后 using=false");
		assertFalse(info.isMoveToHide(), "reset 后 moveToHide=false");
		UnityEngine.Object.DestroyImmediate(go);
	}

	private static void testSetFileWithPath()
	{
		var info = new GameObjectInfo();
		// 通过反射或 createObject 间接测试 fileWithPath
		// createObject 需要 prefab，通过反射设置
		var field = typeof(GameObjectInfo).GetField("mFileWithPath",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(info, "Prefabs/TestPrefab");
		assertEqual("Prefabs/TestPrefab", info.getFileWithPath(), "反射设置 fileWithPath 后 getFileWithPath 返回相同值");
	}

	private static void testGetFileWithPathDefault()
	{
		var info = new GameObjectInfo();
		assertNull(info.getFileWithPath(), "默认 getFileWithPath=null");
	}
}
