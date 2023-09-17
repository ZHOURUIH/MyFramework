using System;

// 所有需要导出给主工程调用的函数
// 也就是在主工程中只允许调用这个类中的函数,不允许调用其他函数
public class ILRExport
{
	public static void start()
	{
		GameILR.start();
	}
}