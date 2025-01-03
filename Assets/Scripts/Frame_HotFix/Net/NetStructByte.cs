using System.Collections.Generic;
using static BinaryUtility;
using static FrameDefine;

// 消息结构体类型的参数
public class NetStructByte : Serializable
{
	public List<Serializable> mParams = new();		// 结构体中成员变量参数列表
	public bool mHasOptionalParams;					// 是否含有可选参数的成员变量
	public sealed override bool read(SerializerRead reader)
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
		return readInternal(fieldFlag, reader);
	}
	public override void write(SerializerWrite writer)
	{
		// 如果没有可选字段,则不使用位标记
		// 如果有可选字段,但是所有字段都需要同步,则也不使用位标记
		int count = mParams.Count;
		if (mHasOptionalParams)
		{
			ulong fieldFlag = 0;
			bool useFlag = false;
			for (byte i = 0; i < count; ++i)
			{
				Serializable field = mParams[i];
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
	}
	public override void resetProperty()
	{
		base.resetProperty();
		foreach (Serializable param in mParams)
		{
			param.resetProperty();
		}
		// 构造中赋值的,不需要重置
		// mHasOptionalParams = false;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addParam(Serializable param, bool isOptional)
	{
		mHasOptionalParams |= isOptional;
		param.mOptional = isOptional;
		mParams.Add(param);
	}
	protected virtual bool readInternal(ulong fieldFlag, SerializerRead reader) { return true; }
}