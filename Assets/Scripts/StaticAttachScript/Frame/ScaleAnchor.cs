using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum ASPECT_BASE : byte
{
	USE_WIDTH_SCALE,    // 使用宽的缩放值来缩放控件
	USE_HEIGHT_SCALE,   // 使用高的缩放值来缩放控件
	AUTO,				// 取宽高缩放值中最小的,保证缩放以后不会超出屏幕范围
	INVERSE_AUTO,       // 取宽高缩放值中最大的,保证缩放以后不会在屏幕范围留出空白
	NONE,
}

public class ScaleAnchor : MonoBehaviour
{
	protected bool mDirty = true;
	protected bool mFirstUpdate = true;
	protected Vector2 mScreenScale = Vector2.one;
	protected Vector2 mOriginSize;
	protected Vector3 mOriginPos;
	// 用于保存属性的变量,需要为public权限
	public bool mAdjustFont = true;
	public bool mKeepAspect;    // 是否保持宽高比
	public ASPECT_BASE mAspectBase = ASPECT_BASE.AUTO;
	public void updateRect(bool force = false)
	{
		// 是否为编辑器手动预览操作,手动预览不需要启动游戏
#if UNITY_EDITOR
		bool preview = !EditorApplication.isPlaying;
#else
		bool preview = false;
#endif
		GUI_TYPE guiType = WidgetUtility.getGUIType(gameObject);
		// 如果是第一次更新,则需要获取原始属性
		if (mFirstUpdate || preview)
		{
			mScreenScale = UnityUtility.getScreenScale(UnityUtility.getRootSize(guiType, preview));
			if (guiType == GUI_TYPE.NGUI)
			{
#if USE_NGUI
				mOriginSize = WidgetUtility.getNGUIRectSize(GetComponent<UIWidget>());
#endif
			}
			else if(guiType == GUI_TYPE.UGUI)
			{
				mOriginSize = WidgetUtility.getUGUIRectSize(GetComponent<RectTransform>());
			}
			mOriginPos = transform.localPosition;
			mFirstUpdate = false;
		}
		if (!preview && !force && !mDirty)
		{
			return;
		}
		mDirty = false;
		Vector3 realScale = UnityUtility.adjustScreenScale(mScreenScale, mKeepAspect ? mAspectBase : ASPECT_BASE.NONE);
		float thisWidth = mOriginSize.x * realScale.x;
		float thisHeight = mOriginSize.y * realScale.y;
		MathUtility.checkInt(ref thisWidth, 0.001f);
		MathUtility.checkInt(ref thisHeight, 0.001f);
		Vector2 newSize = new Vector2(thisWidth, thisHeight);
		// 只有在刷新时才能确定父节点,所以父节点需要实时获取
		Vector2 parentSize = Vector2.zero;
		if (guiType == GUI_TYPE.NGUI)
		{
#if USE_NGUI
			WidgetUtility.setNGUIWidgetSize(GetComponent<UIWidget>(), newSize);
			parentSize = WidgetUtility.getNGUIRectSize(WidgetUtility.findNGUIParentRect(gameObject));
#endif
		}
		else if(guiType == GUI_TYPE.UGUI)
		{
			WidgetUtility.setUGUIRectSize(GetComponent<RectTransform>(), newSize, mAdjustFont);
			parentSize = WidgetUtility.getUGUIRectSize(transform.parent.GetComponent<RectTransform>());
		}
		transform.localPosition = MathUtility.round(MathUtility.multiVector3(mOriginPos, realScale));
	}
}