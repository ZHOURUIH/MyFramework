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
	protected override bool readInternal(ulong fieldFlag, SerializerBitRead reader)
	{
		bool success = true;
		success = success && reader.read(out mItemID.mValue, out mItemCount.mValue);
		return success;
	}
	public override void write(SerializerBitWrite writer)
	{
		base.write(writer);
		writer.write(stackalloc int[2]{ mItemID, mItemCount });
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mItemID.resetProperty();
		mItemCount.resetProperty();
	}
}

public class NetStructItemInfo_List : SerializableBit, IEnumerable<NetStructItemInfo>
{
	public List<NetStructItemInfo> mList = new();
	public NetStructItemInfo this[int index]
	{
		get { return mList[index]; }
		set { mList[index] = value; }
	}
	public int Count{ get { return mList.Count; } }
	public override bool read(SerializerBitRead reader)
	{
		return reader.readCustomList(mList);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.writeCustomList(mList);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		UN_CLASS_LIST(mList);
	}
	public IEnumerator<NetStructItemInfo> GetEnumerator(){ return mList.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mList.GetEnumerator(); }
}