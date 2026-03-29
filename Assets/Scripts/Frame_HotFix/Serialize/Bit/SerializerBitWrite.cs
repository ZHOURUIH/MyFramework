using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using static BinaryUtility;
using static SerializeBitUtility;
using static MathUtility;

// 只写缓冲区,用于生成二进制数据流,按位进行写入
public class SerializerBitWrite : ClassObject
{
	protected byte[] mBuffer;	// 缓冲区
	protected int mBitIndex;	// 当前写下标
	public override void resetProperty()
	{
		base.resetProperty();
		// 这里需要保留已经new出来的数组,尽量复用
		mBuffer?.setAllDefault();
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
		writeCheck(3 + (sizeof(byte) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(byte value)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多7bit长度位,所以需要多扩容1个字节
		writeCheck(sizeof(byte) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<sbyte> values, bool needWriteSign)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多8bit,7bit长度位+1bit符号位,所以需要多扩容1个字节
		writeCheck(3 + (sizeof(sbyte) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values, needWriteSign);
	}
	public void write(sbyte value, bool needWriteSign)
	{
		// 因为最差情况下实际所需的空间比value自身占的空间多8bit,7bit长度位+1bit符号位,所以需要多扩容1个字节
		writeCheck(sizeof(sbyte) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value, needWriteSign);
	}
	public void write(Span<short> values, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(short) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values, needWriteSign);
	}
	public void write(short value, bool needWriteSign)
	{
		writeCheck(sizeof(short) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value, needWriteSign);
	}
	public void write(Span<ushort> values)
	{
		writeCheck(3 + (sizeof(ushort) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(ushort value)
	{
		writeCheck(sizeof(ushort) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<int> values, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(int) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values, needWriteSign);
	}
	public void write(int value, bool needWriteSign)
	{
		writeCheck(sizeof(int) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value, needWriteSign);
	}
	public void write(Span<uint> values)
	{
		writeCheck(3 + (sizeof(uint) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(uint value)
	{
		writeCheck(sizeof(uint) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<long> values, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(long) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values, needWriteSign);
	}
	public void write(long value, bool needWriteSign)
	{
		writeCheck(sizeof(long) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value, needWriteSign);
	}
	public void write(Span<ulong> values)
	{
		writeCheck(3 + (sizeof(ulong) + 1) * values.Length);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, values);
	}
	public void write(ulong value)
	{
		writeCheck(sizeof(ulong) + 1);
		writeBit(mBuffer, mBuffer.Length, ref mBitIndex, value);
	}
	public void write(Span<float> values, bool needWriteSign, int precision = 3)
	{
		int powValue = pow10(precision);
		int count = values.Length;
		Span<int> ints = stackalloc int[count];
		for (int i = 0; i < count; ++i)
		{
			ints[i] = round(values[i] * powValue);
		}
		write(ints, needWriteSign);
	}
	public void write(float value, bool needWriteSign, int precision = 3)
	{
		write(round(value * pow10(precision)), needWriteSign);
	}
	public void write(Span<double> values, bool needWriteSign, int precision = 4)
	{
		long powValue = pow10Long(precision);
		int count = values.Length;
		Span<long> longs = stackalloc long[count];
		for (int i = 0; i < count; ++i)
		{
			longs[i] = round(values[i] * powValue);
		}
		write(longs, needWriteSign);
	}
	public void write(double value, bool needWriteSign, int precision = 4)
	{
		write(round(value * pow10Long(precision)), needWriteSign);
	}
	public void write(Vector2 value, bool needWriteSign, int precision = 3)
	{
		int powValue = pow10(precision);
		write(stackalloc int[2] { round(value.x * powValue), round(value.y * powValue) }, needWriteSign);
	}
	public void write(Vector2UShort value)
	{
		write(stackalloc ushort[2] { value.x, value.y });
	}
	public void write(Vector2Short value, bool needWriteSign)
	{
		write(stackalloc short[2] { value.x, value.y }, needWriteSign);
	}
	public void write(Vector2Int value, bool needWriteSign)
	{
		write(stackalloc int[2] { value.x, value.y }, needWriteSign);
	}
	public void write(Vector2IntMy value, bool needWriteSign)
	{
		write(stackalloc int[2] { value.x, value.y }, needWriteSign);
	}
	public void write(Vector2UInt value)
	{
		write(stackalloc uint[2] { value.x, value.y });
	}
	public void write(Vector3 value, bool needWriteSign, int precision = 3)
	{
		int powValue = pow10(precision);
		write(stackalloc int[3] { round(value.x * powValue), round(value.y * powValue), round(value.z * powValue) }, needWriteSign);
	}
	public void write(Vector4 value, bool needWriteSign, int precision = 3)
	{
		int powValue = pow10(precision);
		write(stackalloc int[4] { round(value.x * powValue), round(value.y * powValue), round(value.z * powValue), round(value.w * powValue) }, needWriteSign);
	}
	public void writeBuffer(byte[] buffer, int dataSize)
	{
		if (buffer.isEmpty() || dataSize == 0)
		{
			return;
		}
		writeCheck(dataSize + 1);
		writeBufferBit(mBuffer, mBuffer.Length, ref mBitIndex, buffer, dataSize);
	}
	// 将最后一个字节未填充数据的位填充为0,并将位下标移动到字节末尾
	public void fillZeroToByteEnd()
	{
		SerializeBitUtility.fillZeroToByteEnd(mBuffer, ref mBitIndex);
	}
	public void writeString(string str, Encoding encoding = null)
	{
		byte[] bytes = str.toBytes(encoding);
		int strLen = bytes.count();
		// 先写入字符串长度,如果长度为0,则不做任何改变
		write((uint)strLen);
		if (strLen == 0)
		{
			return;
		}
		writeBuffer(bytes, strLen);
	}
	public void writeCustomList<T>(List<T> list, bool needWriteSign) where T : SerializableBit
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			list[i].write(this, needWriteSign);
		}
	}
	public void writeList(List<byte> list)
	{
		// 头部：2字节count(ushort) + 1字节lengthBitType标志；每元素：原始字节 + 1字节VLQ长度位开销
		writeCheck(3 + (sizeof(byte) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<sbyte> list, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(sbyte) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, needWriteSign);
	}
	public void writeList(List<short> list, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(short) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, needWriteSign);
	}
	public void writeList(List<ushort> list)
	{
		writeCheck(3 + (sizeof(ushort) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<int> list, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(int) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, needWriteSign);
	}
	public void writeList(List<uint> list)
	{
		writeCheck(3 + (sizeof(uint) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<long> list, bool needWriteSign)
	{
		writeCheck(3 + (sizeof(long) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, needWriteSign);
	}
	public void writeList(List<ulong> list)
	{
		writeCheck(3 + (sizeof(ulong) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list);
	}
	public void writeList(List<float> list, bool needWriteSign, int precision = 3)
	{
		writeCheck(3 + (sizeof(float) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, needWriteSign, precision);
	}
	public void writeList(List<double> list, bool needWriteSign, int precision = 4)
	{
		writeCheck(3 + (sizeof(double) + 1) * list.Count);
		writeListBit(mBuffer, mBuffer.Length, ref mBitIndex, list, needWriteSign, precision);
	}
	public void writeList(List<Vector2> list, bool needWriteSign, int precision = 3)
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], needWriteSign, precision);
		}
	}
	public void writeList(List<Vector2UShort> list)
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<Vector2UInt> list)
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<Vector2Int> list, bool needWriteSign)
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], needWriteSign);
		}
	}
	public void writeList(List<Vector3> list, bool needWriteSign, int precision = 3)
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], needWriteSign, precision);
		}
	}
	public void writeList(List<Vector4> list, bool needWriteSign, int precision = 3)
	{
		int count = list.Count;
		write((uint)count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i], needWriteSign, precision);
		}
	}
	public void writeList(List<string> list)
	{
		int count = list.Count;
		write((uint)count);
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
		mBuffer?.setAllDefault();
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
			mBuffer.setAllDefault();
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