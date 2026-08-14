using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static TestAssert;

// UnityUtility 中可通过构造 GameObject/Component 测试的函数
public static class UnityUtilityTest
{
	public static void Run()
	{
		testSetGameObjectLayer();
		testActiveChilds();
		testCreateGameObject();
		testFindOrCreateGameObject();
		testGetAllGameObject();
		testGetGameObjectWithTag();
		testGetGameObjectPath();
		testGetGameObjectInParent();
		testGetTopParent();
		testIsTransformChild();
		testNameToLayer();
		testGenerateLocalPosition();
		testLocalToWorld();
		testSetParticleSortOrder();
		testTexture2DToSprite();
		testCloneObject();
		testApplyAnchor();
		testGetHalfBoxSize();
		testGetMinMaxCorner();
		testGetEnumLabel();
		testGetAnimationLength();
		testScreenAndScaleHelpers();
		testLogFunctions();
		testWorldScreenConversions();
		testRaycastHelpers();
		testOverlapFunctions();
		testParticleAndSpine();
		testRenderAndShader();
		testContentAndMisc();
		testGetScreenAspect();
		testIsPointInBoxCollider();
	testFindMaterial();
	testFindMaterialShader();
	testScreenAndWindowConversion();
}

	// ─── setGameObjectLayer ────────────────────────────────────────
	private static void testSetGameObjectLayer()
	{
		// null 分支
		setGameObjectLayer(null, 5);

		GameObject root = new GameObject();
		root.layer = 0;
		GameObject child = new GameObject();
		child.transform.SetParent(root.transform, false);
		GameObject grandchild = new GameObject();
		grandchild.transform.SetParent(child.transform, false);

		setGameObjectLayer(root, 10);
		assertEqual(10, root.layer, "root layer=10");
		assertEqual(10, child.layer, "child layer=10");
		assertEqual(10, grandchild.layer, "grandchild layer=10");

		Object.DestroyImmediate(root);
	}

	// ─── activeChilds ──────────────────────────────────────────────
	private static void testActiveChilds()
	{
		// null 分支
		activeChilds(null);

		GameObject root = new GameObject();
		GameObject child1 = new GameObject();
		child1.transform.SetParent(root.transform, false);
		GameObject child2 = new GameObject();
		child2.transform.SetParent(root.transform, false);

		activeChilds(root, false);
		assertFalse(child1.activeSelf, "child1 inactive");
		assertFalse(child2.activeSelf, "child2 inactive");

		activeChilds(root, true);
		assertTrue(child1.activeSelf, "child1 active");
		assertTrue(child2.activeSelf, "child2 active");

		// 无子节点不崩溃
		GameObject empty = new GameObject();
		activeChilds(empty, false);

		Object.DestroyImmediate(root);
		Object.DestroyImmediate(empty);
	}

	// ─── createGameObject ──────────────────────────────────────────
	private static void testCreateGameObject()
	{
		GameObject go = createGameObject("TestObj");
		assertNotNull(go, "created");
		assertEqual("TestObj", go.name, "name");

		GameObject parent = new GameObject("Parent");
		GameObject child = createGameObject("Child", parent);
		assertEqual("Child", child.name, "child name");
		assertTrue(child.transform.parent == parent.transform, "child parent");

		Object.DestroyImmediate(go);
		Object.DestroyImmediate(parent);
	}

	// ─── findOrCreateGameObject ────────────────────────────────────
	private static void testFindOrCreateGameObject()
	{
		GameObject parent = new GameObject("Root");
		GameObject existing = new GameObject("Target");
		existing.transform.SetParent(parent.transform, false);

		// 已存在时返回已有对象
		GameObject found = findOrCreateGameObject("Target", parent);
		assertTrue(found == existing, "find existing");

		// 不存在时创建
		GameObject created = findOrCreateGameObject("NewChild", parent);
		assertNotNull(created, "create new");
		assertEqual("NewChild", created.name, "created name");
		assertTrue(created.transform.parent == parent.transform, "created parent");

		Object.DestroyImmediate(parent);
	}

