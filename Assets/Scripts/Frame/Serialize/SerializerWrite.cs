using System;
using System.Collections.Generic;
using System.Text;

// 只写缓冲区,用于生成二进制数据流的
public class SerializerWrite : FrameBase
{
	protected byte[] mBuffer;	// 缓冲区
	protected int mBufferSize;	// 缓冲区大小
	protected int mIndex;		// 当前写下标
	protected bool mShowError;	// 是否显示错误信息,比如在检查到写入失败或者读取失败时是否显示报错
	public SerializerWrite()
	{
		mBuffer = null;
		mShowError = true;
		mIndex = 0;
		mBufferSize = 0;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		// mBuffer,mBufferSize不重置,保证可以尽可能复用数组
		//mBuffer = null;
		//mBufferSize = 0;
		mShowError = true;
		mIndex = 0;
	}
	public bool write(byte value)
	{
		if (!writeCheck(sizeof(byte)))
		{
			return false;
		}
		writeByte(mBuffer, ref mIndex, value);
		return true;
	}
	public bool write(short value)
	{
		if (!writeCheck(sizeof(short)))
		{
			return false;
		}
		writeShort(mBuffer, ref mIndex, value);
		return true;
	}
	public bool writeBigEndian(short value)
	{
		if (!writeCheck(sizeof(short)))
		{
			return false;
		}
		writeShortBigEndian(mBuffer, ref mIndex, value);
		return true;
	}
	public bool write(ushort value)
	{
		if (!writeCheck(sizeof(ushort)))
		{
			return false;
		}
		writeUShort(mBuffer, ref mIndex, value);
		return true;
	}
	public bool writeBigEndian(ushort value)
	{
		if (!writeCheck(sizeof(ushort)))
		{
			return false;
		}
		writeUShortBigEndian(mBuffer, ref mIndex, value);
		return true;
	}
	public bool write(int value)
	{
		if (!writeCheck(sizeof(int)))
		{
			return false;
		}
		writeInt(mBuffer, ref mIndex, value);
		return true;
	}
	public bool writeBigEndian(int value)
	{
		if (!writeCheck(sizeof(int)))
		{
			return false;
		}
		writeIntBigEndian(mBuffer, ref mIndex, value);
		return true;
	}
	public bool write(uint value)
	{
		if (!writeCheck(sizeof(uint)))
		{
			return false;
		}
		writeUInt(mBuffer, ref mIndex, value);
		return true;
	}
	public bool writeBigEndian(uint value)
	{
		if (!writeCheck(sizeof(uint)))
		{
			return false;
		}
		writeUIntBigEndian(mBuffer, ref mIndex, value);
		return true;
	}
	public bool write(float value)
	{
		if (!writeCheck(sizeof(float)))
		{
			return false;
		}
		writeFloat(mBuffer, ref mIndex, value);
		return true;
	}
	public bool writeBigEndian(float value)
	{
		if (!writeCheck(sizeof(float)))
		{
			return false;
		}
		writeFloatBigEndian(mBuffer, ref mIndex, value);
		return true;
	}
	public bool writeBuffer(byte[] buffer, int bufferSize)
	{
		if (!writeCheck(bufferSize))
		{
			return false;
		}
		writeBytes(mBuffer, ref mIndex, buffer, -1, -1, bufferSize);
		return true;
	}
	public bool writeString(string str, Encoding encoding = null)
	{
		int strLen = 0;
		byte[] strBytes = null;
		if (str != null)
		{
			strBytes = stringToBytes(str, encoding);
			strLen = strBytes.Length;
		}
		if (!writeCheck(strLen + sizeof(int)))
		{
			return false;
		}
		// 先写入字符串长度
		write(strLen);
		if(strLen > 0)
		{
			writeBuffer(strBytes, strLen);
		}
		return true;
	}
	public void skipIndex(int skip) { mIndex += skip; }
	public bool canRead(int readLen) { return mIndex + readLen <= mBufferSize; }
	public bool canWrite(int writeLen){ return writeLen + mIndex <= mBufferSize; }
	public byte[] getBuffer() { return mBuffer; }
	public int getBufferSize() { return mBufferSize; }
	public int getDataSize() { return mIndex; }
	public int getIndex() { return mIndex; }
	public void setIndex(int index) { mIndex = index; }
	public void setShowError(bool showError) { mShowError = showError; }
	//-------------------------------------------------------------------------------------------------------------------------------------------
	protected bool writeCheck(int writeLen)
	{
		// 如果缓冲区为空,则创建缓冲区
		if (mBuffer == null)
		{
			createBuffer(writeLen);
		}
		// 如果缓冲区已经不够用了,则重新扩展缓冲区
		else if (!canWrite(writeLen))
		{
			resizeBuffer(writeLen + mIndex);
		}
		return true;
	}
	protected void resizeBuffer(int maxSize)
	{
		int newSize = maxSize > mBufferSize * 2 ? maxSize : mBufferSize * 2;
		byte[] newBuffer = new byte[newSize];
		for (int i = 0; i < mBufferSize; ++i)
		{
			newBuffer[i] = mBuffer[i];
		}
		mBuffer = newBuffer;
		mBufferSize = newSize;
	}
	protected void createBuffer(int bufferSize)
	{
		if (mBuffer == null)
		{
			mBufferSize = bufferSize;
			mBuffer = new byte[mBufferSize];
		}
	}
}