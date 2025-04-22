#if USE_TMP
using TMPro;
#endif
using UnityEngine;
using UnityEngine.UI;
using static FrameBaseUtility;
using static MathUtility;

// RectTransform和UI相关的工具函数类
public class WidgetUtility
{
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
				logErrorBase("父节点为空,无法计算适配,当前节点:" + rect.name);
			}
			else
			{
				logErrorBase("父节点不是RectTransform,无法计算适配,当前节点:" + rect.name + ",父节点:" + rect.parent.name);
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
		if (lastHeight < 1.0f)
		{
			return;
		}
		// 文字控件需要根据高度重新计算字体大小
		if (rectTransform.TryGetComponent(out Text text))
		{
			text.fontSize = Mathf.Clamp((int)Mathf.Floor(text.fontSize / lastHeight * size.y), minFontSize, 200);
		}
#if USE_TMP
		else if (rectTransform.TryGetComponent(out TextMeshProUGUI tmproText))
		{
			tmproText.fontSize = Mathf.Clamp((int)Mathf.Floor(tmproText.fontSize / lastHeight * size.y), minFontSize, 200);
		}
#endif
	}
}