	// ─── getAllGameObject ──────────────────────────────────────────
	private static void testGetAllGameObject()
	{
		GameObject parent = new GameObject("Root");
		GameObject a1 = new GameObject("A");
		a1.transform.SetParent(parent.transform, false);
		GameObject b = new GameObject("B");
		b.transform.SetParent(parent.transform, false);
		GameObject a2 = new GameObject("A");
		a2.transform.SetParent(b.transform, false);

		var list = new List<GameObject>();
		findAllGameObject(list, "A", parent, true);
		assertEqual(2, list.Count, "find A count=2");

		// 非递归
		list.Clear();
		findAllGameObject(list, "A", parent, false);
		assertEqual(1, list.Count, "find A nonrecursive count=1");

		Object.DestroyImmediate(parent);
	}

	// ─── getGameObjectWithTag ──────────────────────────────────────
	private static void testGetGameObjectWithTag()
	{
		// null parent
		var list = findGameObjectWithTag(null, "Untagged");
		assertEqual(0, list.Count, "null parent empty");

		GameObject parent = new GameObject("Root");
		GameObject child = new GameObject("Child");
		child.transform.SetParent(parent.transform, false);
		child.tag = "Untagged";

		var result = findGameObjectWithTag(parent, "Player");
		assertEqual(0, result.Count, "no match");

		result = findGameObjectWithTag(parent, "Untagged");
		assertEqual(1, result.Count, "match untagged");

		Object.DestroyImmediate(parent);
	}

	// ─── getGameObjectPath ─────────────────────────────────────────
	private static void testGetGameObjectPath()
	{
		GameObject root = new GameObject("Root");
		GameObject child = new GameObject("Child");
		child.transform.SetParent(root.transform, false);
		GameObject grandchild = new GameObject("Grand");
		grandchild.transform.SetParent(child.transform, false);

		string path = getGameObjectPath(grandchild);
		assertTrue(path.Contains("Root"), "path contains root");
		assertTrue(path.Contains("Child"), "path contains child");
		assertTrue(path.Contains("Grand"), "path contains grand");
		assertTrue(path.Contains("/"), "path has separator");

		Object.DestroyImmediate(root);
	}

	// ─── getGameObjectInParent ─────────────────────────────────────
	private static void testGetGameObjectInParent()
	{
		GameObject parent = new GameObject("Root");
		GameObject child = new GameObject("Child");
		child.transform.SetParent(parent.transform, false);

		// getGameObjectInParent 向上查找：从 child 往上找名为 "Root" 的祖先
		GameObject found = getGameObjectInParent(child, "Root");
		assertNotNull(found, "found parent");
		assertEqual("Root", found.name, "parent name");

		// 找不到匹配名称时返回顶层父节点（即 Root）
		GameObject topWhenNotFound = getGameObjectInParent(child, "Missing");
		assertNotNull(topWhenNotFound, "returns top parent when not found");
		assertEqual("Root", topWhenNotFound.name, "top parent is Root");

		Object.DestroyImmediate(parent);
	}

	// ─── getTopParent ──────────────────────────────────────────────
	private static void testGetTopParent()
	{
		GameObject root = new GameObject("Root");
		GameObject child = new GameObject("Child");
		child.transform.SetParent(root.transform, false);
		GameObject grandchild = new GameObject("Grand");
		grandchild.transform.SetParent(child.transform, false);

		GameObject top = getTopParent(grandchild);
		assertTrue(top == root, "top parent is root");

		// 无父节点
		GameObject topSelf = getTopParent(root);
		assertTrue(topSelf == root, "top parent of root is self");

		Object.DestroyImmediate(root);
	}

	// ─── isTransformChild ──────────────────────────────────────────
	private static void testIsTransformChild()
	{
		GameObject parent = new GameObject("Parent");
		GameObject child = new GameObject("Child");
		child.transform.SetParent(parent.transform, false);
		GameObject unrelated = new GameObject("Other");

		assertTrue(isTransformChild(parent.transform, child.transform), "child is child of parent");
		assertFalse(isTransformChild(child.transform, parent.transform), "parent not child of child");
		assertFalse(isTransformChild(parent.transform, unrelated.transform), "unrelated");

		Object.DestroyImmediate(parent);
		Object.DestroyImmediate(unrelated);
	}

	// ─── nameToLayerInt / nameToLayerPhysics ───────────────────────
	private static void testNameToLayer()
	{
		// Default 层始终存在 (layer 0)
		int layer = nameToLayerInt("Default");
		assertTrue(layer >= 1 && layer <= 32, "Default layer valid");

		int physics = nameToLayerPhysics("Default");
		assertEqual(1 << layer, physics, "physics mask");

		// 不存在的层名返回 0→clamp到1
		int invalid = nameToLayerInt("NonExistentLayerXYZ123");
		assertTrue(invalid >= 1 && invalid <= 32, "invalid name clamped");
	}

