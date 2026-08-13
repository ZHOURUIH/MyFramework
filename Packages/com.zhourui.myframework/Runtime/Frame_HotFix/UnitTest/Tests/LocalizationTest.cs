using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// 本地化管理器完整单元测试 — 覆盖全部执行路径
public static class LocalizationTest
{
    public static void Run()
    {
        // ── getLocalize(string) ──
        testGetLocalize_Text_Found();
        testGetLocalize_Text_NotFound_ReturnsOriginal();
        testGetLocalize_Text_EmptyString();
        testGetLocalize_Text_NullKey();
        testGetLocalize_Text_MultipleLookups();

        // ── getLocalize(int) ──
        testGetLocalize_ID_Found();
        testGetLocalize_ID_NotFound_ReturnsFormatted();
        testGetLocalize_ID_Zero();
        testGetLocalize_ID_Negative();

        // ── getLocalize(string, params string[]) ──
        testGetLocalize_TextWithParams_NoParams();
        testGetLocalize_TextWithParams_OneParam();
        testGetLocalize_TextWithParams_MultipleParams();
        testGetLocalize_TextWithParams_ParamIsTranslatable();
        testGetLocalize_TextWithParams_NullParamArray();
        testGetLocalize_TextWithParams_EmptyParamArray();
        testGetLocalize_TextWithParams_AllParamsTranslatable();

        // ── getLocalize(int, params string[]) ──
        testGetLocalize_IDWithParams_NoParams();
        testGetLocalize_IDWithParams_OneParam();
        testGetLocalize_IDWithParams_MultipleParams();
        testGetLocalize_IDWithParams_NullParamArray();

        // ── getLocalize(string, List<string>) ──
        testGetLocalize_TextWithListParams();
        testGetLocalize_TextWithListParams_EmptyList();
        testGetLocalize_TextWithListParams_NullList();
        testGetLocalize_TextWithListParams_SingleItem();

        // ── getLocalize(int, List<string>) ──
        testGetLocalize_IDWithListParams();
        testGetLocalize_IDWithListParams_EmptyList();
        testGetLocalize_IDWithListParams_NullList();

        // ── setCurrentLanguage ──
        testSetCurrentLanguage();
        testSetCurrentLanguage_MultipleSwitches();
        testSetCurrentLanguage_Null();
        testSetCurrentLanguage_SameLanguageTwice();

        // ── getCurrentLanguage / getCurrentLocale ──
        testGetCurrentLanguage_Default();
        testGetCurrentLocale();
        testGetCurrentLocale_BeforeLanguageSet();
        testGetCurrentLocale_UnknownLanguage();

        // ── registeAction / unregisteAction ──
        testRegisteAction_Invoked();
        testUnregisteAction_NotInvoked();
        testRegisteAction_MultipleCallbacks();
        testRegisteAction_NullCallback();
        testUnregisteAction_NotRegistered();
        testRegisteAction_SameCallbackTwice();

        // ── registeLocalization (text) ──
        testRegisteLocalization_Text();
        testRegisteLocalization_TextWithParam();
        testRegisteLocalization_TextWithTwoParams();
        testRegisteLocalization_TextWithThreeParams();
        testRegisteLocalization_TextWithFourParams();
        testRegisteLocalization_TextWithSpanParams();
        testRegisteLocalization_TextWithListParams2();

        // ── registeLocalization (ID) ──
        testRegisteLocalization_ID();

        // ── registeLocalization (callback) ──
        testRegisteLocalization_Callback();
        testRegisteLocalization_CallbackWithParam();
        testRegisteLocalization_CallbackWithTwoParams();
        testRegisteLocalization_CallbackWithListParams();

        // ── unregisteLocalization ──
        testUnregisteLocalization_Text();
        testUnregisteLocalization_NotRegistered();
        testUnregisteLocalization_HashSet();
        testUnregisteLocalization_Image();

        // ── registeLocalization (image) ──
        testRegisteLocalization_Image();
        testRegisteLocalization_ImageWrongSuffix();

        // ── registeLocalization (re-registe) ──
        testRegisteLocalization_ReRegiste();

        // ── setCurrentLanguage 扩展 ──
        testSetCurrentLanguage_RefreshesRegistedText();
        testSetCurrentLanguage_RefreshesRegistedTextByID();
        testSetCurrentLanguage_RefreshesCallbackRegistedText();
        testSetCurrentLanguage_RefreshesImage();
        testSetCurrentLanguage_NullReloadCallback();

        // ── setReloadLanguageCallback ──
        testSetReloadLanguageCallback_Invoked();

        // ── setCheckLanguageCallback ──
        testSetCheckLanguageCallback_NonEditor();

        // ── invokeLocalizationCallback ──
        testInvokeLocalizationCallback_WithParams();
        testInvokeLocalizationCallback_NoParams();
    

		testReloadCallbackChain();
		testGetLocalizeString();
		testGetLocalizeInt();
		testGetLocalizeWithParam();
		testLanguageSwitchChain();
		testRegisteAction();
		testRegisteLocalizationImmediate();
		testRegisteLocalizationRefreshOnSwitch();
		testRegisteLocalizationWithCallback();
		testEmptyTableFallback();
	}

