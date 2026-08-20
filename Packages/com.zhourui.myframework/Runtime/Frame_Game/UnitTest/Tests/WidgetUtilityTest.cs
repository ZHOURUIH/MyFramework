using UnityEngine;
using static TestAssert;

// Frame_Game 精简层 WidgetUtility.cornerToSide 测试(纯几何)
public static class WidgetUtilityTest
{
	public static void Run()
	{
		testCornerToSideSquare();
		testCornerToSideRectangle();
		testCornerToSideWrongLengthNoOp();
		testCornerToSideSpanOverload();
		testSetGetPositionNoPivot();
		testSetGetPositionNoPivotPivotZero();
		testSetPositionNoPivotInParentNoParent();
		testSetRectSizeNullSafe();
		testSetRectSizeValid();
		testGetParentSizeNoParent();
	}

	// 正方形四角 → 四边中点
	static void testCornerToSideSquare()
	{
		var corners = new Vector3[4]
		{
			new(0f, 0f, 0f), new(2f, 0f, 0f), new(2f, 2f, 0f), new(0f, 2f, 0f)
		};
		var sides = new Vector3[4];
		WidgetUtility.cornerToSide(corners, sides);
		assertEqual(new Vector3(1f, 0f, 0f), sides[0], "side0 = 边中点");
		assertEqual(new Vector3(2f, 1f, 0f), sides[1], "side1 = 边中点");
		assertEqual(new Vector3(1f, 2f, 0f), sides[2], "side2 = 边中点");
		assertEqual(new Vector3(0f, 1f, 0f), sides[3], "side3 = 边中点");
	}

	// 矩形(非均匀)验证逐边中点
	static void testCornerToSideRectangle()
	{
		var corners = new Vector3[4]
		{
			new(0f, 0f, 0f), new(4f, 0f, 0f), new(4f, 2f, 0f), new(0f, 2f, 0f)
		};
		var sides = new Vector3[4];
		WidgetUtility.cornerToSide(corners, sides);
		assertEqual(new Vector3(2f, 0f, 0f), sides[0], "长边中点 x=2");
		assertEqual(new Vector3(4f, 1f, 0f), sides[1], "短边中点 y=1");
	}

	// sides 长度不为 4 → 不修改
	static void testCornerToSideWrongLengthNoOp()
	{
		var corners = new Vector3[4];
		var sides = new Vector3[2];
		WidgetUtility.cornerToSide(corners, sides);
		assertEqual(new Vector3(0f, 0f, 0f), sides[0], "长度不对不修改");
	}

	// Span 重载版
	static void testCornerToSideSpanOverload()
	{
		var corners = new Vector3[4]
		{
			new(0f, 0f, 0f), new(2f, 0f, 0f), new(2f, 2f, 0f), new(0f, 2f, 0f)
		};
		var sides = new Vector3[4];
		WidgetUtility.cornerToSide(corners, sides);
		assertEqual(new Vector3(1f, 0f, 0f), sides[0], "Span 版 side0");
	}

	// ═════════════════════════════════════════════════════════════════
	// RectTransform 深度(EditMode, 无布局依赖)
	// ═════════════════════════════════════════════════════════════════

	private static RectTransform NewRect(out GameObject go, Vector2 size, Vector2 pivot)
	{
		go = new GameObject("WU_Rect");
		var rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = size;
		rect.pivot = pivot;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		return rect;
	}

	// set/getPositionNoPivot 往返(pivot 0.5 直通)
	static void testSetGetPositionNoPivot()
	{
		RectTransform rect = NewRect(out GameObject go, new Vector2(100f, 50f), new Vector2(0.5f, 0.5f));
		try
		{
			WidgetUtility.setPositionNoPivot(rect, new Vector3(10f, 20f, 0f), false);
			Vector3 got = WidgetUtility.getPositionNoPivot(rect, false);
			assertEqual(new Vector3(10f, 20f, 0f), got, "pivot 0.5 往返");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}

	// pivot (0,0): localPosition 偏移 size/2
	static void testSetGetPositionNoPivotPivotZero()
	{
		RectTransform rect = NewRect(out GameObject go, new Vector2(100f, 50f), Vector2.zero);
		try
		{
			WidgetUtility.setPositionNoPivot(rect, new Vector3(10f, 20f, 0f), false);
			// pivot(0,0): localPosition = pos + size * (0 - 0.5) = pos - size/2
			assertEqual(new Vector3(-40f, -5f, 0f), rect.localPosition, "pivot(0,0) 偏移");
			Vector3 got = WidgetUtility.getPositionNoPivot(rect, false);
			assertEqual(new Vector3(10f, 20f, 0f), got, "还原 pos");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}

	// 无父节点: setPositionNoPivotInParent 走 setPositionNoPivot 分支
	static void testSetPositionNoPivotInParentNoParent()
	{
		RectTransform rect = NewRect(out GameObject go, new Vector2(80f, 40f), new Vector2(0.5f, 0.5f));
		try
		{
			WidgetUtility.setPositionNoPivotInParent(rect, new Vector3(5f, 6f, 0f), false);
			assertEqual(new Vector3(5f, 6f, 0f), rect.localPosition, "无父直通");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}

	// setRectSize(null) 安全
	static void testSetRectSizeNullSafe()
	{
		WidgetUtility.setRectSize(null, Vector2.zero);
		// 无异常即通过
	}

	// setRectSize: 无父时 sizeDelta = size
	static void testSetRectSizeValid()
	{
		RectTransform rect = NewRect(out GameObject go, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
		try
		{
			WidgetUtility.setRectSize(rect, new Vector2(200f, 100f));
			assertEqual(new Vector2(200f, 100f), rect.sizeDelta, "无父 sizeDelta = size");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}

	// getParentSize: 无父 → zero
	static void testGetParentSizeNoParent()
	{
		RectTransform rect = NewRect(out GameObject go, new Vector2(10f, 10f), Vector2.zero);
		try
		{
			assertEqual(Vector2.zero, WidgetUtility.getParentSize(rect), "无父返回 zero");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}
}
