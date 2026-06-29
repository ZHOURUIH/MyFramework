using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 以指定的缩放列表缩放物体
public class CmdTransformableScaleCurve
{
	// 缩放列表
	// 缩放中回调
	// 缩放完成时回调
	// 单次所需时间
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(ITransformable obj, List<Vector3> scaleList, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			!isFloatZero(onceLength) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableScaleCurve com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setKeyList(scaleList);
		com.play(keyframe, loop, onceLength, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(ITransformable obj)
	{
		obj.getOrAddComponent(out COMTransformableScaleCurve com);
		com.play(0, false, 0.0f, 0.0f);
	}
}