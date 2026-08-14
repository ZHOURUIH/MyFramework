using UnityEngine;
using static FrameDefine;
using static TestAssert;

// GameObjectPool: GameObject 对象池(局部 new 不调 init, mObject=null, 纯池逻辑可测)
// 语义: newObject 复用 unused 否则 new+setNormalProperty; destroyObject 入 unused + (moveToHide→FAR_POSITION / false→SetActive(false))
// 清理: 池内对象经 destroyObject 回收后由 clearUnused 一次性销毁, 绝不二次 DestroyImmediate
public static class GameObjectPoolTest
{
	public static void Run()
	{
		testNewObjectName();
		testNewObjectEmptyName();
		testDestroyReuse();
		testDestroyMoveToHide();
		testDestroySetActiveFalse();
		testReuseResetsName();
		testClearUnused();
		testInuseTracking();
	}

	// newObject(name) 创建 + 命名 + 激活
	private static void testNewObjectName()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("Cube");
			assertTrue(go != null, "newObject 非空");
			assertEqual("Cube", go.name, "newObject 设置名字");
			assertTrue(go.activeSelf, "newObject 后激活");
			pool.destroyObject(go, false);
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// 空名字不改名(保持 Unity 默认名 "New Game Object")
	private static void testNewObjectEmptyName()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("");
			assertEqual("New Game Object", go.name, "空名字不改名(保持默认 New Game Object)");
			pool.destroyObject(go, false);
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// 销毁后复用同一实例
	private static void testDestroyReuse()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("A");
			GameObject saved = go;
			pool.destroyObject(go, false);
			GameObject reused = pool.newObject("A");
			assertTrue(ReferenceEquals(reused, saved), "销毁后复用同一实例");
			pool.destroyObject(reused, false);
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// moveToHide=true: 移到远处(不隐藏 active), 位置=FAR_POSITION
	private static void testDestroyMoveToHide()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("X");
			pool.destroyObject(go, true);
			assertTrue(go.transform.localPosition.isEqual(FAR_POSITION, 0.01f), "moveToHide 后位置=FAR_POSITION");
			assertTrue(go.activeSelf, "moveToHide 不改变 active(仍激活)");
			// 复用后位置重置
			GameObject reused = pool.newObject("Y");
			assertTrue(reused.transform.localPosition.isEqual(Vector3.zero, 0.01f), "复用后位置重置为 0");
			pool.destroyObject(reused, false);
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// moveToHide=false: SetActive(false)
	private static void testDestroySetActiveFalse()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("X");
			pool.destroyObject(go, false);
			assertFalse(go.activeSelf, "moveToHide=false 后 SetActive(false)");
			// 复用后重新激活
			GameObject reused = pool.newObject("X");
			assertTrue(reused.activeSelf, "复用后重新激活");
			pool.destroyObject(reused, false);
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// 复用路径重设名字
	private static void testReuseResetsName()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("OldName");
			pool.destroyObject(go, false);
			GameObject reused = pool.newObject("NewName");
			assertEqual("NewName", reused.name, "复用后名字重设为 NewName");
			pool.destroyObject(reused, false);
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// clearUnused: 清空未使用队列
	private static void testClearUnused()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("A");
			pool.destroyObject(go, false);
			assertEqual(1, pool.getUnusedList().Count, "销毁后 unused 1");
			pool.clearUnused();
			assertEqual(0, pool.getUnusedList().Count, "clearUnused 后 unused 0");
		}
		finally
		{
			pool.clearUnused();
		}
	}

	// 编辑器下 inuse 跟踪: 创建入 inuse, 销毁移出
	private static void testInuseTracking()
	{
		GameObjectPool pool = new GameObjectPool();
		try
		{
			GameObject go = pool.newObject("A");
			assertTrue(pool.getInuseList().Contains(go), "创建后在 inuse 列表");
			pool.destroyObject(go, false);
			assertFalse(pool.getInuseList().Contains(go), "销毁后移出 inuse 列表");
		}
		finally
		{
			pool.clearUnused();
		}
	}
}
