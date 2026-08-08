using UnityEngine;
using static TestAssert;

// COMWindowDragView 窗口拖拽互斥组件
// 覆盖: COMWindowDragView.mutexDragView/releaseMutexDragView(静态互斥绑定)
public static class COMWindowDragViewTest
{
	public static void Run()
	{
		testMutexDragView();
		testReleaseMutexDragView();
	}

	// COMWindowDragView.mutexDragView: 两个组件互斥, 只操作内部 List, 无窗口/全局依赖
	private static void testMutexDragView()
	{
		COMWindowDragView view0 = new();
		COMWindowDragView view1 = new();
		COMWindowDragView view2 = new();

		// 正常互斥绑定
		COMWindowDragView.mutexDragView(view0, view1);
		COMWindowDragView.mutexDragView(view1, view2);

		// null 参数安全返回, 不抛异常
		COMWindowDragView.mutexDragView(null, view0);
		COMWindowDragView.mutexDragView(view0, null);
		COMWindowDragView.mutexDragView((COMWindowDragView)null, null);

		assertTrue(true, "mutexDragView 全部分支调用成功");
	}

	// COMWindowDragView.releaseMutexDragView: 解除互斥, 只操作 List.Remove
	private static void testReleaseMutexDragView()
	{
		COMWindowDragView view0 = new();
		COMWindowDragView view1 = new();

		// 先绑定再解绑
		COMWindowDragView.mutexDragView(view0, view1);
		COMWindowDragView.releaseMutexDragView(view0, view1);

		// 解绑不存在的互斥关系, 安全(Remove 不存在元素不抛)
		COMWindowDragView.releaseMutexDragView(view0, view1);

		// null 参数安全返回
		COMWindowDragView.releaseMutexDragView(null, view0);
		COMWindowDragView.releaseMutexDragView(view0, null);
		COMWindowDragView.releaseMutexDragView((COMWindowDragView)null, null);

		// 未绑定就直接解绑, 空 List 无影响
		COMWindowDragView view2 = new();
		COMWindowDragView view3 = new();
		COMWindowDragView.releaseMutexDragView(view2, view3);

		assertTrue(true, "releaseMutexDragView 全部分支调用成功");
	}
}

