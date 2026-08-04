using static TestAssert;

// AStarNode 寻路节点结构体测试
public static class AStarNodeTest
{
    public static void Run()
    {
        testConstructor();
        testInit();
    }

    static void testConstructor()
    {
        AStarNode node = new AStarNode(10, 20, 30, 5, 3, NODE_STATE.OPEN);
        assertEqual(10, node.mG, "g=10");
        assertEqual(20, node.mH, "h=20");
        assertEqual(30, node.mF, "f=30");
        assertEqual(5, node.mIndex, "index=5");
        assertEqual(3, node.mParent, "parent=3");
        assertEqual(NODE_STATE.OPEN, node.mState, "state=OPEN");
    }

    static void testInit()
    {
        AStarNode node = new AStarNode();
        node.init(42);
        assertEqual(0, node.mG, "init g=0");
        assertEqual(0, node.mH, "init h=0");
        assertEqual(0, node.mF, "init f=0");
        assertEqual(42, node.mIndex, "init index=42");
        assertEqual(-1, node.mParent, "init parent=-1");
        assertEqual(NODE_STATE.NONE, node.mState, "init state=NONE");
    }
}
