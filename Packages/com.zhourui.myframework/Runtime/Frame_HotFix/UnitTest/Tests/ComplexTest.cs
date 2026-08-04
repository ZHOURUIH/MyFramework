using static TestAssert;

// Complex 复数结构体测试
public static class ComplexTest
{
    public static void Run()
    {
        testConstructor();
        testEquals();
        testAdd();
        testSubtract();
    }

    static void testConstructor()
    {
        Complex c = new Complex(3.0f, 4.0f);
        assertTrue(c.mReal.isEqual(3.0f), "real=3");
        assertTrue(c.mImg.isEqual(4.0f), "img=4");
    }

    static void testEquals()
    {
        Complex a = new Complex(1, 2);
        Complex b = new Complex(1, 2);
        Complex c = new Complex(3, 4);
        assertTrue(a.Equals(b), "equals same");
        assertFalse(a.Equals(c), "equals diff");
    }

    static void testAdd()
    {
        Complex a = new Complex(1, 2);
        Complex b = new Complex(3, 4);
        Complex result = a + b;
        assertTrue(result.mReal.isEqual(4.0f), "add real 1+3=4");
        assertTrue(result.mImg.isEqual(6.0f), "add img 2+4=6");
    }

    static void testSubtract()
    {
        Complex a = new Complex(5, 7);
        Complex b = new Complex(2, 3);
        Complex result = a - b;
        assertTrue(result.mReal.isEqual(3.0f), "sub real 5-2=3");
        assertTrue(result.mImg.isEqual(4.0f), "sub img 7-3=4");
    }
}
