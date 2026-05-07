#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// Character/DTree/StateMachine 纯数据测试
public static class CharacterDecisionStateTest
{
	public static void Run()
	{
		testDTreeNodeDefault();
		testDTreeNodeSetGet();
		testDTreeNodeAddRemoveChild();
		testDTreeNodeResetProperty();
		testDTreeControlDefault();
		testDTreeControlRandomDefault();
		testCharacterStateDefault();
		testStateGroupDefault();
		testStateParamDefault();
	}

	private static void testDTreeNodeDefault()
	{
		var node = new DTreeNode();
		AssertEqual(0, node.getID());
		AssertEqual(1.0f, node.getRandomWeight());
		AssertEqual(0, node.getPriority());
	}

	private static void testDTreeNodeSetGet()
	{
		var node = new DTreeNode();
		node.setID(42);
		node.setRandomWeight(0.5f);
		node.setPriority(3);
		AssertEqual(42, node.getID());
		AssertEqual(0.5f, node.getRandomWeight());
		AssertEqual(3, node.getPriority());
	}

	private static void testDTreeNodeAddRemoveChild()
	{
		var p = new DTreeNode();
		var c = new DTreeNode();
		c.setID(1);
		bool ok = p.addChild(c);
		Assert(ok);
		AssertEqual(1, p.getChildList().Count);
		ok = p.removeChild(c);
		Assert(ok);
		AssertEqual(0, p.getChildList().Count);
	}

	private static void testDTreeNodeResetProperty()
	{
		var node = new DTreeNode();
		node.setID(99);
		node.setRandomWeight(0.25f);
		node.setPriority(10);
		node.resetProperty();
		AssertEqual(0, node.getID());
		AssertEqual(1.0f, node.getRandomWeight());
		AssertEqual(0, node.getPriority());
	}

	private static void testDTreeControlDefault()
	{
		var c = new DTreeControl();
		AssertNotNull(c);
	}

	private static void testDTreeControlRandomDefault()
	{
		var c = new DTreeControlRandom();
		AssertNotNull(c);
	}

	private static void testCharacterStateDefault()
	{
		var s = new CharacterState();
		AssertNotNull(s);
	}

	private static void testStateGroupDefault()
	{
		var g = new StateGroup();
		AssertNotNull(g);
	}

	private static void testStateParamDefault()
	{
		var p = new StateParam();
		AssertNotNull(p);
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertNotNull(object o) { if (o == null) throw new Exception("Should not be null"); }
}
#endif
