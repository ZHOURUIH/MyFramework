using static TestAssert;

public static class WaitingTest
{
	public static void Run()
	{
		testEmptyWaitingDoneAndProgress();
		testConditionsAndAsyncOperations();
		testCancelDoneAutoDestroyAndReset();
	}
	private static void testEmptyWaitingDoneAndProgress()
	{
		Waiting waiting = new();
		assertTrue(waiting.isDone(), "没有条件时应视为完成");
		assertEqual(1.0f, waiting.getProgress(), "没有条件时进度为 1");
		assertFalse(waiting.isCancel());
	}
	private static void testConditionsAndAsyncOperations()
	{
		bool condition0 = false;
		bool condition1 = true;
		CustomAsyncOperation op = new();
		Waiting waiting = new();
		waiting.addCondition(() => condition0);
		waiting.addCondition(() => condition1);
		waiting.addAsyncOperation(op);
		assertFalse(waiting.isDone());
		float progress = waiting.getProgress();
		assertTrue(progress > 0.32f && progress < 0.34f, "三项条件完成一项时进度约 1/3");
		condition0 = true;
		op.setFinish();
		assertTrue(waiting.isDone());
		assertEqual(1.0f, waiting.getProgress());
	}
	private static void testCancelDoneAutoDestroyAndReset()
	{
		int doneCount = 0;
		bool cancel = false;
		Waiting waiting = new();
		waiting.setCancelCondition(() => cancel);
		assertFalse(waiting.isCancel());
		cancel = true;
		assertTrue(waiting.isCancel());
		waiting.setDoneFunction(() => ++doneCount);
		waiting.done();
		waiting.done();
		assertEqual(1, doneCount, "done 只能调用一次回调");
		waiting.setAutoDestroy(true);
		assertTrue(waiting.isAutoDestroy());
		waiting.resetProperty();
		assertFalse(waiting.isAutoDestroy());
		assertFalse(waiting.isCancel());
		assertTrue(waiting.isDone());
		assertEqual(1.0f, waiting.getProgress());
	}
}