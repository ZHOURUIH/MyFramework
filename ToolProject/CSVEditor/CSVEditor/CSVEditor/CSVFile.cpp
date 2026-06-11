#include "AllHeader.h"

Vector<string> CSVFile::mValidFlagList{ "Path", "ItemName", "PropertyName", "EquipTypeName" };

void CSVFile::openFile(const string& file)
{
	const string fileContent = openTxtFile(file, true);
	if (fileContent.empty())
	{
		return;
	}
	mFileOpened = true;
	mFilePath = file;
	Vector<Vector<string>> result;
	parseCSV(fileContent, result);
	FOR(EditorDefine::HEADER_DATA_ROW)
	{
		const Vector<string>& line = result[i];
		// 表名
		if (i == EditorDefine::ROW_TABLE_NAME)
		{
			mTableName = line[0];
		}
		// 表所属
		else if (i == EditorDefine::ROW_TABLE_OWNER)
		{
			mOwner = getOwner(line[0]);
		}
		// 字段名
		else if (i == EditorDefine::ROW_COLUMN_NAME)
		{
			FOR_VECTOR_J(line)
			{
				ColumnData* colData = new ColumnData;
				colData->mName = line[j];
				mHeaderDataList.push_back(colData);
			}
		}
		// 字段类型
		else if (i == EditorDefine::ROW_COLUMN_TYPE)
		{
			FOR_VECTOR_J(line)
			{
				mHeaderDataList[j]->mType = line[j];
			}
		}
		// 字段所属
		else if (i == EditorDefine::ROW_COLUMN_OWNER)
		{
			FOR_VECTOR_J(line)
			{
				mHeaderDataList[j]->mOwner = getOwner(line[j]);
			}
		}
		// 字段注释
		else if (i == EditorDefine::ROW_COLUMN_COMMENT)
		{
			FOR_VECTOR_J(line)
			{
				mHeaderDataList[j]->mComment = line[j];
			}
		}
		// 链接表格
		else if (i == EditorDefine::ROW_COLUMN_LINK_TABLE)
		{
			FOR_VECTOR_J(line)
			{
				mHeaderDataList[j]->mLinkTable = line[j];
			}
		}
		// 长度关联
		else if (i == EditorDefine::ROW_COLUMN_LINK_LENGTH)
		{
			FOR_VECTOR_J(line)
			{
				mHeaderDataList[j]->mLinkLength = line[j];
			}
		}
		// 字段标签
		else if (i == EditorDefine::ROW_COLUMN_FLAG)
		{
			FOR_VECTOR_J(line)
			{
				mHeaderDataList[j]->mFlag = line[j];
			}
		}
	}
	for (int i = EditorDefine::HEADER_DATA_ROW; i < result.size(); ++i)
	{
		const Vector<string>& line = result[i];
		Vector<GridData*> dataLine;
		FOR_VECTOR_J(line)
		{
			GridData* data = new GridData();
			data->mOriginData = line[j];
			dataLine.push_back(data);
		}
		if (!dataLine.isEmpty())
		{
			mAllGrid.push_back(move(dataLine));
		}
	}
	// 检查每一行的数据数量是否跟列名数量一致
	FOR_VECTOR(mAllGrid)
	{
		auto& row = mAllGrid[i];
		if (row.size() > mHeaderDataList.size())
		{
			FOR(row.size() - mHeaderDataList.size())
			{
				row.eraseAt(row.size() - 1);
			}
			dialogOK("第" + IToS(i) + "行数据错误,ID:" + row[0]->mOriginData);
		}
		else if (row.size() < mHeaderDataList.size())
		{
			FOR(mHeaderDataList.size() - row.size())
			{
				GridData* data = new GridData();
				data->mOriginData = "";
				row.push_back(data);
			}
			dialogOK("第" + IToS(i) + "行数据错误,ID:" + row[0]->mOriginData);
		}
	}
	setDirty(false);
}

