#pragma once

#include "FrameHeader.h"
#include "EditorEnum.h"
#include "GridData.h"
#include "ColumnData.h"
#include "EditorCallback.h"

class CSVFile
{
public:
	CSVFile() = default;
	~CSVFile() = default;
	void openFile(const string& file);
	void closeFile();
	void newFile();
	bool save();
	const Vector<Vector<GridData*>>& getAllGrid() const { return mAllGrid; }
	const Vector<ColumnData*>& getHeaderDataList() const { return mHeaderDataList; }
	const ColumnData* getHeaderData(int index) const { return mHeaderDataList[index]; }
	const string& getTableName() const { return mTableName; }
	const string& getFilePath() const { return mFilePath; }
	void setFilePath(const string& path) { mFilePath = path; }
	OWNER getTableOwner() const { return mOwner; }
	const string& getColumnName(int col);
	string getColumnOwner(int col);
	const string& getColumnType(int col);
	const string& getColumnComment(int col);
	const string& getColumnLinkTable(int col);
	const string& getColumnLinkLength(int col);
	const string& getColumnFlag(int col);
	const string& getCellData(int row, int col);
	int getColumnByName(const string& name);
	int getRowCount() const { return mAllGrid.size(); }
	int getColumnCount() const { return mHeaderDataList.size(); }
	bool isOpened() const { return mFileOpened; }
	void setDirty(bool dirty) { mDirty = dirty; CALL(mDirtyCallback); }
	bool isDirty() const { return mDirty; }
	void setDirtyCallback(const Action& callback) { mDirtyCallback = callback; }
	bool validate();
	const Vector<GridData*>& getRow(int row) const { return mAllGrid[row]; }
	string getCellDataAuto(int row, int col);
	void setCellDataAuto(int row, int col, const string& value);
	void setCellData(int row, int col, const string& data);
	void setColumnName(int col, const string& name);
	void setColumnOwner(int col, const string& owner);
	void setColumnType(int col, const string& type);
	void setColumnComment(int col, const string& comment);
	void setColumnLinkTable(int col, const string& linkTable);
	void setColumnLinkLength(int col, const string& linkLength);
	void setColumnFlag(int col, const string& flag);
	void setTableName(const string& name);
	void setTableOwner(const string& owner);
	void deleteColumn(const Vector<int>& cols, Vector<Vector<GridData*>>& outList, Vector<ColumnData*>& outHeaders);
	void addColumn(const Vector<int>& cols, Vector<Vector<GridData*>>&& colOriginDatas, Vector<ColumnData*>& headers);
	void swapColumn(int col0, int col1);
	void getRows(const Vector<int>& rows, Vector<Vector<GridData*>>& outRows);
	void deleteRow(const Vector<int>& rows);
	void addRow(const Vector<int>& rows, const Vector<Vector<GridData*>>& rowDatas);
	void swapRow(int row0, int row1);
	int getListLength(const string& value, const string& colType);
protected:
	string mFilePath;
	Vector<Vector<GridData*>> mAllGrid;
	Vector<ColumnData*> mHeaderDataList;
	string mTableName;
	OWNER mOwner = OWNER::NONE;
	bool mDirty = false;
	bool mFileOpened = false;
	Action mDirtyCallback;
	static Vector<string> mValidFlagList;
};