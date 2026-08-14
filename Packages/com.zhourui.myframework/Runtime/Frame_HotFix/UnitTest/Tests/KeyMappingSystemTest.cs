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
        testGetKeyNameDefault();
        testKeyMappingResetProperty();
        testKeyListenInfoResetProperty();
        testFullPipeline();
        testMultipleMappingsIndependent();
        testSetDefaultKeyMappingOverwrite();
        testSetKeyMappingKeepsDefault();
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

    // ---- getKeyName default (未注册键返回空串) ----
    static void testGetKeyNameDefault()
    {
        KeyMappingSystem sys = createSystem();
        assertEqual("", sys.getKeyName(KeyCode.Q), "getKeyName 未注册键返回空串");
        assertEqual("", sys.getKeyName(KeyCode.None), "getKeyName(None) 返回空串");
    }

    // ---- KeyMapping.resetProperty: 纯字段复位 ----
    static void testKeyMappingResetProperty()
    {
        KeyMapping mapping = new KeyMapping();
        mapping.mMappingID = 7;
        mapping.mDefaultKey = KeyCode.A;
        mapping.mKey = KeyCode.B;
        mapping.mMappingName = "Move";
        mapping.resetProperty();
        assertEqual(0, mapping.mMappingID, "resetProperty 后 mMappingID=0");
        assertEqual(KeyCode.None, mapping.mDefaultKey, "resetProperty 后 mDefaultKey=None");
        assertEqual(KeyCode.None, mapping.mKey, "resetProperty 后 mKey=None");
        assertTrue(mapping.mMappingName == null, "resetProperty 后 mMappingName=null");
    }

    // ---- KeyListenInfo.resetProperty: 纯字段复位 ----
    static void testKeyListenInfoResetProperty()
    {
        KeyListenInfo info = new KeyListenInfo();
        info.mCallback = () => { };
        info.mKey = KeyCode.W;
        info.mCombinationKey = COMBINATION_KEY.CTRL;
        info.resetProperty();
        assertTrue(info.mCallback == null, "resetProperty 后 mCallback=null");
        assertEqual(KeyCode.None, info.mKey, "resetProperty 后 mKey=None");
        assertEqual(COMBINATION_KEY.NONE, info.mCombinationKey, "resetProperty 后组合键=NONE");
    }

    // ---- 全链路组合: 注册 → 查询 → 更新 → 默认键 → actionName → 列表 ----
    static void testFullPipeline()
    {
        KeyMappingSystem sys = createSystem();
        // 注册
        assertTrue(sys.setKeyMapping(1, KeyCode.W, "Move"), "注册 Move");
        assertTrue(sys.setKeyMapping(2, KeyCode.Space, "Jump"), "注册 Jump");
        // 查询
        assertEqual(KeyCode.W, sys.getKeyMapping(1), "查询映射 1");
        assertEqual("Move", sys.getKeyMappingActionName(1), "查询 actionName");
        // 更新(同 ID)
        assertTrue(sys.setKeyMapping(1, KeyCode.UpArrow), "更新映射 1 按键");
        assertEqual(KeyCode.UpArrow, sys.getKeyMapping(1), "更新后按键为 UpArrow");
        assertEqual("Move", sys.getKeyMappingActionName(1), "更新不覆盖 actionName");
        // 设置默认键
        sys.setDefaultKeyMapping(1, KeyCode.W);
        assertEqual(KeyCode.W, sys.getDefaultMappingKey(1), "默认键设置为 W");
        assertEqual(KeyCode.UpArrow, sys.getKeyMapping(1), "当前键不受默认键影响");
        // 列表完整性
        var list = sys.getKeyMappingList();
        assertEqual(2, list.Count, "列表含 2 组映射");
        assertTrue(list.ContainsKey(2), "列表含映射 2");
    }

    // ---- 多组映射独立更新 ----
    static void testMultipleMappingsIndependent()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        sys.setKeyMapping(2, KeyCode.A, "Left");
        sys.setKeyMapping(3, KeyCode.S, "Back");
        // 更新映射 2 不影响其他
        sys.setKeyMapping(2, KeyCode.D, "Right");
        assertEqual(KeyCode.W, sys.getKeyMapping(1), "映射 1 不变");
        assertEqual(KeyCode.D, sys.getKeyMapping(2), "映射 2 已更新");
        assertEqual(KeyCode.S, sys.getKeyMapping(3), "映射 3 不变");
        assertEqual("Right", sys.getKeyMappingActionName(2), "映射 2 actionName 更新");
        assertEqual(3, sys.getKeyMappingList().Count, "3 组映射都在");
    }

    // ═════════════════════════════════════════════════════════════════
    // 组合场景
    // ═════════════════════════════════════════════════════════════════

    // setDefaultKeyMapping 两次覆盖: 第二次生效
    static void testSetDefaultKeyMappingOverwrite()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        sys.setDefaultKeyMapping(1, KeyCode.A);
        assertEqual(KeyCode.A, sys.getDefaultMappingKey(1), "第一次默认 A");
        sys.setDefaultKeyMapping(1, KeyCode.S);
        assertEqual(KeyCode.S, sys.getDefaultMappingKey(1), "第二次覆盖为 S");
        // 当前绑定 key 不受默认值影响
        assertEqual(KeyCode.W, sys.getKeyMapping(1), "当前绑定仍 W");
    }

    // setKeyMapping 覆盖后 getDefaultMappingKey 保留原默认
    static void testSetKeyMappingKeepsDefault()
    {
        KeyMappingSystem sys = createSystem();
        sys.setKeyMapping(1, KeyCode.W, "Move");
        sys.setDefaultKeyMapping(1, KeyCode.A);
        // 覆盖当前绑定为 D, 默认值 A 保留
        sys.setKeyMapping(1, KeyCode.D, "Move");
        assertEqual(KeyCode.D, sys.getKeyMapping(1), "当前绑定更新为 D");
        assertEqual(KeyCode.A, sys.getDefaultMappingKey(1), "默认值 A 保留");
    }
}
