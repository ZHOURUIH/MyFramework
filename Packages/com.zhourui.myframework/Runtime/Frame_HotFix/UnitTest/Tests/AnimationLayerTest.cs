using System;
using static TestAssert;

// AnimationLayer 单元测试 — 动画状态机层的纯字段逻辑
// 继承 ClassObject(构造为纯字段初始化, new 安全), 覆盖 getter/setter/resetProperty
// (update() 依赖 Character, 本测试不测)
public static class AnimationLayerTest
{
	public static void Run()
	{
		test_DefaultValues();
		test_SetGet_Character();
		test_SetGet_DefaultState();
		test_SetGet_Group();
		test_SetGet_LayerIndex();
		test_ResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认值
	// ═════════════════════════════════════════════════════════════════
	private static void test_DefaultValues()
	{
		AnimationLayer layer = new AnimationLayer();
		assertNull(layer.getCharacter(), "默认 character 为 null");
		assertNull(layer.getDefaultState(), "默认 defaultState 为 null");
		assertNull(layer.getGroup(), "默认 group 为 null");
		assertEqual(0, layer.getLayerIndex(), "默认 layerIndex 为 0");
	}

	// ═════════════════════════════════════════════════════════════════
	// setCharacter/getCharacter
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetGet_Character()
	{
		AnimationLayer layer = new AnimationLayer();
		// 用 null 验证 getter/setter 通路(Character 依赖 Unity 对象, 不实际构造)
		layer.setCharacter(null);
		assertNull(layer.getCharacter(), "setCharacter(null) 后仍为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// setDefaultState/getDefaultState
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetGet_DefaultState()
	{
		AnimationLayer layer = new AnimationLayer();
		Type stateType = typeof(TestStateMarker);
		layer.setDefaultState(stateType);
		assertTrue(layer.getDefaultState() == stateType, "setDefaultState 后 getDefaultState 一致");
		layer.setDefaultState(null);
		assertNull(layer.getDefaultState(), "setDefaultState(null) 后为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// setGroup/getGroup
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetGet_Group()
	{
		AnimationLayer layer = new AnimationLayer();
		Type groupType = typeof(TestStateMarker);
		layer.setGroup(groupType);
		assertTrue(layer.getGroup() == groupType, "setGroup 后 getGroup 一致");
		layer.setGroup(null);
		assertNull(layer.getGroup(), "setGroup(null) 后为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// setLayerIndex/getLayerIndex
	// ═════════════════════════════════════════════════════════════════
	private static void test_SetGet_LayerIndex()
	{
		AnimationLayer layer = new AnimationLayer();
		layer.setLayerIndex(3);
		assertEqual(3, layer.getLayerIndex(), "setLayerIndex(3) 后一致");
		layer.setLayerIndex(-1);
		assertEqual(-1, layer.getLayerIndex(), "setLayerIndex(-1) 后一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty — 全部字段复位
	// ═════════════════════════════════════════════════════════════════
	private static void test_ResetProperty()
	{
		AnimationLayer layer = new AnimationLayer();
		layer.setDefaultState(typeof(TestStateMarker));
		layer.setGroup(typeof(TestStateMarker));
		layer.setLayerIndex(5);
		layer.resetProperty();
		assertNull(layer.getDefaultState(), "reset 后 defaultState 为 null");
		assertNull(layer.getGroup(), "reset 后 group 为 null");
		assertEqual(0, layer.getLayerIndex(), "reset 后 layerIndex 为 0");
	}

	// 仅用作 Type 标记, 不实例化
	private class TestStateMarker { }
}