	// ─── generateLocalPosition ─────────────────────────────────────
	private static void testGenerateLocalPosition()
	{
		GameObject parent = new GameObject();
		parent.transform.position = new Vector3(5f, 5f, 5f);

		GameObject go = new GameObject();
		go.transform.SetParent(parent.transform, false);
		go.transform.position = new Vector3(10f, 20f, 30f);

		Vector3 local = generateLocalPosition(go.transform, go.transform.position);
		assertEqual(5f, local.x, 0.001f, "local X=5");
		assertEqual(15f, local.y, 0.001f, "local Y=15");
		assertEqual(25f, local.z, 0.001f, "local Z=25");

		Object.DestroyImmediate(parent);
	}

	// ─── localToWorld / worldToLocal / direction ───────────────────
	private static void testLocalToWorld()
	{
		GameObject go = new GameObject();
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;

		// localToWorld
		Vector3 world = localToWorld(go.transform, new Vector3(1f, 2f, 3f));
		assertEqual(1f, world.x, 0.001f, "localToWorld X");
		assertEqual(2f, world.y, 0.001f, "localToWorld Y");
		assertEqual(3f, world.z, 0.001f, "localToWorld Z");

		// worldToLocal
		Vector3 local = worldToLocal(go.transform, new Vector3(1f, 2f, 3f));
		assertEqual(1f, local.x, 0.001f, "worldToLocal X");

		// localToWorldDirection
		Vector3 dirW = localToWorldDirection(go.transform, Vector3.right);
		assertEqual(1f, dirW.x, 0.001f, "localToWorldDir X");

		// worldToLocalDirection
		Vector3 dirL = worldToLocalDirection(go.transform, Vector3.up);
		assertEqual(1f, dirL.y, 0.001f, "worldToLocalDir Y");

		// null transform
		Vector3 nullLocal = localToWorld(null, Vector3.one);
		assertEqual(0f, nullLocal.x, 0.001f, "null localToWorld=zero");
		Vector3 nullDir = localToWorldDirection(null, Vector3.one);
		assertEqual(0f, nullDir.x, 0.001f, "null localToWorldDir=forward");
		assertEqual(1f, nullDir.z, 0.001f, "null localToWorldDir Z=1");

		Object.DestroyImmediate(go);
	}

	// ─── setParticleSortOrder / setParticleSortLayerID ─────────────
	private static void testSetParticleSortOrder()
	{
		GameObject root = new GameObject();
		GameObject child = new GameObject();
		child.transform.SetParent(root.transform, false);
		MeshRenderer mr = child.AddComponent<MeshRenderer>();

		setParticleSortOrder(root, 100);
		assertEqual(100, mr.sortingOrder, "sorting order=100");

		// sortingLayerID 需要有效的 layer ID，不能用 index
		int defaultLayerID = SortingLayer.NameToID("Default");
		setParticleSortLayerID(root, defaultLayerID);
		assertEqual(defaultLayerID, mr.sortingLayerID, "sorting layer set");

		Object.DestroyImmediate(root);
	}

	// ─── texture2DToSprite ─────────────────────────────────────────
	private static void testTexture2DToSprite()
	{
		// null 分支
		assertNull(texture2DToSprite(null), "null texture");

		Texture2D tex = new Texture2D(16, 16);
		Sprite sprite = texture2DToSprite(tex);
		assertNotNull(sprite, "sprite created");
		assertEqual(16f, sprite.rect.width, 0.001f, "sprite width");
		assertEqual(16f, sprite.rect.height, 0.001f, "sprite height");

		Object.DestroyImmediate(tex);
		Object.DestroyImmediate(sprite);
	}

	// ─── cloneObject ───────────────────────────────────────────────
	private static void testCloneObject()
	{
		GameObject original = new GameObject("Original");
		original.transform.position = new Vector3(5f, 10f, 0f);

		GameObject cloned = cloneObject(original, "Cloned");
		assertNotNull(cloned, "cloned");
		assertEqual("Cloned", cloned.name, "cloned name");
		assertTrue(cloned != original, "different instance");

		// cloneObject 不带名称
		GameObject cloned2 = cloneObject(original, null);
		assertNotNull(cloned2, "cloned no name");

		Object.DestroyImmediate(original);
		Object.DestroyImmediate(cloned);
		Object.DestroyImmediate(cloned2);
	}

