using System.Collections.Generic;
using static BinaryUtility;
using static UnityUtility;
using static TestAssert;

// NetPacketBit 序列化参数测试
// 测试 BIT_INT / BIT_BOOL / BIT_FLOAT 参数的 write / read 往返正确性
// 以及 NetPacketBit.hasSign / resetProperty 等行为
public static class NetPacketBitTest
{
	public static void Run()
	{
		testBitInt_WriteRead();
		testBitBool_WriteRead();
		testBitFloat_WriteRead();
		testMixedPacket_WriteRead();
		testPacketReset();
		testBitInt_ResetProperty();
		testBitBool_ResetProperty();
		// SCPackItem 风格测试
		testSCPackItem_AllFields();
		testSCPackItem_PartialFields();
		testSCPackItem_NegativeIDs();
		testSCPackItem_EmptyLists();
		testSCPackItem_PropertyChangeOnly();
		testSCPackItem_ResetProperty();
		testSCPackItem_LargeList();
	}

	// ─── BIT_INT ──────────────────────────────────────────────────────────

	private static void testBitInt_WriteRead()
	{
		var param = new BIT_INT();
		param.set(12345);

		// 通过 write/read 接口往返
		var w = new SerializerBitWrite();
		param.write(w, true);   // needWriteSign=true（有符号整数）

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var outParam = new BIT_INT();
		outParam.read(r, true);

		assertEqual(12345, outParam.mValue, "BIT_INT write/read: 正整数往返应相等");

		// 负数
		param.set(-999);
		var w2 = new SerializerBitWrite();
		param.write(w2, true);
		var r2 = new SerializerBitRead();
		r2.init(w2.getBuffer(), w2.getByteCount());
		var outParam2 = new BIT_INT();
		outParam2.read(r2, true);
		assertEqual(-999, outParam2.mValue, "BIT_INT write/read: 负整数往返应相等");
	}

	// ─── BIT_BOOL ─────────────────────────────────────────────────────────

	private static void testBitBool_WriteRead()
	{
		// true
		var param = new BIT_BOOL();
		param.set(true);
		var w = new SerializerBitWrite();
		param.write(w, false);  // bool needWriteSign 参数无效，但接口仍需传

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var outParam = new BIT_BOOL();
		outParam.read(r, false);
		assert(outParam.mValue, "BIT_BOOL write/read: true 往返应为 true");

		// false
		var param2 = new BIT_BOOL();
		param2.set(false);
		var w2 = new SerializerBitWrite();
		param2.write(w2, false);
		var r2 = new SerializerBitRead();
		r2.init(w2.getBuffer(), w2.getByteCount());
		var outParam2 = new BIT_BOOL();
		outParam2.read(r2, false);
		assert(!outParam2.mValue, "BIT_BOOL write/read: false 往返应为 false");
	}

	// ─── BIT_FLOAT ────────────────────────────────────────────────────────

	private static void testBitFloat_WriteRead()
	{
		var param = new BIT_FLOAT();
		param.set(3.14f);

		var w = new SerializerBitWrite();
		param.write(w, false);  // needWriteSign=false（正数）

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var outParam = new BIT_FLOAT();
		outParam.read(r, false);

		// 精度为默认 3 位小数，1e-3 容差
		float diff = outParam.mValue - 3.14f;
		if (diff < 0) diff = -diff;
		assert(diff < 1e-3f, $"BIT_FLOAT write/read: 期望约 3.14，实际 {outParam.mValue}");
	}

	// ─── 混合参数包往返 ───────────────────────────────────────────────────

	private static void testMixedPacket_WriteRead()
	{
		var packet = new TestBitPacket();
		packet.mIntField.set(777);
		packet.mBoolField.set(true);
		packet.mFloatField.set(-1.5f);

		// 写入
		var w = new SerializerBitWrite();
		packet.write(w, true, out ulong fieldFlag);

		// 读取
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var readPacket = new TestBitPacket();
		readPacket.read(r, true, fieldFlag);

		assertEqual(777,   readPacket.mIntField.mValue,  "混合包: int 字段往返应相等");
		assert(readPacket.mBoolField.mValue,              "混合包: bool 字段往返应为 true");
		float diff = readPacket.mFloatField.mValue - (-1.5f);
		if (diff < 0) diff = -diff;
		assert(diff < 1e-3f, $"混合包: float 字段往返误差应 < 1e-3，实际 {readPacket.mFloatField.mValue}");
	}