void CSVFile::newFile()
{
	mFileOpened = true;
	mTableName = "NewTable";
	mOwner = OWNER::BOTH;
	ColumnData* tempCol = new ColumnData();
	tempCol->mName = "ID";
	tempCol->mType = "int";
	tempCol->mOwner = OWNER::BOTH;
	tempCol->mComment = "唯一ID";
	mHeaderDataList.push_back(tempCol);
	setDirty(true);
}

void CSVFile::closeFile()
{
	for (auto& item : mAllGrid)
	{
		for (GridData* data : item)
		{
			delete data;
		}
	}
	mAllGrid.clear();
	for (ColumnData* data : mHeaderDataList)
	{
		delete data;
	}
	mHeaderDataList.clear();
	mTableName = "";
	mFilePath = "";
	mOwner = OWNER::NONE;
	mFileOpened = false;
	setDirty(false);
}

bool CSVFile::save()
{
	if (!validate())
	{
		return false;
	}
	string csv;
	const int colCount = mHeaderDataList.size();
	FOR(EditorDefine::HEADER_DATA_ROW)
	{
		// 表名
		if (i == EditorDefine::ROW_TABLE_NAME)
		{
			appendCSVString(csv, mTableName, 0 < colCount - 1);
			for (int j = 1; j < colCount; ++j)
			{
				appendCSVString(csv, "", j < colCount - 1);
			}
		}
		// 表所属
		else if (i == EditorDefine::ROW_TABLE_OWNER)
		{
			appendCSVString(csv, getOwnerString(mOwner), 0 < colCount - 1);
			for (int j = 1; j < colCount; ++j)
			{
				appendCSVString(csv, "", j < colCount - 1);
			}
		}
		// 字段名字
		else if (i == EditorDefine::ROW_COLUMN_NAME)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, mHeaderDataList[j]->mName, j < colCount - 1);
			}
		}
		// 字段类型
		else if (i == EditorDefine::ROW_COLUMN_TYPE)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, mHeaderDataList[j]->mType, j < colCount - 1);
			}
		}
		// 字段所属
		else if (i == EditorDefine::ROW_COLUMN_OWNER)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, getOwnerString(mHeaderDataList[j]->mOwner), j < colCount - 1);
			}
		}
		// 字段注释
		else if (i == EditorDefine::ROW_COLUMN_COMMENT)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, mHeaderDataList[j]->mComment, j < colCount - 1);
			}
		}
		// 字段链接表格
		else if (i == EditorDefine::ROW_COLUMN_LINK_TABLE)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, mHeaderDataList[j]->mLinkTable, j < colCount - 1);
			}
		}
		// 字段链接表格
		else if (i == EditorDefine::ROW_COLUMN_LINK_LENGTH)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, mHeaderDataList[j]->mLinkLength, j < colCount - 1);
			}
		}
		// 字段标签
		else if (i == EditorDefine::ROW_COLUMN_FLAG)
		{
			FOR_VECTOR_J(mHeaderDataList)
			{
				appendCSVString(csv, mHeaderDataList[j]->mFlag, j < colCount - 1);
			}
		}
	}
	// 表数据
	for (const auto& row : mAllGrid)
	{
		FOR_VECTOR(row)
		{
			appendCSVString(csv, row[i]->mOriginData, i < row.size() - 1);
		}
	}
	writeFile(mFilePath, ANSIToUTF8(csv));
	setDirty(false);
	return true;
}

