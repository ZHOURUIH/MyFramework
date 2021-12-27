using System;
using System.IO;

// 表单内容的基类
public abstract class FormItem : FrameBase
{
	public abstract void write(MemoryStream postStream, string boundary);
}