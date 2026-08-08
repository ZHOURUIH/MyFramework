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
}