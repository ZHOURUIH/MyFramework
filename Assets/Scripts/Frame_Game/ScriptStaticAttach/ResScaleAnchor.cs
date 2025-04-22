using UnityEngine;
using static MathUtility;
using static FrameBaseUtility;
using static UnityUtility;
using static WidgetUtility;
using static FrameBase;

// 缩放自适应组件
public class ResScaleAnchor : MonoBehaviour
{
	protected bool mDirty = true;							// 是否需要刷新数据
	protected bool mFirstUpdate = true;						// 是否为第一次更新,如果是第一次更新,则需要获取原始属性
	protected Vector2 mScreenScale = Vector2.one;			// 屏幕相对于标准分辨率的缩放
	protected Vector2 mOriginSize;							// 原始的窗口大小
	protected Vector3 mOriginPos;							// 原始的位置
	// 用于保存属性的变量,需要为public权限
	public bool mKeepAspect = true;							// 是否保持宽高比
	public ASPECT_BASE mAspectBase = ASPECT_BASE.AUTO;      // 缩放基准
	public void updateRect(bool force = false)
	{
		// 如果是第一次更新,则需要获取原始属性
		if (!TryGetComponent<RectTransform>(out var rectTransform))
		{
			logErrorBase("物体上找不到RectTransform,name:" + name);
		}
		if (mFirstUpdate)
		{
			mScreenScale = getScreenScale(mLayoutManager.getRootSize());
			mOriginSize = rectTransform.rect.size;
			mOriginPos = getPositionNoPivotInParent(rectTransform);
			mFirstUpdate = false;
		}
		if (!force && !mDirty)
		{
			return;
		}
		mDirty = false;
		Vector3 realScale = adjustScreenScale(mScreenScale, mKeepAspect ? mAspectBase : ASPECT_BASE.NONE);
		float thisWidth = Mathf.RoundToInt(mOriginSize.x * realScale.x);
		float thisHeight = Mathf.RoundToInt(mOriginSize.y * realScale.y);
		Vector2 newSize = new(thisWidth, thisHeight);
		// 只有在刷新时才能确定父节点,所以父节点需要实时获取
		setRectSizeWithFontSize(rectTransform, newSize, 12);
		setPositionNoPivotInParent(rectTransform, round(multiVector3(mOriginPos, realScale)));
	}
}