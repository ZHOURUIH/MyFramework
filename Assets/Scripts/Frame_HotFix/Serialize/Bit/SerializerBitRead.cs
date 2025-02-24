using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BinaryUtility;
using static StringUtility;
using static CSharpUtility;
using static MathUtility;
using static FrameUtility;

// 只读缓冲区,用于解析二进制数组,按位进行读取
public class SerializerBitRead : ClassObject
{
	protected byte[] mBuffer;   // 缓冲区
	protected int mBufferSize;  // 缓冲区大小
	protected int mBitIndex;    // 当前读下标
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffer = null;
		mBufferSize = 0;
		mBitIndex = 0;
	}
	public void init(byte[] buffer, int bufferSize = -1, int bitIndex = 0)
	{
		mBuffer = buffer;
		mBufferSize = bufferSize < 0 ? buffer.Length : bufferSize;
		mBitIndex = bitIndex;
	}
	public bool readEnumByte<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out byte temp);
		value = intToEnum<T, byte>(temp);
		return success;
	}
	public bool readEnumSByte<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out sbyte temp);
		value = intToEnum<T, sbyte>(temp);
		return success;
	}
	public bool readEnumUShort<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out ushort temp);
		value = intToEnum<T, ushort>(temp);
		return success;
	}
	public bool readEnumShort<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out short temp);
		value = intToEnum<T, short>(temp);
		return success;
	}
	public bool readEnumUInt<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out uint temp);
		value = intToEnum<T, uint>(temp);
		return success;
	}
	public bool readEnumInt<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out int temp);
		value = intToEnum<T, int>(temp);
		return success;
	}
	public bool readEnumULong<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out ulong temp);
		value = intToEnum<T, ulong>(temp);
		return success;
	}
	public bool readEnumLong<T>(out T value) where T : Enum
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out long temp);
		value = intToEnum<T, long>(temp);
		return success;
	}
	public bool readEnumByteList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<byte>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (byte item in temp)
		{
			list.Add(intToEnum<T, byte>(item));
		}
		return success;
	}
	public bool readEnumSByteList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<sbyte>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (sbyte item in temp)
		{
			list.Add(intToEnum<T, sbyte>(item));
		}
		return success;
	}
	public bool readEnumUShortList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<ushort>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (ushort item in temp)
		{
			list.Add(intToEnum<T, ushort>(item));
		}
		return success;
	}
	public bool readEnumShortList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<short>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (short item in temp)
		{
			list.Add(intToEnum<T, short>(item));
		}
		return success;
	}
	public bool readEnumUIntList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<uint>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (uint item in temp)
		{
			list.Add(intToEnum<T, uint>(item));
		}
		return success;
	}
	public bool readEnumIntList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<int>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (int item in temp)
		{
			list.Add(intToEnum<T, int>(item));
		}
		return success;
	}
	public bool readEnumULongList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<ulong>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (ulong item in temp)
		{
			list.Add(intToEnum<T, ulong>(item));
		}
		return success;
	}
	public bool readEnumLongList<T>(List<T> list) where T : Enum
	{
		using var a = new ListScope<long>(out var temp);
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, temp);
		list.Capacity = temp.Capacity;
		foreach (long item in temp)
		{
			list.Add(intToEnum<T, long>(item));
		}
		return success;
	}
	public bool read(out bool value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out byte value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out byte value0, out byte value1)
	{
		Span<byte> list = stackalloc byte[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out byte value0, out byte value1, out byte value2)
	{
		Span<byte> list = stackalloc byte[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out byte value0, out byte value1, out byte value2, out byte value3)
	{
		Span<byte> list = stackalloc byte[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<byte> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out sbyte value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out sbyte value0, out sbyte value1)
	{
		Span<sbyte> list = stackalloc sbyte[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out sbyte value0, out sbyte value1, out sbyte value2)
	{
		Span<sbyte> list = stackalloc sbyte[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out sbyte value0, out sbyte value1, out sbyte value2, out sbyte value3)
	{
		Span<sbyte> list = stackalloc sbyte[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<sbyte> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out short value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out short value0, out short value1)
	{
		Span<short> list = stackalloc short[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out short value0, out short value1, out short value2)
	{
		Span<short> list = stackalloc short[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out short value0, out short value1, out short value2, out short value3)
	{
		Span<short> list = stackalloc short[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<short> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out ushort value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out ushort value0, out ushort value1)
	{
		Span<ushort> list = stackalloc ushort[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out ushort value0, out ushort value1, out ushort value2)
	{
		Span<ushort> list = stackalloc ushort[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out ushort value0, out ushort value1, out ushort value2, out ushort value3)
	{
		Span<ushort> list = stackalloc ushort[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<ushort> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out int value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out int value0, out int value1)
	{
		Span<int> list = stackalloc int[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out int value0, out int value1, out int value2)
	{
		Span<int> list = stackalloc int[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out int value0, out int value1, out int value2, out int value3)
	{
		Span<int> list = stackalloc int[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<int> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out uint value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out uint value0, out uint value1)
	{
		Span<uint> list = stackalloc uint[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out uint value0, out uint value1, out uint value2)
	{
		Span<uint> list = stackalloc uint[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out uint value0, out uint value1, out uint value2, out uint value3)
	{
		Span<uint> list = stackalloc uint[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<uint> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out long value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out long value0, out long value1)
	{
		Span<long> list = stackalloc long[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out long value0, out long value1, out long value2)
	{
		Span<long> list = stackalloc long[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out long value0, out long value1, out long value2, out long value3)
	{
		Span<long> list = stackalloc long[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<long> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out ulong value)
	{
		return readBit(mBuffer, mBufferSize, ref mBitIndex, out value);
	}
	public bool read(out ulong value0, out ulong value1)
	{
		Span<ulong> list = stackalloc ulong[2];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		return result;
	}
	public bool read(out ulong value0, out ulong value1, out ulong value2)
	{
		Span<ulong> list = stackalloc ulong[3];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		return result;
	}
	public bool read(out ulong value0, out ulong value1, out ulong value2, out ulong value3)
	{
		Span<ulong> list = stackalloc ulong[4];
		bool result = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		value0 = list[0];
		value1 = list[1];
		value2 = list[2];
		value3 = list[3];
		return result;
	}
	public bool read(ref Span<ulong> values)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, ref values);
	}
	public bool read(out float value, int precision = 3)
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out int intValue);
		value = divide(intValue, pow10(precision));
		return success;
	}
	public bool read(out float value0, out float value1, int precision = 3)
	{
		Span<int> list = stackalloc int[2];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		float inversePow = inversePow10(precision);
		value0 = list[0] * inversePow;
		value1 = list[1] * inversePow;
		return success;
	}
	public bool read(out float value0, out float value1, out float value2, int precision = 3)
	{
		Span<int> list = stackalloc int[3];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		float inversePow = inversePow10(precision);
		value0 = list[0] * inversePow;
		value1 = list[1] * inversePow;
		value2 = list[2] * inversePow;
		return success;
	}
	public bool read(out float value0, out float value1, out float value2, out float value3, int precision = 3)
	{
		Span<int> list = stackalloc int[4];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		float inversePow = inversePow10(precision);
		value0 = list[0] * inversePow;
		value1 = list[1] * inversePow;
		value2 = list[2] * inversePow;
		value3 = list[3] * inversePow;
		return success;
	}
	public bool read(ref Span<float> values, int precision = 3)
	{
		int count = values.Length;
		Span<int> list = stackalloc int[values.Length];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		float inversePow = inversePow10(precision);
		for (int i = 0; i < count; ++i)
		{
			values[i] = list[i] * inversePow;
		}
		return success;
	}
	public bool read(out double value, int precision = 4)
	{
		bool success = readBit(mBuffer, mBufferSize, ref mBitIndex, out long intValue);
		value = intValue * inversePow10Long(precision);
		return success;
	}
	public bool read(out double value0, out double value1, int precision = 4)
	{
		Span<long> list = stackalloc long[2];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		double inversePow = inversePow10Long(precision);
		value0 = list[0] * inversePow;
		value1 = list[1] * inversePow;
		return success;
	}
	public bool read(out double value0, out double value1, out double value2, int precision = 3)
	{
		Span<long> list = stackalloc long[3];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		double inversePow = inversePow10Long(precision);
		value0 = list[0] * inversePow;
		value1 = list[1] * inversePow;
		value2 = list[2] * inversePow;
		return success;
	}
	public bool read(out double value0, out double value1, out double value2, out double value3, int precision = 3)
	{
		Span<long> list = stackalloc long[4];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		double inversePow = inversePow10Long(precision);
		value0 = list[0] * inversePow;
		value1 = list[1] * inversePow;
		value2 = list[2] * inversePow;
		value3 = list[3] * inversePow;
		return success;
	}
	public bool read(ref Span<double> values, int precision = 4)
	{
		int count = values.Length;
		Span<long> list = stackalloc long[count];
		bool success = readBitList(mBuffer, mBufferSize, ref mBitIndex, ref list);
		double inversePow = inversePow10Long(precision);
		for (int i = 0; i < count; ++i)
		{
			values[i] = list[i] * inversePow;
		}
		return success;
	}
	public bool read(out Vector2 value, int precision = 3)
	{
		return read(out value.x, out value.y, precision);
	}
	public bool read(out Vector2Int value)
	{
		bool result = read(out int value0, out int value1);
		value = new Vector2Int(value0, value1);
		return result;
	}
	public bool read(out Vector2IntMy value)
	{
		bool result = read(out int value0, out int value1);
		value = new Vector2IntMy(value0, value1);
		return result;
	}
	public bool read(out Vector2Short value)
	{
		return read(out value.x, out value.y);
	}
	public bool read(out Vector2UShort value)
	{
		return read(out value.x, out value.y);
	}
	public bool read(out Vector2UInt value)
	{
		return read(out value.x, out value.y);
	}
	public bool read(out Vector3 value, int precision = 3)
	{
		return read(out value.x, out value.y, out value.z, precision);
	}
	public bool read(out Vector3Int value)
	{
		bool result = read(out int value0, out int value1, out int value2);
		value = new Vector3Int(value0, value1, value2);
		return result;
	}
	public bool read(out Vector4 value, int precision = 3)
	{
		return read(out value.x, out value.y, out value.z, out value.w, precision);
	}
	public bool readCustomList<T>(List<T> list) where T : SerializableBit, new()
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		bool success = true;
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			CLASS(out T value);
			success = value.read(this) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<byte> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<sbyte> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<short> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<ushort> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<int> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<uint> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<long> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<ulong> list)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list);
	}
	public bool readList(List<float> list, int precision = 3)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list, precision);
	}
	public bool readList(List<double> list, int precision = 4)
	{
		return readBitList(mBuffer, mBufferSize, ref mBitIndex, list, precision);
	}
	public bool readList(List<Vector2> list, int precision = 3)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector2 value, precision) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector2UShort> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector2UShort value) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector2Int> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector2Int value) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector3> list, int precision = 3)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector3 value, precision) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<Vector4> list, int precision = 3)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		bool success = true;
		for (int i = 0; i < elementCount; ++i)
		{
			success = read(out Vector4 value, precision) && success;
			list.Add(value);
		}
		return success;
	}
	public bool readList(List<string> list)
	{
		// 先读入列表长度
		if (!read(out int elementCount))
		{
			return false;
		}
		if (elementCount == 0)
		{
			return true;
		}
		list.Clear();
		list.Capacity = elementCount;
		for (int i = 0; i < elementCount; ++i)
		{
			if (!readString(out string value))
			{
				return false;
			}
			list.Add(value);
		}
		return true;
	}
	// 返回值表示是否读取完全
	public bool readString(byte[] str, int strBufferSize)
	{
		// 先读入字符串长度
		if (!read(out int readLen))
		{
			return false;
		}
		// 如果字符串长度为0,则不做任何改变
		if (readLen == 0)
		{
			return true;
		}
		bool result = readBuffer(str, readLen);
		str[getMin(readLen, strBufferSize) - 1] = 0;
		return result;
	}
	public bool readBuffer(byte[] buffer, int readLength)
	{
		if (readLength == 0)
		{
			return true;
		}
		int byteIndex = getReadByteCount();
		mBitIndex = (byteIndex + readLength) << 3;
		// 如果存放数据的空间大小不足以放入当前要读取的数据,则只拷贝能容纳的长度,但是下标应该正常跳转
		memcpy(buffer, mBuffer, 0, byteIndex, getMin(buffer.Length, readLength));
		return readLength <= buffer.Length;
	}
	public void skipToByteEnd() { mBitIndex = bitCountToByteCount(mBitIndex) << 3; }
	// 返回值表示是否读取完全
	public bool readString(out string value, Encoding encoding = null)
	{
		value = null;
		// 先读入字符串长度
		if (!read(out int readLen))
		{
			return false;
		}
		// 如果字符串长度为0,则不做任何改变
		if (readLen == 0)
		{
			value = EMPTY;
			return true;
		}

		int byteIndex = getReadByteCount();
		mBitIndex = (byteIndex + readLen) << 3;
		value = bytesToString(mBuffer, byteIndex, readLen, encoding);
		return true;
	}
	public byte[] getBuffer() { return mBuffer; }
	public int getBufferSize() { return mBufferSize; }
	public int getBitIndex() { return mBitIndex; }
	// 获取已读取的字节数,最后一个字节不一定已经读完全部位
	public int getReadByteCount() { return bitCountToByteCount(mBitIndex); }
}