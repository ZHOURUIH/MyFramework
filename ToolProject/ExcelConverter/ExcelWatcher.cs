using System;
using System.IO;

public static class ExcelWatcher
{
	public static void start()
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"Watching changes in {Config.mExcelPath}");
		Console.ResetColor();

		using var watcher = new FileSystemWatcher(Config.mExcelPath);

		// excel的更新是 rename 模式
		watcher.Renamed += OnRenamed;

		watcher.Filter = "*.xlsx";
		watcher.IncludeSubdirectories = false;
		watcher.EnableRaisingEvents = true;

		Console.WriteLine("Press enter to exit.");
		Console.ReadLine();
	}
	private static void OnRenamed(object sender, RenamedEventArgs e)
	{
		// 过滤掉不是 excel 的文件
		if (e.Name.StartsWith("~") || !e.Name.EndsWith(".xlsx"))
		{
			return;
		}
		Console.ForegroundColor = ConsoleColor.DarkCyan;
		Console.Write($"[{DateTime.Now}] ");
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine($"Update: {e.Name}");
		Console.ResetColor();
		// 生成 bytes
		var reader = new ExcelReader(e.FullPath);
		ExcelConverter.generateBytesOnly(reader);
		reader.close();
	}

}

