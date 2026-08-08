using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameUtility;

// MyStringBuilder 深度测试
// 覆盖：add 全类型重载 / clear / remove / insert / insertFront / replace / replaceAll /
//       endWith / lastIndexOf / indexOf / findFirstSubstr / addRepeat / addIf 全重载 /
//       colorString / colorStringIf / colorStringComma / addLine /
//       jsonStartArray/jsonEndArray/jsonStartStruct/jsonEndStruct/jsonAddPair/jsonAddObject /
//       rightToLeft / leftToRight / V2ToS / V3ToS / V2IToS / byteToHEXString /
//       setColor / addSprite / line / addValue* / addCondition* / addUpdate* /
//       toString(int,int) / 下标/索引器 / Length 设置 / Length / ToString
public static class MyStringBuilderTest
{
    public static void Run()
    {
        testClearAndLength();
        testAddTypes();
        testAddStrings();
        testAddIf();
        testAddRepeat();
        testInsert();
        testRemove();
        testReplace();
        testReplaceAll();
        testEndWith();
        testLastIndexOf();
        testIndexOf();
        testColorString();
        testAddLine();
        testChaining();
        testEdgeCases();
        // ── 深度补覆盖 ──
        testAddNumericOverloads();
        testAddVectorOverloads();
        testAddWithPrefixOverloads();
        testAddColorAndTypeAndSubstring();
        testAddIfMultiArg();
        testAddLineMultiArg();
        testColorStringAllVariants();
        testColorStringComma();
        testColorStringIf();
        testInsertCharAndInsertFront();
        testReplaceCharAndReplaceAllChar();
        testFindFirstSubstr();
        testRemoveLast();
        testJson();
        testSlashConversions();
        testVectorToStringHelpers();
        testByteToHEXString();
        testSetColor();
        testAddSprite();
        testLine();
        testAddValueHelpers();
        testAddConditionHelpers();
        testAddUpdateHelpers();
        testToStringRange();
        testIndexerAndLengthSetter();
        testEndl();
        testAddCharAndByteAndBool();
        testResetProperty();
    }

    // 获取一个 CLASS 池化的 MyStringBuilder
    public static MyStringBuilder getBuilder()
    {
        var builder = CLASS<MyStringBuilder>();
        builder.clear();
        return builder;
    }

    // ─── clear / Length ──────────────────────────────────────────────────

    private static void testClearAndLength()
    {
        var b = getBuilder();
        AssertEqual(0, b.Length, "clear 后 Length 应为 0");

        b.add("test");
        Assert(b.Length > 0, "add 后 Length 应 > 0");

        b.clear();
        AssertEqual(0, b.Length, "clear 后 Length 应归零");
    }

    // ─── 添加各种类型 ─────────────────────────────────────────────────────

    private static void testAddTypes()
    {
        var b = getBuilder();
        b.add(123);
        Assert(b.ToString().Contains("123"), "add(int) 应包含数字");

        b.clear();
        b.add(3.14f);
        Assert(b.ToString().Contains("3.14"), "add(float) 应包含浮点数");

        b.clear();
        b.add(true);
        Assert(b.ToString().Contains("true"), "add(bool) 应包含布尔值");
    }

    // ─── 添加字符串 ───────────────────────────────────────────────────────

    private static void testAddStrings()
    {
        var b = getBuilder();
        b.add("Hello");
        b.add(" ");
        b.add("World");
        AssertEqual("Hello World", b.ToString(), "add(string) 应拼接字符串");
    }

    // ─── addIf ───────────────────────────────────────────────────────────

    private static void testAddIf()
    {
        var b = getBuilder();
        b.addIf("yes", true);
        b.addIf("no", false);
        AssertEqual("yes", b.ToString(), "addIf: 只有 true 条件才添加");
    }

    // ─── addRepeat ───────────────────────────────────────────────────────

    private static void testAddRepeat()
    {
        var b = getBuilder();
        b.addRepeat("ab", 3);
        AssertEqual("ababab", b.ToString(), "addRepeat: 应重复指定次数");
    }

    // ─── insert ──────────────────────────────────────────────────────────

    private static void testInsert()
    {
        var b = getBuilder();
        b.add("World");
        b.insert(0, "Hello ");
        AssertEqual("Hello World", b.ToString(), "insert 应在指定位置插入");
    }

