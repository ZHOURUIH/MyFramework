using System.Text;

// 表示一个文件的信息
public class GameFileInfo
{
	public string mFileName;        // StreamingAssets下的相对路径
	public long mFileSize;          // 文件大小
	public string mMD5;				// 文件MD5
	public static GameFileInfo createInfo(string infoString)
	{
		string[] list = infoString.Split('\t');
		if (list == null || list.Length != 3)
		{
			return default;
		}
		GameFileInfo info = new()
		{
			mFileName = list[0],
			mFileSize = int.Parse(list[1]),
			mMD5 = list[2]
		};
		return info;
	}
	public void toString(StringBuilder builder)
	{
		builder.Append(mFileName);
		builder.Append('\t');
		builder.Append(mFileSize);
		builder.Append('\t');
		builder.Append(mMD5);
	}
}