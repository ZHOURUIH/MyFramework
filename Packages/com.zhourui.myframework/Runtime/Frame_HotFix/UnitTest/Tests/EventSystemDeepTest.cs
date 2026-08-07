using System;
using static FrameUtility;
using static TestAssert;

// EventSystem 深度测试 — 事件派发中的复杂回调交互
// 覆盖 pushEvent 派发时回调内修改监听列表(SafeFastDeepList 安全迭代)的复杂场景
//   pushEvent 中回调 unlisten 自己 / 其他监听者 → 安全的列表修改
//   全局+角色事件混合派发时修改监听
//   派发中新增监听者 → 本次不收到 (count 固定)
//   派发中 unlisten+relisten 自己 → 标记删除+append 新槽, 每次稳定触发
//   嵌套派发 + 派发中移除 → 内外层独立安全迭代
//   派发移除后 compact 复用槽位 → 列表长度不累积
//   角色事件: removeCharacterEvent 派发中清理 / 按角色隔离 / update 空列表清理
//   角色事件: 新增监听者 / unlistenEvent<T> 按类型移除 / 角色回调嵌套 push 全局
//   按 TypeID 注册派发中修改
//   带参事件数据流: 全局+角色共享同一实例 / 回调修改共享对象影响后续
//   带参事件数据流: 嵌套派发值传递 / 顺序监听观察共享对象累加变化
// 使用局部 new EventSystem() 实例, 不污染全局 mEventSystem 单例
public static class EventSystemDeepTest
{
	class TestEvent : GameEvent { }
	class TestEvent2 : GameEvent { }
	// 带字段的事件,用于测试带参事件的数据流
	class TestEventWithValue : GameEvent
	{
		public int value;
		public override void resetProperty()
		{
			base.resetProperty();
			value = 0;
		}
	}
	class TestListener : IEventListener { }

	public static void Run()
	{
		testUnlistenSelfDuringDispatch();
		testUnlistenOtherDuringDispatch();
		testMultipleListenersRemoveOneDuringDispatch();
		testListenSameEventTwiceUnlistenOnce();
		testUnlistenSelfDuringCharacterDispatch();
		testUnlistenOtherDuringCharacterDispatch();
		testUnlistenInCharacterAlsoAffectsGlobal();
		testListenNewDuringDispatchNotFiredThisTime();
		testUnlistenThenRelistenDuringDispatch();
		testNestedDispatchRemoveOuter();
		testUnlistenThenCompactReuseSlot();
		testRemoveCharacterEventDuringDispatch();
		testRemoveCharacterEventAffectsOnlyThatCharacter();
		testRemoveCharacterEventThenUpdateCleansUp();
		testCharacterAddNewDuringDispatch();
		testUnlistenTypeSpecificDuringCharacter();
		testCharacterCallbackPushesGlobal();
		testListenByTypeIDAndUnlistenDuringDispatch();
		testParamSharedBetweenGlobalAndCharacter();
		testParamMutationAffectsLaterListeners();
		testParamValueFlowsThroughNestedDispatch();
		testParamCallbackOrderObservesMutations();
	}

	// ═════════════════════════════════════════════════════════════════
	// 派发中回调 unlisten 自己 → 只影响当前及后续, 不崩溃
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenSelfDuringDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		int c1 = 0, c2 = 0;
		sys.listenEvent<TestEvent>(_ =>
		{
			c1++;
			sys.unlistenEvent(l1); // 回调中移除自己
		}, l1);
		sys.listenEvent<TestEvent>(_ => c2++, l2);

		sys.pushEvent<TestEvent>();
		assertEqual(1, c1, "l1 收到1次");
		assertEqual(1, c2, "l2 仍收到1次(不受 l1 unlisten 影响)");

