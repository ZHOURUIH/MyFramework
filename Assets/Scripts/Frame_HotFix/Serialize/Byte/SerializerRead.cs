using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BinaryUtility;
using static UnityUtility;
using static StringUtility;
using static FrameUtility;

// 只读缓冲区,用于解析二进制数组,按字节进行读取
public class SerializerRead : ClassObject
{
	protected byte[] mBuffer;			// 缓冲区
	protected int mBufferSize;			// 缓冲区大小
	protected int mIndex;				// 当前读下标
	protected bool mNeedCheck;			// 是否需要检查有没有足够的数据可以读,有需要提高效率时可以减少不必要的检查
	protected byte[] mFloatHelpBuffer;	// 辅助对象
	public SerializerRead()
	{
		mNeedCheck = true;
		mFloatHelpBuffer = new byte[4];
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffer = null;
		mBufferSize = 0;
		mIndex = 0;
		mNeedCheck = true;
		memset(mFloatHelpBuffer, (byte)0);
	}
	public void init(byte[] buffer, int bufferSize = -1, int index = 0)
	{
		mIndex = index;
		mBuffer = buffer;
		mBufferSize = bufferSize < 0 ? buffer.Length : bufferSize;
	}
	public bool readCustom<T>(out T seri) where T : Serializable, new()
	{
		return CLASS(out seri).read(this);
	}
	public bool readEnumByte<T>(out T value) where T : Enum
	{
		value = intToEnum<T, byte>(readByte(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumSByte<T>(out T value) where T : Enum
	{
		value = intToEnum<T, sbyte>(readSByte(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumUShort<T>(out T value) where T : Enum
	{
		value = intToEnum<T, ushort>(readUShort(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumShort<T>(out T value) where T : Enum
	{
		value = intToEnum<T, short>(readShort(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumUInt<T>(out T value) where T : Enum
	{
		value = intToEnum<T, uint>(readUInt(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumInt<T>(out T value) where T : Enum
	{
		value = intToEnum<T, int>(readInt(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumULong<T>(out T value) where T : Enum
	{
		value = intToEnum<T, ulong>(readULong(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumLong<T>(out T value) where T : Enum
	{
		value = intToEnum<T, long>(readLong(mBuffer, mBufferSize, ref mIndex, out bool success));
		return success;
	}
	public bool readEnumByteList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(byte)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out byte value);
			list.Add(intToEnum<T, byte>(value));
		}
		return true;
	}
	public bool readEnumSByteList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(sbyte)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out sbyte value);
			list.Add(intToEnum<T, sbyte>(value));
		}
		return true;
	}
	public bool readEnumShortList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(short)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out short value);
			list.Add(intToEnum<T, short>(value));
		}
		return true;
	}
	public bool readEnumUShortList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(ushort)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out ushort value);
			list.Add(intToEnum<T, ushort>(value));
		}
		return true;
	}
	public bool readEnumIntList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(int)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out int value);
			list.Add(intToEnum<T, int>(value));
		}
		return true;
	}
	public bool readEnumUIntList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(uint)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out uint value);
			list.Add(intToEnum<T, uint>(value));
		}
		return true;
	}
	public bool readEnumLongList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(long)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out long value);
			list.Add(intToEnum<T, long>(value));
		}
		return true;
	}
	public bool readEnumULongList<T>(List<T> list) where T : Enum
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(ulong)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out ulong value);
			list.Add(intToEnum<T, ulong>(value));
		}
		return true;
	}
	public bool read(out bool value)
	{
		value = readBool(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out byte value)
	{
		value = readByte(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out sbyte value)
	{
		value = readSByte(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out short value)
	{
		value = readShort(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out ushort value)
	{
		value = readUShort(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out int value)
	{
		value = readInt(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out uint value)
	{
		value = readUInt(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out long value)
	{
		value = readLong(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out ulong value)
	{
		value = readULong(mBuffer, mBufferSize, ref mIndex, out bool success);
		return success;
	}
	public bool read(out float value)
	{
		value = readFloat(mBuffer, mBufferSize, ref mIndex, out bool success, mFloatHelpBuffer);
		return success;
	}
	public bool read(out double value)
	{
		value = readDouble(mBuffer, mBufferSize, ref mIndex, out bool success, mFloatHelpBuffer);
		return success;
	}
	public bool read(out Vector2 value)
	{
		bool success0 = read(out value.x);
		bool success1 = read(out value.y);
		return success0 && success1;
	}
	public bool read(out Vector2Int value)
	{
		bool success0 = read(out int value0);
		bool success1 = read(out int value1);
		value = new(value0, value1);
		return success0 && success1;
	}
	public bool read(out Vector2UInt value)
	{
		bool success0 = read(out uint value0);
		bool success1 = read(out uint value1);
		value = new(value0, value1);
		return success0 && success1;
	}
	public bool read(out Vector2Short value)
	{
		bool success0 = read(out value.x);
		bool success1 = read(out value.y);
		return success0 && success1;
	}
	public bool read(out Vector2UShort value)
	{
		bool success0 = read(out value.x);
		bool success1 = read(out value.y);
		return success0 && success1;
	}
	public bool read(out Vector3 value)
	{
		bool success0 = read(out value.x);
		bool success1 = read(out value.y);
		bool success2 = read(out value.z);
		return success0 && success1 && success2;
	}
	public bool read(out Vector3Int value)
	{
		bool success0 = read(out int value0);
		bool success1 = read(out int value1);
		bool success2 = read(out int value2);
		value = new(value0, value1, value2);
		return success0 && success1 && success2;
	}
	public bool read(out Vector4 value)
	{
		bool success0 = read(out value.x);
		bool success1 = read(out value.y);
		bool success2 = read(out value.z);
		bool success3 = read(out value.w);
		return success0 && success1 && success2 && success3;
	}
	public bool readBuffer(byte[] buffer, int readLen, int bufferSize = -1)
	{
		return readBytes(mBuffer, ref mIndex, buffer, -1, bufferSize, readLen);
	}
	public bool readBuffer(byte[] buffer)
	{
		return readBytes(mBuffer, ref mIndex, buffer, -1, buffer.Length, buffer.Length);
	}
	public bool readCustomList<T>(List<T> list) where T : Serializable, new()
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		bool success = true;
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			success = readCustom(out T value) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<byte> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(byte)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out byte value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<sbyte> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(sbyte)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out sbyte value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<short> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(short)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out short value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<ushort> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(ushort)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out ushort value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<int> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(int)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out int value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<uint> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(uint)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out uint value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<long> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(long)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out long value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<ulong> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(ulong)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out ulong value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<float> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(float)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out float value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<double> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(double)))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out double value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<Vector2Int> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(int) * 2))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out Vector2Int value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<Vector2> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(float) * 2))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out Vector2 value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<Vector3Int> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(int) * 3))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out Vector3Int value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<Vector3> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(elementCount * sizeof(float) * 3))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			read(out Vector3 value);
			list.Add(value);
		}
		return true;
	}
	public bool readList(List<string> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			if (!readString(out string value))
			{
				return false;
			}
			list.Add(value);
		}
		return true;
	}
	// 返回值表示是否读取完全
	public bool readString(byte[] str, int strBufferSize)
	{
		// 先读入字符串长度
		if (!read(out int readLen))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(readLen))
		{
			return false;
		}
		// 如果存放字符串的空间大小不足以放入当前要读取的字符串,则只拷贝能容纳的长度,但是下标应该正常跳转
		if (strBufferSize <= readLen)
		{
			memcpy(str, mBuffer, 0, mIndex, strBufferSize - 1);
			mIndex += readLen;
			// 加上结束符
			str[strBufferSize - 1] = 0;
			return false;
		}
		else
		{
			memcpy(str, mBuffer, 0, mIndex, readLen);
			mIndex += readLen;
			// 加上结束符
			str[readLen] = 0;
			return true;
		}
	}
	// 返回值表示是否读取完全
	public bool readString(Span<byte> str, int strBufferSize)
	{
		// 先读入字符串长度
		if (!read(out int readLen))
		{
			return false;
		}
		if (mNeedCheck && !readCheck(readLen))
		{
			return false;
		}
		// 如果存放字符串的空间大小不足以放入当前要读取的字符串,则只拷贝能容纳的长度,但是下标应该正常跳转
		if (strBufferSize <= readLen)
		{
			memcpy(str, mBuffer, 0, mIndex, strBufferSize - 1);
			mIndex += readLen;
			// 加上结束符
			str[strBufferSize - 1] = 0;
			return false;
		}
		else
		{
			memcpy(str, mBuffer, 0, mIndex, readLen);
			mIndex += readLen;
			// 加上结束符
			str[readLen] = 0;
			return true;
		}
	}
	// 返回值表示是否读取完全
	public bool readString(out string value, Encoding encoding = null)
	{
		value = null;
		// 先读入字符串长度
		if (!read(out int readLen))
		{
			return false;
		}
		if (readLen == 0)
		{
			value = EMPTY;
			return true;
		}
		if (mNeedCheck && !readCheck(readLen))
		{
			return false;
		}
		value = bytesToString(mBuffer, mIndex, readLen, encoding);
		mIndex += readLen;
		return true;
	}
	public void skipIndex(int skip) 			{ mIndex += skip; }
	public void setIndex(int index) 			{ mIndex = index; }
	public void setNeedCheck(bool need)			{ mNeedCheck = need; }
	public byte[] getBuffer() 					{ return mBuffer; }
	public int getDataSize() 					{ return mBufferSize; }
	public int getIndex() 						{ return mIndex; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected bool readCheck(int readLen)
	{
		if (mBuffer == null)
		{
			logError("buffer is null! can not read");
			return false;
		}
		return mIndex + readLen <= mBufferSize;
	}
}