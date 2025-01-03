using System;
using System.Data;
using static StringUtility;

public class ExcelTable
{
	protected DataTable mTable;
	public void setTable(DataTable table) { mTable = table; }
	public int getRowCount() { return mTable.Rows.Count; }
	public DataColumn getColumn(int index) { return mTable.Columns[index]; }
	public int getColumnCount() { return mTable.Columns.Count; }
	public object getCell(int row, int col) { return mTable.Rows[row][col]; }
	public string getCellString(int row, int col, bool checkNull = false)
	{
		object value = mTable.Rows[row][col];
		if (value is DBNull)
		{
			value = null;
		}
		if (checkNull && value == null)
		{
			throw new Exception("获取数据错误, row:" + row + ",col:" + col);
		}
		try
		{
			return value?.ToString();
		}
		catch (Exception e)
		{
			throw new Exception("获取数据错误, row:" + row + ",col:" + col + ", " + e.Message);
		}
	}
	public int getCellInt(int row, int col)
	{
		try
		{
			int intValue = 0;
			object value = mTable.Rows[row][col];
			if (value is DBNull)
			{
				intValue = 0;
			}
			else if (value is double)
			{
				intValue = (int)(double)value;
			}
			else if (value is string)
			{
				intValue = SToI((string)value);
			}
			return intValue;
		}
		catch (Exception e)
		{
			throw new Exception("获取数据错误, row:" + row + ",col:" + col + ", " + e.Message);
		}
	}
	public float getCellFloat(int row, int col)
	{
		try
		{
			float floatValue = 0.0f;
			object value = mTable.Rows[row][col];
			if (value is DBNull)
			{
				floatValue = 0.0f;
			}
			else if (value is double)
			{
				floatValue = (float)(double)value;
			}
			else if (value is string)
			{
				floatValue = SToF((string)value);
			}
			return floatValue;
		}
		catch (Exception e)
		{
			throw new Exception("获取数据错误, row:" + row + ",col:" + col + ", " + e.Message);
		}
	}
}