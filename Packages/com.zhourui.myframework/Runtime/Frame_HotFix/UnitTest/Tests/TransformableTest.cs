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
	

		testParentChildWorldTransform();
		testNestedParentWorldTransform();
		testLocalWorldRoundTrip();
		testWorldToLocalDirectionRoundTrip();
		testAllModifyCallbacks_FireTogether();
		testWorldScaleCallback_FiredOnUpdate();
		testRotateAndLookAt();
		testYawPitch();
		testMoveSelfVsWorld();
		testLayerOperations();
		testColliderEnable();
		testComponentQueryAndEnable();
		testCopyObjectTransform();
		testIsChildOfNested();
		testGetObjectPathAndInstanceID();
		testSetParentResetsTransform();
		testResetActive();
		testGetRotationRadian_Consistency();
		testSetRotationComponentWise();
		testCombinedTransformManipulation();
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


	

	// ─── 父子层级：父缩放影响子世界坐标 ───────────────────────────────
	private static void testParentChildWorldTransform()
	{
		Transformable parent = new();
		Transformable child = new();
		var parentGo = new GameObject("ParentW");
		var childGo = new GameObject("ChildW");
		try
		{
			parent.setObject(parentGo);
			child.setObject(childGo);
			// 父位置 (10,0,0)，子本地位置 (1,0,0) → 世界 (11,0,0)
			parent.setPosition(new Vector3(10f, 0f, 0f));
			child.setParent(parentGo); // 挂到父节点（resetTrans=true 会清零子位移）
			child.setPosition(new Vector3(1f, 0f, 0f));
			assertEqual(11f, child.getWorldPosition().x, 0.001f, "子世界 x = 父10 + 本地1");
			assertEqual(0f, child.getWorldPosition().y, 0.001f);
		}
		finally
		{
			Object.DestroyImmediate(childGo);
			Object.DestroyImmediate(parentGo);
			child.destroy();
			parent.destroy();
		}
	}

	// ─── 多重嵌套父级：派生世界坐标 ────────────────────────────────────
	private static void testNestedParentWorldTransform()
	{
		Transformable t = new();
		Transformable p = new();
		Transformable gp = new();
		var tGo = new GameObject("NestedT");
		var pGo = new GameObject("NestedP");
		var gpGo = new GameObject("NestedGP");
		try
		{
			gp.setObject(gpGo);
			p.setObject(pGo);
			t.setObject(tGo);
			gp.setPosition(new Vector3(100f, 0f, 0f));
			p.setParent(gpGo);
			t.setParent(pGo);
			p.setPosition(new Vector3(10f, 0f, 0f));
			t.setPosition(new Vector3(1f, 0f, 0f));
			// 祖 100 + 父 10 + 子 1 = 111
			assertEqual(111f, t.getWorldPosition().x, 0.001f, "三级嵌套世界位置");
		}
		finally
		{
			Object.DestroyImmediate(tGo);
			Object.DestroyImmediate(pGo);
			Object.DestroyImmediate(gpGo);
			t.destroy();
			p.destroy();
			gp.destroy();
		}
	}

	// ─── localToWorld / worldToLocal 往返 ──────────────────────────────
	private static void testLocalWorldRoundTrip()
	{
		Transformable t = new();
		var go = new GameObject("LW");
		try
		{
			t.setObject(go);
			t.setPosition(new Vector3(5f, -3f, 2f));
			t.setRotation(new Vector3(0f, 30f, 0f));
			Vector3 local = new(1f, 1f, 1f);
			Vector3 world = t.localToWorld(local);
			Vector3 back = t.worldToLocal(world);
			assertEqual(local.x, back.x, 0.01f, "localToWorld→worldToLocal 往返 x");
			assertEqual(local.y, back.y, 0.01f, "y");
			assertEqual(local.z, back.z, 0.01f, "z");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── localToWorldDirection / worldToLocalDirection 往返 ────────────
	private static void testWorldToLocalDirectionRoundTrip()
	{
		Transformable t = new();
		var go = new GameObject("WLD");
		try
		{
			t.setObject(go);
			t.setRotation(new Vector3(15f, 45f, 0f));
			Vector3 dir = new(0f, 1f, 0f);
			Vector3 worldDir = t.localToWorldDirection(dir);
			Vector3 back = t.worldToLocalDirection(worldDir);
			assertEqual(dir.x, back.x, 0.01f, "方向往返 x");
			assertEqual(dir.y, back.y, 0.01f, "y");
			assertEqual(dir.z, back.z, 0.01f, "z");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── 位置/旋转/缩放回调在各自修改时触发 ──────────────────────────
	private static void testAllModifyCallbacks_FireTogether()
	{
		Transformable t = new();
		var go = new GameObject("CbAll");
		try
		{
			t.setObject(go);
			int pos = 0, rot = 0, scale = 0;
			t.addPositionModifyCallback(() => ++pos);
			t.addRotationModifyCallback(() => ++rot);
			t.addScaleModifyCallback(() => ++scale);

			t.setPosition(new Vector3(1f, 2f, 3f));
			t.setRotation(new Vector3(0f, 10f, 0f));
			t.setScale(new Vector3(2f, 2f, 2f));
			assertEqual(1, pos, "位置回调触发");
			assertEqual(1, rot, "旋转回调触发");
			assertEqual(1, scale, "缩放回调触发");

			// 修改各自组件，互不干扰
			t.setPositionX(5f);
			t.setRotationY(20f);
			t.setScaleX(3f);
			assertEqual(2, pos, "setPositionX 再次触发位置回调");
			assertEqual(2, rot, "setRotationY 再次触发旋转回调");
			assertEqual(2, scale, "setScaleX 再次触发缩放回调");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── 世界缩放回调：update 检测到变化才触发 ────────────────────────
	private static void testWorldScaleCallback_FiredOnUpdate()
	{
		Transformable parent = new();
		Transformable child = new();
		var pGo = new GameObject("WSCbP");
		var cGo = new GameObject("WSCbC");
		try
		{
			parent.setObject(pGo);
			child.setObject(cGo);
			child.setParent(pGo);
			child.setPosition(Vector3.zero);

			int calls = 0;
			child.addWorldScaleModifyCallback(() => ++calls);
			parent.setScale(new Vector3(2f, 2f, 2f));
			// 父节点缩放改变了子的 world scale，需调用 update 后回调才在内部被检测
			child.update(0.016f);
			assertEqual(1, calls, "父缩放后 update 触发子世界缩放回调");
			assertEqual(1, calls, "重复 update 无变化不再触发");
		}
		finally
		{
			Object.DestroyImmediate(cGo);
			Object.DestroyImmediate(pGo);
			child.destroy();
			parent.destroy();
		}
	}

	// ─── rotate / lookAt / rotateAround 朝向 ──────────────────────────
	private static void testRotateAndLookAt()
	{
		Transformable t = new();
		var go = new GameObject("Look");
		try
		{
			t.setObject(go);
			t.rotate(new Vector3(0f, 90f, 0f));
			assertEqual(90f, t.getRotation().y, 1f, "rotate 后 y 角=90");
			// lookAt 沿 +Z 应由 y 角归零
			t.lookAt(new Vector3(0f, 0f, 1f));
			Vector3 fwdZ = t.getForward();
			assertTrue(fwdZ.z > 0.9f, "lookAt(+Z) 前方指向 +Z");
			t.lookAt(new Vector3(1f, 0f, 0f));
			Vector3 fwdX = t.getForward();
			assertTrue(fwdX.x > 0.9f, "lookAt(+X) 前方指向 +X");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── yawPitch 累积俯仰/偏航 ───────────────────────────────────────
	private static void testYawPitch()
	{
		Transformable t = new();
		var go = new GameObject("Yaw");
		try
		{
			t.setObject(go);
			t.yawPitch(30f, 10f);
			t.yawPitch(20f, 5f);
			assertEqual(50f, t.getRotation().y, 0.5f, "偏航累加 30+20=50");
			assertEqual(15f, t.getRotation().x, 0.5f, "俯仰累加 10+5=15");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── move 的本地 vs 世界空间区别 ──────────────────────────────────
	private static void testMoveSelfVsWorld()
	{
		Transformable t = new();
		var go = new GameObject("MoveSp");
		try
		{
			t.setObject(go);
			t.setRotation(new Vector3(0f, 90f, 0f));
			t.setPosition(Vector3.zero);
			// 本地空间 move：旋转90°后，本地 +X 方向在世界中为 +Z
			t.move(new Vector3(1f, 0f, 0f), Space.Self);
			assertEqual(0f, t.getPosition().x, 0.01f, "本地move x为0");
			assertEqual(0f, t.getPosition().y, 0.01f);
			// 世界空间 move：直接加世界偏移
			t.move(new Vector3(1f, 0f, 0f), Space.World);
			assertEqual(1f, t.getPosition().x, 0.01f, "世界move x=1");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── setLayer / getLayer / getLayerName ───────────────────────────
	private static void testLayerOperations()
	{
		Transformable t = new();
		var go = new GameObject("Layer");
		try
		{
			t.setObject(go);
			int defaultLayer = 0;
			int target = 9; // 任意合法图层
			t.setLayer(target);
			assertEqual(target, t.getLayer(), "setLayer 后 getLayer");
			assertEqual(LayerMask.LayerToName(target), t.getLayerName(), "getLayerName 与 Unity 一致");
			// 未设置前默认图层
			t.setLayer(defaultLayer);
			assertEqual(defaultLayer, t.getLayer(), "还原默认图层");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── enableAllColliders ───────────────────────────────────────────
	private static void testColliderEnable()
	{
		Transformable t = new();
		var go = new GameObject("Col");
		try
		{
			var c = go.AddComponent<BoxCollider>();
			// 调用 getCollider() 确保取到碰撞体
			Transformable t2 = t;
			t2.setObject(go);
			Collider collider = t2.getCollider();
			assertEqual(c, collider, "getCollider 返回 BoxCollider");
			t2.enableAllColliders(false);
			assertFalse(c.enabled, "enableAllColliders(false) 禁用碰撞体");
			t2.enableAllColliders(true);
			assertTrue(c.enabled, "enableAllColliders(true) 启用碰撞体");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── 组件查询 / 使能 ─────────────────────────────────────────────
	private static void testComponentQueryAndEnable()
	{
		Transformable t = new();
		var go = new GameObject("Comp");
		try
		{
			var light = go.AddComponent<Light>();
			t.setObject(go);
			assertTrue(t.tryGetUnityComponent<Light>(out Light found), "tryGetUnityComponent Light");
			assertEqual(light, found, "查到同组件实例");
			t.enableUnityComponent<Light>(false);
			assertFalse(light.enabled, "enableUnityComponent(false)");
			t.enableUnityComponent<Light>(true);
			assertTrue(light.enabled, "enableUnityComponent(true)");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── copyObjectTransform：从其它物体拷贝变换 ─────────────────────
	private static void testCopyObjectTransform()
	{
		Transformable t = new();
		var go = new GameObject("CopyT");
		var srcGo = new GameObject("CopySrc");
		try
		{
			srcGo.transform.localPosition = new Vector3(4f, 5f, 6f);
			srcGo.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
			srcGo.transform.localScale = new Vector3(3f, 3f, 3f);
			t.setObject(go);
			t.copyObjectTransform(srcGo);
			assertEqual(4f, t.getPosition().x, 0.001f, "拷贝位置 x");
			assertEqual(45f, t.getRotation().y, 0.5f, "拷贝旋转 y");
			assertEqual(3f, t.getScale().x, 0.001f, "拷贝缩放 x");
		}
		finally
		{
			Object.DestroyImmediate(srcGo);
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── isChildOf 嵌套层级判断 ─────────────────────────────────────
	private static void testIsChildOfNested()
	{
		// isChildOf(IMouseEventCollect) 需要参数实现 IMouseEventCollect,故用 MovableObject
		MovableObject parent = new();
		MovableObject child = new();
		var pGo = new GameObject("IcoP");
		var cGo = new GameObject("IcoC");
		try
		{
			parent.setObject(pGo);
			child.setObject(cGo);
			child.setParent(pGo);
			assertTrue(child.isChildOf(parent), "子物体是父的 child");
			assertFalse(parent.isChildOf(child), "父不是子的 child");
		}
		finally
		{
			Object.DestroyImmediate(cGo);
			Object.DestroyImmediate(pGo);
			child.destroy();
			parent.destroy();
		}
	}

	// ─── getGameObjectPath / instanceID ─────────────────────────────
	private static void testGetObjectPathAndInstanceID()
	{
		Transformable t = new();
		var go = new GameObject("PathObj");
		try
		{
			// 注意: setObject 会把 GameObject 名改为 mName(mName 默认空串),
			// 所以需先 setName 再 setObject, 否则根节点名被清空导致 path 为空
			t.setName("PathObj");
			t.setObject(go);
			int id = t.getGameObjectInstanceID();
			// getGameObjectID 在 6000.4+ 用 EntityId, 旧版本用 GetInstanceID(已废弃)
			// 为保持测试程序集在不同 Unity 版本都能编译, 仅在高版本下交叉校验一致性
#if UNITY_6000_4_OR_NEWER
			// GetInstanceID 已废弃, 改用 GetEntityId(与 getGameObjectID 在 UNITY_6000_4_OR_NEWER 的实现一致)
			assertEqual((int)EntityId.ToULong(go.GetEntityId()), id, "instanceID 与 Unity 一致");
#else
			assertTrue(id != 0, "instanceID 非零");
#endif
			string path = t.getGameObjectPath();
			assertTrue(!string.IsNullOrEmpty(path), "path 非空");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── setParent 时 resetTrans=true 清零子变换 ─────────────────────
	private static void testSetParentResetsTransform()
	{
		Transformable child = new();
		var pGo = new GameObject("SprP");
		var cGo = new GameObject("SprC");
		try
		{
			child.setObject(cGo);
			child.setPosition(new Vector3(9f, 9f, 9f));
			child.setScale(new Vector3(5f, 5f, 5f));
			child.setParent(pGo, true); // resetTrans=true
			assertEqual(0f, child.getPosition().x, 0.001f, "resetTrans 后本地位移清零");
			assertEqual(1f, child.getScale().x, 0.001f, "resetTrans 后缩放恢复1");
		}
		finally
		{
			Object.DestroyImmediate(cGo);
			Object.DestroyImmediate(pGo);
			child.destroy();
		}
	}

	// ─── resetActive：禁用再启用 ────────────────────────────────────
	private static void testResetActive()
	{
		Transformable t = new();
		var go = new GameObject("RSt");
		try
		{
			t.setObject(go);
			t.setActive(false);
			t.resetActive();
			assertTrue(t.isActive(), "resetActive 后 active=true");
			assertTrue(go.activeSelf, "GameObject active");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── getRotationRadian 与 getRotation 度数一致 ──────────────────
	private static void testGetRotationRadian_Consistency()
	{
		Transformable t = new();
		var go = new GameObject("Rad");
		try
		{
			t.setObject(go);
			t.setRotation(new Vector3(0f, 30f, 0f));
			Vector3 deg = t.getRotation();
			Vector3 rad = t.getRotationRadian();
			assertEqual(deg.y * Mathf.Deg2Rad, rad.y, 0.01f, "旋转弧度 = 度数精转换");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── 组件级 setter ──────────────────────────────────────────────
	private static void testSetRotationComponentWise()
	{
		Transformable t = new();
		var go = new GameObject("RotC");
		try
		{
			t.setObject(go);
			t.setRotationX(10f);
			t.setRotationY(20f);
			t.setRotationZ(30f);
			assertEqual(10f, t.getRotation().x, 0.01f, "setRotationX");
			assertEqual(20f, t.getRotation().y, 0.01f, "setRotationY");
			assertEqual(30f, t.getRotation().z, 0.01f, "setRotationZ");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}

	// ─── 组合变换：位置+旋转+缩放多次叠加 ──────────────────────────
	private static void testCombinedTransformManipulation()
	{
		Transformable t = new();
		var go = new GameObject("Combined");
		try
		{
			t.setObject(go);
			// 连续 move 累加
			t.move(new Vector3(1f, 0f, 0f), Space.World);
			t.move(new Vector3(2f, 0f, 0f), Space.World);
			t.move(new Vector3(3f, 0f, 0f), Space.World);
			assertEqual(6f, t.getPosition().x, 0.001f, "三次世界移动累加 x=6");
			// 缩放连续乘等价于设置
			t.setScale(new Vector3(2f, 2f, 2f));
			assertEqual(2f, t.getScale().x, 0.001f, "缩放设置");
			// 旋转 + 缩放交叉操作不破坏位置
			t.setRotation(new Vector3(0f, 45f, 0f));
			assertEqual(6f, t.getPosition().x, 0.5f, "旋转不影响位置");
		}
		finally
		{
			Object.DestroyImmediate(go);
			t.destroy();
		}
	}
}
