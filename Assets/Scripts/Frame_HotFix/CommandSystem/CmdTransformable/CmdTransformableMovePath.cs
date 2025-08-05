using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseUtility;

// 以指定的关键帧路径移动物体
public class CmdTransformableMovePath
{
	// 时间与位置的关键帧列表
	// 移动中回调
	// 移动完成时回调
	// 位置偏移,计算出的位置会再加上这个偏移作为最终世界坐标
	// 起始时间偏移
	// 移动速度
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
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableMovePath com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(valueKeyFrame);
		com.setSpeed(speed);
		com.setValueOffset(valueOffset);
		com.play(keyframe, loop, timeOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(Transformable obj)
	{
		obj?.getComponent<COMTransformableMovePath>()?.stop();
	}
}