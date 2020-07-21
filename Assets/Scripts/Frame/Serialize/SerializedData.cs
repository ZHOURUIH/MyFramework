using System;
using System.Collections;
using System.Collections.Generic;

public abstract class SerializedData : GameBase
{
	protected List<OBJECT> mParameterInfoList;
	protected int mMaxDataSize;
	public SerializedData()
	{
		mMaxDataSize = 0;
		mParameterInfoList = new List<OBJECT>();
	}
	public bool read(byte[] buffer, int offset = 0)
	{
		return read(buffer, ref offset);
	}
	// 从buffer中读取数据到所有参数中
	public bool read(byte[] buffer, ref int offset)
	{
		if(buffer == null)
		{
			return false;
		}
		int parameterCount = mParameterInfoList.Count;
		for (int i = 0; i < parameterCount; ++i)
		{
			if(!mParameterInfoList[i].readFromBuffer(buffer, ref offset))
			{
				return false;
			}
		}
		return true;
	}
	public bool write(byte[] buffer, int offset = 0)
	{
		return write(buffer, ref offset);
	}
	// 将所有参数的值写入buffer
	public bool write(byte[] buffer, ref int offset)
	{
		int parameterCount = mParameterInfoList.Count;
		for (int i = 0; i < parameterCount; ++i)
		{
			if(!mParameterInfoList[i].writeToBuffer(buffer, ref offset))
			{
				return false;
			}
		}
		return true;
	}
	public int getMaxSize() { return mMaxDataSize; }
	public int generateSize()
	{
		int size = 0;
		int parameterCount = mParameterInfoList.Count;
		for (int i = 0; i < parameterCount; ++i)
		{
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
		mMaxDataSize = 0;
		int count = mParameterInfoList.Count;
		for (int i = 0; i < count; ++i)
		{
			mParameterInfoList[i].zero();
			mMaxDataSize += mParameterInfoList[i].mSize;
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
		mParameterInfoList.Add(param);
	}
}