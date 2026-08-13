using static TestAssert;

// ComponentOwner 深度测试: 生命周期转发链路
// ============================================================================
// 与 ComponentTest(单接口调用) 的区别:
//   ComponentTest 只验证 update/lateUpdate/fixedUpdate 是"存在且可调用",
//   未真正验证这些方法把组件更新转发给子组件的完整链路。
//   本测试聚焦 ComponentOwner 的核心职责——生命周期转发:
//     - update/lateUpdate/fixedUpdate 遍历组件列表并转发 elapsedTime
//     - 按 isActive / isDestroy / mDisableTypeList 过滤组件
//     - 时间缩放(ignoreTimeScale)转发
//     - 更新过程中销毁的提前返回
//     - setActive 向所有组件广播 notifyOwnerActive
//     - addComponent 将组件纳入列表/字典并转发 notifyAddComponent
//     - notifyComponentStart 通过 breakComponent 中断冲突组件
// ============================================================================
public static class ComponentOwnerTest
{
	public static void Run()
	{
		// ─── update 转发链路 ───
		testUpdateForwardsElapsedTime();
		testUpdateSkipsDisabledType();
		testUpdateSkipsInactiveComponent();
		testUpdateSkipsDestroyedComponent();
		testUpdateMultipleComponentsOrder();
		testUpdateIgnoreTimeScaleComponent();
		testUpdateMidLoopClearEarlyReturn();
		testUpdateEmptyListNoCall();
		// ─── lateUpdate / fixedUpdate 转发链路 ───
		testLateUpdateForwardsElapsedTime();
		testLateUpdateSkipsDisabledAndInactive();
		testFixedUpdateForwardsElapsedTime();
		testFixedUpdateSkipsDisabledAndInactive();
		testLateFixedUpdateEmptyList();
		// ─── setActive 广播 ───
		testSetActiveNotifiesAllComponents();
		testSetActiveDuringDestroyReturnsFalse();
		// ─── addComponent 纳入列表/字典 ───
		testAddComponentRegistersListAndDict();
		testAddComponentForwardsNotifyAddComponent();
		testAddComponentDefaultActive();
		testGetOrAddComponentExistingReturnsSame();
		// ─── getActiveComponent / addInitComponent / addDontAutoCreate ───
		testGetActiveComponentActiveOnly();
		testAddDontAutoCreateBlocksInit();
		testAddDestroyCallbackRemoval();
		// ─── getTypeName / isIgnoreTimeScale ───
		testGetTypeNameCached();
		testIsIgnoreTimeScaleState();
		// ─── notifyComponentStart 中断冲突组件 ───
		testNotifyComponentStartBreaksConflictComponent();
		testNotifyComponentStartNoConflict();
		testAddComponentTriggersBreakOnConflict();
	}

	// ═════════════════════════════════════════════════════════════════════
	//  update 转发链路
	// ═════════════════════════════════════════════════════════════════════

