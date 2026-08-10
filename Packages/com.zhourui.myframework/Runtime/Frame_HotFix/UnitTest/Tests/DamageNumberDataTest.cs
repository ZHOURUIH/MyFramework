using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// DamageNumberData 伤害数字数据单元测试(纯逻辑, 继承 ClassObject 无池依赖)
// setPositionKeyframes/setScaleKeyframes: 字典展开到时间/值数组(null 安全)
// init: 空数组 mKeyFrameMaxTime=1.0f; 有数组取最后时间; mPosition/mScale 取 offset
// cloneTo: 全字段复制
// 注意: Dictionary 遍历顺序不保证, 展开数组断言"包含值"而非特定下标
public static class DamageNumberDataTest
{
	public static void Run()
	{
		testInitEmpty();
		testSetPositionKeyframes();
		testSetScaleKeyframes();
		testInitWithKeyframes();
		testCloneTo();
		testResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// init — 空数组: mKeyFrameMaxTime=1.0f, mPosition=mPositionOffset
	// ═════════════════════════════════════════════════════════════════
	private static void testInitEmpty()
	{
		DamageNumberData data = new DamageNumberData();
		data.mPositionOffset = new Vector3(1.0f, 2.0f, 3.0f);
		data.mScaleOffset = new Vector3(4.0f, 5.0f, 6.0f);
		data.init();
		assertEqual(1.0f, data.mKeyFrameMaxTime, 0.0001f, "空关键帧时 mKeyFrameMaxTime 应为 1.0f");
		assertEqual(new Vector3(1.0f, 2.0f, 3.0f), data.mPosition, "init 后 mPosition = mPositionOffset");
		assertEqual(new Vector3(4.0f, 5.0f, 6.0f), data.mScale, "init 后 mScale = mScaleOffset");
	}

	// ═════════════════════════════════════════════════════════════════
	// setPositionKeyframes — 字典展开到时间/值数组
	// ═════════════════════════════════════════════════════════════════
	private static void testSetPositionKeyframes()
	{
		DamageNumberData data = new DamageNumberData();
		Dictionary<float, Vector3> keyframes = new Dictionary<float, Vector3>();
		keyframes.Add(0.1f, new Vector3(1.0f, 0.0f, 0.0f));
		keyframes.Add(0.5f, new Vector3(0.0f, 1.0f, 0.0f));
		keyframes.Add(1.0f, new Vector3(0.0f, 0.0f, 1.0f));
		data.setPositionKeyframes(keyframes);
		assertEqual(3, data.mPositionKeyFrames.Count, "mPositionKeyFrames 保留原字典");
		assertEqual(3, data.mPositionTimeList.Length, "mPositionTimeList 展开为 3 项");
		assertEqual(3, data.mPositionList.Length, "mPositionList 展开为 3 项");
		// 遍历顺序不保证, 断言所有时间值都存在
		assertTrue(containsTime(data.mPositionTimeList, 0.1f), "mPositionTimeList 含 0.1f");
		assertTrue(containsTime(data.mPositionTimeList, 0.5f), "mPositionTimeList 含 0.5f");
		assertTrue(containsTime(data.mPositionTimeList, 1.0f), "mPositionTimeList 含 1.0f");
	}

	// ═════════════════════════════════════════════════════════════════
	// setScaleKeyframes — 缩放关键帧展开
	// ═════════════════════════════════════════════════════════════════
	private static void testSetScaleKeyframes()
	{
		DamageNumberData data = new DamageNumberData();
		Dictionary<float, Vector3> keyframes = new Dictionary<float, Vector3>();
		keyframes.Add(0.2f, new Vector3(1.0f, 1.0f, 1.0f));
		keyframes.Add(1.0f, new Vector3(2.0f, 2.0f, 2.0f));
		data.setScaleKeyframes(keyframes);
		assertEqual(2, data.mScaleKeyFrames.Count, "mScaleKeyFrames 保留原字典");
		assertEqual(2, data.mScaleTimeList.Length, "mScaleTimeList 展开为 2 项");
		assertEqual(2, data.mScaleList.Length, "mScaleList 展开为 2 项");
		assertTrue(containsTime(data.mScaleTimeList, 0.2f), "mScaleTimeList 含 0.2f");
		assertTrue(containsTime(data.mScaleTimeList, 1.0f), "mScaleTimeList 含 1.0f");
	}

	// ═════════════════════════════════════════════════════════════════
	// init — 有关键帧: mKeyFrameMaxTime 取 position 最后时间(非 1.0f)
	// ═════════════════════════════════════════════════════════════════
	private static void testInitWithKeyframes()
	{
		DamageNumberData data = new DamageNumberData();
		// 相同字典 → 展开顺序一致 → position/scale 最后时间一致 → 不触发 logError
		Dictionary<float, Vector3> keyframes = new Dictionary<float, Vector3>();
		keyframes.Add(0.1f, new Vector3(1.0f, 0.0f, 0.0f));
		keyframes.Add(1.0f, new Vector3(0.0f, 1.0f, 0.0f));
		data.setPositionKeyframes(keyframes);
		data.setScaleKeyframes(keyframes);
		data.init();
		// 最后时间等于字典中的某个 key(顺序不保证, 但非空数组时不为 1.0f 默认值)
		assertTrue(data.mKeyFrameMaxTime == 0.1f || data.mKeyFrameMaxTime == 1.0f,
			"mKeyFrameMaxTime 取 position 最后时间(0.1 或 1.0)");
	}

	// ═════════════════════════════════════════════════════════════════
	// cloneTo — 全字段复制
	// ═════════════════════════════════════════════════════════════════
	private static void testCloneTo()
	{
		DamageNumberData src = new DamageNumberData();
		Dictionary<float, Vector3> posKf = new Dictionary<float, Vector3>();
		posKf.Add(0.5f, new Vector3(1.0f, 2.0f, 3.0f));
		src.setPositionKeyframes(posKf);
		src.mPositionOffset = new Vector3(10.0f, 20.0f, 30.0f);
		src.mScaleOffset = new Vector3(1.0f, 1.0f, 1.0f);
		src.mPosition = new Vector3(5.0f, 5.0f, 5.0f);
		src.mScale = new Vector3(2.0f, 2.0f, 2.0f);
		src.mNumbers.Add(7);
		src.mSpeed = 2.5f;
		src.mCurTime = 1.5f;
		src.mTotalWidth = 100.0f;
		src.mKeyFrameMaxTime = 3.0f;

		DamageNumberData dst = new DamageNumberData();
		src.cloneTo(dst);
		assertTrue(ReferenceEquals(src.mPositionKeyFrames, dst.mPositionKeyFrames), "cloneTo 复制 mPositionKeyFrames 引用");
		assertEqual(new Vector3(10.0f, 20.0f, 30.0f), dst.mPositionOffset, "cloneTo 复制 mPositionOffset");
		assertEqual(new Vector3(5.0f, 5.0f, 5.0f), dst.mPosition, "cloneTo 复制 mPosition");
		assertEqual(7, dst.mNumbers[0], "cloneTo 复制 mNumbers 内容");
		assertEqual(2.5f, dst.mSpeed, 0.0001f, "cloneTo 复制 mSpeed");
		assertEqual(1.5f, dst.mCurTime, 0.0001f, "cloneTo 复制 mCurTime");
		assertEqual(100.0f, dst.mTotalWidth, 0.0001f, "cloneTo 复制 mTotalWidth");
		assertEqual(3.0f, dst.mKeyFrameMaxTime, 0.0001f, "cloneTo 复制 mKeyFrameMaxTime");
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty — 清空关键帧与状态
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty()
	{
		DamageNumberData data = new DamageNumberData();
		Dictionary<float, Vector3> keyframes = new Dictionary<float, Vector3>();
		keyframes.Add(0.5f, new Vector3(1.0f, 0.0f, 0.0f));
		data.setPositionKeyframes(keyframes);
		data.mSpeed = 3.0f;
		data.mCurTime = 2.0f;
		data.resetProperty();
		assertNull(data.mPositionKeyFrames, "resetProperty 后 mPositionKeyFrames 为 null");
		assertNull(data.mScaleKeyFrames, "resetProperty 后 mScaleKeyFrames 为 null");
		assertEqual(1.0f, data.mSpeed, 0.0001f, "resetProperty 后 mSpeed 恢复默认 1.0f");
		assertEqual(0.0f, data.mCurTime, 0.0001f, "resetProperty 后 mCurTime 归 0");
		assertEqual(0, data.mNumbers.Count, "resetProperty 后 mNumbers 清空");
		assertEqual(Vector3.zero, data.mPosition, "resetProperty 后 mPosition 归零");
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 时间数组是否包含指定值(float 精确匹配, 因展开自同一 float key)
	// ═════════════════════════════════════════════════════════════════
	private static bool containsTime(float[] list, float time)
	{
		if (list == null)
		{
			return false;
		}
		foreach (float t in list)
		{
			if (t == time)
			{
				return true;
			}
		}
		return false;
	}
}
