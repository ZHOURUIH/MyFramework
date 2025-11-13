using UnityEngine;
using static FrameBaseUtility;
using static FrameBaseDefine;
using static UnityUtility;

// 缩放自适应组件
public class ScaleAnchor3D : MonoBehaviour
{
	protected bool mDirty = true;						// 是否需要刷新数据
	protected bool mFirstUpdate = true;					// 是否为第一次更新,如果是第一次更新,则需要获取原始属性
	protected Vector2 mScreenScale = Vector2.one;		// 屏幕相对于标准分辨率的缩放
	protected Vector3 mOriginPos;						// 原始的位置
	protected Vector2 mOriginScale = Vector2.one;       // 原始的缩放
	public ASPECT_BASE mAspectBase = ASPECT_BASE.AUTO;
	public void Awake()
	{
		enabled = !Application.isPlaying;
	}
	public void updateRect(bool force = false)
	{
		// 是否为编辑器手动预览操作,手动预览不需要启动游戏
		bool preview = !isPlaying();
		// 如果是第一次更新,则需要获取原始属性
		if (mFirstUpdate || preview)
		{
			mScreenScale = getScreenScale(preview ? getGameViewSize() : getRootSize());
			mOriginPos = transform.localPosition;
			mOriginScale = transform.localScale;
			mFirstUpdate = false;
		}
		if (!preview && !force && !mDirty)
		{
			return;
		}
		mDirty = false;
		// 只有在刷新时才能确定父节点,所以父节点需要实时获取
		float scale = adjustScreenScale(mScreenScale, mAspectBase).x;
		transform.localScale = mOriginScale * scale;
		transform.localPosition = mOriginPos * scale;
	}
	public void Reset()
	{
		if (!isEditor())
		{
			return;
		}
		// 挂载该脚本时候检查当前GameView的分辨率是否是标准分辨率
		Vector2 screenSize = getGameViewSize();
		if ((int)screenSize.x != STANDARD_WIDTH || (int)screenSize.y != STANDARD_HEIGHT)
		{
			displayDialog("错误", "当前分辨率不是标准分辨率,适配结果可能不对,请将Game视图的分辨率修改为" +
							STANDARD_WIDTH + "*" + STANDARD_HEIGHT, "确定");
			DestroyImmediate(this);
		}
	}
}