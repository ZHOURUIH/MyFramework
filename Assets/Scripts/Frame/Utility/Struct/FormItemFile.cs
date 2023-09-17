using System;
using System.IO;
using static StringUtility;
using static BinaryUtility;

// 表单中的文件内容
public class FormItemFile : FormItem
{
	public byte[] mFileContent;     // 文件内容
	public int mFileLength;         // 文件长度
	public string mFileName;        // 文件名
	public FormItemFile(byte[] file, int length, string fileName)
	{
		mFileContent = file;
		mFileLength = length;
		mFileName = fileName;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mFileContent = null;
		mFileName = null;
		mFileLength = 0;
	}
	public override void write(MemoryStream postStream, string boundary)
	{
		string formTemplate = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: binary/octet-stream\r\n\r\n";
		byte[] formdataBytes = stringToBytes(formatThread(formTemplate, mFileName));
		postStream.Write(formdataBytes, 0, formdataBytes.Length);
		// 写入文件内容
		postStream.Write(mFileContent, 0, mFileLength);
	}
}