#include "AllHeader.h"

enum
{
	ID_DELETE_COL,
	ID_DELETE_ROW,
	ID_ADD_COL_1_RIGHT,
	ID_ADD_COL_5_RIGHT,
	ID_COL_MOVE_LEFT,
	ID_COL_MOVE_RIGHT,
	ID_ADD_ROW_1_ABOVE,
	ID_ADD_ROW_10_ABOVE,
	ID_ADD_ROW_100_ABOVE,
	ID_ADD_ROW_1_BELOW,
	ID_ADD_ROW_10_BELOW,
	ID_ADD_ROW_100_BELOW,
	ID_SELECT_ROW_TO_FIRST,
	ID_SELECT_ROW_TO_END,
	ID_FILL_ROW_SEQUENCE,
	ID_MOVE_UP,
	ID_MOVE_DOWN,
	ID_FREEZE_COL,
	ID_UNFREEZE_COL,
	ID_AUTO_ID,
	ID_FOCUS_FIRST_ROW,
	ID_FOCUS_END_ROW,
};

BEGIN_EVENT_TABLE(MainListWindow, wxPanel)
EVT_GRID_LABEL_RIGHT_CLICK(MainListWindow::OnGridLabelRightClick)
EVT_GRID_CELL_RIGHT_CLICK(MainListWindow::OnGridCellRightClick)
EVT_MENU(ID_DELETE_COL, MainListWindow::OnDeleteColumn)
EVT_MENU(ID_DELETE_ROW, MainListWindow::OnDeleteRow)
EVT_MENU(ID_ADD_COL_1_RIGHT, MainListWindow::OnAddColumn1Right)
EVT_MENU(ID_ADD_COL_5_RIGHT, MainListWindow::OnAddColumn5Right)
EVT_MENU(ID_COL_MOVE_LEFT, MainListWindow::OnColumnMoveLeft)
EVT_MENU(ID_COL_MOVE_RIGHT, MainListWindow::OnColumnMoveRight)
EVT_MENU(ID_ADD_ROW_1_ABOVE, MainListWindow::OnAddRow1Above)
EVT_MENU(ID_ADD_ROW_10_ABOVE, MainListWindow::OnAddRow10Above)
EVT_MENU(ID_ADD_ROW_100_ABOVE, MainListWindow::OnAddRow100Above)
EVT_MENU(ID_ADD_ROW_1_BELOW, MainListWindow::OnAddRow1Below)
EVT_MENU(ID_ADD_ROW_10_BELOW, MainListWindow::OnAddRow10Below)
EVT_MENU(ID_ADD_ROW_100_BELOW, MainListWindow::OnAddRow100Below)
EVT_MENU(ID_SELECT_ROW_TO_FIRST, MainListWindow::OnSelectRowToFirst)
EVT_MENU(ID_SELECT_ROW_TO_END, MainListWindow::OnSelectRowToEnd)
EVT_MENU(ID_FILL_ROW_SEQUENCE, MainListWindow::OnFillRowSequence)
EVT_MENU(ID_MOVE_UP, MainListWindow::OnMoveUp)
EVT_MENU(ID_MOVE_DOWN, MainListWindow::OnMoveDown)
EVT_MENU(ID_FREEZE_COL, MainListWindow::OnFreezeColumn)
EVT_MENU(ID_UNFREEZE_COL, MainListWindow::OnUnFreezeColumn)
EVT_MENU(ID_AUTO_ID, MainListWindow::OnAutoID)
EVT_MENU(ID_FOCUS_FIRST_ROW, MainListWindow::OnFocusFirstRow)
EVT_MENU(ID_FOCUS_END_ROW, MainListWindow::OnFocusEndRow)
END_EVENT_TABLE()

MainListWindow::MainListWindow(wxWindow* parent, long style)
:wxPanel(parent, wxID_ANY, wxDefaultPosition, wxDefaultSize, style)
{
	mMainListWindow = this;
	SetSizeHints(wxDefaultSize, wxDefaultSize);

	wxBoxSizer* sizer1 = new wxBoxSizer(wxVERTICAL);

	mEditViewText = new wxTextCtrl(this, wxID_ANY, "", wxDefaultPosition, wxSize(-1, 60), wxTE_MULTILINE | wxTE_WORDWRAP | wxTE_PROCESS_ENTER);
	sizer1->Add(mEditViewText, 0, wxEXPAND | wxALL, 5);

	mGrid = new wxGrid(this, wxID_ANY, wxDefaultPosition, wxDefaultSize, 0);

	// Grid
	mGrid->CreateGrid(0, 0);
	mGrid->EnableEditing(true);
	mGrid->EnableGridLines(true);
	mGrid->EnableDragGridSize(false);
	mGrid->SetMargins(0, 0);
	mGrid->SetSelectionMode(wxGrid::wxGridSelectCells);

	// Columns
	mGrid->EnableDragColMove(false);
	mGrid->EnableDragColSize(true);
	mGrid->SetColLabelAlignment(wxALIGN_CENTER, wxALIGN_CENTER);

	// Rows
	mGrid->EnableDragRowSize(false);
	mGrid->SetRowLabelAlignment(wxALIGN_CENTER, wxALIGN_CENTER);

	// Cell Defaults
	mGrid->SetDefaultCellAlignment(wxALIGN_LEFT, wxALIGN_TOP);
	sizer1->Add(mGrid, 1, wxALL | wxEXPAND, 5);
	SetSizer(sizer1);
	Layout();
	Centre(wxBOTH);

	mEditViewText->Bind(wxEVT_TEXT, &MainListWindow::OnEditViewTextChanged, this);
	mGrid->Bind(wxEVT_GRID_CELL_CHANGED, &MainListWindow::OnCellChanged, this);
	mGrid->Bind(wxEVT_GRID_SELECT_CELL, &MainListWindow::OnCellSelected, this);
	mGrid->Bind(wxEVT_GRID_EDITOR_CREATED, &MainListWindow::OnGridEditorCreated, this);
	mGrid->Bind(wxEVT_KEY_DOWN, &MainListWindow::OnKeyDown, this);
	mGrid->Bind(wxEVT_GRID_CELL_LEFT_CLICK, &MainListWindow::OnGridLeftClick, this);
}

void MainListWindow::initData(CSVFile* table)
{
	const Vector<ColumnData*>& headerList = table->getHeaderDataList();
	if (mGrid->GetNumberCols() > 0)
	{
		mGrid->DeleteCols(0, mGrid->GetNumberCols());
	}
	if (mGrid->GetNumberRows() > 0)
	{
		mGrid->DeleteRows(0, mGrid->GetNumberRows());
	}
	mGrid->AppendCols(headerList.size());
	mGrid->AppendRows(EditorDefine::HEADER_ROW);
	mGrid->SetRowLabelValue(EditorDefine::ROW_TABLE_NAME, "表格名");
	mGrid->SetRowLabelValue(EditorDefine::ROW_TABLE_OWNER, "表格所属");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_NAME, "字段名");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_TYPE, "字段类型");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_OWNER, "字段所属");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_COMMENT, "字段注释");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_LINK_TABLE, "字段链接表");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_LINK_LENGTH, "长度链接名");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_FLAG, "字段标签");
	mGrid->SetRowLabelValue(EditorDefine::ROW_COLUMN_FILTER, "过滤");
	// 表名
	mGrid->SetCellValue(EditorDefine::ROW_TABLE_NAME, 0, table->getTableName());
	// 表所属
	mGrid->SetCellValue(EditorDefine::ROW_TABLE_OWNER, 0, getOwnerString(table->getTableOwner()));
	mFilterValueList.clear();
	// 前几行是列信息
	FOR_VECTOR(headerList)
	{
		ColumnData* headerData = headerList[i];
		mGrid->SetColLabelValue(i, headerData->mName);
		// 字段名字
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_NAME, i, headerData->mName);
		// 字段类型
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_TYPE, i, headerData->mType);
		// 字段所属
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_OWNER, i, getOwnerString(headerData->mOwner));
		// 字段注释
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_COMMENT, i, headerData->mComment);
		// 字段链接表
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_LINK_TABLE, i, headerData->mLinkTable);
		// 字段长度链接
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_LINK_LENGTH, i, headerData->mLinkLength);
		// 字段标签
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_FLAG, i, headerData->mFlag);
		mFilterValueList.push_back();
	}

	// 表头需要全部都设置成黄色背景,过滤行为灰色
	FOR(EditorDefine::HEADER_ROW)
	{
		FOR_J(headerList.size())
		{
			if (i < 2 && j > 0)
			{
				mGrid->SetCellBackgroundColour(i, j, *wxLIGHT_GREY);
				mGrid->SetReadOnly(i, j, true);
			}
			else
			{
				mGrid->SetCellBackgroundColour(i, j, i == EditorDefine::ROW_COLUMN_FILTER ? *wxLIGHT_GREY : *wxYELLOW);
			}
		}
	}
	// 固定冻结表头行和ID列
	mGrid->FreezeTo(EditorDefine::HEADER_ROW, 1);

	showData(table);
	autoAdjustColumnWidthByHeader();
}

