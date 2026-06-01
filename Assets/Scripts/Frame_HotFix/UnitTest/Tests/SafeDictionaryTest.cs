#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;

public static class SafeDictionaryTest
{
    public static void Run()
    {
        testBasic();
        testContainsValue();
        testForKey();
        testForValue();
        testTryGetValue();
        testRemoveIf();
    }

    static void testBasic()
    {
        SafeDictionary<string, int> d = new();
        d.add("a", 1);
        assertFalse(d.isEmpty());
        assertTrue(d.containsKey("a"));
        d.clear();
        assertTrue(d.isEmpty());
    }

    static void testContainsValue()
    {
        SafeDictionary<int, string> d = new();
        d.add(1, "one");
        assertTrue(d.containsValue("one"));
        assertFalse(d.containsValue("two"));
    }

    static void testForKey()
    {
        SafeDictionary<string, int> d = new();
        d.add("a", 1);
        d.add("b", 2);
        int s = 0;
        d.forKey((k) =>
        {
            if (k == "a") s += 1;
            if (k == "b") s += 2;
        });
        assertEqual(3, s);
    }

    static void testForValue()
    {
        SafeDictionary<string, int> d = new();
        d.add("a", 5);
        d.add("b", 10);
        int t = 0;
        d.forValue((v) => t += v);
        assertEqual(15, t);
    }

    static void testTryGetValue()
    {
        SafeDictionary<string, int> d = new();
        d.add("k", 42);
        assertTrue(d.tryGetValue("k", out int v));
        assertEqual(42, v);
        assertFalse(d.tryGetValue("x", out _));
    }

    static void testRemoveIf()
    {
        SafeDictionary<string, int> d = new();
        d.add("a", 1);
        d.add("b", 2);
        d.removeIf("a", true);
        assertFalse(d.containsKey("a"));
        assertTrue(d.containsKey("b"));
    }
}
#endif
