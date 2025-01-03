using System.Text;
using System.Collections.Generic;
using UnityEngine;
using static BinaryUtility;
using static MathUtility;

// 只写缓冲区,用于生成二进制数据流的,按字节进行写入
public class SerializerWrite : ClassObject
{
	protected byte[] mBuffer;	// 缓冲区
	protected int mIndex;		// 当前写下标
	public override void resetProperty()
	{
		base.resetProperty();
		// mBuffer不重置,保证可以尽可能复用数组
		// mBuffer = null;
		mIndex = 0;
	}
	public void writeCustom(Serializable seri)
	{
		seri.write(this);
	}
	public void write(bool value)
	{
		writeCheck(sizeof(bool));
		writeBool(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(byte value)
	{
		writeCheck(sizeof(byte));
		writeByte(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(short value)
	{
		writeCheck(sizeof(short));
		writeShort(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(short value)
	{
		writeCheck(sizeof(short));
		writeShortBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(ushort value)
	{
		writeCheck(sizeof(ushort));
		writeUShort(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(ushort value)
	{
		writeCheck(sizeof(ushort));
		writeUShortBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(int value)
	{
		writeCheck(sizeof(int));
		writeInt(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(int value)
	{
		writeCheck(sizeof(int));
		writeIntBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(uint value)
	{
		writeCheck(sizeof(uint));
		writeUInt(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(uint value)
	{
		writeCheck(sizeof(uint));
		writeUIntBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(long value)
	{
		writeCheck(sizeof(long));
		writeLong(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(long value)
	{
		writeCheck(sizeof(long));
		writeLongBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(ulong value)
	{
		writeCheck(sizeof(ulong));
		writeULong(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(ulong value)
	{
		writeCheck(sizeof(long));
		writeULongBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(float value)
	{
		writeCheck(sizeof(float));
		writeFloat(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(float value)
	{
		writeCheck(sizeof(float));
		writeFloatBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(double value)
	{
		writeCheck(sizeof(double));
		writeDouble(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBigEndian(double value)
	{
		writeCheck(sizeof(double));
		writeDoubleBigEndian(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector2 value)
	{
		writeCheck(sizeof(float) * 2);
		writeVector2(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector2UShort value)
	{
		writeCheck(sizeof(ushort) * 2);
		writeVector2UShort(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector2Short value)
	{
		writeCheck(sizeof(short) * 2);
		writeVector2Short(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector2Int value)
	{
		writeCheck(sizeof(int) * 2);
		writeVector2Int(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector2UInt value)
	{
		writeCheck(sizeof(uint) * 2);
		writeVector2UInt(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector3 value)
	{
		writeCheck(sizeof(float) * 3);
		writeVector3(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void write(Vector4 value)
	{
		writeCheck(sizeof(float) * 4);
		writeVector4(mBuffer, mBuffer.Length, ref mIndex, value);
	}
	public void writeBuffer(byte[] buffer, int bufferSize)
	{
		writeCheck(bufferSize);
		writeBytes(mBuffer, ref mIndex, buffer, -1, -1, bufferSize);
	}
	public void writeString(string str, Encoding encoding = null)
	{
		int strLen = 0;
		byte[] strBytes = null;
		if (str != null)
		{
			strBytes = stringToBytes(str, encoding);
			strLen = strBytes.Length;
		}
		writeCheck(strLen + sizeof(int));
		// 先写入字符串长度
		write(strLen);
		if(strLen > 0)
		{
			writeBuffer(strBytes, strLen);
		}
	}
	public void writeCustomList<T>(List<T> list) where T : Serializable
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			writeCustom(list[i]);
		}
	}
	public void writeList(List<byte> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<sbyte> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<short> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<ushort> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<int> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<uint> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<long> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<ulong> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<float> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<double> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
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
	public void skipIndex(int skip)				{ mIndex += skip; }
	public byte[] getBuffer()					{ return mBuffer; }
	public int getDataSize()					{ return mIndex; }
	public void setIndex(int index)				{ mIndex = index; }
	public void clear()							{ mIndex = 0; }
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
			return;
		}

		// 如果缓冲区已经不够用了,则重新扩展缓冲区
		int curSize = mBuffer.Length;
		if (writeLen + mIndex > curSize)
		{
			int maxSize = writeLen + mIndex;
			byte[] newBuffer = new byte[maxSize > curSize << 1 ? maxSize : curSize << 1];
			memcpy(newBuffer, mBuffer, 0, 0, curSize);
			mBuffer = newBuffer;
		}
	}
}