void MainListWindow::showData(CSVFile* table)
{
	const Vector<Vector<GridData*>>& dataList = table->getAllGrid();
	bool hasFilter = false;
	FOR_VECTOR(mFilterValueList)
	{
		if (!mFilterValueList[i].empty())
		{
			hasFilter = true;
			break;
		}
	}

	if (mGrid->GetNumberRows() > EditorDefine::HEADER_ROW)
	{
		mGrid->DeleteRows(EditorDefine::HEADER_ROW, mGrid->GetNumberRows() - EditorDefine::HEADER_ROW);
	}
	mGridCoordToDataCoordList.clear();
	if (hasFilter)
	{
		// 根据过滤器计算出需要显示哪些行
		Vector<int> showRows;
		FOR_VECTOR(dataList)
		{
			const auto& line = dataList[i];
			bool canShowRow = true;
			FOR_VECTOR_J(line)
			{
				if (!mFilterValueList[j].empty() && !findString(line[j]->mOriginData, mFilterValueList[j].c_str()))
				{
					canShowRow = false;
					break;
				}
			}
			if (canShowRow)
			{
				showRows.push_back(i);
			}
		}
		if (!showRows.isEmpty())
		{
			mGrid->AppendRows(showRows.size());
			FOR_VECTOR(showRows)
			{
				const int rowIndex = i + EditorDefine::HEADER_ROW;
				const auto& line = dataList[showRows[i]];
				FOR_VECTOR_J(line)
				{
					mGrid->SetCellValue(rowIndex, j, line[j]->mOriginData);
				}
				mGridCoordToDataCoordList.push_back(showRows[i]);
				mGrid->SetRowLabelValue(rowIndex, IToS(i + 1));
			}
		}
	}
	else
	{
		mGrid->AppendRows(dataList.size());
		FOR_VECTOR(dataList)
		{
			const int rowIndex = i + EditorDefine::HEADER_ROW;
			const auto& line = dataList[i];
			FOR_VECTOR_J(line)
			{
				mGrid->SetCellValue(rowIndex, j, line[j]->mOriginData);
			}
			mGridCoordToDataCoordList.push_back(i);
			mGrid->SetRowLabelValue(rowIndex, IToS(i + 1));
		}
	}
}

void MainListWindow::update(float elapsedTime)
{
	if (mNeedRefreshData)
	{
		mNeedRefreshData = false;
		wxGridCellCoords editCell;
		wxString editValue;
		long insertionPoint = 0;  // 保存光标位置
		bool wasEditing = mGrid->IsCellEditControlShown();
		if (wasEditing)
		{
			editCell = mGrid->GetGridCursorCoords();
			if (auto* textCtrl = dynamic_cast<wxTextCtrl*>(mGrid->GetCellEditor(editCell.GetRow(), editCell.GetCol())->GetControl()))
			{
				editValue = textCtrl->GetValue();
				insertionPoint = textCtrl->GetInsertionPoint(); // 保存光标位置
			}
			else
			{
				editValue = mGrid->GetCellValue(editCell);
				insertionPoint = editValue.Length();
			}
		}

		// 刷新数据（可能删除或添加行）
		showData(mCSVFile);

		// 刷新后恢复编辑状态
		if (wasEditing &&
			editCell.GetRow() == EditorDefine::ROW_COLUMN_FILTER &&
			editCell.GetCol() < mGrid->GetNumberCols())
		{
			CallAfter([this, editCell, editValue, insertionPoint]()
			{
				mGrid->SetGridCursor(editCell);
				mGrid->MakeCellVisible(editCell);
				mGrid->EnableCellEditControl(true);

				wxGridCellEditor* editor = mGrid->GetCellEditor(editCell.GetRow(), editCell.GetCol());
				mGrid->SetCellEditor(editCell.GetRow(), editCell.GetCol(), editor);
				mGrid->ShowCellEditControl();

				if (auto* newTextCtrl = dynamic_cast<wxTextCtrl*>(editor->GetControl()))
				{
					newTextCtrl->SetValue(editValue);
					newTextCtrl->SetInsertionPoint(insertionPoint); // 恢复光标位置
					newTextCtrl->SetFocus();
				}
			});
		}
	}
}

void MainListWindow::OnEditViewTextChanged(wxCommandEvent& event)
{
	// 仅当顶部编辑框是焦点时才触发同步,否则会在单元格同步到文本编辑框时导致单元格的刷新
	if (mEditViewText->HasFocus())
	{
		const int row = mGrid->GetGridCursorRow();
		const int col = mGrid->GetGridCursorCol();
		if (col != -1)
		{
			mGrid->SetCellValue(row, col, mEditViewText->GetValue());
			mCSVFile->setCellDataAuto(row, col, mEditViewText->GetValue().ToStdString());
		}
	}
	event.Skip();
}

void MainListWindow::OnCellSelected(wxGridEvent& event)
{
	mEditViewText->ChangeValue(mGrid->GetCellValue(event.GetRow(), event.GetCol()));
	event.Skip();
}

void MainListWindow::OnCellChanged(wxGridEvent& event)
{
	const int row = event.GetRow();
	const int col = event.GetCol();
	if (row >= 0 && col >= 0 && row != EditorDefine::ROW_COLUMN_FILTER)
	{
		const string data = mGrid->GetCellValue(row, col).ToStdString();
		// 编辑的是表头行
		HashMap<Vector2Int, string> temp;
		if (row < EditorDefine::HEADER_ROW)
		{
			temp.insert({ row, col }, data);
		}
		// 编辑的是数据行
		else
		{
			temp.insert({ toDataRow(row), col }, data);
		}
		setCellValue(temp, row < EditorDefine::HEADER_ROW, false);
	}
	event.Skip();
}

void MainListWindow::OnGridEditorCreated(wxGridEditorCreatedEvent& event)
{
	if (auto* editor = dynamic_cast<wxTextCtrl*>(event.GetWindow()))
	{
		editor->Bind(wxEVT_TEXT, &MainListWindow::OnCellEditorTextChanged, this);
	}
	event.Skip();
}

void MainListWindow::OnCellEditorTextChanged(wxCommandEvent& event)
{
	auto* editor = dynamic_cast<wxTextCtrl*>(event.GetEventObject());
	if (editor == nullptr)
	{
		event.Skip();
		return;
	}

	if (mGrid->GetGridCursorRow() == EditorDefine::ROW_COLUMN_FILTER)
	{
		// 获取每个过滤条件
		bool filterChanged = false;
		for (int i = 0; i < mGrid->GetNumberCols(); ++i)
		{
			string cellValue = mGrid->GetCellValue(EditorDefine::ROW_COLUMN_FILTER, i);
			if (mGrid->GetGridCursorCol() == i)
			{
				cellValue = editor->GetValue();
			}
			if (mFilterValueList[i] != cellValue)
			{
				mFilterValueList[i] = cellValue;
				filterChanged = true;
			}
		}
		mNeedRefreshData |= filterChanged;
	}
	else
	{
		if (mGrid->GetGridCursorRow() >= 0 && mGrid->GetGridCursorCol() >= 0)
		{
			mEditViewText->ChangeValue(editor->GetValue());
		}
	}
	event.Skip();
}

void MainListWindow::openFile()
{
	wxFileDialog openFileDialog(this, "Open CSV file", mEditorFrame->getCSVPath(), "", "CSV files (*.csv)|*.csv", wxFD_OPEN | wxFD_FILE_MUST_EXIST);
	if (openFileDialog.ShowModal() == wxID_CANCEL)
	{
		return;
	}
	openFile(openFileDialog.GetPath().ToStdString());
}

void MainListWindow::openFile(const string& path)
{
	mCSVFile->closeFile();
	mCSVFile->openFile(path);
	if (!mCSVFile->isOpened())
	{
		dialogOK("文件打开失败,文件可能被占用或者文件内容无法解析");
		return;
	}
	initData(mCSVFile);
}

