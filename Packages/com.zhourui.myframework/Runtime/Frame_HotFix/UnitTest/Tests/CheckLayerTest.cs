using UnityEngine;
using static TestAssert;

// CheckLayer 结构体测试：方向映射逻辑
public static class CheckLayerTest
{
	public static void Run()
	{
		testDownDirection();
		testUpDirection();
		testLeftDirection();
		testRightDirection();
		testForwardDirection();
		testBackDirection();
		testDefaultDirection();
		testFieldsStorage();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testDownDirection()
	{
		var cl = new CheckLayer(5, CHECK_DIRECTION.DOWN, 10.0f, 1.0f);
		assertEqual(CHECK_DIRECTION.DOWN, cl.mDirection, "方向=DOWN");
		assertEqual(Vector3.down, cl.mDirectionVector, "DOWN → Vector3.down");
		assertEqual(10.0f, cl.mCheckDistance, "checkDistance=10");
		assertEqual(1.0f, cl.mMinDistance, "minDistance=1");
		assertEqual(5, cl.mLayerIndex, "layerIndex=5");
	}

	private static void testUpDirection()
	{
		var cl = new CheckLayer(0, CHECK_DIRECTION.UP, 5.0f, 0.5f);
		assertEqual(Vector3.up, cl.mDirectionVector, "UP → Vector3.up");
	}

	private static void testLeftDirection()
	{
		var cl = new CheckLayer(1, CHECK_DIRECTION.LEFT, 8.0f, 2.0f);
		assertEqual(Vector3.left, cl.mDirectionVector, "LEFT → Vector3.left");
	}

	private static void testRightDirection()
	{
		var cl = new CheckLayer(2, CHECK_DIRECTION.RIGHT, 3.0f, 1.5f);
		assertEqual(Vector3.right, cl.mDirectionVector, "RIGHT → Vector3.right");
	}

	private static void testForwardDirection()
	{
		var cl = new CheckLayer(3, CHECK_DIRECTION.FORWARD, 12.0f, 3.0f);
		assertEqual(Vector3.forward, cl.mDirectionVector, "FORWARD → Vector3.forward");
	}

	private static void testBackDirection()
	{
		var cl = new CheckLayer(4, CHECK_DIRECTION.BACK, 7.0f, 2.5f);
		assertEqual(Vector3.back, cl.mDirectionVector, "BACK → Vector3.back");
	}

	private static void testDefaultDirection()
	{
		// 非法方向值（超出枚举范围）→ Vector3.zero
		// CHECK_DIRECTION 有 6 个有效值 (0-5)，使用 unchecked 强制转换
		var cl = new CheckLayer(0, unchecked((CHECK_DIRECTION)(int.MaxValue)), 1.0f, 0.1f);
		assertEqual(Vector3.zero, cl.mDirectionVector, "非法方向 → Vector3.zero");
	}

	private static void testFieldsStorage()
	{
		var cl = new CheckLayer(10, CHECK_DIRECTION.RIGHT, 15.5f, 3.3f);
		assertEqual(10, cl.mLayerIndex, "layerIndex 正确");
		assertEqual(15.5f, cl.mCheckDistance, "checkDistance 正确");
		assertEqual(3.3f, cl.mMinDistance, "minDistance 正确");
	}
}
