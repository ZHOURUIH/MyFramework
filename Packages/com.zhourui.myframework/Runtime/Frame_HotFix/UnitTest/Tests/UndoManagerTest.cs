using System;
using static FrameUtility;
using static TestAssert;

public static class UndoManagerTest
{
    class TestUndo : MyUndo
    {
        public Action mAction;
        public override void resetProperty()
        {
            base.resetProperty();
            mAction = null;
        }
        public override void undo()
        {
            mAction?.Invoke();
        }
    }

    public static void Run()
    {
        testAddUndo();
        testUndoRedo();
        testCanUndoCanRedo();
        testClearAll();
        testCallback();
        testClearRedo();
        testRemoveCallback();
    

		testMaxUndoOverflowEvictsOldest();
		testAddUndoClearsRedo();
		testRedoAddUndoKeepsRedo();
		testUndoCallbackAddsToRedo();
		testLifoExactOrder();
		testMaxUndoCountSetter();
		testSetMaxUndoThenOverflow();
	}

    static TestUndo makeUndo(Action action)
    {
        TestUndo u = CLASS<TestUndo>();
        u.mAction = action;
        return u;
    }

    static void releaseUndo(ref TestUndo u)
    {
        UN_CLASS(ref u);
    }

    static void testAddUndo()
    {
        UndoManager undo = new UndoManager();
        TestUndo u = makeUndo(() => { });
        undo.addUndo(u);
        assertTrue(undo.canUndo(), "After addUndo, canUndo should be true");
        u = null;
        undo.clearAll();
    }

    static void testUndoRedo()
    {
        UndoManager undo = new UndoManager();
        int value = 0;
        // UndoManager 是 LIFO 顺序：后添加的先撤销
        TestUndo u1 = makeUndo(() => { value = 0; });  // 倒数第二个撤销 → value=0
        undo.addUndo(u1);
        TestUndo u2 = makeUndo(() => { value = 1; });  // 最先撤销 → value=1
        undo.addUndo(u2);

        undo.undo();
        assertEqual(1, value, "After first undo, value should be 1");
        undo.undo();
        assertEqual(0, value, "After second undo, value should be 0");

        assertFalse(undo.canUndo(), "After undoing all, canUndo should be false");
        undo.clearAll();
    }

    static void testCanUndoCanRedo()
    {
        UndoManager undo = new UndoManager();
        assertFalse(undo.canUndo(), "Empty should not allow undo");
        assertFalse(undo.canRedo(), "Empty should not allow redo");

        // undo 回调：addUndo 进入 redo 列表（mUndoing=true）
        // redo 回调：addUndo 进入 undo 列表（mUndoing=false, mRedoing=true）
        bool redoDone = false;
        TestUndo u = makeUndo(() => undo.addUndo(makeUndo(() =>
        {
            redoDone = true;
            undo.addUndo(makeUndo(() => { })); // 重做时回到 undo 列表
        })));
        undo.addUndo(u);
        assertTrue(undo.canUndo(), "After add, canUndo should be true");
        assertFalse(undo.canRedo(), "After add, canRedo should be false");

        undo.undo();
        assertFalse(undo.canUndo(), "After undo all, canUndo false");
        assertTrue(undo.canRedo(), "After undo, canRedo true");

        undo.redo();
        assertTrue(redoDone, "Redo action should have executed");
        assertTrue(undo.canUndo(), "After redo, canUndo true");
        assertFalse(undo.canRedo(), "After redo, canRedo false");
        undo.clearAll();
    }

    static void testClearAll()
    {
        UndoManager undo = new UndoManager();
        undo.addUndo(makeUndo(() => { }));
        undo.addUndo(makeUndo(() => { }));
        undo.clearAll();
        assertFalse(undo.canUndo(), "After clearAll, canUndo should be false");
        assertFalse(undo.canRedo(), "After clearAll, canRedo should be false");
    }

    static void testCallback()
    {
        UndoManager undo = new UndoManager();
        int callbackCount = 0;
        Action callback = () => { callbackCount++; };

        undo.addUndoRedoChangeCallback(callback);
        // undo 回调中 addUndo 会进入 redo 列表（mUndoing=true）
        undo.addUndo(makeUndo(() => undo.addUndo(makeUndo(() => { }))));
        assertEqual(1, callbackCount, "Callback should fire on addUndo");

        undo.undo();
        // callback fires: 1) addUndo 内部 canRedo 变化 2) undo() 结尾 canUndo 变化
        assertTrue(callbackCount >= 3, "Callback should fire during undo");

        undo.redo();
        // callback fires: 1) redo() 结尾 canRedo 变化
        assertTrue(callbackCount >= 4, "Callback should fire on redo");

        undo.clearAll();
    }

