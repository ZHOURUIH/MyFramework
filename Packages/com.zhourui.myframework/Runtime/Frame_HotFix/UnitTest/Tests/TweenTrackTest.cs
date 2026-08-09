using UnityEngine;
using static TestAssert;

// TweenTrack 单元测试 — 补间轨道的时间线/起止值纯逻辑
// 覆盖 getBeginTime/getEndTime/setBeginTime/setEndTime/setCurveID/getCurve/
//       getTargetValue/getStartValue/play/stop/isPlaying
// 所有测试均用"无父节点"的 Transform,使 TweenTrack.getParentAnchorScale 返回
// Vector3.zero,从而完全避开 getScreenScale(依赖 mScreenSize)等环境依赖,保证确定性。
// GameObject 统一 try-finally 清理,避免断言抛异常时残留。
public static class TweenTrackTest
{
	public static void Run()
	{
		test_BeginEndTime_GetSet();
		test_DefaultFlags();
		test_SetCurveID_Zero_GetCurveNull();
		test_SetCurveID_ChangeCacheInvalidated_Field();
		test_PlayStop_Flag();
		test_Play_StartValueValueMode();
		test_Play_StartValueSelfMode_Move();
		test_Play_RuntimeTarget_SelfMode_Scale();
		test_GetTargetValue_ValueMode_ZeroScale();
		test_GetTargetValue_SnapshotMode_NoPlay();
	}

