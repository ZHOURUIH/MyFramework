using UnityEngine;
using static TestAssert;

// Transformable 深度测试
// 聚焦复杂交互链：父子层级下的世界坐标换算、位置/旋转/缩放回调组合触发、
// 世界/本地坐标互相转换往返、lookAt/rotate/yawPitch 朝向行为、
// 图层操作、碰撞体使能、组件查询、copyObjectTransform 同步。
public static class TransformableDeepTest
{
	public static void Run()
	{
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
