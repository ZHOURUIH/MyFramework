using System;
using System.Text;
using UnityEngine;
using static StringUtility;
using static MathUtility;

// 自定已的StringBuilder,用于封装C#自己的StringBuilder,提高其效率
public class MyStringBuilder : ClassObject
{
	protected StringBuilder mBuilder = new(128);        // 内置的StringBuilder实例,初始默认128个字节的缓冲区,足以应对大部分的字符串拼接
	public override void resetProperty()
	{
		base.resetProperty();
		mBuilder.Clear();
	}
	public MyStringBuilder clear()
	{
		mBuilder.Clear();
		return this;
	}
	public bool endWith(char value)
	{
		return mBuilder.Length > 0 && mBuilder[^1] == value;
	}
	// 因为是从后往前找的,startIndex表示从后面的哪个下标开始查找
	public int lastIndexOf(char value, int startIndex = -1)
	{
		int length = mBuilder.Length;
		if (startIndex < 0)
		{
			startIndex = length - 1;
		}
		else
		{
			clampMax(ref startIndex, length - 1);
		}
		for (int i = startIndex; i >= 0; --i)
		{
			if (mBuilder[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
	public int indexOf(char value, int startIndex = 0)
	{
		int length = mBuilder.Length;
		for (int i = startIndex; i < length; ++i)
		{
			if (mBuilder[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
	// 命名方式是按照c++的std::endl
	public MyStringBuilder endl()
	{
		mBuilder.Append('\n');
		return this;
	}
	public MyStringBuilder append(char value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder append(byte value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder append(bool value)
	{
		mBuilder.Append(boolToString(value));
		return this;
	}
	public MyStringBuilder append(short value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder append(ushort value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder append(int value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder append(uint value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder append(float value, int precision = 4)
	{
		mBuilder.Append(FToS(value, precision));
		return this;
	}
	public MyStringBuilder append(double value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder append(long value)
	{
		mBuilder.Append(LToS(value));
		return this;
	}
	public MyStringBuilder append(ulong value)
	{
		mBuilder.Append(ULToS(value));
		return this;
	}
	public MyStringBuilder append(Vector2 value, int precision = 4)
	{
		mBuilder.Append(V2ToS(value, precision));
		return this;
	}
	public MyStringBuilder append(Vector3 value, int precision = 4)
	{
		mBuilder.Append(V3ToS(value, precision));
		return this;
	}
	public MyStringBuilder append(Color32 value)
	{
		mBuilder.Append(value.ToString());
		return this;
	}
	public MyStringBuilder color(string color, int value)
	{
		return append("<color=#", color, ">", IToS(value), "</color>");
	}
	public MyStringBuilder color(string color, int value0, string str0, int value1)
	{
		return append("<color=#", color, ">", IToS(value0), str0, IToS(value1), "</color>");
	}
	public MyStringBuilder color(string color, string str0)
	{
		return append("<color=#", color, ">", str0, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1)
	{
		return append("<color=#", color, ">", str0, str1, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1, string str2)
	{
		return append("<color=#", color, ">", str0, str1, str2, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1, string str2, string str3)
	{
		return append("<color=#", color, ">", str0, str1, str2, str3, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1, string str2, string str3, string str4)
	{
		return append("<color=#", color, ">", str0, str1, str2, str3, str4, "</color>");
	}
	public MyStringBuilder appendLine(string value0)
	{
		return append(value0, "\r\n");
	}
	public MyStringBuilder appendLine(string value0, string value1)
	{
		return append(value0, value1, "\r\n");
	}
	public MyStringBuilder appendLine(string value0, string value1, string value2)
	{
		return append(value0, value1, value2, "\r\n");
	}
	public MyStringBuilder appendLine(string value0, string value1, string value2, string value3)
	{
		return append(value0, value1, value2, value3, "\r\n");
	}
	public MyStringBuilder appendLine(string value0, string value1, string value2, string value3, string value4)
	{
		return append(value0, value1, value2, value3, value4, "\r\n");
	}
	public MyStringBuilder appendLine(string value0, string value1, string value2, string value3, string value4, string value5)
	{
		return append(value0, value1, value2, value3, value4, value5, "\r\n");
	}
	public MyStringBuilder append(string value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder appendRepeat(string value, int repearCount)
	{
		for (int i = 0; i < repearCount; ++i)
		{
			mBuilder.Append(value);
		}
		return this;
	}
	public MyStringBuilder append(string str0, string str1)
	{
		if (str0 != null && str1 != null)
		{
			checkCapacity(str0.Length + str1.Length);
		}
		mBuilder.Append(str0).Append(str1);
		return this;
	}
	public MyStringBuilder append(string str0, int value)
	{
		return append(str0, IToS(value));
	}
	public MyStringBuilder append(string str0, int value, string str1)
	{
		return append(str0, IToS(value), str1);
	}
	public MyStringBuilder append(string str0, float value, int precision = 4)
	{
		return append(str0, FToS(value, precision));
	}
	public MyStringBuilder append(string str0, float value, int precision, string str1)
	{
		return append(str0, FToS(value, precision), str1);
	}
	public MyStringBuilder append(string str0, float value, string str1)
	{
		return append(str0, FToS(value), str1);
	}
	public MyStringBuilder append(string str0, bool value)
	{
		return append(str0, boolToString(value));
	}
	public MyStringBuilder append(string str0, long value)
	{
		return append(str0, LToS(value));
	}
	public MyStringBuilder append(string str0, long value, string str1)
	{
		return append(str0, LToS(value), str1);
	}
	public MyStringBuilder append(string str0, ulong value)
	{
		return append(str0, ULToS(value));
	}
	public MyStringBuilder append(string str0, ulong value, string str1)
	{
		return append(str0, ULToS(value), str1);
	}
	public MyStringBuilder append(string str0, Vector2 value, int precision = 4)
	{
		return append(str0, V2ToS(value, precision));
	}
	public MyStringBuilder append(string str0, Vector3 value, int precision = 4)
	{
		return append(str0, V3ToS(value, precision));
	}
	public MyStringBuilder append(string str0, Color32 value)
	{
		return append(str0, value.ToString());
	}
	public MyStringBuilder append(string str0, Type value)
	{
		if (value != null)
		{
			return append(str0, value.ToString());
		}
		else
		{
			return append(str0);
		}
	}
	public MyStringBuilder append(string str0, string str1, string str2)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9);
		return this;
	}
	public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + str10.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9).Append(str10);
		return this;
	}
	public MyStringBuilder append(string value, int startIndex, int count)
	{
		mBuilder.Append(value, startIndex, count);
		return this;
	}
	public MyStringBuilder insert(int index, string value)
	{
		mBuilder.Insert(index, value);
		return this;
	}
	public MyStringBuilder insert(int index, char value)
	{
		mBuilder.Insert(index, value);
		return this;
	}
	public MyStringBuilder insertFront(string str0, string str1)
	{
		checkCapacity(str0.Length + str1.Length + 1);
		mBuilder.Insert(0, str1);
		mBuilder.Insert(0, str0);
		return this;
	}
	public MyStringBuilder insertFront(string str0, string str1, string str2)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + 1);
		mBuilder.Insert(0, str2);
		mBuilder.Insert(0, str1);
		mBuilder.Insert(0, str0);
		return this;
	}
	public MyStringBuilder insertFront(string str0, string str1, string str2, string str3)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
		mBuilder.Insert(0, str3);
		mBuilder.Insert(0, str2);
		mBuilder.Insert(0, str1);
		mBuilder.Insert(0, str0);
		return this;
	}
	public MyStringBuilder remove(int startIndex, int length = -1)
	{
		if (length < 0)
		{
			length = mBuilder.Length - startIndex;
		}
		mBuilder.Remove(startIndex, length);
		return this;
	}
	public MyStringBuilder replace(char oldString, char newString)
	{
		mBuilder.Replace(oldString, newString);
		return this;
	}
	public MyStringBuilder replace(string oldString, string newString)
	{
		mBuilder.Replace(oldString, newString);
		return this;
	}
	public override string ToString()
	{
		return mBuilder.ToString();
	}
	public string toString(int startIndex, int length)
	{
		return mBuilder.ToString(startIndex, length);
	}
	public char this[int index]		// 根据下标获得字符
	{
		get { return mBuilder[index]; }
		set { mBuilder[index] = value; }
	}
	public int Length				// 当前字符串长度
	{
		get { return mBuilder.Length; } 
		set { mBuilder.Length = value; } 
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkCapacity(int increaseSize)
	{
		mBuilder.EnsureCapacity(mBuilder.Length + increaseSize);
	}
};