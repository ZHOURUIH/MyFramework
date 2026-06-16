using System.Collections;
using static TestAssert;

public static class AsyncOperationAndTaskGroupTest
{
	public static void Run()
	{
		testCustomAsyncOperation();
		testCustomMultiAsyncOperation();
		testAsyncTaskGroupEmptyAndCallback();
		testAsyncTaskGroupWaitsUntilAllEnumeratorsFinish();
		testResetProperty();
	}
	private static void testCustomAsyncOperation()
	{
		CustomAsyncOperation op = new();
		assertTrue(op.keepWaiting, "默认应等待");
		assertEqual(op, op.setFinish(), "setFinish 返回自身便于链式调用");
		assertFalse(op.keepWaiting, "setFinish 后不再等待");
		op.Reset();
		assertTrue(op.keepWaiting, "Reset 后重新等待");
	}
	private static void testCustomMultiAsyncOperation()
	{
		CustomMultiAsyncOperation multi = new();
		CustomAsyncOperation a = new();
		CustomAsyncOperation b = new();
		multi.addOperation(a);
		multi.addOperation(b);
		assertTrue(multi.keepWaiting, "任意子操作未完成时应等待");
		a.setFinish();
		assertTrue(multi.keepWaiting, "仍有子操作未完成时应等待");
		b.setFinish();
		assertFalse(multi.keepWaiting, "所有子操作完成后不等待");
		multi.Reset();
		assertFalse(multi.keepWaiting, "Reset 会清空子操作列表");
	}
	private static void testAsyncTaskGroupEmptyAndCallback()
	{
		int callbackCount = 0;
		AsyncTaskGroup group = new();
		group.setCallback(() => ++callbackCount);
		assertTrue(group.checkDone(), "空任务组应立即完成");
		assertEqual(1, callbackCount, "完成时应回调一次");
	}
    private static void testAsyncTaskGroupWaitsUntilAllEnumeratorsFinish()
    {
        int callbackCount = 0;
        AsyncTaskGroup group = new();
        group.setCallback(() => ++callbackCount);
        group.addTask(waitFrames(1));
        group.addTask(waitFrames(2));

        bool done = false;
        for (int i = 0; i < 10; ++i)
        {
            done = group.checkDone();
            if (done)
            {
                break;
            }
            assertEqual(0, callbackCount, "任务未完成前不应触发回调");
        }
        assertTrue(done, "多次检查后全部任务应完成");
        assertEqual(1, callbackCount, "完成时应回调一次");
    }
    private static void testResetProperty()
	{
		AsyncTaskGroup group = new();
		group.addTask(waitFrames(1));
		group.setCallback(() => { });
		group.resetProperty();
		assertEqual(0, group.mEnumerators.Count, "resetProperty 清空任务列表");
		assertNull(group.mCallback, "resetProperty 清空回调");
		assertTrue(group.isDestroy(), "resetProperty 调用 base.resetProperty");
	}
	private static IEnumerator waitFrames(int frames)
	{
		for (int i = 0; i < frames; ++i)
		{
			yield return null;
		}
	}
}