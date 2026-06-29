using static StringUtility;

public struct RemoteFileInfo
{
	public string mReadType;
	public int mLinkCount;
	public string mUserName;
	public string mGroupName;
	public long mFileSize;
	public string mModifyMonth;
	public string mModifyDay;
	public string mModifyTime;
	public string mFileName;
	public GameFileInfo toGameFileInfo()
	{
		GameFileInfo info = new();
		info.mFileName = mFileName;
		info.mFileSize = mFileSize;
		return info;
	}
	public static RemoteFileInfo parse(string[] infos)
	{
		RemoteFileInfo fileInfo = new();
		fileInfo.mReadType = infos[0];
		fileInfo.mLinkCount = SToI(infos[1]);
		fileInfo.mUserName = infos[2];
		fileInfo.mGroupName = infos[3];
		fileInfo.mFileSize = SToL(infos[4]);
		fileInfo.mModifyMonth = infos[5];
		fileInfo.mModifyDay = infos[6];
		fileInfo.mModifyTime = infos[7];
		fileInfo.mFileName = infos[8];
		if (infos.Length > 9)
		{
			int remainCount = infos.Length - 9;
			for (int i = 0; i < remainCount; ++i)
			{
				fileInfo.mFileName += " " + infos[i + 9];
			}
		}
		// 如果是通过sshpass获得的列表,且文件名中带空格的,会自动被加上',需要将'去除
		fileInfo.mFileName = fileInfo.mFileName.removeAll('\'');
		return fileInfo;
	}
}