	// ─── resetProperty ───────────────────────────────────────────────────

	private static void testPacketReset()
	{
		var packet = new TestBitPacket();
		packet.mIntField.set(100);
		packet.mBoolField.set(true);
		packet.mFloatField.set(9.9f);

		packet.resetProperty();

		assertEqual(0,     packet.mIntField.mValue,   "resetProperty: int 字段应归零");
		assert(!packet.mBoolField.mValue,             "resetProperty: bool 字段应为 false");
		assertEqual(0.0f,  packet.mFloatField.mValue, "resetProperty: float 字段应归零");
	}

	// ─── BIT_INT resetProperty ────────────────────────────────────────────

	private static void testBitInt_ResetProperty()
	{
		var p = new BIT_INT();
		p.set(9999);
		p.resetProperty();
		assertEqual(0, p.mValue, "BIT_INT resetProperty: mValue 应归零");
		assert(p.mValid, "BIT_INT resetProperty: mValid 应恢复为 true");
	}

	// ─── BIT_BOOL resetProperty ───────────────────────────────────────────

	private static void testBitBool_ResetProperty()
	{
		var p = new BIT_BOOL();
		p.set(true);
		p.resetProperty();
		assert(!p.mValue, "BIT_BOOL resetProperty: mValue 应为 false");
		assert(p.mValid,  "BIT_BOOL resetProperty: mValid 应恢复为 true");
	}

	// ═══════════════════════════════════════════════════════════════════════
	// SCPackItem 风格序列化测试
	// 覆盖：全字段/部分字段/负数ID/空列表/仅属性变化/resetProperty/大列表
	// ═══════════════════════════════════════════════════════════════════════

	// 全字段写入，read 还原后所有值正确
	private static void testSCPackItem_AllFields()
	{
		var pkt = new TestSCPackItemPacket();
		pkt.mQuickItemIDList.add(1001L);
		pkt.mQuickItemIDList.add(1002L);
		pkt.mItemIDList.add(2001L);
		pkt.mItemIDList.add(2002L);
		pkt.mItemIDList.add(2003L);
		pkt.mPackSize.set(50);
		pkt.mPropertyChangeOnly.set(false);

		var (readPkt, ok) = writeRead(pkt);
		assert(ok, "SCPackItem 全字段: write/read 应成功");
		assertEqual(2, readPkt.mQuickItemIDList.Count,        "SCPackItem 全字段: quickItems 数量=2");
		assertEqual(1001L, readPkt.mQuickItemIDList[0],       "SCPackItem 全字段: quickItems[0]=1001");
		assertEqual(1002L, readPkt.mQuickItemIDList[1],       "SCPackItem 全字段: quickItems[1]=1002");
		assertEqual(3, readPkt.mItemIDList.Count,             "SCPackItem 全字段: items 数量=3");
		assertEqual(2001L, readPkt.mItemIDList[0],            "SCPackItem 全字段: items[0]=2001");
		assertEqual(2002L, readPkt.mItemIDList[1],            "SCPackItem 全字段: items[1]=2002");
		assertEqual(2003L, readPkt.mItemIDList[2],            "SCPackItem 全字段: items[2]=2003");
		assertEqual((ushort)50, readPkt.mPackSize.mValue,     "SCPackItem 全字段: packSize=50");
		assert(!readPkt.mPropertyChangeOnly.mValue,           "SCPackItem 全字段: propertyChangeOnly=false");
	}

	// 只有 packSize + propertyChangeOnly 有效，两个列表 mValid=false（可选字段未写入）
	private static void testSCPackItem_PartialFields()
	{
		var pkt = new TestSCPackItemPacket();
		// quickItemIDList 和 itemIDList 保持 mValid=false（不调用 add，不 set mValid）
		pkt.mQuickItemIDList.mValid = false;
		pkt.mItemIDList.mValid = false;
		pkt.mPackSize.set(30);
		pkt.mPropertyChangeOnly.set(false);

		var (readPkt, ok) = writeRead(pkt);
		assert(ok, "SCPackItem 部分字段: write/read 应成功");
		assertEqual(0, readPkt.mQuickItemIDList.Count,        "SCPackItem 部分字段: quickItems 未写入应为空");
		assertEqual(0, readPkt.mItemIDList.Count,             "SCPackItem 部分字段: items 未写入应为空");
		assertEqual((ushort)30, readPkt.mPackSize.mValue,     "SCPackItem 部分字段: packSize=30");
		assert(!readPkt.mPropertyChangeOnly.mValue,           "SCPackItem 部分字段: propertyChangeOnly=false");
	}

