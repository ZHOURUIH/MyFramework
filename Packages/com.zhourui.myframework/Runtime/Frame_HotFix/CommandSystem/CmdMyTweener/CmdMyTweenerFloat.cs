
// 渐变一个浮点数
public class CmdMyTweenerFloat
{
	// 变化中回调
	// 变化完成时回调
	// 起始值
	// 目标值
	// 单次所需时间
	// 起始时间偏移
	// 所使用的关键帧曲线ID
	// 是否循环
	public static void execute(ComponentOwner obj, float start, float target, float onceLength, float offset, int keyframeID, bool loop, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallBack)
	{
		obj.getOrAddComponent(out COMMyTweenerFloat com);
		com.setDoingCallback(doingCallBack);
		com.setDoneCallback(doneCallBack);
		com.setActive(true);
		com.setStart(start);
		com.setTarget(target);
		com.play(keyframeID, loop, onceLength, offset);
	}
}