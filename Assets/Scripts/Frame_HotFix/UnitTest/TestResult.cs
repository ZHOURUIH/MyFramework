
// 运行时集成测试结果
public class TestResult
{
	public string testName;
	public bool passed;
	public string message;
	public float elapsedMs;

	public TestResult(string name, bool passed, string message, float ms)
	{
		this.testName = name;
		this.passed = passed;
		this.message = message;
		this.elapsedMs = ms;
	}
}
