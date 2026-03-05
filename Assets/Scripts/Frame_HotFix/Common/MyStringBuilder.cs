using System;
using System.Text;
using System.Collections.Generic;
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
	public MyStringBuilder addIf(char value, bool condition)
	{
		if (condition)
		{
			mBuilder.Append(value);
		}
		return this;
	}
	public MyStringBuilder add(char value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder add(byte value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder add(bool value)
	{
		mBuilder.Append(boolToString(value));
		return this;
	}
	public MyStringBuilder add(short value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder add(ushort value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder add(int value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder add(uint value)
	{
		mBuilder.Append(IToS(value));
		return this;
	}
	public MyStringBuilder add(float value, int precision = 4)
	{
		mBuilder.Append(FToS(value, precision));
		return this;
	}
	public MyStringBuilder add(double value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder add(long value)
	{
		mBuilder.Append(LToS(value));
		return this;
	}
	public MyStringBuilder add(ulong value)
	{
		mBuilder.Append(ULToS(value));
		return this;
	}
	public MyStringBuilder add(Vector2 value, int precision = 4)
	{
		mBuilder.Append(StringUtility.V2ToS(value, precision));
		return this;
	}
	public MyStringBuilder add(Vector3 value, int precision = 4)
	{
		mBuilder.Append(StringUtility.V3ToS(value, precision));
		return this;
	}
	public MyStringBuilder add(Color32 value)
	{
		mBuilder.Append(value.ToString());
		return this;
	}
	public MyStringBuilder colorStringComma(string color, long value)
	{
		return add("<color=#", color, ">", LToSComma(value), "</color>");
	}
	public MyStringBuilder color(string color, long value)
	{
		return add("<color=#", color, ">", LToS(value), "</color>");
	}
	public MyStringBuilder colorStringComma(string color, int value)
	{
		return add("<color=#", color, ">", IToSComma(value), "</color>");
	}
	public MyStringBuilder colorString(string color, int value)
	{
		return add("<color=#", color, ">", IToS(value), "</color>");
	}
	public MyStringBuilder colorString(string color, int value0, string str0, int value1)
	{
		return add("<color=#", color, ">", IToS(value0), str0, IToS(value1), "</color>");
	}
	public MyStringBuilder colorString(string color, string str0)
	{
		return add("<color=#", color, ">", str0, "</color>");
	}
	public MyStringBuilder colorString(string color, string str0, string str1)
	{
		return add("<color=#", color, ">", str0, str1, "</color>");
	}
	public MyStringBuilder colorString(string color, string str0, string str1, string str2)
	{
		return add("<color=#", color, ">", str0, str1, str2, "</color>");
	}
	public MyStringBuilder colorString(string color, string str0, string str1, string str2, string str3)
	{
		return add("<color=#", color, ">", str0, str1, str2, str3, "</color>");
	}
	public MyStringBuilder colorString(string color, string str0, string str1, string str2, string str3, string str4)
	{
		return add("<color=#", color, ">", str0, str1, str2, str3, str4, "</color>");
	}
	public MyStringBuilder addLine(string value0)
	{
		return add(value0, "\r\n");
	}
	public MyStringBuilder addLine(string value0, string value1)
	{
		return add(value0, value1, "\r\n");
	}
	public MyStringBuilder addLine(string value0, string value1, string value2)
	{
		return add(value0, value1, value2, "\r\n");
	}
	public MyStringBuilder addLine(string value0, string value1, string value2, string value3)
	{
		return add(value0, value1, value2, value3, "\r\n");
	}
	public MyStringBuilder addLine(string value0, string value1, string value2, string value3, string value4)
	{
		return add(value0, value1, value2, value3, value4, "\r\n");
	}
	public MyStringBuilder addLine(string value0, string value1, string value2, string value3, string value4, string value5)
	{
		return add(value0, value1, value2, value3, value4, value5, "\r\n");
	}
	public MyStringBuilder addIf(string value, bool condition)
	{
		if (condition)
		{
			mBuilder.Append(value);
		}
		return this;
	}
	public MyStringBuilder add(string value)
	{
		mBuilder.Append(value);
		return this;
	}
	public MyStringBuilder addRepeat(string value, int repeatCount)
	{
		for (int i = 0; i < repeatCount; ++i)
		{
			mBuilder.Append(value);
		}
		return this;
	}
	public MyStringBuilder addIf(string str0, string str1, bool condition)
	{
		if (condition)
		{
			add(str0, str1);
		}
		return this;
	}
	public MyStringBuilder addIf(string str0, string str1, string str2, bool condition)
	{
		if (condition)
		{
			add(str0, str1, str2);
		}
		return this;
	}
	public MyStringBuilder add(string str0, string str1)
	{
		if (str0 != null && str1 != null)
		{
			checkCapacity(str0.Length + str1.Length);
		}
		mBuilder.Append(str0).Append(str1);
		return this;
	}
	public MyStringBuilder add(string str0, int value)
	{
		return add(str0, IToS(value));
	}
	public MyStringBuilder add(string str0, int value, string str1)
	{
		return add(str0, IToS(value), str1);
	}
	public MyStringBuilder add(string str0, float value, int precision = 4)
	{
		return add(str0, FToS(value, precision));
	}
	public MyStringBuilder add(string str0, float value, int precision, string str1)
	{
		return add(str0, FToS(value, precision), str1);
	}
	public MyStringBuilder add(string str0, float value, string str1)
	{
		return add(str0, FToS(value), str1);
	}
	public MyStringBuilder add(string str0, bool value)
	{
		return add(str0, boolToString(value));
	}
	public MyStringBuilder add(string str0, long value)
	{
		return add(str0, LToS(value));
	}
	public MyStringBuilder add(string str0, long value, string str1)
	{
		return add(str0, LToS(value), str1);
	}
	public MyStringBuilder add(string str0, ulong value)
	{
		return add(str0, ULToS(value));
	}
	public MyStringBuilder add(string str0, ulong value, string str1)
	{
		return add(str0, ULToS(value), str1);
	}
	public MyStringBuilder add(string str0, Vector2 value, int precision = 4)
	{
		return add(str0, StringUtility.V2ToS(value, precision));
	}
	public MyStringBuilder add(string str0, Vector3 value, int precision = 4)
	{
		return add(str0, StringUtility.V3ToS(value, precision));
	}
	public MyStringBuilder add(string str0, Color32 value)
	{
		return add(str0, value.ToString());
	}
	public MyStringBuilder add(string str0, Type value)
	{
		if (value != null)
		{
			return add(str0, value.ToString());
		}
		else
		{
			return add(str0);
		}
	}
	public MyStringBuilder add(string str0, string str1, string str2)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9);
		return this;
	}
	public MyStringBuilder add(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
	{
		checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + str10.Length + 1);
		mBuilder.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9).Append(str10);
		return this;
	}
	public MyStringBuilder add(string value, int startIndex, int count)
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
	public void replace(int begin, int end, string reStr)
	{
		remove(begin, end - begin);
		if (!reStr.isEmpty())
		{
			insert(begin, reStr);
		}
	}
	public void replaceAll(string key, string newWords)
	{
		int startPos = 0;
		while (true)
		{
			int pos = findFirstSubstr(key, startPos);
			if (pos < 0)
			{
				break;
			}
			replace(pos, pos + key.Length, newWords);
			startPos = pos + newWords.length();
		}
	}
	public void replaceAll(char key, char newWords)
	{
		int len = mBuilder.Length;
		for (int i = 0; i < len; ++i)
		{
			if (mBuilder[i] == key)
			{
				mBuilder[i] = newWords;
			}
		}
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public int findFirstSubstr(char pattern, int startPos = 0, bool sensitive = true)
	{
		if (!sensitive)
		{
			pattern = toLower(pattern);
		}
		int posFind = -1;
		int len = mBuilder.Length;
		for (int i = startPos; i < len; ++i)
		{
			if ((sensitive && mBuilder[i] == pattern) ||
				(!sensitive && toLower(mBuilder[i]) == pattern))
			{
				posFind = i;
				break;
			}
		}
		return posFind;
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public int findFirstSubstr(string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
	{
		if (mBuilder.Length < pattern.Length)
		{
			return -1;
		}
		int posFind = -1;
		int subLen = pattern.Length;
		int len = mBuilder.Length;
		for (int i = startPos; i < len; ++i)
		{
			if (len - i < subLen)
			{
				continue;
			}
			int j = 0;
			// 大小写敏感
			if (sensitive)
			{
				for (; j < subLen; ++j)
				{
					if (i + j >= 0 && i + j < len && mBuilder[i + j] != pattern[j])
					{
						break;
					}
				}
			}
			// 大小写不敏感,则需要都转换为小写
			else
			{
				for (; j < subLen; ++j)
				{
					if (i + j >= 0 && i + j < len && toLower(mBuilder[i + j]) != toLower(pattern[j]))
					{
						break;
					}
				}
			}
			if (j == subLen)
			{
				posFind = i;
				break;
			}
		}
		if (returnEndIndex && posFind >= 0)
		{
			posFind += subLen;
		}
		return posFind;
	}
	public void removeLast(char key)
	{
		int length = mBuilder.Length;
		for (int i = 0; i < length; ++i)
		{
			if (mBuilder[length - 1 - i] == key)
			{
				remove(length - 1 - i, 1);
				break;
			}
		}
	}
	public void removeLastComma()
	{
		removeLast(',');
	}
	// json
	public void jsonStartArray(string name = null, int preTableCount = 0, bool returnLine = false)
	{
		// 如果不是最外层的数组,则需要加上数组的名字
		if (!name.isEmpty())
		{
			addRepeat("\t", preTableCount);
			add("\"", name, "\"", ":");
			addIf("\r\n", returnLine);
		}
		addRepeat("\t", preTableCount);
		add('[');
		addIf("\r\n", returnLine);
	}
	public void jsonEndArray(int preTableCount = 0, bool returnLine = false)
	{
		if (endWith(','))
		{
			remove(mBuilder.Length - 1);
		}
		addRepeat("\t", preTableCount);
		add("],");
		addIf("\r\n", returnLine);
	}
	public void jsonStartStruct(string name = null, int preTableCount = 0, bool returnLine = false)
	{
		// 如果不是最外层的数组,则需要加上数组的名字
		if (!name.isEmpty())
		{
			addRepeat("\t", preTableCount);
			add("\"", name, "\"", ":");
			addIf("\r\n", returnLine);
		}
		// 如果不是最外层且非数组元素的结构体,则需要加上结构体的名字
		addRepeat("\t", preTableCount);
		add('}');
		addIf("\r\n", returnLine);
	}
	public void jsonEndStruct(bool keepComma= true, int preTableCount = 0, bool returnLine = false)
	{
		if (endWith(','))
		{
			remove(mBuilder.Length - 1);
		}
		addRepeat("\t", preTableCount);
		add('}');
		addIf(',', keepComma);
		addIf("\r\n", returnLine);
	}
	public void jsonAddPair(string name, string value, int preTableCount = 0, bool returnLine = false)
	{
		addRepeat("\t", preTableCount);
		// 如果是数组中的元素则不需要名字
		addIf("\"", name, "\": ", !name.isEmpty());
		add("\"", value, "\",");
		addIf("\r\n", returnLine);
	}
	public void jsonAddObject(string name, string value, int preTableCount = 0, bool returnLine = false)
	{
		addRepeat("\t", preTableCount);
		add("\"", name, "\": ", value, ",");
		addIf("\r\n", returnLine);
	}
	public void rightToLeft()
	{
		replace('\\', '/');
	}
	public void leftToRight()
	{
		replace('/', '\\');
	}
	public void V2IToS(Vector2Int value, int limitLength = 0)
	{
		add(IToS(value.x, limitLength), ",", IToS(value.y, limitLength));
	}
	public void V2ToS(Vector2 value, int precision = 4)
	{
		add(FToS(value.x, precision), ",", FToS(value.y, precision));
	}
	public void V3ToS(Vector3 value, int precision = 4)
	{
		add(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	public void byteToHEXString(byte value, bool upperOrLower = true)
	{
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		// 等效于int high = value / 16;
		// 等效于int low = value % 16;
		int high = value >> 4;
		int low = value & 15;
		if (high < 10)
		{
			add((char)('0' + high));
		}
		else
		{
			add(hexChar[high - 10]);
		}
		if (low < 10)
		{
			add((char)('0' + low));
		}
		else
		{
			add(hexChar[low - 10]);
		}
	}
	public MyStringBuilder setColor(string color)
	{
		if (mBuilder.Length == 0)
		{
			return this;
		}
		insertFront("<color=#", color, ">");
		add("</color>");
		return this;
	}
	public void addSprite(string spriteName, float width = 1.0f)
	{
		add("<quad width=").add(width).add(" sprite=").add(spriteName).add("/>");
	}
	public void line(string line, bool returnLine = true)
	{
		if (returnLine)
		{
			add(line, "\r\n");
		}
		else
		{
			add(line);
		}
	}
	public void addValueString(string str)
	{
		add("\"", str, "\",");
	}
	public void addValueVector2(Vector2 value)
	{
		V2ToS(value);
		add(',');
	}
	public void addValueVector2Int(Vector2Int value)
	{
		V2IToS(value);
		add(',');
	}
	public void addValueVector3(Vector3 value)
	{
		V3ToS(value);
		add(',');
	}
	public void addValueInt(int value)
	{
		add(IToS(value), ",");
	}
	public void addValueUInt(uint value)
	{
		add(IToS(value), ",");
	}
	public void addValueFloat(float value)
	{
		add(FToS(value), ",");
	}
	public void addValueFloats(List<float> floatArray)
	{
		addValueString(FsToS(floatArray));
	}
	public void addValueInts(List<int> intArray)
	{
		addValueString(IsToS(intArray));
	}
	public void addConditionString(string col, string str, string operate)
	{
		add(col, "=\"", str, "\"", operate);
	}
	public void addConditionInt(string col, int value, string operate)
	{
		add(col, " = ", IToS(value), operate);
	}
	public void addUpdateString(string col, string str)
	{
		add(col, " = \"", str, "\",");
	}
	public void addUpdateInt(string col, int value)
	{
		add(col, " = ", IToS(value), ",");
	}
	public void addUpdateInts(string col, List<int> intArray)
	{
		addUpdateString(col, IsToS(intArray));
	}
	public void addUpdateFloats(string col, List<float> floatArray)
	{
		addUpdateString(col, FsToS(floatArray));
	}
	public override string ToString()
	{
		return mBuilder.ToString();
	}
	public string toString(int startIndex, int length)
	{
		return mBuilder.ToString(startIndex, length);
	}
	public char this[int index]     // 根据下标获得字符
	{
		get { return mBuilder[index]; }
		set { mBuilder[index] = value; }
	}
	public int Length               // 当前字符串长度
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