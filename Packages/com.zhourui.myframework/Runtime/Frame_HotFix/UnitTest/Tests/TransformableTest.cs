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
		// ─── 方向向量 ───
		testLeftRightBack();
		testRotateWorld();
		testRotateAroundWorld();
		testLookAtPoint();
		// ─── 组件查询 ───
		testGetRotationQuaternion();
		testIsActiveInHierarchy();
		testIsUnityComponentEnabled();
		testGetOrAddUnityComponent();
		testGetUnityComponentInChild();
		testGetAlpha();
		// ─── 世界变换 ───
		testGetWorldRotation();
		testSetWorldScale();
		testGetUnityObject();
		testGetColliderInChild();
		testRaycastSelfNoCollider();
		testRemoveRotationModifyCallback();
		testRemoveScaleModifyCallback();
		testRemoveWorldScaleModifyCallback();
		testGetUnityComponentsInChild();
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
		GameObject tmp = new("Tmp");
		try
		{
			t.setObject(tmp);
			t.setObject(null);
			assertNull(t.getGameObject(), "setObject(null) 后 getGameObject 为 null");
			assertNull(t.getTransform(), "setObject(null) 后 getTransform 为 null");
		}
		finally
		{
			// setObject(null) 后 mObject 已置空, t.destroy() 不会销毁 Tmp —— 必须手动销毁
			Object.DestroyImmediate(tmp);
			t.destroy();
		}
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
	// 方向向量
	// ═════════════════════════════════════════════════════════════════
	private static void testLeftRightBack()
	{
		Transformable t = new();
		var go = new GameObject("DirObj");
		try
		{
			t.setObject(go);
			// 无旋转时: right=(1,0,0), left=(-1,0,0), back=(0,0,-1)
			assertEqual(-1f, t.getLeft().x, 0.001f, "无旋转 getLeft.x 应为 -1");
			assertEqual(1f, t.getRight().x, 0.001f, "无旋转 getRight.x 应为 1");
			assertEqual(-1f, t.getBack().z, 0.001f, "无旋转 getBack.z 应为 -1");
			// ignoreY 归一化
			Vector3 leftY = t.getLeft(true);
			assertEqual(0f, leftY.y, 0.001f, "ignoreY 后 y 应为 0");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testRotateWorld()
	{
		Transformable t = new();
		var go = new GameObject("RotW");
		try
		{
			t.setObject(go);
			t.rotateWorld(new Vector3(0f, 90f, 0f));
			assertEqual(90f, t.getRotation().y, 0.01f, "rotateWorld 应改变 y 旋转");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testRotateAroundWorld()
	{
		Transformable t = new();
		var go = new GameObject("RotA");
		try
		{
			t.setObject(go);
			t.rotateAroundWorld(Vector3.up, 90f);
			assertEqual(90f, t.getRotation().y, 0.01f, "rotateAroundWorld 绕 y 轴旋转 90 度");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testLookAtPoint()
	{
		Transformable t = new();
		var go = new GameObject("LookAt");
		try
		{
			t.setObject(go);
			t.setPosition(Vector3.zero);
			// 朝 +x 方向观察, 应产生非零旋转
			t.lookAtPoint(new Vector3(10f, 0f, 0f));
			Quaternion q = t.getRotationQuaternion();
			// 朝向 +x, 旋转应非 identity
			assertTrue(q != Quaternion.identity, "lookAtPoint 应产生旋转");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组件查询
	// ═════════════════════════════════════════════════════════════════
	private static void testGetRotationQuaternion()
	{
		Transformable t = new();
		assertEqual(Quaternion.identity, t.getRotationQuaternion(), "未绑定对象时 getRotationQuaternion 返回 identity");
		var go = new GameObject("RotQ");
		try
		{
			t.setObject(go);
			t.setRotation(Quaternion.Euler(0f, 45f, 0f));
			// transform.localRotation 往返可能产生 ULP 尾数差, 用四分量容差比较而非 assertEqual<T>(精确 Equals)
			Quaternion expected = Quaternion.Euler(0f, 45f, 0f);
			Quaternion actualRot = t.getRotationQuaternion();
			assertEqual(expected.x, actualRot.x, 0.0001f, "getRotationQuaternion.x");
			assertEqual(expected.y, actualRot.y, 0.0001f, "getRotationQuaternion.y");
			assertEqual(expected.z, actualRot.z, 0.0001f, "getRotationQuaternion.z");
			assertEqual(expected.w, actualRot.w, 0.0001f, "getRotationQuaternion.w");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testIsActiveInHierarchy()
	{
		Transformable t = new();
		assertFalse(t.isActiveInHierarchy(), "未绑定对象时 isActiveInHierarchy 为 false");
		var go = new GameObject("ActiveH");
		try
		{
			t.setObject(go);
			assertTrue(t.isActiveInHierarchy(), "绑定激活对象时 isActiveInHierarchy 为 true");
			go.SetActive(false);
			assertFalse(t.isActiveInHierarchy(), "GameObject 禁用后 isActiveInHierarchy 为 false");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testIsUnityComponentEnabled()
	{
		Transformable t = new();
		var go = new GameObject("CompEn");
		try
		{
			t.setObject(go);
			// isUnityComponentEnabled<T> 约束 where T : Behaviour, 需用继承 Behaviour 的组件(AudioSource), 不能用 BoxCollider
			var aud = go.AddComponent<AudioSource>();
			assertTrue(t.isUnityComponentEnabled<AudioSource>(), "组件存在且启用时 isUnityComponentEnabled 为 true");
			aud.enabled = false;
			assertFalse(t.isUnityComponentEnabled<AudioSource>(), "组件禁用后 isUnityComponentEnabled 为 false");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testGetOrAddUnityComponent()
	{
		Transformable t = new();
		var go = new GameObject("GetOrAdd");
		try
		{
			t.setObject(go);
			// 不存在时添加
			var col1 = t.getOrAddUnityComponent<BoxCollider>();
			assertNotNull(col1, "getOrAddUnityComponent 应添加组件");
			// 已存在时复用
			var col2 = t.getOrAddUnityComponent<BoxCollider>();
			assertEqual(col1, col2, "getOrAddUnityComponent 已存在组件应复用");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testGetUnityComponentInChild()
	{
		Transformable t = new();
		var parent = new GameObject("ParentC");
		try
		{
			t.setObject(parent);
			var child = new GameObject("ChildC");
			try
			{
				child.transform.SetParent(parent.transform);
				var col = child.AddComponent<BoxCollider>();
				// 在子节点中查找组件
				var found = t.getUnityComponentInChild<BoxCollider>(true);
				assertEqual(col, found, "getUnityComponentInChild 应找到子节点组件");
			}
			finally
			{
				Object.DestroyImmediate(child);
			}
		}
		finally
		{
			Object.DestroyImmediate(parent);
			t.destroy();
		}
	}
	private static void testGetAlpha()
	{
		Transformable t = new();
		var go = new GameObject("Alpha");
		try
		{
			t.setObject(go);
			// 无 Renderer 时返回默认 1.0f
			assertEqual(1.0f, t.getAlpha(), 0.001f, "无 Renderer 时 getAlpha 返回 1.0f");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 世界变换
	// ═════════════════════════════════════════════════════════════════
	private static void testGetWorldRotation()
	{
		Transformable t = new();
		assertEqual(Vector3.zero, t.getWorldRotation(), "未绑定对象时 getWorldRotation 返回 zero");
		assertEqual(Quaternion.identity, t.getWorldQuaternionRotation(), "未绑定对象时 getWorldQuaternionRotation 返回 identity");
		var go = new GameObject("WorldRot");
		try
		{
			t.setObject(go);
			t.setWorldRotation(new Vector3(0f, 90f, 0f));
			assertEqual(90f, t.getWorldRotation().y, 0.01f, "setWorldRotation 后 getWorldRotation 应反映 y 旋转");
			// Quaternion 不能直接 assertEqual(泛型 Equals 是精确位比较, Euler/Transform 计算路径尾数可能差 ULP)
			// 用四分量容差比较
			Quaternion expect = Quaternion.Euler(0f, 90f, 0f);
			Quaternion actualQ = t.getWorldQuaternionRotation();
			assertEqual(expect.x, actualQ.x, 0.0001f, "getWorldQuaternionRotation.x");
			assertEqual(expect.y, actualQ.y, 0.0001f, "getWorldQuaternionRotation.y");
			assertEqual(expect.z, actualQ.z, 0.0001f, "getWorldQuaternionRotation.z");
			assertEqual(expect.w, actualQ.w, 0.0001f, "getWorldQuaternionRotation.w");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testSetWorldScale()
	{
		Transformable t = new();
		var go = new GameObject("WorldScale");
		try
		{
			t.setObject(go);
			t.setWorldScale(new Vector3(3f, 3f, 3f));
			// 无父节点时 world scale 直接等于 localScale
			assertEqual(3f, t.getScale().x, 0.001f, "无父节点时 setWorldScale 应设置 localScale");
			assertEqual(3f, t.getWorldScale().x, 0.001f, "getWorldScale 应返回 3");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testGetUnityObject()
	{
		Transformable t = new();
		var go = new GameObject("UnityObj");
		try
		{
			t.setObject(go);
			assertEqual(go, t.getUnityObject(), "getUnityObject 应返回绑定对象");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testGetColliderInChild()
	{
		Transformable t = new();
		var parent = new GameObject("ParentCol");
		try
		{
			t.setObject(parent);
			// 无 Collider 时返回 null
			assertNull(t.getColliderInChild(), "无 Collider 时 getColliderInChild 返回 null");
			var child = new GameObject("ChildCol");
			try
			{
				child.transform.SetParent(parent.transform);
				var col = child.AddComponent<BoxCollider>();
				assertEqual(col, t.getColliderInChild(), "子节点含 Collider 时 getColliderInChild 应找到");
			}
			finally
			{
				Object.DestroyImmediate(child);
			}
		}
		finally
		{
			Object.DestroyImmediate(parent);
			t.destroy();
		}
	}
	private static void testRaycastSelfNoCollider()
	{
		Transformable t = new();
		var go = new GameObject("Raycast");
		try
		{
			t.setObject(go);
			// 无 Collider 时 raycastSelf 返回 false
			Ray ray = new(Vector3.zero, Vector3.forward);
			bool hit = t.raycastSelf(ref ray, out var hitInfo, 100f);
			assertFalse(hit, "无 Collider 时 raycastSelf 返回 false");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testRemoveRotationModifyCallback()
	{
		Transformable t = new();
		var go = new GameObject("RmRotCb");
		try
		{
			t.setObject(go);
			int calls = 0;
			System.Action cb = () => ++calls;
			t.addRotationModifyCallback(cb);
			t.setRotation(new Vector3(0f, 10f, 0f));
			t.removeRotationModifyCallback(cb);
			t.setRotation(new Vector3(0f, 20f, 0f));
			assertEqual(1, calls, "移除旋转回调后不再触发");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	private static void testRemoveScaleModifyCallback()
	{
		Transformable t = new();
		var go = new GameObject("RmScaleCb");
		try
		{
			t.setObject(go);
			int calls = 0;
			System.Action cb = () => ++calls;
			t.addScaleModifyCallback(cb);
			t.setScale(new Vector3(2f, 2f, 2f));
			t.removeScaleModifyCallback(cb);
			t.setScale(new Vector3(3f, 3f, 3f));
			assertEqual(1, calls, "移除缩放回调后不再触发");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testRemoveWorldScaleModifyCallback()
	{
		Transformable t = new();
		var go = new GameObject("RmWS");
		try
		{
			t.setObject(go);
			int calls = 0;
			System.Action cb = () => ++calls;
			t.addWorldScaleModifyCallback(cb);
			t.setWorldScale(new Vector3(2f, 2f, 2f));
			t.removeWorldScaleModifyCallback(cb);
			t.setWorldScale(new Vector3(4f, 4f, 4f));
			assertEqual(1, calls, "移除世界缩放回调后不再触发");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
	private static void testGetUnityComponentsInChild()
	{
		Transformable t = new();
		var parent = new GameObject("ParentComps");
		try
		{
			t.setObject(parent);
			// 无子节点组件时列表为空
			var list = new System.Collections.Generic.List<BoxCollider>();
			t.getUnityComponentsInChild(true, list);
			assertEqual(0, list.Count, "无子节点组件时列表为空");
			// 添加子节点组件后能收集到
			var child = new GameObject("ChildComps");
			try
			{
				child.transform.SetParent(parent.transform);
				var col = child.AddComponent<BoxCollider>();
				t.getUnityComponentsInChild(true, list);
				assertEqual(1, list.Count, "子节点含组件时 getUnityComponentsInChild 应收集");
				assertEqual(col, list[0], "收集到的组件应为子节点上的组件");
			}
			finally
			{
				Object.DestroyImmediate(child);
			}
		}
		finally
		{
			Object.DestroyImmediate(parent);
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
