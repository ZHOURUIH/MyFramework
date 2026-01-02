
// Excel表格导出的数据基类,表示表格中的一条数据
public class ExcelData
{
	public int mID;		// 每一条数据的唯一ID
	public virtual bool read(SerializerRead reader)
	{
		return reader.read(out mID);
	}
}