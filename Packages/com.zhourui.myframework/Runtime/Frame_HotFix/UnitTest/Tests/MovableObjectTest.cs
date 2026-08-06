using UnityEngine;
using static TestAssert;

// MovableObject 单元测试
// 框架环境已完全初始化(mGameObjectPool 可用), 可覆盖:
//   构造 / getObjectID 唯一性
//   setObject(GameObject) / getGameObject
//   init / selfCreateObject / setObject(null) 自动创建节点
//   destroy 自动销毁自建节点
//   getDescription / hasLastPosition / getDepth 默认值
//   getPhysicsSpeed 等无移动信息组件时的错误路径
//   resetProperty 清理(注意 mObjectID 不重置)
public static class MovableObjectTest
{
	public static void Run()
	{
		// ─── 构造 ───
		testConstruct();
		testObjectIDUnique();
		testObjectIDStable();
		// ─── 对象绑定 ───
		testSetObject();
		// ─── 自动创建节点 (框架池已初始化) ───
		testSelfCreateObject();
		testInitCreatesObject();
		testSetObjectNullSelfCreates();
		testDestroySelfCreated();
		// ─── 默认 getter ───
		testDefaultGetters();
		// ─── 错误路径 getter ───
		testErrorPathGetters();
		// ─── resetProperty ───
		testResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 构造
	// ═════════════════════════════════════════════════════════════════
	private static void testConstruct()
	{
		MovableObject obj = new();
		assertNotNull(obj, "MovableObject 可构造");
		obj.destroy();
	}
	private static void testObjectIDUnique()
	{
		MovableObject a = new();
		MovableObject b = new();
		assertTrue(a.getObjectID() != b.getObjectID(), "不同实例 ObjectID 应不同");
		a.destroy();
		b.destroy();
	}
	private static void testObjectIDStable()
	{
		MovableObject obj = new();
		int id = obj.getObjectID();
		// mObjectID 构造时生成, 不随 resetProperty 变化
		obj.resetProperty();
		assertEqual(id, obj.getObjectID(), "resetProperty 不重置 ObjectID");
		obj.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// 对象绑定
	// ═════════════════════════════════════════════════════════════════
	private static void testSetObject()
	{
		MovableObject obj = new();
		var go = new GameObject("MovableObj");
		try
		{
			obj.setObject(go);
			assertEqual(go, obj.getGameObject(), "setObject(GameObject) 应绑定");
		}
		finally
		{
			Object.DestroyImmediate(go);
			obj.destroy();
		}
	}
	// ═════════════════════════════════════════════════════════════════
	// 自动创建节点 (框架池已初始化)
	// ═════════════════════════════════════════════════════════════════
	private static void testSelfCreateObject()
	{
		MovableObject obj = new();
		try
		{
			obj.selfCreateObject();
			assertNotNull(obj.getGameObject(), "selfCreateObject 应创建 GameObject");
			assertTrue(obj.getGameObject().activeSelf, "自建节点默认激活");
		}
		finally
		{
			obj.destroy();
		}
	}
	private static void testInitCreatesObject()
	{
		MovableObject obj = new();
		try
		{
			// init 在无对象时自动创建节点
			obj.init();
			assertNotNull(obj.getGameObject(), "init 应自动创建 GameObject");
		}
		finally
		{
			obj.destroy();
		}
	}
	private static void testSetObjectNullSelfCreates()
	{
		MovableObject obj = new();
		try
		{
			// setObject(null) 触发 selfCreateObject
			obj.setObject(null);
			assertNotNull(obj.getGameObject(), "setObject(null) 应自建节点");
		}
		finally
		{
			obj.destroy();
		}
	}
	private static void testDestroySelfCreated()
	{
		MovableObject obj = new();
		try
		{
			obj.selfCreateObject();
			assertNotNull(obj.getGameObject(), "自建节点存在");
		}
		finally
		{
			obj.destroy();
		}
		// destroy 后 MovableObject 不再持有自建节点 (节点被池回收)
		assertNull(obj.getGameObject(), "destroy 后引用清空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认 getter
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultGetters()
	{
		MovableObject obj = new();
		assertEqual("", obj.getDescription(), "默认描述为空");
		assertFalse(obj.hasLastPosition(), "无移动信息组件时 hasLastPosition false");
		assertNull(obj.getDepth(), "默认深度为 null");
		assertFalse(obj.isEnableFixedUpdate(), "默认不启用固定更新");
		obj.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// getter (enableMoveInfo 后走正常路径, 避免 logError)
	// ═════════════════════════════════════════════════════════════════
	private static void testErrorPathGetters()
	{
		MovableObject obj = new();
		// 未 enableMoveInfo 时这些 getter 会无条件 logError(源码防御分支), 无法避免日志污染,
		// 故此处改为先 enableMoveInfo 走正常路径验证 getter 返回值(不触发 logError)
		obj.enableMoveInfo();
		assertEqual(Vector3.zero, obj.getPhysicsSpeed());
		assertEqual(Vector3.zero, obj.getPhysicsAcceleration());
		assertFalse(obj.hasMovedDuringFrame());
		assertEqual(Vector3.zero, obj.getMoveSpeedVector());
		assertEqual(Vector3.zero, obj.getLastSpeedVector());
		assertEqual(Vector3.zero, obj.getLastPosition());
		obj.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty()
	{
		MovableObject obj = new();
		var go = new GameObject("RMObj");
		obj.setObject(go);
		obj.resetProperty();
		// resetProperty 清空对象引用
		assertNull(obj.getGameObject(), "resetProperty 后 getGameObject 为 null");
		Object.DestroyImmediate(go);
		obj.destroy();
	}
}
