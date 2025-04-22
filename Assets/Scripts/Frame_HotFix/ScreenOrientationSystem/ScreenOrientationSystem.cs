using static FrameBaseUtility;

public class ScreenOrientationSystem : FrameSystem
{
	protected ANDROID_ORIENTATION mAndroidOrientation = ANDROID_ORIENTATION.NONE;
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isEditor())
		{
			return;
		}
		// 由于屏幕旋转的设置可能会被安卓系统自动重置,所以强制设置为指定的旋转方式,此方法可能比较耗时
		if (isAndroid() && mAndroidOrientation >= 0)
		{
			AndroidPluginManager.getMainActivity().Call("setRequestedOrientation", (int)mAndroidOrientation);
		}
		else if (isIOS())
		{
			// ios还没有找到办法可以强制设置旋转方式
		}
	}
	public void setAndroidOrientation(ANDROID_ORIENTATION orientation) { mAndroidOrientation = orientation; }
}