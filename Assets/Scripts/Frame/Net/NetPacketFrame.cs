using System;
using System.Collections.Generic;
using static BinaryUtility;

// Frame层默认的可序列化和反序列化的消息包,应用层可根据实际需求仿照此类封装自己的UDP消息包
public abstract class NetPacketFrame : NetPacket
{
	protected List<SerializableBit> mParameters;
	protected bool mSequenceValid;              // 序列号是否正常,序列号检测不正常时,此消息包不会执行
	public NetPacketFrame()
	{
		mParameters = new List<SerializableBit>();
		mSequenceValid = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mSequenceValid = true;
		int count = mParameters.Count;
		for (int i = 0; i < count; ++i)
		{
			mParameters[i].resetProperty();
		}
	}
	public override bool canExecute() { return mSequenceValid; }
	// 从buffer中读取数据到所有参数中
	public bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool result = true;
		int count = mParameters.Count;
		for (int i = 0; i < count; ++i)
		{
			SerializableBit field = mParameters[i];
			// 非可选字段或者是有效的可选字段才会进行读取
			// 最多只能表示前64个字段是否有效,超过的后面都认为是有效的
			field.mValid = !field.mOptional || i >= 64 || hasBit(fieldFlag, i);
			if (field.mValid)
			{
				result = result && field.read(reader);
			}
		}
		return result;
	}
	// 将所有参数的值写入buffer
	public void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		fieldFlag = 0;
		int count = mParameters.Count;
		for (int i = 0; i < count; ++i)
		{
			SerializableBit field = mParameters[i];
			// 非可选字段或者是有效的可选字段才会写入到数据
			if (!field.mOptional || field.mValid)
			{
				if (i < 64 && !field.mOptional)
				{
					setBitOne(ref fieldFlag, i);
				}
				field.write(writer);
			}
		}
	}
	public void markAllFiled(bool valid)
	{
		int count = mParameters.Count;
		for (int i = 0; i < count; ++i)
		{
			mParameters[i].mValid = valid;
		}
	}
	public void setSequenceValid(bool valid) { mSequenceValid = valid; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addParam(SerializableBit param, bool isOptional)
	{
		param.mOptional = isOptional;
		mParameters.Add(param);
	}
}