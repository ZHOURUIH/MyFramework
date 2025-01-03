using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using static StringUtility;
using static BinaryUtility;

// 只写缓冲区,用于生成二进制数据流的,按字节进行写入
public class SerializerWrite
{
	protected byte[] mBuffer;   // 缓冲区
	protected int mIndex;       // 当前写下标
	public void write(bool value)
	{
		writeCheck(sizeof(bool));
		writeBool(mBuffer, ref mIndex, value);
	}
	public void write(byte value)
	{
		writeCheck(sizeof(byte));
		writeByte(mBuffer, ref mIndex, value);
	}
	public void write(short value)
	{
		writeCheck(sizeof(short));
		writeShort(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(short value)
	{
		writeCheck(sizeof(short));
		writeShortBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(ushort value)
	{
		writeCheck(sizeof(ushort));
		writeUShort(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(ushort value)
	{
		writeCheck(sizeof(ushort));
		writeUShortBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(int value)
	{
		writeCheck(sizeof(int));
		writeInt(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(int value)
	{
		writeCheck(sizeof(int));
		writeIntBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(uint value)
	{
		writeCheck(sizeof(uint));
		writeUInt(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(uint value)
	{
		writeCheck(sizeof(uint));
		writeUIntBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(long value)
	{
		writeCheck(sizeof(long));
		writeLong(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(long value)
	{
		writeCheck(sizeof(long));
		writeLongBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(ulong value)
	{
		writeCheck(sizeof(ulong));
		writeULong(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(ulong value)
	{
		writeCheck(sizeof(long));
		writeULongBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(float value)
	{
		writeCheck(sizeof(float));
		writeFloat(mBuffer, ref mIndex, value);
	}
	public void writeBigEndian(float value)
	{
		writeCheck(sizeof(float));
		writeFloatBigEndian(mBuffer, ref mIndex, value);
	}
	public void write(Vector2 value)
	{
		writeCheck(sizeof(float) * 2);
		writeFloat(mBuffer, ref mIndex, value.X);
		writeFloat(mBuffer, ref mIndex, value.Y);
	}
	public void write(Vector2Int value)
	{
		writeCheck(sizeof(int) * 2);
		writeInt(mBuffer, ref mIndex, value.x);
		writeInt(mBuffer, ref mIndex, value.y);
	}
	public void write(Vector3 value)
	{
		writeCheck(sizeof(float) * 3);
		writeFloat(mBuffer, ref mIndex, value.X);
		writeFloat(mBuffer, ref mIndex, value.Y);
		writeFloat(mBuffer, ref mIndex, value.Z);
	}
	public void write(Vector3Int value)
	{
		writeCheck(sizeof(int) * 3);
		writeInt(mBuffer, ref mIndex, value.x);
		writeInt(mBuffer, ref mIndex, value.y);
		writeInt(mBuffer, ref mIndex, value.z);
	}
	public void write(Vector4 value)
	{
		writeCheck(sizeof(float) * 4);
		writeFloat(mBuffer, ref mIndex, value.X);
		writeFloat(mBuffer, ref mIndex, value.Y);
		writeFloat(mBuffer, ref mIndex, value.Z);
		writeFloat(mBuffer, ref mIndex, value.W);
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
		if (strLen > 0)
		{
			writeBuffer(strBytes, strLen);
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
	public void writeList(List<Vector2> list)
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
	public void writeList(List<Vector3> list)
	{
		int count = list.Count;
		write(count);
		for (int i = 0; i < count; ++i)
		{
			write(list[i]);
		}
	}
	public void writeList(List<Vector3Int> list)
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
	public void skipIndex(int skip) { mIndex += skip; }
	public byte[] getBuffer() { return mBuffer; }
	public int getDataSize() { return mIndex; }
	public void setIndex(int index) { mIndex = index; }
	public void clear() { mIndex = 0; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void writeCheck(int writeLen)
	{
		// 如果缓冲区为空,则创建缓冲区
		if (mBuffer == null)
		{
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