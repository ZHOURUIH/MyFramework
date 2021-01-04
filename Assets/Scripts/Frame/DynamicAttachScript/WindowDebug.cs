using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowDebug : MonoBehaviour
{
	public bool ForceRefresh;					// 是否强制刷新,无论是否启用了EnableScriptDebug
	public myUIObject mWindow;
	public string Depth;
	public float LongPressTimeThreshold;		// 长按的时间阈值,超过阈值时检测为长按
	public float LongPressLengthThreshold;		// 小于0表示不判断鼠标移动对长按检测的影响
	public bool DepthOverAllChild;              // 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
	public bool PassRay;                        // 当存在且注册了碰撞体时是否允许射线穿透
	public bool Enable;							// 是否启用更新
	public int OrderInParent;					// 在父节点中的顺序
	public int ID;                              // 每个窗口的唯一ID
	public void setWindow(myUIObject window) { mWindow = window; }
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug() && !ForceRefresh)
		{
			return;
		}
		if (mWindow == null)
		{
			return;
		}
		Depth = mWindow.getDepth().toDepthString();
		LongPressTimeThreshold = mWindow.getLongPressTimeThreshold();
		LongPressLengthThreshold = mWindow.getLongPressLengthThreshold();
		DepthOverAllChild = mWindow.isDepthOverAllChild();
		PassRay = mWindow.isPassRay();
		Enable = mWindow.isEnable();
		OrderInParent = mWindow.getDepth().getOrderInParent();
		ID = mWindow.getID();
	}
}