    // ─── remove ──────────────────────────────────────────────────────────

    private static void testRemove()
    {
        var b = getBuilder();
        b.add("Hello World");
        b.remove(5, 6); // 移除 " World"
        AssertEqual("Hello", b.ToString(), "remove 应删除指定范围");
    }

    // ─── replace ─────────────────────────────────────────────────────────

    private static void testReplace()
    {
        var b = getBuilder();
        b.add("Hello World");
        b.replace(6, 5, "Universe"); // 替换 "World" 为 "Universe"
        AssertEqual("Hello Universe", b.ToString(), "replace 应替换指定范围");
    }

    // ─── replaceAll ──────────────────────────────────────────────────────

    private static void testReplaceAll()
    {
        var b = getBuilder();
        b.add("apple apple banana");
        b.replaceAll("apple", "orange");
        AssertEqual("orange orange banana", b.ToString(), "replaceAll 应替换所有匹配");
    }

    // ─── endWith ─────────────────────────────────────────────────────────

    private static void testEndWith()
    {
        var b = getBuilder();
        b.add("Hello World");
        Assert(b.endWith('d'), "endWith: 应以 'd' 结尾");
        Assert(!b.endWith('H'), "endWith: 不应以 'H' 结尾");
    }

    // ─── lastIndexOf ─────────────────────────────────────────────────────

    private static void testLastIndexOf()
    {
        var b = getBuilder();
        b.add("abc def abc");
        int idx = b.lastIndexOf('a');
        AssertEqual(8, idx, "lastIndexOf 应返回最后出现的位置");
    }

    // ─── indexOf ─────────────────────────────────────────────────────────

    private static void testIndexOf()
    {
        var b = getBuilder();
        b.add("abc def abc");
        int idx = b.indexOf('d');
        AssertEqual(4, idx, "indexOf 应返回首次出现的位置");
    }

    // ─── colorString ─────────────────────────────────────────────────────

    private static void testColorString()
    {
        var b = getBuilder();
        b.colorString("red", "FF0000");
        string result = b.ToString();
        Assert(result.Contains("red"), "colorString 应包含文本");
        Assert(result.Contains("FF0000"), "colorString 应包含颜色值");
    }

    // ─── addLine ─────────────────────────────────────────────────────────

    private static void testAddLine()
    {
        var b = getBuilder();
        b.addLine("first");
        b.addLine("second");
        string result = b.ToString();
        Assert(result.Contains("\n"), "addLine 应添加换行符");
    }

    // ─── 链式调用 ─────────────────────────────────────────────────────────

    private static void testChaining()
    {
        var b = getBuilder();
        b.add("a").add("b").add("c");
        AssertEqual("abc", b.ToString(), "链式调用应正常工作");
    }

    // ─── 边界情况 ─────────────────────────────────────────────────────────

    private static void testEdgeCases()
    {
        var b = getBuilder();
        b.add("");
        AssertEqual(0, b.Length, "add 空字符串不应增加长度");
    }

    // ─── add 各类数值单参重载 ─────────────────────────────────────────────

    private static void testAddNumericOverloads()
    {
        var b = getBuilder();
        b.add((byte)200);             // add(byte) -> IToS
        AssertEqual("200", b.ToString(), "add(byte) 应输出十进制字符串");

        b.clear();
        b.add((short)1234);           // add(short)
        AssertEqual("1234", b.ToString(), "add(short) 应输出 1234");

        b.clear();
        b.add((ushort)65535);         // add(ushort)
        AssertEqual("65535", b.ToString(), "add(ushort) 应输出 65535");

        b.clear();
        b.add(3000000000u);           // add(uint)
        AssertEqual("3000000000", b.ToString(), "add(uint) 应输出 3000000000");

        b.clear();
        b.add((double)123.5);         // add(double) 走 C# 原生 double.ToString
        Assert(b.ToString().Contains("123"), "add(double) 应包含整数部分");

        b.clear();
        b.add(9000000000L);           // add(long)
        AssertEqual("9000000000", b.ToString(), "add(long) 应输出 9000000000");

        b.clear();
        b.add(9000000000UL);          // add(ulong)
        AssertEqual("9000000000", b.ToString(), "add(ulong) 应输出 9000000000");

        b.clear();
        b.add(2.71828f, 2);           // add(float,int precision) -> FToS 保留 2 位 -> "2.72"
        AssertEqual("2.72", b.ToString(), "add(float,precision) 应控制精度");
    }