void MainListWindow::newFile()
{
	mCSVFile->newFile();
	initData(mCSVFile);
}

void MainListWindow::copySelection()
{
	if (!wxTheClipboard->Open())
	{
		return;
	}
	int topRow = -1;
	int leftCol = -1;
	int bottomRow = -1;
	int rightCol = -1;
	bool isHeader = false;
	if (!getSelectionRect(topRow, leftCol, bottomRow, rightCol, isHeader))
	{
		wxTheClipboard->Close();
		return;
	}

	wxString data;
	for (int r = topRow; r <= bottomRow; ++r)
	{
		for (int c = leftCol; c <= rightCol; ++c)
		{
			data += mGrid->GetCellValue(r, c);
			// 单元格分隔符（制表符）
			if (c < rightCol)
			{
				data += "\t";
			}
		}
		// 行分隔符（换行符）
		data += "\n";
	}
	// 移除最后一行的多余换行符
	if (!data.IsEmpty())
	{
		data.RemoveLast();
	}

	wxTheClipboard->SetData(new wxTextDataObject(data));
	wxTheClipboard->Close();
}

void MainListWindow::pasteSelection()
{
	if (!wxTheClipboard->Open())
	{
		return;
	}

	if (!wxTheClipboard->IsSupported(wxDF_TEXT))
	{
		wxTheClipboard->Close();
		return;
	}

	wxTextDataObject dataObj;
	wxTheClipboard->GetData(dataObj);
	wxTheClipboard->Close();

	int topRow = -1;
	int leftCol = -1;
	int bottomRow = -1;
	int rightCol = -1;
	bool isHeader = false;
	if (!getSelectionRect(topRow, leftCol, bottomRow, rightCol, isHeader))
	{
		wxTheClipboard->Close();
		return;
	}

	wxArrayString rows = wxSplit(dataObj.GetText(), '\n');
	if (rows.IsEmpty())
	{
		return;
	}
	// 如果最后一行是空的,就移除掉,可能是复制的时候将最后一行的换行符复制了,导致多拆分出一行
	if (rows[rows.size() - 1].empty())
	{
		rows.RemoveAt(rows.size() - 1);
	}
	// 需要检查每一行的元素数量是否一样的
	int pasteColCount = -1;
	Vector<Vector<string>> dataList;
	for (const wxString& str : rows)
	{
		if (str.empty())
		{
			dataList.push_back(Vector<string>{""});
		}
		else
		{
			dataList.push_back(split(str.char_str(), '\t', false));
		}
		
		if (pasteColCount < 0)
		{
			pasteColCount = dataList[dataList.size() - 1].size();
		}
		else if (pasteColCount != dataList[dataList.size() - 1].size())
		{
			dialogOK("要粘贴的内容格式不正确");
			return;
		}
	}

	const int pasteRowCount = dataList.size();
	if (pasteRowCount == 0)
	{
		return;
	}

	HashMap<Vector2Int, string> dataMap;
	// 为了兼容表头行的批量复制,行的下标0到6表示表头,过滤行无法被复制,大于等于8的下标表示数据行
	// 如果只复制了1格,但是选中了多格,则全部填充为相同的数据
	if (pasteRowCount == 1 && pasteColCount == 1)
	{
		const string& data = dataList[0][0];
		for (int i = topRow; i <= bottomRow; ++i)
		{
			const int dataRow = isHeader ? i : toDataRow(i);
			for (int j = leftCol; j <= rightCol; ++j)
			{
				dataMap.insert({ dataRow, j }, data);
			}
		}
	}
	// 多行多列粘贴
	else
	{
		// 要粘贴的内容超过了选择的单元格范围,需要询问是否确认粘贴,而且不能粘贴到数据区域
		if (bottomRow - topRow + 1 < pasteRowCount || rightCol - leftCol + 1 < pasteColCount)
		{
			if (!dialogYesNo("要粘贴的内容超出了选中范围,是否粘贴?"))
			{
				return;
			}
			if (isHeader)
			{
				bottomRow = clampMax(topRow + pasteRowCount - 1, EditorDefine::ROW_COLUMN_FILTER - 1);
			}
			else
			{
				bottomRow = clampMax(topRow + pasteRowCount - 1, mGrid->GetNumberRows() - 1);
			}
			rightCol = clampMax(leftCol + pasteColCount - 1, mGrid->GetNumberCols() - 1);
		}
		for (int i = topRow; i <= bottomRow; ++i)
		{
			if (i - topRow < 0 || i - topRow >= pasteRowCount)
			{
				break;
			}
			const int dataRow = isHeader ? i : toDataRow(i);
			const auto& cols = dataList[i - topRow];
			for (int j = leftCol; j <= rightCol; ++j)
			{
				if (j - leftCol < 0 || j - leftCol >= cols.size())
				{
					i = bottomRow + 1;
					break;
				}
				dataMap.insert({ dataRow, j }, cols[j - leftCol]);
			}
		}
	}
	setCellValue(dataMap, isHeader, true);
}

// isHeader为true时,dataList中的first.x就是渲染表格中显示的行下标
// isHeader为false时,dataList中的first.x就是数据表的行下标,从0开始的,与渲染无关的
void MainListWindow::setCellValue(const HashMap<Vector2Int, string>& dataList, bool isHeader, bool refreshGrid)
{
	// 添加撤销操作
	HashMap<Vector2Int, string> temp;
	if (isHeader)
	{
		for (const auto& item : dataList)
		{
			temp.insert(item.first, mCSVFile->getCellDataAuto(item.first.x, item.first.y));
		}
	}
	else
	{
		for (const auto& item : dataList)
		{
			temp.insert(item.first, mCSVFile->getCellData(item.first.x, item.first.y));
		}
	}
	mUndoManager->addUndo<UndoSetCellData>()->setData(temp, isHeader);
	
	// 设置单元格数据
	if (isHeader)
	{
		for (const auto& item : dataList)
		{
			mCSVFile->setCellDataAuto(item.first.x, item.first.y, item.second);
		}
	}
	else
	{
		for (const auto& item : dataList)
		{
			mCSVFile->setCellData(item.first.x, item.first.y, item.second);
		}
	}
	if (refreshGrid)
	{
		for (const auto& item : dataList)
		{
			const int showRow = isHeader ? item.first.x : mGridCoordToDataCoordList.findFirstIndex(item.first.x) + EditorDefine::HEADER_ROW;
			mGrid->SetCellValue(showRow, item.first.y, item.second);
			if (showRow == mGrid->GetGridCursorRow() && item.first.y == mGrid->GetGridCursorCol())
			{
				mEditViewText->ChangeValue(item.second);
			}
		}
	}
	if (isHeader)
	{
		bool colNameChanged = false;
		for (const auto& item : dataList)
		{
			if (item.first.x == EditorDefine::ROW_COLUMN_NAME)
			{
				colNameChanged = true;
				mGrid->SetColLabelValue(item.first.y, item.second);
			}
		}
		if (colNameChanged)
		{
			autoAdjustColumnWidthByHeader();
		}
	}
	mGrid->Refresh();
	mGrid->Update();
}

bool MainListWindow::getSelectionRect(int& topRow, int& leftCol, int& bottomRow, int& rightCol, bool& isHeader)
{
	topRow = -1;
	leftCol = -1;
	bottomRow = -1;
	rightCol = -1;
	// 尝试获取连续块选区
	wxGridCellCoordsArray topLeft = mGrid->GetSelectionBlockTopLeft();
	wxGridCellCoordsArray bottomRight = mGrid->GetSelectionBlockBottomRight();
	if (!topLeft.IsEmpty() && !bottomRight.IsEmpty())
	{
		topRow = topLeft[0].GetRow();
		leftCol = topLeft[0].GetCol();
		bottomRow = bottomRight[0].GetRow();
		rightCol = bottomRight[0].GetCol();
	}
	// 如果没有块选区，尝试获取光标位置作为单格选区
	else if (mGrid->GetGridCursorRow() >= 0 && mGrid->GetGridCursorCol() >= 0)
	{
		topRow = bottomRow = mGrid->GetGridCursorRow();
		leftCol = rightCol = mGrid->GetGridCursorCol();
	}
	// 无法检测到选区则退出
	else
	{
		return false;
	}

	// 0表示选中的是表头行,1表示选中的是数据行,两个区域不能混合选择
	int state = -1;
	for (int i = topRow; i <= bottomRow; ++i)
	{
		if (i < EditorDefine::HEADER_DATA_ROW)
		{
			if (state != 0 && state != -1)
			{
				dialogOK("表头区域和数据区域不能混合选中");
				return false;
			}
			state = 0;
		}
		else if (i == EditorDefine::ROW_COLUMN_FILTER)
		{
			dialogOK("过滤行无法被复制或者粘贴");
			return false;
		}
		else if (i >= EditorDefine::HEADER_ROW)
		{
			if (state != 1 && state != -1)
			{
				dialogOK("表头区域和数据区域不能混合选中");
				return false;
			}
			state = 1;
		}
	}
	isHeader = state == 0;
	return true;
}