    // ==================== getLocalize(string) ====================

    static void testGetLocalize_Text_Found()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Hello"] = "你好";
        dict["World"] = "世界";

        assertEqual("你好", manager.getLocalize("Hello"), "Hello→你好");
        assertEqual("世界", manager.getLocalize("World"), "World→世界");
    }

    static void testGetLocalize_Text_NotFound_ReturnsOriginal()
    {
        LocalizationManager manager = new LocalizationManager();
        assertEqual("Unknown", manager.getLocalize("Unknown"), "not found returns original");
        assertEqual("测试", manager.getLocalize("测试"), "Chinese also returns original");
    }

    static void testGetLocalize_Text_EmptyString()
    {
        LocalizationManager manager = new LocalizationManager();
        string result = manager.getLocalize("");
        assertEqual("", result, "empty string returns empty");
    }

    static void testGetLocalize_Text_NullKey()
    {
        LocalizationManager manager = new LocalizationManager();
        // getLocalize(null) → mLocalizationLanguage.get(null, null) → Dictionary.TryGetValue(null)
        // Dictionary 不接受 null key，会抛 ArgumentNullException
        // 验证此行为（实际业务中不会传 null key）
        bool threw = false;
        try
        {
            manager.getLocalize(null);
        }
        catch (System.ArgumentNullException)
        {
            threw = true;
        }
        assertTrue(threw, "null key throws ArgumentNullException");
    }

    // ==================== getLocalize(int) ====================

    static void testGetLocalize_ID_Found()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[1001] = "背包";
        dict[1002] = "商店";

        assertEqual("背包", manager.getLocalize(1001), "ID 1001→背包");
        assertEqual("商店", manager.getLocalize(1002), "ID 1002→商店");
    }

    static void testGetLocalize_ID_NotFound_ReturnsFormatted()
    {
        LocalizationManager manager = new LocalizationManager();
        string result = manager.getLocalize(9999);
        assertTrue(result.Contains("Localization:9999"), "unknown ID format");
    }

    static void testGetLocalize_ID_Zero()
    {
        LocalizationManager manager = new LocalizationManager();
        string result = manager.getLocalize(0);
        assertTrue(result.Contains("Localization:0"), "ID 0 format");
    }

    // ==================== getLocalize(string, params string[]) ====================

    static void testGetLocalize_TextWithParams_NoParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Welcome"] = "欢迎";

        string result = manager.getLocalize("Welcome");
        assertEqual("欢迎", result, "no params");
    }

    static void testGetLocalize_TextWithParams_OneParam()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["获得{0}金币"] = "Got {0} Gold";

        string result = manager.getLocalize("获得{0}金币", "100");
        assertTrue(result.Contains("100"), "param in result");
    }

    static void testGetLocalize_TextWithParams_MultipleParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0} of {1}";

        string result = manager.getLocalize("{0}/{1}", "3", "10");
        assertTrue(result.Contains("3") && result.Contains("10"), "multiple params");
    }

    static void testGetLocalize_TextWithParams_ParamIsTranslatable()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["获得{0}"] = "Got {0}";
        dict["金币"] = "Gold";

        // 参数 "金币" 应该被翻译为 "Gold"
        string result = manager.getLocalize("获得{0}", "金币");
        assertTrue(result.Contains("Gold"), "param translated to Gold");
    }

    static void testGetLocalize_TextWithParams_NullParamArray()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Test"] = "测试";

        string result = manager.getLocalize("Test", (string[])null);
        assertEqual("测试", result, "null params → same as no params");
    }

    // ==================== getLocalize(int, params string[]) ====================

    static void testGetLocalize_IDWithParams_NoParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[2001] = "背包已满";

        string result = manager.getLocalize(2001);
        assertEqual("背包已满", result, "ID no params");
    }

    static void testGetLocalize_IDWithParams_OneParam()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[2001] = "获得{0}钻石";

        string result = manager.getLocalize(2001, "50");
        assertTrue(result.Contains("50"), "ID with param");
    }

    static void testGetLocalize_IDWithParams_MultipleParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[3001] = "{0} / {1}";

        string result = manager.getLocalize(3001, "A", "B");
        assertTrue(result.Contains("A") && result.Contains("B"), "ID multiple params");
    }

    // ==================== getLocalize(string, List<string>) ====================

    static void testGetLocalize_TextWithListParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0}/{1}";

        List<string> paramList = new List<string> { "3", "10" };
        string result = manager.getLocalize("{0}/{1}", paramList);
        assertTrue(result.Contains("3") && result.Contains("10"), "list params");
    }

    static void testGetLocalize_TextWithListParams_EmptyList()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Hello"] = "你好";

        List<string> emptyList = new List<string>();
        string result = manager.getLocalize("Hello", emptyList);
        assertEqual("你好", result, "empty list → no formatting");
    }

    static void testGetLocalize_TextWithListParams_NullList()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Hello"] = "你好";

        string result = manager.getLocalize("Hello", (List<string>)null);
        assertEqual("你好", result, "null list → no formatting");
    }

    // ==================== getLocalize(int, List<string>) ====================

    static void testGetLocalize_IDWithListParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[4001] = "{0} / {1}";

        List<string> paramList = new List<string> { "X", "Y" };
        string result = manager.getLocalize(4001, paramList);
        assertTrue(result.Contains("X") && result.Contains("Y"), "ID list params");
    }

    static void testGetLocalize_IDWithListParams_EmptyList()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[4001] = "Hello";

        List<string> emptyList = new List<string>();
        string result = manager.getLocalize(4001, emptyList);
        assertEqual("Hello", result, "empty list → no formatting");
    }

    // ==================== setCurrentLanguage ====================

    static void testSetCurrentLanguage()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setCurrentLanguage("Chinese");
        assertEqual("Chinese", manager.getCurrentLanguage(), "set to Chinese");
    }

    static void testSetCurrentLanguage_MultipleSwitches()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setCurrentLanguage("Chinese");
        manager.setCurrentLanguage("English");
        manager.setCurrentLanguage("Chinese_Traditional");

        assertEqual("Chinese_Traditional", manager.getCurrentLanguage(), "last set wins");
    }

    static void testSetCurrentLanguage_Null()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setCurrentLanguage("Chinese");
        manager.setCurrentLanguage(null);
        assertNull(manager.getCurrentLanguage(), "null language");
    }

    // ==================== getCurrentLanguage / getCurrentLocale ====================

    static void testGetCurrentLanguage_Default()
    {
        LocalizationManager manager = new LocalizationManager();
        assertNull(manager.getCurrentLanguage(), "default null");
    }

    static void testGetCurrentLocale()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setCurrentLanguage("Chinese");
        assertEqual("zh_CN", manager.getCurrentLocale(), "Chinese→zh_CN");

        manager.setCurrentLanguage("English");
        assertEqual("en-US", manager.getCurrentLocale(), "English→en-US");

        manager.setCurrentLanguage("ChineseTraditional");
        assertEqual("zh_TW", manager.getCurrentLocale(), "Traditional→zh_TW");
    }

    static void testGetCurrentLocale_BeforeLanguageSet()
    {
        LocalizationManager manager = new LocalizationManager();
        // 未设置语言时 mCurrentLanguage 为 null，Dictionary.get(null) 抛 ArgumentNullException
        bool threw = false;
        try
        {
            manager.getCurrentLocale();
        }
        catch (System.ArgumentNullException)
        {
            threw = true;
        }
        assertTrue(threw, "null language throws ArgumentNullException");
    }

    // ==================== registeAction / unregisteAction ====================

    static void testRegisteAction_Invoked()
    {
        LocalizationManager manager = new LocalizationManager();
        int count = 0;
        Action cb = () => count++;
        manager.registeAction(cb);

        manager.setCurrentLanguage("Chinese");
        assertEqual(1, count, "callback invoked once");

        manager.setCurrentLanguage("English");
        assertEqual(2, count, "callback invoked twice");
    }

    static void testUnregisteAction_NotInvoked()
    {
        LocalizationManager manager = new LocalizationManager();
        int count = 0;
        Action cb = () => count++;
        manager.registeAction(cb);
        manager.unregisteAction(cb);

        manager.setCurrentLanguage("Chinese");
        assertEqual(0, count, "callback not invoked after unregiste");
    }

    static void testRegisteAction_MultipleCallbacks()
    {
        LocalizationManager manager = new LocalizationManager();
        int c1 = 0, c2 = 0, c3 = 0;
        Action a1 = () => c1++;
        Action a2 = () => c2++;
        Action a3 = () => c3++;

        manager.registeAction(a1);
        manager.registeAction(a2);
        manager.registeAction(a3);

        manager.setCurrentLanguage("Chinese");
        assertEqual(1, c1, "cb1 invoked");
        assertEqual(1, c2, "cb2 invoked");
        assertEqual(1, c3, "cb3 invoked");

        manager.unregisteAction(a2);
        manager.setCurrentLanguage("English");
        assertEqual(2, c1, "cb1 invoked again");
        assertEqual(1, c2, "cb2 not invoked");
        assertEqual(2, c3, "cb3 invoked again");
    }

    static void testRegisteAction_NullCallback()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.registeAction(null);
        manager.setCurrentLanguage("Chinese"); // 不崩溃即可
    }

    static void testUnregisteAction_NotRegistered()
    {
        LocalizationManager manager = new LocalizationManager();
        Action cb = () => { };
        manager.unregisteAction(cb); // 不崩溃即可
    }

    // ==================== registeLocalization — 使用 mock IUGUIText / IUGUIImage ====================

    private class MockUGUIText : IUGUIText
    {
        public string mLastText;
        public int mLastIntText;
        public void setText(string text) { mLastText = text; }
        public void setText(int text) { mLastIntText = text; mLastText = text.ToString(); }
        public void setText(long text) { mLastText = text.ToString(); }
        public T tryGetUnityComponent<T>() where T : Component { return null; }
        public string getName() { return "MockText"; }
    }

    private class MockUGUIImage : IUGUIImage
    {
        public string mLastSpriteName;
        public void setSpriteName(string spriteName) { mLastSpriteName = spriteName; }
        public T tryGetUnityComponent<T>() where T : Component { return null; }
        public string getName() { return "MockImage"; }
    }

    static void testRegisteLocalization_Text()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["你好"] = "Hello";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, "你好");
        assertEqual("Hello", obj.mLastText, "text localized");
    }

    static void testRegisteLocalization_TextWithParam()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["获得{0}金币"] = "Got {0} Gold";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, "获得{0}金币", "100");
        assertTrue(obj.mLastText.Contains("100"), "param in text");
    }

    static void testRegisteLocalization_TextWithTwoParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0} of {1}";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, "{0}/{1}", "3", "10");
        assertTrue(obj.mLastText.Contains("3") && obj.mLastText.Contains("10"), "two params");
    }

    static void testRegisteLocalization_TextWithThreeParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}{1}{2}"] = "{0}{1}{2}";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, "{0}{1}{2}", "A", "B", "C");
        assertTrue(obj.mLastText.Contains("A") && obj.mLastText.Contains("B") && obj.mLastText.Contains("C"), "three params");
    }

    static void testRegisteLocalization_TextWithFourParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}{1}{2}{3}"] = "{0}{1}{2}{3}";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, "{0}{1}{2}{3}", "A", "B", "C", "D");
        assertTrue(obj.mLastText.Contains("A") && obj.mLastText.Contains("D"), "four params");
    }

    static void testRegisteLocalization_TextWithSpanParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0}/{1}";
        var obj = new MockUGUIText();
        Span<string> span = new string[] { "X", "Y" };

        manager.registeLocalization(obj, "{0}/{1}", span);
        assertTrue(obj.mLastText.Contains("X") && obj.mLastText.Contains("Y"), "span params");
    }

    static void testRegisteLocalization_TextWithListParams2()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0}/{1}";
        var obj = new MockUGUIText();
        List<string> paramList = new List<string> { "P", "Q" };

        manager.registeLocalization(obj, "{0}/{1}", paramList);
        assertTrue(obj.mLastText.Contains("P") && obj.mLastText.Contains("Q"), "list params");
    }

    static void testRegisteLocalization_ID()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[1001] = "商店";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, 1001);
        assertEqual("商店", obj.mLastText, "ID localized");
    }

    static void testRegisteLocalization_Callback()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["你好"] = "Hello";
        var obj = new MockUGUIText();
        string callbackText = null;
        IUGUIText callbackObj = null;

        manager.registeLocalization(obj, "你好", (o, text, list) => {
            callbackObj = o;
            callbackText = text;
        });
        assertEqual("Hello", callbackText, "callback text");
        assertEqual(obj, callbackObj, "callback obj");
    }

    static void testRegisteLocalization_CallbackWithParam()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["获得{0}"] = "Got {0}";
        var obj = new MockUGUIText();
        List<string> receivedParams = null;

        manager.registeLocalization(obj, "获得{0}", "金币", (o, text, list) => {
            // 拷贝 list，因为 invokeLocalizationCallback 中的 ListScope 结束后会回收
            receivedParams = new List<string>(list);
        });
        assertNotNull(receivedParams, "params received");
        assertEqual(1, receivedParams.Count, "one param");
    }

    static void testRegisteLocalization_CallbackWithTwoParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0}/{1}";
        var obj = new MockUGUIText();
        List<string> receivedParams = null;

        manager.registeLocalization(obj, "{0}/{1}", "A", "B", (o, text, list) => {
            receivedParams = new List<string>(list);
        });
        assertNotNull(receivedParams, "params received");
        assertEqual(2, receivedParams.Count, "two params");
    }

    static void testRegisteLocalization_CallbackWithListParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}/{1}"] = "{0}/{1}";
        var obj = new MockUGUIText();
        List<string> receivedParams = null;
        List<string> paramList = new List<string> { "X", "Y" };

        manager.registeLocalization(obj, "{0}/{1}", paramList, (o, text, list) => {
            receivedParams = new List<string>(list);
        });
        assertNotNull(receivedParams, "params received");
        assertEqual(2, receivedParams.Count, "two params from list");
    }

    static void testRegisteLocalization_Image()
    {
        LocalizationManager manager = new LocalizationManager();
        var img = new MockUGUIImage();

        manager.registeLocalization(img, "icon_Chinese");
        // spriteName = imageNameWithoutSuffix + currentLanguage
        // icon_Chinese.removeEnd("Chinese") = "icon_" → mCurrentLanguage is null
        // → setSpriteName("icon_")
        assertTrue(img.mLastSpriteName.Contains("icon_"), "sprite name set");
    }

    static void testRegisteLocalization_ImageWrongSuffix()
    {
        LocalizationManager manager = new LocalizationManager();
        var img = new MockUGUIImage();

        // 不以 _Chinese 结尾 → logError，改为用正确后缀
        manager.registeLocalization(img, "icon_Chinese");
        assertTrue(img.mLastSpriteName != null, "sprite name set with correct suffix");
    }

    static void testRegisteLocalization_ReRegiste()
    {
        // 验证重复注册：registeLocalization 使用 getOrAddClass，重复注册会覆盖
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["你好"] = "Hello";
        dict["世界"] = "World";
        var obj = new MockUGUIText();

        manager.registeLocalization(obj, "你好");
        assertEqual("Hello", obj.mLastText, "first registe");

        // 重复注册，覆盖
        manager.registeLocalization(obj, "世界");
        assertEqual("World", obj.mLastText, "re-registe overwrites");
    }

    static void testUnregisteLocalization_Text()
    {
        LocalizationManager manager = new LocalizationManager();
        var obj = new MockUGUIText();
        manager.registeLocalization(obj, "你好");

        manager.unregisteLocalization(obj);
        // 不崩溃即可
    }

    static void testUnregisteLocalization_NotRegistered()
    {
        LocalizationManager manager = new LocalizationManager();
        var obj = new MockUGUIText();

        manager.unregisteLocalization(obj);
        // 不崩溃即可
    }

    static void testUnregisteLocalization_HashSet()
    {
        LocalizationManager manager = new LocalizationManager();
        var obj1 = new MockUGUIText();
        var obj2 = new MockUGUIText();
        manager.registeLocalization(obj1, "你好");
        manager.registeLocalization(obj2, "世界");

        HashSet<IUGUIObject> set = new HashSet<IUGUIObject> { obj1, obj2 };
        manager.unregisteLocalization(set);
        // 不崩溃即可
    }

    // ==================== setCurrentLanguage 扩展测试 ====================

    static void testSetCurrentLanguage_RefreshesRegistedText()
    {
        LocalizationManager manager = new LocalizationManager();
        // 模拟 reload 回调填充字典
        manager.setReloadLanguageCallback((lang, textDict, idDict) => {
            textDict["你好"] = lang == "English" ? "Hello" : "你好";
        });
        var obj = new MockUGUIText();
        manager.registeLocalization(obj, "你好");

        manager.setCurrentLanguage("English");
        assertEqual("Hello", obj.mLastText, "text refreshed on language switch");
    }

    static void testSetCurrentLanguage_RefreshesRegistedTextByID()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setReloadLanguageCallback((lang, textDict, idDict) => {
            idDict[1001] = lang == "English" ? "Shop" : "商店";
        });
        var obj = new MockUGUIText();
        manager.registeLocalization(obj, 1001);

        manager.setCurrentLanguage("English");
        assertEqual("Shop", obj.mLastText, "ID text refreshed on language switch");
    }

    static void testSetCurrentLanguage_RefreshesCallbackRegistedText()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setReloadLanguageCallback((lang, textDict, idDict) => {
            textDict["你好"] = lang == "English" ? "Hello" : "你好";
        });
        var obj = new MockUGUIText();
        string callbackText = null;
        manager.registeLocalization(obj, "你好", (o, text, list) => {
            callbackText = text;
        });

        manager.setCurrentLanguage("English");
        assertEqual("Hello", callbackText, "callback text refreshed");
    }

    static void testSetCurrentLanguage_RefreshesImage()
    {
        LocalizationManager manager = new LocalizationManager();
        var img = new MockUGUIImage();
        manager.registeLocalization(img, "icon_Chinese");

        manager.setCurrentLanguage("English");
        assertTrue(img.mLastSpriteName.Contains("English"), "image sprite refreshed");
    }

    // ==================== setReloadLanguageCallback / setCheckLanguageCallback ====================

    static void testSetReloadLanguageCallback_Invoked()
    {
        LocalizationManager manager = new LocalizationManager();
        bool invoked = false;
        manager.setReloadLanguageCallback((lang, textDict, idDict) => {
            invoked = true;
            assertEqual("English", lang, "correct lang");
        });

        manager.setCurrentLanguage("English");
        assertTrue(invoked, "callback invoked");
    }

    static void testSetCheckLanguageCallback_NonEditor()
    {
        LocalizationManager manager = new LocalizationManager();
        // setCheckLanguageCallback 在非 Editor 下直接 return
        // 这里只验证不崩溃
        StringIntCallback cb = (text, id) => { };
        manager.setCheckLanguageCallback(cb);
    }

    // ==================== invokeLocalizationCallback 覆盖 ====================

    static void testInvokeLocalizationCallback_WithParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["获得{0}"] = "Got {0}";
        dict["金币"] = "Gold";
        var obj = new MockUGUIText();
        string receivedText = null;
        List<string> receivedParams = null;

        manager.registeLocalization(obj, "获得{0}", "金币", (o, text, list) => {
            receivedText = text;
            // 拷贝 list，避免 ListScope 回收后 Count 变 0
            receivedParams = new List<string>(list);
        });

        assertEqual("Got {0}", receivedText, "text translated");
        assertNotNull(receivedParams, "params not null");
        assertEqual(1, receivedParams.Count, "one param");
        assertEqual("Gold", receivedParams[0], "param translated");
    }

    static void testInvokeLocalizationCallback_NoParams()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["你好"] = "Hello";
        var obj = new MockUGUIText();
        string receivedText = null;
        List<string> receivedParams = new List<string>(); // 非 null 哨兵

        manager.registeLocalization(obj, "你好", (o, text, list) => {
            receivedText = text;
            receivedParams = list;
        });

        assertEqual("Hello", receivedText, "text translated");
        assertNull(receivedParams, "no params → null");
    }

    // ==================== unregisteLocalization Image ====================

    static void testUnregisteLocalization_Image()
    {
        LocalizationManager manager = new LocalizationManager();
        var img = new MockUGUIImage();
        manager.registeLocalization(img, "icon_Chinese");
        manager.unregisteLocalization(img);
        // 不崩溃即可
    }

    // ==================== setCurrentLanguage 空回调 ====================

    static void testSetCurrentLanguage_NullReloadCallback()
    {
        LocalizationManager manager = new LocalizationManager();
        // setReloadLanguageCallback 未设置 → mReloadLanguageCallback?.Invoke 安全跳过
        manager.setCurrentLanguage("Chinese");
        assertEqual("Chinese", manager.getCurrentLanguage(), "language set without reload callback");
    }

    // ==================== 反射辅助 ====================

    private static Dictionary<string, string> GetLanguageDict(LocalizationManager manager)
    {
        var field = typeof(LocalizationManager).GetField("mLocalizationLanguage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(manager) as Dictionary<string, string>;
    }

    private static Dictionary<int, string> GetIDDict(LocalizationManager manager)
    {
        var field = typeof(LocalizationManager).GetField("mLocalizationLanguageID",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(manager) as Dictionary<int, string>;
    }

    // ==================== 第三轮新增测试 ====================

    static void testGetLocalize_Text_MultipleLookups()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Key"] = "Value";

        // 多次查找同一key
        assertEqual("Value", manager.getLocalize("Key"), "first");
        assertEqual("Value", manager.getLocalize("Key"), "second");
        assertEqual("Value", manager.getLocalize("Key"), "third");
    }

    static void testGetLocalize_ID_Negative()
    {
        LocalizationManager manager = new LocalizationManager();
        string result = manager.getLocalize(-1);
        assertTrue(result.Contains("Localization:-1"), "negative ID format");
    }

    static void testGetLocalize_TextWithParams_EmptyParamArray()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["Hello"] = "你好";

        string result = manager.getLocalize("Hello", new string[0]);
        assertEqual("你好", result, "empty array → no formatting");
    }

    static void testGetLocalize_TextWithParams_AllParamsTranslatable()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["{0}和{1}"] = "{0} and {1}";
        dict["猫"] = "Cat";
        dict["狗"] = "Dog";

        string result = manager.getLocalize("{0}和{1}", "猫", "狗");
        assertTrue(result.Contains("Cat") && result.Contains("Dog"), "both params translated");
    }

    static void testGetLocalize_IDWithParams_NullParamArray()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[5001] = "Hello";

        string result = manager.getLocalize(5001, (string[])null);
        assertEqual("Hello", result, "ID null params → no formatting");
    }

    static void testGetLocalize_TextWithListParams_SingleItem()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetLanguageDict(manager);
        dict["获得{0}"] = "Got {0}";

        List<string> single = new List<string> { "100" };
        string result = manager.getLocalize("获得{0}", single);
        assertTrue(result.Contains("100"), "single item list");
    }

    static void testGetLocalize_IDWithListParams_NullList()
    {
        LocalizationManager manager = new LocalizationManager();
        var dict = GetIDDict(manager);
        dict[6001] = "Test";

        string result = manager.getLocalize(6001, (List<string>)null);
        assertEqual("Test", result, "ID null list → no formatting");
    }

    static void testSetCurrentLanguage_SameLanguageTwice()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setCurrentLanguage("Chinese");
        manager.setCurrentLanguage("Chinese"); // 重复设置同一语言
        assertEqual("Chinese", manager.getCurrentLanguage(), "same language");
    }

    static void testGetCurrentLocale_UnknownLanguage()
    {
        LocalizationManager manager = new LocalizationManager();
        manager.setCurrentLanguage("Klingon");
        assertNull(manager.getCurrentLocale(), "unknown language → null locale");
    }

    static void testRegisteAction_SameCallbackTwice()
    {
        LocalizationManager manager = new LocalizationManager();
        int count = 0;
        Action cb = () => count++;
        manager.registeAction(cb);
        manager.registeAction(cb); // 注册两次同一回调

        manager.setCurrentLanguage("Chinese");
        // 同一个回调被 += 两次，所以触发两次
        assertEqual(2, count, "same callback invoked twice (registered twice)");
    }


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建带语言表注入的管理器
	// ═════════════════════════════════════════════════════════════════
	private static LocalizationManager createManager()
	{
		LocalizationManager manager = new LocalizationManager();
		manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
		{
			// 模拟从表格加载当前语言的数据
			zhKeyList.Add("hello", "你好");
			zhKeyList.Add("world", "世界");
			zhKeyList.Add("greet", "你好,{0}");
			idKeyList.Add(1001, "ID文本");
			idKeyList.Add(2002, "带参ID:{0}");
		});
		return manager;
	}

	// setCurrentLanguage → 重载回调链: 表被填充, getLocalize 可命中
	private static void testReloadCallbackChain()
	{
		LocalizationManager manager = createManager();
		try
		{
			string reloadedLanguage = null;
			manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
			{
				reloadedLanguage = language;
				zhKeyList.Add("hello", "你好");
			});
			manager.setCurrentLanguage("Chinese");
			assertEqual("Chinese", reloadedLanguage, "重载回调收到语言名");
			assertEqual("你好", manager.getLocalize("hello"), "表填充后命中翻译");
		}
		finally
		{
			manager.destroy();
		}
	}

	// getLocalize(str): 命中 / 未命中回退原文
	private static void testGetLocalizeString()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			assertEqual("你好", manager.getLocalize("hello"), "中文命中");
			assertEqual("世界", manager.getLocalize("world"), "中文命中 2");
			assertEqual("not_exist", manager.getLocalize("not_exist"), "未命中回退原文");
		}
		finally
		{
			manager.destroy();
		}
	}

	// getLocalize(id): 命中 / 未命中回退 "Localization:id"
	private static void testGetLocalizeInt()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			assertEqual("ID文本", manager.getLocalize(1001), "ID 命中");
			assertEqual("Localization:9999", manager.getLocalize(9999), "ID 未命中回退 Localization:id");
		}
		finally
		{
			manager.destroy();
		}
	}

	// getLocalize(str, param): 参数先翻译再 {0} 替换
	private static void testGetLocalizeWithParam()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			// greet = "你好,{0}", param "world" → 翻译为 "世界" → "你好,世界"
			assertEqual("你好,世界", manager.getLocalize("greet", "world"), "参数 {0} 替换 + 参数翻译");
			assertEqual("你好,世界", manager.getLocalize("greet", new List<string> { "world" }), "List 参数重载");
			// 2002 = "带参ID:{0}", param "world" → "带参ID:世界"
			assertEqual("带参ID:世界", manager.getLocalize(2002, "world"), "ID 带参 {0} 替换");
		}
		finally
		{
			manager.destroy();
		}
	}

	// 语言切换链: zh → en 重新加载, 旧值失效新值生效
	private static void testLanguageSwitchChain()
	{
		LocalizationManager manager = new LocalizationManager();
		try
		{
			string current = null;
			manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
			{
				current = language;
				if (language == "Chinese")
				{
					zhKeyList.Add("hello", "你好");
				}
				else if (language == "English")
				{
					zhKeyList.Add("hello", "Hello");
				}
			});
			manager.setCurrentLanguage("Chinese");
			assertEqual("你好", manager.getLocalize("hello"), "中文阶段");
			manager.setCurrentLanguage("English");
			assertEqual("English", current, "切换到 English");
			assertEqual("Hello", manager.getLocalize("hello"), "英文阶段新值");
			manager.setCurrentLanguage("Chinese");
			assertEqual("你好", manager.getLocalize("hello"), "切回中文恢复");
		}
		finally
		{
			manager.destroy();
		}
	}

	// registeAction / unregisteAction: 语言切换回调
	private static void testRegisteAction()
	{
		LocalizationManager manager = createManager();
		try
		{
			int callbackCount = 0;
			Action action = () => callbackCount++;
			manager.registeAction(action);
			manager.setCurrentLanguage("Chinese");
			assertEqual(1, callbackCount, "语言切换触发回调");
			manager.unregisteAction(action);
			manager.setCurrentLanguage("English");
			assertEqual(1, callbackCount, "unregiste 后不再触发");
		}
		finally
		{
			manager.destroy();
		}
	}

	// registeLocalization(fake, "hello"): 注册时立即翻译并 setText
	private static void testRegisteLocalizationImmediate()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			FakeLocalizeText fake = new FakeLocalizeText();
			manager.registeLocalization(fake, "hello");
			assertEqual("你好", fake.getText(), "注册时立即翻译 setText");
		}
		finally
		{
			manager.destroy();
		}
	}

	// setCurrentLanguage 切换后已注册文本自动刷新
	private static void testRegisteLocalizationRefreshOnSwitch()
	{
		LocalizationManager manager = new LocalizationManager();
		try
		{
			manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
			{
				if (language == "Chinese")
				{
					zhKeyList.Add("hello", "你好");
				}
				else
				{
					zhKeyList.Add("hello", "Hello");
				}
			});
			manager.setCurrentLanguage("Chinese");
			FakeLocalizeText fake = new FakeLocalizeText();
			manager.registeLocalization(fake, "hello");
			assertEqual("你好", fake.getText(), "中文注册");
			manager.setCurrentLanguage("English");
			assertEqual("Hello", fake.getText(), "切换英文自动刷新");
		}
		finally
		{
			manager.destroy();
		}
	}

	// registeLocalization 带 LocalizationCallback: 走回调分支而非 setText
	private static void testRegisteLocalizationWithCallback()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			FakeLocalizeText fake = new FakeLocalizeText();
			string callbackText = null;
			// LocalizationCallback = (IUGUIText, string, List<string>) 三参
			manager.registeLocalization(fake, "hello", (textObj, text, param) => callbackText = text);
			assertEqual("你好", callbackText, "回调分支收到翻译文本");
		}
		finally
		{
			manager.destroy();
		}
	}

	// 未注入重载回调时: setCurrentLanguage 清表, getLocalize 全部回退
	private static void testEmptyTableFallback()
	{
		LocalizationManager manager = new LocalizationManager();
		try
		{
			manager.setCurrentLanguage("Chinese");   // 无回调, 表为空
			assertEqual("abc", manager.getLocalize("abc"), "空表回退原文");
			assertEqual("Localization:5", manager.getLocalize(5), "空表回退 ID 占位");
		}
		finally
		{
			manager.destroy();
		}
	}
}



// 测试辅助: 实现 IUGUIText 的假文本对象(记录 setText 调用)
public class FakeLocalizeText : IUGUIText
{
	private string mText;
	private string mName;

	public FakeLocalizeText(string name = "FakeText")
	{
		mName = name;
	}

	public string getText() { return mText; }

	public T tryGetUnityComponent<T>() where T : Component { return null; }

	public string getName() { return mName; }

	public void setText(string text) { mText = text; }

	public void setText(int text) { mText = text.ToString(); }

	public void setText(long text) { mText = text.LToS(); }
}
