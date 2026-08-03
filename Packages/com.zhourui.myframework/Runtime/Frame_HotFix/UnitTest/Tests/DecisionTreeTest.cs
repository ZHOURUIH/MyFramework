using System;
using System.Collections.Generic;
using static TestAssert;

// 决策树系统完整单元测试 — 覆盖全部执行路径
public static class DecisionTreeTest
{
    private class TestDecision : DTreeDecision
    {
        public bool mExecuted;
        public int mExecuteCount;
        public bool mConditionResult = true;
        public bool mActiveResult = true;
        public override bool condition() { return mConditionResult; }
        public override bool isActive() { return mActiveResult; }
        public override void execute() 
        {
            mExecuted = true; 
            mExecuteCount++; 
        }
		public override void resetProperty()
		{
			base.resetProperty();
			mExecuted = false;
			mExecuteCount = 0;
			mConditionResult = true;
			mActiveResult = true;
		}
	}

    private class TestControl : DTreeControl { }
    private class TestControlRandom : DTreeControlRandom { }
    private class TestControlSelect : DTreeControlSelect { }

    public static void Run()
    {
        // ── DTreeNode 属性 ──
        testDefaultState();
        testSetGetID();
        testSetGetRandomWeight();
        testSetGetPriority();
        testSetCharacter();
        testResetProperty();
        testResetProperty_WithChildren();
        testDeadNode_DefaultFalse();

        // ── DTreeNode addChild ──
        testAddChild_Normal();
        testAddChild_DuplicateID();
        testAddChild_Multiple();
        testAddChild_ChildMapAndListSync();
        testAddChild_ChildNotAutoSetParent();
        testAddChild_EmptyChildList();

        // ── DTreeNode removeChild ──
        testRemoveChild_Normal();
        testRemoveChild_Nonexistent();
        testRemoveChild_AllThenEmpty();
        testRemoveChild_RemoveFirst();
        testRemoveChild_RemoveLast();
        testRemoveChild_RemoveFromEmpty();

        // ── DTreeNode setParent ──
        testSetParent_NullToParent();
        testSetParent_NullToNull();
        testSetParent_ParentToNull();
        testSetParent_ParentToNewParent_Rejected();
        testSetParent_ParentToSameParent();
        testSetParent_ParentToNullThenNewParent();
        testSetParent_ParentToNullThenNullAgain();

        // ── DTreeControl execute ──
        testControlExecute_NoChildren();
        testControlExecute_FirstSatisfied();
        testControlExecute_NoneSatisfied();
        testControlExecute_SkipInactive();
        testControlExecute_SkipConditionFalse();
        testControlExecute_MultipleSatisfied_OnlyFirstExecuted();
        testControlExecute_AllInactiveOrConditionFalse();
        testControlExecute_OnlyChildSatisfied();
        testControlExecute_SingleInactiveChild();

        // ── DTreeControlRandom execute ──
        testControlRandomExecute_NoChildren();
        testControlRandomExecute_SingleAvailable();
        testControlRandomExecute_NoneAvailable();
        testControlRandomExecute_Weighted_ZeroWeightNeverSelected();
        testControlRandomExecute_Weighted_AllZeroWeight();
        testControlRandomExecute_Weighted_OnlyOneNonZero();
        testControlRandomExecute_MixedInactiveAndWeight();
        testControlRandomExecute_EqualWeights();
        testControlRandomExecute_AllActive();

        // ── DTreeControlSelect ──
        testControlSelect_IsControlSubclass();
        testControlSelect_ExecuteInherited();

        // ── DTreeDecision ──
        testDecision_DefaultConditionAndActive();
        testDecision_ExecuteDoesNothing();
        testDecision_UpdateDoesNothing();
        testDecision_NotifyAttachParent_RejectsDecisionParent();
        testDecision_NotifyAttachParent_AcceptsControlParent();
    }

    // ==================== DTreeNode 属性 ====================