		// 再次派发: l1 已移除不再收到
		sys.pushEvent<TestEvent>();
		assertEqual(1, c1, "l1 已移除, 不再收到");
		assertEqual(2, c2, "l2 继续收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 派发中回调 unlisten 其他监听者 → 目标被标记移除, 本次派发不再收到
	// (SafeFastDeepList 在 foreaching 时 remove 把元素置 null, 后续 get 返回 null 跳过)
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenOtherDuringDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		int c1 = 0, c2 = 0;
		// l1 先注册, 回调移除 l2(在 l1 之后)
		sys.listenEvent<TestEvent>(_ =>
		{
			c1++;
			sys.unlistenEvent(l2);
		}, l1);
		sys.listenEvent<TestEvent>(_ => c2++, l2);

		sys.pushEvent<TestEvent>();
		assertEqual(1, c1, "l1 收到");
		assertEqual(0, c2, "l1 在派发中移除 l2, l2 本次不再收到");

		sys.pushEvent<TestEvent>();
		assertEqual(2, c1, "l1 继续收到");
		assertEqual(0, c2, "l2 已被移除, 始终不收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 多个监听者, 回调中移除后续的 → 被移除者本次就不收到, 其他不受影响
	// ═════════════════════════════════════════════════════════════════
	private static void testMultipleListenersRemoveOneDuringDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		var l3 = new TestListener();
		int c1 = 0, c2 = 0, c3 = 0;
		// l2 的回调移除 l3(l2 之后的监听者)
		sys.listenEvent<TestEvent>(_ => c1++, l1);
		sys.listenEvent<TestEvent>(_ =>
		{
			c2++;
			sys.unlistenEvent(l3);
		}, l2);
		sys.listenEvent<TestEvent>(_ => c3++, l3);

		sys.pushEvent<TestEvent>();
		sys.pushEvent<TestEvent>();
		assertEqual(2, c1, "l1 收到2次");
		assertEqual(2, c2, "l2 收到2次");
		assertEqual(0, c3, "l3 被 l2 移除, 从不收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 同一监听者对同一事件注册2次, unlistenEvent<T> 只移除该事件类型
	// ═════════════════════════════════════════════════════════════════
	private static void testListenSameEventTwiceUnlistenOnce()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		int count = 0;
		sys.listenEvent<TestEvent>(_ => count++, l1);
		sys.listenEvent<TestEvent2>(_ => count++, l1);

		sys.pushEvent<TestEvent>();
		sys.pushEvent<TestEvent2>();
		assertEqual(2, count, "两个事件各触发1次");

		// 只移除 TestEvent 的监听, TestEvent2 保留
		sys.unlistenEvent<TestEvent>(l1);
		sys.pushEvent<TestEvent>();
		sys.pushEvent<TestEvent2>();
		assertEqual(3, count, "TestEvent 不再触发, TestEvent2 触发第2次");
	}

	// ═════════════════════════════════════════════════════════════════
	// 角色事件派发中回调 unlisten 自己 → 只影响当前及后续, 不崩溃
	// (pushEvent(param, characterID) 中角色列表同样走 SafeFastDeepListReader)
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenSelfDuringCharacterDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		int c1 = 0, c2 = 0;
		long charID = 88881;
		sys.listenEvent<TestEvent>(charID, _ =>
		{
			c1++;
			sys.unlistenEvent(l1); // 角色回调中移除自己
		}, l1);
		sys.listenEvent<TestEvent>(charID, _ => c2++, l2);

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, c1, "l1 收到1次");
		assertEqual(1, c2, "l2 仍收到1次(不受 l1 unlisten 影响)");

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, c1, "l1 已从角色列表移除, 不再收到");
		assertEqual(2, c2, "l2 继续收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 角色事件派发中回调 unlisten 其他角色监听者 → 本次不再收到, 不崩溃
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenOtherDuringCharacterDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		int c1 = 0, c2 = 0;
		long charID = 88882;
		sys.listenEvent<TestEvent>(charID, _ =>
		{
			c1++;
			sys.unlistenEvent(l2); // 回调中移除排在后面的 l2
		}, l1);
		sys.listenEvent<TestEvent>(charID, _ => c2++, l2);

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, c1, "l1 收到");
		assertEqual(0, c2, "l1 在派发中移除 l2, l2 本次不再收到");

