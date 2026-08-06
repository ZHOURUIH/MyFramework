using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// 补间系统单元测试 — TweenGroup(纯逻辑) + TweenTrack(纯字段方法)
// TweenGroup.getGroupLength/hasSelfValueType 不依赖 Unity/全局单例, 纯逻辑可测
// TweenTrack 的 setBeginTime/getBeginTime/setEndTime/getEndTime/setCurveID/isPlaying/stop 为纯字段逻辑
// (play/getTargetValue 依赖 Transform, 本测试仅验证 play 对 mPlaying/mRuntimeStart 的字段写入)
public static class TweenGroupTest
{
	public static void Run()
	{
		// ─── TweenGroup ───
		testGroup_EmptyLength();
		testGroup_Length_SingleTrack();
		testGroup_Length_DisabledTrack();
		testGroup_Length_MultipleTracks();
		testGroup_HasSelfValueType_None();
		testGroup_HasSelfValueType_StartSelf();
		testGroup_HasSelfValueType_TargetSelf();
		// ─── TweenTrack ───
		testTrack_BeginEndTime();
		testTrack_SetCurveID();
		testTrack_IsPlaying_Stop();
		testTrack_Play_Fields();
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 空组长度 0
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_EmptyLength()
	{
		TweenGroup group = new TweenGroup();
		assertEqual(0.0f, group.getGroupLength(), "空组长度应为 0");
		assertFalse(group.hasSelfValueType(), "空组不含 SELF 模式");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 单轨道长度 = duration + startDelay
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_Length_SingleTrack()
	{
		TweenGroup group = new TweenGroup();
		TweenTrack track = new TweenTrack();
		track.mEnable = true;
		track.mDuration = 2.0f;
		track.mStartDelay = 0.5f;
		group.mTrackList.Add(track);
		assertEqual(2.5f, group.getGroupLength(), "单轨道长度应为 duration+delay");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 禁用轨道不计入长度
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_Length_DisabledTrack()
	{
		TweenGroup group = new TweenGroup();
		TweenTrack enabled = new TweenTrack();
		enabled.mEnable = true;
		enabled.mDuration = 1.0f;
		enabled.mStartDelay = 0.0f;
		TweenTrack disabled = new TweenTrack();
		disabled.mEnable = false;
		disabled.mDuration = 100.0f;
		disabled.mStartDelay = 50.0f;
		group.mTrackList.Add(enabled);
		group.mTrackList.Add(disabled);
		assertEqual(1.0f, group.getGroupLength(), "禁用轨道不参与长度计算");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 多轨道长度累加
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_Length_MultipleTracks()
	{
		TweenGroup group = new TweenGroup();
		TweenTrack t1 = new TweenTrack();
		t1.mEnable = true;
		t1.mDuration = 1.0f;
		t1.mStartDelay = 0.5f;
		TweenTrack t2 = new TweenTrack();
		t2.mEnable = true;
		t2.mDuration = 3.0f;
		t2.mStartDelay = 0.0f;
		group.mTrackList.Add(t1);
		group.mTrackList.Add(t2);
		// 1.0+0.5 + 3.0+0.0 = 4.5
		assertEqual(4.5f, group.getGroupLength(), "多轨道长度应为各轨道(duration+delay)之和");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 无 SELF 模式
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_HasSelfValueType_None()
	{
		TweenGroup group = new TweenGroup();
		TweenTrack t1 = new TweenTrack();
		t1.mStartMode = START_MODE.VALUE;
		t1.mTargetMode = TARGET_MODE.VALUE;
		TweenTrack t2 = new TweenTrack();
		t2.mStartMode = START_MODE.VALUE;
		t2.mTargetMode = TARGET_MODE.TRANSFORM_REALTIME;
		group.mTrackList.Add(t1);
		group.mTrackList.Add(t2);
		assertFalse(group.hasSelfValueType(), "无 SELF 模式轨道返回 false");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 起始模式为 SELF
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_HasSelfValueType_StartSelf()
	{
		TweenGroup group = new TweenGroup();
		TweenTrack track = new TweenTrack();
		track.mStartMode = START_MODE.SELF;
		track.mTargetMode = TARGET_MODE.VALUE;
		group.mTrackList.Add(track);
		assertTrue(group.hasSelfValueType(), "起始模式为 SELF 返回 true");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenGroup — 目标模式为 SELF
	// ═════════════════════════════════════════════════════════════════
	private static void testGroup_HasSelfValueType_TargetSelf()
	{
		TweenGroup group = new TweenGroup();
		TweenTrack track = new TweenTrack();
		track.mStartMode = START_MODE.VALUE;
		track.mTargetMode = TARGET_MODE.SELF;
		group.mTrackList.Add(track);
		assertTrue(group.hasSelfValueType(), "目标模式为 SELF 返回 true");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenTrack — 开始/结束时间读写
	// ═════════════════════════════════════════════════════════════════
	private static void testTrack_BeginEndTime()
	{
		TweenTrack track = new TweenTrack();
		assertEqual(0.0f, track.getBeginTime(), "默认开始时间 0");
		assertEqual(0.0f, track.getEndTime(), "默认结束时间 0");
		track.setBeginTime(1.5f);
		track.setEndTime(3.0f);
		assertEqual(1.5f, track.getBeginTime(), "setBeginTime 后开始时间正确");
		assertEqual(3.0f, track.getEndTime(), "setEndTime 后结束时间正确");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenTrack — 设置曲线 ID
	// ═════════════════════════════════════════════════════════════════
	private static void testTrack_SetCurveID()
	{
		TweenTrack track = new TweenTrack();
		track.setCurveID(7);
		// 相同 ID 再次设置不应清缓存
		track.setCurveID(7);
		// 不同 ID 设置时若 mCurve 非空会清缓存; mCurve 此时为 null, 直接覆盖
		track.setCurveID(8);
		// getCurve 在 mKeyFrameManager 为 null 时走 EditorCurveFactory, 此处不测
		// 仅验证 setCurveID 不抛异常
		assertTrue(true, "setCurveID 正常执行");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenTrack — 播放状态切换
	// ═════════════════════════════════════════════════════════════════
	private static void testTrack_IsPlaying_Stop()
	{
		TweenTrack track = new TweenTrack();
		assertFalse(track.isPlaying(), "初始不在播放");
		track.stop();
		assertFalse(track.isPlaying(), "stop 后仍不在播放");
	}

	// ═════════════════════════════════════════════════════════════════
	// TweenTrack — play 写入字段(不验证 Transform 计算)
	// ═════════════════════════════════════════════════════════════════
	private static void testTrack_Play_Fields()
	{
		TweenTrack track = new TweenTrack();
		track.mType = TWEEN_TYPE.MOVE;
		track.mStartMode = START_MODE.VALUE;
		track.mTargetMode = TARGET_MODE.VALUE;
		track.mStartValue = new Vector3(1.0f, 2.0f, 3.0f);
		// 用 dummy Transform 调用 play, 仅验证 mPlaying 写入
		GameObject go = new GameObject();
		try
		{
			Transform transform = go.transform;
			track.play(transform);
			assertTrue(track.isPlaying(), "play 后进入播放状态");
			// VALUE 模式 mRuntimeStart = getParentAnchorScale(transform).multi(mStartValue)
			// getParentAnchorScale 在无 ScaleAnchor 父节点时返回 zero, 故 mRuntimeStart 为 zero
			assertEqual(Vector3.zero, track.getStartValue(), "无 ScaleAnchor 时 mRuntimeStart 为 zero");
			track.stop();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
