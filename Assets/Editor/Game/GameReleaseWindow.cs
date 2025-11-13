using UnityEditor;
using UnityEngine;
using static FrameBaseUtility;
using static StringUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;

// 以下代码为示例代码,需要根据自己项目的需求进行调整
public class GameReleaseWindow : GameEditorWindow
{
	protected PlatformInfo mPlatform;           // 平台逻辑实例
	protected bool mAutoVersion = true;         // 是否在打包时自动生成版本号
	protected bool mAutoUploadVersion = false;  // 是否在上传资源后自动更新版本号
	protected bool mNeedCheckVersion = true;    // 打包时是否需要检查版本号是按照规则递增,避免生成错误的版本号,为了有时候方便调试,允许打包时使用之前的版本号
	protected Vector2 mScrollPos;
	public void start()
	{
		Show();
		minSize = new(550, 950);
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected override void onGUI()
	{
		if (mPlatform == null)
		{
			createPlatform();
		}

		using var a = new GUILayout.ScrollViewScope(mScrollPos);
		mScrollPos = a.scrollPosition;
		label("当前平台:" + mPlatform.mName, 25);
		space(30);

		// 版本号
		using (new GUILayout.VerticalScope())
		{
			using (new GUILayout.HorizontalScope())
			{
				label((mPlatform.mTestClient ? "远端测试版本号:" : "远端正式版本号:") + mPlatform.mRemoteVersion);
				if (button("更新", 60, 25))
				{
					mPlatform.updateRemoteVersion();
				}
			}

			using (new GUILayout.VerticalScope())
			{
				label("本地版本号:" + mPlatform.mLocalVersion);
			}
			toggle(ref mAutoVersion, "自动生成打包版本号");
			if (!mAutoVersion)
			{
				using (new GUILayout.HorizontalScope())
				{
					label("打包版本号:");
					bool modified = false;
					for (int i = 0; i < mPlatform.mVersionNumber.Length; ++i)
					{
						modified |= textField(ref mPlatform.mVersionNumber[i]);
					}
					if (modified)
					{
						mPlatform.mBuildVersion = stringsToString(mPlatform.mVersionNumber, '.');
					}
				}
			}
			toggle(ref mNeedCheckVersion, "检查版本号是否正确");
		}

		space(30);

		// 客户端选项
		using (new GUILayout.VerticalScope())
		{
			// 仅安卓平台需要选择要上架的游戏渠道
			if (isAndroid() && displayEnum("上架渠道:", "选择要上架的渠道", ref mPlatform.mGameChannel))
			{
				// 更改游戏渠道后,需要刷新一次版本号
				mPlatform.updateRemoteVersion();
				mPlatform.generateFolderPreName();
				PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), mPlatform.getDefaultPlatformDefine());
				AssetDatabase.Refresh();
			}
			if (toggle(ref mPlatform.mTestClient, "测试客户端"))
			{
				mPlatform.generateFolderPreName();
				mPlatform.updateRemoteVersion();
				if (!mPlatform.mTestClient)
				{
					mPlatform.mEnableHotFix = !isWebGL();
				}
				PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), mPlatform.getDefaultPlatformDefine());
				AssetDatabase.Refresh();
			}

			label("远端路径:" + ObsSystem.getURL() + mPlatform.getRemoteFolderInEditor(""));
			label("输出路径:" + mPlatform.mOutputPath);
			label("输出文件夹前缀: " + mPlatform.mFolderPreName);
			if (button("ProjectSettings", 130))
			{
				GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectSettingsWindow")).Show();
			}
			label("当前宏定义:");
			string def = PlayerSettings.GetScriptingDefineSymbols(getNameBuildTarget());
			foreach (string line in split(def, ";"))
			{
				label(line);
			}
			if (button("还原宏定义", 120))
			{
				PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), mPlatform.getDefaultPlatformDefine());
				AssetDatabase.Refresh();
			}
		}

		// 打包操作
		using (new GUILayout.VerticalScope())
		{
			space(30);
			if (mPlatform.mTestClient && !isWebGL())
			{
				if (toggle(ref mPlatform.mEnableHotFix, "启用热更"))
				{
					PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), mPlatform.getDefaultPlatformDefine());
					AssetDatabase.Refresh();
					mPlatform.generateFolderPreName();
				}
			}
			label("一键打包");
			string packPreTip = "打包前需要确认两件事情:\n1.已经将项目更新到最新.\n2.已经更新了表格文件.";
			if (mPlatform.mEnableHotFix)
			{
				bool isMinVersionGreater = mPlatform.isMinVersionGreater();
				// 小版本的热更打包
				if (button("热更新,打包AB+热更dll+上传", "小版本的热更打包,会执行打包AB,更新热更dll,更新版本号,更新文件列表,上传资源文件到服务器", 200, 30))
				{
					// 需要大版本号与远端一致,小版本号大于远端小版本号
					if (!mNeedCheckVersion || mPlatform.mRemoteVersion.isEmpty() || isMinVersionGreater || mAutoVersion)
					{
						string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
						if (mAutoVersion)
						{
							buildVersion = mPlatform.generateSubVersion();
							packPreTip += "\n自动生成的版本号为:" + buildVersion;
						}
						_ = messageYesNo(packPreTip) &&
							setBuildVersion(buildVersion) &&
							MenuAssetBundle.packAssetBundle(mPlatform.mTarget, fullPathToProjectPath(mPlatform.mAssetBundleFullPath), false) &&
							mPlatform.buildHotFix(false) &&
							mPlatform.writeVersion() &&
							mPlatform.writeFileList(mPlatform.mAssetBundleFullPath) &&
							mPlatform.upload(mAutoUploadVersion);
					}
					else
					{
						messageOK("热更新打包时,需要大版本号与远端一致,且小版本号大于远端小版本号");
					}
				}
				if (button("热更新,热更dll+上传", "小版本的热更打包,会执行更新热更dll,更新版本号,更新文件列表,上传资源文件到服务器", 200, 30))
				{
					// 需要大版本号与远端一致,小版本号大于远端小版本号
					if (!mNeedCheckVersion || mPlatform.mRemoteVersion.isEmpty() || isMinVersionGreater || mAutoVersion)
					{
						string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
						if (mAutoVersion)
						{
							buildVersion = mPlatform.generateSubVersion();
							packPreTip += "\n自动生成的版本号为:" + buildVersion;
						}
						_ = messageYesNo(packPreTip) &&
							setBuildVersion(buildVersion) &&
							mPlatform.buildHotFix(false) &&
							mPlatform.writeVersion() &&
							mPlatform.writeFileList(mPlatform.mAssetBundleFullPath) &&
							mPlatform.upload(mAutoUploadVersion);
					}
					else
					{
						messageOK("热更新打包时,需要大版本号与远端一致,且小版本号大于远端小版本号");
					}
				}
				if (button("热更新,热更dll", "小版本的热更打包,会执行更新热更dll,更新版本号,更新文件列表", 200, 30))
				{
					// 需要大版本号与远端一致,小版本号大于远端小版本号
					if (!mNeedCheckVersion || mPlatform.mRemoteVersion.isEmpty() || isMinVersionGreater || mAutoVersion)
					{
						string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
						if (mAutoVersion)
						{
							buildVersion = mPlatform.generateSubVersion();
							packPreTip += "\n自动生成的版本号为:" + buildVersion;
						}
						_ = messageYesNo(packPreTip) &&
							setBuildVersion(buildVersion) &&
							mPlatform.buildHotFix(false) &&
							mPlatform.writeVersion() &&
							mPlatform.writeFileList(mPlatform.mAssetBundleFullPath);
					}
					else
					{
						messageOK("热更新打包时,需要大版本号与远端一致,且小版本号大于远端小版本号");
					}
				}

				long localBigVersion = SToL(mPlatform.mVersionNumber[0]) * 1000000000 + SToL(mPlatform.mVersionNumber[1]);
				long remoteBigVersion = PlatformBase.getVersionPart(mPlatform.mRemoteVersion, 0) * 1000000000 + PlatformBase.getVersionPart(mPlatform.mRemoteVersion, 1);
				// 大版本更新打包
				if (button("大版本更新,打包AB+打包程序+上传", "大版本更新打包,会执行打包AB,构建xcode工程或生成apk,并且上传StreamingAssets资源", 200, 30))
				{
					// 需要大版本号大于远端
					if (!mNeedCheckVersion || localBigVersion > remoteBigVersion || mAutoVersion)
					{
						string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
						if (mAutoVersion)
						{
							buildVersion = mPlatform.generateMainVersion();
							packPreTip += "\n自动生成的版本号为:" + buildVersion;
						}
						_ = messageYesNo(packPreTip) &&
							setBuildVersion(buildVersion) &&
							MenuAssetBundle.packAssetBundle(mPlatform.mTarget, fullPathToProjectPath(mPlatform.mAssetBundleFullPath), false) &&
							mPlatform.build(true, false) &&
							mPlatform.upload(mAutoUploadVersion);
					}
					else
					{
						messageOK("大版本更新打包时,需要大版本号大于远端");
					}
				}
				if (button("大版本更新,打包程序+上传", "大版本更新打包,会构建xcode工程或生成apk,并且上传StreamingAssets资源", 200, 30))
				{
					// 需要大版本号大于远端
					if (!mNeedCheckVersion || localBigVersion > remoteBigVersion || mAutoVersion)
					{
						string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
						if (mAutoVersion)
						{
							buildVersion = mPlatform.generateMainVersion();
							packPreTip += "\n自动生成的版本号为:" + buildVersion;
						}
						_ = messageYesNo(packPreTip) &&
							setBuildVersion(buildVersion) &&
							mPlatform.build(true, false) &&
							// 这里可以直接上传,当大版本更新不影响网络消息时,可以不用更新也能继续运行游戏
							// 如果网络消息不兼容,则需要手动去更新大版本
							mPlatform.upload(mAutoUploadVersion);
					}
					else
					{
						messageOK("大版本更新打包时,需要大版本号大于远端");
					}
				}
			}
			else
			{
				if (button("打包AB+生成程序", "打包AB,生成可执行程序", 150, 30))
				{
					string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
					if (mAutoVersion)
					{
						buildVersion = mPlatform.generateMainVersion();
						packPreTip += "\n自动生成的版本号为:" + buildVersion;
					}
					_ = messageYesNo(packPreTip) &&
						setBuildVersion(buildVersion) &&
						MenuAssetBundle.packAssetBundle(mPlatform.mTarget, fullPathToProjectPath(mPlatform.mAssetBundleFullPath), false) &&
						mPlatform.build(true, false) &&
						mAutoUploadVersion &&
						mPlatform.uploadVersion();
				}
				if (button("生成程序", "生成可执行程序", 120, 30))
				{
					string buildVersion = stringsToString(mPlatform.mVersionNumber, '.');
					if (mAutoVersion)
					{
						buildVersion = mPlatform.generateMainVersion();
						packPreTip += "\n自动生成的版本号为:" + buildVersion;
					}
					_ = messageYesNo(packPreTip) &&
						setBuildVersion(buildVersion) &&
						mPlatform.build(true, false) &&
						mAutoUploadVersion &&
						mPlatform.uploadVersion();
				}
			}
			space(10);
			toggle(ref mAutoUploadVersion, "自动上传版本号");
			if (!mAutoUploadVersion)
			{
				if (button("上传版本号", 150, 30))
				{
					string tip = "是否要将远端的版本号更新为" + mPlatform.mLocalVersion;
					_ = messageYesNo(tip) &&
						mPlatform.uploadVersion();
				}
			}

			space(30);

			if (button("更新AssetBundle", 120, 30))
			{
				MenuAssetBundle.packAssetBundle(mPlatform.mTarget, fullPathToProjectPath(mPlatform.mAssetBundleFullPath), false);
			}
			if (mPlatform.mEnableHotFix)
			{
				if (mPlatform.mTestClient)
				{
					if (button("上传测试资源", "Windows上会上传指定版本的启动器文件,其余文件从StreamingAssets中上传,其他平台都是从StreamingAsset中上传", 100, 35) &&
						messageYesNo("确认上传版本号为" + mPlatform.mLocalVersion + "的资源?"))
					{
						_ = mPlatform.writeFileList(mPlatform.mAssetBundleFullPath) &&
							mPlatform.upload(mAutoUploadVersion);
					}
				}
				else
				{
					if (button("上传正式资源", 100, 35) && messageYesNo("确认上传版本号为" + mPlatform.mLocalVersion + "的资源?"))
					{
						_ = mPlatform.writeFileList(mPlatform.mAssetBundleFullPath) &&
							mPlatform.upload(mAutoUploadVersion);
					}
				}
			}

			space(30);
			label("测试打包");
			if (button("测试打包过程", 100, 35))
			{
				mPlatform.build(false, false);
			}
			if (isAndroid() && button("导出安卓工程", 100, 35))
			{
				mPlatform.build(false, true);
			}
		}
	}
	protected void createPlatform()
	{
		Debug.Log("create platform:" + getBuildTarget());
		mPlatform = PlatformInfo.create();
		mPlatform.mIgnoreFile = new() { VERSION, FILE_LIST, FILE_LIST_MD5, mPlatform.mName, mPlatform.mName + ".manifest" };
		mPlatform.mGameChannel = GAME_CHANNEL.NONE;
		mPlatform.mTestClient = true;
		mPlatform.mEnableHotFix = !isWebGL();
		mPlatform.generateFolderPreName();
		mPlatform.updateRemoteVersion();
		mPlatform.updateLocalVersion();
		setBuildVersion(stringsToString(mPlatform.mVersionNumber, '.'));
		mNeedCheckVersion = true;
	}
	protected bool setBuildVersion(string version)
	{
		mPlatform.mBuildVersion = version;
		return true;
	}
}