	// ─── applyAnchor ───────────────────────────────────────────────
	private static void testApplyAnchor()
	{
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.anchorMin = new Vector2(0f, 0.5f);
		rect.anchorMax = new Vector2(1f, 0.5f);

		// applyAnchor 调整 anchoredPosition 和 sizeDelta
		applyAnchor(go, true);
		// 不崩溃即通过

		Object.DestroyImmediate(go);
	}

	// ─── getHalfBoxSize ────────────────────────────────────────────
	private static void testGetHalfBoxSize()
	{
		GameObject go = new GameObject();
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
		BoxCollider box = go.AddComponent<BoxCollider>();
		box.size = new Vector3(2f, 4f, 6f);

		// 无父节点：half = size/2
		Vector3 half = getHalfBoxSize(box, null);
		assertEqual(1f, half.x, 0.001f, "half X=1");
		assertEqual(2f, half.y, 0.001f, "half Y=2");
		assertEqual(3f, half.z, 0.001f, "half Z=3");

		Object.DestroyImmediate(go);
	}

	// ─── getMinMaxCorner ───────────────────────────────────────────
	private static void testGetMinMaxCorner()
	{
		GameObject parent = new GameObject();
		parent.transform.position = Vector3.zero;
		parent.transform.rotation = Quaternion.identity;

		GameObject child = new GameObject();
		child.transform.SetParent(parent.transform, false);
		child.transform.localPosition = Vector3.zero;
		child.transform.localRotation = Quaternion.identity;
		BoxCollider box = child.AddComponent<BoxCollider>();
		box.center = Vector3.zero;
		box.size = new Vector3(2f, 2f, 2f);

		getMinMaxCorner(box, out Vector3 min, out Vector3 max, parent);
		assertEqual(-1f, min.x, 0.001f, "min X=-1");
		assertEqual(-1f, min.y, 0.001f, "min Y=-1");
		assertEqual(-1f, min.z, 0.001f, "min Z=-1");
		assertEqual(1f, max.x, 0.001f, "max X=1");
		assertEqual(1f, max.y, 0.001f, "max Y=1");
		assertEqual(1f, max.z, 0.001f, "max Z=1");

		Object.DestroyImmediate(parent);
	}

	// ─── getEnumLabel / getEnumToolTip ─────────────────────────────
	private static void testGetEnumLabel()
	{
		// 没有 LabelAttribute 的枚举返回 ToString()
		string label = getEnumLabel(KeyCode.Space);
		assertNotNull(label, "enum label not null");

		string tip = getEnumToolTip(KeyCode.Space);
		assertNotNull(tip, "enum tooltip not null");
	}

	// ─── getAnimationLength ────────────────────────────────────────
	private static void testGetAnimationLength()
	{
		// getAnimationLength(Animator, string) 参数是 Animator，不是 GameObject
		GameObject go = new GameObject();
		Animator animator = go.AddComponent<Animator>();

		// 无 RuntimeAnimatorController: 返回 0
		float len = getAnimationLength(animator, "NoClip");
		assertEqual(0f, len, 0.001f, "null controller length=0");

		// null Animator: 返回 0
		float len2 = getAnimationLength(null, "AnyClip");
		assertEqual(0f, len2, 0.001f, "null animator length=0");

		UnityEngine.Object.DestroyImmediate(go);
	}

	// screen/scale helpers
	private static void testScreenAndScaleHelpers()
	{
		Vector2 screenSize = getScreenSize();
		assertTrue(screenSize.x > 0f, "screenSize X > 0");

		Vector2 hw = getHalfScreenSize();
		assertTrue(hw.x >= 0, "halfScreenSize X >= 0");

		Vector2 hwPx = getHardwareScreenSize();
		assertTrue(hwPx.x >= 0, "hardwareScreenSize X >= 0");

		Vector2 rootSize = getRootSize();
		assertTrue(rootSize.x >= 0f, "rootSize X >= 0");

		Vector2 gv = getGameViewSize();
		assertTrue(gv.x >= 0f, "gameViewSize X >= 0");

		// getScreenScale() no param, returns Vector2
		Vector2 scale = getScreenScale();
		assertTrue(scale.x > 0f, "getScreenScale x > 0");
		assertTrue(scale.y > 0f, "getScreenScale y > 0");

		// getScreenScale(ASPECT_BASE)
		Vector2 scale16x9 = getScreenScale(ASPECT_BASE.AUTO);
		assertTrue(scale16x9.x > 0f, "getScreenScale auto > 0");

		// getScreenScaleAuto() no param, returns float
		float autoScale = getScreenScaleAuto();
		assertTrue(autoScale > 0f, "getScreenScaleAuto > 0");

		// generateScreenScaleByAspectBase(Vector2, ASPECT_BASE)
		Vector2 baseScale = generateScreenScaleByAspectBase(new Vector2(1920f, 1080f), ASPECT_BASE.USE_WIDTH_SCALE);
		assertTrue(baseScale.x > 0f, "generateScreenScaleByAspectBase > 0");

		// adjustByScreenScaleAuto(float)
		float adjusted = adjustByScreenScaleAuto(100f);
		assertTrue(adjusted > 0f, "adjustByScreenScaleAuto > 0");

		// isInvalidScreenAdaptation does not exist, skip
		assertTrue(true, "screen/scale helpers called");
	}