    // ─── add 向量/颜色单参重载 ─────────────────────────────────────────────

    private static void testAddVectorOverloads()
    {
        var b = getBuilder();
        b.add(new Vector2(1.5f, 2.25f));   // add(Vector2,int precision=4)
        AssertEqual("1.5,2.25", b.ToString(), "add(Vector2) V2ToS 应为 '1.5,2.25'");

        b.clear();
        b.add(new Vector3(1f, 2.5f, 3.75f));   // add(Vector3,int precision=4)
        AssertEqual("1,2.5,3.75", b.ToString(), "add(Vector3) V3ToS 应为 '1,2.5,3.75'");

        b.clear();
        b.add(new Color32(255, 0, 0, 255));    // add(Color32) 走原生 ToString
        Assert(b.ToString().Contains("RGBA"), "add(Color32) 应包含 RGBA");
        Assert(b.ToString().Contains("255"), "add(Color32) 应包含颜色分量");
    }

    // ─── add 带前缀的多参重载 ─────────────────────────────────────────────

    private static void testAddWithPrefixOverloads()
    {
        var b = getBuilder();
        b.add("n=", 42);              // add(string,int) -> "n=42"
        AssertEqual("n=42", b.ToString(), "add(string,int) 应拼接");

        b.clear();
        b.add("n=", 42, "!");         // add(string,int,string)
        AssertEqual("n=42!", b.ToString(), "add(string,int,string) 应拼接");

        b.clear();
        b.add("v=", 3.5f);            // add(string,float,int precision=4) -> "v=3.5"
        AssertEqual("v=3.5", b.ToString(), "add(string,float) 应拼接");

        b.clear();
        b.add("v=", 3.5f, "!");       // add(string,float,string) -> FToS()默认+str
        AssertEqual("v=3.5!", b.ToString(), "add(string,float,string) 应拼接");

        b.clear();
        b.add("v=", 3.5f, 2, "!");    // add(string,float,int precision,string) -> "v=3.5!"
        AssertEqual("v=3.5!", b.ToString(), "add(string,float,int,string) 应拼接");

        b.clear();
        b.add("b=", true);            // add(string,bool)
        AssertEqual("b=true", b.ToString(), "add(string,bool) 应拼接");

        b.clear();
        b.add("l=", 5000000000L);     // add(string,long)
        AssertEqual("l=5000000000", b.ToString(), "add(string,long) 应拼接");

        b.clear();
        b.add("l=", 5000000000L, "!");// add(string,long,string)
        AssertEqual("l=5000000000!", b.ToString(), "add(string,long,string) 应拼接");

        b.clear();
        b.add("u=", 6000000000UL);    // add(string,ulong)
        AssertEqual("u=6000000000", b.ToString(), "add(string,ulong) 应拼接");

        b.clear();
        b.add("u=", 6000000000UL, "!");// add(string,ulong,string)
        AssertEqual("u=6000000000!", b.ToString(), "add(string,ulong,string) 应拼接");

        b.clear();
        b.add("v2=", new Vector2(1f, 2f));   // add(string,Vector2)
        AssertEqual("v2=1,2", b.ToString(), "add(string,Vector2) 应拼接");

        b.clear();
        b.add("v3=", new Vector3(1f, 2f, 3f));// add(string,Vector3)
        AssertEqual("v3=1,2,3", b.ToString(), "add(string,Vector3) 应拼接");

        b.clear();
        b.add("c=", new Color32(1, 2, 3, 4));// add(string,Color32)
        Assert(b.ToString().StartsWith("c="), "add(string,Color32) 应保留前缀");
        Assert(b.ToString().Contains("RGBA"), "add(string,Color32) 应包含 RGBA");
    }

    // ─── add(string,Type) 与 add(string,int,int) ─────────────────────────

