using System;
using System.Collections.Generic;

// 序列化的单元测试
public struct SerializeTest
{
	public static void test()
	{
		SerializerBitWrite writer = new();
		writer.write(true);
		writer.write(1, false);
		writer.write(-1, true);
		writer.write(0, false);
		writer.write(1231243125, false);
		writer.write(0x7FFFFFFF, false);
		writer.write(1.2f, false);
		writer.write(-2.2f, true);
		writer.write(2.2222, false);
		writer.write(stackalloc sbyte[5] { -1, 2, -3, 4, 0 }, true);
		writer.write((uint)0);
		writer.write((uint)1);
		writer.write((uint)2);
		writer.write(stackalloc int[4] { 1, 2, 3, 4 }, false);
		writer.write(0xFFFFFFFF);
		writer.writeList(new List<int> { 1, 2, 3, 123123123 }, false);
		writer.writeList(new List<int> { 0, 0, 0, 0 }, false);
		writer.writeList(new List<uint> { 0, 0, 0, 0 });
		writer.writeList(new List<uint> { 1, 2, 3, 123123123 });
		writer.writeList(new List<short> { 1, 2, 3 }, false);
		writer.writeList(new List<long> { 1, 2, 3 }, false);
		writer.writeList(new List<byte> { 1, 2, 3 });
		writer.writeList(new List<float> { 1.5f, 2.5f, 3.5f }, false);
		writer.writeList(new List<float> { 1.5f, 2.5f, 312312.5f, 0.0f, 1.0f }, false);
		writer.writeList(new List<double> { 1.555, 2.555, 3.555 }, false);
		writer.writeList(new List<double> { 1.5, 2.5, 312312.51, 0.0, 1.0 }, false);
		writer.writeList(new List<double> { }, false);
		writer.writeList(new List<sbyte> { 1, 2, -3 }, true);

		SerializerBitRead reader = new();
		reader.init(writer.getBuffer(), writer.getByteCount());
		List<int> list0 = new();
		List<int> list00 = new();
		List<uint> list01 = new();
		List<uint> list02 = new();
		List<short> list1 = new();
		List<long> list2 = new();
		List<byte> list3 = new();
		List<float> list4 = new();
		List<float> list40 = new();
		List<double> list5 = new();
		List<double> list50 = new();
		List<double> list6 = new();
		List<sbyte> list7 = new();
		reader.read(out bool value0);
		reader.read(out int value1, false);
		reader.read(out int value2, true);
		reader.read(out int value3, false);
		reader.read(out int value30, false);
		reader.read(out int value31, false);
		reader.read(out float value4, false);
		reader.read(out float value5, true);
		reader.read(out double value6, false);
		Span<sbyte> tempList = stackalloc sbyte[5];
		reader.read(ref tempList, true);
		reader.read(out uint value7);
		reader.read(out uint value8);
		reader.read(out uint value9);
		reader.read(out int intValue0, out int intValue1, out int intValue2, out int intValue3, false);
		reader.read(out uint value90);
		reader.readList(list0, false);
		reader.readList(list00, false);
		reader.readList(list01);
		reader.readList(list02);
		reader.readList(list1, false);
		reader.readList(list2, false);
		reader.readList(list3);
		reader.readList(list4, false);
		reader.readList(list40, false);
		reader.readList(list5, false);
		reader.readList(list50, false);
		reader.readList(list6, false);
		reader.readList(list7, true);

		long mTargetGUID = 0;
		long mSkillFireTimeStamp = 1718813086792;
		List<float> mFloatParam = new(){ 4163, 4155, 2};
		bool mTargetIsMonster = false;
		float mAttackSpeed = 2.05f;
		float mAttackStateTime = -1.0f;
		int mSkillID = 27;
		int mDamageToken = 1;

		SerializerBitWrite writer0 = new();
		writer0.write(stackalloc long[2] { mTargetGUID, mSkillFireTimeStamp }, false);
		writer0.writeList(mFloatParam, false);
		writer0.write(mTargetIsMonster);
		writer0.write(stackalloc float[2] { mAttackSpeed, mAttackStateTime }, true);
		writer0.write(stackalloc int[2] { mSkillID, mDamageToken }, false);

		SerializerBitRead reader0 = new();
		reader0.init(writer0.getBuffer(), writer0.getByteCount());
		reader0.read(out mTargetGUID, out mSkillFireTimeStamp, false);
		reader0.readList(mFloatParam, false);
		reader0.read(out mTargetIsMonster);
		reader0.read(out mAttackSpeed, out mAttackStateTime, true);
		reader0.read(out mSkillID, out mDamageToken, false);
	}
}