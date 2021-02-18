using System;
using System.Collections.Generic;
using System.Text;

// 用于生成二进制文件的
public class Serializer : FrameBase
{
	protected byte[] mBuffer;
	protected bool mReadOnly;  // 如果为真,则表示是只写的,为假则表示是只读的
	protected bool mShowError;	// 是否显示错误信息,比如在检查到写入失败或者读取失败时是否显示报错
	protected int mIndex;
	protected int mBufferSize;
	public Serializer()
	{
		mBuffer = null;
		mReadOnly = false;
		mShowError = true;
		mIndex = 0;
		mBufferSize = 0;
	}
	public Serializer(byte[] buffer)
	{
		init(buffer);
	}
	public Serializer(byte[] buffer, int bufferSize)
	{
		init(buffer, bufferSize);
	}
	public Serializer(byte[] buffer, int bufferSize, int index)
	{
		init(buffer, bufferSize, index);
	}
	public void init(byte[] buffer, int bufferSize = -1, int index = 0)
	{
		mReadOnly = true;
		mIndex = index;
		mBuffer = buffer;
		mBufferSize = bufferSize < 0 ? buffer.Length : bufferSize;
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
	public bool write(short value, bool inverse = false)
	{
		if (!writeCheck(sizeof(short)))
		{
			return false;
		}
		writeShort(mBuffer, ref mIndex, value, inverse);
		return true;
	}
	public bool write(ushort value, bool inverse = false)
	{
		if (!writeCheck(sizeof(ushort)))
		{
			return false;
		}
		writeUShort(mBuffer, ref mIndex, value, inverse);
		return true;
	}
	public bool write(int value, bool inverse = false)
	{
		if (!writeCheck(sizeof(int)))
		{
			return false;
		}
		writeInt(mBuffer, ref mIndex, value, inverse);
		return true;
	}
	public bool write(uint value, bool inverse = false)
	{
		if (!writeCheck(sizeof(uint)))
		{
			return false;
		}
		writeUInt(mBuffer, ref mIndex, value, inverse);
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
	public bool read(out short value, bool inverse = false)
	{
		value = 0;
		int readLen = sizeof(short);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readShort(mBuffer, ref mIndex, out _, inverse);
		return true;
	}
	public bool read(out ushort value, bool inverse = false)
	{
		value = 0;
		int readLen = sizeof(ushort);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readUShort(mBuffer, ref mIndex, out _, inverse);
		return true;
	}
	public bool read(out int value, bool inverse = false)
	{
		value = 0;
		int readLen = sizeof(int);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readInt(mBuffer, ref mIndex, out _, inverse);
		return true;
	}
	public bool read(out uint value, bool inverse = false)
	{
		value = 0;
		int readLen = sizeof(uint);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readUInt(mBuffer, ref mIndex, out _, inverse);
		return true;
	}
	public bool read(out float value, bool inverse = false)
	{
		value = 0.0f;
		int readLen = sizeof(float);
		if (!readCheck(readLen))
		{
			return false;
		}
		value = readFloat(mBuffer, ref mIndex, out _, inverse);
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
		// 如果是只读的,则不能写入
		if (mReadOnly)
		{
			if(mShowError)
			{
				logError("the buffer is read only, can not write!");
			}
			return false;
		}
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
	protected bool readCheck(int readLen)
	{
		// 如果是只写的,则不能读取
		if (!mReadOnly)
		{
			if(mShowError)
			{
				logError("the buffer is write only, can not read!");
			}
			return false;
		}
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
	protected void resizeBuffer(int maxSize)
	{
		// 只读的缓冲区不能改变缓冲区大小
		if (mReadOnly)
		{
			return;
		}
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
		// 只读的缓冲区不能创建缓冲区
		if (mReadOnly)
		{
			return;
		}
		if (mBuffer == null)
		{
			mBufferSize = bufferSize;
			mBuffer = new byte[mBufferSize];
		}
	}
}