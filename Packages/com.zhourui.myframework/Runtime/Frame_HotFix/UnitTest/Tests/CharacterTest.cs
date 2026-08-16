using System;
using UnityEngine;
using static TestAssert;

// Character(角色基类)深度测试
// 聚焦 Character 自身的"正常使用入口": 状态机/模型相关 getter 在"组件已建但未加载模型/未添加状态"
// 空态下的确定性行为, 以及身份 getter/setter。这些方法之前无任何直接测试覆盖(覆盖率缺口),
// 且是外部对角色最常见的查询入口。
//
// 设计要点:
//   - 使用局部 new CharacterManager()(构造 mCreateObject=true) + createCharacter<Character>(name, id)
//     创建真实角色: createCharacter 内部会 character.init() -> initComponents() -> addInitComponent
//     建好 mAvatar / mStateMachine / mCOMAnimation 三个组件(组件对象非 null)。
//   - 未调 loadModel / addState, 因此组件内部为空态: mAnimator=null、状态字典为空。
//   - 因此本测试锁定的是"组件已挂但无真模型/无状态"下的守卫语义, 全部断言确定性、
//     不触发 error 日志、不依赖真资源/真模型/全局单例。
//   - 各用例 finally 调 sys.destroy() 自清理(内部 destroyAllCharacter 归池, 不手动
//     DestroyImmediate 池内对象, 遵守 GameObjectPool 特殊约定)。
//   - 不测(触发真加载/真状态链, 依赖异步资源与复杂 enter 流程, 违反"测试不触发 error 日志"):
//       * initModel/initModelAsync(会走 mAvatar.loadModel 真实资源加载)
//       * destroyModel(mObject==null 时会 selfCreateObject 依赖 mCharacterManager.getObject() 全局)
//       * addState 完整链(getStateMachine().addState 内 state.enter() 与状态时间依赖 mGameFrameworkHotFix)
//
// 源码事实(作为断言期望, 已核对 Character.cs / COMCharacterStateMachine.cs / COMCharacterAvatar.cs):
//   - Character.initComponents() 用 addInitComponent(out mAvatar/mStateMachine/mCOMAnimation, true)
//     建好组件 -> 创建后这些 getter 返回非 null 组件。
//   - getAvatar()/getCOMAnimation()/getStateMachine() 返回组件(非 null);
//     getAnimator()/getRigidBody() 是 mAvatar?.getAnimator()/getRigidBody() -> 未 loadModel 时 null;
//     getAnimationLength(name) 是 mAvatar?.getAnimationLength(name) ?? 0 -> mAnimator==null 时 0。
//   - getStateList() 返回空 SafeDictionary; getFirstState/getFirstGroupState/getState 空态返回 null;
//     hasState/hasStateGroup 空态返回 false。
//   - createCharacter 内 setCharacterType(type) + setID(id) -> getType()==typeof(Character)、
//     getGUID()==传入 id; isMyself() 默认 false。
public static class CharacterTest
{
	public static void Run()
	{
		// ─── 状态机相关空态 getter ───
		testStateAccessors_EmptyMachine_NonNullButEmpty();
		testStateAccessors_QueryReturnsNull_False();
		// ─── 模型相关 getter(组件已建但未加载模型) ───
		testModelGetters_ComponentsBuilt_ModelNotLoaded();
		testModelGetters_AnimationLength_ZeroWhenNoModel();
		// ─── 身份 getter/setter ───
		testIdentity_TypeAndGUID_FromCreate();
		testIdentity_SetID_Overrides();
		testIdentity_IsMyself_FalseForNormalCharacter();
		// ─── 空安全守卫(组件引用访问不因未挂载而抛错) ───
		testNullSafety_GetStateMachineChain_NoThrow();
		testNotifyModelLoadedSafe();
		testNotifyModelLoadedTwice();
		testSetCharacterTypeRoundTrip();
		testSetCharacterTypeNull();
		testComponentsBuilt();
	}

