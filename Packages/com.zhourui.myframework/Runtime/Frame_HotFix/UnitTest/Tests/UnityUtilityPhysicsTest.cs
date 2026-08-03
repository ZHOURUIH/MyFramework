using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static TestAssert;
using static UnityUtility;

// overlapCollider(Collider, Vector3, Collider[], int) 真实物理环境测试
// 覆盖：
// 1. 空参数与无位移
// 2. BoxCollider / SphereCollider / CapsuleCollider 高速移动
// 3. 上一帧与当前帧都未重叠，但移动路径中发生碰撞
// 4. 多目标、斜向移动、center偏移、固定旋转和缩放
// 5. 当前帧重叠与扫掠结果去重
// 6. LayerMask、Trigger、结果数组清理和容量限制
public static class UnityUtilityPhysicsTest
{
	private const int SOURCE_LAYER = 30;
	private const int TARGET_LAYER = 31;
	private const int OTHER_LAYER = 29;
	private const int TARGET_MASK = 1 << TARGET_LAYER;

	// 测试区域放到远离常规游戏场景的位置，避免与场景中的真实物体互相干扰
	private static readonly Vector3 TEST_ORIGIN = new(4096.0f, 4096.0f, 4096.0f);
	private static readonly List<GameObject> mTestObjects = new();

	public static void Run()
	{
		bool originQueriesHitTriggers = Physics.queriesHitTriggers;
		try
		{
			Physics.queriesHitTriggers = true;

			testNullCollider();
			testNoMovementDetectsCurrentOverlap();
			testBoxColliderRealMovement();
			testSphereColliderRealMovement();
			testCapsuleColliderRealMovement();
			testCapsuleColliderAllDirections();
			testMultipleTargetsAlongMovementPath();
			testTargetOutsideMovementPath();
			testDiagonalMovement();
			testColliderCenterOffset();
			testBoxFixedRotationAndScale();
			testCurrentOverlapDoesNotDuplicate();
			testLayerMask();
			testTrigger();
			testResultArrayOldValuesAreCleared();
			testResultCapacity();
		}
		finally
		{
			Physics.queriesHitTriggers = originQueriesHitTriggers;
			destroyTestObjects();
		}
	}

	// ─── 空Collider ───────────────────────────────────────────────────────
	private static void testNullCollider()
	{
		Collider[] results = new Collider[4];
		int count = overlapCollider(null, Vector3.zero, results, TARGET_MASK);
		assert(count == 0, "Collider为空时应返回0");
	}

	// ─── 没有移动时仍检测当前位置的重叠 ──────────────────────────────────
	private static void testNoMovementDetectsCurrentOverlap()
	{
		BoxCollider source = createMovingBox(
			"NoMovement_Source",
			TEST_ORIGIN,
			Vector3.one,
			SOURCE_LAYER);

		BoxCollider target = createBox(
			"NoMovement_Target",
			TEST_ORIGIN + Vector3.right * 0.75f,
			Vector3.one,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		int count = overlapCollider(
			source,
			source.transform.position,
			results,
			TARGET_MASK);

		assert(count == 1, "没有移动时应返回当前位置重叠的1个碰撞体");
		assert(containsCollider(results, count, target), "没有移动时应检测到当前位置的目标");

		destroyTestObjects();
	}

	// ─── BoxCollider真实高速移动 ──────────────────────────────────────────
	private static void testBoxColliderRealMovement()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;
		BoxCollider source = createMovingBox("BoxMovement_Source", lastWorldPos, Vector3.one, SOURCE_LAYER);

		// 很薄的墙位于移动路径中间，起点和终点都不会与它重叠
		BoxCollider target = createBox("BoxMovement_ThinWall", TEST_ORIGIN, new(0.2f, 4.0f, 4.0f), TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		// 验证上一帧没有发生重叠
		int count = overlapCollider(source, results, TARGET_MASK);
		assert(count == 0, "Box移动前不应与中间薄墙重叠");

		// 模拟物体从上一帧位置真实移动到当前帧位置
		moveCollider(source, currentWorldPos);

		// 只检测当前帧位置时会漏掉中间薄墙，这正是扫掠检测要解决的问题
		count = overlapCollider(source, results, TARGET_MASK);
		assert(count == 0, "Box移动后只检测当前位置时应检测不到中间薄墙");

		// 使用上一帧世界坐标检测完整移动路径
		count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 1, "Box扫掠应命中移动路径中的薄墙");
		assert(containsCollider(results, count, target), "Box扫掠结果中应包含中间薄墙");

		destroyTestObjects();
	}

