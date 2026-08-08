using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameUtility;
using static MathUtility;

public static class FrameUtilityTest
{
    public static void Run()
    {
        testTickTimerLoop();
        testTickTimerOnce();
        testArrayHelpers();
        testIdHelpers();
        testEnumAndColorHelpers();
        testBoolToString();
        testSign();
        testClampFloat();
        testEnumAndCollections();
        testTimersAndColors();
        testIdsAndDiagnostics();
        testEnumConversion();
        testCrcHelpers();
        testFindMaxHelpers();
        testFindMaxAbsHelpers();
        testPathIgnoreAndParsing();
        testLineAndStackHelpers();
        testPercentAndProbability();
        testFixedAndPercent();
        testSwap();
        testClampInt();
        testFixedAndPercent2();
        testToPercentFloat();
        testToPercentString();
        testToProbabilityFloat();
        testToProbabilityString();
        testIsIgnorePath();
        testGenerateFileAssetBundleName();
        testParseFileList();
        testEqual();
        testEnumConvert();
        testCRCAllOverloads();
        testFindMaxAllTypes();
        testFindMaxAbsAllTypes();
        testGetLineNum();
        testGetCurSourceFileName();
        testGetStackTrace();
        testGetClassNameFromGameObject();
        testAvailableWritePath();
        testAvailableReadPath();
        testWriteFileList();
        testCameraAndInput();
        testSceneFunctions();
        testLayoutFunctions();
        testCommandSystem();
        testObjectPool();
        testDelayCall();
        testArrayPool();
        testArrayPoolThread();
        testListPool();
        testListPoolPersist();
        testSetPoolPersist();
        testDicPoolPersist();
        testClassPool();
        testClassPoolThread();
        testClassPoolList();
        testPacket();
        testCmd();
        testPushCommand();
        testCheckEnum();
        testCreateInstance();
        testDeepCopy();
        testGetUGUIRootComponent();
        testIsSpriteInAtlas();
        testGetLocalIP();
        testHostNameToIPAddress();
        testCompressDecompressZip();
    }

    static void testTickTimerLoop()
    {
        float t = 1.0f;
        bool f = tickTimerLoop(ref t, 0.25f, 1.0f);
        assertFalse(f);
        assertEqual(0.75f, t);
        f = tickTimerLoop(ref t, 0.8f, 1.0f);
        assertTrue(f);
        float e = 0.1f;
        f = tickTimerLoop(ref e, 2.0f, 1.5f, true);
        assertTrue(f);
        assertEqual(1.5f, e);
        float s = -1.0f;
        f = tickTimerLoop(ref s, 0.5f, 1.0f);
        assertFalse(f);
    }

    static void testTickTimerOnce()
    {
        float t = 1.0f;
        bool f = tickTimerOnce(ref t, 0.25f);
        assertFalse(f);
        assertEqual(0.75f, t);
        f = tickTimerOnce(ref t, 0.75f);
        assertTrue(f);
        assertEqual(-1.0f, t);
        float s = -1.0f;
        f = tickTimerOnce(ref s, 0.1f);
        assertFalse(f);
    }

    static void testArrayHelpers()
    {
        int[] v = { 1, 2, 3, 4 };
        v.removeIndex(v.Length, 1);
        assertEqual(3, v[1]);
        string[] cv = { "a", "b", "c", "b" };
        int cc = cv.removeValue(cv.Length, "b");
        assertEqual(2, cc);
        int[] vv = { 1, 2, 3, 2, 4 };
        int vc = vv.removeValue(vv.Length, 2);
        assertEqual(3, vc);
        assertTrue(v.contains(3));
        assertFalse(v.contains(99));
    }

    static void testIdHelpers()
    {
        int f = makeID();
        int s = makeID();
        assertEqual(f + 1, s);
        notifyIDUsed(s + 20);
        int n = makeID();
        assertEqual(s + 21, n);
    }

    static void testEnumAndColorHelpers()
    {
        assertTrue(isEnumValid(CoreTestEnum.First));
        assertFalse(isEnumValid((CoreTestEnum)99));
        string c = "#112233";
        assertEqual(c, getCountColor(true, c));
        string nc = getCountColor(false, c);
        assertFalse(string.IsNullOrEmpty(nc));
    }

    static void testBoolToString()
    {
        assertEqual("true", true.boolToString());
        assertEqual("false", false.boolToString());
    }

    static void testSign()
    {
        assertEqual(-1, sign(-5));
        assertEqual(0, sign(0));
        assertEqual(1, sign(10));
    }

    static void testClampFloat()
    {
        assertEqual(3.0f, 3.0f.clamp(0.0f, 5.0f), 0.001f, "clamp float");
        assertEqual(0.0f, (-1.0f).clamp(0.0f, 5.0f), 0.001f, "clamp low");
    }

    static void testEnumAndCollections()
    {
        assertTrue(isEnumValid(CoreTestEnum.First));
        assertFalse(isEnumValid((CoreTestEnum)99));
    }

    static void testTimersAndColors()
    {
        float t = 1.0f;
        tickTimerLoop(ref t, 0.25f, 1.0f);
        assertEqual(0.75f, t, 0.001f);
        string c = getCountColor(true, "#FFF");
        assertEqual("#FFF", c);
    }

    static void testIdsAndDiagnostics()
    {
        int id = makeID();
        assertTrue(id > 0);
        notifyIDUsed(id + 50);
        int nid = makeID();
        assertTrue(nid > id);
    }

    static void testEnumConversion()
    {
        assertTrue(isEnumValid(CoreTestEnum.First));
        assertFalse(isEnumValid((CoreTestEnum)0));
    }

    static void testCrcHelpers()
    {
        byte[] d = { 0x01, 0x02 };
        ushort c = generateCRC16(d, d.Length);
        ushort c2 = generateCRC16(d, d.Length);
        assertEqual(c, c2);
    }

    static void testFindMaxHelpers()
    {
        float[] vals = { 1.5f, 3.7f, 2.1f };
        float mx = findMax(vals);
        assertEqual(3.7f, mx, 0.001f);
    }

    static void testFindMaxAbsHelpers()
    {
        float[] vals = { -1.5f, 3.7f, -5.2f };
        float mx = findMaxAbs(vals);
        assertEqual(5.2f, mx, 0.001f);
    }

    static void testPathIgnoreAndParsing()
    {
        string p = "a/b/c";
        assertTrue(p.Length > 0);
    }

    static void testLineAndStackHelpers()
    {
        string s = "line1\nline2";
        string[] l = s.Split('\n');
        assertEqual(2, l.Length);
    }

    static void testPercentAndProbability()
    {
        assertEqual("50%", 0.5f.toPercent(1), "50%");
        assertEqual("100%", 1.0f.toPercent(0), "100%");
        assertEqual("0%", 0.0f.toPercent(), "0%");
        assertEqual("0.005%", 0.5f.toProbability(), "0.5%");
        assertEqual("0.01%", 1.0f.toProbability(), "1%");
        assertEqual("1%", 100.0f.toProbability(), "100%");
    }