// 获取选中区域的行列表,起始列,终止列,行列表中是数据行的下标,不是显示行的下标,返回值表示选择是否有效
bool MainListWindow::getDataSelectionList(Vector<int>& rowList, int& leftCol, int& rightCol, bool& isHeader)
{
	HashMap<Vector2Int, string> dataMap;
	int topRow = -1;
	int bottomRow = -1;
	leftCol = -1;
	rightCol = -1;
	if (!getSelectionRect(topRow, leftCol, bottomRow, rightCol, isHeader))
	{
		return false;
	}

	for (int i = topRow; i <= bottomRow; ++i)
	{
		int dataRow = -1;
		if (i < EditorDefine::HEADER_DATA_ROW)
		{
			dataRow = i;
		}
		else if (i >= EditorDefine::HEADER_ROW)
		{
			dataRow = toDataRow(i);
		}
		if (i - topRow < 0)
		{
			break;
		}
		rowList.push_back(dataRow);
	}
	return !rowList.isEmpty();
}

void MainListWindow::OnGridLabelRightClick(wxGridEvent& event)
{
	// 点击列标签
	if (event.GetRow() == -1 && event.GetCol() != -1)
	{
		const int clickedCol = event.GetCol();
		Vector<int> selectCols = getSelectedCols();
		if (!selectCols.contains(clickedCol))
		{
			mGrid->ClearSelection();
			mGrid->SelectCol(clickedCol);
			selectCols.clear();
			selectCols.push_back(clickedCol);
		}

		mGrid->SetGridCursor(0, clickedCol);
		wxMenu menu;
		menu.Append(ID_DELETE_COL, "删除");
		if (selectCols.size() == 1)
		{
			menu.Append(ID_ADD_COL_1_RIGHT, "在右侧插入1列");
			menu.Append(ID_ADD_COL_5_RIGHT, "在右侧插入5列");
			if (clickedCol > 1)
			{
				menu.Append(ID_COL_MOVE_LEFT, "向左移动");
			}
			if (clickedCol > 0 && clickedCol < mGrid->GetNumberCols() - 1)
			{
				menu.Append(ID_COL_MOVE_RIGHT, "向右移动");
			}
			if (clickedCol > 0)
			{
				if (clickedCol < mGrid->GetNumberFrozenCols())
				{
					menu.Append(ID_UNFREEZE_COL, "取消冻结");
				}
				else
				{
					menu.Append(ID_FREEZE_COL, "冻结前" + IToS(clickedCol + 1) + "列");
				}
			}
		}
		
		if (wxGrid* grid = dynamic_cast<wxGrid*>(event.GetEventObject()))
		{
			grid->PopupMenu(&menu);
		}
	}
	// 点击行标签
	else if (event.GetRow() >= EditorDefine::HEADER_ROW && event.GetCol() == -1)
	{
		// 如果未选中过该行则清空已有选择, 并仅选中当前行,否则保持当前的选中行
		const int clickedRow = event.GetRow();
		Vector<int> selectRows = getSelectedRows();
		if (!selectRows.contains(clickedRow))
		{
			mGrid->ClearSelection();
			mGrid->SelectRow(clickedRow);
			selectRows.clear();
			selectRows.push_back(clickedRow);
		}

		mGrid->SetGridCursor(clickedRow, 0);
		wxMenu menu;
		menu.Append(ID_DELETE_ROW, "删除");
		if (selectRows.size() == 1)
		{
			menu.Append(ID_ADD_ROW_1_ABOVE, "在上面插入一行");
			menu.Append(ID_ADD_ROW_10_ABOVE, "在上面插入10行");
			menu.Append(ID_ADD_ROW_100_ABOVE, "在上面插入100行");
			menu.Append(ID_ADD_ROW_1_BELOW, "在下面插入一行");
			menu.Append(ID_ADD_ROW_10_BELOW, "在下面插入10行");
			menu.Append(ID_ADD_ROW_100_BELOW, "在下面插入100行");
			if (clickedRow != EditorDefine::HEADER_ROW)
			{
				menu.Append(ID_SELECT_ROW_TO_FIRST, "选中至第一行");
				menu.Append(ID_MOVE_UP, "上移一行");
			}
			if (clickedRow != EditorDefine::HEADER_ROW + mCSVFile->getRowCount() - 1)
			{
				menu.Append(ID_SELECT_ROW_TO_END, "选中至最后一行");
				menu.Append(ID_MOVE_DOWN, "下移一行");
			}
		}
		else if (selectRows.size() > 1)
		{
			menu.Append(ID_AUTO_ID, "自动生成ID");
			menu.Append(ID_FILL_ROW_SEQUENCE, "自动填充等差数列");
		}
		if (wxGrid* grid = dynamic_cast<wxGrid*>(event.GetEventObject()))
		{
			grid->PopupMenu(&menu);
		}
	}
	event.Skip();
}

void MainListWindow::OnGridCellRightClick(wxGridEvent& event)
{
	const int clickedRow = event.GetRow();
	const int clickedCol = event.GetCol();
	if (clickedRow != -1 && clickedCol != -1)
	{
		int colRanges = 0;
		Vector<Vector2Int> selectCells = getSelectedCells(nullptr, &colRanges);
		if (!selectCells.contains({ clickedCol, clickedRow }))
		{
			mGrid->SelectBlock(clickedRow, clickedCol, clickedRow, clickedCol);
			selectCells.clear();
			selectCells.push_back({ clickedCol, clickedRow });
		}
		wxMenu menu;
		if (selectCells.size() == 1)
		{
			if (clickedRow != EditorDefine::HEADER_ROW)
			{
				menu.Append(ID_SELECT_ROW_TO_FIRST, "选中至第一行");
				menu.Append(ID_FOCUS_FIRST_ROW, "定位到第一行");
			}
			if (clickedRow != EditorDefine::HEADER_ROW + mCSVFile->getRowCount() - 1)
			{
				menu.Append(ID_SELECT_ROW_TO_END, "选中至最后一行");
				menu.Append(ID_FOCUS_END_ROW, "定位到最后一行");
			}
		}
		else if (colRanges == 1)
		{
			menu.Append(ID_FILL_ROW_SEQUENCE, "自动填充等差数列");
		}
		if (wxGrid* grid = dynamic_cast<wxGrid*>(event.GetEventObject()))
		{
			grid->PopupMenu(&menu);
		}
	}
	event.Skip();
}

void MainListWindow::OnGridLeftClick(wxGridEvent& event)
{
	int row = event.GetRow();
	int col = event.GetCol();

	if (row == EditorDefine::ROW_COLUMN_FILTER)
	{
		// 立即完成之前的编辑
		if (mGrid->IsCellEditControlEnabled())
		{
			mGrid->DisableCellEditControl();
		}

		// 设置光标并强制进入编辑模式
		mGrid->SetGridCursor(row, col);
		mGrid->SelectBlock(row, col, row, col);

		// 兼容Windows输入法处理
		CallAfter([this, row, col]() 
		{
			mGrid->EnableCellEditControl(true);
			mGrid->ShowCellEditControl();

			// 强制设置焦点到文本输入控件
			if (auto* editor = dynamic_cast<wxGridCellTextEditor*>(mGrid->GetCellEditor(row, col)))
			{
				// 这句话会引起Debug模式下过滤行拥有焦点时关闭编辑器的崩溃
				editor->GetWindow()->SetFocus();
			}
		});
	}

	event.Skip();
}

void MainListWindow::OnDeleteColumn(wxCommandEvent& event)
{
	Vector<int> cols = getSelectedCols();
	if (mGrid->GetNumberCols() <= 0 || cols.isEmpty())
	{
		return;
	}
	deleteColumn(cols);
	mGrid->ForceRefresh();
}

void MainListWindow::OnAddColumn1Right(wxCommandEvent& event)
{
	const int col = getFirstSelectCol();
	if (mGrid->GetNumberCols() <= 0 || col < 0)
	{
		return;
	}
	// 在右侧插入1列
	Vector<int> cols;
	cols.push_back(col + 1);
	Vector<ColumnData*> headers;
	addColumn(cols, {}, headers);
	mGrid->ForceRefresh();
}

