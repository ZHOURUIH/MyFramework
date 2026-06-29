using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 以抛物线移动物体
public class CmdTransformableMoveParabola
{
	// 移动中回调
	// 移动完成时回调
	// 起始位置
	// 目标位置
	// 单次所需时间
	// 抛物线最高的高度,相对于起始位置
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(ITransformable obj, Vector3 startPos, Vector3 targetPos, float onceLength, float topHeight, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
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
		obj.getOrAddComponent(out COMTransformableMoveParabola com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setTargetPos(targetPos);
		com.setStartPos(startPos);
		com.setTopHeight(topHeight);
		com.play(keyframe, loop, onceLength, offset);
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
		obj.getOrAddComponent(out COMTransformableMoveParabola com);
		com.play(0, false, 0.0f, 0.0f);
	}
}