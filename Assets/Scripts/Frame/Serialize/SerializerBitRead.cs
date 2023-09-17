using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BinaryUtility;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;
using static MathUtility;
using static FrameUtility;

// 只读缓冲区,用于解析二进制数组,按位进行读取
public class SerializerBitRead : ClassObject
{
	protected byte[] mBuffer;   // 缓冲区
	protected int mBufferSize;  // 缓冲区大小
	protected int mBitIndex;    // 当前读下标
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffer = null;
		mBufferSize = 0;
		mBitIndex = 0;
	}
	public void init(byte[] buffer, int bufferSize = -1, int bitIndex = 0)
	{
		mBuffer = buffer;
		mBufferSize = bufferSize < 0 ? buffer.Length : bufferSize;
		mBitIndex = bitIndex;
	}
	public bool read(out bool value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out byte value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out sbyte value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out short value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out ushort value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out int value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out uint value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out long value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out ulong value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out float value, int precision = 3)
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out int intValue);
		value = (float)intValue / pow10(precision);
		return success;
	}
	public bool read(out double value, int precision = 4)
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out long intValue);
		value = (double)intValue / pow10Long(precision);
		return success;
	}
	public bool read(out Vector2 value, int precision = 3)
	{
		bool success0 = read(out value.x, precision);
		bool success1 = read(out value.y, precision);
		return success0 && success1;
	}
	public bool read(out Vector2Int value)
	{
		bool success0 = read(out int value0);
		bool success1 = read(out int value1);
		value = new Vector2Int(value0, value1);
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
	public bool read(out Vector3 value, int precision = 3)
	{
		bool success0 = read(out value.x, precision);
		bool success1 = read(out value.y, precision);
		bool success2 = read(out value.z, precision);
		return success0 && success1 && success2;
	}
	public bool read(out Vector3Int value)
	{
		bool success0 = read(out int value0);
		bool success1 = read(out int value1);
		bool success2 = read(out int value2);
		value = new Vector3Int(value0, value1, value2);
		return success0 && success1 && success2;
	}
	public bool read(out Vector4 value, int precision = 3)
	{
		bool success0 = read(out value.x, precision);
		bool success1 = read(out value.y, precision);
		bool success2 = read(out value.z, precision);
		bool success3 = read(out value.w, precision);
		return success0 && success1 && success2 && success3;
	}
	// 为了兼容ILRuntime,需要传一个type进来
	public bool readCustomList<T>(List<T> list, Type type = null) where T : SerializableBit
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (type == null)
		{
			type = Typeof<T>();
		}
		if (type == null)
		{
			logError("热更工程必须手动传一个类型参数,才能正常创建对象, " + typeof(T));
		}
		bool success = true;
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			T value = CLASS(type) as T;
			success = value.read(this) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<byte> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<sbyte> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<short> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<ushort> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<int> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<uint> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<long> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<ulong> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<float> list, int precision = 3)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list, precision);
	}
	public bool readList(List<double> list, int precision = 4)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list, precision);
	}
	public bool readList(List<Vector2> list, int precision = 3)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector2 value, precision) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector2UShort> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector2UShort value) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector2Int> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector2Int value) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector3> list, int precision = 3)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector3 value, precision) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector4> list, int precision = 3)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector4 value, precision) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<string> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
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
		// 如果字符串长度为0,则不做任何改变
		if (readLen == 0)
		{
			return true;
		}
		bool result = readBuffer(str, readLen);
		str[getMin(readLen, strBufferSize) - 1] = 0;
		return result;
	}
	public bool readBuffer(byte[] buffer, int readLength)
	{
		if (readLength == 0)
		{
			return true;
		}
		int byteIndex = getReadByteCount();
		mBitIndex = (byteIndex + readLength) << 3;
		// 如果存放数据的空间大小不足以放入当前要读取的数据,则只拷贝能容纳的长度,但是下标应该正常跳转
		memcpy(buffer, mBuffer, 0, byteIndex, getMin(buffer.Length, readLength));
		return readLength <= buffer.Length;
	}
	public void skipToByteEnd() { mBitIndex = bitCountToByteCount(mBitIndex) << 3; }
	// 返回值表示是否读取完全
	public bool readString(out string value, Encoding encoding = null)
	{
		value = null;
		// 先读入字符串长度
		if (!read(out int readLen))
		{
			return false;
		}
		// 如果字符串长度为0,则不做任何改变
		if (readLen == 0)
		{
			value = EMPTY;
			return true;
		}

		int byteIndex = getReadByteCount();
		mBitIndex = (byteIndex + readLen) << 3;
		value = bytesToString(mBuffer, byteIndex, readLen, encoding);
		return true;
	}
	public byte[] getBuffer() { return mBuffer; }
	public int getBufferSize() { return mBufferSize; }
	public int getBitIndex() { return mBitIndex; }
	// 获取已读取的字节数,最后一个字节不一定已经读完全部位
	public int getReadByteCount() { return bitCountToByteCount(mBitIndex); }
}