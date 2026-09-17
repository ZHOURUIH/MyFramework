using UnityEngine;

// 插值改变一个物体的旋转,如果目标旋转不变,离目标旋转越近,旋转速度越慢
public class CmdTransformableLerpRotation
{
	// 插值中回调
	// 插值完成时回调
	// 目标旋转值
	// 插值速度
	public static void execute(ITransformable obj, Vector3 targetRotation, float lerpSpeed, LerpCallback doingCallBack, LerpCallback doneCallBack)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableLerpRotation com);
		com.setLerpingCallback(doingCallBack);
		com.setLerpDoneCallback(doneCallBack);
		com.setActive(true);
		com.setTargetRotation(targetRotation);
		com.setLerpSpeed(lerpSpeed);
		com.play();
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
		obj.getOrAddComponent(out COMTransformableLerpRotation com);
		com.stop();
		com.setActive(false);
	}
}