    static void testClearRedo()
    {
        UndoManager undo = new UndoManager();
        try
        {
            assertFalse(undo.canRedo(), "初始无 redo");
            // 构造 redo 列表: addUndo 的 TestUndo 在 undo 期间(mUndoing=true)再次 addUndo, 该内层 addUndo 进 redo 列表
            undo.addUndo(makeUndo(() => undo.addUndo(makeUndo(() => { }))));
            undo.undo();
            // undo 弹出一个 mUndoList 项, mUndoing 期间内层 addUndo 进 redoList
            assertTrue(undo.canRedo(), "undo 期间 addUndo 落入 redo 列表, canRedo true");
            undo.clearRedo();
            assertFalse(undo.canRedo(), "clearRedo 后 canRedo false");
            // 再 clearRedo 空列表也不报错(真实行为: UN_CLASS_LIST 空列表无害)
            undo.clearRedo();
        }
        finally
        {
            undo.clearAll();
        }
    }

    static void testRemoveCallback()
    {
        UndoManager undo = new UndoManager();
        int count = 0;
        Action callback = () => { count++; };
        undo.addUndoRedoChangeCallback(callback);
        // 触发一次变更 → 回调
        undo.addUndo(makeUndo(() => { }));
        int afterFire = count;
        assertTrue(afterFire > 0, "add 回调后变更触发回调");
        // 移除回调后再变更 → 不再触发
        undo.removeUndoRedoChangeCallback(callback);
        int beforeNoFire = count;
        undo.addUndo(makeUndo(() => { }));
        assertEqual(beforeNoFire, count, "移除回调后变更不再触发");
        undo.clearAll();
    }


	class TestUndo_Deep : MyUndo
	{
		public Action mAction;
		public string mTag;
		public override void resetProperty()
		{
			base.resetProperty();
			mAction = null;
			mTag = null;
		}
		public override void undo()
		{
			mAction?.Invoke();
		}
	}

	

	static TestUndo_Deep makeUndo_Deep(Action action, string tag)
	{
		TestUndo_Deep u = CLASS<TestUndo_Deep>();
		u.mAction = action;
		u.mTag = tag;
		return u;
	}