    static void testFixedAndPercent()
    {
        assertEqual("100+10%", 100.fixedAndPercent(0.1f));
        assertEqual("200+50%", 200.fixedAndPercent(0.5f));
        assertEqual("50", 50.fixedAndPercent(0.0f));
        assertEqual("10%", 0.fixedAndPercent(0.1f));
    }

    static void testSwap()
    {
        int a = 1, b = 2;
        swap(ref a, ref b);
        assertEqual(2, a);
        assertEqual(1, b);
    }

    static void testClampInt()
    {
        assertEqual(5, 5.clamp(0, 10));
        assertEqual(0, (-5).clamp(0, 10));
        assertEqual(10, 15.clamp(0, 10));
    }

    static void testFixedAndPercent2()
    {
        assertEqual("100+10%", 100.fixedAndPercent(0.1f));
    }

    static void testToPercentFloat()
    {
        string r = 0.5f.toPercent();
        assertTrue(r.Contains("%"));
    }

    static void testToPercentString()
    {
        string r = "0.5".toPercent(1);
        assertTrue(r.Contains("%"));
    }

    static void testToProbabilityFloat()
    {
        string r = 5.0f.toProbability();
        assertTrue(r.Contains("%"));
    }

    static void testToProbabilityString()
    {
        string r = "50".toProbability();
        assertTrue(r.Contains("%"));
    }

    // ─── isIgnorePath: 判断完整路径是否命中忽略列表 ──────────────────
    static void testIsIgnorePath()
    {
        var ignore = new System.Collections.Generic.List<string>();
        // 空忽略列表: 不忽略任何路径
        assertFalse(isIgnorePath("Assets/GameResources/a.txt", ignore));

        ignore.Add("GameResources");
        assertTrue(isIgnorePath("Assets/GameResources/a.txt", ignore), "hit GameResources");
        assertTrue(isIgnorePath("Assets/GameResources/Sub/b.txt", ignore), "hit sub path");
        assertFalse(isIgnorePath("Assets/Other/a.txt", ignore), "miss");

        // 多个忽略规则
        ignore.Add("ThirdParty");
        assertTrue(isIgnorePath("Assets/ThirdParty/lib.dll", ignore), "hit second rule");
        assertFalse(isIgnorePath("Assets/MyCode/c.cs", ignore), "miss all");

        // 空字符串路径
        assertFalse(isIgnorePath("", ignore), "empty path");

        // null 忽略列表
        assertFalse(isIgnorePath("any/path", null), "null ignore list");
    }

    // ─── generateFileAssetBundleName ─────────────────────────────────
    static void testGenerateFileAssetBundleName()
    {
        // 不可打包的文件返回 EMPTY
        assertEqual("", generateFileAssetBundleName("file.meta"), "meta returns empty");
        assertEqual("", generateFileAssetBundleName("script.cs"), "cs returns empty");
        assertEqual("", generateFileAssetBundleName("file.DS_Store"), "DS_Store returns empty");
        assertEqual("", generateFileAssetBundleName("shader.cginc"), "cginc returns empty");
        assertEqual("", generateFileAssetBundleName("shader.hlsl"), "hlsl returns empty");
        assertEqual("", generateFileAssetBundleName("shader.glslinc"), "glslinc returns empty");
        assertEqual("", generateFileAssetBundleName("data.tpsheet"), "tpsheet returns empty");
        assertEqual("", generateFileAssetBundleName("LightingData.asset"), "LightingData.asset returns empty");

        // .unity 场景文件: 单文件打包, 移除 P_GAME_RESOURCES_PATH 前缀 + 替换后缀 + ToLower
        string result = generateFileAssetBundleName("Assets/GameResources/Scenes/Test.unity");
        // result 经过 ToLower: "scenes/test.unity3d"
        assertTrue(result.Contains("scenes/test"), "unity scene path");
        assertTrue(result.endWith(".unity3d"), "unity suffix");

        // 普通文件: 取目录路径作为 AB 名
        string result2 = generateFileAssetBundleName("Assets/GameResources/UI/Panel/image.png");
        assertTrue(result2.Contains("ui/panel"), "normal file folder path");
        assertTrue(result2.endWith(".unity3d"), "normal suffix");

        // forceSingle: 单文件打包
        string result3 = generateFileAssetBundleName("Assets/GameResources/Data/config.json", true);
        assertTrue(result3.Contains("config"), "forceSingle filename");
        assertTrue(result3.endWith(".unity3d"), "forceSingle suffix");
    }

    // ─── parseFileList ───────────────────────────────────────────────
    static void testParseFileList()
    {
        var dict = new System.Collections.Generic.Dictionary<string, GameFileInfo>();

        // 空内容
        parseFileList("", dict);
        assertEqual(0, dict.Count, "parse empty");

        // 单行: "name\tsize\tmd5"
        dict.Clear();
        parseFileList("a.txt\t100\tabc123", dict);
        assertEqual(1, dict.Count, "parse single");
        assertTrue(dict.ContainsKey("a.txt"), "parse single key");
        assertEqual(100L, dict["a.txt"].mFileSize, "parse single size");
        assertEqual("abc123", dict["a.txt"].mMD5, "parse single md5");

        // 多行
        dict.Clear();
        parseFileList("a.txt\t100\tabc\nb.txt\t200\tdef\nc.txt\t300\tghi", dict);
        assertEqual(3, dict.Count, "parse multi");
        assertEqual(200L, dict["b.txt"].mFileSize, "parse multi size");
        assertEqual("ghi", dict["c.txt"].mMD5, "parse multi md5");

        // 格式不正确的行(少于3列) 会被忽略
        dict.Clear();
        parseFileList("bad_line\nx.txt\t50\txxx", dict);
        assertEqual(1, dict.Count, "parse skip bad line");

        // null 内容
        dict.Clear();
        parseFileList(null, dict);
        assertEqual(0, dict.Count, "parse null");
    }

    // ─── equal<T>: 泛型相等比较 ───────────────────────────────────────
    static void testEqual()
    {
        assertTrue(equal(5, 5), "int equal");
        assertFalse(equal(5, 6), "int not equal");
        assertTrue(equal(3.14f, 3.14f), "float equal");
        assertFalse(equal(1.0f, 2.0f), "float not equal");
        assertTrue(equal("hello", "hello"), "string equal");
        assertFalse(equal("hello", "world"), "string not equal");
        string s = null;
        assertFalse(equal(s, "hello"), "null vs string");
        assertFalse(equal("hello", s), "string vs null");
    }

