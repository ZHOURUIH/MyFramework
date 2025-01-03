using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using static StringUtility;

public class ExcelReader
{
	protected List<ExcelTable> mTableList;
	protected IExcelDataReader mReader;
	protected Stream mStream;
	protected DataSet mDataSet;
	protected string mFilePath;
	public ExcelReader(string filePath)
	{
		mFilePath = filePath;
		mTableList = new List<ExcelTable>();
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		try
		{
			mStream = new FileStream(mFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
		catch
		{
			Console.WriteLine("文件打开失败,是否已在其他应用程序打开? 文件名:" + mFilePath);
			return;
		}
		if (endWith(filePath, ".csv", false))
		{
			mReader = ExcelReaderFactory.CreateCsvReader(mStream);
		}
		else
		{
			mReader = ExcelReaderFactory.CreateReader(mStream);
		}
		mDataSet = mReader.AsDataSet();
		int tableCount = mDataSet.Tables.Count;
		if (tableCount == 0)
		{
			Console.WriteLine("文件中没有表格. 文件名:" + mFilePath);
			return;
		}
		for (int i = 0; i < tableCount; ++i)
		{
			ExcelTable table = new();
			table.setTable(mDataSet.Tables[i]);
			mTableList.Add(table);
		}
	}
	public bool isValid() { return mReader != null; }
	public ExcelTable getTable() { return mTableList[0]; }
	public string getFilePath() { return mFilePath; }
	public void close()
	{
		mReader?.Close();
		mStream?.Close();
	}
}