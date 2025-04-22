using System;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 用来代替普通的long类型,防止内存被修改器修改
public struct MostSafeLong : IEquatable<MostSafeLong>
{
	private SafeLong gghwehg;		// 存储读写的数据
	private SafeLong guwhgnweg;		// 存储校验的数据
	public MostSafeLong(long value)
	{
		gghwehg = new(value);
		guwhgnweg = new(value);
	}
	public long get() 
	{
		long curValue = gghwehg.get();
		long checkValue = guwhgnweg.get();
		if (curValue != checkValue)
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFrameworkHotFix.onMemoryModified(9, 0, 0, 0, 0);
		}
		return curValue;
	}
	public void set(long value) 
	{
		gghwehg.set(value);
		guwhgnweg.set(value);
	}
	public bool Equals(MostSafeLong other)
	{
		return gghwehg.Equals(other.gghwehg) && guwhgnweg.Equals(other.guwhgnweg);
	}
}