using System;
using System.Collections.Generic;
using static BinaryUtility;
using static FrameDefine;

public class NetStruct : SerializableBit
{
	public List<SerializableBit> mParams = new List<SerializableBit>();
	public bool mHasOptionalParams = false;
	public sealed override bool read(SerializerBitRead reader)
	{
		// 先读取字段有效性标记
		ulong fieldFlag = FULL_FIELD_FLAG;
		if (mHasOptionalParams)
		{
			reader.read(out bool useFlag);
			if (useFlag)
			{
				reader.read(out fieldFlag);
			}
		}
		
		bool success = true;
		int count = mParams.Count;
		for (int i = 0; i < count; ++i)
		{
			SerializableBit field = mParams[i];
			// 非可选字段或者是有效的可选字段才会进行读取
			// 最多只能表示前64个字段(ulong的位数)是否有效,超过的后面都认为是有效的
			field.mValid = !field.mOptional || i >= 64 || hasBit(fieldFlag, i);
			if (field.mValid)
			{
				// 因为不支持结构体嵌套,所以结构体内的字段读取都不需要位标记
				success = success && field.read(reader);
			}
		}
		return success;
	}
	public sealed override void write(SerializerBitWrite writer)
	{
		// 如果没有可选字段,则不使用位标记
		// 如果有可选字段,但是所有字段都需要同步,则也不使用位标记
		int count = mParams.Count;
		if (mHasOptionalParams)
		{
			ulong fieldFlag = 0;
			bool useFlag = false;
			for (int i = 0; i < count; ++i)
			{
				SerializableBit field = mParams[i];
				if (!field.mOptional)
				{
					continue;
				}
				if (field.mValid)
				{
					setBitOne(ref fieldFlag, i);
				}
				else
				{
					setBitZero(ref fieldFlag, i);
					useFlag = true;
				}
			}
			writer.write(useFlag);
			if (useFlag)
			{
				writer.write(fieldFlag);
			}
		}

		for (int i = 0; i < count; ++i)
		{
			SerializableBit field = mParams[i];
			if (!field.mOptional || field.mValid)
			{
				mParams[i].write(writer);
			}
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		int count = mParams.Count;
		for (int i = 0; i < count; ++i)
		{
			mParams[i].resetProperty();
		}
		// 构造中赋值的,不需要重置
		//mHasOptionalParams = false;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addParam(SerializableBit param, bool isOptional)
	{
		mHasOptionalParams |= isOptional;
		param.mOptional = isOptional;
		mParams.Add(param);
	}
}