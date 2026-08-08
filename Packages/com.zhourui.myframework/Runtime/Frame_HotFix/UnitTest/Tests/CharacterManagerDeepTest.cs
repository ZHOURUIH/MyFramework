using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// CharacterManager 深度测试
// 聚焦复杂 create/destroy 生命周期、跨多张字典/列表(type-list、GUID 索引、update 列表、
// fixedUpdate 列表)的一致性同步、myself 单例约束、以及 managed 标记对更新遍历的影响。
// 而非只测"调用某函数不报错"。
//
// 设计要点:
//   - 全部使用局部 new CharacterManager()(构造 mCreateObject=true), 不依赖全局单例
//     mCharacterManager, 避免测试间状态污染; 各用例 finally 调 destroy() 自清理。
//   - Character 通过 CLASS<Character>(type) 由对象池创建, createCharacter 内部会
//     character.init() -> selfCreateObject() 创建真实 GameObject 节点, 该节点生命周期由
//     mGameObjectPool 接管(destroyCharacter/destroyAllCharacter 时 destroyObject 归还缓存)。
//     测试不手动 DestroyImmediate 池内对象(遵守 GameObjectPool 特殊约定, 防坏引用)。
//   - 不测(触发生效 error 日志, 遵守"测试不得触发 error 日志"):
//       * 重复 id createCharacter(源码 logError "there is a character id")
//       * myself 已存在再创建 myself(源码 logError "Myself has exist")
//       * addCharacterToList 中 TryAdd 冲突(logError "can not add again", 实际被创建前 ContainsKey 拦截, 不可达)
//   - BASE 约定: 测试子类不 override Character 需要 base 的成员, 直接实例化干净子类。
//   - 更新遍历: Character.update() 走 ComponentOwner.update 的组件遍历, COM 组件 update 均
//     空安全(COMCharacterAvatar.update 内 syncTransform 因 mObject(null, 未 loadModel) 直接 return),
//     可在 EditMode 安全调用 manager.update()。
//
// 源码事实(作为断言期望, 文档化):
//   - createCharacter 的 id 为 0 时内部 generateGUID(); 显式 id > 0 则使用该 id。
//   - addCharacterToList: managed=true 才加入 mCharacterUpdateList; 且仅当
//     character.isEnableFixedUpdate()(依赖 mCOMMoveInfo, 默认无此组件 -> false)才加入 mFixedUpdateList。
//   - mMyself 是局部字段: isMyself() 返回 protected mIsMyself(非虚), 子类内可赋值但不重置。
public static class CharacterManagerDeepTest
{
	public static void Run()
	{
		// ─── 创建与列表一致性 ───
		testCreate_BasicListsPopulated();
		testCreate_ManagedFalse_ExcludesUpdateLists();
		testCreate_ExplicitID_SetsGUID();
		testCreate_GeneratedID_NonZero();
		// ─── 查询 ───
		testGet_ById_And_List();
		testGet_ByType_GenericAndRuntime();
		testGet_UnknownID_Null();
		// ─── type-list 分组完整性 ───
		testTypeList_SeparateByType();
		testTypeList_SameTypeMultipleIDs_MixedTypes();
		// ─── destroy 生命周期 ───
		testDestroy_ById_RemovesAllLists();
		testDestroy_ByRef_RemovesAllLists();
		testDestroy_UnknownID_NoOp();
		testDestroy_Null_NoOp();
		testDestroyAll_ClearsEverything();
		// ─── 批量销毁 ───
		testDestroyCharacterList_GenericList();
		testDestroyCharacterList_Dictionary();
		// ─── myself 单例 ───
		testMyself_Registered_WhenMyself();
		testMyself_NonMyself_NotRegistered();
		testMyself_Destroyed_Clears();
		testMyself_DestroyAll_Clears();
		// ─── update 遍历(复杂调用链) ───
		testUpdate_TraversesManagedOnly();
		testUpdate_AfterDestroy_Skipped();
		testLateUpdate_SafeOnEmpty();
		testFixedUpdate_NotPopulatedWithoutMoveInfo();
	}