	// 含负数 ID —— 触发 hasSign=true，needWriteSign/needReadSign=true 路径
	private static void testSCPackItem_NegativeIDs()
	{
		var pkt = new TestSCPackItemPacket();
		pkt.mItemIDList.add(-100L);
		pkt.mItemIDList.add(200L);
		pkt.mItemIDList.add(-300L);
		pkt.mPackSize.set(10);
		pkt.mPropertyChangeOnly.set(false);

		assert(pkt.hasSign(), "SCPackItem 负数ID: hasSign 应为 true");

		var (readPkt, ok) = writeRead(pkt);
		assert(ok, "SCPackItem 负数ID: write/read 应成功");
		assertEqual(3, readPkt.mItemIDList.Count,             "SCPackItem 负数ID: items 数量=3");
		assertEqual(-100L, readPkt.mItemIDList[0],            "SCPackItem 负数ID: items[0]=-100");
		assertEqual(200L,  readPkt.mItemIDList[1],            "SCPackItem 负数ID: items[1]=200");
		assertEqual(-300L, readPkt.mItemIDList[2],            "SCPackItem 负数ID: items[2]=-300");
	}

	// 空列表（Count=0）应能正常序列化，不崩溃
	private static void testSCPackItem_EmptyLists()
	{
		var pkt = new TestSCPackItemPacket();
		// 列表 mValid=true 但不添加任何元素
		pkt.mPackSize.set(0);
		pkt.mPropertyChangeOnly.set(false);

		var (readPkt, ok) = writeRead(pkt);
		assert(ok, "SCPackItem 空列表: write/read 应成功");
		assertEqual(0, readPkt.mQuickItemIDList.Count, "SCPackItem 空列表: quickItems=0");
		assertEqual(0, readPkt.mItemIDList.Count,      "SCPackItem 空列表: items=0");
		assertEqual((ushort)0, readPkt.mPackSize.mValue, "SCPackItem 空列表: packSize=0");
	}

	// 仅属性变化模式：两个列表 mValid=false，propertyChangeOnly=true
	private static void testSCPackItem_PropertyChangeOnly()
	{
		var pkt = new TestSCPackItemPacket();
		pkt.mQuickItemIDList.mValid = false;
		pkt.mItemIDList.mValid = false;
		pkt.mPackSize.mValid = false;
		pkt.mPropertyChangeOnly.set(true);

		assert(!pkt.hasSign(), "SCPackItem PropertyChangeOnly: 无负数，hasSign=false");

		var (readPkt, ok) = writeRead(pkt);
		assert(ok, "SCPackItem PropertyChangeOnly: write/read 应成功");
		assertEqual(0, readPkt.mQuickItemIDList.Count,    "SCPackItem PropertyChangeOnly: quickItems=0");
		assertEqual(0, readPkt.mItemIDList.Count,         "SCPackItem PropertyChangeOnly: items=0");
		assert(readPkt.mPropertyChangeOnly.mValue,        "SCPackItem PropertyChangeOnly: propertyChangeOnly=true");
	}

	// resetProperty 后所有字段归零，mValid 恢复
	private static void testSCPackItem_ResetProperty()
	{
		var pkt = new TestSCPackItemPacket();
		pkt.mQuickItemIDList.add(999L);
		pkt.mItemIDList.add(888L);
		pkt.mPackSize.set(100);
		pkt.mPropertyChangeOnly.set(true);

		pkt.resetProperty();

		assertEqual(0, pkt.mQuickItemIDList.Count,         "SCPackItem Reset: quickItems 清空");
		assertEqual(0, pkt.mItemIDList.Count,              "SCPackItem Reset: items 清空");
		assertEqual((ushort)0, pkt.mPackSize.mValue,       "SCPackItem Reset: packSize=0");
		assert(!pkt.mPropertyChangeOnly.mValue,            "SCPackItem Reset: propertyChangeOnly=false");
		assert(pkt.mQuickItemIDList.mValid,                "SCPackItem Reset: quickItems mValid 恢复");
		assert(pkt.mPackSize.mValid,                       "SCPackItem Reset: packSize mValid 恢复");
	}

