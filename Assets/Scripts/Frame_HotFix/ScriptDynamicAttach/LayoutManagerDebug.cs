using UnityEngine;
using static FrameBase;

// 布局管理器调试信息
public class LayoutManagerDebug : MonoBehaviour
{
	public bool UseAnchor;	// 是否使用自定义自适应组件
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		UseAnchor = mLayoutManager.isUseAnchor();
	}
}