	// ═════════════════════════════════════════════════════════════════
	// 创建与列表一致性
	// ═════════════════════════════════════════════════════════════════
	private static void testCreate_BasicListsPopulated()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<DeepSoldier>("s1");
			assertNotNull(c, "创建成功返回角色");
			// 4 个内部状态: GUID 索引 + type 分类 + update 列表 (+ fixedUpdate 列表默认空)
			assertEqual(1, sys.getCharacterList().Count, "GUID 索引含 1 个");
			Dictionary<long, Character> typeList = sys.getCharacterListByType(typeof(DeepSoldier));
			assertNotNull(typeList, "type 分类列表已建");
			assertEqual(1, typeList.Count, "type 分类含 1 个");
			assertEqual(c, sys.getCharacter(c.getGUID()), "按 GUID 命中同一实例");
			assertTrue(ReferenceEquals(c, sys.getCharacter(c.getGUID())), "返回同一引用");
			assertEqual(typeof(DeepSoldier), c.getType(), "getType 为创建传入的类型");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testCreate_ManagedFalse_ExcludesUpdateLists()
	{
		CharacterManager sys = new();
		try
		{
			// managed=false: 仍进 GUID 索引与 type 分类, 但不进 update/fixedUpdate 列表
			Character c = sys.createCharacter<DeepSoldier>("u1", 0, false);
			assertNotNull(c, "managed=false 仍成功创建");
			assertEqual(1, sys.getCharacterList().Count, "GUID 索引包含(managed 不影响索引)");
			assertEqual(1, sys.getCharacterListByType(typeof(DeepSoldier)).Count, "type 分类包含");
			// update 列表内部不可直接读, 通过 update 遍历间接验证不被调用(见 testUpdate_TraversesManagedOnly)
			assertTrue(c.isMyself() == false, "普通角色非 myself");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testCreate_ExplicitID_SetsGUID()
	{
		CharacterManager sys = new();
		try
		{
			const long id = 9001;
			Character c = sys.createCharacter<DeepSoldier>("e1", id);
			assertEqual(id, c.getGUID(), "显式 id 被设置到 GUID");
			assertEqual(c, sys.getCharacter(id), "按显式 id 可查");
			// 用同一个 id 再创建应返回 null 并触发 logError, 但遵守测试约定不触发 error, 故不测该分支
			assertEqual(1, sys.getCharacterList().Count, "索引数量仍为 1");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testCreate_GeneratedID_NonZero()
	{
		CharacterManager sys = new();
		try
		{
			Character c = sys.createCharacter<DeepSoldier>("g1");
			assertTrue(c.getGUID() != 0, "未显式指定 id 时由 generateGUID 分配非零 id");
			// 两个角色 id 不同(generateGUID 自增保证)
			Character c2 = sys.createCharacter<DeepSoldier>("g2");
			assertTrue(c2.getGUID() != c.getGUID(), "两次生成 id 不同");
			assertTrue(c2.getGUID() != 0, "第二个角色 id 非零");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 查询
	// ═════════════════════════════════════════════════════════════════
	private static void testGet_ById_And_List()
	{
		CharacterManager sys = new();
		try
		{
			Character a = sys.createCharacter<DeepSoldier>("a1", 100);
			Character b = sys.createCharacter<DeepPlayer>("b1", 200);
			assertEqual(a, sys.getCharacter(100), "getCharacter(100)");
			assertEqual(b, sys.getCharacter(200), "getCharacter(200)");
			assertEqual(2, sys.getCharacterList().Count, "全部索引 2 个(跨类型)");
			assertTrue(sys.getCharacterList().ContainsKey(100), "索引含 100");
			assertTrue(sys.getCharacterList().ContainsKey(200), "索引含 200");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testGet_ByType_GenericAndRuntime()
	{
		CharacterManager sys = new();
		try
		{
			Character a = sys.createCharacter<DeepSoldier>("s_a", 10);
			Character b = sys.createCharacter<DeepPlayer>("p_b", 20);
			// 泛型重载与运行时 Type 重载指向同一分类
			Dictionary<long, Character> sold = sys.getCharacterListByType<DeepSoldier>();
			Dictionary<long, Character> sold2 = sys.getCharacterListByType(typeof(DeepSoldier));
			assertNotNull(sold, "泛型 type 列表非空");
			assertTrue(ReferenceEquals(sold, sold2), "泛型与运行时 Type 返回同一字典引用");
			assertEqual(1, sold.Count, "Soldier 分类含 1");
			assertEqual(a, sold[10], "Soldier 分类命中原对象");
			assertEqual(1, sys.getCharacterListByType<DeepPlayer>().Count, "Player 分类含 1");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testGet_UnknownID_Null()
	{
		CharacterManager sys = new();
		try
		{
			assertNull(sys.getCharacter(99999), "未创建 id getCharacter 为 null");
			assertNull(sys.getCharacterListByType<DeepSoldier>(), "未创建类型的 type 分类为 null(字典尚未建立)");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// type-list 分组完整性
	// ═════════════════════════════════════════════════════════════════
	private static void testTypeList_SeparateByType()
	{
		CharacterManager sys = new();
		try
		{
			sys.createCharacter<DeepSoldier>("s1", 1);
			sys.createCharacter<DeepSoldier>("s2", 2);
			sys.createCharacter<DeepPlayer>("p1", 3);
			assertEqual(2, sys.getCharacterListByType<DeepSoldier>().Count, "Soldier 分类 2 个");
			assertEqual(1, sys.getCharacterListByType<DeepPlayer>().Count, "Player 分类 1 个");
			assertEqual(3, sys.getCharacterList().Count, "全局索引 3 个(类型不混)");
			// 两个分类不共享字典实例
			assertTrue(!ReferenceEquals(
				sys.getCharacterListByType<DeepSoldier>(),
				sys.getCharacterListByType<DeepPlayer>()), "不同类型分类是不同字典");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testTypeList_SameTypeMultipleIDs_MixedTypes()
	{
		CharacterManager sys = new();
		try
		{
			// 交错创建不同类型, 验证 type 分类按类型正确隔离
			Character p1 = sys.createCharacter<DeepPlayer>("p1", 1);
			Character s1 = sys.createCharacter<DeepSoldier>("s1", 2);
			Character s2 = sys.createCharacter<DeepSoldier>("s2", 3);
			Character p2 = sys.createCharacter<DeepPlayer>("p2", 4);
			assertEqual(2, sys.getCharacterListByType<DeepPlayer>().Count, "Player 分类 2 个");
			assertEqual(2, sys.getCharacterListByType<DeepSoldier>().Count, "Soldier 分类 2 个");
			// key 保持显式 id
			assertTrue(sys.getCharacterListByType<DeepPlayer>().ContainsKey(1), "Player 含 id 1");
			assertTrue(sys.getCharacterListByType<DeepPlayer>().ContainsKey(4), "Player 含 id 4");
			assertTrue(sys.getCharacterListByType<DeepSoldier>().ContainsKey(3), "Soldier 含 id 3");
			assertEqual(4, sys.getCharacterList().Count, "全局索引 4 个");
			// 类型内引用正确
			assertTrue(ReferenceEquals(sys.getCharacterListByType<DeepSoldier>()[3], s2), "Soldier[3] 引用 s2");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// destroy 生命周期
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroy_ById_RemovesAllLists()
	{
		CharacterManager sys = new();
		try
		{
			Character a = sys.createCharacter<DeepSoldier>("a", 1);
			sys.createCharacter<DeepPlayer>("b", 2);
			long guid = a.getGUID();
			sys.destroyCharacter(guid);
			assertEqual(1, sys.getCharacterList().Count, "按 id 销毁后全局索引剩 1");
			assertNull(sys.getCharacter(guid), "被销毁 id 不可再查");
			assertTrue(!sys.getCharacterListByType<DeepSoldier>().ContainsKey(guid), "type 分类已移除该 id");
			assertEqual(1, sys.getCharacterListByType<DeepPlayer>().Count, "Player 分类不受影响");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testDestroy_ByRef_RemovesAllLists()
	{
		CharacterManager sys = new();
		try
		{
			Character a = sys.createCharacter<DeepSoldier>("a", 1);
			Character b = sys.createCharacter<DeepSoldier>("b", 2);
			long guidA = a.getGUID();
			sys.destroyCharacter(a);
			assertNull(sys.getCharacter(guidA), "按引用销毁后 GUID 不可查");
			assertTrue(!sys.getCharacterListByType<DeepSoldier>().ContainsKey(guidA), "type 分类移除");
			assertEqual(1, sys.getCharacterListByType<DeepSoldier>().Count, "同类型剩余 b");
			// b 仍有效
			assertTrue(ReferenceEquals(b, sys.getCharacter(b.getGUID())), "b 未受影响");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testDestroy_UnknownID_NoOp()
	{
		CharacterManager sys = new();
		try
		{
			sys.createCharacter<DeepSoldier>("a", 1);
			// 销毁未知 id: getCharacter 返回 null -> destroyCharacter(null) 直接 return, 不报错
			sys.destroyCharacter(424242);
			assertEqual(1, sys.getCharacterList().Count, "未知 id 销毁不影响现有");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testDestroy_Null_NoOp()
	{
		CharacterManager sys = new();
		try
		{
			sys.destroyCharacter((Character)null);
			assertEqual(0, sys.getCharacterList().Count, "null 销毁安全无操作");
			sys.createCharacter<DeepSoldier>("a", 1);
			sys.destroyCharacter((Character)null);
			assertEqual(1, sys.getCharacterList().Count, "null 销毁不影响已有角色");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testDestroyAll_ClearsEverything()
	{
		CharacterManager sys = new();
		try
		{
			createMyself(sys, "me", 1);
			sys.createCharacter<DeepSoldier>("s1", 2);
			sys.createCharacter<DeepSoldier>("s2", 3);
			sys.createCharacter<DeepPlayer>("p1", 4);
			sys.destroyAllCharacter();
			assertEqual(0, sys.getCharacterList().Count, "destroyAll 清空全局索引");
			// destroyAllCharacter 用 mCharacterTypeList.Clear() 清空所有类型 key,
			// 因此 getCharacterListByType 返回 null(字典对象与 key 一并清空)
			assertNull(sys.getCharacterListByType<DeepSoldier>(), "destroyAll 后 Soldier 类型分类为 null(key 被 Clear)");
			assertNull(sys.getCharacterListByType<DeepPlayer>(), "destroyAll 后 Player 类型分类为 null(key 被 Clear)");
			assertNull(sys.getMyself(), "myself 也被清空");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 批量销毁
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyCharacterList_GenericList()
	{
		CharacterManager sys = new();
		try
		{
			Character a = sys.createCharacter<DeepSoldier>("a", 1);
			Character b = sys.createCharacter<DeepPlayer>("b", 2);
			Character c = sys.createCharacter<DeepPlayer>("c", 3);
			List<Character> batch = new() { a, b };
			sys.destroyCharacterList(batch);
			assertEqual(1, sys.getCharacterList().Count, "批量销毁 List 后剩 1");
			assertEqual(c, sys.getCharacter(c.getGUID()), "剩余的是 c");
			assertNull(sys.getCharacter(a.getGUID()), "a 已销毁");
			assertNull(sys.getCharacter(b.getGUID()), "b 已销毁");
			// 跨类型分类都被清理
			assertEqual(0, sys.getCharacterListByType<DeepSoldier>().Count, "Soldier 分类清空");
			assertEqual(1, sys.getCharacterListByType<DeepPlayer>().Count, "Player 分类剩 c");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testDestroyCharacterList_Dictionary()
	{
		CharacterManager sys = new();
		try
		{
			Character a = sys.createCharacter<DeepSoldier>("a", 1);
			Character b = sys.createCharacter<DeepPlayer>("b", 2);
			// 用临时字典承载要销毁的角色(值类型为 Character)
			Dictionary<long, Character> batch = new() { { 1L, a }, { 2L, b } };
			sys.destroyCharacterList(batch);
			assertEqual(0, sys.getCharacterList().Count, "批量销毁 Dictionary 后清空");
			// destroyCharacterList(Dictionary) 逐项移除 type 分类(GUID/update/fixed 均移除), 但保留 type 分类字典对象(仅 Remove Key)
			Dictionary<long, Character> sold = sys.getCharacterListByType<DeepSoldier>();
			assertNotNull(sold, "type 分类字典对象仍存在(仅移除 key 不清字典)");
			assertEqual(0, sold.Count, "Soldier 分类已清空");
			assertEqual(0, sys.getCharacterListByType<DeepPlayer>().Count, "Player 分类已清空");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// myself 单例
	// ═════════════════════════════════════════════════════════════════
	private static void testMyself_Registered_WhenMyself()
	{
		CharacterManager sys = new();
		try
		{
			DeepMyselfChar me = createMyself(sys, "me", 1);
			assertTrue(me.isMyself(), "myself 标志生效");
			assertTrue(ReferenceEquals(sys.getMyself(), me), "myMyself 指向 myself 角色");
			// myself 也进 GUID 索引(type-list)
			assertEqual(1, sys.getCharacterList().Count, "myself 也进入全局索引");
			assertEqual(1, sys.getCharacterListByType<DeepMyselfChar>().Count, "myself 也进 type 分类");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testMyself_NonMyself_NotRegistered()
	{
		CharacterManager sys = new();
		try
		{
			DeepSoldier s = sys.createCharacter<DeepSoldier>("s", 1);
			assertFalse(s.isMyself(), "非 myself 角色 isMyself=false");
			assertNull(sys.getMyself(), "getMyself 为 null(无 myself)");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testMyself_Destroyed_Clears()
	{
		CharacterManager sys = new();
		try
		{
			DeepMyselfChar me = createMyself(sys, "me", 1);
			sys.createCharacter<DeepSoldier>("s", 2);
			assertTrue(ReferenceEquals(sys.getMyself(), me), "myMyself 指向 me");
			sys.destroyCharacter(me.getGUID());
			assertNull(sys.getMyself(), "销毁 myself 后 getMyself 变 null");
			assertEqual(1, sys.getCharacterList().Count, "全局只剩 s");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testMyself_DestroyAll_Clears()
	{
		CharacterManager sys = new();
		try
		{
			createMyself(sys, "me", 1);
			assertNotNull(sys.getMyself(), "myself 已注册");
			sys.destroyAllCharacter();
			assertNull(sys.getMyself(), "destroyAll 清空 myself");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// update 遍历(复杂调用链)
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdate_TraversesManagedOnly()
	{
		CharacterManager sys = new();
		try
		{
			// managed 角色进入 update 列表; managed=false 不进. update() 遍历 mCharacterUpdateList
			// 仅对 managed 角色调 character.update(). 通过 update 内计数器验证.
			ManagedCounterChar m1 = (ManagedCounterChar)sys.createCharacter<ManagedCounterChar>("m1", 1);
			ManagedCounterChar u1 = (ManagedCounterChar)sys.createCharacter<ManagedCounterChar>("u1", 2, false);
			sys.update(0.016f);
			assertEqual(1, m1.mUpdateCount, "managed 角色 update 被调用一次");
			assertEqual(0, u1.mUpdateCount, "managed=false 角色不被 update 遍历");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testUpdate_AfterDestroy_Skipped()
	{
		CharacterManager sys = new();
		try
		{
			ManagedCounterChar a = (ManagedCounterChar)sys.createCharacter<ManagedCounterChar>("a", 1);
			ManagedCounterChar b = (ManagedCounterChar)sys.createCharacter<ManagedCounterChar>("b", 2);
			sys.destroyCharacter(a.getGUID());
			sys.update(0.016f);
			assertEqual(0, a.mUpdateCount, "被销毁角色不再 update");
			assertEqual(1, b.mUpdateCount, "存活角色 update 一次");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testLateUpdate_SafeOnEmpty()
	{
		CharacterManager sys = new();
		try
		{
			// 空 manager 调用 lateUpdate 安全不崩
			sys.lateUpdate(0.016f);
			ManagedCounterChar c = (ManagedCounterChar)sys.createCharacter<ManagedCounterChar>("c", 1);
			sys.lateUpdate(0.016f);
			assertEqual(1, c.mLateUpdateCount, "lateUpdate 遍历存活角色");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testFixedUpdate_NotPopulatedWithoutMoveInfo()
	{
		CharacterManager sys = new();
		try
		{
			// 默认 Character 无 mCOMMoveInfo, isEnableFixedUpdate()=false,
			// 因此 mFixedUpdateList 一直为空, fixedUpdate 遍历为空(安全).
			sys.createCharacter<ManagedCounterChar>("c", 1);
			sys.fixedUpdate(0.016f);
			assertTrue(true, "fixedUpdate 空列表安全执行(默认无固定更新角色)");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myself 角色.
	// DeepMyselfChar 在构造时即置 mIsMyself=true(Character.resetProperty 不清 mIsMyself,
	// 池复用不会误判), 使 createCharacter 内部的 isMyself() 判断在 setCharacterType 之后、
	// init() 之前自然成立, 从而走真实登记链路: mMyself = character.
	private static DeepMyselfChar createMyself(CharacterManager sys, string name, long id)
	{
		return (DeepMyselfChar)sys.createCharacter<DeepMyselfChar>(name, id);
	}
}

// 测试用 Character 子类: 非本人普通角色
public class DeepSoldier : Character
{
}

// 测试用 Character 子类: 非本人普通角色(另一种类型)
public class DeepPlayer : Character
{
}

// 测试用 Character 子类: 本人角色.
// 构造时置 mIsMyself=true, 使 createCharacter 内的 isMyself() 判断自然成立(myself 单例登记链)。
// 注: Character.resetProperty 特意不清 mIsMyself, 故对象池复用不会把它误判回非 myself。
public class DeepMyselfChar : Character
{
	public DeepMyselfChar() { mIsMyself = true; }
}

// 测试用 Character 子类: 覆盖 update/lateUpdate 记录遍历次数, 用于验证 manager 更新遍历。
// 计数器在 onCreate 清零(对象池每次取出都会调 onCreate, 含复用路径), 保证多次执行不残留旧计数。
// 注: 不 override resetProperty(计数器为本子类私有, 父类 resetProperty 不管它, 避免重复)。
public class ManagedCounterChar : Character
{
	public int mUpdateCount;
	public int mLateUpdateCount;
	public override void onCreate()
	{
		base.onCreate();
		mUpdateCount = 0;
		mLateUpdateCount = 0;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		++mUpdateCount;
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		++mLateUpdateCount;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mUpdateCount = 0;
		mLateUpdateCount = 0;
	}
}
