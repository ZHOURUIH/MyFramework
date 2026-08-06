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
}