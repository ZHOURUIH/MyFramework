using System;

// 作为字段参数时,只填写mKey和mValue
// 作为文件内容时,只填写mFileContont和mFileName
public struct FormItem
{
	public byte[] mFileContent;
	public string mFileName;
	public string mKey;
	public string mValue;
	public FormItem(byte[] file, string fileName)
	{
		mFileContent = file;
		mFileName = fileName;
		mKey = "";
		mValue = "";
	}
	public FormItem(string key, string value)
	{
		mFileContent = null;
		mFileName = "";
		mKey = key;
		mValue = value;
	}
}