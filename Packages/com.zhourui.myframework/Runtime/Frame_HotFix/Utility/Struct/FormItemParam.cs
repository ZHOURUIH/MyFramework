using System.IO;
using static StringUtility;

// 表单中的字段参数
public class FormItemParam : FormItem
{
	public string mKey;     // 字段Key
	public string mValue;   // 字段Value
	public FormItemParam(string key, string value)
	{
		mKey = key;
		mValue = value;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mKey = null;
		mValue = null;
	}
	public override void write(MemoryStream postStream, string boundary)
	{
		string formTemplate = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";
		byte[] formDataBytes = format(formTemplate, mKey, mValue).toBytes();
		postStream.Write(formDataBytes, 0, formDataBytes.Length);
	}
}