	// ─── SphereCollider真实高速移动 ───────────────────────────────────────
	private static void testSphereColliderRealMovement()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		SphereCollider source = createMovingSphere(
			"SphereMovement_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		BoxCollider target = createBox(
			"SphereMovement_ThinWall",
			TEST_ORIGIN,
			new(0.2f, 4.0f, 4.0f),
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		int count = overlapCollider(source, results, TARGET_MASK);
		assert(count == 0, "Sphere移动前不应与中间薄墙重叠");

		moveCollider(source, currentWorldPos);

		count = overlapCollider(source, results, TARGET_MASK);
		assert(count == 0, "Sphere移动后只检测当前位置时应检测不到中间薄墙");

		count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 1, "Sphere扫掠应命中移动路径中的薄墙");
		assert(containsCollider(results, count, target), "Sphere扫掠结果中应包含中间薄墙");

		destroyTestObjects();
	}

	// ─── CapsuleCollider真实高速移动 ──────────────────────────────────────
	private static void testCapsuleColliderRealMovement()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		CapsuleCollider source = createMovingCapsule(
			"CapsuleMovement_Source",
			lastWorldPos,
			0.5f,
			2.0f,
			1,
			SOURCE_LAYER);

		BoxCollider target = createBox(
			"CapsuleMovement_ThinWall",
			TEST_ORIGIN,
			new(0.2f, 4.0f, 4.0f),
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		int count = overlapCollider(source, results, TARGET_MASK);
		assert(count == 0, "Capsule移动前不应与中间薄墙重叠");

		moveCollider(source, currentWorldPos);

		count = overlapCollider(source, results, TARGET_MASK);
		assert(count == 0, "Capsule移动后只检测当前位置时应检测不到中间薄墙");

		count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 1, "Capsule扫掠应命中移动路径中的薄墙");
		assert(containsCollider(results, count, target), "Capsule扫掠结果中应包含中间薄墙");

		destroyTestObjects();
	}

	// ─── CapsuleCollider三个方向 ──────────────────────────────────────────
	private static void testCapsuleColliderAllDirections()
	{
		testCapsuleDirection(0, Vector3.right);
		testCapsuleDirection(1, Vector3.up);
		testCapsuleDirection(2, Vector3.forward);
	}

	private static void testCapsuleDirection(int direction, Vector3 capsuleAxis)
	{
		// 移动方向必须与胶囊轴线垂直，否则目标可能仅靠移动路径就被命中，
		// 无法真正验证CapsuleCollider.direction是否参与了扫掠形状计算。
		Vector3 moveAxis = direction == 0 ? Vector3.forward : Vector3.right;
		Vector3 lastWorldPos = TEST_ORIGIN - moveAxis * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + moveAxis * 5.0f;

		CapsuleCollider source = createMovingCapsule(
			"CapsuleDirection_Source_" + direction,
			lastWorldPos,
			0.5f,
			4.0f,
			direction,
			SOURCE_LAYER);

		// 胶囊height=4、radius=0.5，两端球心距离中心1.5。
		// 将目标放在对应轴线1.25的位置，保证位于胶囊内部，
		// 同时又超出错误方向胶囊的半径范围，能够准确验证direction。
		SphereCollider target = createSphere(
			"CapsuleDirection_Target_" + direction,
			TEST_ORIGIN + capsuleAxis * 1.25f,
			0.2f,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		// 验证起始位置没有与目标重叠。
		int count = overlapCollider(source, results, TARGET_MASK);
		assert(
			count == 0,
			"Capsule direction=" + direction + " 移动前不应命中目标,实际数量:" + count);

		moveCollider(source, currentWorldPos);

		// 验证终点位置也没有与目标重叠。
		count = overlapCollider(source, results, TARGET_MASK);
		assert(
			count == 0,
			"Capsule direction=" + direction + " 移动后只检测当前位置不应命中目标,实际数量:" + count);

		// 只有扫过区域应当命中目标。
		count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(
			containsCollider(results, count, target),
			"Capsule direction=" +
			direction +
			" 时应命中轴线范围内的目标,count:" +
			count +
			",capsuleAxis:" +
			capsuleAxis +
			",moveAxis:" +
			moveAxis);

		destroyTestObjects();
	}

	// ─── 一次移动命中多个目标 ─────────────────────────────────────────────
	private static void testMultipleTargetsAlongMovementPath()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 6.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 6.0f;

		SphereCollider source = createMovingSphere(
			"MultipleTargets_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		SphereCollider target0 = createSphere(
			"MultipleTargets_Target0",
			TEST_ORIGIN + Vector3.left * 3.0f,
			0.25f,
			TARGET_LAYER);

		SphereCollider target1 = createSphere(
			"MultipleTargets_Target1",
			TEST_ORIGIN,
			0.25f,
			TARGET_LAYER);

		SphereCollider target2 = createSphere(
			"MultipleTargets_Target2",
			TEST_ORIGIN + Vector3.right * 3.0f,
			0.25f,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 3, "移动路径中有3个目标时应返回3个");
		assert(containsCollider(results, count, target0), "结果中应包含目标0");
		assert(containsCollider(results, count, target1), "结果中应包含目标1");
		assert(containsCollider(results, count, target2), "结果中应包含目标2");
		for (int i = 0; i < count; ++i)
		{
			assert(results[i] != null, "有效结果中不能出现null,index:" + i);
			for (int j = i + 1; j < count; ++j)
			{
				assert(results[i] != results[j], "结果中不能出现重复碰撞体,index:" + i + "," + j);
			}
		}

		destroyTestObjects();
	}

	// ─── 移动路径外的目标不能误判 ─────────────────────────────────────────
	private static void testTargetOutsideMovementPath()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		SphereCollider source = createMovingSphere(
			"OutsidePath_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		SphereCollider target = createSphere(
			"OutsidePath_Target",
			TEST_ORIGIN + Vector3.up * 2.0f,
			0.25f,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 0, "移动路径外的目标不应被检测到");
		assert(!containsCollider(results, count, target), "结果中不应包含移动路径外的目标");

		destroyTestObjects();
	}

	// ─── 斜向移动 ─────────────────────────────────────────────────────────
	private static void testDiagonalMovement()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + new Vector3(-5.0f, -5.0f, 0.0f);
		Vector3 currentWorldPos = TEST_ORIGIN + new Vector3(5.0f, 5.0f, 0.0f);

		SphereCollider source = createMovingSphere(
			"DiagonalMovement_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		SphereCollider target = createSphere(
			"DiagonalMovement_Target",
			TEST_ORIGIN,
			0.25f,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 1, "斜向移动时应命中路径中间的目标");
		assert(containsCollider(results, count, target), "斜向移动结果中应包含路径中间的目标");

		destroyTestObjects();
	}

	// ─── Collider.center偏移 ──────────────────────────────────────────────
	private static void testColliderCenterOffset()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		SphereCollider source = createMovingSphere(
			"CenterOffset_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		source.center = Vector3.up * 2.0f;

		SphereCollider target = createSphere(
			"CenterOffset_Target",
			TEST_ORIGIN + Vector3.up * 2.0f,
			0.25f,
			TARGET_LAYER);

		SphereCollider wrongTarget = createSphere(
			"CenterOffset_WrongTarget",
			TEST_ORIGIN,
			0.25f,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(containsCollider(results, count, target), "扫掠路径应包含Collider.center偏移");
		assert(!containsCollider(results, count, wrongTarget), "不应按Transform.position错误命中未偏移位置的目标");

		destroyTestObjects();
	}

	// ─── 移动期间旋转和缩放保持不变 ───────────────────────────────────────
	private static void testBoxFixedRotationAndScale()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.back * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.forward * 5.0f;

		BoxCollider source = createMovingBox(
			"RotationScale_Source",
			lastWorldPos,
			Vector3.one,
			SOURCE_LAYER);

		source.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
		source.transform.localScale = new(1.0f, 4.0f, 1.0f);

		SphereCollider target = createSphere(
			"RotationScale_Target",
			TEST_ORIGIN + Vector3.right * 1.5f,
			0.2f,
			TARGET_LAYER);

		SphereCollider wrongTarget = createSphere(
			"RotationScale_WrongTarget",
			TEST_ORIGIN + Vector3.up * 1.5f,
			0.2f,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(containsCollider(results, count, target), "固定旋转和缩放后的Box应命中实际扫过区域");
		assert(!containsCollider(results, count, wrongTarget), "固定旋转和缩放后的Box不应使用错误方向");

		destroyTestObjects();
	}

	// ─── 当前Overlap与Sweep结果必须去重 ───────────────────────────────────
	private static void testCurrentOverlapDoesNotDuplicate()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		BoxCollider source = createMovingBox(
			"Duplicate_Source",
			lastWorldPos,
			Vector3.one,
			SOURCE_LAYER);

		BoxCollider target = createBox(
			"Duplicate_Target",
			currentWorldPos + Vector3.right * 0.4f,
			Vector3.one,
			TARGET_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 1, "同一个目标同时被Overlap和Sweep检测到时只能返回一次");
		assert(countCollider(results, count, target) == 1, "重复目标在结果中只能出现一次");

		destroyTestObjects();
	}

	// ─── LayerMask ────────────────────────────────────────────────────────
	private static void testLayerMask()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		SphereCollider source = createMovingSphere(
			"LayerMask_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		SphereCollider includedTarget = createSphere(
			"LayerMask_IncludedTarget",
			TEST_ORIGIN + Vector3.left * 2.0f,
			0.25f,
			TARGET_LAYER);

		SphereCollider excludedTarget = createSphere(
			"LayerMask_ExcludedTarget",
			TEST_ORIGIN + Vector3.right * 2.0f,
			0.25f,
			OTHER_LAYER);

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(containsCollider(results, count, includedTarget), "LayerMask内的目标应被检测到");
		assert(!containsCollider(results, count, excludedTarget), "LayerMask外的目标不应被检测到");

		destroyTestObjects();
	}

	// ─── Trigger遵循Physics.queriesHitTriggers ────────────────────────────
	private static void testTrigger()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		SphereCollider source = createMovingSphere(
			"Trigger_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		BoxCollider target = createBox(
			"Trigger_Target",
			TEST_ORIGIN,
			new(0.2f, 4.0f, 4.0f),
			TARGET_LAYER);

		target.isTrigger = true;

		Collider[] results = new Collider[8];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		Physics.queriesHitTriggers = false;
		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 0, "queriesHitTriggers=false时不应返回Trigger");

		Physics.queriesHitTriggers = true;
		count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 1, "queriesHitTriggers=true时应返回Trigger");
		assert(containsCollider(results, count, target), "Trigger开启检测后结果中应包含目标");

		destroyTestObjects();
	}

	// ─── 每次调用前应清理results中的旧值 ──────────────────────────────────
	private static void testResultArrayOldValuesAreCleared()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 5.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 5.0f;

		SphereCollider source = createMovingSphere(
			"ClearResults_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		SphereCollider staleTarget = createSphere(
			"ClearResults_StaleTarget",
			TEST_ORIGIN + Vector3.up * 10.0f,
			0.25f,
			TARGET_LAYER);

		Collider[] results = new Collider[4]
		{
			staleTarget,
			staleTarget,
			staleTarget,
			staleTarget,
		};

		Physics.SyncTransforms();
		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == 0, "没有命中目标时应返回0");

		for (int i = 0; i < results.Length; ++i)
		{
			assert(results[i] == null, "没有命中时results旧值应被清空,index:" + i);
		}

		destroyTestObjects();
	}

	// ─── results容量不足时不能越界或重复 ──────────────────────────────────
	private static void testResultCapacity()
	{
		Vector3 lastWorldPos = TEST_ORIGIN + Vector3.left * 6.0f;
		Vector3 currentWorldPos = TEST_ORIGIN + Vector3.right * 6.0f;

		SphereCollider source = createMovingSphere(
			"Capacity_Source",
			lastWorldPos,
			0.5f,
			SOURCE_LAYER);

		Collider[] targets = new Collider[]
		{
			createSphere("Capacity_Target0", TEST_ORIGIN + Vector3.left * 3.0f, 0.25f, TARGET_LAYER),
			createSphere("Capacity_Target1", TEST_ORIGIN + Vector3.left, 0.25f, TARGET_LAYER),
			createSphere("Capacity_Target2", TEST_ORIGIN + Vector3.right, 0.25f, TARGET_LAYER),
			createSphere("Capacity_Target3", TEST_ORIGIN + Vector3.right * 3.0f, 0.25f, TARGET_LAYER),
		};

		Collider[] results = new Collider[2];
		Physics.SyncTransforms();

		moveCollider(source, currentWorldPos);

		int count = overlapCollider(source, lastWorldPos, results, TARGET_MASK);
		assert(count == results.Length, "命中数量超过results容量时返回值不能超过数组长度");
		assert(results[0] != null, "容量不足时第一个结果不能为空");
		assert(results[1] != null, "容量不足时第二个结果不能为空");
		assert(results[0] != results[1], "容量不足时结果中也不能出现重复碰撞体");
		assert(containsCollider(targets, targets.Length, results[0]), "第一个结果必须来自真实目标");
		assert(containsCollider(targets, targets.Length, results[1]), "第二个结果必须来自真实目标");

		destroyTestObjects();
	}

	// ─── 创建真实物理对象 ─────────────────────────────────────────────────
	private static GameObject createGameObject(string name, Vector3 position, int layer)
	{
		GameObject go = new(name);
		go.layer = layer;
		go.transform.position = position;
		mTestObjects.Add(go);
		return go;
	}

	private static Rigidbody addMovingRigidbody(GameObject go)
	{
		Rigidbody body = go.AddComponent<Rigidbody>();
		body.useGravity = false;
		body.isKinematic = true;
		body.detectCollisions = true;
		return body;
	}

	private static BoxCollider createMovingBox(string name, Vector3 position, Vector3 size, int layer)
	{
		GameObject go = createGameObject(name, position, layer);
		BoxCollider collider = go.AddComponent<BoxCollider>();
		collider.size = size;
		addMovingRigidbody(go);
		return collider;
	}

	private static SphereCollider createMovingSphere(string name, Vector3 position, float radius, int layer)
	{
		GameObject go = createGameObject(name, position, layer);
		SphereCollider collider = go.AddComponent<SphereCollider>();
		collider.radius = radius;
		addMovingRigidbody(go);
		return collider;
	}

	private static CapsuleCollider createMovingCapsule(
		string name,
		Vector3 position,
		float radius,
		float height,
		int direction,
		int layer)
	{
		GameObject go = createGameObject(name, position, layer);
		CapsuleCollider collider = go.AddComponent<CapsuleCollider>();
		collider.radius = radius;
		collider.height = height;
		collider.direction = direction;
		addMovingRigidbody(go);
		return collider;
	}

	private static BoxCollider createBox(string name, Vector3 position, Vector3 size, int layer)
	{
		GameObject go = createGameObject(name, position, layer);
		BoxCollider collider = go.AddComponent<BoxCollider>();
		collider.size = size;
		return collider;
	}

	private static SphereCollider createSphere(string name, Vector3 position, float radius, int layer)
	{
		GameObject go = createGameObject(name, position, layer);
		SphereCollider collider = go.AddComponent<SphereCollider>();
		collider.radius = radius;
		return collider;
	}

	// ─── 模拟上一帧到当前帧的真实位置变化 ───────────────────────────────────
	private static void moveCollider(Collider collider, Vector3 worldPosition)
	{
		assert(collider != null, "移动的Collider不能为空");

		// 直接修改Transform位置。
		// overlapCollider内部也是读取collider.transform.position计算移动距离，
		// 因此测试必须保证Transform已经移动到当前帧位置。
		collider.transform.position = worldPosition;

		// 将Transform的变化立即同步到Unity物理查询场景。
		Physics.SyncTransforms();

		assert(
			collider.transform.position.isEqual(worldPosition, 0.001f),
			"移动后Collider位置错误,expect:" +
			worldPosition +
			",actual:" +
			collider.transform.position);
	}

	// ─── 结果检查 ─────────────────────────────────────────────────────────
	private static bool containsCollider(Collider[] results, int count, Collider target)
	{
		for (int i = 0; i < count; ++i)
		{
			if (results[i] == target)
			{
				return true;
			}
		}
		return false;
	}

	private static int countCollider(Collider[] results, int count, Collider target)
	{
		int result = 0;
		for (int i = 0; i < count; ++i)
		{
			if (results[i] == target)
			{
				++result;
			}
		}
		return result;
	}
	// ─── 清理测试环境 ─────────────────────────────────────────────────────
	private static void destroyTestObjects()
	{
		for (int i = mTestObjects.Count - 1; i >= 0; --i)
		{
			if (mTestObjects[i] != null)
			{
				UObject.DestroyImmediate(mTestObjects[i]);
			}
		}
		mTestObjects.Clear();
		Physics.SyncTransforms();
	}
}
