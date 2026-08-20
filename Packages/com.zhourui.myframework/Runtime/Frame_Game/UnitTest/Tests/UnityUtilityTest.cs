using UnityEngine;
using static TestAssert;

// Frame_Game 精简层 UnityUtility 测试(纯逻辑/null 安全/EditMode Transform)
public static class UnityUtilityTest
{
	public static void Run()
	{
		testAdjustScaleNone();
		testAdjustScaleUseHeight();
		testAdjustScaleUseWidth();
		testAdjustScaleAuto();
		testAdjustScaleInverseAuto();
		testLocalToWorldNull();
		testWorldToLocalNull();
		testLocalToWorldIdentity();
		testGetMethodRecursive();
		testInstantiatePrefab();
		testLocalToWorldTranslation();
	}

	// NONE: 原值, z=y
	static void testAdjustScaleNone()
	{
		Vector3 r = UnityUtility.adjustScreenScale(new Vector2(2f, 3f), ASPECT_BASE.NONE);
		assertEqual(new Vector3(2f, 3f, 3f), r, "NONE 原值 z=y");
	}

	// USE_HEIGHT_SCALE: x=y=height
	static void testAdjustScaleUseHeight()
	{
		Vector3 r = UnityUtility.adjustScreenScale(new Vector2(2f, 3f), ASPECT_BASE.USE_HEIGHT_SCALE);
		assertEqual(new Vector3(3f, 3f, 3f), r, "按高度缩放");
	}

	// USE_WIDTH_SCALE: x=y=width
	static void testAdjustScaleUseWidth()
	{
		Vector3 r = UnityUtility.adjustScreenScale(new Vector2(2f, 3f), ASPECT_BASE.USE_WIDTH_SCALE);
		assertEqual(new Vector3(2f, 2f, 2f), r, "按宽度缩放");
	}

	// AUTO: min
	static void testAdjustScaleAuto()
	{
		Vector3 r = UnityUtility.adjustScreenScale(new Vector2(5f, 2f));
		assertEqual(new Vector3(2f, 2f, 2f), r, "AUTO 取最小");
	}

	// INVERSE_AUTO: max
	static void testAdjustScaleInverseAuto()
	{
		Vector3 r = UnityUtility.adjustScreenScale(new Vector2(5f, 2f), ASPECT_BASE.INVERSE_AUTO);
		assertEqual(new Vector3(5f, 5f, 5f), r, "INVERSE_AUTO 取最大");
	}

	// localToWorld(null) → zero
	static void testLocalToWorldNull()
	{
		assertEqual(Vector3.zero, UnityUtility.localToWorld(null, Vector3.one), "null transform 返回 zero");
	}

	// worldToLocal(null) → zero
	static void testWorldToLocalNull()
	{
		assertEqual(Vector3.zero, UnityUtility.worldToLocal(null, Vector3.one), "null transform 返回 zero");
	}

	// 单位变换: local == world
	static void testLocalToWorldIdentity()
	{
		var go = new GameObject("UUT_Identity");
		try
		{
			go.transform.position = Vector3.zero;
			go.transform.rotation = Quaternion.identity;
			Vector3 r = UnityUtility.localToWorld(go.transform, new Vector3(1f, 2f, 3f));
			assertEqual(new Vector3(1f, 2f, 3f), r, "单位变换 local==world");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}

	// getMethodRecursive: 沿继承链找方法(注意: 多重重载方法会 AmbiguousMatchException——用无重载方法)
	static void testGetMethodRecursive()
	{
		System.Reflection.MethodInfo m = UnityUtility.getMethodRecursive(typeof(GameObject), "GetInstanceID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		assertNotNull(m, "GetInstanceID 应找到");
	}

	// instantiatePrefab: 克隆并重置 localPosition
	static void testInstantiatePrefab()
	{
		var prefab = new GameObject("UUT_Prefab");
		try
		{
			var clone = UnityUtility.instantiatePrefab(null, prefab, "UUT_Clone", true);
			try
			{
				assertNotNull(clone, "克隆非 null");
				assertEqual("UUT_Clone", clone.name, "克隆命名");
				assertTrue(clone.activeSelf, "active true");
				assertEqual(Vector3.zero, clone.transform.localPosition, "localPosition 重置");
			}
			finally
			{
				Object.DestroyImmediate(clone);
			}
		}
		finally
		{
			Object.DestroyImmediate(prefab);
		}
	}

	// 平移变换: world = local + position
	static void testLocalToWorldTranslation()
	{
		var go = new GameObject("UUT_Trans");
		try
		{
			go.transform.position = new Vector3(10f, 0f, 0f);
			go.transform.rotation = Quaternion.identity;
			Vector3 r = UnityUtility.localToWorld(go.transform, new Vector3(1f, 2f, 3f));
			assertEqual(new Vector3(11f, 2f, 3f), r, "平移后 world");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}
}
