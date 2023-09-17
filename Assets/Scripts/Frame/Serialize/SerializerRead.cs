using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BinaryUtility;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;

// 只读缓冲区,用于解析二进制数组,按字节进行读取
public class SerializerRead : ClassObject
{
	protected byte[] mBuffer;			// 缓冲区
	protected int mBufferSize;			// 缓冲区大小
	protected int mIndex;				// 当前读下标
	protected bool mNeedCheck = true;	// 是否需要检查有没有足够的数据可以读,有需要提高效率时可以减少不必要的检查
	protected byte[] mFloatHelpBuffer;
	public SerializerRead()
	{
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
	public bool readCustom<T>(out T seri, Type type = null) where T : Serializable
	{
		if (type == null)
		{
			type = Typeof<T>();
		}
		if (type == null)
		{
			logError("热更工程必须手动传一个类型参数,才能正常创建对象");
		}
		seri = FrameUtility.CLASS(type) as T;
		return seri.read(this);
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
		value = new Vector3Int(value0, value1, value2);
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
	// 为了兼容ILRuntime,需要传一个type进来
	public bool readCustomList<T>(List<T> list, Type type = null) where T : Serializable
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
			success = readCustom(out T value, type) && success;
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
		if (mIndex + readLen > mBufferSize)
		{
			logError("read buffer out of range! cur index : " + mIndex + ", buffer size : " + mBufferSize + ", read length : " + readLen);
			return false;
		}
		return true;
	}
}