using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// RectTransform和UI相关的工具函数类,下面所有的intervalNoScreenScale都会再次经过当前屏幕缩放计算出最终的值
public static class WidgetUtility
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
	public static bool isWindowInScreen(myUGUIObject window, GameCamera camera)
	{
		Vector3 pos = worldToScreen(window.getWorldPosition(), camera.getCamera());
		return overlapBox2(pos, window.getSize(), Vector3.zero, getScreenSize());
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
	// 限制一个窗口的位置,使其父节点不能超出此窗口的范围,通常用于viewport内的更大的子窗口拖拽时限制位置
	public static void clampNoOverParentRectInverse(myUGUIObject window, myUGUIObject parent)
	{
		Vector2 mapSize = window.getSize();
		Vector3 pos = window.getPosition();
		clamp(ref pos.x, parent.getRightInSelf() - mapSize.x * 0.5f, parent.getLeftInSelf() + mapSize.x * 0.5f);
		clamp(ref pos.y, parent.getTopInSelf() - mapSize.y * 0.5f, parent.getBottomInSelf() + mapSize.y * 0.5f);
		window.setPosition(pos);
	}
	// 获得指定屏幕坐标下的可交互UI,比如勾选了RaycastTarget的Image或Text等,Button,InputField等
	public static void checkUGUIInteractable(Vector2 screenPosition, List<GameObject> clickList)
	{
		if (clickList == null)
		{
			return;
		}
		using var a = new ListScope<RaycastResult>(out var results);
		raycastUGUI(screenPosition, results);
		// 如果点击到了非透明的图片或者文字,则不可穿透射线
		foreach (RaycastResult result in results)
		{
			GameObject go = result.gameObject;
			if (go.TryGetComponent<Graphic>(out var graphic) && graphic.raycastTarget)
			{
				clickList.Add(go);
				continue;
			}
			if (go.TryGetComponent<Selectable>(out var selectable) && selectable.interactable)
			{
				clickList.Add(go);
				continue;
			}
		}
	}
	// 获得指定屏幕坐标下的可见UI,可见UI是指已激活且透明度不为0
	public static GameObject getPointerOnUI(Vector2 screenPosition)
	{
		using var a = new ListScope<RaycastResult>(out var results);
		raycastUGUI(screenPosition, results);
		// 如果点击到了非透明的图片或者文字,则不可穿透射线
		foreach (RaycastResult result in results)
		{
			GameObject go = result.gameObject;
			if (go.TryGetComponent<Image>(out var image))
			{
				if (image.raycastTarget && !isFloatZero(image.color.a))
				{
					return go;
				}
				continue;
			}
			if (go.TryGetComponent<Text>(out var text))
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
	public static void autoGridFixedRootHeight(myUGUIObject root, Vector2 gridSize, bool autoRefreshUIDepth = true, CORNER startCorner = CORNER.LEFT_TOP)
	{
		autoGridFixedRootHeight(root, gridSize, Vector2.zero, autoRefreshUIDepth, startCorner);
	}
	// 保持父节点的高度,从指定角开始纵向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	// 与autoGrid不同,autoGrid是先横向排列,使用频率较高,所以名字简化了,纵向排列的使用频率较低,所以单独写出来完整名字
	public static void autoGridFixedRootHeight(myUGUIObject root, Vector2 gridSize, Vector2 intervalNoScreenScale, bool autoRefreshUIDepth = true, CORNER startCorner = CORNER.LEFT_TOP)
	{
		RectTransform transform = root.getRectTransform();
		if (transform == null)
		{
			return;
		}
		Vector2 interval = new();
		interval.x = adjustByScreenScaleAuto(intervalNoScreenScale.x);
		interval.y = adjustByScreenScaleAuto(intervalNoScreenScale.y);
		// 先找出所有激活的子节点
		using var a = new ListScope<RectTransform>(out var childList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var childRect = transform.GetChild(i) as RectTransform;
			childList.addIf(childRect, childRect != null && childRect.gameObject.activeSelf);
		}

		// 计算父节点大小
		Vector2 rootSize = root.getSize();
		Vector3 beforeRealPosition = root.getPositionNoPivot();
		Vector3 beforeRootLeftTop = new(beforeRealPosition.x - rootSize.x * 0.5f, beforeRealPosition.y + rootSize.y * 0.5f);
		Vector3 beforeRootLeftBottom = new(beforeRealPosition.x - rootSize.x * 0.5f, beforeRealPosition.y - rootSize.y * 0.5f);
		Vector3 beforeRootRightTop = new(beforeRealPosition.x + rootSize.x * 0.5f, beforeRealPosition.y + rootSize.y * 0.5f);
		Vector3 beforeRootRightBottom = new(beforeRealPosition.x + rootSize.x * 0.5f, beforeRealPosition.y - rootSize.y * 0.5f);
		int rowCount = 1;
		if (rootSize.y > gridSize.y)
		{
			rowCount = (int)divide(rootSize.y - gridSize.y, interval.y + gridSize.y) + 1;
		}
		int activeChildCount = childList.Count;
		// 固定父节点高度时只能纵向排列
		int columnCount = divideInt(activeChildCount, rowCount) + clampMax(activeChildCount % rowCount, 1);
		rootSize.x = columnCount * gridSize.x + (columnCount - 1) * interval.x;
		// 确保宽高都是偶数,这样才能使边和坐标都是整数
		rootSize.x += (int)rootSize.x & 1;
		rootSize.y += (int)rootSize.y & 1;
		root.setSize(rootSize);

		// 计算排列子节点所需的竖直和水平方向的坐标变化符号以及起始坐标,并且调整父节点的坐标
		Vector2 startPos = Vector2.zero;
		int horizontalSign = 0;
		int verticalSign = 0;
		Vector3 curRealPosition = root.getPositionNoPivot();
		if (startCorner == CORNER.LEFT_TOP)
		{
			startPos = new Vector2(gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getLeftInSelf(), root.getTopInSelf());
			horizontalSign = 1;
			verticalSign = -1;
			// 保持左上角的坐标与改变大小之前的左上角坐标一致
			Vector3 curRootLeftTop = new(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootLeftTop, curRootLeftTop))
			{
				root.setPosition(round(root.getPosition() + beforeRootLeftTop - curRootLeftTop));
			}
		}
		else if (startCorner == CORNER.LEFT_BOTTOM)
		{
			startPos = new Vector2(gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getLeftInSelf(), root.getBottomInSelf());
			horizontalSign = 1;
			verticalSign = 1;
			// 保持左下角的坐标与改变大小之前的左下角坐标一致
			Vector3 curRootLeftBottom = new(curRealPosition.x - rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootLeftBottom, curRootLeftBottom))
			{
				root.setPosition(round(root.getPosition() + beforeRootLeftBottom - curRootLeftBottom));
			}
		}
		else if (startCorner == CORNER.RIGHT_TOP)
		{
			startPos = new Vector2(-gridSize.x * 0.5f, -gridSize.y * 0.5f) + new Vector2(root.getRightInSelf(), root.getTopInSelf());
			horizontalSign = -1;
			verticalSign = -1;
			// 保持右上角的坐标与改变大小之前的右上角坐标一致
			Vector3 curRootRightTop = new(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y + rootSize.y * 0.5f);
			if (!isVectorEqual(beforeRootRightTop, curRootRightTop))
			{
				root.setPosition(round(root.getPosition() + beforeRootRightTop - curRootRightTop));
			}
		}
		else if (startCorner == CORNER.RIGHT_BOTTOM)
		{
			startPos = new Vector2(-gridSize.x * 0.5f, gridSize.y * 0.5f) + new Vector2(root.getRightInSelf(), root.getBottomInSelf());
			horizontalSign = -1;
			verticalSign = 1;
			// 保持右下角的坐标与改变大小之前的右下角坐标一致
			Vector3 curRootRightBottom = new(curRealPosition.x + rootSize.x * 0.5f, curRealPosition.y - rootSize.y * 0.5f);
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
			if (!isVectorEqual(child.pivot, new Vector2(0.5f, 0.5f)))
			{
				logError("子节点的pivot不在中心,计算位置可能会错误");
			}
			int indexX = divideInt(i, rowCount);
			int indexY = i % rowCount;
			Vector2 pos = new((indexX * gridSize.x + indexX * interval.x) * horizontalSign,
								(indexY * gridSize.y + indexY * interval.y) * verticalSign);
			child.localPosition = round(startPos + pos);
			child.setRectSize(gridSize);
		}

		if (autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, true);
		}
	}
	public static void autoGrid(myUGUIObject root, Vector2 gridSize)
	{
		autoGrid(root, gridSize, Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT);
	}
	public static void autoGrid(myUGUIObject root, Vector2 gridSize, bool autoRefreshUIDepth)
	{
		autoGrid(root, gridSize, Vector2.zero, autoRefreshUIDepth, true, true, HORIZONTAL_DIRECTION.LEFT);
	}
	public static void autoGrid(myUGUIObject root, Vector2 gridSize, Vector2 intervalNoScreenScale)
	{
		autoGrid(root, gridSize, intervalNoScreenScale, true, true, true, HORIZONTAL_DIRECTION.LEFT);
	}
	public static void autoGrid(myUGUIObject root, Vector2 gridSize, HORIZONTAL_DIRECTION horizontal)
	{
		autoGrid(root, gridSize, Vector2.zero, true, true, true, horizontal);
	}
	public static void autoGrid(myUGUIObject root, Vector2 gridSize, Vector2 intervalNoScreenScale, HORIZONTAL_DIRECTION horizontal)
	{
		autoGrid(root, gridSize, intervalNoScreenScale, true, true, true, horizontal);
	}
	// 从左上角开始横向排列子节点,并且会改变子节点的大小
	// 也会改变root的高度,keepTop表示改变高度以后是否仍然保持root的顶部位置不变
	// gridSize是子节点的大小
	// horizontal是水平方向的停靠方式
	public static void autoGrid(myUGUIObject root, Vector2 gridSize, Vector2 intervalNoScreenScale, bool autoRefreshUIDepth, bool refreshIgnoreInactive, bool keepTopSide, HORIZONTAL_DIRECTION horizontal = HORIZONTAL_DIRECTION.LEFT)
	{
		if (root == null)
		{
			return;
		}
		root.getRectTransform().autoGrid(gridSize, intervalNoScreenScale, keepTopSide, horizontal);
		if (root.getRectTransform() != null && autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, refreshIgnoreInactive);
		}
	}
	// 在节点的顶部追加一定高度,但是不影响子节点在节点中的位置,不考虑锚点
	public static void appendTopHeight(myUGUIObject root, float appendHeight)
	{
		RectTransform transform = root.getRectTransform();
		if (transform == null)
		{
			return;
		}
		using var a = new DicScope<Transform, Vector3>(out var childWorldPositionList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			Transform child = transform.GetChild(i);
			childWorldPositionList.Add(child, child.position);
		}

		// 设置父节点新的位置和大小,重新设置所有子节点的世界坐标
		root.setHeight(root.getSize().y + appendHeight);
		root.setPositionY(root.getPosition().y + appendHeight * 0.5f);
		foreach (var item in childWorldPositionList)
		{
			item.Key.position = item.Value;
		}
	}
	// 在节点的底部追加一定高度,但是不影响子节点在节点中的位置,不考虑锚点
	public static void appendBottomHeight(myUGUIObject root, float appendHeight)
	{
		RectTransform transform = root.getRectTransform();
		if (transform == null)
		{
			return;
		}
		using var a = new DicScope<Transform, Vector3>(out var childWorldPositionList);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			Transform child = transform.GetChild(i);
			childWorldPositionList.Add(child, child.position);
		}

		// 设置父节点新的位置和大小,重新设置所有子节点的世界坐标
		root.setHeight(root.getSize().y + appendHeight);
		root.setPositionY(root.getPosition().y - appendHeight * 0.5f);
		foreach (var item in childWorldPositionList)
		{
			item.Key.position = item.Value;
		}
	}
	// 修改节点高度的同时保证顶部不会改变
	// 修改大小不会改变子节点的世界坐标和相对坐标
	// 修改位置会导致子节点的世界坐标改变,相对坐标不变
	// 所以可以选择是否保持子节点世界坐标不变
	public static void setWindowHeightKeepTop(myUGUIObject root, float height, bool keepChildWorldPosition = true)
	{
		if ((int)root.getSize().y == (int)height)
		{
			return;
		}
		int beforeRootHeight = (int)root.getSize().y;
		root.setHeight(height);
		if (keepChildWorldPosition)
		{
			using var a = new DicScope<Transform, Vector3>(out var childWorldPositionList);
			RectTransform transform = root.getRectTransform();
			if (transform == null)
			{
				return;
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				Transform child = transform.GetChild(i);
				childWorldPositionList.Add(child, child.position);
			}
			root.setPositionY(round(root.getPosition().y + (beforeRootHeight - height) * 0.5f));
			foreach (var item in childWorldPositionList)
			{
				item.Key.position = item.Value;
			}
		}
		else
		{
			root.setPositionY(round(root.getPosition().y + (beforeRootHeight - height) * 0.5f));
		}
	}
	// 根据所有子节点所占用的范围,自动计算父节点的高度,使其在y方向上正好包含所有子节点,并且保持父节点上边界位置不变,不考虑子节点的锚点
	public static void setWindowBestHeight(myUGUIObject root, bool ignoreInactive = true, bool keepTopSide = true)
	{
		using var a = new DicScope<RectTransform, Vector3>(out var childPositionList);
		float minY = 99999.0f;
		float maxY = -99999.0f;
		RectTransform transform = root.getRectTransform();
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var child = transform.GetChild(i) as RectTransform;
			if (child == null || (ignoreInactive && !child.gameObject.activeSelf))
			{
				continue;
			}
			childPositionList.Add(child, child.localPosition);
			clampMax(ref minY, child.localPosition.y - child.rect.height * 0.5f);
			clampMin(ref maxY, child.localPosition.y + child.rect.height * 0.5f);
		}

		// 设置父节点的大小,位置,已经根据之前子节点到父节点顶部的距离,还原子节点的位置
		float newHeight = maxY - minY;
		float beforeRootHeight = root.getSize().y;
		root.setHeight(newHeight);
		if (keepTopSide)
		{
			root.setPositionY(round(root.getPosition().y + (beforeRootHeight - newHeight) * 0.5f));
		}
		foreach (var item in childPositionList)
		{
			item.Key.setPositionY(newHeight * 0.5f + item.Key.localPosition.y - maxY);
		}
	}
	public static void autoGridVertical(myUGUIObject root)
	{
		autoGridVertical(root, true, true, 0.0f, 0.0f, 0.0f, 0.0f, true);
	}
	public static void autoGridVertical(myUGUIObject root, bool keepTopSide)
	{
		autoGridVertical(root, true, true, 0.0f, 0.0f, 0.0f, 0.0f, keepTopSide);
	}
	public static void autoGridVertical(myUGUIObject root, float intervalNoScreenScale)
	{
		autoGridVertical(root, true, true, intervalNoScreenScale, 0.0f, 0.0f, 0.0f, true);
	}
	public static void autoGridVertical(myUGUIObject root, bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		autoGridVertical(root, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, 0.0f, 0.0f, 0.0f, true);
	}
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列,并且不改变子节点的大小
	public static void autoGridVertical(myUGUIObject root, bool autoRefreshUIDepth, bool refreshIgnoreInactive, float intervalNoScreenScale, float minHeight = 0.0f, float extraTopHeight = 0.0f, float extraBottomHeight = 0.0f, bool keepTopSide = true, bool fromTopToBottom = true)
	{
		if (root == null)
		{
			return;
		}
		root.getRectTransform().autoGridVertical(intervalNoScreenScale, minHeight, extraTopHeight, extraBottomHeight, keepTopSide, fromTopToBottom);
		// 需要重新再调用一下setSize,以便触发虚函数的逻辑
		root.setSize(root.getSize());
		if (root.getRectTransform() != null && autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, refreshIgnoreInactive);
		}
	}
	public static void autoGridHorizontal(myUGUIObject root)
	{
		autoGridHorizontal(root, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	public static void autoGridHorizontal(myUGUIObject root, float intervalNoScreenScale)
	{
		autoGridHorizontal(root, true, true, intervalNoScreenScale, true, 0.0f, 0.0f, 0.0f, true);
	}
	public static void autoGridHorizontal(myUGUIObject root, bool keepLeftSide)
	{
		autoGridHorizontal(root, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, keepLeftSide);
	}
	public static void autoGridHorizontal(myUGUIObject root, float intervalNoScreenScale, bool keepLeftSide)
	{
		autoGridHorizontal(root, true, true, intervalNoScreenScale, true, 0.0f, 0.0f, 0.0f, keepLeftSide);
	}
	public static void autoGridHorizontal(myUGUIObject root, bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		autoGridHorizontal(root, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列,并且不改变子节点的大小,keepLeftSide为true表示改变大小后保持父节点的左边界位置不变,false表示保持右边界位置不变
	public static void autoGridHorizontal(myUGUIObject root, bool autoRefreshUIDepth, bool refreshIgnoreInactive, float intervalNoScreenScale, bool changeRootPosSize = true, float minWidth = 0.0f, float extraLeftWidth = 0.0f, float extraRightWidth = 0.0f, bool keepLeftSide = true)
	{
		if (root == null)
		{
			return;
		}
		root.getRectTransform().autoGridHorizontal(intervalNoScreenScale, changeRootPosSize, minWidth, extraLeftWidth, extraRightWidth, keepLeftSide);
		// 需要重新再调用一下setSize,以便触发虚函数的逻辑
		root.setSize(root.getSize());
		if (root.getRectTransform() != null && autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, refreshIgnoreInactive);
		}
	}
	public static void autoGridHorizontalCenter(myUGUIObject root, bool autoRefreshUIDepth, bool refreshIgnoreInactive, float intervalNoScreenScale)
	{
		if (root == null)
		{
			return;
		}
		root.getRectTransform().autoGridHorizontalCenter(intervalNoScreenScale);
		// 需要重新再调用一下setSize,以便触发虚函数的逻辑
		root.setSize(root.getSize());
		if (root.getRectTransform() != null && autoRefreshUIDepth)
		{
			root.getLayout().refreshUIDepth(root, refreshIgnoreInactive);
		}
	}
	// 自动横向排列target下的所有节点,并且如果排列后的宽度超过了parent的宽度,则左对齐,没有超过则居中
	public static void alignParentCenterOrLeft(myUGUIObject parent, myUGUIObject target)
	{
		autoGridHorizontal(target);
		// 如果宽度超过了可显示区域,则需要左对齐
		if (target.getSize().x >= parent.getSize().x)
		{
			target.setLeftToParentLeft();
		}
		// 没有超过,则需要居中显示
		else
		{
			target.setSize(replaceX(target.getSize(), parent.getSize().x));
			target.setPositionX(0.0f);
		}
	}
	// 调整transform的范围,使其包含所有子节点,并且保持子节点世界坐标不变
	public static void adjustRectTransformToContainsAllChildRect(myUGUIObject obj, bool includeInactive = false)
	{
		// 获得父节点的四个边界
		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;
		Dictionary<RectTransform, Vector3> childWorldPositionList = new();
		int childCount = obj.getRectTransform().childCount;
		for (int i = 0; i < childCount; ++i)
		{
			var child = obj.getRectTransform().GetChild(i) as RectTransform;
			if (child == null)
			{
				Debug.LogError("子节点必须有RectTransform组件");
				return;
			}
			if (!child.gameObject.activeSelf && !includeInactive)
			{
				continue;
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
		obj.setWorldPosition(new(ceil((right + left) * 0.5f), ceil((top + bottom) * 0.5f)));
		obj.setSize(new(ceil(right - left), ceil(top - bottom)));
		foreach (var item in childWorldPositionList)
		{
			obj.getLayout().getUIObject(item.Key.gameObject)?.setWorldPosition(item.Value);
		}
	}
}