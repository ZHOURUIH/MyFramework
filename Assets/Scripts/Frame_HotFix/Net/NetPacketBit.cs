using System.Collections.Generic;
using static BinaryUtility;

// 消息基类,按bit传输
public abstract class NetPacketBit : NetPacket
{
	protected List<SerializableBit> mParameters = new();	// 消息参数对象列表
	public override void resetProperty()
	{
		base.resetProperty();
		foreach (SerializableBit item in mParameters)
		{
			item.resetProperty();
		}
	}
	// 从buffer中读取数据到所有参数中
	public virtual bool read(SerializerBitRead reader, ulong fieldFlag) { return true; }
	// 将所有参数的值写入buffer
	public virtual void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		fieldFlag = 0;
		int count = mParameters.Count;
		for (byte i = 0; i < count; ++i)
		{
			// 非可选字段或者是有效的可选字段才会写入到数据
			if (!mParameters[i].mOptional)
			{
				setBitOne(ref fieldFlag, i);
			}
		}
	}
	public void markAllFiled(bool valid)
	{
		foreach (SerializableBit item in mParameters)
		{
			item.mValid = valid;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addParam(SerializableBit param, bool isOptional)
	{
		param.mOptional = isOptional;
		mParameters.Add(param);
	}
}