void MainListWindow::OnAddColumn5Right(wxCommandEvent& event)
{
	const int col = getFirstSelectCol();
	if (mGrid->GetNumberCols() <= 0 || col < 0)
	{
		return;
	}
	// 在右侧插入5列
	Vector<int> cols;
	FOR(5)
	{
		cols.push_back(col + 1 + i);
	}
	Vector<ColumnData*> headers;
	addColumn(cols, {}, headers);
	mGrid->ForceRefresh();
}

void MainListWindow::OnColumnMoveLeft(wxCommandEvent& event)
{
	const int col = getFirstSelectCol();
	if (mGrid->GetNumberCols() <= 0 || col < 0)
	{
		return;
	}
	// 向左移动
	swapColumn(col, col - 1);
	mGrid->ForceRefresh();
}

void MainListWindow::OnColumnMoveRight(wxCommandEvent& event)
{
	const int col = getFirstSelectCol();
	if (mGrid->GetNumberCols() <= 0 || col < 0)
	{
		return;
	}
	// 向右移动
	swapColumn(col, col + 1);
	mGrid->ForceRefresh();
}

void MainListWindow::OnDeleteRow(wxCommandEvent& event)
{
	if (mGrid->GetNumberCols() <= 0)
	{
		return;
	}
	Vector<int> dataRows;
	for (const int showRow : getSelectedRows())
	{
		if (showRow < EditorDefine::HEADER_ROW)
		{
			return;
		}
		dataRows.push_back(toDataRow(showRow));
	}
	deleteRow(dataRows);
	mGrid->ForceRefresh();
}

void MainListWindow::OnAddRow1Above(wxCommandEvent& event)
{
	addRowAbove(1);
}

void MainListWindow::OnAddRow10Above(wxCommandEvent& event)
{
	addRowAbove(10);
}

void MainListWindow::OnAddRow100Above(wxCommandEvent& event)
{
	addRowAbove(100);
}

void MainListWindow::OnAddRow1Below(wxCommandEvent& event)
{
	addRowBelow(1);
}

void MainListWindow::OnAddRow10Below(wxCommandEvent& event)
{
	addRowBelow(10);
}

void MainListWindow::OnAddRow100Below(wxCommandEvent& event)
{
	addRowBelow(100);
}

void MainListWindow::OnSelectRowToFirst(wxCommandEvent& event)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	Vector<Vector2Int> cells = getSelectedCells();
	if (cells.isEmpty())
	{
		dialogOK("没有选中任何单元格");
		return;
	}
	mGrid->SelectBlock(EditorDefine::HEADER_ROW, cells[0].x, cells[0].y, cells[0].x);
}

void MainListWindow::OnSelectRowToEnd(wxCommandEvent& event)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	Vector<Vector2Int> cells = getSelectedCells();
	if (cells.isEmpty())
	{
		dialogOK("没有选中任何单元格");
		return;
	}
	mGrid->SelectBlock(cells[0].y, cells[0].x, mGrid->GetNumberRows() - 1, cells[0].x);
}

void MainListWindow::OnFillRowSequence(wxCommandEvent& event)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	int colCount = 0;
	Vector<Vector2Int> cells = getSelectedCells(nullptr, &colCount);
	if (cells.isEmpty())
	{
		dialogOK("没有选中任何单元格");
		return;
	}
	if (colCount != 1)
	{
		dialogOK("只能选中一列单元格进行填充");
		return;
	}
	bool hasHeader = false;
	for (const Vector2Int item : cells)
	{
		if (item.y < EditorDefine::HEADER_ROW)
		{
			hasHeader = true;
			break;
		}
	}
	if (hasHeader)
	{
		dialogOK("不能选中表头单元格");
		return;
	}

	// 计算差值
	const ColumnData* header = mCSVFile->getHeaderData(cells[0].x);
	if (header->mType == "int")
	{
		int value0 = SToI(mCSVFile->getCellData(toDataRow(cells[0].y), cells[0].x));
		int delta = 1;
		if (cells.size() > 1)
		{
			const int value1 = SToI(mCSVFile->getCellData(toDataRow(cells[1].y), cells[1].x));
			delta = clampMin(value1 - value0, 1);
		}
		HashMap<Vector2Int, string> cellValues;
		for (int i = 1; i < cells.size(); ++i)
		{
			cellValues.insert({ toDataRow(cells[i].y), cells[i].x}, IToS(value0 += delta));
		}
		setCellValue(cellValues, false, true);
	}
	else if (header->mType == "float")
	{
		float value0 = SToF(mCSVFile->getCellData(toDataRow(cells[0].y), cells[0].x));
		float delta = 1.0f;
		if (cells.size() > 1)
		{
			const float value1 = SToF(mCSVFile->getCellData(toDataRow(cells[1].y), cells[1].x));
			delta = clampMin(value1 - value0);
		}
		HashMap<Vector2Int, string> cellValues;
		for (int i = 1; i < cells.size(); ++i)
		{
			cellValues.insert({ toDataRow(cells[i].y), cells[i].x }, FToS(value0 += delta));
		}
		setCellValue(cellValues, false, true);
	}
	else if (header->mType == "llong")
	{
		llong value0 = SToLL(mCSVFile->getCellData(toDataRow(cells[0].y), cells[0].x));
		llong delta = 1;
		if (cells.size() > 1)
		{
			const llong value1 = SToLL(mCSVFile->getCellData(toDataRow(cells[1].y), cells[1].x));
			delta = clampMin(value1 - value0, 1LL);
		}
		HashMap<Vector2Int, string> cellValues;
		for (int i = 1; i < cells.size(); ++i)
		{
			cellValues.insert({ toDataRow(cells[i].y), cells[i].x }, LLToS(value0 += delta));
		}
		setCellValue(cellValues, false, true);
	}
	else if (header->mType == "string")
	{
		const string& str0 = mCSVFile->getCellData(toDataRow(cells[0].y), cells[0].x);
		const int index0 = getFirstNumberPos(str0);
		if (index0 >= 0)
		{
			string preStr = str0.substr(0, index0);
			string endStr;
			int value0;
			const int index1 = getFirstNotNumberPos(str0, index0);
			if (index1 >= 0)
			{
				value0 = SToI(str0.substr(index0, index1 - index0));
				endStr = str0.substr(index1);
			}
			else
			{
				value0 = SToI(str0.substr(index0));
				endStr = "";
			}
			int delta = 1;
			if (cells.size() > 1)
			{
				const string& str1 = mCSVFile->getCellData(toDataRow(cells[1].y), cells[1].x);
				const int index00 = getFirstNumberPos(str1);
				if (index0 == index00)
				{
					const int index11 = getFirstNotNumberPos(str1, index00);
					int value1;
					if (index11 >= 0)
					{
						value1 = SToI(str1.substr(index00, index11 - index00));
					}
					else
					{
						value1 = SToI(str1.substr(index00));
					}
					delta = clampMin(value1 - value0);
				}
			}
			HashMap<Vector2Int, string> cellValues;
			for (int i = 1; i < cells.size(); ++i)
			{
				cellValues.insert({ toDataRow(cells[i].y), cells[i].x }, preStr + IToS(value0 += delta) + endStr);
			}
			setCellValue(cellValues, false, true);
		}
	}
	else
	{
		dialogOK("不支持的列数据格式:" + header->mType);
		return;
	}
	mGrid->ForceRefresh();
}

void MainListWindow::addRowAbove(int count)
{
	if (count <= 0)
	{
		return;
	}
	const int row = getFirstSelectRow();
	if (mGrid->GetNumberCols() <= 0 || row < EditorDefine::HEADER_ROW)
	{
		return;
	}
	const int dataRow = toDataRow(row);
	Vector<int> rows;
	FOR(count)
	{
		rows.push_back(dataRow + i);
	}
	addRow(rows, {});
	mGrid->ForceRefresh();
}

void MainListWindow::addRowBelow(int count)
{
	if (count <= 0)
	{
		return;
	}
	int row = getFirstSelectRow();
	if (mGrid->GetNumberCols() <= 0)
	{
		return;
	}
	if (row < EditorDefine::HEADER_ROW)
	{
		row = mGrid->GetNumberRows() - 1;
	}
	const int dataRow = toDataRow(row);
	Vector<int> rows;
	FOR(count)
	{
		rows.push_back(dataRow + i + 1);
	}
	addRow(rows, {});
	mGrid->ForceRefresh();
}