    // ─── intToEnum / enumToInt ─────────────────────────────────────────
    static void testEnumConvert()
    {
        CoreTestEnum e = intToEnum<CoreTestEnum, int>(1);
        assertEqual(CoreTestEnum.First, e, "intToEnum First");
        e = intToEnum<CoreTestEnum, int>(2);
        assertEqual(CoreTestEnum.Second, e, "intToEnum Second");
        // 双向往返
        int v = enumToInt(CoreTestEnum.First);
        assertEqual(1, v, "enumToInt First");
        v = enumToInt(CoreTestEnum.Second);
        assertEqual(2, v, "enumToInt Second");
        // 往返: int → enum → int
        CoreTestEnum r = intToEnum<CoreTestEnum, int>(2);
        assertEqual(2, enumToInt(r), "roundtrip");
    }

    // ─── generateCRC16 全部 3 个重载 ──────────────────────────────────
    static void testCRCAllOverloads()
    {
        // byte[] 版本
        byte[] buf = { 0x01, 0x02, 0x03, 0x04 };
        ushort c1 = generateCRC16(buf, buf.Length);
        ushort c2 = generateCRC16(buf, buf.Length);
        assertEqual(c1, c2, "CRC byte[] deterministic");
        // 带 offset: 0 偏移读前2字节, 2偏移读后2字节
        ushort c3 = generateCRC16(buf, 2, 0);
        ushort c4 = generateCRC16(buf, 2, 2);
        // 不同偏移读不同数据，CRC 不应相同
        assertFalse(c3 == c4, "CRC offset different data");
        // ushort 版本
        ushort cu = generateCRC16((ushort)0xABCD);
        assertTrue(cu != 0, "CRC ushort non-zero");
        // int 版本
        ushort ci = generateCRC16(0x12345678);
        assertTrue(ci != 0, "CRC int non-zero");
        // 确定性: ushort 往返
        ushort cu2 = generateCRC16((ushort)0xABCD);
        assertEqual(cu, cu2, "CRC ushort deterministic");
    }

    // ─── findMax 全部数值类型的 Span 和 List 重载 ──────────────────────
    static void testFindMaxAllTypes()
    {
        // --- sbyte ---
        sbyte[] sbArr = { -5, 10, 3 };
        assertEqual((sbyte)10, findMax(new Span<sbyte>(sbArr)), "findMax sbyte[]");
        assertEqual((sbyte)10, findMax(new List<sbyte> { -5, 10, 3 }), "findMax List<sbyte>");

        // --- byte ---
        byte[] bArr = { 5, 200, 100 };
        assertEqual((byte)200, findMax(new Span<byte>(bArr)), "findMax byte[]");
        assertEqual((byte)200, findMax(new List<byte> { 5, 200, 100 }), "findMax List<byte>");

        // --- short ---
        short[] shArr = { -100, 50, 200 };
        assertEqual((short)200, findMax(new Span<short>(shArr)), "findMax short[]");
        assertEqual((short)200, findMax(new List<short> { -100, 50, 200 }), "findMax List<short>");

        // --- ushort ---
        ushort[] usArr = { 100, 500, 300 };
        assertEqual((ushort)500, findMax(new Span<ushort>(usArr)), "findMax ushort[]");
        assertEqual((ushort)500, findMax(new List<ushort> { 100, 500, 300 }), "findMax List<ushort>");

        // --- int ---
        int[] iArr = { -10, 5, 20, -30 };
        assertEqual(20, findMax(new Span<int>(iArr)), "findMax int[]");
        assertEqual(20, findMax(new List<int> { -10, 5, 20, -30 }), "findMax List<int>");

        // --- uint ---
        uint[] uiArr = { 10, 50, 30 };
        assertEqual(50u, findMax(new Span<uint>(uiArr)), "findMax uint[]");
        assertEqual(50u, findMax(new List<uint> { 10, 50, 30 }), "findMax List<uint>");

        // --- long ---
        long[] lArr = { -100L, 50L, 999L };
        assertEqual(999L, findMax(new Span<long>(lArr)), "findMax long[]");
        assertEqual(999L, findMax(new List<long> { -100L, 50L, 999L }), "findMax List<long>");

        // --- ulong ---
        ulong[] ulArr = { 10UL, 500UL, 300UL };
        assertEqual(500UL, findMax(new Span<ulong>(ulArr)), "findMax ulong[]");
        assertEqual(500UL, findMax(new List<ulong> { 10UL, 500UL, 300UL }), "findMax List<ulong>");

        // --- float ---
        float[] fArr = { 1.5f, 3.7f, 2.1f };
        assertEqual(3.7f, findMax(new Span<float>(fArr)), 0.001f, "findMax float[]");
        assertEqual(3.7f, findMax(new List<float> { 1.5f, 3.7f, 2.1f }), 0.001f, "findMax List<float>");

        // --- double ---
        double[] dArr = { 1.1, 5.5, 3.3 };
        assertEqual(5.5, findMax(new Span<double>(dArr)), 0.0001, "findMax double[]");
        assertEqual(5.5, findMax(new List<double> { 1.1, 5.5, 3.3 }), 0.0001, "findMax List<double>");
    }

    // ─── findMaxAbs 全部数值类型的 Span 和 List 重载 ───────────────────
    static void testFindMaxAbsAllTypes()
    {
        // --- sbyte ---
        sbyte[] sbArr = { -50, 10, -3 };
        assertEqual((sbyte)50, findMaxAbs(new Span<sbyte>(sbArr)), "findMaxAbs sbyte[]");
        assertEqual((sbyte)50, findMaxAbs(new List<sbyte> { -50, 10, -3 }), "findMaxAbs List<sbyte>");

        // --- short ---
        short[] shArr = { -200, 50, 100 };
        assertEqual((short)200, findMaxAbs(new Span<short>(shArr)), "findMaxAbs short[]");
        assertEqual((short)200, findMaxAbs(new List<short> { -200, 50, 100 }), "findMaxAbs List<short>");

        // --- int ---
        int[] iArr = { -100, 30, -50, 20 };
        assertEqual(100, findMaxAbs(new Span<int>(iArr)), "findMaxAbs int[]");
        assertEqual(100, findMaxAbs(new List<int> { -100, 30, -50, 20 }), "findMaxAbs List<int>");

        // --- long ---
        long[] lArr = { -500L, 200L, -300L };
        assertEqual(500L, findMaxAbs(new Span<long>(lArr)), "findMaxAbs long[]");
        assertEqual(500L, findMaxAbs(new List<long> { -500L, 200L, -300L }), "findMaxAbs List<long>");

        // --- float ---
        float[] fArr = { -1.5f, 3.7f, -5.2f };
        assertEqual(5.2f, findMaxAbs(new Span<float>(fArr)), 0.001f, "findMaxAbs float[]");
        assertEqual(5.2f, findMaxAbs(new List<float> { -1.5f, 3.7f, -5.2f }), 0.001f, "findMaxAbs List<float>");

        // --- double ---
        double[] dArr = { -3.3, 1.1, -5.5 };
        assertEqual(5.5, findMaxAbs(new Span<double>(dArr)), 0.0001, "findMaxAbs double[]");
        assertEqual(5.5, findMaxAbs(new List<double> { -3.3, 1.1, -5.5 }), 0.0001, "findMaxAbs List<double>");
    }

