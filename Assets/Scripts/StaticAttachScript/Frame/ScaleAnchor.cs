using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum ASPECT_BASE
{
	AB_USE_WIDTH_SCALE,     // 使用宽的缩放值来缩放控件
	AB_USE_HEIGHT_SCALE,    // 使用高的缩放值来缩放控件
	AB_AUTO,				// 取宽高缩放值中最小的,保证缩放以后不会超出屏幕范围
	AB_INVERSE_AUTO,        // 取宽高缩放值中最大的,保证缩放以后不会在屏幕范围留出空白
	AB_NONE,
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
	public ASPECT_BASE mAspectBase = ASPECT_BASE.AB_AUTO;
	public void updateRect(bool force = false)
	{
		// 是否为编辑器手动预览操作,手动预览不需要启动游戏
#if UNITY_EDITOR
		bool preview = !EditorApplication.isPlaying;
#else
		bool preview = false;
#endif
		// 如果是第一次更新,则需要获取原始属性
		if (mFirstUpdate || preview)
		{
			bool ngui = ReflectionUtility.isNGUI(gameObject);
			mScreenScale = ReflectionUtility.getScreenScale(ReflectionUtility.getRootSize(ngui, preview));
			if (ngui)
			{
#if USE_NGUI
				mOriginSize = ReflectionUtility.getNGUIRectSize(GetComponent<UIWidget>());
#endif
			}
			else
			{
				mOriginSize = ReflectionUtility.getUGUIRectSize(GetComponent<RectTransform>());
			}
			mOriginPos = transform.localPosition;
			mFirstUpdate = false;
		}
		if (!preview && !force && !mDirty)
		{
			return;
		}
		mDirty = false;
		Vector3 realScale = ReflectionUtility.adjustScreenScale(mScreenScale, mKeepAspect ? mAspectBase : ASPECT_BASE.AB_NONE);
		float thisWidth = mOriginSize.x * realScale.x;
		float thisHeight = mOriginSize.y * realScale.y;
		MathUtility.checkInt(ref thisWidth, 0.001f);
		MathUtility.checkInt(ref thisHeight, 0.001f);
		Vector2 newSize = new Vector2(thisWidth, thisHeight);
		// 只有在刷新时才能确定父节点,所以父节点需要实时获取
		Vector2 parentSize = Vector2.zero;
		if (ReflectionUtility.isNGUI(gameObject))
		{
#if USE_NGUI
			ReflectionUtility.setNGUIWidgetSize(GetComponent<UIWidget>(), newSize);
			parentSize = ReflectionUtility.getNGUIRectSize(ReflectionUtility.findNGUIParentRect(gameObject));
#endif
		}
		else
		{
			ReflectionUtility.setUGUIRectSize(GetComponent<RectTransform>(), newSize, mAdjustFont);
			parentSize = ReflectionUtility.getUGUIRectSize(transform.parent.GetComponent<RectTransform>());
		}
		transform.localPosition = ReflectionUtility.round(ReflectionUtility.multiVector3(mOriginPos, realScale));
	}
}