void MainListWindow::OnMoveUp(wxCommandEvent& event)
{
	const int row = getFirstSelectRow();
	if (mGrid->GetNumberCols() <= 0 || row <= EditorDefine::HEADER_ROW)
	{
		return;
	}
	// 和上面一行交换位置
	const int dataRow0 = toDataRow(row);
	swapRow(dataRow0, dataRow0 - 1);
}

void MainListWindow::OnMoveDown(wxCommandEvent& event)
{
	const int row = getFirstSelectRow();
	if (mGrid->GetNumberCols() <= 0 || row < EditorDefine::HEADER_ROW || row >= EditorDefine::HEADER_ROW + mCSVFile->getRowCount() - 1)
	{
		return;
	}
	// 和下面一行交换位置
	const int dataRow0 = toDataRow(row);
	swapRow(dataRow0, dataRow0 + 1);
}

void MainListWindow::OnFreezeColumn(wxCommandEvent& event)
{
	const int col = getFirstSelectCol();
	if (col < 0)
	{
		return;
	}
	mGrid->FreezeTo(EditorDefine::HEADER_ROW, col + 1);
}

void MainListWindow::OnUnFreezeColumn(wxCommandEvent& event)
{
	mGrid->FreezeTo(EditorDefine::HEADER_ROW, 1);
}

void MainListWindow::OnAutoID(wxCommandEvent& event)
{
	autoSetSelectionID();
}

void MainListWindow::OnFocusFirstRow(wxCommandEvent& event)
{
	Vector<Vector2Int> cells = getSelectedCells();
	if (cells.isEmpty())
	{
		dialogOK("没有选中任何单元格");
		return;
	}
	mGrid->MakeCellVisible(EditorDefine::HEADER_ROW, cells[0].x);
}

void MainListWindow::OnFocusEndRow(wxCommandEvent& event)
{
	Vector<Vector2Int> cells = getSelectedCells();
	if (cells.isEmpty())
	{
		dialogOK("没有选中任何单元格");
		return;
	}
	mGrid->MakeCellVisible(mGrid->GetNumberRows() - 1, cells[0].x);
}

void MainListWindow::OnKeyDown(wxKeyEvent& event)
{
	if (event.GetKeyCode() == WXK_DELETE)
	{
		int topRow = -1;
		int leftCol = -1;
		int bottomRow = -1;
		int rightCol = -1;
		bool isHeader = false;
		if (!getSelectionRect(topRow, leftCol, bottomRow, rightCol, isHeader))
		{
			return;
		}

		HashMap<Vector2Int, string> dataMap;
		for (int i = topRow; i <= bottomRow; ++i)
		{
			const int dataRow = isHeader ? i : toDataRow(i);
			for (int j = leftCol; j <= rightCol; ++j)
			{
				dataMap.insert({ dataRow, j }, "");
			}
		}
		setCellValue(dataMap, isHeader, true);
	}
	// 其他按键正常处理
	else
	{
		event.Skip();
	}
}

// 这里的cols一定要是有序的
void MainListWindow::deleteColumn(const Vector<int>& cols)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (cols.isEmpty())
	{
		dialogOK("没有需要删除的列");
		return;
	}
	if (cols.contains(0))
	{
		dialogOK("不能删除ID列");
		return;
	}
	FOR_VECTOR(cols)
	{
		if (i > 0 && cols[i] <= cols[i - 1])
		{
			dialogOK("删除列参数错误");
			return;
		}
	}

	Vector<Vector<GridData*>> colDatas;
	Vector<ColumnData*> headerList;
	mCSVFile->deleteColumn(cols, colDatas, headerList);
	// 添加撤销操作
	mUndoManager->addUndo<UndoDeleteColumn>()->setData(cols, move(colDatas), move(headerList));
	FOR_INVERSE_I(cols.size())
	{
		mGrid->DeleteCols(cols[i]);
	}
}

void MainListWindow::addColumn(const Vector<int>& cols, Vector<Vector<GridData*>>&& dataList, Vector<ColumnData*>& headerDatas)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (cols.isEmpty())
	{
		dialogOK("没有需要添加的列");
		return;
	}
	if (dataList.size() > 0 && cols.size() != dataList[0].size())
	{
		dialogOK("添加列所需的表格数据数量错误");
		return;
	}
	if (headerDatas.size() > 0 && cols.size() != headerDatas.size())
	{
		dialogOK("添加列所需的表头数据数量错误");
		return;
	}

	// 添加撤销操作
	mUndoManager->addUndo<UndoAddColumn>()->setData(cols);

	// 先插入空的列
	for (const int col : cols)
	{
		mGrid->InsertCols(col);
	}
	
	// 设置列数据
	// 遍历每一行
	FOR_VECTOR(dataList)
	{
		const auto& datas = dataList[i];
		// 遍历每一列
		FOR_VECTOR_J(datas)
		{
			mGrid->SetCellValue(i + EditorDefine::HEADER_ROW, cols[j], datas[j]->mOriginData);
		}
	}
	// 往数据类中插入列,如果columnData是空的,表示是插入新的一列,会在内部创建出一个来
	mCSVFile->addColumn(cols, move(dataList), headerDatas);

	// 最后再刷新列的表头部分
	FOR_VECTOR(cols)
	{
		const int col = cols[i];
		ColumnData* colHeaderData = headerDatas[i];
		mGrid->SetColLabelValue(col, colHeaderData->mName);
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_NAME, col, colHeaderData->mName);
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_TYPE, col, colHeaderData->mType);
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_OWNER, col, getOwnerString(colHeaderData->mOwner));
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_COMMENT, col, colHeaderData->mComment);
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_LINK_TABLE, col, colHeaderData->mLinkTable);
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_LINK_LENGTH, col, colHeaderData->mLinkLength);
		mGrid->SetCellValue(EditorDefine::ROW_COLUMN_FLAG, col, colHeaderData->mFlag);
	}
	// 表头需要全部都设置成黄色背景,过滤行为灰色
	FOR(EditorDefine::HEADER_ROW)
	{
		FOR_J(mGrid->GetNumberCols())
		{
			mGrid->SetCellBackgroundColour(i, j, i == EditorDefine::ROW_COLUMN_FILTER ? *wxLIGHT_GREY : *wxYELLOW);
		}
	}

	mFilterValueList.clear();
	FOR(mGrid->GetNumberCols())
	{
		mFilterValueList.push_back(mGrid->GetCellValue(EditorDefine::ROW_COLUMN_FILTER, i).ToStdString());
	}
	mGrid->MakeCellVisible(0, cols[cols.size() - 1]);
	autoAdjustColumnWidthByHeader();
}

void MainListWindow::swapColumn(int srcCol, int destCol)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (srcCol < 0 || destCol < 0 || srcCol >= mGrid->GetNumberCols() || destCol >= mGrid->GetNumberCols())
	{
		dialogOK("交换列的参数错误");
		return;
	}
	// 添加撤销操作
	mUndoManager->addUndo<UndoSwapColumn>()->setData(srcCol, destCol);
	mCSVFile->swapColumn(srcCol, destCol);
	Vector<string> colValue;
	FOR(mGrid->GetNumberRows())
	{
		wxString value = mGrid->GetCellValue(i, destCol);
		mGrid->SetCellValue(i, destCol, mGrid->GetCellValue(i, srcCol));
		mGrid->SetCellValue(i, srcCol, value);
	}
	wxString colLabel = mGrid->GetColLabelValue(destCol);
	mGrid->SetColLabelValue(destCol, mGrid->GetColLabelValue(srcCol));
	mGrid->SetColLabelValue(srcCol, colLabel);
}

