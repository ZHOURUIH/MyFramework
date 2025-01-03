using System;
using static UnityUtility;
using static MathUtility;
using static CSharpUtility;
using static FrameBase;
using static FrameEditorUtility;

// 用来代替普通的int类型,防止内存被修改器修改,由于用到了随机数,所以只能在主线程使用
public struct SafeInt : IEquatable<SafeInt>
{
	private int sidhgsg;        // 密文,实际使用的随机一个密文存储
	private int isdhy34;        // 密文,实际使用的随机一个密文存储
	private int hibsd;          // 密文,实际使用的随机一个密文存储
	private int gikowjeg;		// 明文,用于校验内存是否被修改过
	private int sgkn;           // 密文,实际使用的随机一个密文存储
	private int jgo2;           // 密文,实际使用的随机一个密文存储
	private int hgwikg;         // 密文,实际使用的随机一个密文存储
	private int wghwe;			// 当前使用的密文下标
	private int gnwe;           // 密文,实际使用的随机一个密文存储
	private int woghjwe;		// 密钥0
	private int gwijhge;        // 密文,实际使用的随机一个密文存储
	private int gikoweg;		// 密钥1
	private static int mValue0 = 135386587;     // gikoweg的最小范围
	private static int mValue1 = 335456345;     // gikoweg的最大范围
	private static int mValue2 = 212346587;     // woghjwe的最小范围
	private static int mValue3 = 311156345;     // woghjwe的最大范围
	private static int mValue5 = 437;           // woghjwe与gikoweg的和需要整除的值,用于校验
	public SafeInt(int value)
	{
		sidhgsg = 0;
		isdhy34 = 0;
		hibsd = 0;
		gikowjeg = 0;
		sgkn = 0;
		jgo2 = 0;
		hgwikg = 0;
		wghwe = 0;
		gnwe = 0;
		woghjwe = 0;
		gwijhge = 0;
		gikoweg = 0;
		set(value);
	}
	public int get() 
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用");
			return 0;
		}
		if (gikoweg < mValue0 || gikoweg > mValue1 || woghjwe < mValue2 || woghjwe > mValue3 || (woghjwe + gikoweg) % mValue5 != 0)
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFramework.onMemoryModified(3, wghwe, woghjwe, gikoweg, gikowjeg);
		}
		int value = 0;
		switch (wghwe)
		{
			case 0: value = sidhgsg; break;
			case 1: value = isdhy34; break;
			case 2: value = jgo2; break;
			case 3: value = hibsd; break;
			case 4: value = sgkn; break;
			case 5: value = gnwe; break;
			case 6: value = hgwikg; break;
			case 7: value = gwijhge; break;
		}
		int curValue = (value - (woghjwe ^ 0x1238) + (woghjwe ^ 0xFF123)) ^ (gikoweg - (gikoweg >> 2)); 
		if (curValue != (gikowjeg ^ woghjwe))
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFramework.onMemoryModified(4, wghwe, woghjwe, gikoweg, gikowjeg);
		}
		return curValue;
	}
	public void set(int value) 
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用");
			return;
		}
		generate();
		gikowjeg = value ^ woghjwe;
		int newValue = (value ^ (gikoweg - (gikoweg >> 2))) + (woghjwe ^ 0x1238) - (woghjwe ^ 0xFF123);
		wghwe = randomInt(0, 7);
		switch (wghwe)
		{
			case 0: sidhgsg = newValue; break;
			case 1: isdhy34 = newValue; break;
			case 2: jgo2 = newValue; break;
			case 3: hibsd = newValue; break;
			case 4: sgkn = newValue; break;
			case 5: gnwe = newValue; break;
			case 6: hgwikg = newValue; break;
			case 7: gwijhge = newValue; break;
		}
	}
	public bool Equals(SafeInt other)
	{
		return sidhgsg == other.sidhgsg &&
				isdhy34 == other.isdhy34 &&
				jgo2 == other.jgo2 &&
				hibsd == other.hibsd &&
				sgkn == other.sgkn &&
				gnwe == other.gnwe &&
				hgwikg == other.hgwikg &&
				gwijhge == other.gwijhge &&
				gikowjeg == other.gikowjeg && 
				gikoweg == other.gikoweg && 
				woghjwe == other.woghjwe &&
				wghwe == other.wghwe;
	}
	private void generate()
	{
		// 每一次设置后都修改密钥
		gikoweg = randomInt(mValue0, mValue1);
		woghjwe = randomInt(mValue2, mValue3);
		// 两个值的和必须能被437整除
		gikoweg = (gikoweg + woghjwe) / mValue5 * mValue5 - woghjwe;
		// 经过调整以后gikoweg会变小,所以如果超过最大值需要减少到范围内
		if (gikoweg < mValue0)
		{
			gikoweg += mValue5;
		}
		else if (gikoweg > mValue1)
		{
			gikoweg -= mValue5;
		}
	}
}