	// ═════════════════════════════════════════════════════════════════
	// 起止时间纯字段 get/set
	// ═════════════════════════════════════════════════════════════════
	private static void test_BeginEndTime_GetSet()
	{
		TweenTrack t = new TweenTrack();
		try
		{
			assertEqual(0.0f, t.getBeginTime(), "默认起始时间为 0");
			assertEqual(0.0f, t.getEndTime(), "默认结束时间为 0");
			t.setBeginTime(1.5f);
			t.setEndTime(2.5f);
			assertEqual(1.5f, t.getBeginTime(), "setBeginTime 后起始时间更新");
			assertEqual(2.5f, t.getEndTime(), "setEndTime 后结束时间更新");
		}
		finally
		{
			// TweenTrack 是普通类(非 ClassObject),无 destroy,无需清理
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认标志位(不播放/默认曲线ID为0)
	// ═════════════════════════════════════════════════════════════════
	private static void test_DefaultFlags()
	{
		TweenTrack t = new TweenTrack();
		try
		{
			assertFalse(t.isPlaying(), "新轨道默认不在播放");
			assertEqual(0, t.mCurveID, "默认曲线ID为 0");
			assertEqual(true, t.mEnable, "默认轨道启用");
			assertEqual(0.3f, t.mDuration, "默认持续时间为 0.3f");
		}
		finally
		{
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 曲线ID为0时 getCurve 恒为 null(不满足 mCurveID>0, 零外部依赖)
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetCurveID_Zero_GetCurveNull()
	{
		TweenTrack t = new TweenTrack();
		try
		{
			t.setCurveID(0);
			assertNull(t.getCurve(), "曲线ID为0时 getCurve 返回 null");
			// 连续两次调用应返回同一引用(缓存空引用, 无重载副作用)
			MyCurve c1 = t.getCurve();
			MyCurve c2 = t.getCurve();
			assertTrue(ReferenceEquals(c1, c2), "两次 getCurve 返回同一引用");
		}
		finally
		{
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setCurveID 更新字段(源码: ID 变化时清空 mCurve 缓存, ID 不变则保留)
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetCurveID_ChangeCacheInvalidated_Field()
	{
		TweenTrack t = new TweenTrack();
		try
		{
			t.setCurveID(10);
			assertEqual(10, t.mCurveID, "setCurveID 更新 mCurveID 字段");
			t.setCurveID(10);
			assertEqual(10, t.mCurveID, "重复设同一 ID 字段不变");
			t.setCurveID(20);
			assertEqual(20, t.mCurveID, "改设新 ID 字段更新");
		}
		finally
		{
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// play 设置 mPlaying=true, stop 复位为 false
	// ═════════════════════════════════════════════════════════════════
	private static void test_PlayStop_Flag()
	{
		GameObject go = new GameObject();
		try
		{
			Transform tf = go.transform;
			TweenTrack t = new TweenTrack();
			t.play(tf);
			assertTrue(t.isPlaying(), "play 后轨道处于播放状态");
			t.stop();
			assertFalse(t.isPlaying(), "stop 后轨道停止播放");
			// stop 幂等
			t.stop();
			assertFalse(t.isPlaying(), "重复 stop 仍为停止");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// START_MODE.VALUE: 无父节点时 getParentAnchorScale=zero → 起始值=zero
	// ═════════════════════════════════════════════════════════════════
	private static void test_Play_StartValueValueMode()
	{
		GameObject go = new GameObject();
		try
		{
			Transform tf = go.transform;
			TweenTrack t = new TweenTrack();
			t.mStartMode = START_MODE.VALUE;
			t.mStartValue = new Vector3(3.0f, 4.0f, 5.0f);
			t.play(tf);
			// getParentAnchorScale(tf)=zero → zero.multi(mStartValue)=zero
			assertEqual(new Vector3(0.0f, 0.0f, 0.0f), t.getStartValue(), "VALUE 模式无父节点时起始值为 zero");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// START_MODE.SELF + MOVE: 起始值取 transform.localPosition
	// ═════════════════════════════════════════════════════════════════
	private static void test_Play_StartValueSelfMode_Move()
	{
		GameObject go = new GameObject();
		try
		{
			Transform tf = go.transform;
			tf.localPosition = new Vector3(7.0f, 8.0f, 9.0f);
			TweenTrack t = new TweenTrack();
			t.mStartMode = START_MODE.SELF;
			t.mType = TWEEN_TYPE.MOVE;
			t.play(tf);
			assertEqual(new Vector3(7.0f, 8.0f, 9.0f), t.getStartValue(), "SELF 模式起始值取当前 localPosition");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// TARGET_MODE.SELF + SCALE: play 后 mRuntimeTarget=localScale(+zero偏移)
	// ═════════════════════════════════════════════════════════════════
	private static void test_Play_RuntimeTarget_SelfMode_Scale()
	{
		GameObject go = new GameObject();
		try
		{
			Transform tf = go.transform;
			tf.localScale = new Vector3(2.0f, 3.0f, 4.0f);
			TweenTrack t = new TweenTrack();
			t.mTargetMode = TARGET_MODE.SELF;
			t.mType = TWEEN_TYPE.SCALE;
			t.play(tf);
			// generateTargetValue: getTransformValue=localScale + zero.multi(mTargetOffset)=zero
			// SELF 模式 getTargetValue 直接返回 mRuntimeTarget
			assertEqual(new Vector3(2.0f, 3.0f, 4.0f), t.getTargetValue(tf), "SELF 模式目标值为 localScale");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// TARGET_MODE.VALUE: 无父节点 → zero.multi(mTargetValue)=zero
	// ═════════════════════════════════════════════════════════════════
	private static void test_GetTargetValue_ValueMode_ZeroScale()
	{
		GameObject go = new GameObject();
		try
		{
			Transform tf = go.transform;
			TweenTrack t = new TweenTrack();
			t.mTargetMode = TARGET_MODE.VALUE;
			t.mTargetValue = new Vector3(6.0f, 7.0f, 8.0f);
			assertEqual(new Vector3(0.0f, 0.0f, 0.0f), t.getTargetValue(tf), "VALUE 模式无父节点时目标值为 zero");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// TARGET_MODE.TRANSFORM_SNAPSHOT 未 play 时返回默认 mRuntimeTarget=zero
	// ═════════════════════════════════════════════════════════════════
	private static void test_GetTargetValue_SnapshotMode_NoPlay()
	{
		GameObject go = new GameObject();
		try
		{
			Transform tf = go.transform;
			TweenTrack t = new TweenTrack();
			t.mTargetMode = TARGET_MODE.TRANSFORM_SNAPSHOT;
			// 未 play, mRuntimeTarget 保持默认 zero
			assertEqual(new Vector3(0.0f, 0.0f, 0.0f), t.getTargetValue(tf), "SNAPSHOT 模式未 play 时返回 zero");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
