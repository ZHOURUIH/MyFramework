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
}