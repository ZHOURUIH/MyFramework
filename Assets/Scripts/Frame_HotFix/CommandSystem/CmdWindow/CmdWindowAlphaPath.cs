using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseUtility;

// 以指定的路径渐变窗口透明度
public class CmdWindowAlphaPath
{
	// 透明度和时间的关键帧列表
	// 变化中回调
	// 变化完成时回调
	// 透明偏移,计算出的值会再加上这个偏移作为最终透明
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(myUIObject obj, Dictionary<float, float> valueKeyFrame, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallBack, float valueOffset, float timeOffset, float speed, bool loop)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			valueKeyFrame != null && 
			!obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + obj.getName());
		}
		obj.getOrAddComponent(out COMWindowAlphaPath com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(valueKeyFrame);
		com.setSpeed(speed);
		com.setValueOffset(valueOffset);
		com.play(loop, timeOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(myUIObject obj)
	{
		obj?.getComponent<COMWindowAlphaPath>()?.stop();
	}
}