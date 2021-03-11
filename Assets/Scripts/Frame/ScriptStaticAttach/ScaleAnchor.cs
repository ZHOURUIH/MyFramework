using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ScaleAnchor : MonoBehaviour
{
	protected bool mDirty = true;
	protected bool mFirstUpdate = true;
	protected Vector2 mScreenScale = Vector2.one;
	protected Vector2 mOriginSize;
	protected Vector3 mOriginPos;
	// 用于保存属性的变量,需要为public权限
	public bool mAdjustFont = true;
	public bool mKeepAspect;			// 是否保持宽高比
	public ASPECT_BASE mAspectBase = ASPECT_BASE.AUTO;
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
			mScreenScale = UnityUtility.getScreenScale(UnityUtility.getRootSize(preview));
			mOriginSize = WidgetUtility.getRectSize(GetComponent<RectTransform>());
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
		WidgetUtility.setRectSize(GetComponent<RectTransform>(), newSize, mAdjustFont);
		transform.localPosition = MathUtility.round(MathUtility.multiVector3(mOriginPos, realScale));
	}
}