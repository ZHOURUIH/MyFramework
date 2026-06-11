#include "AllHeader.h"

enum
{
	ID_TIMER,
	ID_UNDO,
	ID_REDO,
	ID_ADD_ROW_TO_FIRST,
	ID_ADD_ROW_TO_END,
	ID_CONVERT_TABLE,
	ID_AUTO_ID,
	ID_FIXED_ITEM_NAME,
	ID_ADD_ROW_BUTTON,
};

BEGIN_EVENT_TABLE(EditorFrame, wxFrame)

EVT_TIMER(ID_TIMER, OnTimer)
EVT_TOOL(ID_UNDO, EditorFrame::OnUndo)
EVT_TOOL(ID_REDO, EditorFrame::OnRedo)
EVT_TOOL(ID_ADD_ROW_TO_FIRST, EditorFrame::OnAddRowToFirst)
EVT_TOOL(ID_ADD_ROW_TO_END, EditorFrame::OnAddRowToEnd)
EVT_TOOL(ID_CONVERT_TABLE, EditorFrame::OnConvertTable)
EVT_TOOL(ID_AUTO_ID, EditorFrame::OnAutoID)
EVT_TOOL(ID_FIXED_ITEM_NAME, EditorFrame::OnFixedItemName)
EVT_BUTTON(ID_ADD_ROW_BUTTON, EditorFrame::OnAddRowButton)
EVT_CLOSE(OnCloseWindow)

END_EVENT_TABLE()

EditorFrame::EditorFrame(wxString title, wxSize size):
	wxFrame(nullptr, wxID_ANY, title, wxDefaultPosition, size)
{
	mEditorFrame = this;
	mCSVFile->setDirtyCallback(onDirty);
	mUndoManager->setUndoChangeCallback(onUndoChanged);
}

void EditorFrame::init()
{
	setup();
	mTimer = new wxTimer();
	mTimer->Start(1);
	mTimer->SetOwner(this, ID_TIMER);
	parseConfig("./CSVEditorConfig.txt");
}

void EditorFrame::parseConfig(const string& filePath)
{
	Vector<string> lines;
	if (!openTxtFileLines(filePath, lines, true))
	{
		return;
	}
	for (const string& line : lines)
	{
		ArrayList<2, string> params;
		if (!splitFull(line.c_str(), "=", params))
		{
			continue;
		}
		const string& paramName = params[0];
		const string& paramValue = params[1];
		if (paramName == "CSVPath")
		{
			mCSVPath = paramValue;
			if (!endWith(mCSVPath, "/"))
			{
				mCSVPath += "/";
			}
		}
		else if (paramName == "CSVToBinaryExePath")
		{
			mCSVToBinaryExePath = paramValue;
		}
	}
}

void EditorFrame::destroy()
{
	if (mTimer != nullptr)
	{
		delete mTimer;
		mTimer = nullptr;
	}
	if (mMainListWindow != nullptr)
	{
		delete mMainListWindow;
		mMainListWindow = nullptr;
	}
	mAuiManager.UnInit();
}

void EditorFrame::setup()
{
	// 创建编辑核心
	CreateEditorCore();

	mAuiManager.SetManagedWindow(this);
	// 创建菜单栏
	CreateMenu();
	// 创建工具栏
	CreateToolBar();
	// 创建各个子窗口
	CreateWindows();
	// 创建底部的状态显示栏
	CreateStatusBar();

	// 创建完后刷新一遍全部控件的选中状态
	RefreshAllMenuToolCheckState();
}

