using System;
using System.Collections.Generic;

class Program : FileUtility
{
	static void Main(string[] args)
	{
		if (!Config.parse("./ExcelConverterConfig.txt"))
		{
			Console.ReadKey();
			return;
		}

		// Usage: [exe] watch
		if (args.Length > 0 && args[0] == "watch")
		{
			// 监听模式，仅处理表格数据的更新
			ExcelWatcher.start();
			return;
		}
		List<ExcelReader> readerList = new();

		List<string> files = new();
		findFiles(Config.mExcelPath, files, Config.mFileSuffix);
		for (int i = 0; i < files.Count; ++i)
		{
			// ~开头的是临时文件,不处理
			if (startWith(getFileName(files[i]), "~"))
			{
				continue;
			}
			ExcelReader reader = new(files[i]);
			if (!reader.isValid())
			{
				Console.WriteLine("打开文件错误:" + files[i]);
				Console.ReadKey();
				return;
			}
			readerList.Add(reader);
		}

		// 生成调试用的txt文件
		if (args.Length > 0 && args[0] == "Debug")
		{
			// 删除生成的bytes文件,不删除meta
			List<string> bytesList = new();
			findFiles(Config.mExcelBytesPath, bytesList, ".txt");
			foreach (string str in bytesList)
			{
				deleteFile(str);
			}
			ExcelConverter.generateDataTxt(readerList);
		}
		// 生成表格二进制文件
		else
		{
			// 删除C#的代码文件,c#的只删除代码文件,不删除meta文件
			List<string> csDataFileList = new();
			findFiles(Config.mExcelDataHotFixPath, csDataFileList, ".cs");
			foreach (string str in csDataFileList)
			{
				deleteFile(str);
			}
			List<string> csTableFileList = new();
			findFiles(Config.mExcelTableHotFixPath, csTableFileList, ".cs");
			foreach (string str in csTableFileList)
			{
				deleteFile(str);
			}

			// 删除生成的bytes文件,不删除meta
			List<string> bytesList = new();
			findFiles(Config.mExcelBytesPath, bytesList, ".bytes");
			foreach (string str in bytesList)
			{
				deleteFile(str);
			}
			ExcelConverter.generate(readerList);
		}

		for (int i = 0; i < readerList.Count; ++i)
		{
			readerList[i].close();
		}

		Console.WriteLine("共" + files.Count + "个文件");
	}
}