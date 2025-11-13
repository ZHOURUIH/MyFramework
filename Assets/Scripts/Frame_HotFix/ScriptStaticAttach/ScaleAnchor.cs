using UnityEngine;
using static MathUtility;
using static UnityUtility;
using static WidgetUtility;
using static FrameBaseUtility;

// 缩放自适应组件
public class ScaleAnchor : MonoBehaviour
{
	protected bool mDirty = true;							// 是否需要刷新数据
	protected bool mFirstUpdate = true;						// 是否为第一次更新,如果是第一次更新,则需要获取原始属性
	protected Vector2 mScreenScale = Vector2.one;			// 屏幕相对于标准分辨率的缩放
	protected Vector2 mOriginSize;							// 原始的窗口大小
	protected Vector3 mOriginPos;							// 原始的位置
	// 用于保存属性的变量,需要为public权限
	public int mMinFontSize = 12;							// 字体的最小大小
	public bool mAdjustFont = true;							// 是否连同字体大小也一起调整
	public bool mAdjustPosition = true;						// 是否根据缩放值改变位置
	public bool mRemoveUGUIAnchor = true;					// 是否移除UGUI的锚点
	public bool mKeepAspect = true;							// 是否保持宽高比
	public ASPECT_BASE mAspectBase = ASPECT_BASE.AUTO;      // 如果要保持宽高比时的缩放基准,也就是选择哪个作为缩放值
	public void Awake()
	{
		enabled = !Application.isPlaying;
	}
	public void updateRect(bool force = false)
	{
		// 是否为编辑器手动预览操作,手动预览不需要启动游戏
		bool preview = !isPlaying();
		// 如果是第一次更新,则需要获取原始属性
		if (!TryGetComponent<RectTransform>(out var rectTransform))
		{
			logError("物体上找不到RectTransform,name:" + name);
		}
		if (mFirstUpdate || preview)
		{
			mScreenScale = getScreenScale(preview ? getGameViewSize() : getRootSize());
			mOriginSize = rectTransform.rect.size;
			mOriginPos = getPositionNoPivotInParent(rectTransform);
			mFirstUpdate = false;
		}
		if (!preview && !force && !mDirty)
		{
			return;
		}
		mDirty = false;
		Vector3 realScale = getRealScale();
		float thisWidth = floor(mOriginSize.x * realScale.x);
		float thisHeight = floor(mOriginSize.y * realScale.y);
		Vector2 newSize = new(thisWidth, thisHeight);
		// 只有在刷新时才能确定父节点,所以父节点需要实时获取
		if (mAdjustFont)
		{
			setRectSizeWithFontSize(rectTransform, newSize, mMinFontSize);
		}
		else
		{
			setRectSize(rectTransform, newSize);
		}
		if (mAdjustPosition)
		{
			setPositionNoPivotInParent(rectTransform, round(multiVector3(mOriginPos, realScale)));
		}
	}
	// 获取实际的可直接用于计算的宽高缩放值,会考虑保持宽高比以及缩放基准
	public Vector2 getRealScale()
	{
		return adjustScreenScale(mScreenScale, mKeepAspect ? mAspectBase : ASPECT_BASE.NONE);
	}
}