	// 多元素列表（100 个 ID）往返，覆盖 VLQ 大值编码路径（框架 writeCheck 已修正）
	private static void testSCPackItem_LargeList()
	{
		var pkt = new TestSCPackItemPacket();
		for (int i = 0; i < 100; ++i)
		{
			pkt.mItemIDList.add((long)(i * 12345678L));
		}
		pkt.mPackSize.set(200);
		pkt.mPropertyChangeOnly.set(false);

		var (readPkt, ok) = writeRead(pkt);
		assert(ok, "SCPackItem 大列表: write/read 应成功");
		assertEqual(100, readPkt.mItemIDList.Count, "SCPackItem 大列表: items 数量=100");
		for (int i = 0; i < 100; ++i)
		{
			assertEqual((long)(i * 12345678L), readPkt.mItemIDList[i], $"SCPackItem 大列表: items[{i}]");
		}
	}

	// ─── 辅助：write→read 封装 ───────────────────────────────────────────
	private static (TestSCPackItemPacket, bool) writeRead(TestSCPackItemPacket pkt)
	{
		bool needSign = pkt.hasSign();
		var w = new SerializerBitWrite();
		pkt.write(w, needSign, out ulong fieldFlag);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var readPkt = new TestSCPackItemPacket();
		bool ok = readPkt.read(r, needSign, fieldFlag);
		return (readPkt, ok);
	}
}

// ─── 测试专用具体 NetPacketBit 子类 ─────────────────────────────────────────

public class TestBitPacket : NetPacketBit
{
	public BIT_INT   mIntField   = new();
	public BIT_BOOL  mBoolField  = new();
	public BIT_FLOAT mFloatField = new();

	public TestBitPacket()
	{
		// 注册三个非可选参数
		addParam(mIntField,   false);
		addParam(mBoolField,  false);
		addParam(mFloatField, false);
	}

	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		mIntField.write(writer, needWriteSign);
		mBoolField.write(writer, needWriteSign);
		mFloatField.write(writer, needWriteSign);
	}

	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		mIntField.read(reader, needReadSign);
		mBoolField.read(reader, needReadSign);
		mFloatField.read(reader, needReadSign);
		return true;
	}

	protected override bool generateHasSignInternal()
	{
		return mIntField.mValue < 0 || mFloatField.mValue < 0;
	}
}

// ─── TestSCPackItemPacket：模拟 SCPackItem 结构 ──────────────────────────────
// 字段布局与 SCPackItem 完全一致：
//   bit0 = mQuickItemIDList（可选）
//   bit1 = mItemIDList（可选）
//   bit2 = mPackSize（可选）
//   mPropertyChangeOnly（非可选，始终写入）
public class TestSCPackItemPacket : NetPacketBit
{
	public BIT_LONGS  mQuickItemIDList   = new();
	public BIT_LONGS  mItemIDList        = new();
	public BIT_USHORT mPackSize          = new();
	public BIT_BOOL   mPropertyChangeOnly = new();

	public TestSCPackItemPacket()
	{
		addParam(mQuickItemIDList,    true);  // 可选
		addParam(mItemIDList,         true);  // 可选
		addParam(mPackSize,           true);  // 可选
		addParam(mPropertyChangeOnly, false); // 非可选，始终写入
	}

	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		if (mQuickItemIDList.mValid)
		{
			setBitOne(ref fieldFlag, 0);
			mQuickItemIDList.write(writer, needWriteSign);
		}
		if (mItemIDList.mValid)
		{
			setBitOne(ref fieldFlag, 1);
			mItemIDList.write(writer, needWriteSign);
		}
		if (mPackSize.mValid)
		{
			setBitOne(ref fieldFlag, 2);
			mPackSize.write(writer, needWriteSign);
		}
		mPropertyChangeOnly.write(writer, needWriteSign);
	}

	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		bool success = true;
		if (hasBit(fieldFlag, 0))
		{
			success = success && mQuickItemIDList.read(reader, needReadSign);
		}
		if (hasBit(fieldFlag, 1))
		{
			success = success && mItemIDList.read(reader, needReadSign);
		}
		if (hasBit(fieldFlag, 2))
		{
			success = success && mPackSize.read(reader, needReadSign);
		}
		success = success && mPropertyChangeOnly.read(reader, needReadSign);
		return success;
	}

	protected override bool generateHasSignInternal()
	{
		foreach (long id in mQuickItemIDList)
		{
			if (id < 0) return true;
		}
		foreach (long id in mItemIDList)
		{
			if (id < 0) return true;
		}
		return false;
	}
}
