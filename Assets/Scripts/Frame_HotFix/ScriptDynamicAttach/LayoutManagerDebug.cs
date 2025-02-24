using UnityEngine;
using static FrameBaseHotFix;

// 布局管理器调试信息
public class LayoutManagerDebug : MonoBehaviour
{
	public bool UseAnchor;	// 是否使用自定义自适应组件
	public void Update()
	{
		if (mGameFrameworkHotFix == null || !mGameFrameworkHotFix.mParam.mEnableScriptDebug)
		{
			return;
		}
		UseAnchor = mLayoutManager.isUseAnchor();
	}
}