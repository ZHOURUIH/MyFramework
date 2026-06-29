using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseUtility;

// 以指定的关键帧路径移动和缩放物体
public class CmdTransformableMoveScalePath
{
	// 时间与位置的关键帧列表
	// 时间与缩放的关键帧列表
	// 移动中回调
	// 移动完成时回调
	// 位置偏移,计算出的位置会再加上这个偏移作为最终世界坐标
	// 缩放偏移
	// 起始时间偏移
	// 移动速度
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(ITransformable obj, Dictionary<float, Vector3> moveKeyFrame, Dictionary<float, Vector3> scaleKeyFrame, 
								KeyFrameCallback doingCallBack, KeyFrameCallback doneCallBack, Vector3 moveOffset, Vector3 scaleOffset, float timeOffset, 
								float speed, int keyframe, bool loop)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			obj is myUGUIObject uiObj &&
			moveKeyFrame != null &&
			scaleKeyFrame != null && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableMoveScalePath com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setValueKeyFrame0(moveKeyFrame);
		com.setValueKeyFrame1(scaleKeyFrame);
		com.setSpeed(speed);
		com.setValueOffset0(moveOffset);
		com.setValueOffset1(scaleOffset);
		com.setOffsetBlendAdd0(true);
		com.setOffsetBlendAdd1(false);
		com.play(keyframe, loop, timeOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(ITransformable obj)
	{
		obj?.getComponent<COMTransformableMoveScalePath>()?.stop();
	}
}