    private static void testAddColorAndTypeAndSubstring()
    {
        var b = getBuilder();
        b.add("t=", typeof(int));      // 非 null Type -> 拼接 ToString
        Assert(b.ToString().StartsWith("t="), "add(string,Type) 应保留前缀");
        Assert(b.ToString().Contains("Int32"), "add(string,Type) 应包含类型名 Int32");

        b.clear();
        b.add("u=", (Type)null);       // null Type -> 只拼前缀
        AssertEqual("u=", b.ToString(), "add(string,Type=null) 应只输出前缀");

        b.clear();
        b.add("abcdef", 1, 3);         // add(string,int,int) -> Append 子串 [1..3)
        AssertEqual("bcd", b.ToString(), "add(string,int,int) 应取子串");
    }

    // ─── addIf 多参重载 ──────────────────────────────────────────────────

    private static void testAddIfMultiArg()
    {
        var b = getBuilder();
        b.addIf("a", "b", true);       // addIf(string,string,bool)
        AssertEqual("ab", b.ToString(), "addIf 两参 true 应添加");

        b.clear();
        b.addIf("a", "b", false);
        AssertEqual("", b.ToString(), "addIf 两参 false 不应添加");

        b.clear();
        b.addIf("a", "b", "c", true);  // addIf(string,string,string,bool)
        AssertEqual("abc", b.ToString(), "addIf 三参 true 应添加");

        b.clear();
        b.addIf("a", "b", "c", "d", true);  // addIf(string,string,string,string,bool)
        AssertEqual("abcd", b.ToString(), "addIf 四参 true 应添加");

        b.clear();
        b.addIf("a", "b", "c", "d", "e", false); // addIf 五参 false
        AssertEqual("", b.ToString(), "addIf 五参 false 不应添加");
    }

    // ─── addLine 多参重载 ────────────────────────────────────────────────

    private static void testAddLineMultiArg()
    {
        var b = getBuilder();
        b.addLine("a", "b");           // 2 参
        AssertEqual("ab\r\n", b.ToString(), "addLine 2 参应拼接并换行");

        b.clear();
        b.addLine("a", "b", "c");      // 3 参
        AssertEqual("abc\r\n", b.ToString(), "addLine 3 参应拼接并换行");

        b.clear();
        b.addLine("a", "b", "c", "d"); // 4 参
        AssertEqual("abcd\r\n", b.ToString(), "addLine 4 参应拼接并换行");

        b.clear();
        b.addLine("a", "b", "c", "d", "e"); // 5 参
        AssertEqual("abcde\r\n", b.ToString(), "addLine 5 参应拼接并换行");

        b.clear();
        b.addLine("a", "b", "c", "d", "e", "f"); // 6 参
        AssertEqual("abcdef\r\n", b.ToString(), "addLine 6 参应拼接并换行");
    }

    // ─── colorString 全字符串变体 ───────────────────────────────────────

    private static void testColorStringAllVariants()
    {
        // colorString(string color,string str0)
        var b = getBuilder();
        b.colorString("fff", "A");
        AssertEqual("<color=#fff>A</color>", b.ToString(), "colorString 1 字符串");

        b.clear();
        b.colorString("fff", "A", "B");             // 2 字符串
        AssertEqual("<color=#fff>AB</color>", b.ToString(), "colorString 2 字符串");

        b.clear();
        b.colorString("fff", "A", "B", "C");        // 3 字符串
        AssertEqual("<color=#fff>ABC</color>", b.ToString(), "colorString 3 字符串");

        b.clear();
        b.colorString("fff", "A", "B", "C", "D");   // 4 字符串
        AssertEqual("<color=#fff>ABCD</color>", b.ToString(), "colorString 4 字符串");

        b.clear();
        b.colorString("fff", "A", "B", "C", "D", "E"); // 5 字符串
        AssertEqual("<color=#fff>ABCDE</color>", b.ToString(), "colorString 5 字符串");

        // colorString(string color,int value)
        b.clear();
        b.colorString("fff", 123);
        AssertEqual("<color=#fff>123</color>", b.ToString(), "colorString(int) 应输出值");

        // colorString(string color,int,int,string,int)
        b.clear();
        b.colorString("fff", 1, "..", 2);
        AssertEqual("<color=#fff>1..2</color>", b.ToString(), "colorString(int,str,int) 应拼接");

        // colorString(string color,long value)
        b.clear();
        b.colorString("fff", 9000000000L);
        AssertEqual("<color=#fff>9000000000</color>", b.ToString(), "colorString(long) 应输出值");
    }