void EditorFrame::CreateMenu()
{
	wxMenuBar* menuBar = new wxMenuBar();

	// 文件菜单
	wxMenu* fileMenu = new wxMenu;
	fileMenu->Append(wxID_OPEN, "打开\tCtrl+O", "Open a file");
	fileMenu->Append(wxID_NEW, "新建\tCtrl+N", "New a file");
	fileMenu->Append(wxID_SAVE, "保存\tCtrl+S", "Save a file");
	fileMenu->Append(ID_CONVERT_TABLE, "转换为二进制", "convert to binary");
	fileMenu->Append(wxID_EXIT, "关闭\tAlt+F4", "Quit the program");
	menuBar->Append(fileMenu, "文件");

	Bind(wxEVT_MENU, &EditorFrame::OnOpenFile, this, wxID_OPEN);
	Bind(wxEVT_MENU, &EditorFrame::OnNewFile, this, wxID_NEW);
	Bind(wxEVT_MENU, &EditorFrame::OnSaveFile, this, wxID_SAVE);
	Bind(wxEVT_MENU, &EditorFrame::OnConvertTable, this, ID_CONVERT_TABLE);
	Bind(wxEVT_MENU, &EditorFrame::OnExit, this, wxID_EXIT);

	// 编辑菜单
	wxMenu* editMenu = new wxMenu;
	editMenu->Append(wxID_COPY, "复制\tCtrl+C", "copy");
	editMenu->Append(wxID_PASTE, "粘贴\tCtrl+V", "paste");
	editMenu->Append(ID_UNDO, "撤销\tCtrl+Z", "undo");
	editMenu->Append(ID_REDO, "重做\tCtrl+Y", "redo");
	menuBar->Append(editMenu, "编辑");

	Bind(wxEVT_MENU, &EditorFrame::OnCopy, this, wxID_COPY);
	Bind(wxEVT_MENU, &EditorFrame::OnPaste, this, wxID_PASTE);
	Bind(wxEVT_MENU, &EditorFrame::OnUndo, this, ID_UNDO);
	Bind(wxEVT_MENU, &EditorFrame::OnRedo, this, ID_REDO);

	SetMenuBar(menuBar);
}

void EditorFrame::CreateToolBar()
{
	wxToolBar* toolbar = wxFrame::CreateToolBar(wxTB_HORIZONTAL | wxTB_FLAT);
	toolbar->AddTool(ID_UNDO, "撤销", wxArtProvider::GetBitmap(wxART_UNDO, wxART_TOOLBAR), wxArtProvider::GetBitmap(wxART_UNDO, wxART_TOOLBAR).ConvertToDisabled(), wxITEM_NORMAL, "撤销上次操作", "撤销上次操作");
	toolbar->AddTool(ID_REDO, "重做", wxArtProvider::GetBitmap(wxART_REDO, wxART_TOOLBAR), wxArtProvider::GetBitmap(wxART_REDO, wxART_TOOLBAR).ConvertToDisabled(), wxITEM_NORMAL, "恢复上次撤销的操作", "恢复上次撤销的操作");
	toolbar->AddTool(ID_ADD_ROW_TO_FIRST, "插入到第一行", wxArtProvider::GetBitmap(wxART_PLUS, wxART_TOOLBAR), "插入到第一行");
	toolbar->AddTool(ID_ADD_ROW_TO_END, "插入到最后一行", wxArtProvider::GetBitmap(wxART_PLUS, wxART_TOOLBAR), "插入到最后一行");
	toolbar->AddTool(ID_CONVERT_TABLE, "转换为二进制", wxArtProvider::GetBitmap(wxART_CDROM, wxART_TOOLBAR), "转换为二进制");
	toolbar->AddTool(ID_AUTO_ID, "自动生成ID", wxArtProvider::GetBitmap(wxART_TICK_MARK, wxART_TOOLBAR), "自动生成ID");
	toolbar->AddTool(ID_FIXED_ITEM_NAME, "自动修复物品名字", wxArtProvider::GetBitmap(wxART_FLOPPY, wxART_TOOLBAR), "自动修复物品名字");
	toolbar->AddSeparator();

	toolbar->AddControl(new wxStaticText(toolbar, wxID_ANY, "添加行:"));
	mRowCountInput = new wxTextCtrl(toolbar, wxID_ANY, "", wxDefaultPosition, wxSize(150, -1), 0, wxTextValidator(wxFILTER_DIGITS));
	toolbar->AddControl(mRowCountInput);
	toolbar->AddControl(new wxButton(toolbar, ID_ADD_ROW_BUTTON, "添加"));
	toolbar->AddSeparator();

	toolbar->Realize();
	mAuiManager.Update();
}

