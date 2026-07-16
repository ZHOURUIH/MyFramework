using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

public static class RectTransformExtension
{
	public static void setPositionNoPivot(this RectTransform rect, Vector3 pos, bool applyWindowScale = true)
	{
		Vector2 windowSize = rect.rect.size;
		if (applyWindowScale)
		{
			windowSize = multiVector2(windowSize, rect.lossyScale);
		}
		rect.localPosition = round(pos + (Vector3)multiVector2(windowSize, rect.pivot - new Vector2(0.5f, 0.5f)));
	}
	// 将当前窗口的顶部对齐父节点的顶部,只改Y坐标
	public static void setTopToParentTop(this RectTransform rect)
	{
		setWindowTopInParent(rect, getWindowTopInSelf(rect));
	}
	// 将当前窗口的顶部中心对齐父节点的顶部中心,X和Y坐标都改
	public static void setTopCenterToParentTopCenter(this RectTransform rect)
	{
		setWindowTopInParent(rect, getWindowTopInSelf(rect));
		setWindowInParentCenterX(rect);
	}
	// 将当前窗口的底部对齐父节点的底部,只改变Y坐标
	public static void setBottomToParentBottom(this RectTransform rect)
	{
		setWindowBottomInParent(rect, getWindowBottomInSelf(rect));
	}
	// 将当前窗口的底部中心对齐父节点的底部中心,X和Y坐标都改
	public static void setBottomCenterToParentBottomCenter(this RectTransform rect)
	{
		setWindowBottomInParent(rect, getWindowBottomInSelf(rect));
		setWindowInParentCenterX(rect);
	}
	// 将当前窗口的左边界对齐父节点的左边界.只改X坐标
	public static void setLeftToParentLeft(this RectTransform rect)
	{
		setWindowLeftInParent(rect, getWindowLeftInSelf(rect));
	}
	// 将当前窗口的左边界中心对齐父节点的左边界中心,X和Y坐标都改
	public static void setLeftCenterToParentLeftCenter(this RectTransform rect)
	{
		setWindowLeftInParent(rect, getWindowLeftInSelf(rect));
		setWindowInParentCenterY(rect);
	}
	// 将当前窗口的右边界对齐父节点的右边界,只改X坐标
	public static void setRightToParentRight(this RectTransform rect)
	{
		setWindowRightInParent(rect, getWindowRightInSelf(rect));
	}
	// 将当前窗口的右边界中心对齐父节点的右边界中心,X和Y坐标都改
	public static void setRightCenterToParentRightCenter(this RectTransform rect)
	{
		setWindowRightInParent(rect, getWindowRightInSelf(rect));
		setWindowInParentCenterY(rect);
	}
	public static void setPositionX(this RectTransform rect, float x) { rect.localPosition = replaceX(rect.localPosition, x); }
	public static void setPositionY(this RectTransform rect, float y) { rect.localPosition = replaceY(rect.localPosition, y); }
	public static void setPositionZ(this RectTransform rect, float z) { rect.localPosition = replaceZ(rect.localPosition, z); }
	// 设置窗口在父节点中横向居中
	public static void setWindowInParentCenterX(this RectTransform rect) { setPositionX(rect, 0.0f); }
	// 设置窗口在父节点中纵向居中
	public static void setWindowInParentCenterY(this RectTransform rect) { setPositionY(rect, 0.0f); }
	// 设置窗口左边界在父节点中的X坐标
	public static void setWindowLeftInParent(this RectTransform rect, float leftInParent) { setPositionX(rect, leftInParent - getWindowLeftInSelf(rect)); }
	// 设置窗口右边界在父节点中的X坐标
	public static void setWindowRightInParent(this RectTransform rect, float rightInParent) { setPositionX(rect, rightInParent - getWindowRightInSelf(rect)); }
	// 设置窗口顶部在父节点中的Y坐标
	public static void setWindowTopInParent(this RectTransform rect, float topInParent) { setPositionY(rect, topInParent - getWindowTopInSelf(rect)); }
	// 设置窗口底部在父节点中的Y坐标
	public static void setWindowBottomInParent(this RectTransform rect, float bottomInParent) { setPositionY(rect, bottomInParent - getWindowBottomInSelf(rect)); }
	// 获得窗口左边界在父窗口中的X坐标
	public static float getWindowLeftInParent(this RectTransform rect) { return rect.localPosition.x + getWindowLeftInSelf(rect); }
	// 获得窗口右边界在父窗口中的X坐标
	public static float getWindowRightInParent(this RectTransform rect) { return rect.localPosition.x + getWindowRightInSelf(rect); }
	// 获得窗口顶部在父窗口中的Y坐标
	public static float getWindowTopInParent(this RectTransform rect) { return rect.localPosition.y + getWindowTopInSelf(rect); }
	// 获得窗口底部在父窗口中的Y坐标
	public static float getWindowBottomInParent(this RectTransform rect) { return rect.localPosition.y + getWindowBottomInSelf(rect); }
	// 获得窗口顶部在窗口中的相对于窗口pivot的Y坐标
	public static float getWindowTopInSelf(this RectTransform rect) { return rect.rect.size.y * (1.0f - rect.pivot.y); }
	// 获得窗口底部在窗口中的相对于窗口pivot的Y坐标
	public static float getWindowBottomInSelf(this RectTransform rect) { return -rect.rect.size.y * rect.pivot.y; }
	// 获得窗口左边界在窗口中的相对于窗口pivot的X坐标
	public static float getWindowLeftInSelf(this RectTransform rect) { return -rect.rect.size.x * rect.pivot.x; }
	// 获得窗口右边界在窗口中的相对于窗口pivot的X坐标
	public static float getWindowRightInSelf(this RectTransform rect) { return rect.rect.size.x * (1.0f - rect.pivot.x); }
	// 使当前窗口右边界对齐另外一个窗口的左边界,只修改x轴,仅限同一父节点下
	public static void setRightToOtherLeft(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionX(rect, other.localPosition.x - other.rect.size.x * 0.5f - rect.rect.size.x * 0.5f - interval);
	}
	// 使当前窗口左边界对齐另外一个窗口的右边界,只修改x轴,仅限同一父节点下
	public static void setLeftToOtherRight(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionX(rect, other.localPosition.x + other.rect.size.x * 0.5f + rect.rect.size.x * 0.5f + interval);
	}
	// 使当前窗口下边界对齐另外一个窗口的上边界,只修改y轴,仅限同一父节点下
	public static void setBottomToOtherTop(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionY(rect, other.localPosition.y + other.rect.size.y * 0.5f + rect.rect.size.y * 0.5f + interval);
	}
	// 使当前窗口上边界对齐另外一个窗口的下边界,只修改y轴,仅限同一父节点下
	public static void setTopToOtherBottom(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionY(rect, other.localPosition.y - other.rect.size.y * 0.5f - rect.rect.size.y * 0.5f - interval);
	}
	// 使当前窗口左边界对齐另外一个窗口的左边界,只修改x轴,仅限同一父节点下
	public static void setLeftToOtherLeft(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionX(rect, other.localPosition.x - other.rect.size.x * 0.5f + rect.rect.size.x * 0.5f + interval);
	}
	// 使当前窗口左边界对齐另外一个窗口的右边界,只修改x轴,仅限同一父节点下
	public static void setRightToOtherRight(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionX(rect, other.localPosition.x + other.rect.size.x * 0.5f - rect.rect.size.x * 0.5f - interval);
	}
	// 使当前窗口下边界对齐另外一个窗口的上边界,只修改y轴,仅限同一父节点下
	public static void setTopToOtherTop(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionY(rect, other.localPosition.y + other.rect.size.y * 0.5f - rect.rect.size.y * 0.5f - interval);
	}
	// 使当前窗口上边界对齐另外一个窗口的下边界,只修改y轴,仅限同一父节点下
	public static void setBottomToOtherBottom(this RectTransform rect, RectTransform other, float intervalNoScreenScale = 0.0f)
	{
		if (rect.parent != other.parent)
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		setPositionY(rect, other.localPosition.y - other.rect.size.y * 0.5f + rect.rect.size.y * 0.5f + interval);
	}
	public static Vector3 getPositionNoPivot(this RectTransform rect, bool applyWindowScale = true)
	{
		Vector2 windowSize = rect.rect.size;
		if (applyWindowScale)
		{
			windowSize = multiVector2(windowSize, rect.lossyScale);
		}
		return rect.localPosition - (Vector3)multiVector2(windowSize, rect.pivot - new Vector2(0.5f, 0.5f));
	}
	// 获取在父节点中不受轴心影响下的本地坐标
	public static void setPositionNoPivotInParent(this RectTransform rect, Vector3 pos, bool applyWindowScale = true)
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
	public static Vector3 getPositionNoPivotInParent(this RectTransform rect, bool applyWindowScale = true)
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
				logError("父节点为空,无法计算适配,path:" + getGameObjectPath(rect.gameObject));
			}
			else
			{
				logError("父节点不是RectTransform,无法计算适配,path:" + getGameObjectPath(rect.gameObject));
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
	public static void setRectWidth(this RectTransform rectTransform, float width)
	{
		Vector2 vector = rectTransform.sizeDelta;
		vector.x = width - getParentSize(rectTransform).x * (rectTransform.anchorMax.x - rectTransform.anchorMin.x);
		rectTransform.sizeDelta = vector;
	}
	public static void setRectHeight(this RectTransform rectTransform, float height)
	{
		Vector2 vector = rectTransform.sizeDelta;
		vector.y = height - getParentSize(rectTransform).y * (rectTransform.anchorMax.y - rectTransform.anchorMin.y);
		rectTransform.sizeDelta = vector;
	}
	public static void setRectSize(this RectTransform rectTransform, Vector2 size)
	{
		if (rectTransform == null)
		{
			return;
		}
		//Vector2 deltaSize = size - rectTransform.rect.size;
		//Vector2 pivot = rectTransform.pivot;
		//rectTransform.offsetMin -= new Vector2(deltaSize.x * pivot.x, deltaSize.y * pivot.y);
		//rectTransform.offsetMax += new Vector2(deltaSize.x * (1.0f - pivot.x), deltaSize.y * (1.0f - pivot.y));
		// 跟上面是等效的
		Vector2 parentSize = getParentSize(rectTransform);
		Vector2 anchorMin = rectTransform.anchorMin;
		Vector2 anchorMax = rectTransform.anchorMax;
		Vector2 vector = rectTransform.sizeDelta;
		vector.x = size.x - parentSize.x * (anchorMax.x - anchorMin.x);
		vector.y = size.y - parentSize.y * (anchorMax.y - anchorMin.y);
		rectTransform.sizeDelta = vector;
	}
	public static Vector2 getParentSize(this RectTransform rectTrans)
	{
		var rectTransform = rectTrans.parent as RectTransform;
		return rectTransform != null ? rectTransform.rect.size : Vector2.zero;
	}
	public static void setRectSizeWithFontSize(this RectTransform rectTransform, Vector2 size, int minFontSize)
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
		else if (rectTransform.TryGetComponent(out TextMeshProUGUI tmproText))
		{
			tmproText.fontSize = clampMin(floor(divide(tmproText.fontSize, lastHeight) * size.y), minFontSize);
		}
	}
	// 保持父节点的大小和位置,从左上角开始横向排列子节点,并且会改变子节点的大小
	// 也会改变root的高度,keepTop表示改变高度以后是否仍然保持root的顶部位置不变
	// gridSize是子节点的大小
	// horizontal是水平方向的停靠方式
	public static void autoGrid(this RectTransform root, Vector2 gridSize, Vector2 intervalNoScreenScale, bool keepTopSide, HORIZONTAL_DIRECTION horizontal = HORIZONTAL_DIRECTION.LEFT)
	{
		if (root == null)
		{
			return;
		}
		Vector2 interval = new();
		interval.x = adjustByScreenScaleAuto(intervalNoScreenScale.x);
		interval.y = adjustByScreenScaleAuto(intervalNoScreenScale.y);
		// 先找出所有激活的子节点
		using var a = new ListScope<RectTransform>(out var childList);
		int childCount = root.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = root.GetChild(i) as RectTransform;
			childList.addIf(childRect, childRect != null && childRect.gameObject.activeSelf);
		}

		// 计算父节点大小
		Vector2 rootSize = root.rect.size;
		int maxColumnCount = 1;
		if (rootSize.x > gridSize.x)
		{
			maxColumnCount = (int)divide(rootSize.x - gridSize.x, interval.x + gridSize.x) + 1;
		}
		int activeChildCount = childList.Count;
		int rowCount = generateBatchCount(activeChildCount, maxColumnCount);
		int curCountOneLine = getMin(maxColumnCount, activeChildCount);
		Vector2 contentSize = new(gridSize.x * curCountOneLine + interval.x * clampMin(curCountOneLine - 1),
									gridSize.y * rowCount + interval.y * clampMin(rowCount - 1));
		// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标
		Vector2 startPos = Vector2.zero;
		if (horizontal == HORIZONTAL_DIRECTION.LEFT)
		{
			startPos.x = -rootSize.x * root.pivot.x;
		}
		else if (horizontal == HORIZONTAL_DIRECTION.CENTER)
		{
			startPos.x = -rootSize.x * root.pivot.x + rootSize.x * 0.5f - contentSize.x * 0.5f;
		}
		else if (horizontal == HORIZONTAL_DIRECTION.RIGHT)
		{
			startPos.x = -rootSize.x * root.pivot.x + rootSize.x - contentSize.x;
		}
		startPos.x += gridSize.x * 0.5f;
		// 固定从上往下排列
		startPos.y = contentSize.y * (1.0f - root.pivot.y);
		startPos.y += -gridSize.y * 0.5f;
		setRectHeight(root, contentSize.y);
		if (keepTopSide)
		{
			setPositionY(root, round(root.localPosition.y + (rootSize.y - contentSize.y) * 0.5f));
		}

		// 计算子节点坐标,始终让子节点位于父节点的矩形范围内
		// 并且会考虑父节点的pivot,但是不考虑子节点的pivot,所以如果子节点的pivot不在中心,可能会计算错误
		for (int i = 0; i < activeChildCount; ++i)
		{
			RectTransform child = childList[i];
			int indexX = i % maxColumnCount;
			int indexY = divideInt(i, maxColumnCount);
			Vector2 pos = new(indexX * gridSize.x + indexX * interval.x,
							-(indexY * gridSize.y + indexY * interval.y));
			child.localPosition = round(startPos + pos);
			setRectSize(child, gridSize);
		}
	}
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列,并且不改变子节点的大小,会改变root的大小
	// intervalNoScreenScale会自动根据屏幕缩放来计算实际的值
	// fromTopToBottom为true表示从上往下排列,false表示从下往上排列
	public static void autoGridVertical(this RectTransform root, float intervalNoScreenScale, float minHeight = 0.0f, float extraTopHeight = 0.0f, float extraBottomHeight = 0.0f, bool keepTopSide = true, bool fromTopToBottom = true)
	{
		if (root == null)
		{
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		// 先找出所有激活的子节点
		using var a = new ListScope<RectTransform>(out var childList);
		int childCount = root.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = root.GetChild(i) as RectTransform;
			childList.addIf(childRect, childRect != null && childRect.gameObject.activeSelf);
		}

		// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
		float height = 0.0f;
		int validChildCount = 0;
		foreach (RectTransform child in childList)
		{
			height += child.rect.height;
			if (child.rect.height > 0.0f)
			{
				++validChildCount;
			}
		}
		height += interval * clampMin(validChildCount - 1) + extraTopHeight + extraBottomHeight;
		int rootHeight = (int)clampMin(height, minHeight);
		// 确保高始终为偶数,这样才能使上边界和窗口位置都是整数
		rootHeight += rootHeight & 1;
		int beforeRootHeight = (int)root.rect.size.y;
		setRectHeight(root, rootHeight);

		// 改变完父节点的大小后需要保持父节点上边界的y坐标不变
		if (rootHeight != beforeRootHeight)
		{
			if (keepTopSide)
			{
				if (fromTopToBottom)
				{
					setPositionY(root, round(root.localPosition.y + (beforeRootHeight - rootHeight) * 0.5f));
				}
				else
				{
					setPositionY(root, round(root.localPosition.y - (beforeRootHeight - rootHeight) * 0.5f));
				}
			}
			else
			{
				setPositionY(root, round(root.localPosition.y + (rootHeight - beforeRootHeight) * 0.5f));
			}
		}

		// 计算子节点坐标
		if (fromTopToBottom)
		{
			float currentTop = getWindowTopInSelf(root) - extraTopHeight;
			for (int i = 0; i < childList.Count; ++i)
			{
				RectTransform childRect = childList[i];
				float curHeight = childRect.rect.height;
				setPositionY(childRect, round(currentTop - curHeight * 0.5f));
				currentTop -= curHeight;
				// 最后一个子节点后不再添加间隔
				if (i != childList.Count - 1 && curHeight > 0.0f)
				{
					currentTop -= interval;
				}
			}
		}
		else
		{
			float currentBottom = getWindowBottomInSelf(root) + extraBottomHeight;
			for (int i = 0; i < childList.Count; ++i)
			{
				RectTransform childRect = childList[i];
				float curHeight = childRect.rect.height;
				setPositionY(childRect, round(currentBottom + curHeight * 0.5f));
				currentBottom += curHeight;
				// 最后一个子节点后不再添加间隔
				if (i != childList.Count - 1 && curHeight > 0.0f)
				{
					currentBottom += interval;
				}
			}
		}
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列,并且不改变子节点的大小,keepLeftSide为true表示改变大小后保持父节点的左边界位置不变,false表示保持右边界位置不变
	public static void autoGridHorizontal(this RectTransform root, float intervalNoScreenScale, bool changeRootPosSize = true, float minWidth = 0.0f, float extraLeftWidth = 0.0f, float extraRightWidth = 0.0f, bool keepLeftSide = true)
	{
		if (root == null)
		{
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		// 先找出所有激活的子节点
		using var a = new ListScope<RectTransform>(out var childList);
		int childCount = root.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = root.GetChild(i) as RectTransform;
			childList.addIf(childRect, childRect != null && childRect.gameObject.activeSelf);
		}

		if (changeRootPosSize)
		{
			// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
			float width = 0.0f;
			int validChildCount = 0;
			foreach (RectTransform child in childList)
			{
				width += child.rect.width;
				if (child.rect.width > 0.0f)
				{
					++validChildCount;
				}
			}
			width += interval * clampMin(validChildCount - 1) + extraLeftWidth + extraRightWidth;
			int rootWidth = (int)clampMin(width, minWidth);
			// 确保宽是偶数,这样才能使边和坐标都是整数
			rootWidth += rootWidth & 1;
			int beforeRootWidth = (int)root.rect.size.x;
			setRectWidth(root, rootWidth);

			// 改变完父节点的大小后需要保持父节点左边界的x坐标不变
			if (rootWidth != beforeRootWidth)
			{
				if (keepLeftSide)
				{
					setPositionX(root, round(root.localPosition.x + (rootWidth - beforeRootWidth) * 0.5f));
				}
				else
				{
					setPositionX(root, round(root.localPosition.x + (beforeRootWidth - rootWidth) * 0.5f));
				}
			}
		}

		// 计算子节点坐标
		float currentLeft = getWindowLeftInSelf(root) + extraLeftWidth;
		for (int i = 0; i < childList.Count; ++i)
		{
			RectTransform childRect = childList[i];
			float curWidth = childRect.rect.width;
			setPositionX(childRect, currentLeft + curWidth * 0.5f);
			currentLeft += curWidth;
			// 最后一个子节点后不再添加间隔
			if (i != childList.Count - 1 && curWidth > 0.0f)
			{
				currentLeft += interval;
			}
		}
	}
	// 自动排列一个节点下的所有子节点的位置,使所有子节点居中排列
	public static void autoGridHorizontalCenter(this RectTransform root, float intervalNoScreenScale)
	{
		if (root == null)
		{
			return;
		}
		float interval = adjustByScreenScaleAuto(intervalNoScreenScale);
		// 先找出所有激活的子节点
		using var a = new ListScope<RectTransform>(out var childList);
		int childCount = root.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = root.GetChild(i) as RectTransform;
			childList.addIf(childRect, childRect != null && childRect.gameObject.activeSelf);
		}

		float width = 0.0f;
		int validChildCount = 0;
		foreach (RectTransform child in childList)
		{
			width += child.rect.width;
			if (child.rect.width > 0.0f)
			{
				++validChildCount;
			}
		}
		width += interval * clampMin(validChildCount - 1);
		int totalWidth = (int)width;
		// 确保宽是偶数,这样才能使边和坐标都是整数
		totalWidth += totalWidth & 1;

		// 计算子节点坐标
		float currentLeft = -totalWidth * 0.5f;
		for (int i = 0; i < childList.Count; ++i)
		{
			RectTransform childRect = childList[i];
			float curWidth = childRect.rect.width;
			setPositionX(childRect, currentLeft + curWidth * 0.5f);
			currentLeft += curWidth;
			// 最后一个子节点后不再添加间隔
			if (i != childList.Count - 1 && curWidth > 0.0f)
			{
				currentLeft += interval;
			}
		}
	}
	// 跳转transform的范围,使其包含所有子节点,并且保持子节点世界坐标不变
	public static void adjustRectTransformToContainsAllChildRect(this RectTransform transform)
	{
		// 获得父节点的四个边界
		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;
		Dictionary<RectTransform, Vector3> childWorldPositionList = new();
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var child = transform.GetChild(i) as RectTransform;
			if (child == null)
			{
				Debug.LogError("子节点必须有RectTransform组件");
				return;
			}
			Vector2 size = child.rect.size;
			Vector3 pos = child.position;
			left = getMin(pos.x - size.x * 0.5f, left);
			right = getMax(pos.x + size.x * 0.5f, right);
			top = getMax(pos.y + size.y * 0.5f, top);
			bottom = getMin(pos.y - size.y * 0.5f, bottom);
			childWorldPositionList.Add(child, pos);
		}

		// 设置父节点新的位置和大小,重新设置所有子节点的世界坐标
		transform.position = new(ceil((right + left) * 0.5f), ceil((top + bottom) * 0.5f));
		setRectSize(transform, new(ceil(right - left), ceil(top - bottom)));
		foreach (var item in childWorldPositionList)
		{
			item.Key.position = item.Value;
		}
	}
}