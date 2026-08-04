using UnityEngine;
using static TestAssert;

// KeyMappingSystem 按键映射逻辑测试
// 测试 setKeyMapping/getKeyMapping/冲突检测/getKeyMappingName 等
public static class KeyMappingSystemTest
{
    public static void Run()
    {
        testSetKeyMapping();
        testGetKeyMapping();
        testGetKeyMappingNotFound();
        testGetKeyMappingActionName();
        testGetKeyMappingActionNameNotFound();
        testGetDefaultMappingKey();
        testGetDefaultMappingKeyNotFound();
        testSetDefaultKeyMapping();
        testSetKeyMappingConflict();
        testSetKeyMappingUpdateExisting();
        testSetKeyMappingWithActionName();
        testSetKeyMappingKeyNone();
        testGetKeyListNameNone();
        testGetKeyMappingList();
    }

    static KeyMappingSystem createSystem()
    {
        return new KeyMappingSystem();
    }

    // ---- setKeyMapping ----
    static void testSetKeyMapping()
    {
        KeyMappingSystem sys = createSystem();
        bool result = sys.setKeyMapping(1, KeyCode.W, "Move");
        assertTrue(result, "setKeyMapping returns true");
        assertEqual(KeyCode.W, sys.getKeyMapping(1), "getKeyMapping returns W");
    }

    // ---- getKeyMapping ----
    static void testGetKeyMapping()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(10, KeyCode.Space);
        assertEqual(KeyCode.Space, sys.getKeyMapping(10), "getKeyMapping returns Space");
    }

    // ---- getKeyMapping not found ----
    static void testGetKeyMappingNotFound()
    {
        KeyMappingSystem sys = createSystem();
        assertEqual(KeyCode.None, sys.getKeyMapping(999), "getKeyMapping not found returns None");
    }

    // ---- getKeyMappingActionName ----
    static void testGetKeyMappingActionName()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        assertEqual("Move", sys.getKeyMappingActionName(1), "getKeyMappingActionName returns Move");
    }

    // ---- getKeyMappingActionName not found ----
    static void testGetKeyMappingActionNameNotFound()
    {
        KeyMappingSystem sys = createSystem();
        assertEqual("", sys.getKeyMappingActionName(999), "getKeyMappingActionName not found returns empty");
    }

    // ---- getDefaultMappingKey ----
    static void testGetDefaultMappingKey()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W);
        // 默认键在 setKeyMapping 时不会被设置，所以是 KeyCode.None
        assertEqual(KeyCode.None, sys.getDefaultMappingKey(1), "getDefaultMappingKey default is None");
    }

    // ---- getDefaultMappingKey not found ----
    static void testGetDefaultMappingKeyNotFound()
    {
        KeyMappingSystem sys = createSystem();
        assertEqual(KeyCode.None, sys.getDefaultMappingKey(999), "getDefaultMappingKey not found returns None");
    }

    // ---- setDefaultKeyMapping ----
    static void testSetDefaultKeyMapping()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W);
        sys.setDefaultKeyMapping(1, KeyCode.UpArrow);
        assertEqual(KeyCode.UpArrow, sys.getDefaultMappingKey(1), "setDefaultKeyMapping changes default");
        // 当前按键不应改变
        assertEqual(KeyCode.W, sys.getKeyMapping(1), "setDefaultKeyMapping does not change current key");
    }

    // ---- setKeyMapping no-conflict (same ID update) ----
    // 验证同 ID 更新不触发冲突检测，以及冲突检测条件 (key != None && different ID)
    static void testSetKeyMappingConflict()
    {
        KeyMappingSystem sys = createSystem();
        // 同 ID 更新不触发冲突（item.mMappingID == mappingID 时跳过）
        bool result1 = sys.setKeyMapping(1, KeyCode.W, "Move");
        assertTrue(result1, "first set returns true");
        bool result2 = sys.setKeyMapping(1, KeyCode.UpArrow, "Move");
        assertTrue(result2, "same ID update returns true (no conflict)");
        assertEqual(KeyCode.UpArrow, sys.getKeyMapping(1), "key updated");

        // 两个不同 ID 可以使用不同 KeyCode，不冲突
        KeyMappingSystem sys2 = createSystem();
        sys2.setKeyMapping(1, KeyCode.W);
        bool result3 = sys2.setKeyMapping(2, KeyCode.Space);
        assertTrue(result3, "different key returns true (no conflict)");
        assertEqual(KeyCode.W, sys2.getKeyMapping(1), "mapping 1 unchanged");
        assertEqual(KeyCode.Space, sys2.getKeyMapping(2), "mapping 2 set");
    }

    // ---- setKeyMapping update existing ----
    static void testSetKeyMappingUpdateExisting()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        // 更新同一个 mappingID 的按键（不应冲突）
        bool result = sys.setKeyMapping(1, KeyCode.UpArrow, "Move");
        assertTrue(result, "setKeyMapping update same ID returns true");
        assertEqual(KeyCode.UpArrow, sys.getKeyMapping(1), "getKeyMapping returns updated key");
    }

    // ---- setKeyMapping with action name ----
    static void testSetKeyMappingWithActionName()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        // 再次设置，不传 actionName，不应覆盖原有名称
        sys.setKeyMapping(1, KeyCode.UpArrow);
        assertEqual("Move", sys.getKeyMappingActionName(1), "actionName preserved when not provided");
    }

    // ---- setKeyMapping KeyCode.None ----
    static void testSetKeyMappingKeyNone()
    {
        KeyMappingSystem sys = createSystem();
        // KeyCode.None 跳过冲突检测
        bool result = sys.setKeyMapping(1, KeyCode.None, "NoneAction");
        assertTrue(result, "setKeyMapping with None returns true");
        assertEqual(KeyCode.None, sys.getKeyMapping(1), "getKeyMapping returns None");
    }

    // ---- getKeyListName with KeyCode.None ----
    static void testGetKeyListNameNone()
    {
        KeyMappingSystem sys = createSystem();
        assertEqual("", sys.getKeyListName(KeyCode.None), "getKeyListName with None returns empty");
    }

    // ---- getKeyMappingList ----
    static void testGetKeyMappingList()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        sys.setKeyMapping(2, KeyCode.Space, "Jump");

        var list = sys.getKeyMappingList();
        assertNotNull(list, "getKeyMappingList returns non-null");
        assertTrue(list.ContainsKey(1), "list contains mapping 1");
        assertTrue(list.ContainsKey(2), "list contains mapping 2");
        assertEqual(2, list.Count, "list has 2 entries");
    }
}
