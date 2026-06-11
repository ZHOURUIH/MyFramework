#pragma once

#include "FrameHeader.h"
#include "EditorDefine.h"

class GridData;
class ColumnData;
class CSVFile;
class MainListWindow : public wxPanel
{
public:
	MainListWindow(wxWindow* parent, long style);
	~MainListWindow() = default;
	void init() {}
	void initData(CSVFile* table);
	void showData(CSVFile* table);
	void update(float elapsedTime);
	void openFile();
	void openFile(const string& path);
	void newFile();
	void copySelection();
	void pasteSelection();
	void setCellValue(const HashMap<Vector2Int, string>& dataList, bool isHeader, bool refreshGrid);
	void deleteColumn(const Vector<int>& cols);
	void addColumn(const Vector<int>& cols, Vector<Vector<GridData*>>&& dataList, Vector<ColumnData*>& headerDatas);
	void swapColumn(int srcCol, int destCol);
	void deleteRow(const Vector<int>& dataRow);
	void addRow(const Vector<int>& row, const Vector<Vector<GridData*>>& dataList);
	void addRowToEnd(Vector<GridData*>&& rowData);
	void addRowToFirst(Vector<GridData*>&& rowData);
	void swapRow(int row0, int row1);
	void save();
	void autoAdjustColumnWidthByHeader();
	void autoSetSelectionID();
	void fixedItemName();
	Vector<int> getSelectedRows();
	void addRowAbove(int count);
	void addRowBelow(int count);
	int getFirstSelectRow();
	int getFirstSelectCol();
	int getShowRowCount() const { return mGrid->GetNumberRows() - EditorDefine::HEADER_ROW; }

	DECLARE_EVENT_TABLE()
	void OnEditViewTextChanged(wxCommandEvent& event);
	void OnCellSelected(wxGridEvent& event);
	void OnCellChanged(wxGridEvent& event);
	void OnGridEditorCreated(wxGridEditorCreatedEvent& event);
	void OnCellEditorTextChanged(wxCommandEvent& event);
	void OnGridLabelRightClick(wxGridEvent& event);
	void OnGridCellRightClick(wxGridEvent& event);
	void OnGridLeftClick(wxGridEvent& event);
	void OnDeleteColumn(wxCommandEvent& event);
	void OnAddColumn1Right(wxCommandEvent& event);
	void OnAddColumn5Right(wxCommandEvent& event);
	void OnColumnMoveLeft(wxCommandEvent& event);
	void OnColumnMoveRight(wxCommandEvent& event);
	void OnDeleteRow(wxCommandEvent& event);
	void OnAddRow1Above(wxCommandEvent& event);
	void OnAddRow10Above(wxCommandEvent& event);
	void OnAddRow100Above(wxCommandEvent& event);
	void OnAddRow1Below(wxCommandEvent& event);
	void OnAddRow10Below(wxCommandEvent& event);
	void OnAddRow100Below(wxCommandEvent& event);
	void OnSelectRowToFirst(wxCommandEvent& event);
	void OnSelectRowToEnd(wxCommandEvent& event);
	void OnFillRowSequence(wxCommandEvent& event);
	void OnMoveUp(wxCommandEvent& event);
	void OnMoveDown(wxCommandEvent& event);
	void OnFreezeColumn(wxCommandEvent& event);
	void OnUnFreezeColumn(wxCommandEvent& event);
	void OnAutoID(wxCommandEvent& event);
	void OnFocusFirstRow(wxCommandEvent& event);
	void OnFocusEndRow(wxCommandEvent& event);
	void OnKeyDown(wxKeyEvent& event);
protected:
	bool getSelectionRect(int& topRow, int& leftCol, int& bottomRow, int& rightCol, bool& isHeader);
	bool getDataSelectionList(Vector<int>& rowList, int& leftCol, int& rightCol, bool& isHeader);
	Vector<int> getSelectedCols();
	Vector<Vector2Int> getSelectedCells(int* outRowCount = nullptr, int* outColCount = nullptr);
	// gridRow是从整个表开始的行序号,内部会减去表头的行数再转换
	int toDataRow(int gridRow) 
	{
		if (mGridCoordToDataCoordList.isEmpty())
		{
			return -1;
		}
		return mGridCoordToDataCoordList[clampMin(gridRow - EditorDefine::HEADER_ROW)]; 
	}
protected:
	wxGrid* mGrid = nullptr;
	wxTextCtrl* mEditViewText;
	Vector<string> mFilterValueList;
	Vector<int> mGridCoordToDataCoordList;	// 下标是渲染表格的行下标,value是数据的行下标,下标不含表头
	bool mNeedRefreshData = false;			// 延迟刷新列表的标记
};