using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 插值改变一个物体的位置,如果目标点不变,离目标点越近,移动速度越慢
public class CmdTransformableLerpPosition
{
	// 目标位置
	// 插值速度
	// 插值中回调
	// 插值完成时回调
	public static void execute(ITransformable obj, Vector3 targetPosition, float lerpSpeed, LerpCallback doingCallBack, LerpCallback doneCallBack)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			!isFloatZero(lerpSpeed) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableLerpPosition com);
		com.setLerpingCallback(doingCallBack);
		com.setLerpDoneCallback(doneCallBack);
		com.setActive(true);
		com.setTargetPosition(targetPosition);
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
		obj.getOrAddComponent(out COMTransformableLerpPosition com);
		com.stop();
		com.setActive(false);
	}
}