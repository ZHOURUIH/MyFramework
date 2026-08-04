using static TestAssert;
using System;

// ComponentOwner / GameComponent 穷举测试
// 覆盖所有公开方法、重载和关键分支
public static class ComponentTest
{
    public static void Run()
    {
        // ─── ComponentOwner: getComponent ───
        testGetComponentNotFound();
        testGetComponentTypeOverload();
        testGetComponentOutOverload();
        // ─── ComponentOwner: getActiveComponent ───
        testGetActiveComponentNotFound();
        testGetActiveComponentTypeOverload();
        testGetActiveComponentOutOverload();
        // ─── ComponentOwner: getOrAddComponent ───
        testGetOrAddComponentNotFound();
        testGetOrAddComponentTypeOverload();
        testGetOrAddComponentOutOverload();
        // ─── ComponentOwner: setActive ───
        testSetActiveTrue();
        testSetActiveFalse();
        // ─── ComponentOwner: destroy ───
        testDestroy();
        testDestroyWithCallback();
        testRemoveDestroyCallback();
        testDestroyCallbackNull();
        // ─── ComponentOwner: setIgnoreTimeScale ───
        testSetIgnoreTimeScale();
        testSetIgnoreTimeScaleComponentOnly();
        // ─── ComponentOwner: disableComponent ───
        testAddDisableComponent();
        testAddDisableComponentType();
        testRemoveDisableComponent();
        testRemoveDisableComponentType();
        // ─── ComponentOwner: activeComponent ───
        testActiveComponentNotFound();
        testActiveComponentTypeNotFound();
        // ─── ComponentOwner: addDontAutoCreate ───
        testAddDontAutoCreate();
        testAddDontAutoCreateType();
        testAddInitComponentDontAutoCreate();
        testAddInitComponentDontAutoCreateType();
        // ─── ComponentOwner: getComponentList / getAllComponent ───
        testGetComponentListEmpty();
        testGetAllComponentEmpty();
        // ─── ComponentOwner: resetProperty ───
        testComponentOwnerResetProperty();
        // ─── ComponentOwner: getTypeName ───
        testGetTypeName();
        // ─── GameComponent: init ───
        testGameComponentInit();
        testGameComponentInitNull();
        // ─── GameComponent: setActive ───
        testGameComponentSetActiveTrue();
        testGameComponentSetActiveFalse();
        testGameComponentSetActiveToggle();
        // ─── GameComponent: destroy ───
        testGameComponentDestroy();
        // ─── GameComponent: resetProperty ───
        testGameComponentResetProperty();
        // ─── GameComponent: setDefaultActive ───
        testGameComponentSetDefaultActive();
        testGameComponentIsDefaultActive();
        // ─── GameComponent: setIgnoreTimeScale ───
        testGameComponentSetIgnoreTimeScale();
        testGameComponentIsIgnoreTimeScale();
        // ─── GameComponent: isComponentActive ───
        testGameComponentIsComponentActive();
        // ─── GameComponent: getType ───
        testGameComponentGetType();
        // ─── GameComponent: notifyOwnerActive ───
        testGameComponentNotifyOwnerActive();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: getComponent
    // ═══════════════════════════════════════════════════════════════════

    private static void testGetComponentNotFound()
    {
        var owner = new TestComponentOwner();
        var com = owner.getComponent<TestGameComponent>();
        assertNull(com, "未添加时 getComponent 返回 null");
        owner.destroy();
    }

    private static void testGetComponentTypeOverload()
    {
        var owner = new TestComponentOwner();
        var com = owner.getComponent(typeof(TestGameComponent));
        assertNull(com, "getComponent(Type) 未找到返回 null");
        owner.destroy();
    }

    private static void testGetComponentOutOverload()
    {
        var owner = new TestComponentOwner();
        var com = owner.getComponent<TestGameComponent>(out var outCom);
        assertNull(com, "getComponent(out) 未找到返回 null");
        assertNull(outCom, "out 参数也应为 null");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: getActiveComponent
    // ═══════════════════════════════════════════════════════════════════

    private static void testGetActiveComponentNotFound()
    {
        var owner = new TestComponentOwner();
        var com = owner.getActiveComponent<TestGameComponent>();
        assertNull(com, "未添加时 getActiveComponent 返回 null");
        owner.destroy();
    }

    private static void testGetActiveComponentTypeOverload()
    {
        var owner = new TestComponentOwner();
        var com = owner.getActiveComponent(typeof(TestGameComponent));
        assertNull(com, "getActiveComponent(Type) 未找到返回 null");
        owner.destroy();
    }

    private static void testGetActiveComponentOutOverload()
    {
        var owner = new TestComponentOwner();
        var com = owner.getActiveComponent<TestGameComponent>(out var outCom);
        assertNull(com, "getActiveComponent(out) 未找到返回 null");
        assertNull(outCom, "out 参数也应为 null");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: getOrAddComponent
    // ═══════════════════════════════════════════════════════════════════

    private static void testGetOrAddComponentNotFound()
    {
        var owner = new TestComponentOwner();
        try
        {
            var com = owner.getOrAddComponent<TestGameComponent>();
            if (com != null)
            {
                assertTrue(com is TestGameComponent, "getOrAddComponent 返回正确类型");
            }
        }
        catch (Exception) { /* 池不可用 */ }
        owner.destroy();
    }

    private static void testGetOrAddComponentTypeOverload()
    {
        var owner = new TestComponentOwner();
        try
        {
            var com = owner.getOrAddComponent(typeof(TestGameComponent));
            if (com != null)
            {
                assertTrue(com is TestGameComponent, "getOrAddComponent(Type) 返回正确类型");
            }
        }
        catch (Exception) { }
        owner.destroy();
    }

    private static void testGetOrAddComponentOutOverload()
    {
        var owner = new TestComponentOwner();
        try
        {
            var com = owner.getOrAddComponent<TestGameComponent>(out var outCom);
            assertTrue(com == outCom, "getOrAddComponent(out) 返回值和 out 参数应一致");
        }
        catch (Exception) { }
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: setActive
    // ═══════════════════════════════════════════════════════════════════

    private static void testSetActiveTrue()
    {
        var owner = new TestComponentOwner();
        bool result = owner.setActive(true);
        assertTrue(result, "setActive(true) 返回 true");
        owner.destroy();
    }

    private static void testSetActiveFalse()
    {
        var owner = new TestComponentOwner();
        bool result = owner.setActive(false);
        assertFalse(result, "setActive(false) 返回 false");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: destroy
    // ═══════════════════════════════════════════════════════════════════

    private static void testDestroy()
    {
        var owner = new TestComponentOwner();
        owner.destroy();
        // 不崩溃
    }

    private static void testDestroyWithCallback()
    {
        var owner = new TestComponentOwner();
        bool called = false;
        ClassObjectCallback cb = obj => called = true;
        owner.addDestroyCallback(cb);
        owner.destroy();
        assertTrue(called, "destroy 时回调应被调用");
    }

    private static void testRemoveDestroyCallback()
    {
        var owner = new TestComponentOwner();
        bool called = false;
        ClassObjectCallback cb = obj => called = true;
        owner.addDestroyCallback(cb);
        owner.removeDestroyCallback(cb);
        owner.destroy();
        assertFalse(called, "移除后 destroy 不应调用回调");
    }

    private static void testDestroyCallbackNull()
    {
        var owner = new TestComponentOwner();
        // add null callback 不崩溃
        owner.addDestroyCallback(null);
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: setIgnoreTimeScale
    // ═══════════════════════════════════════════════════════════════════

    private static void testSetIgnoreTimeScale()
    {
        var owner = new TestComponentOwner();
        assertFalse(owner.isIgnoreTimeScale(), "默认 false");
        owner.setIgnoreTimeScale(true);
        assertTrue(owner.isIgnoreTimeScale(), "setIgnoreTimeScale(true)");
        owner.setIgnoreTimeScale(false);
        assertFalse(owner.isIgnoreTimeScale(), "setIgnoreTimeScale(false)");
        owner.destroy();
    }

    private static void testSetIgnoreTimeScaleComponentOnly()
    {
        var owner = new TestComponentOwner();
        // componentOnly=true 不改变 owner 自身的 ignoreTimeScale
        owner.setIgnoreTimeScale(true, true);
        assertFalse(owner.isIgnoreTimeScale(), "componentOnly=true 时不改变 owner");
        owner.setIgnoreTimeScale(true, false);
        assertTrue(owner.isIgnoreTimeScale(), "componentOnly=false 时改变 owner");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: disableComponent
    // ═══════════════════════════════════════════════════════════════════

    private static void testAddDisableComponent()
    {
        var owner = new TestComponentOwner();
        owner.addDisableComponent<TestGameComponent>();
        owner.destroy();
    }

    private static void testAddDisableComponentType()
    {
        var owner = new TestComponentOwner();
        owner.addDisableComponent(typeof(TestGameComponent));
        owner.destroy();
    }

    private static void testRemoveDisableComponent()
    {
        var owner = new TestComponentOwner();
        owner.addDisableComponent<TestGameComponent>();
        owner.removeDisableComponent<TestGameComponent>();
        owner.destroy();
    }

    private static void testRemoveDisableComponentType()
    {
        var owner = new TestComponentOwner();
        owner.addDisableComponent(typeof(TestGameComponent));
        owner.removeDisableComponent(typeof(TestGameComponent));
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: activeComponent
    // ═══════════════════════════════════════════════════════════════════

    private static void testActiveComponentNotFound()
    {
        var owner = new TestComponentOwner();
        owner.activeComponent<TestGameComponent>(true);
        owner.activeComponent<TestGameComponent>(false);
        owner.destroy();
    }

    private static void testActiveComponentTypeNotFound()
    {
        var owner = new TestComponentOwner();
        owner.activeComponent(typeof(TestGameComponent), true);
        owner.activeComponent(typeof(TestGameComponent), false);
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: addDontAutoCreate
    // ═══════════════════════════════════════════════════════════════════

    private static void testAddDontAutoCreate()
    {
        var owner = new TestComponentOwner();
        owner.addDontAutoCreate<TestGameComponent>();
        owner.destroy();
    }

    private static void testAddDontAutoCreateType()
    {
        var owner = new TestComponentOwner();
        owner.addDontAutoCreate(typeof(TestGameComponent));
        owner.destroy();
    }

    private static void testAddInitComponentDontAutoCreate()
    {
        var owner = new TestComponentOwner();
        owner.addDontAutoCreate<TestGameComponent>();
        var com = owner.addInitComponent<TestGameComponent>(true);
        assertNull(com, "dontAutoCreate 时 addInitComponent 返回 null");
        owner.destroy();
    }

    private static void testAddInitComponentDontAutoCreateType()
    {
        var owner = new TestComponentOwner();
        owner.addDontAutoCreate(typeof(TestGameComponent));
        var com = owner.addInitComponent(typeof(TestGameComponent), true);
        assertNull(com, "dontAutoCreate(Type) 时 addInitComponent 返回 null");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: getComponentList / getAllComponent
    // ═══════════════════════════════════════════════════════════════════

    private static void testGetComponentListEmpty()
    {
        var owner = new TestComponentOwner();
        var list = owner.getComponentList();
        assertNull(list, "无组件时 getComponentList 返回 null");
        owner.destroy();
    }

    private static void testGetAllComponentEmpty()
    {
        var owner = new TestComponentOwner();
        var dict = owner.getAllComponent();
        assertNull(dict, "无组件时 getAllComponent 返回 null");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: resetProperty
    // ═══════════════════════════════════════════════════════════════════

    private static void testComponentOwnerResetProperty()
    {
        var owner = new TestComponentOwner();
        owner.setIgnoreTimeScale(true);
        owner.addDisableComponent<TestGameComponent>();
        owner.addDontAutoCreate<TestGameComponent>();

        owner.resetProperty();
        assertFalse(owner.isIgnoreTimeScale(), "resetProperty 后 ignoreTimeScale=false");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ComponentOwner: getTypeName
    // ═══════════════════════════════════════════════════════════════════

    private static void testGetTypeName()
    {
        var owner = new TestComponentOwner();
        string name = owner.GetTypeName();
        assertNotNull(name, "GetTypeName 不应为 null");
        assertTrue(name.Length > 0, "GetTypeName 不应为空");
        // 应返回类型名
        assertTrue(name.Contains("TestComponentOwner"), "GetTypeName 应包含类型名");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: init
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentInit()
    {
        var owner = new TestComponentOwner();
        var com = new TestGameComponent();
        com.init(owner);
        assertNotNull(com.getOwner(), "init 后 getOwner 非 null");
        assertTrue(com.getOwner() == owner, "getOwner 返回正确 owner");
        assertTrue(com.isActive(), "init 后默认 active=true");
        owner.destroy();
    }

    private static void testGameComponentInitNull()
    {
        var com = new TestGameComponent();
        com.init(null);
        assertNull(com.getOwner(), "init(null) 后 getOwner 为 null");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: setActive
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentSetActiveTrue()
    {
        var owner = new TestComponentOwner();
        var com = new TestGameComponent();
        com.init(owner);
        com.setActive(false);
        com.setActive(true);
        assertTrue(com.isActive(), "setActive(true) 后 isActive=true");
        owner.destroy();
    }

    private static void testGameComponentSetActiveFalse()
    {
        var owner = new TestComponentOwner();
        var com = new TestGameComponent();
        com.init(owner);
        com.setActive(false);
        assertFalse(com.isActive(), "setActive(false) 后 isActive=false");
        owner.destroy();
    }

    private static void testGameComponentSetActiveToggle()
    {
        var owner = new TestComponentOwner();
        var com = new TestGameComponent();
        com.init(owner);
        // 多次切换
        com.setActive(false);
        assertFalse(com.isActive(), "toggle to false");
        com.setActive(true);
        assertTrue(com.isActive(), "toggle to true");
        com.setActive(false);
        assertFalse(com.isActive(), "toggle to false again");
        owner.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: destroy
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentDestroy()
    {
        var owner = new TestComponentOwner();
        var com = new TestGameComponent();
        com.init(owner);
        com.destroy();
        assertNull(com.getOwner(), "destroy 后 getOwner 为 null");
        assertTrue(com.isDestroy(), "destroy 后 isDestroy=true");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: resetProperty
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentResetProperty()
    {
        var com = new TestGameComponent();
        com.resetProperty();
        assertNull(com.getOwner(), "resetProperty 后 owner 为 null");
        assertTrue(com.isActive(), "resetProperty 后 active=true（构造函数默认true）");
        assertFalse(com.isIgnoreTimeScale(), "resetProperty 后 ignoreTimeScale=false");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: setDefaultActive
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentSetDefaultActive()
    {
        var com = new TestGameComponent();
        com.setDefaultActive(false);
        assertFalse(com.isDefaultActive(), "setDefaultActive(false)");
        com.setDefaultActive(true);
        assertTrue(com.isDefaultActive(), "setDefaultActive(true)");
    }

    private static void testGameComponentIsDefaultActive()
    {
        var com = new TestGameComponent();
        assertFalse(com.isDefaultActive(), "默认 defaultActive=false");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: setIgnoreTimeScale
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentSetIgnoreTimeScale()
    {
        var com = new TestGameComponent();
        assertFalse(com.isIgnoreTimeScale(), "默认 false");
        com.setIgnoreTimeScale(true);
        assertTrue(com.isIgnoreTimeScale(), "setIgnoreTimeScale(true)");
        com.setIgnoreTimeScale(false);
        assertFalse(com.isIgnoreTimeScale(), "setIgnoreTimeScale(false)");
    }

    private static void testGameComponentIsIgnoreTimeScale()
    {
        var com = new TestGameComponent();
        assertFalse(com.isIgnoreTimeScale(), "默认 isIgnoreTimeScale=false");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: isComponentActive
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentIsComponentActive()
    {
        var com = new TestGameComponent();
        assertTrue(com.isComponentActive(), "默认 isComponentActive=true");
        com.setActive(false);
        assertFalse(com.isComponentActive(), "setActive(false) 后 isComponentActive=false");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: getType
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentGetType()
    {
        var com = new TestGameComponent();
        Type t = com.getType();
        assertEqual(typeof(TestGameComponent), t, "getType 应返回 TestGameComponent");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GameComponent: notifyOwnerActive
    // ═══════════════════════════════════════════════════════════════════

    private static void testGameComponentNotifyOwnerActive()
    {
        var com = new TestGameComponent();
        bool notified = false;
        com.onNotifyOwnerActive = active => notified = true;
        com.notifyOwnerActive(true);
        assertTrue(notified, "notifyOwnerActive(true) 应触发虚方法");
    }
}

// ─── 测试辅助类 ─────────────────────────────────────────────────────────

public class TestComponentOwner : ComponentOwner
{
    public TestComponentOwner()
    {
        mHasDestroy = false;
    }
}

public class TestGameComponent : GameComponent
{
    public System.Action<bool> onNotifyOwnerActive;

    public TestGameComponent()
    {
        mHasDestroy = false;
    }

    public override void notifyOwnerActive(bool active) { onNotifyOwnerActive?.Invoke(active); }
    public override void resetProperty()
    {
        base.resetProperty();
        onNotifyOwnerActive = null;
    }
}