void MainListWindow::deleteRow(const Vector<int>& dataRows)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (dataRows.isEmpty())
	{
		dialogOK("没有需要删除的行");
		return;
	}
	FOR_VECTOR(dataRows)
	{
		const int row = dataRows[i];
		if (row < 0)
		{
			dialogOK("不能删除表头行");
			return;
		}
	}
	// 因为下面会修改mGridCoordToDataCoordList,所以提前保存数据行对应的显示行到一个列表中
	Vector<int> showRowList;
	FOR_VECTOR(dataRows)
	{
		showRowList.push_back(mGridCoordToDataCoordList.findFirstIndex(dataRows[i]));
	}
	const int deleteCount = dataRows.size();
	FOR_VECTOR_INVERSE(showRowList)
	{
		const int dataRow = dataRows[i];
		const int showRow = showRowList[i];
		if (showRow >= 0)
		{
			mGridCoordToDataCoordList.eraseAt(showRow);
			FOR_VECTOR_J(mGridCoordToDataCoordList)
			{
				if (j >= showRow)
				{
					--mGridCoordToDataCoordList[j];
				}
			}
		}
	}

	// 从后往前删除
	FOR_VECTOR_INVERSE(showRowList)
	{
		const int showRow = showRowList[i];
		if (showRow >= 0)
		{
			mGrid->DeleteRows(showRow + EditorDefine::HEADER_ROW);
		}
	}
	// 需要找到删除行对应的数据行
	Vector<Vector<GridData*>> rows;
	mCSVFile->getRows(dataRows, rows);
	mCSVFile->deleteRow(dataRows);
	// 添加撤销操作
	mUndoManager->addUndo<UndoDeleteRow>()->setData(dataRows, move(rows));
	mFilterValueList.clear();
	FOR(mGrid->GetNumberCols())
	{
		mFilterValueList.push_back(mGrid->GetCellValue(EditorDefine::ROW_COLUMN_FILTER, i).ToStdString());
	}
}

void MainListWindow::addRow(const Vector<int>& dataRows, const Vector<Vector<GridData*>>& dataList)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (dataRows.isEmpty())
	{
		dialogOK("没有要添加的行");
		return;
	}
	if (dataList.size() > 0 && dataRows.size() != dataList.size())
	{
		dialogOK("添加行的数据错误");
		return;
	}

	for (const int dataRow : dataRows)
	{
		// 找到新行后面第一个在表格中显示出来的行,插入到这一行的前面
		int insertRowIndex = -1;
		FOR_VECTOR_J(mGridCoordToDataCoordList)
		{
			if (mGridCoordToDataCoordList[j] >= dataRow)
			{
				insertRowIndex = j;
				break;
			}
		}
		// 找不到就插入到最后
		if (insertRowIndex < 0)
		{
			insertRowIndex = mGridCoordToDataCoordList.size();
		}
		mGridCoordToDataCoordList.insert(insertRowIndex, dataRow);
		FOR_VECTOR_J(mGridCoordToDataCoordList)
		{
			if (j > dataRow)
			{
				++mGridCoordToDataCoordList[j];
			}
		}
	}

	FOR_VECTOR(dataRows)
	{
		const int showRow = mGridCoordToDataCoordList.findFirstIndex(dataRows[i]);
		if (showRow >= 0)
		{
			mGrid->InsertRows(showRow + EditorDefine::HEADER_ROW);
			mGrid->MakeCellVisible(showRow + EditorDefine::HEADER_ROW, 0);
			if (i < dataList.size())
			{
				const auto& list = dataList[i];
				FOR_VECTOR_J(list)
				{
					mGrid->SetCellValue(showRow + EditorDefine::HEADER_ROW, j, list[j]->mOriginData);
				}
			}
		}
	}

	// 添加撤销操作
	mUndoManager->addUndo<UndoAddRow>()->setData(dataRows);
	mCSVFile->addRow(dataRows, dataList);
}

void MainListWindow::addRowToEnd(Vector<GridData*>&& rowData)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	// 因为需要添加在行即使在筛选情况下也需要显示出来,所以不调用addRow
	const int dataRow = mCSVFile->getRowCount();
	const int showRow = mGrid->GetNumberRows() - EditorDefine::HEADER_ROW;
	mGridCoordToDataCoordList.insert(showRow, dataRow);

	// 添加撤销操作
	mUndoManager->addUndo<UndoAddRow>()->setData(Vector<int>{dataRow});
	Vector<Vector<GridData*>> tempRows;
	tempRows.push_back(move(rowData));
	mCSVFile->addRow(Vector<int>{ dataRow }, tempRows);
	// 因为在addRow里面可能会修改dataList,设置一些默认数据,所以添加完了再显示
	mGrid->InsertRows(showRow + EditorDefine::HEADER_ROW);
	const auto& tempRow = mCSVFile->getRow(dataRow);
	FOR_VECTOR(tempRow)
	{
		mGrid->SetCellValue(showRow + EditorDefine::HEADER_ROW, i, tempRow[i]->mOriginData);
	}
	mGrid->MakeCellVisible(showRow + EditorDefine::HEADER_ROW, 0);
}

void MainListWindow::addRowToFirst(Vector<GridData*>&& rowData)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	// 因为需要添加在行即使在筛选情况下也需要显示出来,所以不调用addRow
	const int dataRow = 0;
	const int showRow = 0;
	FOR_VECTOR(mGridCoordToDataCoordList)
	{
		++mGridCoordToDataCoordList[i];
	}
	mGridCoordToDataCoordList.insert(showRow, dataRow);
	mGrid->InsertRows(showRow + EditorDefine::HEADER_ROW);
	FOR_VECTOR(rowData)
	{
		mGrid->SetCellValue(showRow + EditorDefine::HEADER_ROW, i, rowData[i]->mOriginData);
	}

	// 添加撤销操作
	Vector<Vector<GridData*>> tempRows;
	tempRows.push_back(move(rowData));
	Vector<int> rows{ dataRow };
	mUndoManager->addUndo<UndoAddRow>()->setData(rows);
	mCSVFile->addRow(rows, tempRows);
	mGrid->MakeCellVisible(EditorDefine::HEADER_ROW, 0);
}

void MainListWindow::swapRow(int dataRow0, int dataRow1)
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (dataRow0 == dataRow1)
	{
		return;
	}

	// 添加撤销操作
	mUndoManager->addUndo<UndoSwapRow>()->setData(dataRow0, dataRow1);
	mCSVFile->swapRow(dataRow0, dataRow1);
	// 重新刷新这两行的数据
	const int showRow0 = mGridCoordToDataCoordList.findFirstIndex(dataRow0);
	const int showRow1 = mGridCoordToDataCoordList.findFirstIndex(dataRow1);
	if (showRow0 >= 0)
	{
		const auto& rowData0 = mCSVFile->getAllGrid()[dataRow0];
		FOR_VECTOR(rowData0)
		{
			mGrid->SetCellValue(showRow0 + EditorDefine::HEADER_ROW, i, rowData0[i]->mOriginData);
		}
	}
	if (showRow1 >= 0)
	{
		const auto& rowData1 = mCSVFile->getAllGrid()[dataRow1];
		FOR_VECTOR(rowData1)
		{
			mGrid->SetCellValue(showRow1 + EditorDefine::HEADER_ROW, i, rowData1[i]->mOriginData);
		}
		mGrid->SetGridCursor(showRow1 + EditorDefine::HEADER_ROW, 0);
		mGrid->SelectRow(showRow1 + EditorDefine::HEADER_ROW);
		mGrid->MakeCellVisible(showRow1 + EditorDefine::HEADER_ROW, 0);
	}
	mGrid->ForceRefresh();
}

void MainListWindow::save()
{
	if (!mCSVFile->isOpened())
	{
		return;
	}
	if (mGrid->IsCellEditControlEnabled())
	{
		// 强制提交编辑器中的值到数据模型
		mGrid->SaveEditControlValue();
		// 关闭编辑器（结束编辑状态）
		mGrid->DisableCellEditControl();
	}

	if (mCSVFile->getFilePath().empty())
	{
		wxFileDialog saveDialog(this, "保存文件", "", mCSVFile->getTableName() + ".csv", "文本文件 (*.csv)|*.csv", wxFD_SAVE | wxFD_OVERWRITE_PROMPT);
		if (saveDialog.ShowModal() == wxID_OK)
		{
			mCSVFile->setFilePath(saveDialog.GetPath().ToStdString());
			mCSVFile->setTableName(getFileNameNoSuffix(mCSVFile->getFilePath(), true));
			// 保存失败再把路径清空,下次再重新选择
			if (!mCSVFile->save())
			{
				mCSVFile->setFilePath("");
			}
			// 由于修改了内容,需要刷新显示
			mGrid->SetCellValue(0, 0, mCSVFile->getTableName());
		}
	}
	else
	{
		mCSVFile->save();
	}
}

void MainListWindow::autoAdjustColumnWidthByHeader()
{
	wxClientDC dc(mGrid);
	dc.SetFont(mGrid->GetLabelFont());
	const int colCount = mGrid->GetNumberCols();
	for (int col = 0; col < colCount; ++col)
	{
		// 获取文字显示宽度
		wxCoord textWidth;
		wxCoord textHeight;
		dc.GetTextExtent(mGrid->GetColLabelValue(col), &textWidth, &textHeight);
		// 获取原始的列宽度,调整后不能小于原始的宽度,因为很多时候会手动拉宽列显示更多内容
		mGrid->SetColSize(col, clampMin(textWidth + 10, clampMin(mGrid->GetColSize(col), 100)));
	}
	mGrid->ForceRefresh();
}

