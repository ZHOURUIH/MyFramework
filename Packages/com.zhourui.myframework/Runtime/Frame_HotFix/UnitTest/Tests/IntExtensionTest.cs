using static TestAssert;

// IntExtension 中 MathExtensionTest 未覆盖的方法测试
// 已覆盖: clamp, clampMin(int), clampMax(int), clampCycle, inRange, inRangeFixed,
//         generateBatchCount, indexToX/Y, indexToIntPos, hasMask, inversePow10, pow10,
//         inversePow10Long, pow10Long, pow2, inverse(int), divide, divideInt, sqrt,
//         getGreaterPow2, getGreaterPowValue
public static class IntExtensionTest
{
    public static void Run()
    {
        testAbsSbyte();
        testAbsShort();
        testClampMinOverloads();
        testClampMaxOverloads();
    }

    // ---- abs(sbyte) ----
    static void testAbsSbyte()
    {
        assertEqual((sbyte)5, ((sbyte)5).abs(), "abs sbyte 5");
        assertEqual((sbyte)5, ((sbyte)(-5)).abs(), "abs sbyte -5");
        assertEqual((sbyte)0, ((sbyte)0).abs(), "abs sbyte 0");
        assertEqual((sbyte)127, ((sbyte)(-127)).abs(), "abs sbyte -127");
        // sbyte.MinValue = -128, abs = 128 溢出 sbyte 范围, C# 行为: -(-128)=128 但转为 sbyte=-128
        // 但这里按框架行为: -value 对 sbyte 运算, (-128) => (sbyte)(-(-128)) = (sbyte)(128) = -128 (溢出回卷)
        // 不测试溢出行为, 仅测正常范围
    }

    // ---- abs(short) ----
    static void testAbsShort()
    {
        assertEqual((short)5, ((short)5).abs(), "abs short 5");
        assertEqual((short)5, ((short)(-5)).abs(), "abs short -5");
        assertEqual((short)0, ((short)0).abs(), "abs short 0");
        assertEqual((short)1000, ((short)(-1000)).abs(), "abs short -1000");
    }

    // ---- clampMin 重载 (byte/sbyte/short/ushort/uint) ----
    static void testClampMinOverloads()
    {
        // byte
        assertEqual((byte)5, ((byte)5).clampMin((byte)3), "clampMin byte no clamp");
        assertEqual((byte)3, ((byte)2).clampMin((byte)3), "clampMin byte clamped");
        assertEqual((byte)5, ((byte)5).clampMin(), "clampMin byte default 0");

        // sbyte
        assertEqual((sbyte)5, ((sbyte)5).clampMin((sbyte)3), "clampMin sbyte no clamp");
        assertEqual((sbyte)3, ((sbyte)1).clampMin((sbyte)3), "clampMin sbyte clamped");
        assertEqual((sbyte)5, ((sbyte)5).clampMin(), "clampMin sbyte default 0");
        assertEqual((sbyte)0, ((sbyte)(-1)).clampMin(), "clampMin sbyte negative to 0");

        // short
        assertEqual((short)5, ((short)5).clampMin((short)3), "clampMin short no clamp");
        assertEqual((short)3, ((short)1).clampMin((short)3), "clampMin short clamped");
        assertEqual((short)5, ((short)5).clampMin(), "clampMin short default 0");

        // ushort
        assertEqual((ushort)5, ((ushort)5).clampMin((ushort)3), "clampMin ushort no clamp");
        assertEqual((ushort)3, ((ushort)1).clampMin((ushort)3), "clampMin ushort clamped");
        assertEqual((ushort)5, ((ushort)5).clampMin(), "clampMin ushort default 0");

        // uint
        assertEqual(5u, 5u.clampMin(3u), "clampMin uint no clamp");
        assertEqual(3u, 1u.clampMin(3u), "clampMin uint clamped");
        assertEqual(5u, 5u.clampMin(), "clampMin uint default 0");
    }

    // ---- clampMax 重载 (byte/sbyte/short/ushort/uint) ----
    static void testClampMaxOverloads()
    {
        // byte
        assertEqual((byte)5, ((byte)5).clampMax((byte)10), "clampMax byte no clamp");
        assertEqual((byte)10, ((byte)15).clampMax((byte)10), "clampMax byte clamped");

        // sbyte
        assertEqual((sbyte)5, ((sbyte)5).clampMax((sbyte)10), "clampMax sbyte no clamp");
        assertEqual((sbyte)10, ((sbyte)15).clampMax((sbyte)10), "clampMax sbyte clamped");
        assertEqual((sbyte)(-5), ((sbyte)(-5)).clampMax((sbyte)10), "clampMax sbyte negative");

        // short
        assertEqual((short)5, ((short)5).clampMax((short)10), "clampMax short no clamp");
        assertEqual((short)10, ((short)15).clampMax((short)10), "clampMax short clamped");

        // ushort
        assertEqual((ushort)5, ((ushort)5).clampMax((ushort)10), "clampMax ushort no clamp");
        assertEqual((ushort)10, ((ushort)15).clampMax((ushort)10), "clampMax ushort clamped");

        // uint
        assertEqual(5u, 5u.clampMax(10u), "clampMax uint no clamp");
        assertEqual(10u, 15u.clampMax(10u), "clampMax uint clamped");
    }
}
