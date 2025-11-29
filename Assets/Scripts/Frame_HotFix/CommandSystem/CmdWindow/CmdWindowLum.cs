using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 渐变一个窗口的亮度,需要此窗口有LumOffset的shader
public class CmdWindowLum
{
	// 变化中回调
	// 变化完成时回调
	// 单次所需时间
	// 目标亮度
	// 起始亮度
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(myUGUIObject obj, float onceLength, float targetLum, float startLum, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			!isFloatZero(onceLength) && 
			!obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + obj.getName());
		}
		obj.getOrAddComponent(out COMWindowLum com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startLum);
		com.setTarget(targetLum);
		com.play(keyframe, loop, onceLength, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(myUGUIObject obj, float targetLum)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMWindowLum com);
		if (com == null || !com.isActive())
		{
			if (obj is IShaderWindow shaderWindow &&
				shaderWindow.getWindowShader() is WindowShaderLumOffset lumOffset)
			{
				lumOffset.setLumOffset(targetLum);
			}
			return;
		}
		com.setStart(targetLum);
		com.setTarget(targetLum);
		com.play(0, false, 0.0f, 0.0f);
	}
}