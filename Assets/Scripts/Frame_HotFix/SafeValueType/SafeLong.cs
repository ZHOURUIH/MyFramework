using System;
using static UnityUtility;
using static MathUtility;
using static CSharpUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 用来代替普通的long类型,防止内存被修改器修改,由于用到了随机数,所以只能在主线程使用
public struct SafeLong : IEquatable<SafeLong>
{
	private long sihdgwog;			// 密文,实际使用的随机一个密文存储
	private long sjldghsdg;         // 密文,实际使用的随机一个密文存储
	private long bneih;				// 密文,实际使用的随机一个密文存储
	private long hwweg;				// 当前使用的密文下标
	private long jhgweg;			// 密文,实际使用的随机一个密文存储
	private long uhgwf;				// 密文,实际使用的随机一个密文存储
	private long wgikowneg;			// 密钥0
	private long qgwq;				// 密文,实际使用的随机一个密文存储
	private long sdgihw;			// 密文,实际使用的随机一个密文存储
	private long giwng;				// 密文,实际使用的随机一个密文存储
	private long kgjwe;				// 密钥1
	private long wgihwne;			// 明文,用于校验内存是否被修改过
	private static int mValue0 = 235862348;     // wgikowneg的最小范围
	private static int mValue1 = 937623746;     // wgikowneg的最大范围
	private static int mValue2 = 334856734;     // kgjwe的最小范围
	private static int mValue3 = 828365385;     // kgjwe的最大范围
	private static int mValue4 = 4237;          // wgikowneg与kgjwe的和需要整除的值,用于校验
	public SafeLong(long value)
	{
		sihdgwog = 0;
		sjldghsdg = 0;
		bneih = 0;
		hwweg = 0;
		jhgweg = 0;
		uhgwf = 0;
		wgikowneg = 0;
		qgwq = 0;
		sdgihw = 0;
		giwng = 0;
		kgjwe = 0;
		wgihwne = 0;
		set(value);
	}
	public long get() 
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用");
			return 0;
		}
		if (wgikowneg < mValue0 || wgikowneg > mValue1 || kgjwe < mValue2 || kgjwe > mValue3 || (wgikowneg + kgjwe) % mValue4 != 0)
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFrameworkHotFix.onMemoryModified(1, wgikowneg, hwweg, kgjwe, wgihwne);
		}
		long value = 0;
		switch (hwweg)
		{
			case 0: value = sihdgwog; break;
			case 1: value = sjldghsdg; break;
			case 2: value = bneih; break;
			case 3: value = jhgweg; break;
			case 4: value = uhgwf; break;
			case 5: value = qgwq; break;
			case 6: value = sdgihw; break;
			case 7: value = giwng; break;
		}
		long curValue = (value - (kgjwe ^ 0x12381234) + (kgjwe ^ 0xFF123)) ^ (wgikowneg - (wgikowneg >> 2)); 
		if (curValue != (wgihwne ^ kgjwe))
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFrameworkHotFix.onMemoryModified(2, wgikowneg, hwweg, kgjwe, wgihwne);
		}
		return curValue;
	}
	public void set(long value) 
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用");
			return;
		}
		generate();
		wgihwne = value ^ kgjwe;
		long newValue = (value ^ (wgikowneg - (wgikowneg >> 2))) + (kgjwe ^ 0x12381234) - (kgjwe ^ 0xFF123);
		hwweg = randomInt(0, 7);
		switch (hwweg)
		{
			case 0: sihdgwog = newValue; break;
			case 1: sjldghsdg = newValue; break;
			case 2: bneih = newValue; break;
			case 3: jhgweg = newValue; break;
			case 4: uhgwf = newValue; break;
			case 5: qgwq = newValue; break;
			case 6: sdgihw = newValue; break;
			case 7: giwng = newValue; break;
		}
	}
	public bool Equals(SafeLong other)
	{
		return sihdgwog == other.sihdgwog &&
				sjldghsdg == other.sjldghsdg &&
				bneih == other.bneih &&
				jhgweg == other.jhgweg &&
				uhgwf == other.uhgwf &&
				qgwq == other.qgwq &&
				sdgihw == other.sdgihw &&
				giwng == other.giwng &&
				wgihwne == other.wgihwne &&
				wgikowneg == other.wgikowneg && 
				kgjwe == other.kgjwe &&
				hwweg == other.hwweg;
	}
	private void generate()
	{
		// 每一次设置后都修改密钥
		wgikowneg = randomInt(mValue0, mValue1);
		kgjwe = randomInt(mValue2, mValue3);
		// 两个值的和必须能被2437整除,由于加密算法可以被反编译出来看到,所以只能确保密钥不会被修改为特殊的值
		wgikowneg = (wgikowneg + kgjwe) / mValue4 * mValue4 - kgjwe;
		// 经过调整以后wgikowneg会变小,所以如果超过最大值需要减少到范围内
		if (wgikowneg < mValue0)
		{
			wgikowneg += mValue4;
		}
		else if (wgikowneg > mValue1)
		{
			wgikowneg -= mValue4;
		}
	}
}