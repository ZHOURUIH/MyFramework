using static TestAssert;

public class ComplexTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testOperatorPlus();
		testOperatorMinus();
		testGetHashCode();
		testZero();
	}

	private static void testConstructor()
	{
		Complex c = new Complex(3.0f, 4.0f);
		assertTrue(c.Equals(new Complex(3.0f, 4.0f)));
	}

	private static void testEquals()
	{
		Complex c1 = new Complex(1.0f, 2.0f);
		Complex c2 = new Complex(1.0f, 2.0f);
		Complex c3 = new Complex(3.0f, 4.0f);

		assertTrue(c1.Equals(c2));
		assertFalse(c1.Equals(c3));
	}

	private static void testOperatorPlus()
	{
		Complex c1 = new Complex(1.0f, 2.0f);
		Complex c2 = new Complex(3.0f, 4.0f);
		Complex result = c1 + c2;
		assertTrue(result.Equals(new Complex(4.0f, 6.0f)));
	}

	private static void testOperatorMinus()
	{
		Complex c1 = new Complex(5.0f, 7.0f);
		Complex c2 = new Complex(2.0f, 3.0f);
		Complex result = c1 - c2;
		assertTrue(result.Equals(new Complex(3.0f, 4.0f)));
	}

	private static void testGetHashCode()
	{
		Complex a = new Complex(1.0f, 2.0f);
		Complex b = new Complex(1.0f, 2.0f);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "相等对象 hash 一致");
	}

	private static void testZero()
	{
		Complex zero = new Complex(0.0f, 0.0f);
		Complex c = new Complex(5.0f, 6.0f);
		Complex result = c + zero;
		assertTrue(result.Equals(c), "加零不变");
		result = c - zero;
		assertTrue(result.Equals(c), "减零不变");
	}
}