void MainListWindow::autoSetSelectionID()
{
	int leftCol = -1;
	int rightCol = -1;
	bool isHeader = false;
	Vector<int> dataRowList;
	if (!getDataSelectionList(dataRowList, leftCol, rightCol, isHeader))
	{
		return;
	}
	if (isHeader)
	{
		dialogOK("表头无法自动生成ID");
		return;
	}
	// 检查是否为连续的数据行,不连续的无法自动生成ID
	FOR_VECTOR(dataRowList)
	{
		if (i > 0 && dataRowList[i] != dataRowList[i - 1] + 1)
		{
			dialogOK("选择的不是连续的行,无法自动生成ID");
			return;
		}
	}
	int startID = SToI(mCSVFile->getCellData(dataRowList[0], 0));
	if (startID <= 0)
	{
		dialogOK("选择的第一个ID无效,无法自动生成ID");
		return;
	}
	HashMap<Vector2Int, string> dataList;
	for (const int dataRow : dataRowList)
	{
		dataList.insert({ dataRow, 0 }, IToS(startID++));
	}
	setCellValue(dataList, false, true);
}

void MainListWindow::fixedItemName()
{
	// 加载临时的Item表
	CSVFile* itemTable = new CSVFile();
	itemTable->openFile(mEditorFrame->getCSVPath() + "Item.csv");
	const int nameCol = itemTable->getColumnByName("Name");
	// 将所有物品的正确名称放入列表中,等待查询
	HashMap<int, string> itemNameMap;
	FOR(itemTable->getRowCount())
	{
		const int itemID = SToI(itemTable->getRow(i)[0]->mOriginData);
		const string& itemName = itemTable->getRow(i)[nameCol]->mOriginData;
		itemNameMap.insert(itemID, itemName);
	}

	HashMap<Vector2Int, string> modifyList;
	FOR(mCSVFile->getColumnCount())
	{
		const ColumnData* header = mCSVFile->getHeaderData(i);
		if (header->mFlag != "ItemName")
		{
			continue;
		}
		// 寻找到对应的ItemID列
		int idColumn = mCSVFile->getColumnByName(removeEndString(header->mName, "Name"));
		if (idColumn < 0)
		{
			idColumn = mCSVFile->getColumnByName(removeEndString(header->mName, "Name") + "ID");
		}
		if (idColumn < 0)
		{
			continue;
		}
		const ColumnData* idColData = mCSVFile->getHeaderData(idColumn);
		if (idColData->mType == "int")
		{
			FOR_J(mCSVFile->getRowCount())
			{
				const int curItemID = SToI(mCSVFile->getRow(j)[idColumn]->mOriginData);
				const string& curItemName = mCSVFile->getRow(j)[i]->mOriginData;
				const string& rightName = itemNameMap.tryGet(curItemID);
				if (!rightName.empty() && rightName != curItemName)
				{
					modifyList.insert({j, i}, rightName);
				}
			}
		}
		else if (idColData->mType == "Vector<int>")
		{
			FOR_J(mCSVFile->getRowCount())
			{
				const Vector<int> curItemIDList = SToIs(mCSVFile->getRow(j)[idColumn]->mOriginData);
				const string& curItemName = mCSVFile->getRow(j)[i]->mOriginData;
				string rightNameList;
				for (int curID : curItemIDList)
				{
					rightNameList += itemNameMap.tryGet(curID) + ",";
				}
				rightNameList = removeEndString(rightNameList, ",");
				if (!rightNameList.empty() && rightNameList != curItemName)
				{
					modifyList.insert({ j, i }, rightNameList);
				}
			}
		}
	}
	setCellValue(modifyList, false, true);
}

Vector<int> MainListWindow::getSelectedRows()
{
	Vector<int> rows;
	const wxGridCellCoordsArray blockTopLeft = mGrid->GetSelectionBlockTopLeft();
	const wxGridCellCoordsArray blockBottomRight = mGrid->GetSelectionBlockBottomRight();
	if (!blockTopLeft.IsEmpty() && !blockBottomRight.IsEmpty())
	{
		const int top = blockTopLeft[0].GetRow();
		const int bottom = blockBottomRight[0].GetRow();
		if (top != -1 && bottom != -1)
		{
			for (int r = top; r <= bottom; ++r)
			{
				rows.push_back(r);
			}
		}
	}
	// 如果没有block选中，则用SelectedRows
	if (rows.isEmpty())
	{
		for (const int r : mGrid->GetSelectedRows())
		{
			rows.push_back(r);
		}
	}
	return rows;
}

Vector<int> MainListWindow::getSelectedCols()
{
	Vector<int> cols;
	const wxGridCellCoordsArray blockTopLeft = mGrid->GetSelectionBlockTopLeft();
	const wxGridCellCoordsArray blockBottomRight = mGrid->GetSelectionBlockBottomRight();
	if (!blockTopLeft.IsEmpty() && !blockBottomRight.IsEmpty())
	{
		const int left = blockTopLeft[0].GetCol();
		const int right = blockBottomRight[0].GetCol();
		if (left != -1 && right != -1)
		{
			for (int i = left; i <= right; ++i)
			{
				cols.push_back(i);
			}
		}
	}
	// 如果没有block选中，则用SelectedCols
	if (cols.isEmpty())
	{
		for (const int col : mGrid->GetSelectedCols())
		{
			cols.push_back(col);
		}
	}
	return cols;
}

Vector<Vector2Int> MainListWindow::getSelectedCells(int* outRowCount, int* outColCount)
{
	Vector<Vector2Int> cells;
	const wxGridCellCoordsArray blockTopLeft = mGrid->GetSelectionBlockTopLeft();
	const wxGridCellCoordsArray blockBottomRight = mGrid->GetSelectionBlockBottomRight();
	if (!blockTopLeft.IsEmpty() && !blockBottomRight.IsEmpty())
	{
		const int top = blockTopLeft[0].GetRow();
		const int bottom = blockBottomRight[0].GetRow();
		const int left = blockTopLeft[0].GetCol();
		const int right = blockBottomRight[0].GetCol();
		if (top != -1 && bottom != -1 && left != -1 && right != -1)
		{
			for (int x = left; x <= right; ++x)
			{
				for (int y = top; y <= bottom; ++y)
				{
					cells.push_back({x, y});
				}
			}
			if (outRowCount != nullptr)
			{
				*outRowCount = bottom - top + 1;
			}
			if (outColCount != nullptr)
			{
				*outColCount = right - left + 1;
			}
		}
	}
	if (cells.isEmpty())
	{
		if (!mGrid->GetSelectedCols().IsEmpty())
		{
			for (const int col : mGrid->GetSelectedCols())
			{
				FOR(mGrid->GetNumberRows())
				{
					cells.push_back({i, col });
				}
			}
			if (outRowCount != nullptr)
			{
				*outRowCount = mGrid->GetNumberRows();
			}
			if (outColCount != nullptr)
			{
				*outColCount = mGrid->GetSelectedCols().Count();
			}
		}
		else
		{
			for (const int row : mGrid->GetSelectedRows())
			{
				FOR(mGrid->GetNumberCols())
				{
					cells.push_back({ row, i });
				}
			}
			if (outRowCount != nullptr)
			{
				*outRowCount = mGrid->GetSelectedRows().Count();
			}
			if (outColCount != nullptr)
			{
				*outColCount = mGrid->GetNumberCols();
			}
		}
	}
	return cells;
}

int MainListWindow::getFirstSelectRow()
{
	const wxGridCellCoordsArray blockTopLeft = mGrid->GetSelectionBlockTopLeft();
	if (!blockTopLeft.IsEmpty() && blockTopLeft[0].GetRow() != -1)
	{
		return blockTopLeft[0].GetRow();
	}
	// 如果没有block选中，则用SelectedRows
	for (const int r : mGrid->GetSelectedRows())
	{
		return r;
	}
	return -1;
}

int MainListWindow::getFirstSelectCol()
{
	const wxGridCellCoordsArray blockTopLeft = mGrid->GetSelectionBlockTopLeft();
	if (!blockTopLeft.IsEmpty() && blockTopLeft[0].GetCol() != -1)
	{
		return blockTopLeft[0].GetCol();
	}
	// 如果没有block选中，则用SelectedRows
	for (const int col : mGrid->GetSelectedCols())
	{
		return col;
	}
	return -1;
}