
// 用于渐变正交摄像机的正交大小
public class CmdGameCameraOrthoSize
{
	// 变化中的回调
	// 变化结束时的回调
	// 起始的大小
	// 终止的大小
	// 变化的持续时间,如果是循环的,则表示单次的时间
	// 时间起始偏移量
	// 所使用的关键帧曲线ID
	// 是否循环
	public static void execute(GameCamera camera, float startOrthoSize, float targetOrthoSize, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (camera == null)
		{
			return;	
		}
		camera.getOrAddComponent(out COMCameraOrthoSize com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startOrthoSize);
		com.setTarget(targetOrthoSize);
		com.play(keyframe, loop, onceLength, offset);
	}
	public static void execute(GameCamera camera, float targetOrthoSize)
	{
		if (camera == null)
		{
			return;
		}
		camera.getOrAddComponent(out COMCameraOrthoSize com);
		com.setStart(targetOrthoSize);
		com.setTarget(targetOrthoSize);
		com.play(0, false, 0.0f, 0.0f);
	}
}