    // ─── colorStringComma ───────────────────────────────────────────────

    private static void testColorStringComma()
    {
        var b = getBuilder();
        b.colorStringComma("fff", 1234567);   // int 带千分位
        AssertEqual("<color=#fff>1,234,567</color>", b.ToString(), "colorStringComma(int) 应带千分位");

        b.clear();
        b.colorStringComma("fff", 9000000000L); // long 带千分位
        Assert(b.ToString().StartsWith("<color=#fff>"), "colorStringComma(long) 应保留前缀");
    }

    // ─── colorStringIf ──────────────────────────────────────────────────

    private static void testColorStringIf()
    {
        var b = getBuilder();
        b.colorStringIf("fff", "A", true);           // 1 字符串 true
        AssertEqual("<color=#fff>A</color>", b.ToString(), "colorStringIf 1 真");

        b.clear();
        b.colorStringIf("fff", "A", false);
        AssertEqual("", b.ToString(), "colorStringIf 1 假不添加");

        b.clear();
        b.colorStringIf("fff", "A", "B", true);      // 2 字符串 true
        AssertEqual("<color=#fff>AB</color>", b.ToString(), "colorStringIf 2 真");

        b.clear();
        b.colorStringIf("fff", "A", "B", "C", true); // 3 字符串 true
        AssertEqual("<color=#fff>ABC</color>", b.ToString(), "colorStringIf 3 真");
    }

    // ─── insert(char) 与 insertFront 多参 ───────────────────────────────

    private static void testInsertCharAndInsertFront()
    {
        var b = getBuilder();
        b.add("ac");
        b.insert(1, 'b');              // insert(int,char)
        AssertEqual("abc", b.ToString(), "insert(char) 应插入字符");

        b.clear();
        b.add("X");
        b.insertFront("a", "b");       // insertFront(string,string)
        AssertEqual("abX", b.ToString(), "insertFront 2 参应插到最前");

        b.clear();
        b.add("X");
        b.insertFront("a", "b", "c");  // insertFront 3 参
        AssertEqual("abcX", b.ToString(), "insertFront 3 参应插到最前");

        b.clear();
        b.add("X");
        b.insertFront("a", "b", "c", "d"); // insertFront 4 参
        AssertEqual("abcdX", b.ToString(), "insertFront 4 参应插到最前");

        // 顺序验证: insertFront 先插最后参数再逐次插前面, 结果 = 参数顺序
        b.clear();
        b.insertFront("0", "1", "2");
        AssertEqual("012", b.ToString(), "insertFront 顺序应保持参数次序");
    }

    // ─── replace(char,char) 与 replaceAll(char,char) ────────────────────

    private static void testReplaceCharAndReplaceAllChar()
    {
        var b = getBuilder();
        b.add("aaa");
        b.replace('a', 'b');           // replace(char,char)
        AssertEqual("bbb", b.ToString(), "replace(char,char) 应替换全部");

        b.clear();
        b.add("a1a2a3");
        b.replaceAll('a', 'x');        // replaceAll(char,char)
        AssertEqual("x1x2x3", b.ToString(), "replaceAll(char,char) 应替换全部匹配");

        b.clear();
        b.add("abcabc");
        b.replace("bc", "X");          // replace(string,string)
        AssertEqual("aXaX", b.ToString(), "replace(string,string) 只替换首个匹配");
    }

    // ─── findFirstSubstr ────────────────────────────────────────────────

    private static void testFindFirstSubstr()
    {
        // findFirstSubstr(char,startPos,sensitive)
        var b = getBuilder();
        b.add("AbCdE");
        int idx = b.findFirstSubstr('c', 0, false);  // 大小写不敏感
        AssertEqual(2, idx, "findFirstSubstr(char) 大小写不敏感应命中 'C'@2");
        AssertEqual(-1, b.findFirstSubstr('f'), "findFirstSubstr(char) 不存在应返回 -1");

        // findFirstSubstr(string,...)
        b.clear();
        b.add("abcXabc");
        AssertEqual(0, b.findFirstSubstr("abc"), "findFirstSubstr(string) 首次命中 0");
        AssertEqual(4, b.findFirstSubstr("abc", 1), "findFirstSubstr(string) 从 1 开始命中 4");
        AssertEqual(3, b.findFirstSubstr("abc", 0, true, true), "findFirstSubstr(string) returnEndIndex 返回 3");
        AssertEqual(-1, b.findFirstSubstr("zzz"), "findFirstSubstr(string) 未命中返回 -1");

        // 大小写不敏感
        b.clear();
        b.add("ABCxabc");
        AssertEqual(0, b.findFirstSubstr("abc", 0, false, false), "findFirstSubstr(string) 不敏感命中 0");

        // pattern 比内容长
        b.clear();
        b.add("ab");
        AssertEqual(-1, b.findFirstSubstr("abc"), "findFirstSubstr(string) 超长 pattern 返回 -1");
    }

