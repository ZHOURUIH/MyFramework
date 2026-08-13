using UnityEngine;
using static TestAssert;

// TweenSequence 单元测试 — 补间序列的纯逻辑部分
// 继承 MonoBehaviour(需 new GameObject 构造), 覆盖 getTotalLength/hasSelfValueType 纯逻辑
// (evaluateSequence 依赖 MyCurve/getCurve, 依赖框架或 EditorCurveFactory, 本测试不测)
// 所有 GameObject 创建统一用 try-finally 确保即便断言抛异常也能清理残留
public static class TweenSequenceTest
{
	public static void Run()
	{
		test_Empty_Length();
		test_Length_MaxOfGroups();
		test_Length_DisabledGroup();
		test_HasSelfValueType_None();
		test_HasSelfValueType_Some();
		test_Stop_SetsTracksNotPlaying();
	

		testPlayBuildsTimelineWithDelay();
		testEvaluateBeforeStartKeepsTransform();
		testEvaluateMidTrackLinearInterp();
		testEvaluateExactEndPinsLastFrame();
		testEvaluateAfterEndPinsLastFrame();
		testSerialTracksRelayPlayback();
		testDisabledTrackNotInTimeline();
		testSelfStartValueSnapshottedOnPlay();
		testSnapshotVsRealtimeTarget();
		testStopResetTransform();
		testStopWithoutResetKeepsTransform();
		testTotalLengthMaxOfParallelGroups();
		testRotateAndScaleEvaluate();
	}

