using System;
using UnityEngine;
using UnityEngine.UI;

public class WidgetUtility : FrameBase
{
	// 用于避免GC而保存的变量
	private static Vector3[] mRootCorner = null;
	private static Vector3[] mTempCorners = new Vector3[4];
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
	public static void setRectSize(RectTransform rectTransform, Vector2 size, bool adjustFont)
	{
		if(rectTransform == null)
		{
			return;
		}
		Rect rect = rectTransform.rect;
		float lastHeight = rect.height;
		Vector2 deltaSize = new Vector2(size.x, size.y) - rect.size;
		rectTransform.offsetMin -= new Vector2(deltaSize.x * rectTransform.pivot.x, deltaSize.y * rectTransform.pivot.y);
		rectTransform.offsetMax += new Vector2(deltaSize.x * (1.0f - rectTransform.pivot.x), deltaSize.y * (1.0f - rectTransform.pivot.y));
		if(adjustFont)
		{
			// 文字控件需要根据高度重新计算字体大小
			Text text = rectTransform.GetComponent<Text>();
			if (text != null)
			{
				text.fontSize = floor(text.fontSize / lastHeight * size.y);
			}
		}
	}
}