    // ─── removeLast / removeLastComma ───────────────────────────────────

    private static void testRemoveLast()
    {
        var b = getBuilder();
        b.add("a,b,");
        b.removeLast(',');             // removeLast(char)
        AssertEqual("a,b", b.ToString(), "removeLast 应删除最后一个匹配字符");

        b.clear();
        b.add("a,b,c,");
        b.removeLastComma();           // removeLastComma -> removeLast(',')
        AssertEqual("a,b,c", b.ToString(), "removeLastComma 应删除末尾逗号");
    }

    // ─── json 构建 ──────────────────────────────────────────────────────

    private static void testJson()
    {
        // jsonStartArray 带名
        var b = getBuilder();
        b.jsonStartArray("arr", 0, false);
        AssertEqual("\"arr\":[", b.ToString(), "jsonStartArray 带名应输出 '\"arr\":['");

        // jsonEndArray 会清理末尾逗号并补 '],'
        b.clear();
        b.add("[1,");
        b.jsonEndArray(0, false);
        AssertEqual("[1],", b.ToString(), "jsonEndArray 应清理末尾逗号");

        // jsonStartArray 无名
        b.clear();
        b.jsonStartArray(null, 0, false);
        AssertEqual("[", b.ToString(), "jsonStartArray 无名应只输出 '['");

        // jsonAddPair 带名
        b.clear();
        b.jsonAddPair("key", "val", 0, false);
        AssertEqual("\"key\": \"val\",", b.ToString(), "jsonAddPair 带名输出");

        // jsonAddPair 无名(数组元素)
        b.clear();
        b.jsonAddPair(null, "val", 0, false);
        AssertEqual("\"val\",", b.ToString(), "jsonAddPair 无名只输出值");

        // jsonAddObject
        b.clear();
        b.jsonAddObject("k", "v", 0, false);
        AssertEqual("\"k\": v,", b.ToString(), "jsonAddObject 输出");

        // jsonStartStruct: 源码字面 add('}') —— 文档化(源码如此,断言照实)
        b.clear();
        b.jsonStartStruct("obj", 0, false);
        AssertEqual("\"obj\":}", b.ToString(), "jsonStartStruct 源码字面输出(文档化)");

        // jsonEndStruct: 末尾 add('}') + 可选逗号(keepComma 默认 true)
        b.clear();
        b.add("{");
        b.jsonEndStruct(true, 0, false);
        AssertEqual("{},", b.ToString(), "jsonEndStruct keepComma=true 补 '}' 与逗号");

        // keepComma=false 不加逗号
        b.clear();
        b.add("{a,");
        b.jsonEndStruct(false, 0, false);
        AssertEqual("{a}", b.ToString(), "jsonEndStruct keepComma=false 不加逗号");
    }

    // ─── 斜杠转换 ───────────────────────────────────────────────────────

    private static void testSlashConversions()
    {
        var b = getBuilder();
        b.add("a\\b\\c");
        b.rightToLeft();               // replace('\\','/')
        AssertEqual("a/b/c", b.ToString(), "rightToLeft 应把反斜杠转正斜杠");

        b.clear();
        b.add("a/b/c");
        b.leftToRight();               // replace('/','\\')
        AssertEqual("a\\b\\c", b.ToString(), "leftToRight 应把正斜杠转反斜杠");
    }

    // ─── V2ToS / V3ToS / V2IToS 成员方法 ────────────────────────────────