void EditorFrame::CreateWindows()
{
	mMainListWindow = new MainListWindow(this, wxMINIMIZE_BOX | wxMAXIMIZE_BOX);
	mMainListWindow->init();
	mAuiManager.AddPane(mMainListWindow, wxAuiPaneInfo().Name("MainListWindow").BestSize(wxSize(-1, -1)).FloatingSize(400, 600).Caption("列表").Center().Dockable(true));
	mMainListWindow->Show();

	mAuiManager.Update();
}

void EditorFrame::CreateStatusBar()
{
	wxFrame::CreateStatusBar(4);
	int widthList[4] = { 100, 100, 100, -1 };
	SetStatusWidths(4, widthList);
}

void EditorFrame::RefreshAllMenuToolCheckState()
{
	onUndoChanged();
}

void EditorFrame::OnTimer(wxTimerEvent& event)
{
	static unsigned long lastTime = timeGetTime();
	unsigned long curTime = timeGetTime();
	float elapsedTime = (curTime - lastTime) / 1000.0f;
	Update(elapsedTime);
	lastTime = curTime;
	Render();
}

void EditorFrame::Update(float elapsedTime)
{
	KeyProcess();
	UpdateStatus();
	mMainListWindow->update(elapsedTime);
}

void EditorFrame::OnExit(wxCommandEvent& event)
{
	if (mCSVFile->isOpened() && mCSVFile->isDirty())
	{
		const int ret = dialogYesNoCancel("文件未保存,是否保存再退出?", "保存并退出", "不保存且退出", "取消退出");
		if (ret == wxID_YES)
		{
			mCSVFile->save();
		}
		else if (ret == wxID_NO) {}
		else if (ret == wxID_CANCEL)
		{
			return;
		}
	}
	// 发出关闭窗口的事件
	Close(true);
}

void EditorFrame::OnOpenFile(wxCommandEvent& event)
{
	if (mCSVFile->isOpened() && mCSVFile->isDirty())
	{
		const int ret = dialogYesNoCancel("文件未保存,是否保存再退出?", "保存并退出", "不保存且退出", "取消退出");
		if (ret == wxID_YES)
		{
			mCSVFile->save();
		}
		else if (ret == wxID_NO) {}
		else if (ret == wxID_CANCEL)
		{
			return;
		}
	}
	mMainListWindow->openFile();
}

void EditorFrame::OnNewFile(wxCommandEvent& event)
{
	if (mCSVFile->isOpened() && mCSVFile->isDirty())
	{
		const int ret = dialogYesNoCancel("文件未保存,是否保存再退出?", "保存并退出", "不保存且退出", "取消退出");
		if (ret == wxID_YES)
		{
			mCSVFile->save();
		}
		else if (ret == wxID_NO) {}
		else if (ret == wxID_CANCEL)
		{
			return;
		}
	}
	mMainListWindow->newFile();
}

void EditorFrame::OnSaveFile(wxCommandEvent& event)
{
	mMainListWindow->save();
}

void EditorFrame::OnConvertTable(wxCommandEvent& event)
{
	// 路径带空格时的处理方式
	system(("cmd /c \"\"" + mCSVToBinaryExePath + "\"\"").c_str());
}

void EditorFrame::OnAutoID(wxCommandEvent& event)
{
	mMainListWindow->autoSetSelectionID();
}

void EditorFrame::OnCopy(wxCommandEvent& event)
{
	mMainListWindow->copySelection();
}

void EditorFrame::OnPaste(wxCommandEvent& event)
{
	mMainListWindow->pasteSelection();
}

void EditorFrame::OnUndo(wxCommandEvent& event)
{
	if (!mUndoManager->canUndo())
	{
		return;
	}
	mUndoManager->undo();
}

void EditorFrame::OnRedo(wxCommandEvent& event)
{
	if (!mUndoManager->canRedo())
	{
		return;
	}
	mUndoManager->redo();
}

void EditorFrame::OnAddRowToFirst(wxCommandEvent& event)
{
	mMainListWindow->addRowToFirst({});
}

void EditorFrame::OnAddRowToEnd(wxCommandEvent& event)
{
	mMainListWindow->addRowToEnd({});
}

void EditorFrame::OnFixedItemName(wxCommandEvent& event)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	mMainListWindow->fixedItemName();
}

