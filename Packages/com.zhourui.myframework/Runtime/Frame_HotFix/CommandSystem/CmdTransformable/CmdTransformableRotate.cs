using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 旋转物体
public class CmdTransformableRotate
{
	// 旋转中回调
	// 旋转完成时回调
	// 起始旋转值
	// 目标旋转值
	// 单次所需时间
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	// 是否在物理更新中执行
	public static void execute(ITransformable obj, Vector3 startRotation, Vector3 targetRotation, float onceLength, float offset, int keyframe, bool loop, bool updateInFixedTick, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			!isFloatZero(onceLength) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableRotate com);
		com.setUpdateInFixedTick(updateInFixedTick);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setTarget(targetRotation);
		com.setStart(startRotation);
		com.play(keyframe, loop, onceLength, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(ITransformable obj, Vector3 rotation)
	{
		if (obj == null)
		{
			return;
		}
		obj.getComponent(out COMTransformableRotate com);
		if (com == null || !com.isActive())
		{
			obj.setRotation(rotation);
			return;
		}
		com.setTarget(rotation);
		com.setStart(rotation);
		com.play(0, false, 0.0f, 0.0f);
	}
}