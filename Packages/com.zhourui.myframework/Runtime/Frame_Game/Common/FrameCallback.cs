
// 游戏委托定义
public delegate void GameLayoutCallback(GameLayout layout);
public delegate void DownloadFileListCallback(StringCallback callback);
public delegate void GameDownloadCallback(float progress, PROGRESS_TYPE type, string info, int bytesPerSecond, int downloadRemainSeconds);
public delegate void GameDownloadTipCallback(DOWNLOAD_TIP type);