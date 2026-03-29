using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static FrameUtility;

public class NetStructItemInfo : NetStructBit
{
	public BIT_INT mItemID = new();
	public BIT_INT mItemCount = new();
	public NetStructItemInfo()
	{
		addParam(mItemID, false);
		addParam(mItemCount, false);
	}
	protected override bool readInternal(ulong fieldFlag, SerializerBitRead reader, bool needReadSign)
	{
		bool success = true;
		success = success && reader.read(out mItemID.mValue, out mItemCount.mValue, needReadSign);
		return success;
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		base.write(writer, needWriteSign);
		writer.write(stackalloc int[2]{ mItemID, mItemCount }, needWriteSign);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mItemID.resetProperty();
		mItemCount.resetProperty();
	}
}

public class NetStructItemInfo_List : SerializableBit
{
	public List<NetStructItemInfo> mList = new();
	public NetStructItemInfo this[int index]
	{
		get { return mList[index]; }
		set { mList[index] = value; }
	}
	public int Count{ get { return mList.Count; } }
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.readCustomList(mList, needReadSign);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.writeCustomList(mList, needWriteSign);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		UN_CLASS_LIST(mList);
	}
	public List<NetStructItemInfo>.Enumerator GetEnumerator(){ return mList.GetEnumerator(); }
}