using System;
using System.Collections.Generic;
using static SerializeBitUtility;

// 按位序列化关键边界永久回归测试。
// 只保留本轮已经发现过真实问题的边界，避免继续维护大规模诊断型穷举。
public class SerializeBitExtremeTest
{
	public static void Run()
	{
		testULongBoundary();
		testListCountOverflow();
		testTruncatedULong();
	}

	private static void testULongBoundary()
	{
		ulong[] values =
		{
			(1UL << 62) - 1UL,
			1UL << 62,
			(1UL << 62) + 1UL,
			(1UL << 63) - 1UL,
			1UL << 63,
			ulong.MaxValue - 1UL,
			ulong.MaxValue,
		};
		for (int i = 0; i < values.Length; ++i)
		{
			byte[] buffer = new byte[32];
			int writeBitIndex = 0;
			check(writeBit(buffer, buffer.Length, ref writeBitIndex, values[i]), "ulong boundary write,index:" + i);
			check(writeBit(buffer, buffer.Length, ref writeBitIndex, false), "ulong boundary sentinel0 write,index:" + i);
			check(writeBit(buffer, buffer.Length, ref writeBitIndex, true), "ulong boundary sentinel1 write,index:" + i);

			int readBitIndex = 0;
			check(readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out ulong result), "ulong boundary read,index:" + i);
			check(result == values[i], "ulong boundary value,index:" + i);
			check(readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out bool sentinel0) && !sentinel0,
				"ulong boundary sentinel0,index:" + i);
			check(readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out bool sentinel1) && sentinel1,
				"ulong boundary sentinel1,index:" + i);
			check(readBitIndex == writeBitIndex,
				"ulong boundary bitIndex,index:" + i + ",Write:" + writeBitIndex + ",Read:" + readBitIndex);
		}
	}

	private static void testListCountOverflow()
	{
		byte[] buffer = new byte[16];
		int bitIndex = 3;

		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<byte>()), "byte List.Count=65536必须拒绝");
		check(bitIndex == 3, "byte List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<sbyte>(), true), "sbyte List.Count=65536必须拒绝");
		check(bitIndex == 3, "sbyte List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<short>(), true), "short List.Count=65536必须拒绝");
		check(bitIndex == 3, "short List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<ushort>()), "ushort List.Count=65536必须拒绝");
		check(bitIndex == 3, "ushort List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<int>(), true), "int List.Count=65536必须拒绝");
		check(bitIndex == 3, "int List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<uint>()), "uint List.Count=65536必须拒绝");
		check(bitIndex == 3, "uint List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<long>(), true), "long List.Count=65536必须拒绝");
		check(bitIndex == 3, "long List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<ulong>()), "ulong List.Count=65536必须拒绝");
		check(bitIndex == 3, "ulong List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<float>(), true, 3), "float List.Count=65536必须拒绝");
		check(bitIndex == 3, "float List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);

		bitIndex = 3;
		check(!writeListBit(buffer, buffer.Length, ref bitIndex, createOverflowList<double>(), true, 4), "double List.Count=65536必须拒绝");
		check(bitIndex == 3, "double List.Count=65536失败后bitIndex不能变化,Actual:" + bitIndex);
	}

	private static void testTruncatedULong()
	{
		SerializerBitWrite writer = new SerializerBitWrite();
		writer.write(ulong.MaxValue);
		check(writer.getByteCount() > 1, "truncated ulong前置条件");

		SerializerBitRead reader = new SerializerBitRead();
		reader.init(writer.getBuffer(), writer.getByteCount() - 1);
		check(!reader.read(out ulong _), "截断ulong必须读取失败");
	}

	private static List<T> createOverflowList<T>()
	{
		const int count = ushort.MaxValue + 1;
		List<T> list = new List<T>(count);
		for (int i = 0; i < count; ++i)
		{
			list.Add(default(T));
		}
		return list;
	}

	private static int bytesForBits(int bitCount)
	{
		return (bitCount + 7) >> 3;
	}

	private static void check(bool condition, string info)
	{
		if (!condition)
		{
			throw new Exception(info);
		}
	}
}