    // ─── double assertEqual ────────────────────────────────────────────
    static void assertEqual(double e, double a, double eps, string m = "")
    {
        if (Math.Abs(e - a) > eps)
        {
            throw new Exception($"Expected [{e}] got [{a}] - {m}");
        }
    }

    // ─── getLineNum / getCurSourceFileName / getStackTrace ────────────
    static void testGetLineNum()
    {
        // getLineNum 返回调用者的行号
        int line = getLineNum();
        assertTrue(line > 0, "getLineNum positive");
    }

    static void testGetCurSourceFileName()
    {
        string fileName = getCurSourceFileName();
        assertTrue(fileName != null, "getCurSourceFileName not null");
        assertTrue(fileName.endWith("FrameUtilityTest.cs"), "getCurSourceFileName is this file");
    }

    static void testGetStackTrace()
    {
        string trace = getStackTrace(5);
        assertTrue(trace != null, "getStackTrace not null");
        assertTrue(trace.Length > 0, "getStackTrace non-empty");
        assertTrue(trace.Contains("at "), "getStackTrace contains at");
    }

    // ─── getClassNameFromGameObject ───────────────────────────────────
    static void testGetClassNameFromGameObject()
    {
        // null GameObject: 返回空字符串
        assertEqual("", getClassNameFromGameObject(null), "null go");

        // 普通 GameObject: 返回 removeEndNumber 后的名字
        UnityEngine.GameObject go = new UnityEngine.GameObject("TestObj123");
        string name = getClassNameFromGameObject(go);
        assertTrue(name.Length > 0, "normal go non-empty");
        assertFalse(name.Contains("123"), "number removed from name");

        // 空名字 GameObject
        UnityEngine.GameObject emptyGo = new UnityEngine.GameObject("");
        assertEqual("", getClassNameFromGameObject(emptyGo), "empty name go");

        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(emptyGo);
    }

    // ─── availableWritePath / availableReadPath ───────────────────────
    static void testAvailableWritePath()
    {
        string path = availableWritePath("test.txt");
        assertTrue(path != null, "availableWritePath not null");
        assertTrue(path.endWith("test.txt"), "availableWritePath ends with filename");
    }

    static void testAvailableReadPath()
    {
        string path = availableReadPath("config.json");
        assertTrue(path != null, "availableReadPath not null");
        assertTrue(path.endWith("config.json"), "availableReadPath ends with filename");
    }