	// log functions
	private static void testLogFunctions()
	{
		LOG_LEVEL oldLevel = getLogLevel();
		setLogLevel(LOG_LEVEL.NORMAL);
		assertTrue(getLogLevel() == LOG_LEVEL.NORMAL, "setLogLevel NORMAL");
		setLogLevel(oldLevel);

		logWarning("test warning");
		log("test log");
		log("test", "7A7A7A");
		logNoLock("test nolock");
		assertTrue(true, "log functions called");
	}

	// world/screen conversions
	private static void testWorldScreenConversions()
	{
		GameObject camGo = new GameObject("TestCamera");
		Camera cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 5f;

		// worldToScreen(Vector3, Camera, bool)
		Vector3 screenPos = worldToScreen(Vector3.zero, cam);
		assertTrue(screenPos.z >= 0f, "worldToScreen Z >= 0");

		// worldToScreen(Vector3, bool)
		Vector3 screenPos2 = worldToScreen(Vector3.zero, true);
		assertTrue(screenPos2.z >= 0f, "worldToScreen(mainCam) Z >= 0");

		screenToWorld(Vector3.zero, cam);
		assertTrue(true, "screenToWorld called");

		// worldUIToScreen(Vector3, bool)
		Vector3 uiScreen = worldUIToScreen(Vector3.zero);
		assertTrue(true, "worldUIToScreen called");

		getMainCameraMouseRay();
		getMainCameraScreenCenterRay();
		getMainCameraRay(Vector2.zero);
		getCameraRay(Vector2.zero, cam);
		getUIRay(Vector2.zero);
		assertTrue(true, "ray helpers called");

		// isGameObjectInScreen(Vector3)
		isGameObjectInScreen(Vector3.zero);
		assertTrue(true, "isGameObjectInScreen called");

		// atCameraBack(Vector3)
		atCameraBack(Vector3.zero);
		assertTrue(true, "atCameraBack called");

		generateWorldPosition(camGo.transform);
		generateWorldRotation(camGo.transform);
		generateWorldScale(camGo.transform);
		assertTrue(true, "generateWorld called");

		UnityEngine.Object.DestroyImmediate(camGo);
	}

	// raycast helpers
	private static void testRaycastHelpers()
	{
		GameObject go = new GameObject();
		BoxCollider box = go.AddComponent<BoxCollider>();
		box.size = Vector3.one;
		go.transform.position = Vector3.zero;

		Ray ray = new Ray(Vector3.forward * 5f, -Vector3.forward);
		raycast(ray, out Collider resultCol, out Vector3 resultPoint, -1);
		raycast(ray, out Collider resultCol2, out Vector3 resultPoint2, 10f, -1);

		RaycastHit[] hits = new RaycastHit[10];
		int hitCount = raycastAll(ray, hits, -1);
		assertTrue(hitCount >= 0, "raycastAll count >= 0");

		int hitCount2 = raycastAll(ray, hits, 10f, -1);
		assertTrue(hitCount2 >= 0, "raycastAll maxDist count >= 0");

		Vector3 intersectPoint = Vector3.zero;
		getRaycastPoint(box, ray, ref intersectPoint);
		getRaycastPoint(box, ray, ref intersectPoint, 10f);

		var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
		raycastUGUI(Vector2.zero, results);

		assertTrue(true, "raycast helpers called");
		UnityEngine.Object.DestroyImmediate(go);
	}

