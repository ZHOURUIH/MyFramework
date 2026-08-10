using static TestAssert;

// 自定义属性标签单元测试(Frame_Base 层, 纯逻辑, 无 Unity 运行时依赖)
// CustomLabelAttribute : PropertyAttribute — 检视面板变量名显示为自定义文字
// EnumLabelAttribute   : Attribute         — 枚举显示名
// 两者均为构造存 label + getLabel() 返回, 测构造/取值/空值
public static class AttributeLabelTest
{
	public static void Run()
	{
		testCustomLabel();
		testCustomLabelEmpty();
		testEnumLabel();
		testEnumLabelEmpty();
	}

	// ═════════════════════════════════════════════════════════════════
	// CustomLabelAttribute — 构造 + getLabel
	// ═════════════════════════════════════════════════════════════════
	private static void testCustomLabel()
	{
		CustomLabelAttribute attr = new CustomLabelAttribute("显示名称");
		assertEqual("显示名称", attr.getLabel(), "getLabel 应返回构造传入的 label");
	}

	// ═════════════════════════════════════════════════════════════════
	// CustomLabelAttribute — 空 label
	// ═════════════════════════════════════════════════════════════════
	private static void testCustomLabelEmpty()
	{
		CustomLabelAttribute attr = new CustomLabelAttribute("");
		assertEqual("", attr.getLabel(), "空 label 应原样返回");
		CustomLabelAttribute attrNull = new CustomLabelAttribute(null);
		assertNull(attrNull.getLabel(), "null label 应返回 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// EnumLabelAttribute — 构造 + getLabel
	// ═════════════════════════════════════════════════════════════════
	private static void testEnumLabel()
	{
		EnumLabelAttribute attr = new EnumLabelAttribute("攻击状态");
		assertEqual("攻击状态", attr.getLabel(), "getLabel 应返回构造传入的 label");
	}

	// ═════════════════════════════════════════════════════════════════
	// EnumLabelAttribute — 空 label
	// ═════════════════════════════════════════════════════════════════
	private static void testEnumLabelEmpty()
	{
		EnumLabelAttribute attr = new EnumLabelAttribute("");
		assertEqual("", attr.getLabel(), "空 label 应原样返回");
		EnumLabelAttribute attrNull = new EnumLabelAttribute(null);
		assertNull(attrNull.getLabel(), "null label 应返回 null");
	}
}