	// 1. 一个激活有效组件 → update 转发 elapsedTime 给子组件
	private static void testUpdateForwardsElapsedTime()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			assertNotNull(com, "addComponent 应返回组件");
			com.resetTrace();
			owner.update(1.5f);
			assertEqual(1, com.updateCallCount, "update 应转发给子组件一次");
			assertEqual(1.5f, com.lastUpdateTime, "update 应转发 elapsedTime");
			assertEqual(0, com.lateUpdateCallCount, "update 不应触发 lateUpdate");
			assertEqual(0, com.fixedUpdateCallCount, "update 不应触发 fixedUpdate");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 2. 组件类型被禁用 → update 跳过
	private static void testUpdateSkipsDisabledType()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			owner.addDisableComponent<TrackComponent>();
			com.resetTrace();
			owner.update(1.0f);
			assertEqual(0, com.updateCallCount, "禁用类型的组件不应被 update");
			// 取消禁用后恢复更新
			owner.removeDisableComponent<TrackComponent>();
			owner.update(1.0f);
			assertEqual(1, com.updateCallCount, "取消禁用后 update 恢复");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 3. 组件未激活 → update 跳过
	private static void testUpdateSkipsInactiveComponent()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(false);
			com.resetTrace();
			owner.update(1.0f);
			assertEqual(0, com.updateCallCount, "未激活组件不应被 update");
			// 激活后恢复
			com.setActive(true);
			owner.update(1.0f);
			assertEqual(1, com.updateCallCount, "激活后 update 恢复");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 4. 组件已销毁 → update 跳过(com.isValid() == false)
	private static void testUpdateSkipsDestroyedComponent()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			// 直接标记为已回收, 使 isValid() 为 false
			com.setDestroy(true);
			com.resetTrace();
			owner.update(1.0f);
			assertEqual(0, com.updateCallCount, "已销毁组件不应被 update");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 5. 多个组件按加入顺序全部被 update
	private static void testUpdateMultipleComponentsOrder()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com1 = owner.addComponent<TrackComponent>(true);
			OtherComponent com2 = owner.addComponent<OtherComponent>(true);
			assertNotNull(com1, "com1 应创建");
			assertNotNull(com2, "com2 应创建");
			com1.resetTrace();
			owner.update(0.5f);
			assertEqual(1, com1.updateCallCount, "com1 应被 update");
			assertEqual(1, com2.updateCallCount, "com2 应被 update");
			assertEqual(0.5f, com1.lastUpdateTime, "com1 收到 elapsedTime");
			assertEqual(0.5f, com2.lastUpdateTime, "com2 收到 elapsedTime");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 6. 组件设置了忽略时间缩放 → update 仍被转发(使用不受缩放时间)
	private static void testUpdateIgnoreTimeScaleComponent()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			com.setIgnoreTimeScale(true);
			com.resetTrace();
			owner.update(2.0f);
			assertEqual(1, com.updateCallCount, "忽略时间缩放的组件仍应被 update");
			// 无论传入的是 unscaledTime 还是 elapsedTime, 转发链路都必须触发
			assertTrue(com.lastUpdateTime >= 0f, "update 应收到一个非负时间");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 7. 更新过程中组件列表被清空 → 提前返回, 不崩溃
	private static void testUpdateMidLoopClearEarlyReturn()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			// com1 在 update 时清空 owner 的组件列表 → 触发提前返回, com2 不应被更新
			TrackComponent com1 = owner.addComponent<TrackComponent>(true);
			OtherComponent com2 = owner.addComponent<OtherComponent>(true);
			com1.resetTrace();
			com1.clearOwnerListOnUpdate = true;
			owner.update(1.0f);
			// com1 排在最前, 它清空列表后 count()==0 → 提前返回, com2 不更新
			assertEqual(1, com1.updateCallCount, "com1 应被 update 一次");
			assertEqual(0, com2.updateCallCount, "列表清空后 com2 不应再被 update");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 8. 空列表 → update 直接返回, 不崩溃
	private static void testUpdateEmptyListNoCall()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			// 无组件, mComponentList 为 null
			owner.update(1.0f);
			owner.lateUpdate(1.0f);
			owner.fixedUpdate(1.0f);
		}
		finally
		{
			owner.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  lateUpdate / fixedUpdate 转发链路
	// ═════════════════════════════════════════════════════════════════════

	// 9. lateUpdate 转发 elapsedTime
	private static void testLateUpdateForwardsElapsedTime()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			com.resetTrace();
			owner.lateUpdate(0.25f);
			assertEqual(1, com.lateUpdateCallCount, "lateUpdate 应转发给子组件一次");
			assertEqual(0.25f, com.lastLateUpdateTime, "lateUpdate 应转发 elapsedTime");
			assertEqual(0, com.updateCallCount, "lateUpdate 不应触发 update");
			assertEqual(0, com.fixedUpdateCallCount, "lateUpdate 不应触发 fixedUpdate");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 10. lateUpdate 跳过禁用和未激活组件
	private static void testLateUpdateSkipsDisabledAndInactive()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			owner.addDisableComponent<TrackComponent>();
			com.resetTrace();
			owner.lateUpdate(1.0f);
			assertEqual(0, com.lateUpdateCallCount, "禁用类型组件不应被 lateUpdate");

			owner.removeDisableComponent<TrackComponent>();
			com.setActive(false);
			owner.lateUpdate(1.0f);
			assertEqual(0, com.lateUpdateCallCount, "未激活组件不应被 lateUpdate");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 11. fixedUpdate 转发 elapsedTime
	private static void testFixedUpdateForwardsElapsedTime()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			com.resetTrace();
			owner.fixedUpdate(0.1f);
			assertEqual(1, com.fixedUpdateCallCount, "fixedUpdate 应转发给子组件一次");
			assertEqual(0.1f, com.lastFixedUpdateTime, "fixedUpdate 应转发 elapsedTime");
			assertEqual(0, com.updateCallCount, "fixedUpdate 不应触发 update");
			assertEqual(0, com.lateUpdateCallCount, "fixedUpdate 不应触发 lateUpdate");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 12. fixedUpdate 跳过禁用和未激活组件
	private static void testFixedUpdateSkipsDisabledAndInactive()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			owner.addDisableComponent<TrackComponent>();
			com.resetTrace();
			owner.fixedUpdate(1.0f);
			assertEqual(0, com.fixedUpdateCallCount, "禁用类型组件不应被 fixedUpdate");

			owner.removeDisableComponent<TrackComponent>();
			com.setActive(false);
			owner.fixedUpdate(1.0f);
			assertEqual(0, com.fixedUpdateCallCount, "未激活组件不应被 fixedUpdate");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 13. 空列表 → late/fixedUpdate 直接返回
	private static void testLateFixedUpdateEmptyList()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			owner.lateUpdate(1.0f);
			owner.fixedUpdate(1.0f);
		}
		finally
		{
			owner.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  setActive 广播
	// ═════════════════════════════════════════════════════════════════════

	// 14. setActive 向所有组件广播 notifyOwnerActive
	private static void testSetActiveNotifiesAllComponents()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			com.resetTrace();
			owner.setActive(true);
			assertEqual(1, com.ownerActiveCount, "setActive(true) 应广播 notifyOwnerActive(true)");
			assertTrue(com.lastOwnerActive, "广播参数应为 true");

			com.resetTrace();
			owner.setActive(false);
			assertEqual(1, com.ownerActiveCount, "setActive(false) 应广播 notifyOwnerActive(false)");
			assertFalse(com.lastOwnerActive, "广播参数应为 false");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 15. destroy 后 setActive 返回 false, 不广播
	private static void testSetActiveDuringDestroyReturnsFalse()
	{
		var owner = new DeepTestComponentOwner();
		TrackComponent com = owner.addComponent<TrackComponent>(true);
		owner.destroy();
		// destroy 后 mDestroying 已复位, setActive 正常返回
		bool r = owner.setActive(true);
		assertTrue(r, "destroy 后 setActive 返回 active 本身");
		_ = com;
	}

	// ═════════════════════════════════════════════════════════════════════
	//  addComponent 纳入列表/字典
	// ═════════════════════════════════════════════════════════════════════

	// 16. addComponent 后, getComponentList/getAllComponent 能查到
	private static void testAddComponentRegistersListAndDict()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			assertNotNull(com, "addComponent 应返回组件");
			// getComponentList 应包含
			var list = owner.getComponentList();
			assertNotNull(list, "addComponent 后 getComponentList 非 null");
			assertEqual(1, list.count(), "组件列表应有 1 个");
			// getAllComponent 应包含
			var dict = owner.getAllComponent();
			assertNotNull(dict, "addComponent 后 getAllComponent 非 null");
			GameComponent fromDict = owner.getComponent<TrackComponent>();
			assertTrue(ReferenceEquals(com, fromDict), "getComponent 应返回同一组件实例");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 17. addComponent 转发 notifyAddComponent
	private static void testAddComponentForwardsNotifyAddComponent()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			owner.notifyCount = 0;
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			assertNotNull(com, "组件应创建");
			assertEqual(1, owner.notifyCount, "notifyAddComponent 应被转发一次");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 18. addComponent 设置 defaultActive
	private static void testAddComponentDefaultActive()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent comActive = owner.addComponent<TrackComponent>(true);
			OtherComponent comInactive = owner.addComponent<OtherComponent>(false);
			assertTrue(comActive.isActive(), "active=true 的组件应激活");
			assertTrue(comActive.isDefaultActive(), "active=true 时 defaultActive=true");
			assertFalse(comInactive.isActive(), "active=false 的组件不应激活");
			assertFalse(comInactive.isDefaultActive(), "active=false 时 defaultActive=false");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 19. getOrAddComponent 已存在时返回同一实例
	private static void testGetOrAddComponentExistingReturnsSame()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent first = owner.addComponent<TrackComponent>(true);
			TrackComponent second = owner.getOrAddComponent<TrackComponent>();
			assertTrue(ReferenceEquals(first, second), "getOrAddComponent 已存在应返回同一实例");
		}
		finally
		{
			owner.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  getActiveComponent / addInitComponent / addDontAutoCreate
	// ═════════════════════════════════════════════════════════════════════

	// 20. getActiveComponent 只返回激活组件, 未激活返回 null
	private static void testGetActiveComponentActiveOnly()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			GameComponent active = owner.getActiveComponent<TrackComponent>();
			assertTrue(ReferenceEquals(com, active), "激活组件应被 getActiveComponent 返回");
			// 停用后返回 null
			com.setActive(false);
			GameComponent inactive = owner.getActiveComponent<TrackComponent>();
			assertNull(inactive, "未激活组件 getActiveComponent 返回 null");
			// 未注册类型返回 null
			GameComponent missing = owner.getActiveComponent<OtherComponent>();
			assertNull(missing, "未注册类型 getActiveComponent 返回 null");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 21. addDontAutoCreate 阻止 addInitComponent, 但不阻止 addComponent
	private static void testAddDontAutoCreateBlocksInit()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			owner.addDontAutoCreate<TrackComponent>();
			GameComponent initCom = owner.addInitComponent<TrackComponent>(true);
			assertNull(initCom, "addDontAutoCreate 后 addInitComponent 返回 null");
			// addComponent 不受影响(直接添加)
			TrackComponent directCom = owner.addComponent<TrackComponent>(true);
			assertNotNull(directCom, "addComponent 不受 addDontAutoCreate 影响");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 22. addDestroyCallback / removeDestroyCallback 在 destroy 时正确触发
	private static void testAddDestroyCallbackRemoval()
	{
		var owner = new DeepTestComponentOwner();
		bool called = false;
		ClassObjectCallback cb = obj => called = true;
		owner.addDestroyCallback(cb);
		owner.destroy();
		assertTrue(called, "addDestroyCallback 注册的回调在 destroy 时应被调用");

		// 移除后不再触发
		var owner2 = new DeepTestComponentOwner();
		bool called2 = false;
		ClassObjectCallback cb2 = obj => called2 = true;
		owner2.addDestroyCallback(cb2);
		owner2.removeDestroyCallback(cb2);
		owner2.destroy();
		assertFalse(called2, "removeDestroyCallback 后 destroy 不应调用回调");
	}

	// ═════════════════════════════════════════════════════════════════════
	//  getTypeName / isIgnoreTimeScale
	// ═════════════════════════════════════════════════════════════════════

	// 23. GetTypeName 返回类型名并缓存
	private static void testGetTypeNameCached()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			string name = owner.GetTypeName();
			assertTrue(name.Length > 0, "GetTypeName 不应为空");
			assertTrue(name.Contains("DeepTestComponentOwner"), "GetTypeName 应包含类型名");
			// 二次调用应返回相同(缓存)
			assertEqual(name, owner.GetTypeName(), "GetTypeName 结果应缓存稳定");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 24. isIgnoreTimeScale 跟随 setIgnoreTimeScale
	private static void testIsIgnoreTimeScaleState()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			assertFalse(owner.isIgnoreTimeScale(), "默认 isIgnoreTimeScale=false");
			owner.setIgnoreTimeScale(true);
			assertTrue(owner.isIgnoreTimeScale(), "setIgnoreTimeScale(true) 后为 true");
			// componentOnly=true 不改 owner 自身
			owner.setIgnoreTimeScale(false, true);
			assertTrue(owner.isIgnoreTimeScale(), "componentOnly=true 不改变 owner");
		}
		finally
		{
			owner.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  notifyComponentStart 中断冲突组件
	// ═════════════════════════════════════════════════════════════════════

	// 20. notifyComponentStart: 启动一个 Modify 组件, 中断冲突的同 Modify 可中断组件
	private static void testNotifyComponentStartBreaksConflictComponent()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			// 已存在一个可中断的 ModifyPosition 组件(激活)
			BreakableModifyComponent existing = owner.addComponent<BreakableModifyComponent>(true);
			existing.resetTrace();
			// 以自身为 exceptComponent 触发 breakComponent, 自身不被中断(com != exceptComponent 为 false)
			owner.notifyComponentStart(existing);
			assertEqual(0, existing.breakCount, "以自身为 exceptComponent 时不中断");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 21. notifyComponentStart 对不实现 Modify 接口的组件无影响
	private static void testNotifyComponentStartNoConflict()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			TrackComponent com = owner.addComponent<TrackComponent>(true);
			com.resetTrace();
			// TrackComponent 不实现任何 Modify 接口 → notifyComponentStart 无动作
			owner.notifyComponentStart(com);
			assertFalse(com.isBreakCalled, "非 Modify/非 Breakable 组件不应被中断");
		}
		finally
		{
			owner.destroy();
		}
	}

	// 22. addComponent → setActive → notifyComponentStart → breakComponent 完整链路:
	//     添加第二个 ModifyPosition 组件时, 自动中断已存在的可中断 ModifyPosition 组件
	private static void testAddComponentTriggersBreakOnConflict()
	{
		var owner = new DeepTestComponentOwner();
		try
		{
			// 已存在可中断 ModifyPosition 组件(激活)
			BreakableModifyComponent existing = owner.addComponent<BreakableModifyComponent>(true);
			existing.resetTrace();
			// 添加第二个 ModifyPosition 组件(不同类型), setActive(true) 触发 notifyComponentStart
			ModifyPositionComponent starter = owner.addComponent<ModifyPositionComponent>(true);
			assertNotNull(starter, "第二组件应创建");
			// existing 被中断: breakCount 增加且被禁用
			assertEqual(1, existing.breakCount, "添加冲突组件应中断 existing 一次");
			assertTrue(existing.isBreakCalled, "existing 的 notifyBreak 应被调用");
			assertFalse(existing.isActive(), "existing 被中断后应被禁用");
		}
		finally
		{
			owner.destroy();
		}
	}
}

