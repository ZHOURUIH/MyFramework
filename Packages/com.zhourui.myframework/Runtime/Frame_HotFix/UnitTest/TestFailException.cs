using System;

public class TestFailException : Exception
{
	public TestFailException(string message) : base(message) { }
}