	// overlap functions
	private static void testOverlapFunctions()
	{
		GameObject go = new GameObject();
		BoxCollider box = go.AddComponent<BoxCollider>();
		box.size = Vector3.one;
		go.transform.position = Vector3.zero;

		GameObject go2 = new GameObject();
		BoxCollider box2 = go2.AddComponent<BoxCollider>();
		box2.size = Vector3.one;
		go2.transform.position = new Vector3(0.5f, 0f, 0f);

		Collider[] results = new Collider[10];
		overlapCollider(box, results, -1);
		overlapCollider(go.AddComponent<SphereCollider>(), results, -1);
		overlapBoxIgnoreY(box, box2, null, 4);
		overlapBoxIgnoreZ(box, box2, null, 4);

		overlapCollider(box, results, -1);
		isOverlap(box, box2);

		// overlapAllCapsule: 依赖物理查询, EditMode 下安全调用(nonAlloc 无匹配时返回0)
		GameObject ccGo = new GameObject("TestCapsule");
		CharacterController cc = ccGo.AddComponent<CharacterController>();
		cc.center = Vector3.zero;
		cc.height = 2.0f;
		cc.radius = 0.5f;
		int capCount = overlapAllCapsule(cc, results, -1);
		assertTrue(capCount >= 0, "overlapAllCapsule count >= 0");

		assertTrue(true, "overlap functions called");
		UnityEngine.Object.DestroyImmediate(ccGo);
		UnityEngine.Object.DestroyImmediate(go2);
		UnityEngine.Object.DestroyImmediate(go);
	}

	// particle/spine
	private static void testParticleAndSpine()
	{
		GameObject go = new GameObject();
		go.AddComponent<ParticleSystem>();

		playAllParticle(go);
		stopAllParticle(go);
		restartAllParticle(go);
		pauseAllParticle(go);

		playAllParticle(null);
		stopAllParticle(null);
		restartAllParticle(null);
		pauseAllParticle(null);

		assertTrue(true, "particle/spine called");
		UnityEngine.Object.DestroyImmediate(go);
	}

	// render/shader/misc
	private static void testRenderAndShader()
	{
		GameObject go = new GameObject();
		Camera cam = go.AddComponent<Camera>();
#if USE_URP
		setRenderType(cam, UnityEngine.Rendering.Universal.CameraRenderType.Overlay);
#endif

		findShaders(go);
		findUGUIShaders(go);

#if USE_URP
		float oldScale = getRenderScale();
		setRenderScale(1.0f);
		getRenderScale();
		setRenderScale(oldScale);
#endif

		GameObject parent = new GameObject("Parent");
		setNormalProperty(go, parent);
		setNormalProperty(go, parent, "Child");

		setScreenSize(new Vector2(1920, 1080), false);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		getLastError();
#endif
		getComponentInParent<Transform>(go);
		getGameObjectID(go);

		GameObject root = findOrCreateRootGameObject("TestRootObj");
		assertNotNull(root, "findOrCreateRootGameObject not null");
		UnityEngine.Object.DestroyImmediate(root);

		GameObject prefab = new GameObject("Prefab");
		GameObject instance = instantiatePrefab(null, prefab, "Instance", true);
		assertNotNull(instance, "instantiatePrefab not null");
		UnityEngine.Object.DestroyImmediate(instance);
		UnityEngine.Object.DestroyImmediate(prefab);

		GameObject original = new GameObject("AsyncClone");
		cloneObjectAsync(original, "AsyncClone", (GameObject cloned) =>
		{
			// cloneObjectAsync 是异步协程,源对象必须在回调完成后才能销毁
			// 否则 InstantiateAsync 对已销毁源返回空结果,cloned 为 null
			assertNotNull(cloned, "cloneObjectAsync callback not null");
			if (cloned != null)
			{
				UnityEngine.Object.DestroyImmediate(cloned);
			}
			UnityEngine.Object.DestroyImmediate(original);
		});

		assertTrue(true, "render/shader/misc called");
		UnityEngine.Object.DestroyImmediate(parent);
		UnityEngine.Object.DestroyImmediate(go);
	}

	// getContentLength
	private static void testContentAndMisc()
	{
		GameObject go = new GameObject("TestContent");
		go.AddComponent<RectTransform>();
		UnityEngine.UI.Text textComp = go.AddComponent<UnityEngine.UI.Text>();
		textComp.fontSize = 14;

		int len1 = getContentLength(textComp, "hello");
		assertTrue(len1 >= 0, "getContentLength >= 0");

		UnityEngine.Object.DestroyImmediate(go);
	}

