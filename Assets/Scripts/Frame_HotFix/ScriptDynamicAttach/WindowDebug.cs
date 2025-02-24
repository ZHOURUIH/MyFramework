using UnityEngine;
using static FrameBaseHotFix;

// UI窗口调试信息
public class WindowDebug : MonoBehaviour
{
	protected myUIObject mWindow;				// 当前的窗口
	public bool ForceRefresh;					// 是否强制刷新,无论是否启用了EnableScriptDebug
	public string Depth;						// 窗口深度
	public bool DepthOverAllChild;              // 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
	public bool PassRay;                        // 当存在且注册了碰撞体时是否允许射线穿透
	public bool Enable;							// 是否启用更新
	public int OrderInParent;					// 在父节点中的顺序
	public int ID;                              // 每个窗口的唯一ID
	public void setWindow(myUIObject window) { mWindow = window; }
	public void Update()
	{
		if (mGameFrameworkHotFix == null || (!mGameFrameworkHotFix.mParam.mEnableScriptDebug && !ForceRefresh) || mWindow == null)
		{
			return;
		}
		Depth = mWindow.getDepth().toDepthString();
		DepthOverAllChild = mWindow.isDepthOverAllChild();
		PassRay = mWindow.isPassRay();
		Enable = mWindow.isNeedUpdate();
		OrderInParent = mWindow.getDepth().getOrderInParent();
		ID = mWindow.getID();
	}
}