void EditorFrame::OnAddRowButton(wxCommandEvent& event)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	mMainListWindow->addRowBelow(SToI(mRowCountInput->GetValue().ToStdString()));
}

void EditorFrame::OnCloseWindow(wxCloseEvent& WXUNUSED(event))
{
	if (mCSVFile->isOpened() && mCSVFile->isDirty())
	{
		const int ret = dialogYesNoCancel("文件未保存,是否保存再退出?", "保存并退出", "不保存且退出", "取消退出");
		if (ret == wxID_YES)
		{
			mCSVFile->save();
		}
		else if (ret == wxID_NO) {}
		else if (ret == wxID_CANCEL)
		{
			return;
		}
	}
	// 销毁自己的数据
	destroy();
	// 销毁窗口
	Destroy();
}

void EditorFrame::onDirty()
{
	string title;
	if (mCSVFile->isOpened())
	{
		title += getFileNameNoSuffix(mCSVFile->getFilePath(), true);
		title += " " + mCSVFile->getFilePath();
		title += " " + mCSVFile->getTableName();
		if (mCSVFile->isDirty())
		{
			title += " *";
		}
	}
	else
	{
		title = "CSVEditor";
	}
	mEditorFrame->SetTitle(title);
}

void EditorFrame::onUndoChanged()
{
	// 获取工具栏控件
	wxToolBar* toolbar = mEditorFrame->GetToolBar();
	if (toolbar != nullptr)
	{
		toolbar->EnableTool(ID_UNDO, mUndoManager->canUndo());
		toolbar->EnableTool(ID_REDO, mUndoManager->canRedo());
	}
	// 获取菜单栏控件
	wxMenuBar* menuBar = mEditorFrame->GetMenuBar();
	if (menuBar != nullptr)
	{
		menuBar->Enable(ID_UNDO, mUndoManager->canUndo());
		menuBar->Enable(ID_REDO, mUndoManager->canRedo());
	}
}

void EditorFrame::UpdateStatus()
{
	if (mCSVFile->isOpened())
	{
		setStatusText("共" + IToS(mCSVFile->getRowCount()) + "行", 0);
		setStatusText("已显示" + IToS(mMainListWindow->getShowRowCount()) + "行", 1);
		setStatusText("已选中" + IToS(mMainListWindow->getSelectedRows().size()) + "行", 2);

		string rowDesc;
		const int selectRow = mMainListWindow->getFirstSelectRow();
		const int selectCol = mMainListWindow->getFirstSelectCol();
		if (selectRow == EditorDefine::ROW_TABLE_NAME && selectCol == 0)
		{
			rowDesc = "填写的是表格名字";
		}
		else if (selectRow == EditorDefine::ROW_TABLE_OWNER && selectCol == 0)
		{
			rowDesc = "填写的是表格所属,None,Client,Server,Both";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_NAME)
		{
			rowDesc = "填写字段的名字";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_TYPE)
		{
			rowDesc = "填写字段的类型,命名风格为C++,不支持结构体,如果是枚举,则后面需要加上具体的类型,比如TYPE(byte)";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_OWNER)
		{
			rowDesc = "填写字段所属,None,Client,Server,Both";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_COMMENT)
		{
			rowDesc = "填写字段的注释说明文本";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_LINK_TABLE)
		{
			rowDesc = "填写字段索引到的表格名字,如果有索引时,则当前列只能填其他表格的ID或者ID列表";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_LINK_LENGTH)
		{
			rowDesc = "字段长度的名字,不一定与字段名一样,只是作为一个标识,用于约束两个字段的列表长度必须一致";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_FLAG)
		{
			rowDesc = "填写字段的类型描述,目前支持的标签有Path,ItemName,PropertyName,EquipTypeName";
		}
		else if (selectRow == EditorDefine::ROW_COLUMN_FILTER)
		{
			rowDesc = "用于过滤显示数据";
		}
		setStatusText(rowDesc, 3);
	}
}

void EditorFrame::setStatusText(const string& text, int index)
{
	if (GetStatusBar()->GetStatusText(index) != text)
	{
		SetStatusText(text, index);
	}
}