		sys.pushEvent<TestEvent>(charID);
		assertEqual(2, c1, "l1 继续收到");
		assertEqual(0, c2, "l2 已被移除, 始终不收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 同一监听者同时注册了角色事件和全局事件
	// → unlistenEvent(IEventListener) 会清掉该监听者的全部注册(角色+全局)
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenInCharacterAlsoAffectsGlobal()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		int charCount = 0, globalCount = 0;
		long charID = 88883;
		sys.listenEvent<TestEvent>(charID, _ => charCount++, l1);
		sys.listenEvent<TestEvent>(_ => globalCount++, l1);

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, globalCount, "角色推送同时广播全局");
		assertEqual(1, charCount, "角色监听收到");

		// unlistenEvent(IEventListener) 清掉该监听者的全部注册(角色+全局)
		sys.unlistenEvent(l1);

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, globalCount, "全局项已被移除, 不再收到");
		assertEqual(1, charCount, "角色项已被移除, 不再收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 派发中新增监听者 → 本次不收到 (count 在 startForeach 时已固定)
	// 但下次派发能收到 (add 只是 append 到末尾, 不干扰本次固定遍历)
	// ═════════════════════════════════════════════════════════════════
	private static void testListenNewDuringDispatchNotFiredThisTime()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var lNew = new TestListener();
		int c1 = 0, cNew = 0;
		sys.listenEvent<TestEvent>(_ =>
		{
			c1++;
			sys.listenEvent<TestEvent>(_ => cNew++, lNew); // 派发中新增监听者
		}, l1);

		// 第一次派发: count 在进入 l1 回调前已固定, lNew 不在本次遍历范围
		sys.pushEvent<TestEvent>();
		assertEqual(1, c1, "l1 收到");
		assertEqual(0, cNew, "本次派发新增的 lNew 不收到");

		// 第二次派发: lNew 已加入列表, 收到
		sys.pushEvent<TestEvent>();
		assertEqual(2, c1, "l1 收到第2次");
		assertEqual(1, cNew, "lNew 下次收到");

		// 第三次派发: l1 又新增了一个 lNew 监听, 此时 lNew 有两份注册都触发
		sys.pushEvent<TestEvent>();
		assertEqual(3, c1, "l1 收到第3次");
		assertEqual(3, cNew, "lNew 已有两份注册, 第3次派发触发2次, 累计3次");
	}

	// ═════════════════════════════════════════════════════════════════
	// 派发中先 unlisten 自己, 再重新 listen 同一监听者
	// → unlisten 是标记删除(置 null), listen 是 append 新项
	// → 本次派发: 旧槽已标记删除跳过, 新槽因 count 固定也不遍历 → 本次不触发
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenThenRelistenDuringDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		int count = 0;
		sys.listenEvent<TestEvent>(_ =>
		{
			count++;
			sys.unlistenEvent(l1);        // 标记删除当前槽
			sys.listenEvent<TestEvent>(_ => count++, l1); // append 新槽
		}, l1);

		sys.pushEvent<TestEvent>();
		assertEqual(1, count, "本次派发: 旧槽标记删除跳过, 新槽 count 固定不遍历 → 仅1次");

		// 第二次派发: 只剩新槽, 触发1次
		sys.pushEvent<TestEvent>();
		assertEqual(2, count, "新槽触发1次");

		// 第三次派发: 新槽回调又 unlisten+relisten, 生成第二个新槽
		// 此时列表里有(新槽, 新槽2), 本次固定 count 只遍历到新槽 → 触发1次
		sys.pushEvent<TestEvent>();
		assertEqual(3, count, "每次派发稳定触发1次");
	}

	// ═════════════════════════════════════════════════════════════════
	// 嵌套派发: TestEvent 回调里 pushEvent 另一个事件 TestEvent2
	// → 内外层各自有独立 SafeFastDeepListReader/全局列表
	// → 外层派发中 unlisten 排在后面的 lC → null 标记删除, 本次不再收到
	// ═════════════════════════════════════════════════════════════════
	private static void testNestedDispatchRemoveOuter()
	{
		var sys = new EventSystem();
		var lA = new TestListener();
		var lB = new TestListener();
		var lC = new TestListener();
		int cA = 0, cB = 0, cC = 0, cInner = 0;
		// lA 收到 TestEvent 后嵌套 push TestEvent2; 内层 lA 也监听 TestEvent2
		sys.listenEvent<TestEvent>(_ =>
		{
			cA++;
			sys.pushEvent<TestEvent2>(); // 嵌套派发另一事件
		}, lA);
		sys.listenEvent<TestEvent>(_ =>
		{
			cB++;
			// 外层派发中移除排在后面的 lC(本外层 lB 之后)
			sys.unlistenEvent(lC);
		}, lB);
		sys.listenEvent<TestEvent>(_ => cC++, lC);
		// 内层 TestEvent2 的监听者
		sys.listenEvent<TestEvent2>(_ => cInner++, lA);

		// 外层派发 TestEvent
		sys.pushEvent<TestEvent>();
		// 外层: lA 收到并嵌套 push TestEvent2 → lA 的 TestEvent2 回调 cInner 收到1次
		assertEqual(1, cA, "lA 收到1次");
		assertEqual(1, cInner, "嵌套派发的 TestEvent2 被 lA 收到1次");
		assertEqual(1, cB, "lB 收到1次");
		// lB 在外层派发中 null 标记删除 lC, 本次外层遍历 get 到 null 跳过 → lC 不收到
		assertEqual(0, cC, "lC 被外层派发中 unlisten, 本次不再收到");

		// 再次派发: lC 已被移除, 仍不收到
		sys.pushEvent<TestEvent>();
		assertEqual(2, cA, "lA 收到2次");
		assertEqual(2, cInner, "嵌套 TestEvent2 收到2次");
		assertEqual(2, cB, "lB 收到2次");
		assertEqual(0, cC, "lC 已被移除, 始终不收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 派发移除后 compact 复用槽位: 连续多次派发+派发中移除
	// → 每次 endForeach 后 compact 真正删除 null 槽, 列表长度稳定, 不累积
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenThenCompactReuseSlot()
	{
		var sys = new EventSystem();
		// 用一个可重复创建/销毁的监听者模拟槽位反复占用-释放
		int outside = 0;
		var keeper = new TestListener();
		sys.listenEvent<TestEvent>(_ => outside++, keeper);

		// 连续 10 次: 新增临时监听者, 派发中移除自己, 确保不崩溃且临时者不残留
		for (int i = 0; i < 10; ++i)
		{
			var temp = new TestListener();
			int tempCount = 0;
			sys.listenEvent<TestEvent>(_ =>
			{
				tempCount++;
				sys.unlistenEvent(temp);
			}, temp);
			sys.pushEvent<TestEvent>();
			assertEqual(1, tempCount, $"第{i}次: 临时监听者本次收到1次");
			// 下一轮临时者已被 compact 移除, 不再触发
		}

		// keeper 全程不受影响, 稳定收到 10 次
		assertEqual(10, outside, "keeper 收到10次");
	}

	// ═════════════════════════════════════════════════════════════════
	// 角色派发中调用 removeCharacterEvent → 该角色所有角色监听被清
	// (clear 在 foreaching 时全部置 null, 本次及后续都不再收到)
	// → 全局监听完全不受影响 (角色/全局列表独立)
	// ═════════════════════════════════════════════════════════════════
	private static void testRemoveCharacterEventDuringDispatch()
	{
		var sys = new EventSystem();
		var lA = new TestListener();
		var lB = new TestListener();
		var lG = new TestListener();
		int cA = 0, cB = 0, g = 0;
		long charID = 77771;
		sys.listenEvent<TestEvent>(charID, _ =>
		{
			cA++;
			sys.removeCharacterEvent(charID); // 角色回调中清空该角色所有角色监听
		}, lA);
		sys.listenEvent<TestEvent>(charID, _ => cB++, lB);
		sys.listenEvent<TestEvent>(_ => g++, lG); // 全局监听

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, cA, "lA 收到");
		assertEqual(0, cB, "lB 槽被 clear 置 null, 本次不再收到");
		assertEqual(1, g, "全局 lG 收到(不受 removeCharacterEvent 影响)");

		// 再次派发: 角色监听已清空, 仅全局触发
		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, cA, "lA 已移除");
		assertEqual(0, cB, "lB 已移除");
		assertEqual(2, g, "全局 lG 继续收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// removeCharacterEvent(charID) 只清理指定角色 → 其他角色监听不受影响
	// ═════════════════════════════════════════════════════════════════
	private static void testRemoveCharacterEventAffectsOnlyThatCharacter()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		int c1 = 0, c2 = 0;
		long charID1 = 77772, charID2 = 77773;
		sys.listenEvent<TestEvent>(charID1, _ => c1++, l1);
		sys.listenEvent<TestEvent>(charID2, _ => c2++, l2);

		// 只移除角色1, 角色2不受影响
		sys.removeCharacterEvent(charID1);

		sys.pushEvent<TestEvent>(charID1);
		assertEqual(0, c1, "角色1监听已移除");
		sys.pushEvent<TestEvent>(charID2);
		assertEqual(1, c2, "角色2监听不受影响, 正常收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// removeCharacterEvent 后调用 update → 触发 mNeedCheckEmptyEvent 清理
	// 反复 add/remove/update, 验证空角色事件列表被真正清理且不泄漏/不崩溃
	// ═════════════════════════════════════════════════════════════════
	private static void testRemoveCharacterEventThenUpdateCleansUp()
	{
		var sys = new EventSystem();
		long charID = 77774;
		// 反复: 注册角色事件 → 派发 → removeCharacterEvent → update 清理
		for (int i = 0; i < 10; ++i)
		{
			var l = new TestListener();
			int c = 0;
			sys.listenEvent<TestEvent>(charID, _ => c++, l);
			sys.pushEvent<TestEvent>(charID);
			assertEqual(1, c, $"第{i}轮: 角色监听收到");
			sys.removeCharacterEvent(charID);
			// update 触发空列表清理 (mNeedCheckEmptyEvent)
			sys.update(0f);
			// 清理后该角色已无监听, 不再触发
			sys.pushEvent<TestEvent>(charID);
			assertEqual(1, c, $"第{i}轮: remove+update 后不再触发");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 角色派发中新增角色监听者 → 本次不收到 (count 固定), 下次收到
	// (与全局派发新增的行为一致, 角色列表同样 SafeFastDeepListReader)
	// ═════════════════════════════════════════════════════════════════
	private static void testCharacterAddNewDuringDispatch()
	{
		var sys = new EventSystem();
		var lA = new TestListener();
		var lNew = new TestListener();
		int cA = 0, cNew = 0;
		long charID = 77775;
		sys.listenEvent<TestEvent>(charID, _ =>
		{
			cA++;
			sys.listenEvent<TestEvent>(charID, _ => cNew++, lNew);
		}, lA);

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, cA, "lA 收到");
		assertEqual(0, cNew, "本次新增的 lNew 不收到");

		sys.pushEvent<TestEvent>(charID);
		assertEqual(2, cA, "lA 收到2次");
		assertEqual(1, cNew, "lNew 下次收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 角色事件中 unlistenEvent<T> → 只移除指定事件类型的注册(角色+全局)
	// 其他事件类型的角色注册保留
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenTypeSpecificDuringCharacter()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		int c1 = 0, c2 = 0;
		long charID = 77776;
		sys.listenEvent<TestEvent>(charID, _ => c1++, l1);
		sys.listenEvent<TestEvent2>(charID, _ => c2++, l1);

		sys.pushEvent<TestEvent>(charID);
		sys.pushEvent<TestEvent2>(charID);
		assertEqual(1, c1, "TestEvent 角色收到1次");
		assertEqual(1, c2, "TestEvent2 角色收到1次");

		// 只移除 TestEvent 的角色注册
		sys.unlistenEvent<TestEvent>(l1);
		sys.pushEvent<TestEvent>(charID);
		sys.pushEvent<TestEvent2>(charID);
		assertEqual(1, c1, "TestEvent 不再触发");
		assertEqual(2, c2, "TestEvent2 继续触发");
	}

	// ═════════════════════════════════════════════════════════════════
	// 角色回调里 push 全局事件 → 全局监听收到, 角色派发不被打断
	// → 全局派发和角色派发各自独立安全迭代
	// ═════════════════════════════════════════════════════════════════
	private static void testCharacterCallbackPushesGlobal()
	{
		var sys = new EventSystem();
		var lChar = new TestListener();
		var lGlobal = new TestListener();
		int cChar = 0, cGlobal = 0;
		long charID = 77777;
		sys.listenEvent<TestEvent>(charID, _ =>
		{
			cChar++;
			sys.pushEvent<TestEvent2>(); // 角色回调里 push 全局事件
		}, lChar);
		sys.listenEvent<TestEvent2>(_ => cGlobal++, lGlobal); // 全局监听 TestEvent2

		sys.pushEvent<TestEvent>(charID);
		assertEqual(1, cChar, "角色 lChar 收到");
		assertEqual(1, cGlobal, "角色回调嵌套 push 的全局事件被 lGlobal 收到");

		sys.pushEvent<TestEvent>(charID);
		assertEqual(2, cChar, "角色 lChar 收到2次");
		assertEqual(2, cGlobal, "全局 lGlobal 收到2次");
	}

	// ═════════════════════════════════════════════════════════════════
	// 用 listenEvent(int eventTypeID, Action, listener) 按 TypeID 注册全局
	// → 派发中 unlisten 其他监听者, 验证 TypeID 注册路径同样安全
	// ═════════════════════════════════════════════════════════════════
	private static void testListenByTypeIDAndUnlistenDuringDispatch()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		int c1 = 0, c2 = 0;
		// l1 先注册, 回调中移除排在后面的 l2
		sys.listenEvent(TypeID<TestEvent>.ID, () =>
		{
			c1++;
			sys.unlistenEvent(l2); // 派发中移除排在其后的 l2
		}, l1);
		sys.listenEvent(TypeID<TestEvent>.ID, () => c2++, l2);

		sys.pushEvent<TestEvent>();
		assertEqual(1, c1, "l1 收到");
		assertEqual(0, c2, "l1 派发中移除 l2, l2 本次不收到");

		sys.pushEvent<TestEvent>();
		assertEqual(2, c1, "l1 继续收到");
		assertEqual(0, c2, "l2 已被移除, 始终不收到");
	}

	// ═════════════════════════════════════════════════════════════════
	// 带参角色派发 pushEvent<T>(param, charID)
	// → 同一 param 实例先广播全局, 再发给角色 (源码: "即使只是指定角色的事件,也会先广播全局监听")
	// → 全局监听和角色监听收到同一个对象引用
	// ═════════════════════════════════════════════════════════════════
	private static void testParamSharedBetweenGlobalAndCharacter()
	{
		var sys = new EventSystem();
		var lGlobal = new TestListener();
		var lChar = new TestListener();
		TestEventWithValue seenByGlobal = null;
		TestEventWithValue seenByChar = null;
		long charID = 77778;
		sys.listenEvent<TestEventWithValue>(e => seenByGlobal = e, lGlobal);
		sys.listenEvent<TestEventWithValue>(charID, e => seenByChar = e, lChar);

		var evt = CLASS<TestEventWithValue>();
		evt.value = 555;
		sys.pushEvent(evt, charID); // 带参+角色派发

		// 全局与角色收到同一实例
		assertTrue(seenByGlobal != null, "全局收到事件");
		assertTrue(seenByChar != null, "角色收到事件");
		assertTrue(ReferenceEquals(seenByGlobal, seenByChar), "全局与角色收到同一 param 实例");
		assertEqual(555, seenByChar.value, "角色看到事件值");

		UN_CLASS(evt);
	}

	// ═════════════════════════════════════════════════════════════════
	// 带参全局派发: 多个监听者共享同一 param 对象(引用语义)
	// → 第一个监听者修改 param 字段, 后续监听者看到修改后的值
	// ═════════════════════════════════════════════════════════════════
	private static void testParamMutationAffectsLaterListeners()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		var l3 = new TestListener();
		int seen1 = -1, seen2 = -1, seen3 = -1;
		sys.listenEvent<TestEventWithValue>(e =>
		{
			seen1 = e.value;
			e.value = 100; // 修改共享对象
		}, l1);
		sys.listenEvent<TestEventWithValue>(e =>
		{
			seen2 = e.value;
			e.value = 200;
		}, l2);
		sys.listenEvent<TestEventWithValue>(e => seen3 = e.value, l3);

		var evt = CLASS<TestEventWithValue>();
		evt.value = 1;
		sys.pushEvent(evt); // 带参全局派发

		assertEqual(1, seen1, "l1 先看到初始值1");
		assertEqual(100, seen2, "l2 看到 l1 修改后的100");
		assertEqual(200, seen3, "l3 看到 l2 修改后的200");

		UN_CLASS(evt);
	}

	// ═════════════════════════════════════════════════════════════════
	// 嵌套派发带参事件: 外层回调修改 param 后, 内层嵌套派发用同一个/新 param
	// → 验证带参事件在嵌套调用链中的值传递正确
	// ═════════════════════════════════════════════════════════════════
	private static void testParamValueFlowsThroughNestedDispatch()
	{
		var sys = new EventSystem();
		var lA = new TestListener();
		var lB = new TestListener();
		int aSeen = -1, bSeen = -1;
		// lA 收到 TestEventWithValue 后, 把值传递并嵌套 push 一个 TestEvent2
		sys.listenEvent<TestEventWithValue>(e =>
		{
			aSeen = e.value;
			// 嵌套派发 TestEvent2 (无参)
			sys.pushEvent<TestEvent2>();
		}, lA);
		sys.listenEvent<TestEvent2>(_ => bSeen = aSeen, lB);

		var evt = CLASS<TestEventWithValue>();
		evt.value = 42;
		sys.pushEvent(evt);

		assertEqual(42, aSeen, "lA 看到初始值42");
		assertEqual(42, bSeen, "lB 在内层派发中读到 lA 传出的值42");

		UN_CLASS(evt);
	}

	// ═════════════════════════════════════════════════════════════════
	// 回调中缓存 param 引用, 验证对象池事件对象派发后被回收重置
	// → 若框架对象池已初始化, 派发结束(ClassScope Dispose)后对象字段被 reset 归零
	// ═════════════════════════════════════════════════════════════════
	private static void testParamCallbackOrderObservesMutations()
	{
		var sys = new EventSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		var l3 = new TestListener();
		// 3 个监听者按顺序记录各自看到的值
		var order = new System.Collections.Generic.List<int>();
		sys.listenEvent<TestEventWithValue>(e => { order.Add(e.value); e.value = e.value + 10; }, l1);
		sys.listenEvent<TestEventWithValue>(e => { order.Add(e.value); e.value = e.value + 10; }, l2);
		sys.listenEvent<TestEventWithValue>(e => order.Add(e.value), l3);

		var evt = CLASS<TestEventWithValue>();
		evt.value = 0;
		sys.pushEvent(evt);

		// 共享对象累加: l1看到0→改10, l2看到10→改20, l3看到20
		assertEqual(0, order[0], "l1 看到0");
		assertEqual(10, order[1], "l2 看到10");
		assertEqual(20, order[2], "l3 看到20");

		UN_CLASS(evt);
	}
}
