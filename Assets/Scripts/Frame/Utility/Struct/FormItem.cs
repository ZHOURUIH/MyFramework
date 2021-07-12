using System;

// 作为字段参数时,只填写mKey和mValue
// 作为文件内容时,只填写mFileContont和mFileName
public struct FormItem
{
	public byte[] mFileContent;
	public string mFileName;
	public int mFileLength;
	public string mKey;
	public string mValue;
	public FormItem(byte[] file, string fileName, int fileLength = -1)
	{
		mFileContent = file;
		mFileName = fileName;
		mFileLength = fileLength < 0 ? file.Length : fileLength;
		mKey = "";
		mValue = "";
	}
	public FormItem(string key, string value)
	{
		mFileContent = null;
		mFileName = "";
		mFileLength = 0;
		mKey = key;
		mValue = value;
	}
}