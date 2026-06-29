using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 以抛物线追踪一个目标
public class CmdTransformableTrackTargetParabola
{
	// 追踪目标
	// 抛物线起点
	// 追踪目标位置偏移
	// 距离目标还剩多少距离时认为是追踪完成
	// 追踪速度
	// 抛物线最高高度,相对于起点
	// 是否在FixedUpdate中更新位置
	// 追踪时回调
	// 追踪结束时回调
	public static void execute(ITransformable obj, ITransformable target, Vector3 startPosition, Vector3 offset, float nearRange, float speed, float maxHeight, bool updateInFixedTick, BoolCallback doingCallback, BoolCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			obj is myUGUIObject uiObj &&
			target != null && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out ComponentTrackTargetParabola com);
		com.setUpdateInFixedTick(updateInFixedTick);
		com.setActive(true);
		com.setTrackNearRange(nearRange);
		com.setSpeed(speed);
		com.setTargetOffset(offset);
		com.setMaxHeight(maxHeight);
		com.setStartPosition(startPosition);
		com.setTrackingCallback(doingCallback);
		com.setMoveDoneTrack(target, doneCallback);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}