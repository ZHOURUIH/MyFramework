using System;
using System.Collections;
using System.Collections.Generic;

public abstract class SerializedData : GameBase
{
	protected List<OBJECT> mParameterInfoList;
	protected int mReadDataSize;            // 写入数据时,总共写入的数据大小
	protected int mMaxDataSize;				// 包体最大的大小,包括如果不使用变长数组时的大小
	protected bool mIntReplaceULLong;       // 如果ullong的值小于int最大值,是否在序列化时写入或读取int
	protected bool mFixedLength;	// 是否所有参数都是定长的
	public SerializedData()
	{
		mMaxDataSize = 0;
		mParameterInfoList = new List<OBJECT>();
		mIntReplaceULLong = true;
	}
	public int read(byte[] buffer, int offset = 0)
	{
		return read(buffer, ref offset);
	}
	// 从buffer中读取数据到所有参数中,返回值表示读取的字节数,小于0表示读取失败
	public int read(byte[] buffer, ref int offset)
	{
		if(buffer == null)
		{
			return -1;
		}
		int startOffset = offset;
		int parameterCount = mParameterInfoList.Count;
		for (int i = 0; i < parameterCount; ++i)
		{
			if (!mParameterInfoList[i].readFromBuffer(buffer, ref offset))
			{
				return -1;
			}
		}
		mReadDataSize = offset - startOffset;
		return mReadDataSize;
	}
	public int getReadDataSize() { return mReadDataSize; }
	public int write(byte[] buffer, int offset = 0)
	{
		return write(buffer, ref offset);
	}
	// 将所有参数的值写入buffer
	public int write(byte[] buffer, ref int offset)
	{
		int startOffset = offset;
		int parameterCount = mParameterInfoList.Count;
		for (int i = 0; i < parameterCount; ++i)
		{
			if(!mParameterInfoList[i].writeToBuffer(buffer, ref offset))
			{
				return -1;
			}
		}
		return offset - startOffset;
	}
	public int getMaxSize() { return mMaxDataSize; }
	public int generateSize(bool ignoreReplace = false)
	{
		if (!ignoreReplace && mIntReplaceULLong)
		{
			logError("当前使用了运行时动态整数类型,只能在序列化时才能获取大小");
			return 0;
		}
		int size = 0;
		int parameterCount = mParameterInfoList.Count;
		for (int i = 0; i < parameterCount; ++i)
		{
			// 变长数组的长度固定使用ushort表示
			if(mParameterInfoList[i].getVariableLength())
			{
				size += sizeof(ushort);
			}
			size += mParameterInfoList[i].getSize();
		}
		return size;
	}
	//----------------------------------------------------------------------------------------------------------------------------------------
	// 在子类构造中调用
	protected abstract void fillParams();
	// 在子类构造中调用
	protected void zeroParams()
	{
		mFixedLength = true;
		mMaxDataSize = 0;
		int count = mParameterInfoList.Count;
		for (int i = 0; i < count; ++i)
		{
			mParameterInfoList[i].zero();
			mMaxDataSize += mParameterInfoList[i].mSize;
			if(mParameterInfoList[i].getVariableLength())
			{
				mFixedLength = false;
			}
		}
	}
	protected void pushParam(OBJECT param, bool variableLength = false)
	{
		if(param == null)
		{
			logError("param is null!");
			return;
		}
		param.setVariableLength(variableLength);
		param.setIntReplaceULLong(mIntReplaceULLong);
		mParameterInfoList.Add(param);
	}
}