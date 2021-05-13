using System;
using System.Text;
using UnityEngine;

public class MyStringBuilder : FrameBase
{
	protected StringBuilder mBuilder;
	public MyStringBuilder()
	{
		// 初始默认128个字节的缓冲区,足以应对大部分的字符串拼接
		mBuilder = new StringBuilder(128);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mBuilder.Clear();
	}
	public MyStringBuilder Clear()
	{
		mBuilder.Clear();
		return this;
	}
	public int LastIndexOf(char value)
	{
		int length = mBuilder.Length;
		for (int i = length - 1; i >= 0; --i)
		{
			if (mBuilder[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
	public int IndexOf(char value)
	{
		int length = mBuilder.Length;
		for (int i = 0; i < length; ++i)
		{
			if (mBuilder[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
	public MyStringBuilder Append(char value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder Append(byte value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder Append(bool value)
	{
		mBuilder.Append(boolToString(value));
		return this;
	}
	public MyStringBuilder Append(short value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder Append(ushort value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder Append(int value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder Append(uint value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder Append(float value)
	{
		mBuilder.Append(FToS(value));
		return this;
	}
	public MyStringBuilder Append(double value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder Append(long value)
	{
		mBuilder.Append(LToS(value));
		return this;
	}
	public MyStringBuilder Append(ulong value)
	{
		mBuilder.Append(ULToS(value));
		return this;
	}
	public MyStringBuilder Append(Vector2 value)
	{
		mBuilder.Append(vector2ToString(value));
		return this;
	}
	public MyStringBuilder Append(Vector3 value)
	{
		mBuilder.Append(vector3ToString(value));
		return this;
	}
	public MyStringBuilder Append(Color32 value)
	{
		mBuilder.Append(value.ToString());
		return this;
	}
	public MyStringBuilder color(string color, int value)
	{
		return Append("<color=#", color, ">", IToS(value), "</color>");
	}
	public MyStringBuilder color(string color, int value0, string str0, int value1)
	{
		return Append("<color=#", color, ">", IToS(value0), str0, IToS(value1), "</color>");
	}
	public MyStringBuilder color(string color, string str0)
	{
		return Append("<color=#", color, ">", str0, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1)
	{
		return Append("<color=#", color, ">", str0, str1, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1, string str2)
	{
		return Append("<color=#", color, ">", str0, str1, str2, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1, string str2, string str3)
	{
		return Append("<color=#", color, ">", str0, str1, str2, str3, "</color>");
	}
	public MyStringBuilder color(string color, string str0, string str1, string str2, string str3, string str4)
	{
		return Append("<color=#", color, ">", str0, str1, str2, str3, str4, "</color>");
	}
	public MyStringBuilder Append(string value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder AppendRepeat(string value, int repearCount)
	{
		for (int i = 0; i < repearCount; ++i)
		{
			mBuilder.Append(value);
		}
		return this;
	}
	public MyStringBuilder Append(string str0, string str1)
	{
		if (str0 != null && str1 != null)
		{
			checkCapacity(str0.Length + str1.Length);
		}
		mBuilder.Append(str0).Append(str1);
		return this;
	}
	public MyStringBuilder Append(string str0, int value)
	{
		return Append(str0, IToS(value));
	}
	public MyStringBuilder Append(string str0, float value)
	{
		return Append(str0, FToS(value));
	}
	public MyStringBuilder Append(string str0, bool value)
	{
		return Append(str0, boolToString(value));
	}
	public MyStringBuilder Append(string str0, ulong value)
	{
		return Append(str0, ULToS(value));
	}
	public MyStringBuilder Append(string str0, Vector2 value)
	{
		return Append(str0, vector2ToString(value));
	}
	public MyStringBuilder Append(string str0, Vector3 value)
	{
		return Append(str0, vector3ToString(value));
	}
	public MyStringBuilder Append(string str0, Color32 value)
	{
		return Append(str0, value.ToString());
	}
	public MyStringBuilder Append(string str0, Type value)
	{
		if (value != null)
		{
			return Append(str0, value.ToString());
		}
		else
		{
			return Append(str0);
		}
	}
	public MyStringBuilder Append(string str0, string str1, string str2)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3, string str4)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8);
		return this;
	}
	public MyStringBuilder Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9);
		return this;
	}
	public MyStringBuilder Append(string value, int startIndex, int count)
	{
		mBuilder.Append(value, startIndex, count);
		return this;
	}
	public MyStringBuilder Insert(int index, string value)
	{
		mBuilder.Insert(index, value);
		return this;
	}
	public MyStringBuilder InsertFront(string str0, string str1)
	{
		checkCapacity(str0.Length + str1.Length + 1);
		mBuilder.Insert(0, str1);
		mBuilder.Insert(0, str0);
		return this;
	}
	public MyStringBuilder InsertFront(string str0, string str1, string str2)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + 1);
		mBuilder.Insert(0, str2);
		mBuilder.Insert(0, str1);
		mBuilder.Insert(0, str0);
		return this;
	}
	public MyStringBuilder InsertFront(string str0, string str1, string str2, string str3)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
		mBuilder.Insert(0, str3);
		mBuilder.Insert(0, str2);
		mBuilder.Insert(0, str1);
		mBuilder.Insert(0, str0);
		return this;
	}
	public MyStringBuilder Remove(int startIndex, int length = -1)
	{
		if (length < 0)
		{
			length = mBuilder.Length - startIndex;
		}
		mBuilder.Remove(startIndex, length);
		return this;
	}
	public MyStringBuilder Replace(char oldString, char newString)
	{
		mBuilder.Replace(oldString, newString);
		return this;
	}
	public MyStringBuilder Replace(string oldString, string newString)
	{
		mBuilder.Replace(oldString, newString);
		return this;
	}
	public override string ToString()
	{
		return mBuilder.ToString();
	}
	public string ToString(int startIndex, int length)
	{
		return mBuilder.ToString(startIndex, length);
	}
	public char this[int index] 
	{
		get { return mBuilder[index]; }
		set { mBuilder[index] = value; }
	}
	public int Length 
	{
		get { return mBuilder.Length; } 
		set { mBuilder.Length = value; } 
	}
	//----------------------------------------------------------------------------------------------------------------------------------------
	protected void checkCapacity(int increaseSize)
	{
		mBuilder.EnsureCapacity(mBuilder.Length + increaseSize);
	}
};