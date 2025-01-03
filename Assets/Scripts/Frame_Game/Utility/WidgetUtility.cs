using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// RectTransform和UI相关的工具函数类
public class WidgetUtility
{
	// 父节点在父节点坐标系下的各条边
	public static void getParentSides(GameObject parent, Vector3[] sides)
	{
		parent.TryGetComponent<RectTransform>(out var trans);
		Vector2 size = trans.rect.size;
		Span<Vector3> tempCorners = stackalloc Vector3[4];
		tempCorners[0] = new(-size.x * 0.5f, -size.y * 0.5f);
		tempCorners[1] = new(-size.x * 0.5f, size.y * 0.5f);
		tempCorners[2] = new(size.x * 0.5f, size.y * 0.5f);
		tempCorners[3] = new(size.x * 0.5f, -size.y * 0.5f);
		cornerToSide(tempCorners, sides);
	}
	// 窗口是否在屏幕范围内,只检查位置和大小
	public static bool isWindowInScreen(myUIObject window, GameCamera camera)
	{
		Vector3 pos = worldToScreen(window.getWorldPosition(), camera.getCamera());
		return overlapBox2(pos, window.getWindowSize(), Vector3.zero, getScreenSize());
	}
	public static void setPositionNoPivot(RectTransform rect, Vector3 pos, bool applyWindowScale = true)
	{
		Vector2 windowSize = rect.rect.size;
		if (applyWindowScale)
		{
			windowSize = multiVector2(windowSize, rect.lossyScale);
		}
		rect.localPosition = round(pos + (Vector3)multiVector2(windowSize, rect.pivot - new Vector2(0.5f, 0.5f)));
	}
	public static Vector3 getPositionNoPivot(RectTransform rect, bool applyWindowScale = true)
	{
		Vector2 windowSize = rect.rect.size;
		if (applyWindowScale)
		{
			windowSize = multiVector2(windowSize, rect.lossyScale);
		}
		return rect.localPosition - (Vector3)multiVector2(windowSize, rect.pivot - new Vector2(0.5f, 0.5f));
	}
	// 获取在父节点中不受轴心影响下的本地坐标
	public static void setPositionNoPivotInParent(RectTransform rect, Vector3 pos, bool applyWindowScale = true)
	{
		if (rect.parent == null)
		{
			setPositionNoPivot(rect, pos, applyWindowScale);
			return;
		}

		var parent = rect.parent as RectTransform;
		if (parent == null)
		{
			return;
		}
		Vector2 parentSize = parent.rect.size;
		if (applyWindowScale)
		{
			parentSize = multiVector2(parentSize, parent.lossyScale);
		}
		rect.localPosition = round(pos - (Vector3)multiVector2(parentSize, parent.pivot - new Vector2(0.5f, 0.5f)));
	}
	// 获取在父节点中不受轴心影响下的本地坐标
	public static Vector3 getPositionNoPivotInParent(RectTransform rect, bool applyWindowScale = true)
	{
		if (rect.parent == null)
		{
			return getPositionNoPivot(rect, applyWindowScale);
		}

		var parent = rect.parent as RectTransform;
		if (parent == null)
		{
			if (rect.parent == null)
			{
				logError("父节点为空,无法计算适配,当前节点:" + rect.name);
			}
			else
			{
				logError("父节点不是RectTransform,无法计算适配,当前节点:" + rect.name + ",父节点:" + rect.parent.name);
			}
			return Vector3.zero;
		}
		Vector2 parentSize = parent.rect.size;
		if (applyWindowScale)
		{
			parentSize = multiVector2(parentSize, parent.lossyScale);
		}
		return rect.localPosition + (Vector3)multiVector2(parentSize, parent.pivot - new Vector2(0.5f, 0.5f));
	}
	public static void cornerToSide(Span<Vector3> corners, Vector3[] sides)
	{
		if (sides.Length != 4)
		{
			return;
		}
		for (int i = 0; i < 4; ++i)
		{
			sides[i] = (corners[i] + corners[(i + 1) % 4]) * 0.5f;
		}
	}
	public static void cornerToSide(Span<Vector3> corners, Span<Vector3> sides)
	{
		if (sides.Length != 4)
		{
			return;
		}
		for (int i = 0; i < 4; ++i)
		{
			sides[i] = (corners[i] + corners[(i + 1) % 4]) * 0.5f;
		}
	}
	public static void setRectSize(RectTransform rectTransform, Vector2 size)
	{
		if(rectTransform == null)
		{
			return;
		}
		Vector2 deltaSize = size - rectTransform.rect.size;
		Vector2 pivot = rectTransform.pivot;
		rectTransform.offsetMin -= new Vector2(deltaSize.x * pivot.x, deltaSize.y * pivot.y);
		rectTransform.offsetMax += new Vector2(deltaSize.x * (1.0f - pivot.x), deltaSize.y * (1.0f - pivot.y));
	}
	public static void setRectSizeWithFontSize(RectTransform rectTransform, Vector2 size, int minFontSize)
	{
		if (rectTransform == null)
		{
			return;
		}
		float lastHeight = rectTransform.rect.height;
		setRectSize(rectTransform, size);
		// 文字控件需要根据高度重新计算字体大小
		if (rectTransform.TryGetComponent(out Text text))
		{
			text.fontSize = clampMin(floor(divide(text.fontSize, lastHeight) * size.y), minFontSize);
		}
	}
	public static void setUGUIChildAlpha(GameObject go, float alpha)
	{
		if (go.TryGetComponent<Graphic>(out var graphic))
		{
			Color color = graphic.color;
			color.a = alpha;
			graphic.color = color;
		}
		Transform transform = go.transform;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			setUGUIChildAlpha(transform.GetChild(i).gameObject, alpha);
		}
	}
}