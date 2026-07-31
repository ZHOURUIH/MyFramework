using UnityEngine;
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
		// null parent
		var list = new System.Collections.Generic.List<GameObject>();
		getAllGameObject(list, "any", null);

		GameObject parent = new GameObject("Root");
		GameObject a1 = new GameObject("A");
		a1.transform.SetParent(parent.transform, false);
		GameObject b = new GameObject("B");
		b.transform.SetParent(parent.transform, false);
		GameObject a2 = new GameObject("A");
		a2.transform.SetParent(b.transform, false);

		getAllGameObject(list, "A", parent, true);
		assertEqual(2, list.Count, "find A count=2");

		// 非递归
		list.Clear();
		getAllGameObject(list, "A", parent, false);
		assertEqual(1, list.Count, "find A nonrecursive count=1");

		Object.DestroyImmediate(parent);
	}

	// ─── getGameObjectWithTag ──────────────────────────────────────
	private static void testGetGameObjectWithTag()
	{
		// null parent
		var list = getGameObjectWithTag(null, "Untagged");
		assertEqual(0, list.Count, "null parent empty");

		GameObject parent = new GameObject("Root");
		GameObject child = new GameObject("Child");
		child.transform.SetParent(parent.transform, false);
		child.tag = "Untagged";

		var result = getGameObjectWithTag(parent, "Player");
		assertEqual(0, result.Count, "no match");

		result = getGameObjectWithTag(parent, "Untagged");
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

		GameObject found = getGameObjectInParent(parent, "Child");
		assertNotNull(found, "found child");
		assertEqual("Child", found.name, "child name");

		GameObject notFound = getGameObjectInParent(parent, "Missing");
		assertNull(notFound, "not found");

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
		GameObject go = new GameObject();
		go.transform.position = new Vector3(10f, 20f, 30f);

		GameObject parent = new GameObject();
		parent.transform.position = new Vector3(5f, 5f, 5f);

		Vector3 local = generateLocalPosition(go.transform, go.transform.position);
		assertEqual(5f, local.x, 0.001f, "local X=5");
		assertEqual(15f, local.y, 0.001f, "local Y=15");
		assertEqual(25f, local.z, 0.001f, "local Z=25");

		Object.DestroyImmediate(go);
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

		setParticleSortLayerID(root, 5);
		assertEqual(5, mr.sortingLayerID, "sorting layer=5");

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
}