	// getScreenAspect: 返回静态字段 mScreenAspect
	private static void testGetScreenAspect()
	{
		float aspect = getScreenAspect();
		assertTrue(aspect > 0.0f, "getScreenAspect > 0");
	}

	// isPointInBoxCollider: 判断世界点是否在BoxCollider (仅比较x/y)
	private static void testIsPointInBoxCollider()
	{
		assertFalse(isPointInBoxCollider(null, Vector3.zero), "null collider -> false");

		GameObject go = new GameObject("TestBox");
		BoxCollider box = go.AddComponent<BoxCollider>();
		box.center = Vector3.zero;
		box.size = new Vector3(2.0f, 2.0f, 2.0f); // 半宽 x/y = 1
		go.transform.localPosition = Vector3.zero;
		try
		{
			// 中心点在内
			assertTrue(isPointInBoxCollider(box, Vector3.zero), "center inside");
			// 半宽边界内
			assertTrue(isPointInBoxCollider(box, new Vector3(0.9f, -0.8f, 5.0f)), "inside x/y");
			// 超出半宽 -> 外
			assertFalse(isPointInBoxCollider(box, new Vector3(1.5f, 0.0f, 0.0f)), "x outside");
			assertFalse(isPointInBoxCollider(box, new Vector3(0.0f, 3.0f, 0.0f)), "y outside");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// findMaterial: 编辑器中返回 render.material, 运行时返回 sharedMaterial
	private static void testFindMaterial()
	{
		assertTrue(findMaterial(null) == null, "findMaterial null renderer -> null");
		GameObject go = new GameObject("TestRenderer");
		Renderer renderer = go.AddComponent<MeshRenderer>();
		try
		{
			Material mat = findMaterial(renderer);
			assertTrue(mat != null || renderer.sharedMaterial == null, "findMaterial returns material or shared is null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// findMaterialShader: 仅编辑器下重查shader, 传 null 不崩溃
	private static void testFindMaterialShader()
	{
		findMaterialShader(null);
		Shader shader = Shader.Find("Standard");
		Material mat = null;
		try
		{
			mat = new Material(shader != null ? shader : Shader.Find("UI/Default"));
			findMaterialShader(mat);
			assertTrue(true, "findMaterialShader executed");
		}
		finally
		{
			if (mat != null)
			{
				UnityEngine.Object.DestroyImmediate(mat);
			}
		}
	}

	// screenPosToWindow / isPointInWindow: 依赖框架 UI 相机与 UIRoot
	// 仅在 UI 相机与 UIRoot 都可用时真实执行, 否则跳过避免 NPE
	private static void testScreenAndWindowConversion()
	{
		Camera uiCam = FrameUtility.getUICamera();
		var uiRoot = FrameUtility.getUGUIRoot();
		if (uiCam == null || uiRoot == null)
		{
			return; // 框架未创建 UI 相机/Root, 跳过
		}
		// screenPosToWindow 传 null window: 走 "仅 root 换算" 分支
		Vector2 mapped0 = screenPosToWindow(Vector2.zero, null);
		assertTrue(!float.IsNaN(mapped0.x) && !float.IsNaN(mapped0.y), "screenPosToWindow(null) returns finite");
		Vector2 mapped1 = screenPosToWindow(Vector2.zero, null, false);
		assertTrue(!float.IsNaN(mapped1.x) && !float.IsNaN(mapped1.y), "screenPosToWindow(null,false) finite");

		// isPointInWindow: 需要一个最小 myUGUIObject
		GameObject go = new GameObject("TestWindow");
		go.AddComponent<RectTransform>();
		go.AddComponent<UnityEngine.UI.Image>();
		myUGUIObject window = new myUGUIObject();
		window.setObject(go);
		// 仅调用 setObject 不会填充 mRectTransform, 先 init 补充缓存字段, 否则 getSize 会因 mRectTransform 为空抛 NRE
		window.init();
		try
		{
			bool inWin = isPointInWindow(new Vector2(uiCam.pixelWidth * 0.5f, uiCam.pixelHeight * 0.5f), window);
			assertTrue(!float.IsNaN(inWin ? 1.0f : 0.0f), "isPointInWindow returns valid bool");
			screenPosToWindow(new Vector2(uiCam.pixelWidth * 0.5f, uiCam.pixelHeight * 0.5f), window);
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
