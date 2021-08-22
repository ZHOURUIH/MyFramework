using System;
using System.Collections.Generic;
using System.Text;

// 只读缓冲区,用于解析二进制数组
public class SerializerRead : FrameBase
{
	protected byte[] mBuffer;	// 缓冲区
	protected int mBufferSize;	// 缓冲区大小
	protected int mIndex;		// 当前读下标
	protected bool mShowError;	// 是否显示错误信息,比如在检查到写入失败或者读取失败时是否显示报错
	public SerializerRead()
	{
		mBuffer = null;
		mShowError = true;
		mIndex = 0;
		mBufferSize = 0;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffer = null;
		mShowError = true;
		mIndex = 0;
		mBufferSize = 0;
	}
	public void init(byte[] buffer, int bufferSize = -1, int index = 0)
	{
		mIndex = index;
		mBuffer = buffer;
		mBufferSize = bufferSize < 0 ? buffer.Length : bufferSize;
	}
	public bool read(out byte value)
	{
		value = 0;
		int readLen = sizeof(byte);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readByte(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool read(out short value)
	{
		value = 0;
		int readLen = sizeof(short);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readShort(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool readBigEndian(out short value)
	{
		value = 0;
		int readLen = sizeof(short);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readShortBigEndian(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool read(out ushort value)
	{
		value = 0;
		int readLen = sizeof(ushort);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readUShort(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool readBigEndian(out ushort value)
	{
		value = 0;
		int readLen = sizeof(ushort);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readUShortBigEndian(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool read(out int value)
	{
		value = 0;
		int readLen = sizeof(int);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readInt(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool readBigEndian(out int value)
	{
		value = 0;
		int readLen = sizeof(int);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readIntBigEndian(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool read(out uint value)
	{
		value = 0;
		int readLen = sizeof(uint);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readUInt(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool readBigEndian(out uint value)
	{
		value = 0;
		int readLen = sizeof(uint);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readUIntBigEndian(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool read(out float value)
	{
		value = 0.0f;
		int readLen = sizeof(float);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readFloat(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool readBigEndian(out float value)
	{
		value = 0.0f;
		int readLen = sizeof(float);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readFloatBigEndian(mBuffer, ref mIndex, out _);
		return true;
	}
	public bool readBuffer(byte[] buffer, int readLen, int bufferSize = -1)
	{
		if (!readCheck(readLen))
		{
			return false;
		}
		readBytes(mBuffer, ref mIndex, buffer, -1, bufferSize, readLen);
		return true;
	}
	public bool readBuffer(byte[] buffer)
	{
		if (!readCheck(buffer.Length))
		{
			return false;
		}
		readBytes(mBuffer, ref mIndex, buffer, -1, buffer.Length, buffer.Length);
		return true;
	}
	// 返回值表示是否读取完全
	public bool readString(byte[] str, int strBufferSize)
	{
		if (!readCheck(sizeof(int)))
		{
			return false;
		}
		// 先读入字符串长度
		read(out int readLen);
		if (!readCheck(readLen))
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
		if (!readCheck(sizeof(int)))
		{
			return false;
		}
		// 先读入字符串长度
		read(out int readLen);
		if (readLen == 0)
		{
			value = EMPTY;
			return true;
		}
		if (!readCheck(readLen))
		{
			return false;
		}
		value = bytesToString(mBuffer, mIndex, readLen, encoding);
		mIndex += readLen;
		return true;
	}
	public void skipIndex(int skip) 			{ mIndex += skip; }
	public bool canRead(int readLen) 			{ return mIndex + readLen <= mBufferSize; }
	public bool canWrite(int writeLen) 			{ return writeLen + mIndex <= mBufferSize; }
	public byte[] getBuffer() 					{ return mBuffer; }
	public int getDataSize() 					{ return mBufferSize; }
	public int getIndex() 						{ return mIndex; }
	public void setIndex(int index) 			{ mIndex = index; }
	public void setShowError(bool showError)	{ mShowError = showError; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected bool readCheck(int readLen)
	{
		if (mBuffer == null)
		{
			if (mShowError)
			{
				logError("buffer is NULL! can not read");
			}
			return false;
		}
		if (!canRead(readLen))
		{
			if (mShowError)
			{
				logError("read buffer out of range! cur index : " + mIndex + ", buffer size : " + mBufferSize + ", read length : " + readLen);
			}
			return false;
		}
		return true;
	}
}