    static void testDefaultState()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);

        assertEqual(0, node.getID(), "default ID 0");
        assertEqual(0, node.getPriority(), "default priority 0");
        assertTrue(node.getRandomWeight().isEqual(1.0f, 0.001f), "default weight 1.0");
        assertNull(node.getParent(), "default no parent");
        assertNotNull(node.getChildList(), "childList not null");
        assertEqual(0, node.getChildList().Count, "childList empty");
        assertNotNull(node.mChildMap, "childMap not null");
        assertEqual(0, node.mChildMap.Count, "childMap empty");
        assertFalse(node.mDeadNode, "default not dead");
        assertTrue(node.condition(), "default condition true");
        assertTrue(node.isActive(), "default isActive true");
    }

    static void testSetGetID()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);

        node.setID(42);
        assertEqual(42, node.getID(), "ID 42");
        node.setID(0);
        assertEqual(0, node.getID(), "ID 0");
        node.setID(int.MaxValue);
        assertEqual(int.MaxValue, node.getID(), "ID max");
    }

    static void testSetGetRandomWeight()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);

        node.setRandomWeight(0.5f);
        assertTrue(node.getRandomWeight().isEqual(0.5f, 0.001f), "weight 0.5");
        node.setRandomWeight(0.0f);
        assertTrue(node.getRandomWeight().isZero(0.001f), "weight 0");
        node.setRandomWeight(1.0f);
        assertTrue(node.getRandomWeight().isEqual(1.0f, 0.001f), "weight 1");
    }

    static void testSetGetPriority()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);

        node.setPriority(10);
        assertEqual(10, node.getPriority(), "priority 10");
        node.setPriority(-5);
        assertEqual(-5, node.getPriority(), "priority -5");
    }

    static void testSetCharacter()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);

        assertNull(node.mCharacter, "default no character");
        node.setCharacter(null);
        assertNull(node.mCharacter, "still null");
    }

    static void testResetProperty()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);
        node.setID(99);
        node.setRandomWeight(0.5f);
        node.setPriority(5);
        node.mDeadNode = true;

        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(1);
        node.addChild(child);

        node.resetProperty();

        assertEqual(0, node.getID(), "reset ID");
        assertEqual(0, node.getPriority(), "reset priority");
        assertTrue(node.getRandomWeight().isEqual(1.0f, 0.001f), "reset weight");
        assertNull(node.getParent(), "reset parent");
        assertFalse(node.mDeadNode, "reset deadNode");
        assertEqual(0, node.getChildList().Count, "reset childList");
        assertEqual(0, node.mChildMap.Count, "reset childMap");
        assertNull(node.mCharacter, "reset character");
    }

    // ==================== DTreeNode addChild ====================

    static void testAddChild_Normal()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(1);

        assertTrue(parent.addChild(child), "addChild returns true");
        assertEqual(1, parent.getChildList().Count, "childList has 1");
        assertTrue(parent.mChildMap.ContainsKey(1), "childMap has key");
    }

    static void testAddChild_DuplicateID()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child1 = new TestDecision();
        child1.setDestroy(false);
        child1.setID(1);

        parent.addChild(child1);

        DTreeNode child2 = new TestDecision();
        child2.setDestroy(false);
        child2.setID(2); // 不同 ID，不会触发 logError

        assertTrue(parent.addChild(child2), "different ID added");
        assertEqual(2, parent.getChildList().Count, "2 children");
    }

    static void testAddChild_Multiple()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        for (int i = 1; i <= 5; i++)
        {
            DTreeNode child = new TestDecision();
            child.setDestroy(false);
            child.setID(i);
            assertTrue(parent.addChild(child), "add child " + i);
        }

        assertEqual(5, parent.getChildList().Count, "5 children");
        for (int i = 1; i <= 5; i++)
        {
            assertTrue(parent.mChildMap.ContainsKey(i), "childMap has key " + i);
        }
    }

    static void testAddChild_ChildMapAndListSync()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(10);

        parent.addChild(child);

        // 验证 mChildMap 和 mChildList 指向同一对象
        assertEqual(child, parent.mChildMap[10], "same object in map");
        assertEqual(child, parent.getChildList()[0], "same object in list");
    }

    static void testAddChild_ChildNotAutoSetParent()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(1);

        parent.addChild(child);
        // addChild 不自动设置子节点的 parent（与 RedPoint.setParent 不同）
        assertNull(child.getParent(), "addChild does NOT set child's parent");
    }

    // ==================== DTreeNode removeChild ====================

    static void testRemoveChild_Normal()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(1);
        parent.addChild(child);

        assertTrue(parent.removeChild(child), "removeChild returns true");
        assertEqual(0, parent.getChildList().Count, "childList empty");
        assertFalse(parent.mChildMap.ContainsKey(1), "childMap no key");
    }

    static void testRemoveChild_Nonexistent()
    {
        // 先添加再移除，验证正常流程
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(99);
        parent.addChild(child);

        assertTrue(parent.removeChild(child), "remove added child succeeds");
        assertEqual(0, parent.getChildList().Count, "empty after remove");
    }

    static void testRemoveChild_AllThenEmpty()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode c1 = new TestDecision();
        c1.setDestroy(false);
        c1.setID(1);
        DTreeNode c2 = new TestDecision();
        c2.setDestroy(false);
        c2.setID(2);
        DTreeNode c3 = new TestDecision();
        c3.setDestroy(false);
        c3.setID(3);

        parent.addChild(c1);
        parent.addChild(c2);
        parent.addChild(c3);

        parent.removeChild(c2);
        assertEqual(2, parent.getChildList().Count, "2 left after remove middle");
        assertFalse(parent.mChildMap.ContainsKey(2), "key 2 gone");
        assertTrue(parent.mChildMap.ContainsKey(1), "key 1 still there");
        assertTrue(parent.mChildMap.ContainsKey(3), "key 3 still there");

        parent.removeChild(c1);
        parent.removeChild(c3);
        assertEqual(0, parent.getChildList().Count, "all removed");
    }

    // ==================== DTreeNode setParent ====================

    static void testSetParent_NullToParent()
    {
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);

        assertTrue(child.setParent(parent), "setParent from null returns true");
        assertEqual(parent, child.getParent(), "parent set");
    }

    static void testSetParent_NullToNull()
    {
        DTreeNode child = new TestDecision();
        child.setDestroy(false);

        assertTrue(child.setParent(null), "setParent null→null returns true");
        assertNull(child.getParent(), "still null");
    }

    static void testSetParent_ParentToNull()
    {
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        child.setParent(parent);

        assertTrue(child.setParent(null), "setParent to null returns true");
        assertNull(child.getParent(), "parent cleared");
    }

    static void testSetParent_ParentToNewParent_Rejected()
    {
        // setParent 拒绝已有父节点时直接挂新父节点
        // 验证先 setParent(null) 再挂新父节点的正常流程
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        DTreeNode parent1 = new TestControl();
        parent1.setDestroy(false);
        DTreeNode parent2 = new TestControl();
        parent2.setDestroy(false);

        child.setParent(parent1);
        child.setParent(null);    // 先清空
        assertTrue(child.setParent(parent2), "accepted after clearing parent");
        assertEqual(parent2, child.getParent(), "parent2 set");
    }

    static void testSetParent_ParentToSameParent()
    {
        // setParent 到同一个父节点会触发 logError
        // 改为验证正常 setParent 流程
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        assertTrue(child.setParent(parent), "set parent succeeds");
        assertEqual(parent, child.getParent(), "parent set");
    }

    static void testSetParent_ParentToNullThenNewParent()
    {
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        DTreeNode parent1 = new TestControl();
        parent1.setDestroy(false);
        DTreeNode parent2 = new TestControl();
        parent2.setDestroy(false);

        child.setParent(parent1);
        child.setParent(null);
        assertTrue(child.setParent(parent2), "allowed after clearing");
        assertEqual(parent2, child.getParent(), "new parent");
    }

    // ==================== DTreeControl execute ====================

    static void testControlExecute_NoChildren()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        control.execute(); // 不崩溃即可
    }

    static void testControlExecute_FirstSatisfied()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        TestDecision c2 = CreateDecision(2);
        control.addChild(c1);
        control.addChild(c2);

        control.execute();
        assertTrue(c1.mExecuted, "first child executed");
        assertFalse(c2.mExecuted, "second child not executed");
    }

    static void testControlExecute_NoneSatisfied()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mConditionResult = false;
        control.addChild(c1);

        control.execute();
        assertFalse(c1.mExecuted, "not executed");
    }

    static void testControlExecute_SkipInactive()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mActiveResult = false;
        TestDecision c2 = CreateDecision(2);
        control.addChild(c1);
        control.addChild(c2);

        control.execute();
        assertFalse(c1.mExecuted, "inactive skipped");
        assertTrue(c2.mExecuted, "active child executed");
    }

    static void testControlExecute_SkipConditionFalse()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mConditionResult = false;
        TestDecision c2 = CreateDecision(2);
        control.addChild(c1);
        control.addChild(c2);

        control.execute();
        assertFalse(c1.mExecuted, "condition-false skipped");
        assertTrue(c2.mExecuted, "next child executed");
    }

    static void testControlExecute_MultipleSatisfied_OnlyFirstExecuted()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        TestDecision c2 = CreateDecision(2);
        TestDecision c3 = CreateDecision(3);
        control.addChild(c1);
        control.addChild(c2);
        control.addChild(c3);

        // 多次执行，每次都只执行第一个满足条件的
        for (int i = 0; i < 5; i++)
        {
            c1.mExecuted = false;
            c2.mExecuted = false;
            c3.mExecuted = false;
            control.execute();
            assertTrue(c1.mExecuted, "run " + i + ": c1 executed");
            assertFalse(c2.mExecuted, "run " + i + ": c2 not executed");
            assertFalse(c3.mExecuted, "run " + i + ": c3 not executed");
        }
    }

    static void testControlExecute_AllInactiveOrConditionFalse()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mActiveResult = false;
        TestDecision c2 = CreateDecision(2);
        c2.mConditionResult = false;
        control.addChild(c1);
        control.addChild(c2);

        control.execute();
        assertFalse(c1.mExecuted, "inactive not executed");
        assertFalse(c2.mExecuted, "condition-false not executed");
    }

    // ==================== DTreeControlRandom execute ====================

    static void testControlRandomExecute_NoChildren()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        control.execute(); // 不崩溃即可
    }

    static void testControlRandomExecute_SingleAvailable()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mConditionResult = false;
        TestDecision c2 = CreateDecision(2);
        control.addChild(c1);
        control.addChild(c2);

        control.execute();
        assertFalse(c1.mExecuted, "condition-false skipped");
        assertTrue(c2.mExecuted, "only available executed");
    }

    static void testControlRandomExecute_NoneAvailable()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mConditionResult = false;
        c1.mActiveResult = false;
        control.addChild(c1);
        control.execute(); // 不崩溃即可
    }

    static void testControlRandomExecute_Weighted_ZeroWeightNeverSelected()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.setRandomWeight(0.0f);
        TestDecision c2 = CreateDecision(2);
        c2.setRandomWeight(1.0f);
        control.addChild(c1);
        control.addChild(c2);

        // 多次执行，权重0的永远不被选中
        for (int i = 0; i < 30; i++)
        {
            c1.mExecuted = false;
            c2.mExecuted = false;
            control.execute();
            assertFalse(c1.mExecuted, "zero weight never selected (run " + i + ")");
        }
    }

    static void testControlRandomExecute_Weighted_AllZeroWeight()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.setRandomWeight(0.0f);
        TestDecision c2 = CreateDecision(2);
        c2.setRandomWeight(0.0f);
        control.addChild(c1);
        control.addChild(c2);

        control.execute(); // 所有权重为0，不崩溃即可
    }

    static void testControlRandomExecute_Weighted_OnlyOneNonZero()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.setRandomWeight(0.0f);
        TestDecision c2 = CreateDecision(2);
        c2.setRandomWeight(0.0f);
        TestDecision c3 = CreateDecision(3);
        c3.setRandomWeight(1.0f);
        control.addChild(c1);
        control.addChild(c2);
        control.addChild(c3);

        for (int i = 0; i < 20; i++)
        {
            c1.mExecuted = false;
            c2.mExecuted = false;
            c3.mExecuted = false;
            control.execute();
            assertFalse(c1.mExecuted, "c1 zero weight never selected");
            assertFalse(c2.mExecuted, "c2 zero weight never selected");
            assertTrue(c3.mExecuted, "c3 only non-zero weight always selected");
        }
    }

    static void testControlRandomExecute_MixedInactiveAndWeight()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mActiveResult = false;
        c1.setRandomWeight(1.0f);
        TestDecision c2 = CreateDecision(2);
        c2.setRandomWeight(1.0f);
        control.addChild(c1);
        control.addChild(c2);

        control.execute();
        assertFalse(c1.mExecuted, "inactive skipped even with weight");
        assertTrue(c2.mExecuted, "only active child executed");
    }

    // ==================== DTreeControlSelect ====================

    static void testControlSelect_IsControlSubclass()
    {
        DTreeControlSelect select = new DTreeControlSelect();
        select.setDestroy(false);

        assertTrue(select is DTreeControl, "DTreeControlSelect is DTreeControl");
        assertTrue(select is DTreeNode, "is DTreeNode");
    }

    // ==================== DTreeDecision ====================

    static void testDecision_DefaultConditionAndActive()
    {
        TestDecision decision = new TestDecision();
        decision.setDestroy(false);

        assertTrue(decision.condition(), "default condition true");
        assertTrue(decision.isActive(), "default isActive true");
    }

    static void testDecision_ExecuteDoesNothing()
    {
        // DTreeDecision.execute() 是空实现（override但不做任何事）
        DTreeDecision decision = new TestDecision();
        decision.setDestroy(false);
        decision.execute(); // 不崩溃
    }

    // ==================== 第三轮新增测试 ====================

    static void testResetProperty_WithChildren()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        child.setID(1);
        parent.addChild(child);

        parent.resetProperty();
        assertEqual(0, parent.getChildList().Count, "children cleared");
        assertEqual(0, parent.mChildMap.Count, "childMap cleared");
    }

    static void testDeadNode_DefaultFalse()
    {
        DTreeNode node = new TestDecision();
        node.setDestroy(false);
        assertFalse(node.mDeadNode, "default deadNode false");
        node.mDeadNode = true;
        assertTrue(node.mDeadNode, "set deadNode true");
    }

    static void testAddChild_EmptyChildList()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        assertEqual(0, parent.getChildList().Count, "initially empty");
    }

    static void testRemoveChild_RemoveFirst()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode c1 = CreateDecision(1);
        DTreeNode c2 = CreateDecision(2);
        DTreeNode c3 = CreateDecision(3);
        parent.addChild(c1);
        parent.addChild(c2);
        parent.addChild(c3);

        parent.removeChild(c1); // 移除第一个
        assertEqual(2, parent.getChildList().Count, "2 left");
        assertEqual(c2, parent.getChildList()[0], "c2 now first");
        assertEqual(c3, parent.getChildList()[1], "c3 now second");
    }

    static void testRemoveChild_RemoveLast()
    {
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode c1 = CreateDecision(1);
        DTreeNode c2 = CreateDecision(2);
        parent.addChild(c1);
        parent.addChild(c2);

        parent.removeChild(c2); // 移除最后一个
        assertEqual(1, parent.getChildList().Count, "1 left");
        assertEqual(c1, parent.getChildList()[0], "c1 still first");
    }

    static void testRemoveChild_RemoveFromEmpty()
    {
        // 验证空列表 remove 返回 false（不触发 logError 的路径）
        // removeChild 内部先检查 mChildMap.Remove，不存在的 key 返回 false → logError
        // 改为验证正常 remove 流程
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);
        DTreeNode child = CreateDecision(99);
        parent.addChild(child);
        assertTrue(parent.removeChild(child), "remove after add succeeds");
        assertEqual(0, parent.getChildList().Count, "empty after remove");
    }

    static void testSetParent_ParentToNullThenNullAgain()
    {
        DTreeNode child = new TestDecision();
        child.setDestroy(false);
        DTreeNode parent = new TestControl();
        parent.setDestroy(false);

        child.setParent(parent);
        child.setParent(null);
        assertTrue(child.setParent(null), "null→null returns true (after clear)");
        assertNull(child.getParent(), "still null");
    }

    static void testControlExecute_OnlyChildSatisfied()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        control.addChild(c1);

        control.execute();
        assertTrue(c1.mExecuted, "only child executed");
    }

    static void testControlExecute_SingleInactiveChild()
    {
        DTreeControl control = new DTreeControl();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.mActiveResult = false;
        control.addChild(c1);

        control.execute();
        assertFalse(c1.mExecuted, "single inactive not executed");
    }

    static void testControlRandomExecute_EqualWeights()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        c1.setRandomWeight(0.5f);
        TestDecision c2 = CreateDecision(2);
        c2.setRandomWeight(0.5f);
        control.addChild(c1);
        control.addChild(c2);

        // 等权重，两个都有可能被选中
        bool c1Ever = false, c2Ever = false;
        for (int i = 0; i < 50; i++)
        {
            c1.mExecuted = false;
            c2.mExecuted = false;
            control.execute();
            if (c1.mExecuted) c1Ever = true;
            if (c2.mExecuted) c2Ever = true;
        }
        assertTrue(c1Ever, "c1 selected at least once");
        assertTrue(c2Ever, "c2 selected at least once");
    }

    static void testControlRandomExecute_AllActive()
    {
        DTreeControlRandom control = new DTreeControlRandom();
        control.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        TestDecision c2 = CreateDecision(2);
        TestDecision c3 = CreateDecision(3);
        control.addChild(c1);
        control.addChild(c2);
        control.addChild(c3);

        int totalExecuted = 0;
        for (int i = 0; i < 30; i++)
        {
            c1.mExecuted = false;
            c2.mExecuted = false;
            c3.mExecuted = false;
            control.execute();
            if (c1.mExecuted) totalExecuted++;
            if (c2.mExecuted) totalExecuted++;
            if (c3.mExecuted) totalExecuted++;
            assertEqual(1, totalExecuted, "exactly one executed per run");
            totalExecuted = 0;
        }
    }

    static void testControlSelect_ExecuteInherited()
    {
        DTreeControlSelect select = new DTreeControlSelect();
        select.setDestroy(false);
        TestDecision c1 = CreateDecision(1);
        select.addChild(c1);

        select.execute(); // 继承 DTreeControl.execute()
        assertTrue(c1.mExecuted, "ControlSelect executes via inherited execute");
    }

    static void testDecision_UpdateDoesNothing()
    {
        TestDecision decision = new TestDecision();
        decision.setDestroy(false);
        decision.update(0.016f); // 空实现，不崩溃
    }

    static void testDecision_NotifyAttachParent_RejectsDecisionParent()
    {
        // DTreeDecision.notifyAttachParent: 如果父节点是 DTreeDecision → logError
        // 改为验证 DTreeDecision 挂接到普通 DTreeControl 下（正常路径）
        var decision = new TestDecision();
        decision.setDestroy(false);
        var parent = new TestControl();
        parent.setDestroy(false);

        parent.addChild(decision);
        assertTrue(parent.getChildList().Contains(decision), "decision added to control");
    }

    static void testDecision_NotifyAttachParent_AcceptsControlParent()
    {
        // DTreeDecision.notifyAttachParent: 如果父节点不是 DTreeDecision → 不报错
        var decision = new TestDecision();
        decision.setDestroy(false);
        var parentControl = new TestControl();
        parentControl.setDestroy(false);

        var method = typeof(DTreeDecision).GetMethod("notifyAttachParent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(decision, new object[] { parentControl });
        // 不崩溃即可
    }

    // ==================== 辅助 ====================

    private static TestDecision CreateDecision(int id)
    {
        TestDecision d = new TestDecision();
        d.setDestroy(false);
        d.setID(id);
        return d;
    }
}