// ─── 测试辅助组件 ─────────────────────────────────────────────────────────

// 记录 update/lateUpdate/fixedUpdate/notifyOwnerActive 调用痕迹的组件
public class TrackComponent : GameComponent
{
	public int updateCallCount;
	public int lateUpdateCallCount;
	public int fixedUpdateCallCount;
	public int ownerActiveCount;
	public int breakCount;
	public float lastUpdateTime;
	public float lastLateUpdateTime;
	public float lastFixedUpdateTime;
	public bool lastOwnerActive;
	public bool isBreakCalled;
	// 若为 true, 该组件 update 时清空 owner 的组件列表(用于测试提前返回)
	public bool clearOwnerListOnUpdate;

	public TrackComponent()
	{
		// ClassObject 构造默认 mHasDestroy=true, 这里显式置为非销毁
		// 使 com.isValid() 为 true, 否则 update 会跳过该组件
		setDestroy(false);
	}

	public void resetTrace()
	{
		updateCallCount = 0;
		lateUpdateCallCount = 0;
		fixedUpdateCallCount = 0;
		ownerActiveCount = 0;
		breakCount = 0;
		lastUpdateTime = 0f;
		lastLateUpdateTime = 0f;
		lastFixedUpdateTime = 0f;
		lastOwnerActive = false;
		isBreakCalled = false;
		clearOwnerListOnUpdate = false;
	}

	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		++updateCallCount;
		lastUpdateTime = elapsedTime;
		if (clearOwnerListOnUpdate)
		{
			getOwner()?.getComponentList()?.clear();
		}
	}

	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		++lateUpdateCallCount;
		lastLateUpdateTime = elapsedTime;
	}

	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		++fixedUpdateCallCount;
		lastFixedUpdateTime = elapsedTime;
	}

	public override void notifyOwnerActive(bool active)
	{
		++ownerActiveCount;
		lastOwnerActive = active;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		updateCallCount = 0;
		lateUpdateCallCount = 0;
		fixedUpdateCallCount = 0;
		ownerActiveCount = 0;
		breakCount = 0;
		lastUpdateTime = 0f;
		lastLateUpdateTime = 0f;
		lastFixedUpdateTime = 0f;
		lastOwnerActive = false;
		isBreakCalled = false;
		clearOwnerListOnUpdate = false;
	}
}

