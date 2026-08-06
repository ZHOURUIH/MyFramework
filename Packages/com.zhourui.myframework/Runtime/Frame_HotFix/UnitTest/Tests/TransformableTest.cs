using UnityEngine;
using static TestAssert;

// Transformable 单元测试
// 通过构造 GameObject + Transform 的方式在 EditMode 测试:
//   setObject / getGameObject / getTransform
//   setPosition/getPosition / setWorldPosition/getWorldPosition
//   setRotation/getRotation / setScale/getScale
//   setPositionX/Y/Z / setScaleX
//   move / resetTransform
//   modify 回调触发 / 移除回调
//   setNeedUpdate / isNeedUpdate / setActive / isActive
//   getSiblingIndex / getChildCount / getChild
//   setParent / isChildOf / getWorldScale
// 注: 依赖 Renderer/物理的 setAlpha/getCollider/canUpdate 需运行时, 部分覆盖
public static class TransformableTest
{
	public static void Run()
	{
		// ─── 对象绑定 ───
		testSetObject();
		testSetObjectNull();
		testGetGameObjectTransform();
		// ─── 位置 ───
		testPosition();
		testWorldPosition();
		testPositionComponents();
		// ─── 旋转 ───
		testRotation();
		// ─── 缩放 ───
		testScale();
		testScaleFloat();
		// ─── 变换操作 ───
		testMove();
		testResetTransform();
		// ─── 回调 ───
		testPositionModifyCallback();
		testRemovePositionModifyCallback();
		testScaleModifyCallback();
		// ─── 活动/更新 ───
		testNeedUpdate();
		testActive();
		// ─── 层级 ───
		testSiblingAndChild();
		testSetParent();
		testWorldScale();
		// ─── resetProperty ───
		testResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 对象绑定
	// ═════════════════════════════════════════════════════════════════
	private static void testSetObject()
	{
		Transformable t = new();
		var go = new GameObject("TestObj");
		try
		{
			t.setObject(go);
			assertEqual(go, t.getGameObject());
			assertEqual(go.transform, t.getTransform());
			// setName 会同步修改 GameObject 名
			t.setName("Renamed");
			assertEqual("Renamed", go.name, "setName 应同步 GameObject.name");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testSetObjectNull()
	{
		Transformable t = new();
		t.setObject(new GameObject("Tmp"));
		t.setObject(null);
		assertNull(t.getGameObject(), "setObject(null) 后 getGameObject 为 null");
		assertNull(t.getTransform(), "setObject(null) 后 getTransform 为 null");
		t.destroy();
	}
	private static void testGetGameObjectTransform()
	{
		Transformable t = new();
		assertNull(t.getGameObject(), "未绑定前为 null");
		assertNull(t.getTransform(), "未绑定前 transform 为 null");
		t.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// 位置
	// ═════════════════════════════════════════════════════════════════
	private static void testPosition()
	{
		Transformable t = new();
		var go = new GameObject("PosObj");
		try
		{
			t.setObject(go);
			t.setPosition(new Vector3(1f, 2f, 3f));
			assertEqual(1f, t.getPosition().x);
			assertEqual(2f, t.getPosition().y);
			assertEqual(3f, t.getPosition().z);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testWorldPosition()
	{
		Transformable t = new();
		var go = new GameObject("WorldPosObj");
		try
		{
			t.setObject(go);
			t.setWorldPosition(new Vector3(5f, 6f, 7f));
			assertEqual(5f, t.getWorldPosition().x);
			assertEqual(6f, t.getWorldPosition().y);
			assertEqual(7f, t.getWorldPosition().z);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testPositionComponents()
	{
		Transformable t = new();
		var go = new GameObject("PosCom");
		try
		{
			t.setObject(go);
			t.setPositionX(10f);
			assertEqual(10f, t.getPosition().x);
			t.setPositionY(20f);
			assertEqual(20f, t.getPosition().y);
			t.setPositionZ(30f);
			assertEqual(30f, t.getPosition().z);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 旋转
	// ═════════════════════════════════════════════════════════════════
	private static void testRotation()
	{
		Transformable t = new();
		var go = new GameObject("RotObj");
		try
		{
			t.setObject(go);
			t.setRotation(new Vector3(0f, 90f, 0f));
			assertEqual(0f, t.getRotation().x, 0.01f);
			assertEqual(90f, t.getRotation().y, 0.01f);
			assertEqual(0f, t.getRotation().z, 0.01f);
			// Quaternion 重载
			t.setRotation(Quaternion.Euler(0f, 45f, 0f));
			assertEqual(45f, t.getRotation().y, 0.1f);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 缩放
	// ═════════════════════════════════════════════════════════════════
	private static void testScale()
	{
		Transformable t = new();
		var go = new GameObject("ScaleObj");
		try
		{
			t.setObject(go);
			t.setScale(new Vector3(2f, 3f, 4f));
			assertEqual(2f, t.getScale().x);
			assertEqual(3f, t.getScale().y);
			assertEqual(4f, t.getScale().z);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testScaleFloat()
	{
		Transformable t = new();
		var go = new GameObject("ScaleF");
		try
		{
			t.setObject(go);
			t.setScale(2.5f);
			assertEqual(2.5f, t.getScale().x);
			assertEqual(2.5f, t.getScale().y);
			assertEqual(2.5f, t.getScale().z);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 变换操作
	// ═════════════════════════════════════════════════════════════════
	private static void testMove()
	{
		Transformable t = new();
		var go = new GameObject("MoveObj");
		try
		{
			t.setObject(go);
			t.setPosition(Vector3.zero);
			t.move(new Vector3(1f, 0f, 0f));
			assertEqual(1f, t.getPosition().x, 0.001f);
			assertEqual(0f, t.getPosition().y, 0.001f);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testResetTransform()
	{
		Transformable t = new();
		var go = new GameObject("ResetObj");
		try
		{
			t.setObject(go);
			t.setPosition(new Vector3(1f, 2f, 3f));
			t.setScale(new Vector3(2f, 2f, 2f));
			t.resetTransform();
			assertEqual(0f, t.getPosition().x, 0.001f);
			assertEqual(0f, t.getPosition().y, 0.001f);
			assertEqual(0f, t.getPosition().z, 0.001f);
			assertEqual(1f, t.getScale().x, 0.001f);
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 回调
	// ═════════════════════════════════════════════════════════════════
	private static void testPositionModifyCallback()
	{
		Transformable t = new();
		var go = new GameObject("CbObj");
		try
		{
			t.setObject(go);
			int calls = 0;
			t.addPositionModifyCallback(() => ++calls);
			t.setPosition(new Vector3(0f, 1f, 0f));
			assertEqual(1, calls, "setPosition 应触发位置回调");
			// 相同值不会触发
			t.setPosition(new Vector3(0f, 1f, 0f));
			assertEqual(1, calls, "位置未变不应触发回调");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testRemovePositionModifyCallback()
	{
		Transformable t = new();
		var go = new GameObject("RmCb");
		try
		{
			t.setObject(go);
			int calls = 0;
			System.Action cb = () => ++calls;
			t.addPositionModifyCallback(cb);
			t.setPosition(new Vector3(0f, 2f, 0f));
			t.removePositionModifyCallback(cb);
			t.setPosition(new Vector3(0f, 3f, 0f));
			assertEqual(1, calls, "移除回调后不再触发");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testScaleModifyCallback()
	{
		Transformable t = new();
		var go = new GameObject("ScaleCb");
		try
		{
			t.setObject(go);
			int calls = 0;
			t.addScaleModifyCallback(() => ++calls);
			t.setScale(new Vector3(3f, 3f, 3f));
			assertEqual(1, calls, "setScale 应触发缩放回调");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 活动/更新
	// ═════════════════════════════════════════════════════════════════
	private static void testNeedUpdate()
	{
		Transformable t = new();
		var go = new GameObject("NeedUpd");
		try
		{
			t.setObject(go);
			assertTrue(t.isNeedUpdate(), "默认需要更新");
			t.setNeedUpdate(false);
			assertFalse(t.isNeedUpdate());
			t.setNeedUpdate(true);
			assertTrue(t.isNeedUpdate());
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testActive()
	{
		Transformable t = new();
		var go = new GameObject("ActiveObj");
		try
		{
			t.setObject(go);
			assertTrue(t.isActive(), "绑定后默认 active");
			t.setActive(false);
			assertFalse(t.isActive(), "setActive(false) 后 isActive false");
			assertFalse(go.activeSelf, "GameObject 也应被禁用");
			t.setActive(true);
			assertTrue(t.isActive());
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 层级
	// ═════════════════════════════════════════════════════════════════
	private static void testSiblingAndChild()
	{
		Transformable t = new();
		var go = new GameObject("Parent");
		try
		{
			t.setObject(go);
			assertEqual(0, t.getChildCount(), "初始无子节点");
			assertNull(t.getChild(0), "无子节点时 getChild 返回 null");
			var child = new GameObject("Child");
			try
			{
				child.transform.SetParent(go.transform);
				assertEqual(1, t.getChildCount(), "添加子节点后计数为1");
				assertEqual(child, t.getChild(0));
			}
			finally
			{
				Object.DestroyImmediate(child);
			}
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testSetParent()
	{
		Transformable t = new();
		var parent = new GameObject("Par");
		var child = new GameObject("Chi");
		try
		{
			t.setObject(child);
			t.setParent(parent);
			assertEqual(parent.transform, child.transform.parent, "setParent 后父节点正确");
			// 重复设置相同父节点不抛异常
			t.setParent(parent);
		}
		finally
		{
			Object.DestroyImmediate(parent);
			Object.DestroyImmediate(child);
			t.destroy();
		}
	}
	private static void testWorldScale()
	{
		Transformable t = new();
		var go = new GameObject("WS");
		try
		{
			t.setObject(go);
			t.setScale(new Vector3(2f, 2f, 2f));
			assertEqual(2f, t.getWorldScale().x, 0.001f, "无父节点时 world scale = local scale");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty()
	{
		Transformable t = new();
		var go = new GameObject("RpObj");
		t.setObject(go);
		t.setPosition(new Vector3(1f, 1f, 1f));
		t.setNeedUpdate(false);
		t.addPositionModifyCallback(() => { });
		t.resetProperty();
		assertNull(t.getGameObject(), "reset 后对象清空");
		assertNull(t.getTransform(), "reset 后 transform 清空");
		assertTrue(t.isNeedUpdate(), "reset 后恢复需要更新");
		Object.DestroyImmediate(go);
		t.destroy();
	}
}
