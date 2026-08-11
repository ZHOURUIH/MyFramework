using UnityEngine;
using static TestAssert;

// LayoutScript 深度测试: 动态创建 UI 对象树
// 覆盖 LayoutScriptTest 未测的核心方法:
//   createUIObject / createUGUIObject(自动 RectTransform) / cloneObject(Instantiate 克隆)
//   newObject(场景查找绑定) / destroyObject / 生命周期回调(update/lateUpdate override)
//   onGameState 基类逻辑(interruptAllCommand + ScrollRect 注册检查, 空列表安全)
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 清理: 子 UI 用 destroyObject(ref, true) 立即销毁(走 destroyWindow); rootGo 手动 DestroyImmediate
public static class LayoutScriptDeepTest
{
	public static void Run()
	{
		testCreateUIObject();
		testCreateUIObjectInactive();
		testCreateUGUIObjectHasRectTransform();
		testCloneObject();
		testDestroyObjectNullsRef();
		testNewObjectNotFoundSafe();
		testNewObjectByName();
		testLifecycleCallbacks();
		testOnGameStateBaseLogic();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 TestLayoutScriptDeep + 根节点(myUGUICanvas)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo)
	{
		rootGo = new GameObject("TestDeepRoot");
		rootGo.AddComponent<RectTransform>();
		// 预加 Canvas: myUGUICanvas.init 的 TryGetComponent 命中后跳过 logError 分支
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		return script;
	}

	// destroyObject(ref, true): 立即销毁(走 destroyWindow)并置空外部引用
	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// createUIObject: 动态创建对象(parent=null → 挂到 mRoot 下)
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateUIObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ui = null;
		try
		{
			ui = script.createUIObject<myUGUIObject>(null, "TestChild", true);
			assertNotNull(ui, "createUIObject 返回非 null");
			assertEqual("TestChild", ui.getGameObject().name, "创建的 GameObject 名正确");
			assertTrue(ui.isActive(), "active=true 后 isActive");
		}
		finally
		{
			destroyUI(ref ui);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// createUIObject(active=false): 创建后不激活
	private static void testCreateUIObjectInactive()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ui = null;
		try
		{
			ui = script.createUIObject<myUGUIObject>(null, "TestChild2", false);
			assertNotNull(ui, "createUIObject 返回非 null");
			assertFalse(ui.isActive(), "active=false 后 isActive=false");
		}
		finally
		{
			destroyUI(ref ui);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// createUGUIObject: 自动添加 RectTransform
	private static void testCreateUGUIObjectHasRectTransform()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ui = null;
		try
		{
			ui = script.createUGUIObject<myUGUIObject>(null, "TestUGUI", true);
			assertNotNull(ui, "createUGUIObject 返回非 null");
			assertNotNull(ui.getGameObject().GetComponent<RectTransform>(), "UGUI 对象自动添加 RectTransform");
		}
		finally
		{
			destroyUI(ref ui);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// cloneObject: Instantiate 克隆 + cloneFrom 复制
	private static void testCloneObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ori = null;
		myUGUIObject clone = null;
		try
		{
			ori = script.createUIObject<myUGUIObject>(null, "Ori", true);
			clone = script.cloneObject<myUGUIObject>(null, ori, "TestClone");
			assertNotNull(clone, "cloneObject 返回非 null");
			assertEqual("TestClone", clone.getGameObject().name, "克隆体名字正确");
			assertTrue(!ReferenceEquals(clone, ori), "克隆体与原件是不同实例");
		}
		finally
		{
			destroyUI(ref clone);
			destroyUI(ref ori);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// destroyObject(ref): 销毁后外部引用置 null
	private static void testDestroyObjectNullsRef()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIObject ui = script.createUIObject<myUGUIObject>(null, "ToDestroy", true);
			assertNotNull(ui, "创建成功");
			LayoutScript.destroyObject(ref ui, true);
			assertNull(ui, "destroyObject(ref) 后引用置 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newObject: 场景中找不到 → 返回 null(showError=false 不触发 logError)
	private static void testNewObjectNotFoundSafe()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIObject ui;
			script.newObject<myUGUIObject>(out ui, null, "NoSuchName12345", false);
			assertNull(ui, "找不到 GameObject 时返回 null(showError=false)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newObject: 场景中存在同名 GameObject → 绑定
	private static void testNewObjectByName()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		GameObject target = new GameObject("TestTarget");
		myUGUIObject ui = null;
		try
		{
			script.newObject<myUGUIObject>(out ui, null, "TestTarget", false);
			assertNotNull(ui, "newObject 找到场景对象");
			assertTrue(ReferenceEquals(target, ui.getGameObject()), "绑定到场景中的同名 GameObject");
		}
		finally
		{
			// ui 非 null 时 target 已被 destroyObject 销毁(Unity 假 null 判断跳过); 否则手动销毁
			destroyUI(ref ui);
			if (target != null)
			{
				UnityEngine.Object.DestroyImmediate(target);
			}
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 生命周期回调: update/lateUpdate override 被驱动
	private static void testLifecycleCallbacks()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			script.update(0.1f);
			script.lateUpdate(0.2f);
			script.update(0.3f);
			assertEqual(2, script.updateCount, "update 被调用 2 次");
			assertEqual(1, script.lateUpdateCount, "lateUpdate 被调用 1 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onGameState 基类逻辑: interruptAllCommand + isEditor 注册检查(mRoot 无 ScrollRect → 空列表安全)
	private static void testOnGameStateBaseLogic()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			// 第一次: mRegisterChecked=false → 走注册检查(空列表安全)
			script.onGameState();
			// 第二次: mRegisterChecked=true → 跳过检查
			script.onGameState();
			// 不抛异常即通过(基类逻辑空安全)
			assertTrue(true, "onGameState 基类逻辑空安全");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// LayoutScript 深度测试子类: override update/lateUpdate 记录调用
public class TestLayoutScriptDeep : LayoutScript
{
	public override void assignWindow() { }
	public int updateCount;
	public int lateUpdateCount;

	public override void update(float elapsedTime) 
	{
		base.update(elapsedTime);
		updateCount++; 
	}
	public override void lateUpdate(float elapsedTime) 
	{
		base.lateUpdate(elapsedTime);
		lateUpdateCount++; 
	}
	public new void resetProperty()
	{
		base.resetProperty();
		updateCount = 0;
		lateUpdateCount = 0;
	}
}
