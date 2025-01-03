using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static BinaryUtility;
using static MathUtility;

// 只写缓冲区,用于生成二进制数据流,按位进行写入
public class SerializerBitWrite : ClassObject
{
	protected byte[] mBuffer;	// 缓冲区
	protected int mBitIndex;	// 当前写下标
	public override void resetProperty()
	{
		base.resetProperty();
		// mBuffer不重置,保证可以尽可能复用数组
		// mBuffer = null;
		if (mBuffer != null)
		{
			memset(mBuffer, (byte)0);
		}
		mBitIndex = 0;
	}
	public void write(bool value)
	{
		writeCheck(sizeof(bool));
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<byte> values)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多7bit长度位,所以需要多扩容1个字节
		writeCheck(sizeof(byte) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(byte value)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多7bit长度位,所以需要多扩容1个字节
		writeCheck(sizeof(byte) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<sbyte> values)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多8bit,7bit长度位+1bit符号位,所以需要多扩容1个字节
		writeCheck(sizeof(sbyte) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(sbyte value)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多8bit,7bit长度位+1bit符号位,所以需要多扩容1个字节
		writeCheck(sizeof(sbyte) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<short> values)
	{
		writeCheck(sizeof(short) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(short value)
	{
		writeCheck(sizeof(short) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<ushort> values)
	{
		writeCheck(sizeof(ushort) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(ushort value)
	{
		writeCheck(sizeof(ushort) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<int> values)
	{
		writeCheck(sizeof(int) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(int value)
	{
		writeCheck(sizeof(int) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<uint> values)
	{
		writeCheck(sizeof(uint) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(uint value)
	{
		writeCheck(sizeof(uint) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<long> values)
	{
		writeCheck(sizeof(long) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(long value)
	{
		writeCheck(sizeof(long) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<ulong> values)
	{
		writeCheck(sizeof(ulong) + 1);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(ulong value)
	{
		writeCheck(sizeof(ulong) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<float> values, int precision = 3)
	{
		int powValue = pow10(precision);
		int count = values.Length;
		Span<int> ints = stackalloc int[count];
		for (int i = 0; i < count; ++i)
		{
			ints[i] = round(values[i] * powValue);
		}
		write(ints);
	}
	public void write(float value, int precision = 3)
	{
		write(round(value * pow10(precision)));
	}
	public void write(Span<double> values, int precision = 4)
	{
		long powValue = pow10Long(precision);
		int count = values.Length;
		Span<long> longs = stackalloc long[count];
		for (int i = 0; i < count; ++i)
		{
			longs[i] = round(values[i] * powValue);
		}
		write(longs);
	}
	public void write(double value, int precision = 4)
	{
		write(round(value * pow10Long(precision)));
	}
	public void write(Vector2 value, int precision = 3)
	{
		int powValue = pow10(precision);
		write(stackalloc int[2] { round(value.x * powValue), round(value.y * powValue) });
	}
	public void write(Vector2UShort value)
	{
		write(stackalloc ushort[2] { value.x, value.y });
	}
	public void write(Vector2Short value)
	{
		write(stackalloc short[2] { value.x, value.y });
	}
	public void write(Vector2Int value)
	{
		write(stackalloc int[2] { value.x, value.y });
	}
	public void write(Vector2IntMy value)
	{
		write(stackalloc int[2] { value.x, value.y });
	}
	public void write(Vector2UInt value)
	{
		write(stackalloc uint[2] { value.x, value.y });
	}
	public void write(Vector3 value, int precision = 3)
	{
		int powValue = pow10(precision);
		write(stackalloc int[3] { round(value.x * powValue), round(value.y * powValue), round(value.z * powValue) });
	}
	public void write(Vector4 value, int precision = 3)
	{
		int powValue = pow10(precision);
		write(stackalloc int[4] { round(value.x * powValue), round(value.y * powValue), round(value.z * powValue), round(value.w * powValue) });
	}
	public void writeBuffer(byte[] buffer, int dataSize)
	{
		if (buffer.isEmpty() || dataSize == 0)
		{
			return;
		}
		writeCheck(dataSize + 1);
		wrtiteBufferBit(mBuffer, mBuffer.Length, ref mBitIndex, buffer, dataSize);
	}
	// 将最后一个字节未填充数据的位填充为0,并将位下标移动到字节末尾
	public void fillZeroToByteEnd()
	{
		BinaryUtility.fillZeroToByteEnd(mBuffer, ref mBitIndex);
	}
	public void writeString(string str, Encoding encoding = null)
	{
		byte[] bytes = stringToBytes(str, encoding);
		int strLen = bytes.count();
		// 先写入字符串长度,如果长度为0,则不做任何改变
		write(strLen);
		if (strLen == 0)
		{
			return;
		}
		writeBuffer(bytes, strLen);
	}
	public void writeCustomList<T>(List<T> list) where T : SerializableBit
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			list[i].write(this);
		}
	}
	public void writeList(List<byte> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(byte) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<sbyte> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(sbyte) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<short> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(short) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<ushort> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(ushort) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<int> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(int) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<uint> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(uint) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<long> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(long) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<ulong> list)
	{
		writeCheck(sizeof(int) + 1 + sizeof(ulong) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<float> list, int precision = 3)
	{
		writeCheck(sizeof(int) + 1 + sizeof(float) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, precision);
	}
	public void writeList(List<double> list, int precision = 4)
	{
		writeCheck(sizeof(int) + 1 + sizeof(double) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, precision);
	}
	public void writeList(List<Vector2> list, int precision = 3)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], precision);
		}
	}
	public void writeList(List<Vector2UShort> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<Vector2UInt> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<Vector2Int> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<Vector3> list, int precision = 3)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], precision);
		}
	}
	public void writeList(List<Vector4> list, int precision = 3)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], precision);
		}
	}
	public void writeList(List<string> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			writeString(list[i]);
		}
	}
	public byte[] getBuffer()					{ return mBuffer; }
	public int getBitCount()					{ return mBitIndex; }
	public int getByteCount()					{ return bitCountToByteCount(mBitIndex); }
	public void clear()							
	{
		mBitIndex = 0; 
		if (mBuffer != null)
		{
			memset(mBuffer, (byte)0);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void writeCheck(int writeLen)
	{
		// 如果缓冲区为空,则创建缓冲区
		if (mBuffer == null)
		{
			writeLen = getGreaterPow2(writeLen);
			// 至少分配32个字节,避免初期频繁扩容
			clampMin(ref writeLen, 32);
			mBuffer = new byte[writeLen];
			memset(mBuffer, (byte)0);
			return;
		}

		// 如果缓冲区已经不够用了,则重新扩展缓冲区
		int curByte = bitCountToByteCount(mBitIndex);
		int curSize = mBuffer.Length;
		if (writeLen + curByte > curSize)
		{
			int maxSize = getGreaterPow2(writeLen + curByte);
			byte[] newBuffer = new byte[maxSize > curSize << 1 ? maxSize : curSize << 1];
			memcpy(newBuffer, mBuffer, 0, 0, curSize);
			memset(newBuffer, (byte)0, curSize, newBuffer.Length - curSize);
			mBuffer = newBuffer;
		}
	}
}