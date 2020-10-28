using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WidgetUtility : FrameBase
{
	// 用于避免GC而保存的变量
	private static Vector3[] mRootCorner = null;
	private static Vector3[] mTempCorners = new Vector3[4];
#if USE_NGUI
	private static Vector3[] mTempSides = new Vector3[4];
	private static Vector2 mTempVector2 = Vector2.zero;
#endif
	public static GUI_TYPE getGUIType(GameObject go)
	{
		if (go.GetComponent<RectTransform>() == null)
		{
			return GUI_TYPE.UGUI;
		}
		return GUI_TYPE.NGUI;
	}
	// 父节点在父节点坐标系下的各条边
	public static void getParentSides(GameObject parent, Vector3[] sides)
	{
		GUI_TYPE guiType = getGUIType(parent);
		if (guiType == GUI_TYPE.NGUI)
		{
#if USE_NGUI
			UIRect parentRect = parent.GetComponent<UIRect>();
			if (parentRect == null)
			{
				int count = sides.Length;
				for (int i = 0; i < count; ++i)
				{
					sides[i] = Vector3.zero;
				}
			}
			else
			{
				getNGUIRectLocalSide(parentRect, sides);
			}
#endif
		}
		else if(guiType == GUI_TYPE.UGUI)
		{
			Vector2 size = parent.GetComponent<RectTransform>().rect.size;
			mTempCorners[0] = new Vector3(-size.x * 0.5f, -size.y * 0.5f);
			mTempCorners[1] = new Vector3(-size.x * 0.5f, size.y * 0.5f);
			mTempCorners[2] = new Vector3(size.x * 0.5f, size.y * 0.5f);
			mTempCorners[3] = new Vector3(size.x * 0.5f, -size.y * 0.5f);
			cornerToSide(mTempCorners, sides);
		}
	}
	public static Vector2 getUGUIRectSize(RectTransform rect)
	{
		return rect.rect.size;
	}
	public static Vector3[] getRootCorner(GUI_TYPE guiType)
	{
		if (mRootCorner == null)
		{
			Vector2 rootSize = getRootSize(guiType);
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
#if USE_NGUI
	public static Vector2 getNGUIRectSize(UIRect rect)
	{
		getNGUIRectLocalSide(rect, mTempSides);
		float width = getLength(mTempSides[0] - mTempSides[2]);
		float height = getLength(mTempSides[1] - mTempSides[3]);
		mTempVector2.x = width;
		mTempVector2.y = height;
		return mTempVector2;
	}
	public static void getNGUIRectLocalSide(UIRect rect, Vector3[] sides)
	{
		// 没有父节点则认为是UIRoot节点的
		Vector3[] localCorners = rect.transform.parent != null ? rect.localCorners : getRootCorner(true);
		cornerToSide(localCorners, sides);
	}
	public static UIRect findNGUIParentRect(GameObject obj)
	{
		if (obj == null || obj.transform.parent == null)
		{
			return null;
		}
		GameObject parent = obj.transform.parent.gameObject;
		if (parent != null)
		{
			// 自己有父节点,并且父节点有UIRect,则返回父节点的UIRect
			UIRect widget = parent.GetComponent<UIRect>();
			if (widget != null)
			{
				return widget;
			}
			// 父节点没有UIRect,则继续往上找
			else
			{
				return findNGUIParentRect(parent);
			}
		}
		else
		{
			return null;
		}
	}
	public static void setNGUIWidgetSize(UIWidget widget, Vector2 size, bool adjustFont = true)
	{
		// 没有widget则是panel,panel是没有宽高的
		if (widget != null)
		{
			widget.keepAspectRatio = UIWidget.AspectRatioSource.Free;
			int lastHeight = widget.height;
			widget.width = ceil(size.x);
			widget.height = ceil(size.y);
			// 文字控件需要根据高度重新计算字体大小
			if (widget is UILabel && adjustFont)
			{
				UILabel label = widget as UILabel;
				label.fontSize = ceil((float)label.fontSize / lastHeight * widget.height);
			}
		}
	}
#endif
	public static void setUGUIRectSize(RectTransform rectTransform, Vector2 size, bool adjustFont)
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