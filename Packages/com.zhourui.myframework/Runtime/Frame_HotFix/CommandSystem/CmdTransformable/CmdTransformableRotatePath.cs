using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseUtility;

// 以一定的关键帧旋转物体
public class CmdTransformableRotatePath
{
	// 旋转与时间的关键帧列表
	// 旋转中回调
	// 旋转完成时回调
	// 旋转偏移,计算出的位置会再加上这个偏移作为最终世界旋转
	// 起始时间偏移
	// 旋转速度
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float offset, float speed, int keyframe, bool loop, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallBack)
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
		obj.getOrAddComponent(out COMTransformableRotatePath com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(valueKeyFrame);
		com.setSpeed(speed);
		com.setValueOffset(valueOffset);
		com.play(keyframe, loop, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(ITransformable obj)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableRotatePath com);
		com.play(0, false, 0);
	}
}