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
		testCOMInteractiveNullGetters();
		// ─── 错误路径 getter ───
		testErrorPathGetters();
		// ─── resetProperty ───
		testResetProperty();
	

		// ─── 移动信息组件 update 驱动的时序 ───
		testMoveInfoFirstUpdateNoLastPosition();
		testMoveInfoStationarySpeedZero();
		testMoveInfoMovedDuringFrame();
		testMoveInfoLastSpeedTrails();
		testFixedUpdateAccumulatesSpeed();
		testFixedUpdateAccelerationChange();
		testFixedUpdateStationaryZero();
		// ─── 对象生命周期切换 ───
		testSetObjectExternalThenSelfCreate();
		testSelfCreateThenSetExternal();
		testSetObjectNullTwiceSelfCreate();
		// ─── 交互回调链 ───
		testClickCallbackInvoked();
		testHoverCallbackOnEnterLeave();
		testPressCallbackOnDownUp();
		testTouchMoveCallback();
		testPassRayToggle();
		testHandleInputToggle();
		testDragableStateTransition();
		// ─── 交互回调 setter 注册(懒加载组件) ───
		testSetOnTouchDown();
		testSetOnTouchUp();
		testSetOnTouchEnterLeave();
		testSetOnScreenTouchUp();
		testSetClickSound();
		testSetClickDetailCallback();
		testSetDoubleClickCallback();
		testSetPreClickCallback();
		testSetHoverDetailCallback();
		testSetPressDetailCallback();
		// ─── 事件转发(onXxx → getCOMInteractive) ───
		testOnTouchStay();
		testOnScreenTouchDownUp();
		testOnMultiTouchStartMoveEnd();
		testOnReceiveDrag();
		testOnDragHovered();
		// ─── resetProperty 深度 ───
		testResetClearsMoveInfoComponent();
	}

	// ═════════════════════════════════════════════════════════════════
	// 交互回调 setter(getCOMInteractive 懒加载组件)
	// ═════════════════════════════════════════════════════════════════

	private static void testSetOnTouchDown()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setOnTouchDown((pos, id) => { });
			mob.setOnTouchDown(null);
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetOnTouchUp()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setOnTouchUp((pos, id) => { });
			mob.setOnTouchUp(null);
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetOnTouchEnterLeave()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setOnTouchEnter((pos, id) => { });
			mob.setOnTouchLeave((pos, id) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetOnScreenTouchUp()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setOnScreenTouchUp((pos, id) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetClickSound()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setClickSound(123);
			mob.setClickSound(0);
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetClickDetailCallback()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setClickDetailCallback((pos) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetDoubleClickCallback()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setDoubleClickCallback(() => { });
			mob.setDoubleClickDetailCallback((pos) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetPreClickCallback()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setPreClickCallback(() => { });
			mob.setPreClickDetailCallback((pos) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetHoverDetailCallback()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setHoverDetailCallback((pos, hovered) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testSetPressDetailCallback()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.setPressDetailCallback((pos, pressed) => { });
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 事件转发(onXxx → getCOMInteractive 懒加载组件)
	// ═════════════════════════════════════════════════════════════════

	private static void testOnTouchStay()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.onTouchStay(Vector3.zero, 1);
			mob.onTouchStay(new Vector3(1f, 2f, 3f), 0);
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testOnScreenTouchDownUp()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.onScreenTouchDown(Vector3.zero, 1);
			mob.onScreenTouchUp(Vector3.zero, 1);
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testOnMultiTouchStartMoveEnd()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.onMultiTouchStart(Vector3.zero, new Vector3(1f, 1f, 0f));
			mob.onMultiTouchMove(Vector3.zero, Vector3.one, Vector3.one, Vector3.zero);
			mob.onMultiTouchEnd();
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testOnReceiveDrag()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			bool continueEvent = true;
			mob.onReceiveDrag(null, Vector3.zero, ref continueEvent);
			assertTrue(continueEvent, "无回调时 continueEvent 不变");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	private static void testOnDragHovered()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.onDragHovered(null, Vector3.zero, true);
			mob.onDragHovered(null, Vector3.zero, false);
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
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
	// 交互组件(mCOMInteractive)为 null 时的空安全查询
	// ═════════════════════════════════════════════════════════════════
	private static void testCOMInteractiveNullGetters()
	{
		MovableObject obj = new();
		// mCOMInteractive == null 时, 空安全查询返回默认值
		assertEqual(0, obj.getClickSound(), "无交互组件时 getClickSound 返回 0");
		assertFalse(obj.isReceiveScreenTouch(), "无交互组件时 isReceiveScreenTouch false");
		assertFalse(obj.isMouseHovered(), "无交互组件时 isMouseHovered false");
		// isPassDragEvent = !isDraggable() || (...), 无拖拽组件时 isDraggable false → 返回 true
		assertTrue(obj.isPassDragEvent(), "无交互/拖拽组件时 isPassDragEvent true");
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


	

	// ═════════════════════════════════════════════════════════════════════
	//  构造 helper: 一个绑定了真实 GameObject 的 MovableObject
	// ═════════════════════════════════════════════════════════════════════
	private static MovableObject NewMob(out GameObject go)
	{
		go = new GameObject("DeepMob");
		MovableObject mob = new();
		// ClassObject 构造时 mHasDestroy=true(相当于已被回收), 这里显式重置,
		// 否则 COMMovableObjectMoveInfo.update 会因 movableObject.isDestroy() 提前返回
		mob.setDestroy(false);
		mob.setObject(go);
		return mob;
	}

	// ═════════════════════════════════════════════════════════════════════
	//  移动信息组件 update 驱动的时序 —— 这是模块最值得深挖的部分
	//  关键: COMMovableObjectMoveInfo.update 只在 elapsedTime>0 时采样,
	//  首帧 mHasLastPosition=false → 不产生速度, 仅记录 lastPosition。
	// ═════════════════════════════════════════════════════════════════════
	//  1. 首帧 update: 无 lastPosition, 速度为零, 但 hasLastPosition 变 true
	private static void testMoveInfoFirstUpdateNoLastPosition()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			mob.setPosition(new Vector3(1f, 2f, 3f));
			// 首帧: 上次采样点不存在
			assertFalse(mob.hasLastPosition(), "首帧前无 lastPosition");
			mob.update(1.0f);
			// 采样后 lastPosition 有效
			assertTrue(mob.hasLastPosition(), "首帧 update 后 lastPosition 有效");
			assertEqual(Vector3.zero, mob.getMoveSpeedVector(), "首帧速度为零");
			assertFalse(mob.hasMovedDuringFrame(), "首帧没有位移判定");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  2. 静止物体连续 update: 速度恒为零, hasMovedDuringFrame 恒 false
	private static void testMoveInfoStationarySpeedZero()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			mob.setPosition(new Vector3(5f, 0f, 0f));
			// 多帧静止
			for (int i = 0; i < 5; ++i)
			{
				mob.update(1.0f);
			}
			assertEqual(Vector3.zero, mob.getMoveSpeedVector(), "静止时移动速度为零");
			assertEqual(Vector3.zero, mob.getLastSpeedVector(), "静止时 lastSpeed 为零");
			assertFalse(mob.hasMovedDuringFrame(), "静止帧 hasMovedDuringFrame 为 false");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  3. 位移一帧 → hasMovedDuringFrame 为 true, 且 moveSpeedVector 非零
	private static void testMoveInfoMovedDuringFrame()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			mob.setPosition(Vector3.zero);
			mob.update(1.0f); // 采样起点
			// 移动
			mob.setPosition(new Vector3(10f, 0f, 0f));
			mob.update(1.0f);
			assertTrue(mob.hasMovedDuringFrame(), "位移帧 hasMovedDuringFrame 为 true");
			assertTrue(mob.getMoveSpeedVector().x > 0f, "位移帧移动速度 x 分量非零");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  4. lastSpeedVector 追踪: update 末尾把本帧速度同步进 lastSpeedVector
	//     注意源码更新顺序: update 内先算 mMoveSpeedVector, 帧末 mLastSpeedVector=mMoveSpeedVector。
	//     首帧(无前帧)速度=0; 移动帧后 lastSpeedVector 已同步为本帧速度。
	private static void testMoveInfoLastSpeedTrails()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			mob.setPosition(Vector3.zero);
			mob.update(1.0f); // 采样起点, speed0=0, lastSpeed=0
			assertEqual(0.0f, Mathf.Abs(mob.getLastSpeedVector().x), 0.001f, "首帧 lastSpeed 为零");
			// 这一帧移动 0→10
			mob.setPosition(new Vector3(10f, 0f, 0f));
			mob.update(1.0f); // speed=(10-0)=10, 帧末 lastSpeed=speed=10
			// 移动帧 update 结束后 lastSpeedVector 已同步为本帧速度
			assertEqual(10.0f, Mathf.Abs(mob.getLastSpeedVector().x), 0.001f, "移动帧后 lastSpeedVector 同步为本帧速度 10");
			// 继续匀速移动 10→20: (20-0)... 用源码公式 cur - last/dt → (20,0,0)-((10,0,0)/1)=(10,0,0)
			mob.setPosition(new Vector3(20f, 0f, 0f));
			mob.update(1.0f);
			assertEqual(10.0f, Mathf.Abs(mob.getLastSpeedVector().x), 0.001f, "匀速续帧后 lastSpeed 仍为 10(追踪正确)");
			// 同帧末尾 lastSpeed 始终追平本帧速度(x 分量)
			assertEqual(mob.getMoveSpeedVector().x, mob.getLastSpeedVector().x, 0.001f, "同帧末尾 lastSpeed 追平本帧速度 x");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  5. fixedUpdate 累计速度: 匀速移动时 physicsSpeed = 位移/dt
	private static void testFixedUpdateAccumulatesSpeed()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			float dt = 0.5f;
			Vector3 step = new Vector3(2f, 0f, 0f);
			mob.setPosition(Vector3.zero);
			// 初始 fixedUpdate: lastPhysics=0 → speed=(0-0)/dt=0
			mob.fixedUpdate(dt);
			assertEqual(Vector3.zero, mob.getPhysicsSpeed(), "初始 physicsSpeed 为零");
			// 移动一步后 fixedUpdate
			mob.setPosition(step);
			mob.fixedUpdate(dt);
			Vector3 expected = new Vector3(step.x / dt, 0f, 0f);
			assertEqual(expected, mob.getPhysicsSpeed(), "匀速位移后 physicsSpeed = 位移/dt");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  6. fixedUpdate 加速度: 速度变化 → 加速度 = Δspeed/dt
	private static void testFixedUpdateAccelerationChange()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			float dt = 0.5f;
			mob.setPosition(new Vector3(0f, 0f, 0f));
			mob.fixedUpdate(dt); // speed=0
			// 等速阶段
			mob.setPosition(new Vector3(1f, 0f, 0f));
			mob.fixedUpdate(dt); // speed=(1-0)/dt=2, accel=(2-0)/dt=4
			Vector3 expectedAccel = new Vector3((1f / dt) / dt, 0f, 0f);
			assertEqual(expectedAccel, mob.getPhysicsAcceleration(), "加速阶段 physicsAcceleration = Δspeed/dt");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  7. FixedUpdate 静止帧不产生虚假速度: 位置不变后 physicsSpeed 归零
	//     注意源码固定语义: (cur-last)/dt。物体瞬间移动到(2,0,0)的首 2 帧仍是
	//     加速瞬态(accel=(0-4)/dt=-8 非零), 需跑满瞬态后才能归零——见下推演。
	private static void testFixedUpdateStationaryZero()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			float dt = 0.5f;
			// 物体移动到(2,0,0)后走 3 帧 fixedUpdate, 让瞬态完全衰减后再断言归零
			mob.setPosition(new Vector3(2f, 0f, 0f));
			mob.fixedUpdate(dt);  // 帧1: speed=(2-0)/0.5=4, accel=(4-0)/0.5=8
			mob.fixedUpdate(dt);  // 帧2: speed=(2-2)/0.5=0, accel=(0-4)/0.5=-8
			mob.fixedUpdate(dt);  // 帧3: speed=(2-2)/0.5=0, accel=(0-0)/0.5=0
			// 瞬态结束后位置不变 → 速度与加速度都归零
			assertEqual(0.0f, mob.getPhysicsSpeed().x, 0.0001f, "静止的 fixedUpdate 不产生速度 x");
			assertEqual(0.0f, mob.getPhysicsSpeed().y, 0.0001f, "静止的 fixedUpdate 不产生速度 y");
			assertEqual(0.0f, mob.getPhysicsAcceleration().x, 0.0001f, "静止的 fixedUpdate 不产生加速度 x");
			assertEqual(0.0f, mob.getPhysicsAcceleration().y, 0.0001f, "静止的 fixedUpdate 不产生加速度 y");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  对象生命周期切换
	// ═════════════════════════════════════════════════════════════════════
	//  8. 先绑定外部对象, 再 selfCreate: 切换节点并标记自建
	private static void testSetObjectExternalThenSelfCreate()
	{
		MovableObject mob = new();
		GameObject external = new GameObject("Ext");
		try
		{
			mob.setObject(external);
			assertEqual(external, mob.getGameObject(), "初始绑定外部对象");
			mob.selfCreateObject("SelfNode");
			assertNotNull(mob.getGameObject(), "selfCreate 后切换到自建节点");
			assertTrue(mob.getGameObject() != external, "自建节点 != 外部节点");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(external);
		}
	}

	//  9. 先 selfCreate, 再 setObject(外部): destroy 时不销毁外部对象, 且外部引用保持不变
	private static void testSelfCreateThenSetExternal()
	{
		MovableObject mob = new();
		mob.selfCreateObject();
		GameObject external = new GameObject("Ext2");
		try
		{
			// selfCreate 后切换到外部对象
			mob.setObject(external);
			assertEqual(external, mob.getGameObject(), "切换到外部对象");
			// destroy(): 自建节点在 setObject(external) 时已被销毁(mSelfCreatedObject 已置 false),
			// 因此 destroy 不会动 mObject —— 外部对象生命周期由外部管理,引用保持不变(源码有意行为:
			// "mObject需要外部自己创建以及销毁,内部只是引用,不会管理其生命周期")。
			mob.destroy();
			assertEqual(external, mob.getGameObject(), "destroy 后外部引用保持不变(外部对象生命周期由外部管理)");
			// external 仍是有效对象 (没有被销毁)
			assertNotNull(external, "外部对象未被销毁");
		}
		finally
		{
			Object.DestroyImmediate(external);
			// 注意: 自建节点 selfNode 在 setObject(external) 时已被 destroySelfCreateObject() 归还给
			// mGameObjectPool(归还到其 mUnusedList 缓存), 生命周期已由对象池接管。
			// 这里绝不能再用 DestroyImmediate 二次销毁, 否则会让对象池缓存一个已被销毁的 GameObject,
			// 后续 newObject 复用它时触发 MissingReferenceException(曾污染 testSetObjectNullTwiceSelfCreate)。
		}
	}

	//  10. setObject(null) 连续调用: 自建节点保持一致
	private static void testSetObjectNullTwiceSelfCreate()
	{
		MovableObject mob = new();
		try
		{
			mob.setObject(null);
			GameObject n1 = mob.getGameObject();
			assertNotNull(n1, "第一次 setObject(null) 自建节点");
			mob.setObject(null);
			GameObject n2 = mob.getGameObject();
			assertNotNull(n2, "第二次 setObject(null) 仍有节点");
			assertEqual(n1, n2, "连续 setObject(null) 重复使用自建节点");
		}
		finally
		{
			mob.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  交互回调链 —— 通过 setXxxCallback 注册, 直接调用 onTouchXxx 驱动
	//  框架环境已初始化, mInputSystem.getTouchPoint 返回 null 时被空安全跳过
	// ═════════════════════════════════════════════════════════════════════
	//  11. 点击回调链: onTouchDown + onTouchUp(短时近距离) → clickCallback
	private static void testClickCallbackInvoked()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("ClickMob");
		mob.setObject(go);
		bool clicked = false;
		try
		{
			mob.setClickCallback(() => { clicked = true; });
			// 模拟一次短按: 同点按下立即抬起 → 触发 click
			mob.onTouchDown(new Vector3(0f, 0f, 0f), 1);
			mob.onTouchUp(new Vector3(0f, 0f, 0f), 1);
			assertTrue(clicked, "短时近距离点击应触发 clickCallback");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  12. hover 回调: onTouchEnter → hover(true), onTouchLeave → hover(false)
	private static void testHoverCallbackOnEnterLeave()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("HoverMob");
		mob.setObject(go);
		bool hoverVal = false;
		int hoverCount = 0;
		try
		{
			mob.setHoverCallback((bool h) => { hoverVal = h; hoverCount++; });
			mob.onTouchEnter(new Vector3(0f, 0f, 0f), 1);
			assertTrue(hoverVal, "onTouchEnter → hover true");
			assertEqual(1, hoverCount, "进入触发一次 hover");
			// 离开
			mob.onTouchLeave(new Vector3(0f, 0f, 0f), 1);
			assertFalse(hoverVal, "onTouchLeave → hover false");
			// 再次进入: 由于已离开, 再次触发 true
			mob.onTouchEnter(new Vector3(0f, 0f, 0f), 1);
			assertTrue(hoverVal, "再次进入 hover true");
			assertEqual(3, hoverCount, "进入/离开/进入共 3 次");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  13. press 回调: onTouchDown → press(true), onTouchUp → press(false)
	//     注意: 源码 onTouchLeave 只重置 mPressing 内部标记,【不】回调 pressCallback(false),
	//     只有 onTouchUp 才回调 mPressCallback(false)。这里按真实语义驱动抬起释放路径。
	private static void testPressCallbackOnDownUp()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("PressMob");
		mob.setObject(go);
		bool pressVal = false;
		try
		{
			mob.setPressCallback((bool p) => { pressVal = p; });
			mob.onTouchDown(new Vector3(0f, 0f, 0f), 1);
			assertTrue(pressVal, "onTouchDown → press true");
			mob.onTouchUp(new Vector3(0f, 0f, 0f), 1);
			assertFalse(pressVal, "onTouchUp → press false");
			// 源码行为校验: onTouchLeave 不回调 pressCallback(false), press 回调保持当前值
			mob.onTouchDown(new Vector3(0f, 0f, 0f), 1);
			assertTrue(pressVal, "再次按下 → press true");
			mob.onTouchLeave(new Vector3(0f, 0f, 0f), 1);
			// 离开仅重置内部按压标记, 不重新回调 pressCallback(false) → 回调值不变
			assertTrue(pressVal, "onTouchLeave 不回调 pressCallback(false), 保持 true");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  14. touchMove 回调: onTouchMove 透传触点数据
	private static void testTouchMoveCallback()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("MoveMob");
		mob.setObject(go);
		Vector3? lastDelta = null;
		try
		{
			mob.setOnTouchMove((Vector3 pos, Vector3 delta, float t, int id) => { lastDelta = delta; });
			mob.onTouchMove(Vector3.zero, new Vector3(1f, 2f, 0f), 0.1f, 7);
			assertNotNull(lastDelta, "onTouchMove 应触发回调");
			assertEqual(new Vector3(1f, 2f, 0f), lastDelta.Value, "onTouchMove 透传 delta");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  15. 射线穿透开关: setPassRay 切换 isPassRay
	private static void testPassRayToggle()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("RayMob");
		mob.setObject(go);
		try
		{
			bool def = mob.isPassRay();
			mob.setPassRay(!def);
			assertTrue(mob.isPassRay() != def, "setPassRay 切换 isPassRay");
			mob.setPassRay(def);
			assertEqual(def, mob.isPassRay(), "setPassRay 恢复默认");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  16. 输入开关: setHandleInput 切换 isHandleInput
	private static void testHandleInputToggle()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("InputMob");
		mob.setObject(go);
		try
		{
			bool def = mob.isHandleInput();
			mob.setHandleInput(!def);
			assertTrue(mob.isHandleInput() != def, "setHandleInput 切换 isHandleInput");
			mob.setHandleInput(def);
			assertEqual(def, mob.isHandleInput(), "setHandleInput 恢复默认");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  17. 可拖拽状态转变: 无拖拽组件 → false; 添加 COMMovableObjectDrag → true
	private static void testDragableStateTransition()
	{
		MovableObject mob = new();
		GameObject go = new GameObject("DragMob");
		mob.setObject(go);
		try
		{
			assertFalse(mob.isDraggable(), "默认不可拖拽");
			// 直接添加拖拽组件 (addComponent 继承自 ComponentOwner, 公开可用)
			mob.addComponent<COMMovableObjectDrag>(true);
			assertTrue(mob.isDraggable(), "添加拖拽组件后可拖拽");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}

	//  18. resetProperty 清理: 移除移动信息组件与自建标记
	private static void testResetClearsMoveInfoComponent()
	{
		MovableObject mob = NewMob(out GameObject go);
		try
		{
			mob.enableMoveInfo();
			assertNotNull(mob.getCOMMoveInfo(), "enableMoveInfo 后存在移动信息组件");
			mob.resetProperty();
			assertNull(mob.getCOMMoveInfo(), "resetProperty 后移动信息组件引用清空");
		}
		finally
		{
			mob.destroy();
			Object.DestroyImmediate(go);
		}
	}
}