	// ═════════════════════════════════════════════════════════════════
	// 状态机相关: 组件已建, 状态字典为空
	// ═════════════════════════════════════════════════════════════════
	private static void testStateAccessors_EmptyMachine_NonNullButEmpty()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c1", 100);
			assertNotNull(c, "创建角色成功");
			// initComponents 已 addInitComponent 建好状态机组件
			assertNotNull(c.getStateMachine(), "getStateMachine 组件已建非 null");
			// 状态列表为空字典
			var stateList = c.getStateList();
			assertNotNull(stateList, "getStateList 返回空字典非 null");
			assertEqual(0, stateList.count(), "空态状态列表 count()==0");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testStateAccessors_QueryReturnsNull_False()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c2", 200);
			// 空态下各种查询返回 null/false(不抛异常)
			assertNull(c.getFirstState(typeof(object)), "空态 getFirstState 返回 null");
			assertNull(c.getFirstGroupState(typeof(object)), "空态 getFirstGroupState 返回 null");
			assertNull(c.getState(123456L), "空态 getState 返回 null");
			assertFalse(c.hasState(typeof(object)), "空态 hasState 为 false");
			assertFalse(c.hasStateGroup(typeof(object)), "空态 hasStateGroup 为 false");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 模型相关: 组件已建, 但未 loadModel
	// ═════════════════════════════════════════════════════════════════
	private static void testModelGetters_ComponentsBuilt_ModelNotLoaded()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c3", 300);
			// 模型/动作组件已 addInitComponent 建好
			assertNotNull(c.getAvatar(), "getAvatar 组件已建非 null");
			assertNotNull(c.getCOMAnimation(), "getCOMAnimation 组件已建非 null");
			// 但未 loadModel -> 内部 mAnimator/mRigidBody 为 null
			assertNull(c.getAnimator(), "未加载模型 getAnimator 返回 null");
			assertNull(c.getRigidBody(), "未加载模型 getRigidBody 返回 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testModelGetters_AnimationLength_ZeroWhenNoModel()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c4", 400);
			// mAnimator==null -> getAnimationLength 返回 0.0f
			assertEqual(0.0f, c.getAnimationLength("idle"), "无模型时动画长度 0");
			assertEqual(0.0f, c.getAnimationLength(null), "无模型时 null 名动画长度 0");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 身份 getter/setter
	// ═════════════════════════════════════════════════════════════════
	private static void testIdentity_TypeAndGUID_FromCreate()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c5", 555);
			// createCharacter 内 setCharacterType(typeof(Character)) + setID(555)
			assertEqual(typeof(Character), c.getType(), "getType 返回 createCharacter 设置的 Character 类型");
			assertEqual(555L, c.getGUID(), "getGUID 返回 createCharacter 显式 id");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testIdentity_SetID_Overrides()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c6", 600);
			c.setID(999L);
			assertEqual(999L, c.getGUID(), "setID 覆盖 getGUID");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testIdentity_IsMyself_FalseForNormalCharacter()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c7", 700);
			assertFalse(c.isMyself(), "普通角色 isMyself 为 false");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 空安全守卫: 状态机链在组件已建时访问不抛异常
	// ═════════════════════════════════════════════════════════════════
	private static void testNullSafety_GetStateMachineChain_NoThrow()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("c8", 800);
			// 连续访问状态机链, 不抛异常
			var sm = c.getStateMachine();
			assertNotNull(sm, "状态机组件非 null");
			assertNotNull(sm.getStateList(), "状态机 getStateList 非 null");
			assertNull(sm.getFirstState(typeof(object)), "状态机空态 getFirstState null");
			assertFalse(sm.hasState(typeof(object)), "状态机空态 hasState false");
			// getStateMachine 重复调用返回同一实例(惰性创建后缓存)
			assertTrue(ReferenceEquals(c.getStateMachine(), sm), "重复 getStateMachine 返回同一实例");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// notifyModelLoaded 空虚方法调用安全
	private static void testNotifyModelLoadedSafe()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("notify1", 200);
			c.notifyModelLoaded();
			// 无异常即通过
		}
		finally
		{
			sys.destroy();
		}
	}

	// notifyModelLoaded 多次调用安全
	private static void testNotifyModelLoadedTwice()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("notify2", 201);
			c.notifyModelLoaded();
			c.notifyModelLoaded();
			// 无异常即通过
		}
		finally
		{
			sys.destroy();
		}
	}

	// setCharacterType → getType 往返
	private static void testSetCharacterTypeRoundTrip()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("type1", 202);
			c.setCharacterType(typeof(string));
			assertEqual(typeof(string), c.getType(), "setCharacterType 读回");
		}
		finally
		{
			sys.destroy();
		}
	}

	// setCharacterType(null) 读回 null
	private static void testSetCharacterTypeNull()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("type2", 203);
			c.setCharacterType(null);
			assertNull(c.getType(), "set null 读回 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	// 组件已建: getAvatar/getCOMAnimation 非 null
	private static void testComponentsBuilt()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<Character>("comp1", 204);
			assertNotNull(c.getAvatar(), "getAvatar 组件已建");
			assertNotNull(c.getCOMAnimation(), "getCOMAnimation 组件已建");
		}
		finally
		{
			sys.destroy();
		}
	}
}
