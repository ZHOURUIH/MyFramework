using System;
using System.IO;

// 表单内容的基类
public abstract class FormItem : ClassObject
{
	public abstract void write(MemoryStream postStream, string boundary);
}