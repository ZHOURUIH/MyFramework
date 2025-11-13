using System.Collections.Generic;

// 按一个指定的时间与透明度的列表来变化透明度
public class CmdMovableObjectAlphaPath
{
	// 透明度与时间的关键帧列表
	// 变化中回调
	// 变化完成时回调
	// 位置偏移,计算出的位置会再加上这个偏移作为最终透明
	// 起始时间偏移
	// 变化速度
	// 是否循环
	public static void execute(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float offset, float speed, bool loop, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallBack)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMMovableObjectAlphaPath com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(valueKeyFrame);
		com.setSpeed(speed);
		com.setValueOffset(valueOffset);
		com.play(loop, offset);
	}
	public static void execute(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMMovableObjectAlphaPath com);
		com.setValueKeyFrame(null);
		com.play(false, 0.0f);
	}
}