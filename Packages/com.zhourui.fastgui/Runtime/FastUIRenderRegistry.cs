using System;
using System.Collections.Generic;
using UnityEngine;

// FastCanvas的managed RenderElement注册表。
// 主序列和VertexSlot Owner只保存FastUIRenderElement；不再维护FastRawImage兼容镜像。
public sealed class FastUIRenderRegistry
{
	private readonly List<FastUIRenderElement> mElements = new();
	private FastUIRenderElement[] mVertexSlotOwners;
	public int Count
	{
		get
		{
			return mElements.Count;
		}
	}
	public FastUIRenderElement this[int index]
	{
		get
		{
			return mElements[index];
		}
		set
		{
			mElements[index] = value;
		}
	}
	public int IndexOf(FastUIRenderElement element)
	{
		return mElements.IndexOf(element);
	}
	public void Add(FastUIRenderElement element)
	{
		mElements.Add(element);
	}
	// 初次注册时Element Append与稳定VertexSlot Owner绑定属于同一个原子初始化步骤。
	// 合并入口避免FastCanvas先ensure/set owner、再Count/Add重复走多层方法和容量判断。
	public int addInitial(FastUIRenderElement element, int slot)
	{
		ensureVertexSlotCapacity(slot + 1);
		int index = mElements.Count;
		mElements.Add(element);
		mVertexSlotOwners[slot] = element;
		return index;
	}
	public void Insert(int index, FastUIRenderElement element)
	{
		mElements.Insert(index, element);
	}
	public void InsertRange(int index, List<FastUIRenderElement> elements)
	{
		if (elements == null || elements.Count == 0)
		{
			return;
		}
		mElements.InsertRange(index, elements);
	}
	public void RemoveAt(int index)
	{
		mElements.RemoveAt(index);
	}
	public void RemoveRange(int index, int count)
	{
		mElements.RemoveRange(index, count);
	}
	public void Clear()
	{
		mElements.Clear();
		mVertexSlotOwners = null;
	}
	public List<FastUIRenderElement> getElements()
	{
		return mElements;
	}
	public bool requiresTMPVertexLayout(FastUIRenderElement element)
	{
		return element != null && element.requiresTMPVertexLayout();
	}
	public void ensureCapacity(int requiredCount)
	{
		if (requiredCount <= 0)
		{
			return;
		}
		if (mElements.Capacity < requiredCount)
		{
			mElements.Capacity = requiredCount;
		}
		ensureVertexSlotCapacity(requiredCount);
	}
	public void ensureVertexSlotCapacity(int requiredCount)
	{
		if (requiredCount <= 0 || (mVertexSlotOwners != null && mVertexSlotOwners.Length >= requiredCount))
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCount, 4));
		Array.Resize(ref mVertexSlotOwners, capacity);
	}
	public FastUIRenderElement[] getVertexSlotOwners()
	{
		return mVertexSlotOwners;
	}
	public FastUIRenderElement getVertexSlotOwner(int slot)
	{
		return mVertexSlotOwners != null && (uint)slot < (uint)mVertexSlotOwners.Length ? mVertexSlotOwners[slot] : null;
	}
	public void setVertexSlotOwner(int slot, FastUIRenderElement element)
	{
		if (slot < 0)
		{
			return;
		}
		ensureVertexSlotCapacity(slot + 1);
		mVertexSlotOwners[slot] = element;
	}
	public void clearVertexSlotOwner(int slot, FastUIRenderElement element)
	{
		if (mVertexSlotOwners == null || (uint)slot >= (uint)mVertexSlotOwners.Length || mVertexSlotOwners[slot] != element)
		{
			return;
		}
		mVertexSlotOwners[slot] = null;
	}
	public void clearVertexSlotOwners()
	{
		mVertexSlotOwners = null;
	}
}
