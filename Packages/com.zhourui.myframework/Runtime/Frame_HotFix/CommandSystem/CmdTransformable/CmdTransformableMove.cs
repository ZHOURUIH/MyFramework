using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 移动物体
public class CmdTransformableMove
{
	// 移动中回调
	// 移动结束时回调
	// 起始坐标
	// 目标坐标
	// 单次所需时间
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	// 是否在物理更新中执行
	public static void execute(ITransformable obj, Vector3 startPos, Vector3 targetPos, float onceLength, float offset, int keyframe, bool loop, bool updateInFixedTick, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
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
		obj.getOrAddComponent(out COMTransformableMove com);
		com.setUpdateInFixedTick(updateInFixedTick);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setTarget(targetPos);
		com.setStart(startPos);
		com.play(keyframe, loop, onceLength, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(ITransformable obj, Vector3 startPos)
	{
		if (obj == null)
		{
			return;
		}
		obj.getComponent(out COMTransformableMove com);
		if (com == null || !com.isActive())
		{
			obj.setPosition(startPos);
			return;
		}
		com.setTarget(startPos);
		com.setStart(startPos);
		com.play(0, false, 0.0f, 0.0f);
	}
}