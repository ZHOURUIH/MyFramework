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
	public bool mAdjustPosition = true;         // 是否根据缩放值改变位置
	public bool mRemoveUGUIAnchor = true;		// 是否移除UGUI的锚点
	public bool mKeepAspect = false;			// 是否保持宽高比
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
			Vector2 rootSize;
			if (preview)
			{
				rootSize = UnityUtility.getGameViewSize();
			}
			else
			{
				rootSize = UnityUtility.getRootSize();
			}
			mScreenScale = UnityUtility.getScreenScale(rootSize);
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
		float thisWidth = MathUtility.checkInt(mOriginSize.x * realScale.x, 0.001f);
		float thisHeight = MathUtility.checkInt(mOriginSize.y * realScale.y, 0.001f);
		Vector2 newSize = new Vector2(thisWidth, thisHeight);
		// 只有在刷新时才能确定父节点,所以父节点需要实时获取
		if(mAdjustFont)
		{
			WidgetUtility.setRectSizeWithFontSize(GetComponent<RectTransform>(), newSize);
		}
		else
		{
			WidgetUtility.setRectSize(GetComponent<RectTransform>(), newSize);
		}
		if(mAdjustPosition)
        {
			transform.localPosition = MathUtility.round(MathUtility.multiVector3(mOriginPos, realScale));
		}
	}
}