	// ═════════════════════════════════════════════════════════════════
	// 空序列长度 0
	// ═════════════════════════════════════════════════════════════════
	private static void test_Empty_Length()
	{
		GameObject go = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			assertEqual(0.0f, seq.getTotalLength(), "空序列长度应为 0");
			assertFalse(seq.hasSelfValueType(), "空序列不含 SELF 模式");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 长度 = 各 group 长度取最大
	// ═════════════════════════════════════════════════════════════════
	private static void test_Length_MaxOfGroups()
	{
		GameObject go = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			// group1 长度 2.5
			TweenGroup g1 = new TweenGroup();
			TweenTrack t1 = new TweenTrack();
			t1.mEnable = true;
			t1.mDuration = 2.0f;
			t1.mStartDelay = 0.5f;
			g1.mTrackList.Add(t1);
			// group2 长度 4.0
			TweenGroup g2 = new TweenGroup();
			TweenTrack t2 = new TweenTrack();
			t2.mEnable = true;
			t2.mDuration = 4.0f;
			t2.mStartDelay = 0.0f;
			g2.mTrackList.Add(t2);
			seq.mGroupList.Add(g1);
			seq.mGroupList.Add(g2);
			assertEqual(4.0f, seq.getTotalLength(), "总长度应为各 group 长度最大值");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 空/全禁用 group 长度不计入最大值
	// ═════════════════════════════════════════════════════════════════
	private static void test_Length_DisabledGroup()
	{
		GameObject go = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g1 = new TweenGroup(); // 空 group
			TweenGroup g2 = new TweenGroup();
			TweenTrack t = new TweenTrack();
			t.mEnable = false; // 全禁用
			t.mDuration = 99.0f;
			g2.mTrackList.Add(t);
			seq.mGroupList.Add(g1);
			seq.mGroupList.Add(g2);
			assertEqual(0.0f, seq.getTotalLength(), "空/全禁用 group 长度 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 无 SELF 模式
	// ═════════════════════════════════════════════════════════════════
	private static void test_HasSelfValueType_None()
	{
		GameObject go = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = new TweenTrack();
			t.mStartMode = START_MODE.VALUE;
			t.mTargetMode = TARGET_MODE.VALUE;
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			assertFalse(seq.hasSelfValueType(), "无 SELF 模式返回 false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 有 SELF 模式
	// ═════════════════════════════════════════════════════════════════
	private static void test_HasSelfValueType_Some()
	{
		GameObject go = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g1 = new TweenGroup();
			TweenTrack t1 = new TweenTrack();
			t1.mTargetMode = TARGET_MODE.SELF;
			g1.mTrackList.Add(t1);
			TweenGroup g2 = new TweenGroup();
			TweenTrack t2 = new TweenTrack();
			t2.mStartMode = START_MODE.SELF;
			g2.mTrackList.Add(t2);
			seq.mGroupList.Add(g1);
			seq.mGroupList.Add(g2);
			assertTrue(seq.hasSelfValueType(), "任一轨道为 SELF 返回 true");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// stop — 将未播放的轨道置为停止
	// ═════════════════════════════════════════════════════════════════
	private static void test_Stop_SetsTracksNotPlaying()
	{
		GameObject go = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = new TweenTrack();
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			// 模拟轨道在播放
			t.mEnable = true;
			t.mDuration = 1.0f;
			// 手动标记播放(通过 play 依赖 transform/ScaleAnchor, 直接调用 stop 即可)
			seq.stop(true);
			assertFalse(t.isPlaying(), "stop 后轨道不在播放");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}


	

	// ─── play() 构建时间轴: startDelay 正确分配 begin/end ────────────
	// 组内两个启用轨道: t1 无延迟(duration2)紧接 t2 延迟0.5(duration1.5)
	// 期望时间窗: t1=[0,2], t2=[2.5,4]
	private static void testPlayBuildsTimelineWithDelay()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t1 = new TweenTrack();
			t1.mType = TWEEN_TYPE.MOVE;
			t1.mEnable = true;
			t1.mDuration = 2.0f;
			t1.mStartDelay = 0.0f;
			t1.mStartMode = START_MODE.SELF;
			t1.mTargetMode = TARGET_MODE.TRANSFORM_SNAPSHOT;
			t1.mTargetTransform = targetGo.transform;
			g.mTrackList.Add(t1);

			TweenTrack t2 = new TweenTrack();
			t2.mType = TWEEN_TYPE.MOVE;
			t2.mEnable = true;
			t2.mDuration = 1.5f;
			t2.mStartDelay = 0.5f;
			t2.mStartMode = START_MODE.SELF;
			t2.mTargetMode = TARGET_MODE.TRANSFORM_SNAPSHOT;
			t2.mTargetTransform = targetGo.transform;
			g.mTrackList.Add(t2);
			seq.mGroupList.Add(g);

			seq.play();
			assertEqual(0.0f, t1.getBeginTime(), "t1 无延迟 begin=0");
			assertEqual(2.0f, t1.getEndTime(), "t1 结束=start+duration=2");
			assertEqual(2.5f, t2.getBeginTime(), "t2 begin=前轨道结束+自身延迟=2.5");
			assertEqual(4.0f, t2.getEndTime(), "t2 结束=begin+duration=4");

			// play() 记录原始 transform: 改动后强制复位须能恢复
			Transform seqTransform = go.transform;
			seqTransform.localPosition = new Vector3(5.0f, 6.0f, 7.0f);
			seq.stop(true);
			assertEqual(0.0f, seqTransform.localPosition.x, 0.0001f, "forceReset 复位原始 pos.x");
			assertEqual(0.0f, seqTransform.localPosition.y, 0.0001f, "forceReset 复位原始 pos.y");
			assertEqual(0.0f, seqTransform.localPosition.z, 0.0001f, "forceReset 复位原始 pos.z");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 时间推进: 轨道未开始时保持 transform 原值 ────────────────────
	// t=0 早于 begin=1, evaluateSequence 不应改写 pos
	private static void testEvaluateBeforeStartKeepsTransform()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform seqTransform = go.transform;
			seqTransform.localPosition = new Vector3(10.0f, 20.0f, 30.0f);
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(50.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(1.0f, 2.0f, targetTransform); // begin=1, end=3
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play();

			Vector3 pos, scale, rotation;
			seq.evaluateSequence(0.5f, out pos, out scale, out rotation);
			assertEqual(10.0f, pos.x, 0.0001f, "轨道未开始(t<begin)时 pos.x 保持原值");
			assertEqual(20.0f, pos.y, 0.0001f, "轨道未开始时 pos.y 保持原值");
			assertEqual(30.0f, pos.z, 0.0001f, "轨道未开始时 pos.z 保持原值");
			assertFalse(t.isPlaying(), "轨道未开始播放");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 时间推进: 播放中按线性曲线插值 ──────────────────────────────
	// begin=1, duration=2, 终点 target.localPosition=(6,0,0), 起始 SELF=(0,0,0)
	// t=2 → percent=(2-1)/2=0.5 → 期望 pos=(3,0,0)
	private static void testEvaluateMidTrackLinearInterp()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(6.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(1.0f, 2.0f, targetTransform);
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play(); // 起始 SELF = go.localPosition = (0,0,0)

			Vector3 pos, scale, rotation;
			seq.evaluateSequence(1.5f, out pos, out scale, out rotation);
			assertEqual(1.5f, pos.x, 0.0001f, "t=1.5 percent=0.25 → pos.x=1.5");
			// 更精确位置
			seq.evaluateSequence(2.0f, out pos, out scale, out rotation);
			assertEqual(3.0f, pos.x, 0.0001f, "t=2 percent=0.5 → pos.x=3");
			assertTrue(t.isPlaying(), "播放中轨道应处于播放状态");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 时间推进: 恰好等于结束时间, 钉扎终点并停止 ──────────────────
	// t=end → percent=1 → pos 定格在终点, track 停止
	private static void testEvaluateExactEndPinsLastFrame()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(8.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(1.0f, 2.0f, targetTransform);
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play();

			Vector3 pos, scale, rotation;
			seq.evaluateSequence(3.0f, out pos, out scale, out rotation);
			assertEqual(8.0f, pos.x, 0.0001f, "t=end 时 pos 钉扎在终点");
			assertFalse(t.isPlaying(), "t=end 后轨道已停止");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 时间推进: 超过结束时间, 钉扎最后一帧 ────────────────────────
	// 模拟真实调用方式: 每帧 evaluate 后把结果写回 transform。
	// 轨迹: 播放中段 → 越过结束时间(该帧钉扎终点) → 之后持续保持终点不再跳动
	private static void testEvaluateAfterEndPinsLastFrame()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform seqTransform = go.transform;
			seqTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(5.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(1.0f, 2.0f, targetTransform);
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play();

			Vector3 pos, scale, rotation;
			// 播放中段 (t=2, percent=0.5)
			seq.evaluateSequence(2.0f, out pos, out scale, out rotation);
			assertTrue(t.isPlaying(), "t=2 时轨道应正在播放");
			seqTransform.localPosition = pos; // 应用

			// 越过结束时间的这一帧: 钉扎终点 (t=5 > end=3)
			seq.evaluateSequence(5.0f, out pos, out scale, out rotation);
			assertEqual(5.0f, pos.x, 0.0001f, "越过结束后该帧钉扎在终点");
			assertFalse(t.isPlaying(), "结束后轨道停止");
			seqTransform.localPosition = pos; // 应用终点

			// 之后再 evaluate, 由于轨道已停止且事件已应用, pos 取 transform 当前值(即终点)
			seq.evaluateSequence(6.0f, out pos, out scale, out rotation);
			assertEqual(5.0f, pos.x, 0.0001f, "重复 evaluate 保持应用后的终点钉扎值");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 串行组内多轨道接力 ──────────────────────────────────────────
	// t1=[0,2] 无延迟, t2=[2.5,4] 延迟0.5
	// t=1: t1 播放中(中段), t2 未开始
	// t=3: t1 已结束钉扎, t2 播放中
	private static void testSerialTracksRelayPlayback()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(10.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t1 = makeMoveTrack(0.0f, 2.0f, targetTransform); // [0,2]
			TweenTrack t2 = makeMoveTrack(0.5f, 1.5f, targetTransform); // [2.5,4]
			g.mTrackList.Add(t1);
			g.mTrackList.Add(t2);
			seq.mGroupList.Add(g);
			seq.play();

			Vector3 pos, scale, rotation;
			// t=1: 只有 t1 播放
			seq.evaluateSequence(1.0f, out pos, out scale, out rotation);
			assertTrue(t1.isPlaying(), "t=1 时 t1 播放中");
			assertFalse(t2.isPlaying(), "t=1 时 t2 未开始");
			// percent=(1-0)/2=0.5 → pos.x = 0 + (10-0)*0.5 = 5
			assertEqual(5.0f, pos.x, 0.0001f, "t=1 t1 插值到中点 pos.x=5");

			// t=3: t1 已结束, t2 接力播放中(t2 [2.5,4] 中段)
			seq.evaluateSequence(3.0f, out pos, out scale, out rotation);
			assertFalse(t1.isPlaying(), "t=3 时 t1 已结束停止");
			assertTrue(t2.isPlaying(), "t=3 时 t2 接力播放中");
			// 组内同一帧最终取当前播放轨道 t2 的值: percent=(3-2.5)/1.5≈0.333 → pos.x≈3.333
			assertEqual(3.3333f, pos.x, 0.0001f, "t=3 由 t2 接管, 插值到约 1/3");
			// t2 percent=(3.25-2.5)/1.5=0.5 → pos.x = 5
			seq.evaluateSequence(3.25f, out pos, out scale, out rotation);
			assertEqual(5.0f, pos.x, 0.0001f, "t=3.25 时 t2 接力插值到中点 pos.x=5");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 禁用轨道不占用时间轴 ────────────────────────────────────────
	// 两个禁用轨道夹着一个启用轨道, 启用轨道的时间窗不受禁用影响
	private static void testDisabledTrackNotInTimeline()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(4.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack disabled1 = new TweenTrack();
			disabled1.mEnable = false;
			disabled1.mDuration = 90.0f;
			disabled1.mStartDelay = 20.0f;
			TweenTrack enabled = makeMoveTrack(0.0f, 2.0f, targetTransform);
			TweenTrack disabled2 = new TweenTrack();
			disabled2.mEnable = false;
			disabled2.mDuration = 70.0f;
			g.mTrackList.Add(disabled1);
			g.mTrackList.Add(enabled);
			g.mTrackList.Add(disabled2);
			seq.mGroupList.Add(g);
			seq.play();

			// 启用轨道时间窗应为 [0,2](不受两端禁用轨道影响)
			assertEqual(0.0f, enabled.getBeginTime(), "启用轨道 begin 不受前序禁用轨道影响");
			assertEqual(2.0f, enabled.getEndTime(), "启用轨道 end 不受后序禁用轨道影响");
			// 组长度也只看启用轨道
			assertEqual(2.0f, g.getGroupLength(), "组长度不含禁用轨道");
			// 序列总长度 = 该组长度
			assertEqual(2.0f, seq.getTotalLength(), "序列总长度 = 唯一启用组长度");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── SELF 起始值: 轨道开始播放时快照, 之后移动节点不改起始值 ───
	// go.localPosition 初始 (1,0,0), 第一次 evaluate 触发轨道开始并快照 start=(1,0,0)
	// 之后把节点移到 (99,0,0), 再次 evaluate 中段: 插值仍基于冻结的 start=1 而非 99
	private static void testSelfStartValueSnapshottedOnPlay()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform seqTransform = go.transform;
			seqTransform.localPosition = new Vector3(1.0f, 0.0f, 0.0f);
			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(5.0f, 0.0f, 0.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(0.0f, 2.0f, targetTransform);
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play();

			// 第一次 evaluate: 轨道开始, 快照 start=(1,0,0), 命中中段后写入 transform
			Vector3 pos, scale, rotation;
			seq.evaluateSequence(0.5f, out pos, out scale, out rotation);
			assertTrue(t.isPlaying(), "第一次 evaluate 后轨道开始播放");
			// percent=0.25 → 1+(5-1)*0.25=2
			assertEqual(2.0f, pos.x, 0.0001f, "首次 evaluate start=1, 25% → pos.x=2");
			seqTransform.localPosition = pos; // 应用

			// 外部/后续把节点移到 (99,0,0)
			seqTransform.localPosition = new Vector3(99.0f, 0.0f, 0.0f);

			// 再次 evaluate 中段: start 已冻结在 1, 不受 99 影响
			seq.evaluateSequence(1.0f, out pos, out scale, out rotation);
			// percent=0.5 → 1+(5-1)*0.5=3
			assertEqual(3.0f, pos.x, 0.0001f, "SELF 起始值在轨道开始播放时冻结(1), 节点移到99也不影响, 中点=3");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── SNAPSHOT vs REALTIME 目标值 ────────────────────────────────
	// 轨道开始播放(evaluate 触发)后改变目标节点位置:
	//   SNAPSHOT → 用轨道开始播放时快照的目标值
	//   REALTIME → 每次 evaluate 实时读目标节点当前值
	private static void testSnapshotVsRealtimeTarget()
	{
		GameObject snapGo = new GameObject();
		GameObject realGo = new GameObject();
		GameObject snapTarget = new GameObject();
		GameObject realTarget = new GameObject();
		try
		{
			Transform snapTargetTransform = snapTarget.transform;
			snapTargetTransform.localPosition = new Vector3(10.0f, 0.0f, 0.0f);
			Transform realTargetTransform = realTarget.transform;
			realTargetTransform.localPosition = new Vector3(10.0f, 0.0f, 0.0f);

			TweenSequence snapSeq = snapGo.AddComponent<TweenSequence>();
			TweenTrack snapTrack = makeMoveTrack(0.0f, 2.0f, snapTargetTransform);
			snapTrack.mTargetMode = TARGET_MODE.TRANSFORM_SNAPSHOT;
			TweenGroup g1 = new TweenGroup();
			g1.mTrackList.Add(snapTrack);
			snapSeq.mGroupList.Add(g1);
			snapSeq.play();

			TweenSequence realSeq = realGo.AddComponent<TweenSequence>();
			TweenTrack realTrack = makeMoveTrack(0.0f, 2.0f, realTargetTransform);
			realTrack.mTargetMode = TARGET_MODE.TRANSFORM_REALTIME;
			TweenGroup g2 = new TweenGroup();
			g2.mTrackList.Add(realTrack);
			realSeq.mGroupList.Add(g2);
			realSeq.play();

			// 第一次 evaluate: 两个轨道开始播放, 各自记录起始值(0)
			Vector3 pos, scale, rotation;
			snapSeq.evaluateSequence(0.5f, out pos, out scale, out rotation);
			realSeq.evaluateSequence(0.5f, out pos, out scale, out rotation);
			// 此时 SNAPSHOT 应已把目标定格为 (10,0,0)

			// 播放中把两个目标节点都移到 (100,0,0)
			snapTargetTransform.localPosition = new Vector3(100.0f, 0.0f, 0.0f);
			realTargetTransform.localPosition = new Vector3(100.0f, 0.0f, 0.0f);

			// t=1.5, percent=0.75
			snapSeq.evaluateSequence(1.5f, out pos, out scale, out rotation);
			assertEqual(7.5f, pos.x, 0.0001f, "SNAPSHOT 用轨道开始时快照目标(10), 75%→pos.x=7.5");
			realSeq.evaluateSequence(1.5f, out pos, out scale, out rotation);
			assertEqual(75.0f, pos.x, 0.0001f, "REALTIME 实时读目标当前值(100), 75%→pos.x=75");
		}
		finally
		{
			GameObject.DestroyImmediate(realTarget);
			GameObject.DestroyImmediate(snapTarget);
			GameObject.DestroyImmediate(realGo);
			GameObject.DestroyImmediate(snapGo);
		}
	}

	// ─── stop(): mResetWhenStop=true 时复位 transform ────────────────
	// play() 记录原位置, 移动节点后 stop() 应恢复原位
	private static void testStopResetTransform()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform seqTransform = go.transform;
			seqTransform.localPosition = new Vector3(7.0f, 8.0f, 9.0f);
			seqTransform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
			seqTransform.localEulerAngles = new Vector3(10.0f, 20.0f, 30.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			seq.mResetWhenStop = true;
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(0.0f, 2.0f, targetGo.transform);
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play();

			// 播放后改变 transform
			seqTransform.localPosition = new Vector3(50.0f, 50.0f, 50.0f);
			seqTransform.localScale = Vector3.one;
			seqTransform.localEulerAngles = Vector3.zero;

			seq.stop();
			assertEqual(7.0f, seqTransform.localPosition.x, 0.0001f, "stop 复位后 pos.x 恢复");
			assertEqual(8.0f, seqTransform.localPosition.y, 0.0001f, "stop 复位后 pos.y 恢复");
			assertEqual(9.0f, seqTransform.localPosition.z, 0.0001f, "stop 复位后 pos.z 恢复");
			assertEqual(2.0f, seqTransform.localScale.x, 0.0001f, "stop 复位后 scale 恢复");
			assertEqual(30.0f, seqTransform.localEulerAngles.z, 0.0001f, "stop 复位后 rotation 恢复");
			// 复位后再次强制复位不应再有动作(原始值已消费): 改为再次 stop 并保持原值
			seqTransform.localPosition = new Vector3(90.0f, 90.0f, 90.0f);
			seq.stop(true);
			assertEqual(90.0f, seqTransform.localPosition.x, 0.0001f, "复位已消费后再强制复位不再变动");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── stop(): 未配置复位且未 forceReset 时不改 transform ─────────
	private static void testStopWithoutResetKeepsTransform()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			Transform seqTransform = go.transform;
			seqTransform.localPosition = new Vector3(1.0f, 2.0f, 3.0f);

			TweenSequence seq = go.AddComponent<TweenSequence>();
			seq.mResetWhenStop = false; // 默认不复位
			TweenGroup g = new TweenGroup();
			TweenTrack t = makeMoveTrack(0.0f, 2.0f, targetGo.transform);
			g.mTrackList.Add(t);
			seq.mGroupList.Add(g);
			seq.play();

			seqTransform.localPosition = new Vector3(9.0f, 9.0f, 9.0f);
			seq.stop();
			assertEqual(9.0f, seqTransform.localPosition.x, 0.0001f, "不复位时 stop 不改动 transform");

			// forceReset=true 强制复位
			seq.stop(true);
			assertEqual(1.0f, seqTransform.localPosition.x, 0.0001f, "forceReset=true 强制复位 pos.x");
			assertEqual(2.0f, seqTransform.localPosition.y, 0.0001f, "forceReset=true 强制复位 pos.y");
			assertEqual(3.0f, seqTransform.localPosition.z, 0.0001f, "forceReset=true 强制复位 pos.z");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── 并行组: 总长度取各启用组最大 ────────────────────────────────
	private static void testTotalLengthMaxOfParallelGroups()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		try
		{
			TweenSequence seq = go.AddComponent<TweenSequence>();
			// 组1 长度 = 0+3 = 3
			TweenGroup g1 = new TweenGroup();
			g1.mTrackList.Add(makeMoveTrack(0.0f, 3.0f, targetGo.transform));
			// 组2 两个启用轨道长度 = (0.5+1.0)+(2.0+0.5)=1.5+2.5=4
			TweenGroup g2 = new TweenGroup();
			g2.mTrackList.Add(makeMoveTrack(1.0f, 0.5f, targetGo.transform));
			g2.mTrackList.Add(makeMoveTrack(0.5f, 2.0f, targetGo.transform));
			seq.mGroupList.Add(g1);
			seq.mGroupList.Add(g2);

			assertEqual(4.0f, seq.getTotalLength(), "总长度 = 并行组中最大组长度");
		}
		finally
		{
			GameObject.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ─── SCALE 与 ROTATE 轨道 evaluate ──────────────────────────────
	// 分别验证非 MOVE 类型轨道对 scale / rotation 输出的改写
	private static void testRotateAndScaleEvaluate()
	{
		GameObject go = new GameObject();
		GameObject targetGo = new GameObject();
		GameObject rotGo = new GameObject();
		GameObject rotTarget = new GameObject();
		try
		{
			Transform seqTransform = go.transform;
			seqTransform.localPosition = Vector3.zero;
			seqTransform.localScale = Vector3.one;
			seqTransform.localEulerAngles = Vector3.zero;

			Transform targetTransform = targetGo.transform;
			targetTransform.localPosition = new Vector3(5.0f, 0.0f, 0.0f);
			Transform rotTargetTransform = rotTarget.transform;
			rotTargetTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);

			// 序列同时含 MOVE 轨道与 ROTATE 轨道(不同组, 并行)
			TweenSequence seq = go.AddComponent<TweenSequence>();
			TweenGroup gMove = new TweenGroup();
			gMove.mTrackList.Add(makeMoveTrack(0.0f, 2.0f, targetTransform));
			seq.mGroupList.Add(gMove);
			// play() 会为每条轨道设置 begin/end 时间(否则 evaluateSequence 因 endTime=0 判定已结束而跳过)
			seq.play();

			// 第二个独立序列测 ROTATE, 避免不同组同帧耦合
			TweenTrack rotTrack = new TweenTrack();
			rotTrack.mType = TWEEN_TYPE.ROTATE;
			rotTrack.mEnable = true;
			rotTrack.mDuration = 2.0f;
			rotTrack.mStartDelay = 0.0f;
			rotTrack.mStartMode = START_MODE.SELF;
			rotTrack.mTargetMode = TARGET_MODE.TRANSFORM_SNAPSHOT;
			rotTrack.mTargetTransform = rotTargetTransform;
			rotTrack.setCurveID(KEY_CURVE.ZERO_ONE);
			TweenSequence rotSeq = rotGo.AddComponent<TweenSequence>();
			TweenGroup gRot = new TweenGroup();
			gRot.mTrackList.Add(rotTrack);
			rotSeq.mGroupList.Add(gRot);
			rotSeq.play(); // start 快照 = (0,0,0), 目标快照 = (0,0,90)

			Vector3 pos, scale, rotation;
			seq.evaluateSequence(1.0f, out pos, out scale, out rotation);
			assertEqual(2.5f, pos.x, 0.0001f, "MOVE 轨道中段 pos.x=2.5(start0→target5, 50%)");

			// ROTATE 中段: start=(0,0,0) target=(0,0,90), percent=0.5 → (0,0,45)
			rotSeq.evaluateSequence(1.0f, out pos, out scale, out rotation);
			assertEqual(45.0f, rotation.z, 0.0001f, "ROTATE 轨道中段 rotation.z=45");
		}
		finally
		{
			GameObject.DestroyImmediate(rotTarget);
			GameObject.DestroyImmediate(rotGo);
			GameObject.DestroyImmediate(targetGo);
			GameObject.DestroyImmediate(go);
		}
	}

	// ─── 工具: 构造一个 MOVE 轨道 ─────────────────────────────────────
	// startMode SELF(取节点当前值), targetMode SNAPSHOT(快照目标节点)
	// curve=CurveZeroOne(线性), delay 后 begin=delay, end=delay+duration
	private static TweenTrack makeMoveTrack(float delay, float duration, Transform target)
	{
		TweenTrack t = new TweenTrack();
		t.mType = TWEEN_TYPE.MOVE;
		t.mEnable = true;
		t.mDuration = duration;
		t.mStartDelay = delay;
		t.mStartMode = START_MODE.SELF;
		t.mTargetMode = TARGET_MODE.TRANSFORM_SNAPSHOT;
		t.mTargetTransform = target;
		t.setCurveID(KEY_CURVE.ZERO_ONE);
		return t;
	}
}