using System.Collections.Generic;
using static BinaryUtility;

// 消息基类,按bit传输
public abstract class NetPacketBit : NetPacket
{
	protected List<SerializableBit> mParameters = new();    // 消息参数对象列表
	protected bool mHasSign = false;                        // 当前消息包中是否有负数
	protected bool mHasGenerateSign = false;				// 是否已经计算过了mHasSign
	public override void resetProperty()
	{
		base.resetProperty();
		foreach (SerializableBit item in mParameters)
		{
			item.resetProperty();
		}
		mHasSign = false;
		mHasGenerateSign = false;
	}
	// 从buffer中读取数据到所有参数中
	public virtual bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag) { return true; }
	// 将所有参数的值写入buffer
	public virtual void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
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
	// 计算当前这个消息包的数据中是否包含负数,如果没有负数,则可以将所有有符号整数的符号全部省略
	public bool hasSign()
	{
		if (!mHasGenerateSign)
		{
			mHasGenerateSign = true;
			mHasSign = generateHasSignInternal();
		}
		return mHasSign;
	}
	public void markAllFiled(bool valid)
	{
		foreach (SerializableBit item in mParameters)
		{
			item.mValid = valid;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual bool generateHasSignInternal() { return false; }
	protected void addParam(SerializableBit param, bool isOptional)
	{
		mParameters.add(param).mOptional = isOptional;
	}
}