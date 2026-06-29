
// 运行时集成测试结果
public class TestResult
{
	public string mTestName;
	public bool mPassed;
	public string mMessage;
	public float mElapsedMs;
	public TestResult(string name, bool passed, string message, float ms)
	{
		mTestName = name;
		mPassed = passed;
		mMessage = message;
		mElapsedMs = ms;
	}
}
