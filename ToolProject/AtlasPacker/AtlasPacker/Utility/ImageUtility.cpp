#include "txSerializer.h"
#include "ImageUtility.h"

bool ImageUtility::texturePackerAll(const string& texturePath, const string& atlasPath)
{
	bool success = true;
	cout << "开始执行cmd,如果长时间卡住不动,可能是第一次安装以后需要手动进行一次验证,使用cmd运行TexturePacker.exe,然后根据提示输入agree,然后再重新尝试打图集" << endl;
	myVector<string> folderList;
	folderList.push_back(texturePath);
	findFolders(texturePath, folderList, true);
	FOR_VECTOR(folderList)
	{
		myVector<string> temp;
		findFiles(folderList[i], temp, ".png", false);
		if (temp.size() > 0)
		{
			string fileNameNoSuffix = removeFirstPath(folderList[i]);
			string newFullPath = atlasPath;
			if (newFullPath.empty())
			{
				newFullPath = getFileName(folderList[i]) + "_atlas";
			}
			else
			{
				deleteFile(newFullPath + "/" + fileNameNoSuffix + ".png");
				deleteFile(newFullPath + "/" + fileNameNoSuffix + ".png.meta");
				deleteFile(newFullPath + "/" + fileNameNoSuffix + ".tpsheet");
				deleteFile(newFullPath + "/" + fileNameNoSuffix + ".tpsheet.meta");
			}
			packAtlas(newFullPath, fileNameNoSuffix, folderList[i]);
			if (isFileExist(newFullPath + "/" + fileNameNoSuffix + ".png"))
			{
				cout << "已打包:" << i + 1 << "/" << folderListCount << ",打包成功" << endl;
			}
			else
			{
				cout << "已打包:" << i + 1 << "/" << folderListCount << ",打包失败,path:" << folderList[i] << endl;
				success = false;
			}
		}
		else
		{
			cout << "跳过空目录" << endl;
		}
	}
	END(folderList);
	return success;
}

void ImageUtility::packAtlas(const string& outputPath, const string& outputFileName, const string& sourcePath)
{
	createFolder(outputPath);
	string cmdLine;
	cmdLine += "--data " + outputPath + "/" + outputFileName + ".tpsheet ";
	cmdLine += "--sheet " + outputPath + "/" + outputFileName + ".png ";
	cmdLine += "--format unity-texture2d ";
	cmdLine += "--alpha-handling KeepTransparentPixels ";
	cmdLine += "--maxrects-heuristics Best ";
	cmdLine += "--disable-rotation ";
	cmdLine += "--size-constraints POT ";
	cmdLine += "--max-size 2048 ";
	cmdLine += "--trim-mode None ";
	cmdLine += "--extrude 0 ";
	cmdLine += "--shape-padding 1 ";
	cmdLine += "--border-padding 1 ";
	cmdLine += sourcePath;

	SHELLEXECUTEINFOA ShExecInfo = { 0 };
	ShExecInfo.cbSize = sizeof(SHELLEXECUTEINFOA);
	ShExecInfo.fMask = SEE_MASK_NOCLOSEPROCESS;
	ShExecInfo.hwnd = NULL;
	ShExecInfo.lpVerb = NULL;
	ShExecInfo.lpFile = "C:\\Program Files\\CodeAndWeb\\TexturePacker\\bin\\TexturePacker.exe";
	ShExecInfo.lpParameters = cmdLine.c_str();
	ShExecInfo.lpDirectory = NULL;
	ShExecInfo.nShow = SW_HIDE;
	ShExecInfo.hInstApp = NULL;
	BOOL ret = ShellExecuteExA(&ShExecInfo);
	if (ShExecInfo.hProcess != 0)
	{
		WaitForSingleObject(ShExecInfo.hProcess, INFINITE);
	}
}