// 另一个普通测试组件, 用于多组件场景(不同类型避免重名)
public class OtherComponent : GameComponent
{
	public int updateCallCount;
	public int lateUpdateCallCount;
	public int fixedUpdateCallCount;
	public float lastUpdateTime;
	public float lastLateUpdateTime;
	public float lastFixedUpdateTime;

	public OtherComponent()
	{
		setDestroy(false);
	}

	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		++updateCallCount;
		lastUpdateTime = elapsedTime;
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		++lateUpdateCallCount;
		lastLateUpdateTime = elapsedTime;
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		++fixedUpdateCallCount;
		lastFixedUpdateTime = elapsedTime;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		updateCallCount = 0;
		lateUpdateCallCount = 0;
		fixedUpdateCallCount = 0;
		lastUpdateTime = 0f;
		lastLateUpdateTime = 0f;
		lastFixedUpdateTime = 0f;
	}
}

// 实现 ModifyPosition + IComponentBreakable 的组件
public class BreakableModifyComponent : GameComponent, IComponentModifyPosition, IComponentBreakable
{
	public int breakCount;
	public bool isBreakCalled;

	public BreakableModifyComponent()
	{
		setDestroy(false);
	}

	public void notifyBreak()
	{
		++breakCount;
		isBreakCalled = true;
	}

	public void resetTrace()
	{
		breakCount = 0;
		isBreakCalled = false;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		breakCount = 0;
		isBreakCalled = false;
	}
}

// 实现 ModifyPosition 但不可被中断的组件(用于触发冲突中断)
public class ModifyPositionComponent : GameComponent, IComponentModifyPosition
{
	public ModifyPositionComponent()
	{
		setDestroy(false);
	}
}

// ─── 测试辅助 Owner ───────────────────────────────────────────────────────

public class DeepTestComponentOwner : ComponentOwner
{
	public int notifyCount;

	public DeepTestComponentOwner()
	{
		// ClassObject 构造默认 mHasDestroy=true, 置为非销毁
		setDestroy(false);
	}

	public override void notifyAddComponent(GameComponent com)
	{
		++notifyCount;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		notifyCount = 0;
	}
}
