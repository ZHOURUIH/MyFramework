using static TestAssert;

public class ComplexTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testOperatorPlus();
		testOperatorMinus();
	}

	// 测试构造函数
	private static void testConstructor()
	{
		Complex c = new Complex(3.0f, 4.0f);
		// 通过 Equals 间接验证字段值
		assertTrue(c.Equals(new Complex(3.0f, 4.0f)));
	}

	// 测试 Equals 方法
	private static void testEquals()
	{
		Complex c1 = new Complex(1.0f, 2.0f);
		Complex c2 = new Complex(1.0f, 2.0f);
		Complex c3 = new Complex(3.0f, 4.0f);

		assertTrue(c1.Equals(c2));
		assertFalse(c1.Equals(c3));
	}

	// 测试 + 运算符
	private static void testOperatorPlus()
	{
		Complex c1 = new Complex(1.0f, 2.0f);
		Complex c2 = new Complex(3.0f, 4.0f);
		Complex result = c1 + c2;

		assertTrue(result.Equals(new Complex(4.0f, 6.0f)));
	}

	// 测试 - 运算符
	private static void testOperatorMinus()
	{
		Complex c1 = new Complex(5.0f, 7.0f);
		Complex c2 = new Complex(2.0f, 3.0f);
		Complex result = c1 - c2;

		assertTrue(result.Equals(new Complex(3.0f, 4.0f)));
	}
}