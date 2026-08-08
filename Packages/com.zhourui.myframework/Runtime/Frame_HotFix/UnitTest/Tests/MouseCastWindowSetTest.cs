using UnityEngine;
using static TestAssert;

// MouseCastWindowSet 正常使用入口守卫式单测(GlobalTouchSystem 射线检测的窗口集合)
//
// 设计要点:
//   - MouseCastWindowSet 继承 ClassObject, 存储"触点检测时窗口"(HashSet<myUGUIObject> + 可用列表),
//     是 GlobalTouchSystem.registeCollider/unregisteCollider 的正常入口集合。
//   - 可局部 new 测试(字段初始化器已建好空容器), 无需全局单例。
//   - 守卫式要点:
//       * addWindow 要求窗口 isDestroy()==false(ClassObject 默认 mHasDestroy=true → 必须 setDestroy(false)),
//         否则触发 logError; removeWindow 移除后若窗口已销毁也触发 logError → 均用未销毁窗口测正常路径。
//       * getWindowOrderList 只在空窗口集合(mWindowSet 空)时安全(foreach 不执行, 不碰
//         isWindowInScreen/mCamera); 有窗口时依赖真实相机/屏幕, 不测(守卫跳过)。
//   - 窗口用 new myUGUIObject()+setDestroy(false)(addWindow/removeWindow 只读 isDestroy,
//     不读 mObject/mRectTransform, 无需 setObject/init)。
public static class MouseCastWindowSetTest
{
	public static void Run()
	{
		testDefaultStateEmpty();
		testAddWindowIncreasesSet();
		testAddDuplicateWindowNoChange();
		testHasWindowExisting();
		testHasWindowNotExisting();
		testRemoveWindowExisting();
		testRemoveWindowNotExisting();
		testUpdateMarksDirty();
		testNotifyWindowActiveChangedMarksDirty();
		testSetCameraThenGetCamera();
		testGetWindowOrderListEmptySafe();
		testResetPropertyClears();
	}

	// ─── 默认状态: 空集合 ────────────────────────────────────────
	private static void testDefaultStateEmpty()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		assertTrue(set.isEmpty(), "新建 MouseCastWindowSet 应为空");
		assertNull(set.getCamera(), "默认 getCamera 应为 null");
	}

	// ─── addWindow: 正常入口, 未销毁窗口加入集合 ─────────────────
	private static void testAddWindowIncreasesSet()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		set.addWindow(win);
		assertFalse(set.isEmpty(), "addWindow 后集合不应为空");
		assertTrue(set.hasWindow(win), "addWindow 后 hasWindow 应为 true");
	}

	// ─── addWindow 重复: HashSet.Add 失败 → 直接返回, 计数不变 ──
	private static void testAddDuplicateWindowNoChange()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		set.addWindow(win);
		set.addWindow(win);
		assertTrue(set.hasWindow(win), "重复 addWindow 后仍应包含该窗口");
		// 移除一次即清空 → 证明集合中只有一份(未重复)
		assertTrue(set.removeWindow(win), "removeWindow 应成功");
		assertTrue(set.isEmpty(), "重复添加同一窗口后集合仍只有一份, 移除后应为空");
	}

	// ─── hasWindow: 已加入窗口 → true ───────────────────────────
	private static void testHasWindowExisting()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		set.addWindow(win);
		assertTrue(set.hasWindow(win), "已加入窗口 hasWindow 应为 true");
	}

	// ─── hasWindow: 未加入窗口 → false ──────────────────────────
	private static void testHasWindowNotExisting()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		myUGUIObject other = new myUGUIObject();
		other.setDestroy(false);
		set.addWindow(win);
		assertFalse(set.hasWindow(other), "未加入窗口 hasWindow 应为 false");
	}

	// ─── removeWindow: 已存在未销毁窗口 → 成功返回 true ─────────
	private static void testRemoveWindowExisting()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		set.addWindow(win);
		bool removed = set.removeWindow(win);
		assertTrue(removed, "removeWindow 已存在未销毁窗口应返回 true");
		assertTrue(set.isEmpty(), "移除后集合应为空");
	}

	// ─── removeWindow: 不存在的窗口 → 返回 false ────────────────
	private static void testRemoveWindowNotExisting()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		myUGUIObject other = new myUGUIObject();
		other.setDestroy(false);
		set.addWindow(win);
		bool removed = set.removeWindow(other);
		assertFalse(removed, "removeWindow 不存在窗口应返回 false");
		assertFalse(set.isEmpty(), "移除不存在的窗口后集合仍应有 1 项");
	}

	// ─── update: 标记列表为脏(不重排, 无依赖) ───────────────────
	private static void testUpdateMarksDirty()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		// update 仅置 mListDirty=true, 空集合下调用安全
		set.update();
		// 验证 getWindowOrderList 在空集合下可安全调用(mListDirty 被重置)
		set.getWindowOrderList();
	}

	// ─── notifyWindowActiveChanged: 标记脏, 无依赖 ───────────────
	private static void testNotifyWindowActiveChangedMarksDirty()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		set.notifyWindowActiveChanged();
		set.getWindowOrderList();
	}

	// ─── setCamera/getCamera: 纯赋值入口 ─────────────────────────
	private static void testSetCameraThenGetCamera()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		GameCamera camera = new GameCamera();
		set.setCamera(camera);
		assertTrue(ReferenceEquals(camera, set.getCamera()), "setCamera 后 getCamera 应为同一实例");
	}

	// ─── getWindowOrderList: 空窗口集合守卫式安全 ────────────────
	//     空集合时 foreach 不执行, 不触发 isWindowInScreen(依赖真实相机/屏幕),
	//     是唯一能守卫式触达的正常入口。
	private static void testGetWindowOrderListEmptySafe()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		set.update();
		var list = set.getWindowOrderList();
		assertNotNull(list, "getWindowOrderList 应返回非 null 列表");
		assertEqual(0, list.Count, "空窗口集合 getWindowOrderList 应返回空列表");
	}

	// ─── resetProperty: 清空集合与相机 ───────────────────────────
	private static void testResetPropertyClears()
	{
		MouseCastWindowSet set = new MouseCastWindowSet();
		myUGUIObject win = new myUGUIObject();
		win.setDestroy(false);
		set.addWindow(win);
		GameCamera camera = new GameCamera();
		set.setCamera(camera);
		set.resetProperty();
		assertTrue(set.isEmpty(), "resetProperty 后集合应为空");
		assertNull(set.getCamera(), "resetProperty 后 getCamera 应为 null");
	}
}
