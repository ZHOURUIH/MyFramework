#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

public static class MyStringBuilderExtendedTest
{
    public static void Run()
    {
        testCoreBuildAndSearch();
        testMutationHelpers();
        testJsonAndRichText();
    }

    private static void testCoreBuildAndSearch()
    {
        MyStringBuilder sb = new MyStringBuilder();
        sb.add("A").add(1).add("B").add(true).add("C");
        assertTrue(sb.endWith('C'), "endWith");
        assertTrue(sb.indexOf('B') >= 0, "indexOf");
        assertTrue(sb.lastIndexOf('A') == 0, "lastIndexOf");
        assertTrue(sb.toString(0, sb.Length).Contains("A1BtrueC"), "build result");
    }

    private static void testMutationHelpers()
    {
        MyStringBuilder sb = new MyStringBuilder();
        sb.add("hello");
        sb.insert(5, " world");
        sb.replace('l', 'L');
        sb.remove(0, 1);
        sb.addRepeat("!", 2);
        sb.removeLast('!');
        sb.removeLastComma();
        string text = sb.toString(0, sb.Length);
        assertTrue(text.Contains("eLLo"), "replace");
        assertTrue(text.EndsWith("!"), "remove last");
    }

    private static void testJsonAndRichText()
    {
        MyStringBuilder sb = new MyStringBuilder();
        sb.jsonStartStruct();
        sb.jsonAddPair("name", "Alice");
        sb.jsonAddPair("level", "3");
        sb.jsonEndStruct(false);
        sb.colorString("FF0000", "hot");
        sb.addSprite("icon", 1.5f);
        sb.line("line text");
        string result = sb.toString(0, sb.Length);
        assertTrue(result.Contains("Alice"), "json pair");
        assertTrue(result.Contains("<color=#FF0000>"), "color string");
        assertTrue(result.Contains("icon"), "sprite tag");
        assertTrue(result.Contains("line text"), "line helper");
    }
}
#endif