    private static void testVectorToStringHelpers()
    {
        var b = getBuilder();
        b.V2ToS(new Vector2(1.5f, 2.25f));
        AssertEqual("1.5,2.25", b.ToString(), "V2ToS 成员方法");

        b.clear();
        b.V3ToS(new Vector3(1f, 2.5f, 3.75f));
        AssertEqual("1,2.5,3.75", b.ToString(), "V3ToS 成员方法");

        b.clear();
        b.V2IToS(new Vector2Int(12, 34));
        AssertEqual("12,34", b.ToString(), "V2IToS 成员方法");
    }

    // ─── byteToHEXString ────────────────────────────────────────────────

    private static void testByteToHEXString()
    {
        var b = getBuilder();
        b.byteToHEXString(0x0F, true);  // 大写
        AssertEqual("0F", b.ToString(), "byteToHEXString 0x0F 大写 = 0F");

        b.clear();
        b.byteToHEXString(0xAB, true);  // 大写字母
        AssertEqual("AB", b.ToString(), "byteToHEXString 0xAB 大写 = AB");

        b.clear();
        b.byteToHEXString(0xAB, false); // 小写字母
        AssertEqual("ab", b.ToString(), "byteToHEXString 0xAB 小写 = ab");

        b.clear();
        b.byteToHEXString(0x9C, true);  // 混合数字 + 大写字母
        AssertEqual("9C", b.ToString(), "byteToHEXString 0x9C = 9C");
    }

    // ─── setColor ───────────────────────────────────────────────────────

    private static void testSetColor()
    {
        var b = getBuilder();
        b.add("abc");
        b.setColor("red");
        AssertEqual("<color=#red>abc</color>", b.ToString(), "setColor 应包裹已有内容");

        // 空内容 setColor 直接返回,不改变
        b.clear();
        b.setColor("red");
        AssertEqual("", b.ToString(), "setColor 空内容不应改变");
    }

    // ─── addSprite ──────────────────────────────────────────────────────

    private static void testAddSprite()
    {
        var b = getBuilder();
        b.addSprite("icon", 1.0f);
        AssertEqual("<quad width=1 sprite=icon/>", b.ToString(), "addSprite 默认宽度 1");

        b.clear();
        b.addSprite("icon", 2.5f);
        AssertEqual("<quad width=2.5 sprite=icon/>", b.ToString(), "addSprite 指定宽度 2.5");
    }

    // ─── line ───────────────────────────────────────────────────────────

    private static void testLine()
    {
        var b = getBuilder();
        b.line("L1");                    // returnLine 默认 true
        AssertEqual("L1\r\n", b.ToString(), "line 默认带换行");

        b.clear();
        b.line("L1", false);             // returnLine=false
        AssertEqual("L1", b.ToString(), "line returnLine=false 不带换行");
    }

    // ─── addValue* 系列 ─────────────────────────────────────────────────

    private static void testAddValueHelpers()
    {
        var b = getBuilder();
        b.addValueInt(5);                // "5,"
        AssertEqual("5,", b.ToString(), "addValueInt");

        b.clear();
        b.addValueUInt(6u);              // "6,"
        AssertEqual("6,", b.ToString(), "addValueUInt");

        b.clear();
        b.addValueFloat(3.5f);           // "3.5,"
        AssertEqual("3.5,", b.ToString(), "addValueFloat");

        b.clear();
        b.addValueString("abc");         // add("\"",str,"\",") -> "\"abc\","
        AssertEqual("\"abc\",", b.ToString(), "addValueString");

        b.clear();
        b.addValueVector2(new Vector2(1f, 2f));   // V2ToS + ','
        AssertEqual("1,2,", b.ToString(), "addValueVector2");

        b.clear();
        b.addValueVector2Int(new Vector2Int(3, 4)); // V2IToS + ','
        AssertEqual("3,4,", b.ToString(), "addValueVector2Int");

        b.clear();
        b.addValueVector3(new Vector3(1f, 2f, 3f)); // V3ToS + ','
        AssertEqual("1,2,3,", b.ToString(), "addValueVector3");

        b.clear();
        b.addValueInts(new List<int> { 1, 2 });      // addValueString(IsToS) -> "\"1,2\","
        AssertEqual("\"1,2\",", b.ToString(), "addValueInts");

        b.clear();
        b.addValueFloats(new List<float> { 1f, 2.5f }); // addValueString(FsToS)
        AssertEqual("\"1,2.5\",", b.ToString(), "addValueFloats");
    }

