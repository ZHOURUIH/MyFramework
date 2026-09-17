using UnityEngine;

// 追踪一个目标
public class CmdTransformableTrackTarget
{
	// 追踪目标
	// 追踪时回调
	// 追踪结束时回调
	// 追踪目标位置偏移
	// 距离目标还剩多少距离时认为是追踪完成
	// 追踪速度
	// 是否在FixedUpdate中更新位置
	public static void execute(ITransformable obj, ITransformable target, Vector3 offset, float nearRange, float speed, bool updateInFixedTick, BoolCallback doingCallback, BoolCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out ComponentTrackTarget com);
		com.setUpdateInFixedTick(updateInFixedTick);
		com.setActive(true);
		com.setTrackNearRange(nearRange);
		com.setSpeed(speed);
		com.setTargetOffset(offset);
		com.setTrackingCallback(doingCallback);
		com.setMoveDoneTrack(target, doneCallback);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}