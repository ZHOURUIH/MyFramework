using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;
using static FrameUtility;

// RectTransform和UI相关的工具函数类
public class WidgetUtility
{
	private static Vector3[] mRootCorner;					// 用于避免GC而保存的变量
	private static Vector3[] mTempCorners = new Vector3[4];	// 用于避免GC而保存的变量
	// 父节点在父节点坐标系下的各条边
	public static void getParentSides(GameObject parent, Vector3[] sides)
	{
		Vector2 size = parent.GetComponent<RectTransform>().rect.size;
		mTempCorners[0] = new Vector3(-size.x * 0.5f, -size.y * 0.5f);
		mTempCorners[1] = new Vector3(-size.x * 0.5f, size.y * 0.5f);
		mTempCorners[2] = new Vector3(size.x * 0.5f, size.y * 0.5f);
		mTempCorners[3] = new Vector3(size.x * 0.5f, -size.y * 0.5f);
		cornerToSide(mTempCorners, sides);
	}
	public static Vector2 getRectSize(RectTransform rect)
	{
		return rect.rect.size;
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
		Vector2 parentSize = parent.rect.size;
		if (applyWindowScale)
		{
			parentSize = multiVector2(parentSize, parent.lossyScale);
		}
		return rect.localPosition + (Vector3)multiVector2(parentSize, parent.pivot - new Vector2(0.5f, 0.5f));
	}
	public static Vector3[] getRootCorner()
	{
		if (mRootCorner == null)
		{
			Vector2 rootSize = getRootSize();
			mRootCorner = new Vector3[4];
			mRootCorner[0] = new Vector3(-rootSize.x * 0.5f, -rootSize.y * 0.5f);
			mRootCorner[1] = new Vector3(-rootSize.x * 0.5f, rootSize.y * 0.5f);
			mRootCorner[2] = new Vector3(rootSize.x * 0.5f, rootSize.y * 0.5f);
			mRootCorner[3] = new Vector3(rootSize.x * 0.5f, -rootSize.y * 0.5f);
		}
		return mRootCorner;
	}
	public static void cornerToSide(Vector3[] corners, Vector3[] sides)
	{
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
		Text text = rectTransform.GetComponent<Text>();
		if (text != null)
		{
			text.fontSize = clampMin(floor(text.fontSize / lastHeight * size.y), minFontSize);
		}
	}
	// 获得指定屏幕坐标下的可交互UI,比如勾选了RaycastTarget的Image或Text等,Button,InputField等
	public static void checkUGUIInteractable(Vector2 screenPosition, List<GameObject> clickList)
	{
		if (clickList == null)
		{
			return;
		}
		using (new ListScope<RaycastResult>(out var results))
		{
			raycastUGUI(screenPosition, results);
			// 如果点击到了非透明的图片或者文字,则不可穿透射线
			int count = results.Count;
			for (int i = 0; i < count; ++i)
			{
				GameObject go = results[i].gameObject;
				var graphic = go.GetComponent<Graphic>();
				if (graphic != null && graphic.raycastTarget)
				{
					clickList.Add(go);
					continue;
				}
				var selectable = go.GetComponent<Selectable>();
				if (selectable != null && selectable.interactable)
				{
					clickList.Add(go);
					continue;
				}
			}
		}
	}
	// 获得指定屏幕坐标下的可见UI,可见UI是指已激活且透明度不为0
	public static GameObject getPointerOnUI(Vector2 screenPosition)
	{
		using (new ListScope<RaycastResult>(out var results))
		{
			raycastUGUI(screenPosition, results);
			// 如果点击到了非透明的图片或者文字,则不可穿透射线
			int count = results.Count;
			for (int i = 0; i < count; ++i)
			{
				GameObject go = results[i].gameObject;
				var image = go.GetComponent<Image>();
				if (image != null)
				{
					if (image.raycastTarget && !isFloatZero(image.color.a))
					{
						return go;
					}
					continue;
				}
				var text = go.GetComponent<Text>();
				if (text != null)
				{
					if (text.raycastTarget && !isFloatZero(text.color.a))
					{
						return go;
					}
					continue;
				}
			}
			return null;
		}
	}
	public static void setUGUIChildAlpha(GameObject go, float alpha)
	{
		var graphic = go.GetComponent<Graphic>();
		if (graphic != null)
		{
			Color color = graphic.color;
			color.a = alpha;
			graphic.color = color;
		}
		Transform transform = go.transform;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			GameObject child = transform.GetChild(i).gameObject;
			setUGUIChildAlpha(child, alpha);
		}
	}
	public static void autoGridFixedRootHeight(myUGUIObject root, Vector2 gridSize, bool autoRefreshUIDepth, CORNER startCorner = CORNER.LEFT_TOP)
	{
		autoGridFixedRootHeight(root, gridSize, Vector2.zero, autoRefreshUIDepth, startCorner);
	}
	// 保持父节点的高度,从指定角开始纵向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	public static void autoGridFixedRootHeight(myUGUIObject root, Vector2 gridSize, Vector2 interval, bool autoRefreshUIDepth, CORNER startCorner = CORNER.LEFT_TOP)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		using (new ListScope<RectTransform>(out var childList))
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				var childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (childRect.gameObject.activeSelf)
				{
					childList.Add(childRect);
				}
			}

			// 计算父节点大小
			Vector2 rootSize = root.getWindowSize();
			Vector3 beforeRealPosition = root.getPositionNoPivot();
			Vector3 beforeRootLeftTop = new Vector3(beforeRealPosition.x - rootSize.x * 0.5f, beforeRealPosition.y + rootSize.y * 0.5f);
			Vector3 beforeRootLeftBottom = new Vector3(beforeRealPosition.x - rootSize.x * 0.5f, beforeRealPosition.y - rootSize.y * 0.5f);
			Vector3 beforeRootRightTop = new Vector3(beforeRealPosition.x + rootSize.x * 0.5f, beforeRealPosition.y + rootSize.y * 0.5f);
			Vector3 beforeRootRightBottom = new Vector3(beforeRealPosition.x + rootSize.x * 0.5f, beforeRealPosition.y - rootSize.y * 0.5f);
			int rowCount = 1;
			if (rootSize.y > gridSize.y)
			{
				rowCount = (int)((rootSize.y - gridSize.y) / (interval.y + gridSize.y)) + 1;
			}
			int activeChildCount = childList.Count;
			// 固定父节点高度时只能纵向排列
			int columnCount = activeChildCount / rowCount + clampMax(activeChildCount % rowCount, 1);
			rootSize.x = columnCount * gridSize.x + (columnCount - 1) * interval.x;
			// 确保宽高都是偶数,这样才能使边和坐标都是整数
			rootSize.x += (int)rootSize.x & 1;
			rootSize.y += (int)rootSize.y & 1;
			root.setWindowSize(rootSize);

			// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标,并且调整父节点的坐标
			Vector2 startPos = Vector2.zero;
			int horizontalSign = 0;
			int verticalSign = 0;
			Vector3 curRealPosition = root.getPositionNoPivot();
			if (startCorner == CORNER.LEFT_TOP)
			{
				startPos = new Vector2(gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowTop());
				horizontalSign = 1;
				verticalSign = -1;
				// 保持左上角的坐标与改变大小之前的左上角坐标一致
				Vector3 curRootLeftTop = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootLeftTop, curRootLeftTop))
				{
					root.setPosition(round(root.getPosition() + beforeRootLeftTop - curRootLeftTop));
				}
			}
			else if (startCorner == CORNER.LEFT_BOTTOM)
			{
				startPos = new Vector2(gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowBottom());
				horizontalSign = 1;
				verticalSign = 1;
				// 保持左下角的坐标与改变大小之前的左下角坐标一致
				Vector3 curRootLeftBottom = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootLeftBottom, curRootLeftBottom))
				{
					root.setPosition(round(root.getPosition() + beforeRootLeftBottom - curRootLeftBottom));
				}
			}
			else if (startCorner == CORNER.RIGHT_TOP)
			{
				startPos = new Vector2(-gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowTop());
				horizontalSign = -1;
				verticalSign = -1;
				// 保持右上角的坐标与改变大小之前的右上角坐标一致
				Vector3 curRootRightTop = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootRightTop, curRootRightTop))
				{
					root.setPosition(round(root.getPosition() + beforeRootRightTop - curRootRightTop));
				}
			}
			else if (startCorner == CORNER.RIGHT_BOTTOM)
			{
				startPos = new Vector2(-gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowBottom());
				horizontalSign = -1;
				verticalSign = 1;
				// 保持右下角的坐标与改变大小之前的右下角坐标一致
				Vector3 curRootRightBottom = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootRightBottom, curRootRightBottom))
				{
					root.setPosition(round(root.getPosition() + beforeRootRightBottom - curRootRightBottom));
				}
			}

			// 计算子节点坐标,始终让子节点位于父节点的矩形范围内
			// 并且会考虑父节点的pivot,但是不考虑子节点的pivot,所以如果子节点的pivot不在中心,可能会计算错误
			for (int i = 0; i < activeChildCount; ++i)
			{
				RectTransform child = childList[i];
				int indexX = i / rowCount;
				int indexY = i % rowCount;
				Vector2 pos = new Vector2((indexX * gridSize.x + indexX * interval.x) * horizontalSign,
										  (indexY * gridSize.y + indexY * interval.y) * verticalSign);
				child.localPosition = round(startPos + pos);
				setRectSize(child, gridSize);
			}
		}

		if (autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, true);
		}
	}
	public static void autoGridFixedRootWidth(myUGUIObject root, Vector2 gridSize, bool autoRefreshUIDepth, CORNER startCorner = CORNER.LEFT_TOP)
	{
		autoGridFixedRootWidth(root, gridSize, Vector2.zero, autoRefreshUIDepth, startCorner);
	}
	// 保持父节点的宽度,从指定角开始横向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	public static void autoGridFixedRootWidth(myUGUIObject root, Vector2 gridSize, Vector2 interval, bool autoRefreshUIDepth, CORNER startCorner = CORNER.LEFT_TOP)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		using (new ListScope<RectTransform>(out var childList))
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				var childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (childRect.gameObject.activeSelf)
				{
					childList.Add(childRect);
				}
			}

			// 计算父节点大小
			Vector2 rootSize = root.getWindowSize();
			Vector3 beforeRealPos = root.getPositionNoPivot();
			Vector3 beforeRootLeftTop = new Vector3(beforeRealPos.x - rootSize.x * 0.5f, beforeRealPos.y + rootSize.y * 0.5f);
			Vector3 beforeRootLeftBottom = new Vector3(beforeRealPos.x - rootSize.x * 0.5f, beforeRealPos.y - rootSize.y * 0.5f);
			Vector3 beforeRootRightTop = new Vector3(beforeRealPos.x + rootSize.x * 0.5f, beforeRealPos.y + rootSize.y * 0.5f);
			Vector3 beforeRootRightBottom = new Vector3(beforeRealPos.x + rootSize.x * 0.5f, beforeRealPos.y - rootSize.y * 0.5f);
			int columnCount = 1;
			if (rootSize.x > gridSize.x)
			{
				columnCount = (int)((rootSize.x - gridSize.x) / (interval.x + gridSize.x)) + 1;
			}
			int activeChildCount = childList.Count;
			int rowCount = generateBatchCount(activeChildCount, columnCount);
			rootSize.y = rowCount * gridSize.y + (rowCount - 1) * interval.y;
			// 确保宽高都是偶数,这样才能使边和坐标都是整数
			rootSize.x += (int)rootSize.x & 1;
			rootSize.y += (int)rootSize.y & 1;
			root.setWindowSize(rootSize);

			// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标,并且调整父节点的坐标
			Vector2 startPos = Vector2.zero;
			int horizontalSign = 0;
			int verticalSign = 0;
			Vector3 curRealPosition = root.getPositionNoPivot();
			if (startCorner == CORNER.LEFT_TOP)
			{
				startPos = new Vector2(gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowTop());
				horizontalSign = 1;
				verticalSign = -1;
				// 保持左上角的坐标与改变大小之前的左上角坐标一致
				Vector3 curRootLeftTop = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootLeftTop, curRootLeftTop))
				{
					root.setPosition(round(root.getPosition() + beforeRootLeftTop - curRootLeftTop));
				}
			}
			else if (startCorner == CORNER.LEFT_BOTTOM)
			{
				startPos = new Vector2(gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowLeft(), root.getWindowBottom());
				horizontalSign = 1;
				verticalSign = 1;
				// 保持左下角的坐标与改变大小之前的左下角坐标一致
				Vector3 curRootLeftBottom = new Vector3(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootLeftBottom, curRootLeftBottom))
				{
					root.setPosition(round(root.getPosition() + beforeRootLeftBottom - curRootLeftBottom));
				}
			}
			else if (startCorner == CORNER.RIGHT_TOP)
			{
				startPos = new Vector2(-gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowTop());
				horizontalSign = -1;
				verticalSign = -1;
				// 保持右上角的坐标与改变大小之前的右上角坐标一致
				Vector3 curRootRightTop = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootRightTop, curRootRightTop))
				{
					root.setPosition(round(root.getPosition() + beforeRootRightTop - curRootRightTop));
				}
			}
			else if (startCorner == CORNER.RIGHT_BOTTOM)
			{
				startPos = new Vector2(-gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getWindowRight(), root.getWindowBottom());
				horizontalSign = -1;
				verticalSign = 1;
				// 保持右下角的坐标与改变大小之前的右下角坐标一致
				Vector3 curRootRightBottom = new Vector3(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
				if (!isVectorEqual(beforeRootRightBottom, curRootRightBottom))
				{
					root.setPosition(round(root.getPosition() + beforeRootRightBottom - curRootRightBottom));
				}
			}

			// 计算子节点坐标,始终让子节点位于父节点的矩形范围内
			// 并且会考虑父节点的pivot,但是不考虑子节点的pivot,所以如果子节点的pivot不在中心,可能会计算错误
			for (int i = 0; i < activeChildCount; ++i)
			{
				RectTransform child = childList[i];
				int indexX = i % columnCount;
				int indexY = i / columnCount;
				Vector2 pos = new Vector2((indexX * gridSize.x + indexX * interval.x) * horizontalSign,
										  (indexY * gridSize.y + indexY * interval.y) * verticalSign);
				child.localPosition = round(startPos + pos);
				setRectSize(child, gridSize);
			}
		}

		if (autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, true);
		}
	}
	// 保持父节点的大小和位置,从左上角开始横向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,horizontal是不足1排时,水平方向的停靠方式,vertical是整体竖直方向上的停靠方式
	public static void autoGrid(myUGUIObject root, Vector2 gridSize, Vector2 interval, bool autoRefreshUIDepth, HORIZONTAL_DIRECTION horizontal = HORIZONTAL_DIRECTION.CENTER, VERTICAL_DIRECTION vertical = VERTICAL_DIRECTION.CENTER)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		using (new ListScope<RectTransform>(out var childList))
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				var childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (childRect.gameObject.activeSelf)
				{
					childList.Add(childRect);
				}
			}

			// 计算父节点大小
			Vector2 rootSize = root.getWindowSize();
			int maxColumnCount = 1;
			if (rootSize.x > gridSize.x)
			{
				maxColumnCount = (int)((rootSize.x - gridSize.x) / (interval.x + gridSize.x)) + 1;
			}
			int activeChildCount = childList.Count;
			int rowCount = generateBatchCount(activeChildCount, maxColumnCount);
			int curCountOneLine = getMin(maxColumnCount, activeChildCount);
			Vector2 contentSize = new Vector2(gridSize.x * curCountOneLine + interval.x * (curCountOneLine - 1), gridSize.y * rowCount + interval.y * (rowCount - 1));
			// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标
			Vector2 startPos = Vector2.zero;
			// 不足1排
			if (activeChildCount < maxColumnCount && horizontal != HORIZONTAL_DIRECTION.NONE)
			{
				if (horizontal == HORIZONTAL_DIRECTION.LEFT)
				{
					startPos.x = root.getWindowLeft();
				}
				else if (horizontal == HORIZONTAL_DIRECTION.CENTER)
				{
					startPos.x = root.getWindowLeft() + rootSize.x * 0.5f - contentSize.x * 0.5f;
				}
				else if (horizontal == HORIZONTAL_DIRECTION.RIGHT)
				{
					startPos.x = root.getWindowLeft() + rootSize.x - contentSize.x;
				}
				startPos.x += gridSize.x * 0.5f;
			}
			if (vertical != VERTICAL_DIRECTION.NONE)
			{
				if (vertical == VERTICAL_DIRECTION.TOP)
				{
					startPos.y = root.getWindowTop();
				}
				else if (vertical == VERTICAL_DIRECTION.CENTER)
				{
					startPos.y = root.getWindowTop() - rootSize.y * 0.5f + contentSize.y * 0.5f;
				}
				else if (vertical == VERTICAL_DIRECTION.BOTTOM)
				{
					startPos.y += root.getWindowTop() - rootSize.y + contentSize.y;
				}
				startPos.y += -gridSize.y * 0.5f;
			}

			// 计算子节点坐标,始终让子节点位于父节点的矩形范围内
			// 并且会考虑父节点的pivot,但是不考虑子节点的pivot,所以如果子节点的pivot不在中心,可能会计算错误
			for (int i = 0; i < activeChildCount; ++i)
			{
				RectTransform child = childList[i];
				int indexX = i % maxColumnCount;
				int indexY = i / maxColumnCount;
				Vector2 pos = new Vector2(indexX * gridSize.x + indexX * interval.x,
										-(indexY * gridSize.y + indexY * interval.y));
				child.localPosition = round(startPos + pos);
				setRectSize(child, gridSize);
			}
		}

		if (autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, true);
		}
	}
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列,并且不改变子节点的大小
	public static void autoGridVertical(myUGUIObject root, bool autoRefreshUIDepth, float interval = 0.0f, float minHeight = 0.0f)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		using (new ListScope<RectTransform>(out var childList))
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				var childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (childRect.gameObject.activeSelf)
				{
					childList.Add(childRect);
				}
			}

			int beforeRootTopY = (int)(root.getPositionNoPivot().y + root.getWindowSize().y * 0.5f);
			// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
			Vector2 rootSize = root.getWindowSize();
			float height = 0.0f;
			for (int i = 0; i < childList.Count; ++i)
			{
				height += childList[i].rect.height;
				// 最后一个子节点后不再添加间隔
				if (i != childList.Count - 1)
				{
					height += interval;
				}
			}
			rootSize.y = clampMin(height, minHeight);
			// 确保高始终为偶数,这样才能使上边界和窗口位置都是整数
			rootSize.y += (int)rootSize.y & 1;
			root.setWindowSize(rootSize);

			// 改变完父节点的大小后需要保持父节点上边界的y坐标不变
			int curRootTopY = (int)(root.getPositionNoPivot().y + root.getWindowSize().y * 0.5f);
			if (curRootTopY != beforeRootTopY)
			{
				Vector3 rootPos = root.getPosition();
				root.setPosition(round(new Vector3(rootPos.x, rootPos.y + beforeRootTopY - curRootTopY, rootPos.z)));
			}

			// 计算子节点坐标
			float currentTop = root.getWindowTop();
			for (int i = 0; i < childList.Count; ++i)
			{
				RectTransform childRect = childList[i];
				float curHeight = childRect.rect.height;
				childRect.localPosition = round(new Vector3(childRect.localPosition.x, currentTop - curHeight * 0.5f));
				currentTop -= curHeight;
				// 最后一个子节点后不再添加间隔
				if (i != childList.Count - 1)
				{
					currentTop -= interval;
				}
			}
		}

		if (autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, true);
		}
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列,并且不改变子节点的大小
	public static void autoGridHorizontal(myUGUIObject root, bool autoRefreshUIDepth, float interval = 0.0f, float minWidth = 0.0f)
	{
		RectTransform transform = root.getRectTransform();
		// 先找出所有激活的子节点
		using (new ListScope<RectTransform>(out var childList))
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				var childRect = transform.GetChild(i).GetComponent<RectTransform>();
				if (childRect.gameObject.activeSelf)
				{
					childList.Add(childRect);
				}
			}

			int beforeRootLeftX = (int)(root.getPositionNoPivot().x - root.getWindowSize().x * 0.5f);
			// 如果要同时修改root的窗口大小为排列以后的内容大小，则需要提前获取内容排列后的宽高
			Vector2 rootSize = root.getWindowSize();
			float width = 0.0f;
			for (int i = 0; i < childList.Count; ++i)
			{
				width += childList[i].rect.width;
				// 最后一个子节点后不再添加间隔
				if (i != childList.Count - 1)
				{
					width += interval;
				}
			}
			rootSize.x = clampMin(width, minWidth);
			// 确保宽是偶数,这样才能使边和坐标都是整数
			rootSize.x += (int)rootSize.x & 1;
			root.setWindowSize(rootSize);

			// 改变完父节点的大小后需要保持父节点左边界的x坐标不变
			int curRootLeftX = (int)(root.getPositionNoPivot().x - root.getWindowSize().x * 0.5f);
			if (curRootLeftX != beforeRootLeftX)
			{
				Vector3 rootPos = root.getPosition();
				root.setPosition(round(new Vector3(rootPos.x + beforeRootLeftX - curRootLeftX, rootPos.y, rootPos.z)));
			}

			// 计算子节点坐标
			float currentLeft = root.getWindowLeft();
			for (int i = 0; i < childList.Count; ++i)
			{
				RectTransform childRect = childList[i];
				float curWidth = childRect.rect.width;
				childRect.localPosition = round(new Vector3(currentLeft + curWidth * 0.5f, childRect.localPosition.y));
				currentLeft += curWidth;
				// 最后一个子节点后不再添加间隔
				if (i != childList.Count - 1)
				{
					currentLeft += interval;
				}
			}
		}

		if (autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, true);
		}
	}
}