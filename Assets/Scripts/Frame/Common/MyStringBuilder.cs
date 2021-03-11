using System;
using System.Text;

public class MyStringBuilder : FrameBase
{
	protected StringBuilder mBuilder;
	public MyStringBuilder()
	{
		mBuilder = new StringBuilder();
	}
	public void Clear()
	{
		mBuilder.Clear();
	}
	public void Append(char value)
	{
		mBuilder.Append(value);
	}
	public void Append(byte value)
	{
		mBuilder.Append(value);
	}
	public void Append(bool value)
	{
		mBuilder.Append(value);
	}
	public void Append(short value)
	{
		mBuilder.Append(value);
	}
	public void Append(ushort value)
	{
		mBuilder.Append(value);
	}
	public void Append(int value)
	{
		mBuilder.Append(value);
	}
	public void Append(uint value)
	{
		mBuilder.Append(value);
	}
	public void Append(float value)
	{
		mBuilder.Append(value);
	}
	public void Append(double value)
	{
		mBuilder.Append(value);
	}
	public void Append(long value)
	{
		mBuilder.Append(value);
	}
	public void Append(ulong value)
	{
		mBuilder.Append(value);
	}
	public void Append(string value, int repearCount = 1)
	{
		for(int i = 0; i < repearCount; ++i)
		{
			mBuilder.Append(value);
		}
	}
	public void Append(string str0, string str1)
	{
		mBuilder.Append(str0).Append(str1);
	}
	public void Append(string str0, string str1, string str2)
	{
		mBuilder.Append(str0).Append(str1).Append(str2);
	}
	public void Append(string str0, string str1, string str2, string str3)
	{
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3);
	}
	public void Append(string str0, string str1, string str2, string str3, string str4)
	{
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4);
	}
	public void Append(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5);
	}
	public void Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6);
	}
	public void Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7);
	}
	public void Append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8);
	}
	public void Append(string value, int startIndex, int count)
	{
		mBuilder.Append(value, startIndex, count);
	}
	public void Insert(int index, string value)
	{
		mBuilder.Insert(index, value);
	}
	public void Remove(int startIndex, int length)
	{
		mBuilder.Remove(startIndex, length);
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
};