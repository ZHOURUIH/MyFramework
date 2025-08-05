using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseUtility;

// 以指定的路径缩放一个物体
public class CmdTransformableScalePath
{
	// 缩放值和时间的关键帧列表
	// 缩放中回调
	// 缩放完成时回调
	// 缩放偏移,计算出的位置会再乘上这个偏移作为最终世界缩放
	// 起始时间偏移
	// 缩放速度
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallBack, Vector3 valueOffset, float timeOffset, float speed, int keyframe, bool loop)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			valueKeyFrame != null && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName() + ", type:" + uiObj.GetType());
		}
		obj.getOrAddComponent(out COMTransformableScalePath com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(valueKeyFrame);
		com.setSpeed(speed);
		com.setValueOffset(valueOffset);
		com.setOffsetBlendAdd(false);
		com.play(keyframe, loop, timeOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(Transformable obj)
	{
		obj?.getComponent<COMTransformableScalePath>()?.stop();
	}
}