using System;
using UnityEngine;
using static TestAssert;
using UObject = UnityEngine.Object;

// GameEffect 单元测试
// 框架环境已完全初始化, 可覆盖:
//   构造 / 继承 MovableObject 的 ObjectID
//   setObject(GameObject) 绑定 + 收集粒子/拖尾/动画组件 + 关闭粒子culling与拖尾autodestruct
//   默认 getter (isExistObject/isDead/isInEffectPool/isMoveToHide/getPlayState/getTag/getFilePath/getUnuseTime/isValidEffect)
//   setter 链路 (setLifeTime/setFilePath/setTag/setUnuseTime/setExistObject/setDead/setInEffectPool/setMoveToHide/setEffectDestroyCallback)
//   play / stop / stopAndMove 状态机与位置
//   setActive(false) 触发 stop
//   update 生命周期计时 (tickTimerOnce → mLifeTimer归-1 → mIsDead)
//   setIgnoreTimeScale 记录 mDefaultIgnoreTimeScale 并切换 Animator.updateMode
//   resetProperty 逐字段默认值 (核心: mLifeTimer=-1, mUnuseTime=MinValue, mPlayState=STOP)
public static class GameEffectTest
{
	public static void Run()
	{
		// ─── 构造 ───
		testConstruct();
		testObjectIDUnique();
		// ─── 对象绑定与组件收集 ───
		testSetObjectBindsAndCollects();
		testSetObjectCollectsTrailAndAnimator();
		// ─── 默认 getter ───
		testDefaultGetters();
		// ─── setter 链路 ───
		testSetters();
		testSetEffectDestroyCallback();
		// ─── 状态机 ───
		testPlayState();
		testStopState();
		testStopAndMove();
		testSetActiveFalseStops();
		// ─── update 生命周期 ───
		testUpdateLifeTimerDeath();
		testUpdateNoLifeTimerNoDeath();
		// ─── checkValid ───
		testCheckValidExistObject();
		testCheckValidNoObject();
		// ─── setIgnoreTimeScale ───
		testSetIgnoreTimeScaleRecordsDefault();
		// ─── clearTrail ───
		testClearTrail();
		// ─── resetProperty 默认值 ───
		testResetPropertyDefaults();
	}

