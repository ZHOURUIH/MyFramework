
// 渐变一个物体的透明度
public class CmdMovableObjectAlpha
{
	// 渐变中回调
	// 渐变结束时回调
	// 起始透明度
	// 目标透明度
	// 单次需要的时间
	// 起始时间偏移
	// 所使用的关键帧曲线ID
	// 是否循环
	public static void execute(MovableObject obj, float startAlpha, float targetAlpha, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMMovableObjectAlpha com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startAlpha);
		com.setTarget(targetAlpha);
		com.play(keyframe, loop, onceLength, offset);
	}
	public static void execute(MovableObject obj, float targetAlpha)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMMovableObjectAlpha com);
		com.setStart(targetAlpha);
		com.setTarget(targetAlpha);
		com.play(0, false, 0.0f, 0.0f);
	}
}