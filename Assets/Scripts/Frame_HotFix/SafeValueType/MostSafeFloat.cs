using System;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameEditorUtility;

// 用来代替普通的float类型,防止内存被修改器修改
public struct MostSafeFloat : IEquatable<MostSafeFloat>
{
	private SafeFloat fwuegbw;			// 存储读写的数据
	private SafeFloat digwjengw;		// 存储校验数据
	public MostSafeFloat(float value)
	{
		fwuegbw = new(value);
		digwjengw = new(value);
	}
	public float get()
	{
		float curValue = fwuegbw.get();
		float checkValue = digwjengw.get();
		if (curValue != checkValue)
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFrameworkHotFix.onMemoryModified(7, 0, 0, 0, 0);
		}
		return curValue;
	}
	public void set(float value)
	{
		fwuegbw.set(value);
		digwjengw.set(value);
	}
	public bool Equals(MostSafeFloat other)
	{
		return fwuegbw.Equals(other.fwuegbw) && digwjengw.Equals(other.digwjengw);
	}
}