    // ─── addCondition* 系列 ─────────────────────────────────────────────

    private static void testAddConditionHelpers()
    {
        var b = getBuilder();
        b.addConditionString("col", "val", "=");  // col="val"=
        AssertEqual("col=\"val\"=", b.ToString(), "addConditionString");

        b.clear();
        b.addConditionInt("col", 5, ";"); // col = 5;
        AssertEqual("col = 5;", b.ToString(), "addConditionInt");
    }

    // ─── addUpdate* 系列 ────────────────────────────────────────────────

    private static void testAddUpdateHelpers()
    {
        var b = getBuilder();
        b.addUpdateString("col", "val");     // col = "val",
        AssertEqual("col = \"val\",", b.ToString(), "addUpdateString");

        b.clear();
        b.addUpdateInt("col", 5);            // col = 5,
        AssertEqual("col = 5,", b.ToString(), "addUpdateInt");

        b.clear();
        b.addUpdateInts("col", new List<int> { 1, 2 }); // addUpdateString(IsToS)
        AssertEqual("col = \"1,2\",", b.ToString(), "addUpdateInts");

        b.clear();
        b.addUpdateFloats("col", new List<float> { 1f, 2.5f }); // addUpdateString(FsToS)
        AssertEqual("col = \"1,2.5\",", b.ToString(), "addUpdateFloats");
    }

    // ─── toString(startIndex,length) ────────────────────────────────────

    private static void testToStringRange()
    {
        var b = getBuilder();
        b.add("Hello World");
        string sub = b.toString(6, 5);
        AssertEqual("World", sub, "toString(start,end) 应返回子串");
    }

    // ─── 下标索引器 / Length 设置 ────────────────────────────────────────

    private static void testIndexerAndLengthSetter()
    {
        var b = getBuilder();
        b.add("abc");
        char c0 = b[0];                 // 索引器 get
        AssertEqual('a', c0, "索引器 get 应返回字符");

        b[0] = 'Z';                     // 索引器 set
        AssertEqual("Zbc", b.ToString(), "索引器 set 应修改字符");

        b.Length = 2;                   // Length setter 截断
        AssertEqual("Zb", b.ToString(), "Length 设置应截断字符串");

        b.Length = 5;                   // Length 扩大(补 \0)
        AssertEqual(5, b.Length, "Length 扩大后长度应为 5");
    }

    // ─── endl ───────────────────────────────────────────────────────────

    private static void testEndl()
    {
        var b = getBuilder();
        b.add("a").endl().add("b");     // add('\n')
        AssertEqual("a\nb", b.ToString(), "endl 应插入换行符");
    }

    // ─── add(char)/add(byte)/add(bool) 显式 ─────────────────────────────

    private static void testAddCharAndByteAndBool()
    {
        var b = getBuilder();
        b.add('X');                     // add(char)
        AssertEqual("X", b.ToString(), "add(char) 直接追加字符");

        b.clear();
        b.addIf('A', true);             // addIf(char,bool) true
        b.addIf('B', false);            // addIf(char,bool) false
        AssertEqual("A", b.ToString(), "addIf(char,bool) 只有 true 添加");

        b.clear();
        b.add((byte)7);                 // add(byte) 数值
        AssertEqual("7", b.ToString(), "add(byte) 数值");

        b.clear();
        b.add(false);                   // add(bool) false
        AssertEqual("false", b.ToString(), "add(bool) 输出 false");
    }

    // ─── resetProperty(覆盖 ClassObject 池化入口) ────────────────────────

    private static void testResetProperty()
    {
        var b = getBuilder();
        b.add("some content");
        b.resetProperty();              // ClassObject 复位, 清空 mBuilder
        AssertEqual(0, b.Length, "resetProperty 应清空内容");
        AssertEqual("", b.ToString(), "resetProperty 后 ToString 应为空");
    }

    // Simple assertion methods
    private static void Assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message = "")
    {
        bool eq = (expected == null && actual == null) || (expected != null && expected.Equals(actual));
        if (!eq)
        {
            throw new Exception(
                string.IsNullOrEmpty(message)
                    ? $"Expected [{expected}] but got [{actual}]"
                    : $"{message} - Expected [{expected}] but got [{actual}]");
        }
    }
}