	// ═════════════════════════════════════════════════════════════════
	// 构造
	// ═════════════════════════════════════════════════════════════════
	private static void testConstruct()
	{
		GameEffect effect = new();
		assertNotNull(effect, "GameEffect 可构造");
		effect.destroy();
	}
	private static void testObjectIDUnique()
	{
		GameEffect a = new();
		GameEffect b = new();
		assertTrue(a.getObjectID() != b.getObjectID(), "不同实例 ObjectID 应不同");
		a.destroy();
		b.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// setObject 绑定与组件收集
	// ═════════════════════════════════════════════════════════════════
	private static void testSetObjectBindsAndCollects()
	{
		GameEffect effect = new();
		var go = new GameObject("GEBind");
		// 注意: setExistObject(true) 避免 destroy 时走 PrefabPoolManager.destroyObject 对裸对象 logError
		effect.setExistObject(true);
		try
		{
			// 添加一个粒子系统, setObject 应收集到 mParticleSystems 并关闭 cullingMode
			var ps = go.AddComponent<ParticleSystem>();
			effect.setObject(go);
			assertEqual(go, effect.getGameObject(), "setObject 应绑定 GameObject");
			assertTrue(effect.isValidEffect(), "绑定对象后 isValidEffect 为 true");
			// setObject 应将粒子 cullingMode 设为 Pause
			var main = ps.main;
			assertEqual(ParticleSystemCullingMode.Pause, main.cullingMode, "setObject 应将粒子 cullingMode 设为 Pause");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
	private static void testSetObjectCollectsTrailAndAnimator()
	{
		GameEffect effect = new();
		var go = new GameObject("GETrailAnim");
		effect.setExistObject(true);
		try
		{
			var trail = go.AddComponent<TrailRenderer>();
			var anim = go.AddComponent<Animator>();
			effect.setObject(go);
			// TrailRenderer.autodestruct 被关掉
			assertFalse(trail.autodestruct, "setObject 应关闭 TrailRenderer.autodestruct");
			// setObject 收集到 Animator 后, setIgnoreTimeScale 应切换其 updateMode
			effect.setIgnoreTimeScale(true);
			assertEqual(AnimatorUpdateMode.UnscaledTime, anim.updateMode, "setIgnoreTimeScale(true) 应切换 Animator 为 UnscaledTime");
			effect.setIgnoreTimeScale(false);
			assertEqual(AnimatorUpdateMode.Normal, anim.updateMode, "setIgnoreTimeScale(false) 应切换 Animator 为 Normal");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认 getter
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultGetters()
	{
		GameEffect effect = new();
		assertFalse(effect.isExistObject(), "默认 mExistedObject false");
		assertFalse(effect.isDead(), "默认 mIsDead false");
		assertFalse(effect.isInEffectPool(), "默认 mIsEffectPool false");
		assertFalse(effect.isMoveToHide(), "默认 mMoveToHide false");
		assertFalse(effect.isValidEffect(), "未绑定对象 isValidEffect false");
		assertEqual(PLAY_STATE.STOP, effect.getPlayState(), "默认 mPlayState STOP");
		assertEqual(0, effect.getTag(), "默认 mTag 0");
		assertNull(effect.getFilePath(), "默认 mFilePath null");
		assertEqual(DateTime.MinValue, effect.getUnuseTime(), "默认 mUnuseTime MinValue");
		effect.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// setter 链路
	// ═════════════════════════════════════════════════════════════════
	private static void testSetters()
	{
		GameEffect effect = new();
		effect.setExistObject(true);
		effect.setDead(true);
		effect.setInEffectPool(true);
		effect.setMoveToHide(true);
		effect.setLifeTime(3.0f);
		effect.setFilePath("effect/abc.prefab");
		effect.setTag(42);
		var time = new DateTime(2026, 8, 8, 12, 30, 0);
		effect.setUnuseTime(time);
		assertTrue(effect.isExistObject(), "setExistObject(true) 生效");
		assertTrue(effect.isDead(), "setDead(true) 生效");
		assertTrue(effect.isInEffectPool(), "setInEffectPool(true) 生效");
		assertTrue(effect.isMoveToHide(), "setMoveToHide(true) 生效");
		assertEqual("effect/abc.prefab", effect.getFilePath(), "setFilePath 生效");
		assertEqual(42, effect.getTag(), "setTag 生效");
		assertEqual(time, effect.getUnuseTime(), "setUnuseTime 生效");
		effect.destroy();
	}
	private static void testSetEffectDestroyCallback()
	{
		GameEffect effect = new();
		effect.setExistObject(true);
		int invoked = 0;
		effect.setEffectDestroyCallback(ge => invoked++);
		effect.destroy();
		// destroy 会 Invoke mEffectDestroyCallback
		assertEqual(1, invoked, "destroy 应触发 mEffectDestroyCallback");
	}

	// ═════════════════════════════════════════════════════════════════
	// 状态机
	// ═════════════════════════════════════════════════════════════════
	private static void testPlayState()
	{
		GameEffect effect = new();
		var go = new GameObject("GEPlay");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			assertEqual(PLAY_STATE.STOP, effect.getPlayState(), "play 前 STOP");
			effect.play();
			assertEqual(PLAY_STATE.PLAY, effect.getPlayState(), "play 后 PLAY");
			// play 会激活节点
			assertTrue(effect.isActive(), "play 应激活特效");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
	private static void testStopState()
	{
		GameEffect effect = new();
		var go = new GameObject("GEStop");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			effect.play();
			effect.stop();
			assertEqual(PLAY_STATE.STOP, effect.getPlayState(), "stop 后 STOP");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
	private static void testStopAndMove()
	{
		GameEffect effect = new();
		var go = new GameObject("GEStopMove");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			effect.play();
			var pos = new Vector3(10, 20, 30);
			effect.stopAndMove(pos);
			assertEqual(PLAY_STATE.STOP, effect.getPlayState(), "stopAndMove 后 STOP");
			assertEqual(pos, effect.getPosition(), "stopAndMove 应移动到指定位置");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
	private static void testSetActiveFalseStops()
	{
		GameEffect effect = new();
		var go = new GameObject("GEActive");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			effect.play();
			assertEqual(PLAY_STATE.PLAY, effect.getPlayState(), "play 后 PLAY");
			effect.setActive(false);
			assertEqual(PLAY_STATE.STOP, effect.getPlayState(), "setActive(false) 应触发 stop");
			assertFalse(effect.isActive(), "setActive(false) 后 inactive");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// checkValid
	// ═════════════════════════════════════════════════════════════════
	private static void testCheckValidExistObject()
	{
		// checkValid: isExistObject()==true 时直接返回 mObject != null
		GameEffect effect = new();
		var go = new GameObject("GECheckValid");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			assertTrue(effect.checkValid(), "setExistObject(true)+setObject 后 checkValid 应 true");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
	private static void testCheckValidNoObject()
	{
		// isExistObject()==true 但 mObject==null 时 checkValid 返回 false
		GameEffect effect = new();
		effect.setExistObject(true);
		assertFalse(effect.checkValid(), "setExistObject(true) 但未绑定对象时 checkValid 应 false");
		effect.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// update 生命周期
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdateLifeTimerDeath()
	{
		GameEffect effect = new();
		var go = new GameObject("GEUpdate");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			effect.setLifeTime(2.0f);
			assertFalse(effect.isDead(), "设置生命周期后未到时不死亡");
			effect.update(1.0f);
			assertFalse(effect.isDead(), "剩余1秒仍存活");
			effect.update(1.0f);
			// tickTimerOnce: timer<=0 → 归-1 并返回 true → mIsDead=true
			assertTrue(effect.isDead(), "生命周期耗尽后 mIsDead true");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
	private static void testUpdateNoLifeTimerNoDeath()
	{
		GameEffect effect = new();
		var go = new GameObject("GENoLife");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			// 默认 mLifeTimer = -1, tickTimerOnce 直接返回 false, 永不死亡
			assertFalse(effect.isDead(), "默认无生命周期不死亡");
			effect.update(5.0f);
			effect.update(100.0f);
			assertFalse(effect.isDead(), "无生命周期多次 update 仍不死亡");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setIgnoreTimeScale
	// ═════════════════════════════════════════════════════════════════
	private static void testSetIgnoreTimeScaleRecordsDefault()
	{
		// 用辅助子类暴露 protected 字段 mDefaultIgnoreTimeScale
		GameEffectTestHelper effect = new();
		var go = new GameObject("GEIgnore");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			assertFalse(effect.getDefaultIgnoreTimeScale(), "初始 mDefaultIgnoreTimeScale false");
			// setIgnoreTimeScale(true) 会无条件记录 mDefaultIgnoreTimeScale = true
			effect.setIgnoreTimeScale(true);
			assertTrue(effect.getDefaultIgnoreTimeScale(), "setIgnoreTimeScale(true) 记录 mDefaultIgnoreTimeScale");
			// 再次设置 false 会覆盖记录
			effect.setIgnoreTimeScale(false);
			assertFalse(effect.getDefaultIgnoreTimeScale(), "setIgnoreTimeScale(false) 覆盖记录 mDefaultIgnoreTimeScale");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// clearTrail
	// ═════════════════════════════════════════════════════════════════
	private static void testClearTrail()
	{
		// clearTrail 遍历 mTrailRenderers 调用 Clear(); 空列表安全无操作
		GameEffect effect = new();
		var go = new GameObject("GEClearTrail");
		effect.setExistObject(true);
		try
		{
			// 空列表时 clearTrail 无副作用
			effect.setObject(go);
			effect.clearTrail();
			// 绑定含 TrailRenderer 的对象后 clearTrail 仍不抛异常
			go.AddComponent<TrailRenderer>();
			effect.setObject(go);
			effect.clearTrail();
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty 默认值
	// ═════════════════════════════════════════════════════════════════
	private static void testResetPropertyDefaults()
	{
		// 用辅助子类暴露 mDefaultIgnoreTimeScale
		GameEffectTestHelper effect = new();
		var go = new GameObject("GEReset");
		effect.setExistObject(true);
		try
		{
			// 先污染各字段
			effect.setObject(go);
			effect.setExistObject(true);
			effect.setDead(true);
			effect.setInEffectPool(true);
			effect.setMoveToHide(true);
			effect.setLifeTime(8.0f);
			effect.setFilePath("effect/xx.prefab");
			effect.setTag(99);
			effect.setUnuseTime(new DateTime(2026, 1, 1));
			effect.setIgnoreTimeScale(true);
			effect.setEffectDestroyCallback(ge => { });
			effect.play();

			effect.resetProperty();
			// resetProperty 清空对象引用
			assertNull(effect.getGameObject(), "resetProperty 后 getGameObject 为 null");
			// 逐字段默认值
			assertFalse(effect.isExistObject(), "resetProperty 重置 mExistedObject");
			assertFalse(effect.isDead(), "resetProperty 重置 mIsDead");
			assertFalse(effect.isInEffectPool(), "resetProperty 重置 mIsEffectPool");
			assertFalse(effect.isMoveToHide(), "resetProperty 重置 mMoveToHide");
			assertFalse(effect.getDefaultIgnoreTimeScale(), "resetProperty 重置 mDefaultIgnoreTimeScale");
			assertNull(effect.getFilePath(), "resetProperty 重置 mFilePath 为 null");
			assertEqual(0, effect.getTag(), "resetProperty 重置 mTag 为 0");
			assertEqual(DateTime.MinValue, effect.getUnuseTime(), "resetProperty 重置 mUnuseTime 为 MinValue");
			assertEqual(PLAY_STATE.STOP, effect.getPlayState(), "resetProperty 重置 mPlayState 为 STOP");
			assertFalse(effect.isActive(), "resetProperty 重置为 inactive");
			// mLifeTimer 重置为 -1 → 后续 setLifeTime 重置后再 update 不死亡
			effect.setLifeTime(5.0f);
			effect.update(100.0f);
			assertTrue(effect.isDead(), "重置后设置 5 秒生命周期, update 100 秒应死亡");
		}
		finally
		{
			effect.destroy();
			UObject.DestroyImmediate(go);
		}
	}
}

// 测试辅助类: 暴露 GameEffect 的 protected 字段 mDefaultIgnoreTimeScale 供断言
// 无新增实例字段, 无需重写 resetProperty (RESET002 自动跳过无字段类)
public class GameEffectTestHelper : GameEffect
{
	public bool getDefaultIgnoreTimeScale() { return mDefaultIgnoreTimeScale; }
}