bool CSVFile::validate()
{
	// 名字不能重复
	Set<string> nameSet;
	for (ColumnData* data : mHeaderDataList)
	{
		if (data->mName.empty())
		{
			dialogOK("有空的列名");
			return false;
		}
		if (!nameSet.insert(data->mName))
		{
			dialogOK("有重复的字段名:" + data->mName);
			return false;
		}
		if (data->mType.empty())
		{
			dialogOK("有空的字段类型");
			return false;
		}
		if (data->mComment.empty())
		{
			dialogOK("有空的字段注释");
			return false;
		}
	}

	// ID不能重复
	Set<int> idSet;
	FOR_VECTOR(mAllGrid)
	{
		const auto& row = mAllGrid[i];
		if (row[0]->mOriginData.empty())
		{
			dialogOK("ID不能为空");
			return false;
		}
		if (!idSet.insert(SToI(row[0]->mOriginData)))
		{
			dialogOK("有重复的ID:" + row[0]->mOriginData);
			return false;
		}
	}

	// 检查字段
	// 筛选出长度链接名一致的字段
	HashMap<string, Vector<int>> linkLengthList;
	FOR_VECTOR(mHeaderDataList)
	{
		ColumnData* colData = mHeaderDataList[i];
		// 非字符串以及列表类型的列是不能为空的
		if (colData->mType != "string" && !startWith(colData->mType, "Vector<"))
		{
			FOR_VECTOR_J(mAllGrid)
			{
				if (mAllGrid[j][i]->mOriginData.empty())
				{
					dialogOK("非字符串以及列表类型的列是不能为空的,ID:" + mAllGrid[j][0]->mOriginData + "字段名:" + colData->mName);
					return false;
				}
			}
		}

		// 检查表格链接,暂时只支持csv和db格式的表格
		if (!colData->mLinkTable.empty())
		{
			if (!isFileExist(StringUtility::getFilePath(mFilePath, true) + colData->mLinkTable + ".csv") && 
				!isFileExist(StringUtility::getFilePath(mFilePath, true) + colData->mLinkTable + ".db"))
			{
				dialogOK("链接的表格不存在,链接的表格名:" + colData->mLinkTable);
				return false;
			}
		}

		// 收集长度链接
		if (!colData->mLinkLength.empty())
		{
			if (!startWith(colData->mType, "Vector<"))
			{
				dialogOK("字段不是列表类型,不能设置长度链接,字段名" + colData->mName);
				return false;
			}
			linkLengthList.insertOrGet(colData->mLinkLength).push_back(i);
		}

		// 检查字段标签
		if (!colData->mFlag.empty())
		{
			if (!mValidFlagList.contains(colData->mFlag))
			{
				dialogOK("字段标签错误:" + colData->mFlag + ", 列名:" + colData->mName);
				return false;
			}
			// 带标签的字段都是客户端需要使用的,所属不能设置为None或者Server
			if (colData->mOwner != OWNER::BOTH && colData->mOwner != OWNER::CLIENT)
			{
				dialogOK("带标签的列的所属只能为Both或者Client, 列名:" + colData->mName);
				return false;
			}
		}
		if (colData->mType == "Vector<Vector2Int>")
		{
			Vector<Vector2Int> elements;
			for (const auto& row : mAllGrid)
			{
				if (!SToV2Is(row[i]->mOriginData, elements, "|"))
				{
					dialogOK("字段内容错误,类型Vector<Vector2Int>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "Vector<Vector3Int>")
		{
			Vector<Vector3Int> elements;
			for (const auto& row : mAllGrid)
			{
				if (!SToV3Is(row[i]->mOriginData, elements, "|"))
				{
					dialogOK("字段内容错误,类型Vector<Vector3Int>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "Vector<Vector2>")
		{
			Vector<Vector2> elements;
			for (const auto& row : mAllGrid)
			{
				if (!SToV2s(row[i]->mOriginData, elements, "|"))
				{
					dialogOK("字段内容错误,类型Vector<Vector2>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "Vector<Vector3>")
		{
			Vector<Vector3> elements;
			for (const auto& row : mAllGrid)
			{
				if (!SToV3s(row[i]->mOriginData, elements, "|"))
				{
					dialogOK("字段内容错误,类型Vector<Vector3>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "Vector2")
		{
			for (const auto& row : mAllGrid)
			{
				bool result = false;
				SToV2(row[i]->mOriginData, ",", &result);
				if (!result)
				{
					dialogOK("字段内容错误,类型Vector<Vector2>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "Vector2Int")
		{
			for (const auto& row : mAllGrid)
			{
				bool result = false;
				SToV2I(row[i]->mOriginData, ",", &result);
				if (!result)
				{
					dialogOK("字段内容错误,类型Vector<Vector2Int>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "Vector3")
		{
			for (const auto& row : mAllGrid)
			{
				bool result = false;
				SToV3(row[i]->mOriginData, ",", &result);
				if (!result)
				{
					dialogOK("字段内容错误,类型Vector<Vector3>,字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "int" || colData->mType == "llong")
		{
			for (const auto& row : mAllGrid)
			{
				if (SToI(row[i]->mOriginData) == 0 && row[i]->mOriginData != "0")
				{
					dialogOK("字段内容错误,类型" + colData->mType + ",字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
		else if (colData->mType == "float")
		{
			for (const auto& row : mAllGrid)
			{
				if (isZero(SToF(row[i]->mOriginData)) && row[i]->mOriginData != "0.0" && row[i]->mOriginData != "0")
				{
					dialogOK("字段内容错误,类型" + colData->mType + ",字段名" + colData->mName + ",ID:" + row[0]->mOriginData);
					return false;
				}
			}
		}
	}
	for (const auto& item : linkLengthList)
	{
		if (item.second.size() <= 0)
		{
			dialogOK("不能存在单独的长度链接名,至少需要两列拥有相同的长度链接名:" + item.first);
			return false;
		}
	}
	// 检查字段连接
	for (const auto& row : mAllGrid)
	{
		// 遍历每一组的字段链接
		for (const auto& item : linkLengthList)
		{
			// 获取每组里每个字段的元素个数
			int elementCount = -1;
			for (const int col : item.second)
			{
				ColumnData* colData = mHeaderDataList[col];
				if (elementCount < 0)
				{
					elementCount = getListLength(row[col]->mOriginData, colData->mType);
				}
				else
				{
					if (elementCount != getListLength(row[col]->mOriginData, colData->mType))
					{
						dialogOK("已链接长度的字段的长度不一致,链接名" + item.first + ", ID:" + row[0]->mOriginData);
						return false;
					}
				}
			}
		}
	}

	// 检查自定义的变量名是否有重复的
	int varNameCol = -1;
	FOR_VECTOR(mHeaderDataList)
	{
		if (mHeaderDataList[i]->mName == "VariableName")
		{
			varNameCol = i;
			break;
		}
	}
	if (varNameCol > 0)
	{
		Vector<string> varNameList;
		for (const auto& row : mAllGrid)
		{
			const string& originData = row[varNameCol]->mOriginData;
			if (!originData.empty() && !varNameList.addUnique(originData))
			{
				dialogOK("有自定义变量名重复:" + originData + ", ID:" + row[0]->mOriginData);
				return false;
			}
		}
	}
	return true;
}

const string& CSVFile::getColumnName(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return mHeaderDataList[col]->mName;
}

string CSVFile::getColumnOwner(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return getOwnerString(mHeaderDataList[col]->mOwner);
}

const string& CSVFile::getColumnType(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return mHeaderDataList[col]->mType;
}

const string& CSVFile::getColumnComment(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return mHeaderDataList[col]->mComment;
}

const string& CSVFile::getColumnLinkTable(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return mHeaderDataList[col]->mLinkTable;
}

const string& CSVFile::getColumnLinkLength(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return mHeaderDataList[col]->mLinkLength;
}

const string& CSVFile::getColumnFlag(int col)
{
	if (col < 0 || col >= mHeaderDataList.size())
	{
		return FrameDefine::EMPTY;
	}
	return mHeaderDataList[col]->mFlag;
}

const string& CSVFile::getCellData(int row, int col)
{
	return mAllGrid[row][col]->mOriginData;
}

int CSVFile::getColumnByName(const string& name)
{
	FOR_VECTOR(mHeaderDataList)
	{
		if (mHeaderDataList[i]->mName == name)
		{
			return i;
		}
	}
	return -1;
}

string CSVFile::getCellDataAuto(int row, int col)
{
	if (row == EditorDefine::ROW_TABLE_NAME)
	{
		if (col == 0)
		{
			return getTableName();
		}
		else
		{
			return FrameDefine::EMPTY;
		}
	}
	else if (row == EditorDefine::ROW_TABLE_OWNER)
	{
		if (col == 0)
		{
			return getOwnerString(getTableOwner());
		}
		else
		{
			return FrameDefine::EMPTY;
		}
	}
	else if (row == EditorDefine::ROW_COLUMN_NAME)
	{
		return getColumnName(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_TYPE)
	{
		return getColumnType(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_OWNER)
	{
		return getColumnOwner(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_COMMENT)
	{
		return getColumnComment(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_LINK_TABLE)
	{
		return getColumnLinkTable(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_LINK_LENGTH)
	{
		return getColumnLinkLength(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_FLAG)
	{
		return getColumnFlag(col);
	}
	else if (row == EditorDefine::ROW_COLUMN_FILTER)
	{
		// 无法获取过滤行
		return "";
	}
	else
	{
		return getCellData(row - EditorDefine::HEADER_ROW, col);
	}
}

void CSVFile::setCellDataAuto(int row, int col, const string& value)
{
	if (row == EditorDefine::ROW_TABLE_NAME)
	{
		if (col == 0)
		{
			setTableName(value);
		}
	}
	else if (row == EditorDefine::ROW_TABLE_OWNER)
	{
		if (col == 0)
		{
			setTableOwner(value);
		}
	}
	else if (row == EditorDefine::ROW_COLUMN_NAME)
	{
		setColumnName(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_TYPE)
	{
		setColumnType(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_OWNER)
	{
		setColumnOwner(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_COMMENT)
	{
		setColumnComment(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_LINK_TABLE)
	{
		setColumnLinkTable(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_LINK_LENGTH)
	{
		setColumnLinkLength(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_FLAG)
	{
		setColumnFlag(col, value);
	}
	else if (row == EditorDefine::ROW_COLUMN_FILTER)
	{
		// 不能设置过滤行
	}
	else
	{
		setCellData(row - EditorDefine::HEADER_ROW, col, value);
	}
}

void CSVFile::setCellData(int row, int col, const string& data)
{
	mAllGrid[row][col]->mOriginData = data;
	setDirty(true);
}

void CSVFile::setColumnName(int col, const string& name)
{
	mHeaderDataList[col]->mName = name;
	setDirty(true);
}

void CSVFile::setColumnOwner(int col, const string& owner)
{
	mHeaderDataList[col]->mOwner = getOwner(owner);
	setDirty(true);
}

void CSVFile::setColumnType(int col, const string& type)
{
	mHeaderDataList[col]->mType = type;
	setDirty(true);
}

void CSVFile::setColumnComment(int col, const string& comment)
{
	mHeaderDataList[col]->mComment = comment;
	setDirty(true);
}

void CSVFile::setColumnLinkTable(int col, const string& linkTable)
{
	mHeaderDataList[col]->mLinkTable = linkTable;
	setDirty(true);
}

void CSVFile::setColumnLinkLength(int col, const string& linkLength)
{
	mHeaderDataList[col]->mLinkLength = linkLength;
	setDirty(true);
}

void CSVFile::setColumnFlag(int col, const string& flag)
{
	mHeaderDataList[col]->mFlag = flag;
	setDirty(true);
}

void CSVFile::setTableName(const string& name)
{
	mTableName = name;
	setDirty(true);
}

void CSVFile::setTableOwner(const string& owner)
{
	mOwner = getOwner(owner);
	setDirty(true);
}

void CSVFile::deleteColumn(const Vector<int>& cols, Vector<Vector<GridData*>>& outList, Vector<ColumnData*>& outHeaders)
{
	for (const int col : cols)
	{
		outHeaders.push_back(mHeaderDataList[col]);
	}
	FOR_INVERSE_I(cols.size())
	{
		mHeaderDataList.eraseAt(cols[i]);
	}
	
	for (auto& item : mAllGrid)
	{
		Vector<GridData*> datas;
		for (const int col : cols)
		{
			datas.push_back(item[col]);
		}
		outList.push_back(move(datas));
	}
	for (auto& item : mAllGrid)
	{
		FOR_INVERSE_I(cols.size())
		{
			item.eraseAt(cols[i]);
		}
	}
	setDirty(true);
}

void CSVFile::addColumn(const Vector<int>& cols, Vector<Vector<GridData*>>&& colOriginDatas, Vector<ColumnData*>& headers)
{
	if (headers.isEmpty())
	{
		FOR_VECTOR(cols)
		{
			headers.push_back(new ColumnData());
		}
	}
	FOR_VECTOR(cols)
	{
		mHeaderDataList.insert(cols[i], headers[i]);
	}
	
	if (colOriginDatas.isEmpty())
	{
		for (auto& item : mAllGrid)
		{
			for (const int col : cols)
			{
				item.insert(col, new GridData());
			}
		}
	}
	else
	{
		FOR_VECTOR(mAllGrid)
		{
			FOR_VECTOR_J(cols)
			{
				mAllGrid[i].insert(cols[j], colOriginDatas[i][j]);
			}
		}
	}
	colOriginDatas.clear();
	setDirty(true);
}

void CSVFile::swapColumn(int col0, int col1)
{
	mHeaderDataList.swapIndex(col0, col1);
	for (auto& item : mAllGrid)
	{
		item.swapIndex(col0, col1);
	}
	setDirty(true);
}

void CSVFile::getRows(const Vector<int>& rows, Vector<Vector<GridData*>>& outRows)
{
	for (const int row : rows)
	{
		outRows.push_back(move(mAllGrid[row]));
	}
}

void CSVFile::deleteRow(const Vector<int>& rows)
{
	FOR_VECTOR_INVERSE(rows)
	{
		mAllGrid.eraseAt(rows[i]);
	}
	setDirty(true);
}

void CSVFile::addRow(const Vector<int>& rows, const Vector<Vector<GridData*>>& rowDatas)
{
	FOR_VECTOR(rows)
	{
		if (i < rowDatas.size())
		{
			if (rowDatas[i].isEmpty())
			{
				Vector<GridData*> tempRowData;
				FOR_J(mHeaderDataList.size())
				{
					GridData* data = new GridData();
					// 如果是在表格末尾添加数据,则自动填充ID
					if (j == 0 && rows[i] == mAllGrid.size())
					{
						if (mAllGrid.size() == 0)
						{
							data->mOriginData = "1";
						}
						else
						{
							data->mOriginData = IToS(SToI(mAllGrid[mAllGrid.size() - 1][0]->mOriginData) + 1);
						}
					}
					tempRowData.push_back(data);
				}
				mAllGrid.insert(rows[i], move(tempRowData));
			}
			else
			{
				mAllGrid.insert(rows[i], rowDatas[i]);
			}
		}
		else
		{
			Vector<GridData*> tempRowData;
			FOR_J(mHeaderDataList.size())
			{
				tempRowData.push_back(new GridData());
			}
			mAllGrid.insert(rows[i], move(tempRowData));
		}
	}
	setDirty(true);
}

void CSVFile::swapRow(int row0, int row1)
{
	Vector<GridData*> temp = move(mAllGrid[row0]);
	mAllGrid[row0] = move(mAllGrid[row1]);
	mAllGrid[row1] = temp;
	setDirty(true);
}

int CSVFile::getListLength(const string& value, const string& colType)
{
	if (value.empty())
	{
		return 0;
	}
	if (colType == "Vector<Vector2Int>")
	{
		Vector<Vector2Int> elements;
		SToV2Is(value, elements, "|");
		return elements.size();
	}
	else if (colType == "Vector<Vector3Int>")
	{
		Vector<Vector3Int> elements;
		SToV3Is(value, elements, "|");
		return elements.size();
	}
	else if (colType == "Vector<Vector2>")
	{
		Vector<Vector2> elements;
		SToV2s(value, elements, "|");
		return elements.size();
	}
	else if (colType == "Vector<Vector3>")
	{
		Vector<Vector3> elements;
		SToV3s(value, elements, "|");
		return elements.size();
	}
	else if (colType == "Vector<string>")
	{
		Vector<string> elements;
		split(value, ",", elements, false);
		return elements.size();
	}
	// 其他的类型就认为是简单非结构体,以逗号分隔
	else if (startWith(colType, "Vector<"))
	{
		Vector<string> elements;
		split(value, ",", elements, false);
		return elements.size();
	}
	return 0;
}