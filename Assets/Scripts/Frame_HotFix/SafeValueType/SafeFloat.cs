using System;
using static UnityUtility;
using static MathUtility;
using static CSharpUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 用来代替普通的float类型,防止内存被修改器修改,由于用到了随机数,所以只能在主线程使用
public struct SafeFloat : IEquatable<SafeFloat>
{
	private int erhgu;          // 密文,实际使用的随机一个密文存储
	private int wgjb;			// 密文,实际使用的随机一个密文存储
	private int gjkwqbg;		// 密文,实际使用的随机一个密文存储
	private int khsgh;			// 密文当前的下标
	private int qgoqjg;			// 密钥1
	private int gjkqwbeg;		// 密文,实际使用的随机一个密文存储
	private int geqbw;			// 密文,实际使用的随机一个密文存储
	private int gjwkehgwe;      // 密文,实际使用的随机一个密文存储
	private int qwfb;			// 密钥0
	private int huihg;          // 密文,实际使用的随机一个密文存储
	private int asgihfasg;		// 明文,用于校验内存是否被修改过
	private int gjqweg;         // 密文,实际使用的随机一个密文存储
	private static int mValue0 = 56859348;  // qwfb的最小范围
	private static int mValue1 = 91239999;  // qwfb的最大范围
	private static int mValue3 = 33776454;  // qgoqjg的最小范围
	private static int mValue4 = 81239999;  // qgoqjg的最大范围
	private static int mValue5 = 737;       // qgoqjg与qwfb的和需要整除的值,用于校验
	public SafeFloat(float value)
	{
		erhgu = 0;
		wgjb = 0;
		gjkwqbg = 0;
		khsgh = 0;
		qgoqjg = 0;
		gjkqwbeg = 0;
		geqbw = 0;
		gjwkehgwe = 0;
		qwfb = 0;
		huihg = 0;
		asgihfasg = 0;
		gjqweg = 0;
		set(value);
	}
	public float get()  
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用");
			return 0;
		}
		if (qwfb < mValue0 || qwfb > mValue1 || qgoqjg < mValue3 || qgoqjg > mValue4 || (qgoqjg + qwfb) % mValue5 != 0)
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFrameworkHotFix.onMemoryModified(5, qgoqjg, qwfb, asgihfasg, khsgh);
		}
		int value = 0;
		switch (khsgh)
		{
			case 0: value = erhgu; break;
			case 1: value = wgjb; break;
			case 2: value = gjwkehgwe; break;
			case 3: value = gjkwqbg; break;
			case 4: value = gjkqwbeg; break;
			case 5: value = gjqweg; break;
			case 6: value = geqbw; break;
			case 7: value = huihg; break;
		}
		float curValue = ((value - (qgoqjg ^ 0x1238) + (qgoqjg ^ 0xFF123)) ^ (qwfb - (qwfb >> 2))) * 0.001f;  
		if (!isFloatEqual(curValue, (asgihfasg ^ qgoqjg) * 0.0001f, 0.01f))
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFrameworkHotFix.onMemoryModified(6, qgoqjg, qwfb, asgihfasg, khsgh);
		}
		return curValue;
	}
	public void set(float value)  
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用");
			return;
		}
		generate();
		asgihfasg = (int)(value * 10000) ^ qgoqjg;
		khsgh = randomInt(0, 7);
		int newValue = (round(value * 1000) ^ (qwfb - (qwfb >> 2))) + (qgoqjg ^ 0x1238) - (qgoqjg ^ 0xFF123);
		switch (khsgh)
		{
			case 0: erhgu = newValue; break;
			case 1: wgjb = newValue; break;
			case 2: gjwkehgwe = newValue; break;
			case 3: gjkwqbg = newValue; break;
			case 4: gjkqwbeg = newValue; break;
			case 5: gjqweg = newValue; break;
			case 6: geqbw = newValue; break;
			case 7: huihg = newValue; break;
		}
	}
	public bool Equals(SafeFloat other)
	{
		return erhgu == other.erhgu &&
			wgjb == other.wgjb &&
			gjwkehgwe == other.gjwkehgwe &&
			gjkwqbg == other.gjkwqbg &&
			gjkqwbeg == other.gjkqwbeg &&
			gjqweg == other.gjqweg &&
			geqbw == other.geqbw &&
			huihg == other.huihg &&
			qwfb == other.qwfb && 
			asgihfasg == other.asgihfasg && 
			qgoqjg == other.qgoqjg &&
			khsgh == other.khsgh;
	}
	private void generate()
	{
		// 每一次设置后都修改密钥
		qwfb = randomInt(mValue0, mValue1);
		qgoqjg = randomInt(mValue3, mValue4);
		// 两个值的和必须能被737整除
		qwfb = (qgoqjg + qwfb) / mValue5 * mValue5 - qgoqjg;
		// 经过调整以后qwfb会变小,所以如果超过最大值需要减少到范围内
		if (qwfb < mValue0)
		{
			qwfb += mValue5;
		}
		else if (qwfb > mValue1)
		{
			qwfb -= mValue5;
		}
	}
}