	// ═════════════════════════════════════════════════════════════════
	// maxUndo 溢出淘汰 — 添加超过上限时移除最早的操作
	// ═════════════════════════════════════════════════════════════════
	private static void testMaxUndoOverflowEvictsOldest()
	{
		UndoManager undo = new UndoManager();
		undo.setMaxUndoCount(2);
		// 添加3个操作, 最早的 op1 应被淘汰
		undo.addUndo(makeUndo_Deep(() => { }, "op1"));
		undo.addUndo(makeUndo_Deep(() => { }, "op2"));
		undo.addUndo(makeUndo_Deep(() => { }, "op3"));

		// 只保留最近2个: op3, op2
		undo.undo(); // 执行 op3
		undo.undo(); // 执行 op2
		assertFalse(undo.canUndo(), "溢出淘汰后只剩2个操作, 撤销2次后无操作");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// 普通 addUndo 清空 redo 列表 — 添加新操作后 redo 失效
	// ═════════════════════════════════════════════════════════════════
	private static void testAddUndoClearsRedo()
	{
		UndoManager undo = new UndoManager();
		// 第一步: 构造出一个可 redo 的状态
		// op1 的 undo 回调会 addUndo 进 redo 列表
		undo.addUndo(makeUndo_Deep(() => undo.addUndo(makeUndo_Deep(() => { }, "redoOp")), "op1"));
		undo.undo(); // 触发 op1.undo → addUndo(redoOp) 进 redoList
		assertTrue(undo.canRedo(), "undo 回调 addUndo 后应可 redo");

		// 第二步: 普通 addUndo(mUndoing=false, mRedoing=false) → 清空 redo
		undo.addUndo(makeUndo_Deep(() => { }, "op2"));
		assertFalse(undo.canRedo(), "普通 addUndo 应清空 redo 列表");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// redo 过程中 addUndo 不清空 redo — mRedoing=true 时保留 redo
	// ═════════════════════════════════════════════════════════════════
	private static void testRedoAddUndoKeepsRedo()
	{
		UndoManager undo = new UndoManager();
		// 构造 redo 列表: 两个操作链
		// op1.undo → addUndo(redo1) ; redo1.undo → addUndo(undo2)
		undo.addUndo(makeUndo_Deep(() => undo.addUndo(makeUndo_Deep(() => undo.addUndo(makeUndo_Deep(() => { }, "u2")), "r1")), "op1"));
		undo.undo(); // op1.undo → addUndo(r1) 进 redoList
		assertTrue(undo.canRedo(), "第一次 undo 后可 redo");

		// 触发 redo: r1.undo → addUndo(u2), 此时 mRedoing=true → 不清空 redoList(除了 popBack 当前)
		undo.redo();
		// r1 被 popBack 弹出, 所以 redo 只剩... 实际上 r1 是唯一 redo 项, 弹出后 shouldCanRedo=false
		assertFalse(undo.canRedo(), "redo 弹出当前项后不可再 redo");
		assertTrue(undo.canUndo(), "redo 回调 addUndo(u2) 进 undo 列表后可 undo");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// undo 回调中 addUndo 进入 redo 列表 — mUndoing=true 分支
	// ═════════════════════════════════════════════════════════════════
	private static void testUndoCallbackAddsToRedo()
	{
		UndoManager undo = new UndoManager();
		bool redoExecuted = false;
		// op1.undo → addUndo(redoX), redoX.undo → redoExecuted=true
		undo.addUndo(makeUndo_Deep(() => undo.addUndo(makeUndo_Deep(() => { redoExecuted = true; }, "redoX")), "op1"));

		undo.undo(); // 触发 op1.undo → addUndo(redoX) 进 redoList
		assertTrue(undo.canRedo(), "undo 回调 addUndo 进入 redo 列表");
		assertFalse(undo.canUndo(), "undo 后 undo 列表为空");

		undo.redo(); // 执行 redoX.undo
		assertTrue(redoExecuted, "redo 操作应被执行");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// LIFO 精确执行顺序 + 带参数校验
	// ═════════════════════════════════════════════════════════════════
	private static void testLifoExactOrder()
	{
		UndoManager undo = new UndoManager();
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		undo.addUndo(makeUndo_Deep(() => sb.Append("1"), "1"));
		undo.addUndo(makeUndo_Deep(() => sb.Append("2"), "2"));
		undo.addUndo(makeUndo_Deep(() => sb.Append("3"), "3"));

		undo.undo(); // 3
		undo.undo(); // 2
		undo.undo(); // 1
		assertEqual("321", sb.ToString(), "LIFO 后进先出撤销顺序");
		assertFalse(undo.canUndo(), "全部撤销后不可再撤销");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// setMaxUndoCount / getMaxUndoCount 存取
	// ═════════════════════════════════════════════════════════════════
	private static void testMaxUndoCountSetter()
	{
		UndoManager undo = new UndoManager();
		assertEqual(10, undo.getMaxUndoCount(), "默认 maxUndo=10");
		undo.setMaxUndoCount(5);
		assertEqual(5, undo.getMaxUndoCount(), "设置后 maxUndo=5");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// setMaxUndoCount 后溢出 — 大上限淘汰行为
	// ═════════════════════════════════════════════════════════════════
	private static void testSetMaxUndoThenOverflow()
	{
		UndoManager undo = new UndoManager();
		undo.setMaxUndoCount(3);
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		undo.addUndo(makeUndo_Deep(() => sb.Append("a"), "a"));
		undo.addUndo(makeUndo_Deep(() => sb.Append("b"), "b"));
		undo.addUndo(makeUndo_Deep(() => sb.Append("c"), "c"));
		undo.addUndo(makeUndo_Deep(() => sb.Append("d"), "d")); // 溢出: 淘汰 "a"

		// 撤销顺序: d, c, b (a 被淘汰)
		undo.undo();
		undo.undo();
		undo.undo();
		assertEqual("dcb", sb.ToString(), "溢出淘汰最早操作后 LIFO 顺序");
		assertFalse(undo.canUndo(), "淘汰后只剩3个, 撤销3次后无操作");
		undo.clearAll();
	}
}