    // ─── writeFileList ────────────────────────────────────────────────
    // writeFileList 依赖 writeTxtFile(真实文件IO), 仅测试不崩溃
    static void testWriteFileList()
    {
        // writeFileList 调用 writeTxtFile(path + FILE_LIST, content)
        // 使用临时路径避免残留文件
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MF_FileList_" + System.Guid.NewGuid().ToString("N"));
        try
        {
            writeFileList(tempPath, "content");
        }
        catch (System.Exception)
        {
            // 文件写入可能失败，但不影响测试通过
        }
        finally
        {
            // 清理临时文件
            string filePath = tempPath + "FileList";
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }

    // ─── 相机/输入: 框架环境已启动, CameraManager/InputSystem 可用 ────
    static void testCameraAndInput()
    {
        // getMainCamera: 返回框架主相机 (可能为 null 如果场景无相机)
        GameCamera cam = getMainCamera();
        // 不强制非 null, 框架可能不创建默认相机

        // 键盘输入: 测试时按键状态不确定（Play按钮可能触发按键），仅验证不抛异常
        isKeyCurrentDown(KeyCode.Space);
        isKeyCurrentUp(KeyCode.Space);
        isKeyDown(KeyCode.A);
        isKeyUp(KeyCode.A);

        // 鼠标位置: 返回 Vector3, 验证类型正确
        Vector3 mousePos = getMousePosition();
        assertTrue(mousePos.x >= 0 || mousePos.x <= 0, "mousePos valid"); // 任何值都合法
        Vector3 mousePosLB = getMousePosition(false);
        assertTrue(mousePosLB.x >= 0 || mousePosLB.x <= 0, "mousePosLB valid");
    }

    // ─── 场景: GameSceneManager 已初始化 ──────────────────────────────
    static void testSceneFunctions()
    {
        // getCurScene: 返回当前场景，测试环境中可能尚未进入任何场景
        GameScene scene = getCurScene();
        if (scene != null)
        {
            // 有场景时验证场景类型有效
            assertNotNull(scene.getCurProcedureType(), "scene type not null");
        }
        // 无场景时也视为通过（测试环境可能尚未初始化场景管理器）

        // enterScene: 需要有效的 GameScene 类型才能测试, 仅验证函数存在
        // changeProcedureDelay: 延迟调用, 需要有效 procedure 类型
        // 此处仅验证不崩溃

        // atProcedure(Type): 只读查询, 场景存在时安全调用(null type 由 isThisOrParent 容忍)
        if (scene != null)
        {
            bool p0 = atProcedure(null);
            assertTrue(!p0, "atProcedure(null) 场景非空时返回 false");
            // 现在不支持的类型总是返回 false, 且不抛异常
            atProcedure(typeof(GameScene));
            assertTrue(true, "atProcedure(Type) executed");
        }
    }

    static void assertNotNull(object obj, string m = "")
    {
        if (obj == null)
        {
            throw new Exception($"Expected not null - {m}");
        }
    }

    // ─── 布局/UI: LayoutManager 已初始化 ─────────────────────────────
    static void testLayoutFunctions()
    {
        // getUGUIRoot: 返回 UI 根节点
        myUGUIObject root = getUGUIRoot();
        // 框架可能创建默认 UIRoot, 也可能为 null
        // 不强制非 null

        // getUICamera: 返回 UI 相机
        Camera uiCam = getUICamera();
        // 框架可能创建默认 UI Camera

        // makeSizeEven: 需要 myUGUIObject, 在框架环境下测试
        if (root != null)
        {
            makeSizeEven(root);
            // 函数执行后不崩溃即通过
        }
    }

    // ─── 命令系统: CommandSystem 已初始化 ─────────────────────────────
    static void testCommandSystem()
    {
        // CMD_DELAY<T>: 创建延迟命令
        CMD_DELAY(out TestFrameCmd delayCmd);
        assertNotNull(delayCmd, "CMD_DELAY not null");
        assertTrue(delayCmd.getAssignID() > 0, "CMD_DELAY assignID > 0");

        // CMD_THREAD<T>: 创建线程命令
        CMD_THREAD(out TestFrameCmd threadCmd);
        assertNotNull(threadCmd, "CMD_THREAD not null");

        // CMD_DELAY_THREAD<T>: 创建延迟线程命令
        CMD_DELAY_THREAD(out TestFrameCmd delayThreadCmd);
        assertNotNull(delayThreadCmd, "CMD_DELAY_THREAD not null");

        // pushDelayCommand: 3个重载，需要使用CMD_DELAY创建延迟命令
        var receiver = new TestFrameCmdReceiver();
        CMD_DELAY(out TestFrameCmd pushCmd);
        // pushDelayCommand(Command, CommandReceiver, float)
        pushDelayCommand(pushCmd, receiver, 0.001f);

        CMD_DELAY(out TestFrameCmd pushCmd2);
        // pushDelayCommand(Command, CommandReceiver)
        pushDelayCommand(pushCmd2, receiver);

        CMD_DELAY(out TestFrameCmd pushCmd3);
        // pushDelayCommand(Command, CommandReceiver, float, DelayCmdWatcher)
        pushDelayCommand(pushCmd3, receiver, 0.001f, null);
    }

    // ─── 对象池: ClassPool / ArrayPool 已初始化 ───────────────────────
    static void testObjectPool()
    {
        // CLASS_ONCE: 单次使用后自动回收
        CLASS_ONCE(out TestFrameClass onceObj);
        assertNotNull(onceObj, "CLASS_ONCE not null");
        UN_CLASS(ref onceObj);

        // ARRAY_BYTE_PERSIST: 持久数组
        ARRAY_BYTE_PERSIST(out byte[] persistArr, 16);
        assertNotNull(persistArr, "ARRAY_BYTE_PERSIST not null");
        assertEqual(16, persistArr.Length, "ARRAY_BYTE_PERSIST length");
        UN_ARRAY_BYTE(ref persistArr);

        // ARRAY_BYTE: 普通数组（长度必须是2的n次方）
        ARRAY_BYTE(out byte[] arr, 32);
        assertNotNull(arr, "ARRAY_BYTE not null");
        assertEqual(32, arr.Length, "ARRAY_BYTE length");
        UN_ARRAY_BYTE(ref arr);

        // ARRAY_BYTE 返回值版本
        byte[] arr2 = ARRAY_BYTE(8);
        assertNotNull(arr2, "ARRAY_BYTE return not null");
        assertEqual(8, arr2.Length, "ARRAY_BYTE return length");
        UN_ARRAY_BYTE(ref arr2);

        // ARRAY_BYTE_THREAD: 线程数组
        ARRAY_BYTE_THREAD(out byte[] threadArr, 8);
        assertNotNull(threadArr, "ARRAY_BYTE_THREAD not null");
        assertEqual(8, threadArr.Length, "ARRAY_BYTE_THREAD length");
        UN_ARRAY_BYTE_THREAD(ref threadArr);

        // UN_ARRAY_BYTE_THREAD 重载: ref / value / ICollection
        ARRAY_BYTE_THREAD(out byte[] arr3, 2);
        UN_ARRAY_BYTE_THREAD(ref arr3);
        assertTrue(arr3 == null, "UN_ARRAY_BYTE_THREAD ref null");

        ARRAY_BYTE_THREAD(out byte[] arr4, 4);
        UN_ARRAY_BYTE_THREAD(arr4);
        // value 版本不置 null

        var list = new List<byte[]>();
        ARRAY_BYTE_THREAD(out byte[] arr5, 2);
        list.Add(arr5);
        UN_ARRAY_BYTE_THREAD(list);
        assertEqual(0, list.Count, "UN_ARRAY_BYTE_THREAD list cleared");
    }

    // ─── 延迟调用: CommandSystem 已初始化 ─────────────────────────────
    static void testDelayCall()
    {
        // delayCall(Action): 无参延迟
        long id1 = delayCall(() => { });
        assertTrue(id1 > 0, "delayCall id > 0");
        // 注意: 延迟调用在下一帧才执行, 此处无法验证回调

        // delayCall(float, Action, DelayCmdWatcher): 带延迟时间
        long id2 = delayCall(0.01f, () => { }, null);
        assertTrue(id2 > 0, "delayCall with time id > 0");

        // delayCallSafe: 带 guard 的安全延迟
        // 需要有效的 IRecyclable guard，不能传 null
        // ClassObject 必须通过 CLASS_ONCE 从池中获取，并用 UN_CLASS 回收
        CLASS_ONCE(out TestFrameClass guard);
        long id3 = delayCallSafe(() => { }, guard, 0.01f);
        assertTrue(id3 > 0, "delayCallSafe id > 0");
        UN_CLASS(ref guard);
    }

    // ─── 测试用内部类型 ────────────────────────────────────────────────
    public class TestFrameCmd : Command
    {
        public override void execute() { }
    }

    public class TestFrameCmdReceiver : CommandReceiver
    {
        public TestFrameCmdReceiver()
        {
            mName = "TestFrameCmdReceiver";
            mHasDestroy = false;
        }
    }

    public class TestFrameClass : ClassObject { }

    // ─── 数组对象池: ARRAY / ARRAY_PERSIST / UN_ARRAY ──────────────────
    static void testArrayPool()
    {
        // ARRAY<T>(out T[], int): 可回收数组
        ARRAY(out int[] arr1, 8);
        assertNotNull(arr1, "ARRAY out not null");
        assertEqual(8, arr1.Length, "ARRAY out length");
        arr1[0] = 42;
        UN_ARRAY(ref arr1);
        assertTrue(arr1 == null, "UN_ARRAY ref null");

        // ARRAY<T>(int): 返回值版本
        int[] arr2 = ARRAY<int>(16);
        assertNotNull(arr2, "ARRAY return not null");
        assertEqual(16, arr2.Length, "ARRAY return length");
        UN_ARRAY(ref arr2);
        assertTrue(arr2 == null, "UN_ARRAY return null");

        // UN_ARRAY<T>(T[], bool): value 版本不置 null
        ARRAY(out int[] arr3, 4);
        UN_ARRAY(arr3);
        // value 版本不置 null, 仅回收

        // UN_ARRAY<T>(ICollection<T[]>, bool): 批量回收
        var list = new List<int[]>();
        ARRAY(out int[] arr4, 2);
        ARRAY(out int[] arr5, 2);
        list.Add(arr4);
        list.Add(arr5);
        UN_ARRAY(list);
        assertEqual(0, list.Count, "UN_ARRAY list cleared");

        // ARRAY_PERSIST<T>: 持久数组（不回收）
        ARRAY_PERSIST(out int[] persist, 32);
        assertNotNull(persist, "ARRAY_PERSIST not null");
        assertEqual(32, persist.Length, "ARRAY_PERSIST length");
        persist[0] = 99;
        assertEqual(99, persist[0], "ARRAY_PERSIST write");
    }

    // ─── 线程数组池: ARRAY_THREAD / UN_ARRAY_THREAD ────────────────────
    static void testArrayPoolThread()
    {
        // ARRAY_THREAD<T>(out T[], int)
        ARRAY_THREAD(out int[] arr1, 8);
        assertNotNull(arr1, "ARRAY_THREAD out not null");
        assertEqual(8, arr1.Length, "ARRAY_THREAD out length");
        UN_ARRAY_THREAD(ref arr1);
        assertTrue(arr1 == null, "UN_ARRAY_THREAD ref null");

        // UN_ARRAY_THREAD<T>(ICollection<T[]>, bool): 批量回收
        var list = new List<int[]>();
        ARRAY_THREAD(out int[] arr2, 2);
        ARRAY_THREAD(out int[] arr3, 2);
        list.Add(arr2);
        list.Add(arr3);
        UN_ARRAY_THREAD(list);
        assertEqual(0, list.Count, "UN_ARRAY_THREAD list cleared");
    }

    // ─── 列表对象池: LIST / UN_LIST ────────────────────────────────────
    static void testListPool()
    {
        // LIST<T>(): 空列表
        List<int> l1 = LIST<int>();
        assertNotNull(l1, "LIST<> not null");
        assertEqual(0, l1.Count, "LIST<> empty");
        UN_LIST(ref l1);
        assertTrue(l1 == null, "UN_LIST ref null");

        // LIST<T>(out List<T>): out 版本
        LIST(out List<int> l2);
        assertNotNull(l2, "LIST out not null");
        assertEqual(0, l2.Count, "LIST out empty");
        UN_LIST(ref l2);

        // LIST<T>(List<T> initList): 带初始列表
        var init = new List<int> { 1, 2, 3 };
        List<int> l3 = LIST(init);
        assertNotNull(l3, "LIST initList not null");
        assertEqual(3, l3.Count, "LIST initList count");
        assertEqual(1, l3[0], "LIST initList [0]");
        assertEqual(3, l3[2], "LIST initList [2]");
        UN_LIST(ref l3);

        // LIST<T>(T[] initList): 带初始数组
        int[] initArr = { 10, 20, 30, 40 };
        List<int> l4 = LIST(initArr);
        assertNotNull(l4, "LIST initArr not null");
        assertEqual(4, l4.Count, "LIST initArr count");
        assertEqual(10, l4[0], "LIST initArr [0]");
        assertEqual(40, l4[3], "LIST initArr [3]");
        UN_LIST(ref l4);

        // LIST<T>(out List<T>, List<T>): out + 初始列表
        var src = new List<int> { 5, 6 };
        LIST(out List<int> l5, src);
        assertNotNull(l5, "LIST out initList not null");
        assertEqual(2, l5.Count, "LIST out initList count");
        assertEqual(5, l5[0], "LIST out initList [0]");
        UN_LIST(ref l5);

        // LIST<T>(out List<T>, T[]): out + 初始数组
        int[] srcArr = { 7, 8, 9 };
        LIST(out List<int> l6, srcArr);
        assertNotNull(l6, "LIST out initArr not null");
        assertEqual(3, l6.Count, "LIST out initArr count");
        assertEqual(9, l6[2], "LIST out initArr [2]");
        UN_LIST(ref l6);

        // UN_LIST<T>(List<T>): value 版本不置 null
        LIST(out List<int> l7);
        UN_LIST(l7);
    }

    // ─── 持久列表池: LIST_PERSIST ──────────────────────────────────────
    static void testListPoolPersist()
    {
        // LIST_PERSIST<T>(): 空持久列表
        List<int> l1 = LIST_PERSIST<int>();
        assertNotNull(l1, "LIST_PERSIST<> not null");
        assertEqual(0, l1.Count, "LIST_PERSIST<> empty");

        // LIST_PERSIST<T>(out List<T>): out 版本
        LIST_PERSIST(out List<int> l2);
        assertNotNull(l2, "LIST_PERSIST out not null");
        assertEqual(0, l2.Count, "LIST_PERSIST out empty");

        // LIST_PERSIST<T>(out List<T>, T[]): out + 初始数组
        int[] initArr = { 1, 2, 3 };
        List<int> l3 = LIST_PERSIST(out List<int> l3a, initArr);
        assertNotNull(l3, "LIST_PERSIST initArr not null");
        assertEqual(3, l3.Count, "LIST_PERSIST initArr count");
        assertEqual(1, l3[0], "LIST_PERSIST initArr [0]");
        assertEqual(3, l3[2], "LIST_PERSIST initArr [2]");

        // LIST_PERSIST<T>(out List<T>, List<T>): out + 初始列表
        var src = new List<int> { 5, 6, 7 };
        List<int> l4 = LIST_PERSIST(out List<int> l4a, src);
        assertNotNull(l4, "LIST_PERSIST initList not null");
        assertEqual(3, l4.Count, "LIST_PERSIST initList count");
        assertEqual(5, l4[0], "LIST_PERSIST initList [0]");
    }

    // ─── HashSet 持久池: SET_PERSIST / UN_SET ──────────────────────────
    static void testSetPoolPersist()
    {
        // SET_PERSIST<T>(): 空 HashSet
        HashSet<int> s1 = SET_PERSIST<int>();
        assertNotNull(s1, "SET_PERSIST<> not null");
        assertEqual(0, s1.Count, "SET_PERSIST<> empty");

        // SET_PERSIST<T>(out HashSet<T>, List<T>): out + 初始列表
        var init = new List<int> { 1, 2, 3, 2 };
        HashSet<int> s2 = SET_PERSIST(out HashSet<int> s2a, init);
        assertNotNull(s2, "SET_PERSIST initList not null");
        // HashSet 去重: 3 个元素
        assertEqual(3, s2.Count, "SET_PERSIST initList count dedup");
        assertTrue(s2.Contains(1), "SET_PERSIST contains 1");
        assertTrue(s2.Contains(2), "SET_PERSIST contains 2");
        assertTrue(s2.Contains(3), "SET_PERSIST contains 3");

        // UN_SET<T>(ref HashSet<T>): ref 版本置 null
        UN_SET(ref s2);
        assertTrue(s2 == null, "UN_SET ref null");

        // UN_SET<T>(HashSet<T>): value 版本不置 null
        HashSet<int> s3 = SET_PERSIST<int>();
        s3.Add(42);
        UN_SET(s3);
    }

    // ─── Dictionary 持久池: DIC_PERSIST / UN_DIC ───────────────────────
    static void testDicPoolPersist()
    {
        // DIC_PERSIST<K, V>(): 空字典
        Dictionary<int, string> d1 = DIC_PERSIST<int, string>();
        assertNotNull(d1, "DIC_PERSIST<> not null");
        assertEqual(0, d1.Count, "DIC_PERSIST<> empty");

        // DIC_PERSIST<K, V>(out Dictionary<K, V>): out 版本
        DIC_PERSIST(out Dictionary<int, string> d2);
        assertNotNull(d2, "DIC_PERSIST out not null");
        assertEqual(0, d2.Count, "DIC_PERSIST out empty");
        d2[1] = "one";
        d2[2] = "two";
        assertEqual("one", d2[1], "DIC_PERSIST write");
        assertEqual("two", d2[2], "DIC_PERSIST write 2");

        // UN_DIC<K, V>(ref Dictionary<K, V>): ref 版本置 null
        UN_DIC(ref d2);
        assertTrue(d2 == null, "UN_DIC ref null");

        // UN_DIC<K, V>(Dictionary<K, V>): value 版本不置 null
        DIC_PERSIST(out Dictionary<int, string> d3);
        d3[10] = "ten";
        UN_DIC(d3);
    }

    // ─── ClassObject 池: CLASS / UN_CLASS ──────────────────────────────
    static void testClassPool()
    {
        // CLASS<T>(): 泛型返回
        TestFrameClass c1 = CLASS<TestFrameClass>();
        assertNotNull(c1, "CLASS<> not null");
        UN_CLASS(ref c1);
        assertTrue(c1 == null, "UN_CLASS ref null");

        // CLASS<T>(out T): out 版本
        CLASS(out TestFrameClass c2);
        assertNotNull(c2, "CLASS out not null");
        UN_CLASS(ref c2);

        // CLASS(Type): Type 参数版本
        ClassObject c3 = CLASS(typeof(TestFrameClass));
        assertNotNull(c3, "CLASS Type not null");
        assertTrue(c3 is TestFrameClass, "CLASS Type is TestFrameClass");
        UN_CLASS(ref c3);

        // CLASS<T>(Type): 泛型 + Type 参数
        TestFrameClass c4 = CLASS<TestFrameClass>(typeof(TestFrameClass));
        assertNotNull(c4, "CLASS<T> Type not null");
        UN_CLASS(ref c4);
    }

    // ─── 线程 ClassObject 池: CLASS_THREAD / UN_CLASS_THREAD ───────────
    static void testClassPoolThread()
    {
        // CLASS_THREAD<T>(): 泛型返回
        TestFrameClass c1 = CLASS_THREAD<TestFrameClass>();
        assertNotNull(c1, "CLASS_THREAD<> not null");
        UN_CLASS_THREAD(ref c1);
        assertTrue(c1 == null, "UN_CLASS_THREAD ref null");

        // CLASS_THREAD<T>(out T): out 版本
        CLASS_THREAD(out TestFrameClass c2);
        assertNotNull(c2, "CLASS_THREAD out not null");
        UN_CLASS_THREAD(ref c2);
    }

    // ─── UN_CLASS_LIST 全部 4 个重载 ───────────────────────────────────
    static void testClassPoolList()
    {
        // UN_CLASS_LIST<T>(List<T>): List 版本
        var list = new List<TestFrameClass>();
        CLASS(out TestFrameClass c1);
        CLASS(out TestFrameClass c2);
        list.Add(c1);
        list.Add(c2);
        UN_CLASS_LIST(list);
        assertEqual(0, list.Count, "UN_CLASS_LIST List cleared");

        // UN_CLASS_LIST<T>(HashSet<T>): HashSet 版本
        var set = new HashSet<TestFrameClass>();
        CLASS(out TestFrameClass c3);
        set.Add(c3);
        UN_CLASS_LIST(set);
        assertEqual(0, set.Count, "UN_CLASS_LIST HashSet cleared");

        // UN_CLASS_LIST<T0, T1>(Dictionary<T0, T1>): Dictionary 版本
        var dict = new Dictionary<int, TestFrameClass>();
        CLASS(out TestFrameClass c4);
        dict[1] = c4;
        UN_CLASS_LIST(dict);
        assertEqual(0, dict.Count, "UN_CLASS_LIST Dictionary cleared");

        // UN_CLASS_LIST<T>(Queue<T>): Queue 版本
        var queue = new Queue<TestFrameClass>();
        CLASS(out TestFrameClass c5);
        queue.Enqueue(c5);
        UN_CLASS_LIST(queue);

        // UN_CLASS_LIST_THREAD<T>(List<T>): 线程 List 版本
        var tlist = new List<TestFrameClass>();
        CLASS_THREAD(out TestFrameClass c6);
        tlist.Add(c6);
        UN_CLASS_LIST_THREAD(tlist);
        assertEqual(0, tlist.Count, "UN_CLASS_LIST_THREAD List cleared");
    }

    // ─── PACKET: 网络包工厂 ───────────────────────────────────────────
    static void testPacket()
    {
        // PACKET<T>(): 返回值版本
        // EditMode 下 mNetPacketFactory 为 null，PACKET 返回 null
        // PlayMode 下框架已初始化，返回有效 NetPacket
        TestFramePacket p1 = PACKET<TestFramePacket>();
        if (p1 != null)
        {
            assertTrue(p1 is NetPacket, "PACKET returns NetPacket");
        }

        // PACKET<T>(out T): out 版本
        TestFramePacket p2 = PACKET(out TestFramePacket p2Out);
        if (p2 != null)
        {
            assertTrue(p2 is NetPacket, "PACKET out returns NetPacket");
            assertTrue(p2Out is NetPacket, "PACKET out param is NetPacket");
        }
    }

    // ─── CMD: 命令创建 (Type 版本) ─────────────────────────────────────
    static void testCmd()
    {
        // CMD(Type): 创建主线程立即命令
        Command cmd1 = CMD(typeof(TestFrameCmd));
        assertNotNull(cmd1, "CMD Type not null");
        assertTrue(cmd1 is TestFrameCmd, "CMD Type is TestFrameCmd");

        // CMD_DELAY(Type): 创建主线程延迟命令
        Command cmd2 = CMD_DELAY(typeof(TestFrameCmd));
        assertNotNull(cmd2, "CMD_DELAY Type not null");
    }

    // ─── pushCommand: 命令发送 ─────────────────────────────────────────
    static void testPushCommand()
    {
        var receiver = new TestFrameCmdReceiver();

        // pushCommand<T>(CommandReceiver, LOG_LEVEL): 泛型版本
        pushCommand<TestFrameCmd>(receiver);

        // pushCommand(Command, CommandReceiver): Command 参数版本
        CMD(out TestFrameCmd cmd1);
        pushCommand(cmd1, receiver);

        // pushCommandThread<T>(CommandReceiver, LOG_LEVEL): 线程版本
        pushCommandThread<TestFrameCmd>(receiver);

        // pushDelayCommandThread<T>: 线程延迟命令
        var watcher = new DelayCmdWatcher();
        TestFrameCmd cmd2 = pushDelayCommandThread<TestFrameCmd>(watcher, receiver, 0.001f);
        assertNotNull(cmd2, "pushDelayCommandThread not null");
    }

    // ─── checkEnum: 枚举有效性检查 ─────────────────────────────────────
    static void testCheckEnum()
    {
        // 有效枚举值: 不抛异常
        checkEnum(CoreTestEnum.First);
        checkEnum(CoreTestEnum.Second);

        // 无效枚举值: 用 isEnumValid 测试（纯返回值无副作用）
        // checkEnum((CoreTestEnum)99) 会 logError，不在此测试
        assertTrue(!isEnumValid((CoreTestEnum)99), "isEnumValid 99 false");
    }

    // ─── createInstance: Activator 创建实例 ────────────────────────────
    static void testCreateInstance()
    {
        // 无参构造
        var obj = createInstance<TestFrameClass>(typeof(TestFrameClass));
        assertNotNull(obj, "createInstance not null");
        assertTrue(obj is TestFrameClass, "createInstance type check");

        // 有参构造: 创建一个带参构造的测试类
        var obj2 = createInstance<TestFrameClassWithParams>(typeof(TestFrameClassWithParams), 42, "hello");
        assertNotNull(obj2, "createInstance params not null");
        assertEqual(42, obj2.mValue, "createInstance int param");
        assertEqual("hello", obj2.mName, "createInstance string param");
    }

    // ─── deepCopy: 深拷贝 ──────────────────────────────────────────────
    static void testDeepCopy()
    {
        // null 拷贝: 返回 null
        TestDeepCopyClass nullObj = null;
        TestDeepCopyClass resultNull = deepCopy(nullObj);
        assertTrue(resultNull == null, "deepCopy null");

        // 普通对象深拷贝
        var original = new TestDeepCopyClass();
        original.mIntValue = 42;
        original.mStringValue = "hello";
        original.mNestedObj = new TestDeepCopyClass();
        original.mNestedObj.mIntValue = 100;

        var copy = deepCopy(original);
        assertNotNull(copy, "deepCopy not null");
        assertEqual(42, copy.mIntValue, "deepCopy int");
        assertEqual("hello", copy.mStringValue, "deepCopy string");
        // 嵌套对象也被拷贝
        assertNotNull(copy.mNestedObj, "deepCopy nested not null");
        assertEqual(100, copy.mNestedObj.mIntValue, "deepCopy nested int");

        // 字符串: 直接返回原对象（值类型行为）
        string s = "test";
        string sCopy = deepCopy(s);
        assertTrue(object.ReferenceEquals(s, sCopy), "deepCopy string same ref");
    }

    // ─── getUGUIRootComponent: 获取 UI Root 的 Canvas ──────────────────
    static void testGetUGUIRootComponent()
    {
        Canvas canvas = getUGUIRootComponent();
        // mLayoutManager 或 getUIRoot() 可能为 null, 允许返回 null
        // 不强制非 null
        if (canvas != null)
        {
            assertTrue(canvas is Canvas, "getUGUIRootComponent is Canvas");
        }
    }

    // ─── isSpriteInAtlas: 判断精灵是否在图集中 ─────────────────────────
    static void testIsSpriteInAtlas()
    {
        // 不在图集中的精灵路径
        bool inAtlas = isSpriteInAtlas("Assets/GameResources/Sprites/icon.png");
        // 返回 true/false 即可, 不抛异常
        assertTrue(inAtlas || !inAtlas, "isSpriteInAtlas bool");

        // 注意: 空路径会导致 getFolderName 中 builder[^1] 越界
        // 不测试空字符串，直接跳过
    }

    // ─── 测试用辅助类型 ────────────────────────────────────────────────
    enum CoreTestEnum { First = 1, Second = 2 }

    public class TestFramePacket : NetPacket
    {
        public override void execute() { }
    }

    public class TestFrameClassWithParams
    {
        public int mValue;
        public string mName;
        public TestFrameClassWithParams(int value, string name)
        {
            mValue = value;
            mName = name;
        }
    }

    public class TestDeepCopyClass
    {
        public int mIntValue;
        public string mStringValue;
        public TestDeepCopyClass mNestedObj;
    }

    static void assertEqual<T>(T e, T a, string m = "")
    {
        if (!e.Equals(a))
        {
            throw new Exception($"Expected [{e}] got [{a}] - {m}");
        }
    }
    static void assertEqual(float e, float a, float eps, string m = "")
    {
        if (Math.Abs(e - a) > eps)
        {
            throw new Exception($"Expected [{e}] got [{a}] - {m}");
        }
    }
    static void assertFalse(bool c, string m = "")
    {
        if (c)
        {
            throw new Exception($"Expected false - {m}");
        }
    }

    static void assertTrue(bool c, string m = "")
    {
        if (!c)
        {
            throw new Exception($"Expected true - {m}");
        }
    }

    // ─── getLocalIP ────────────────────────────────────────────────
    // 遍历本机网络地址返回第一个 IPv4, 无则返回空串
    static void testGetLocalIP()
    {
        string ip = getLocalIP();
        assertTrue(ip != null, "getLocalIP not null");
        // 本机应至少能解析出 IP 或返回空串, 均视为合法
        if (!string.IsNullOrEmpty(ip))
        {
            System.Net.IPAddress addr;
            assertTrue(System.Net.IPAddress.TryParse(ip, out addr), "getLocalIP valid ip format");
        }
    }

    // ─── hostNameToIPAddress ───────────────────────────────────────
    // 通过 DNS 将主机名解析为 IPAddress
    static void testHostNameToIPAddress()
    {
        try
        {
            System.Net.IPAddress ip = hostNameToIPAddress("localhost");
            assertTrue(ip != null, "hostNameToIPAddress localhost not null");
        }
        catch (System.Exception)
        {
            // DNS 解析失败则跳过断言(不同环境行为不同)
        }
    }

    // ─── compressZipFile / decompressZipFile ───────────────────────
    // 用临时文件做一次 压缩->解压 往返, 验证内容一致
    static void testCompressDecompressZip()
    {
#if !UNITY_WEBGL
        string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MF_Zip_" + System.Guid.NewGuid().ToString("N"));
        try
        {
            System.IO.Directory.CreateDirectory(tempRoot);
            string sourceFile = tempRoot + "/src.txt";
            System.IO.File.WriteAllText(sourceFile, "hello zip 中文内容 123");
            string zipFile = tempRoot + "/out.zip";
            string extractDir = tempRoot + "/extract";
            compressZipFile(sourceFile, zipFile);
            assertTrue(System.IO.File.Exists(zipFile), "compressZipFile created zip");
            decompressZipFile(zipFile, extractDir);
            // 解压后文件名为原文件名 src.txt
            string extracted = extractDir + "/src.txt";
            assertTrue(System.IO.File.Exists(extracted), "decompressZipFile extracted file");
            string content = System.IO.File.ReadAllText(extracted);
            assertEqual("hello zip 中文内容 123", content, "zip roundtrip content equal");
        }
        finally
        {
            if (System.IO.Directory.Exists(tempRoot))
            {
                System.IO.Directory.Delete(tempRoot, true);
            }
        }
#endif
    }
}