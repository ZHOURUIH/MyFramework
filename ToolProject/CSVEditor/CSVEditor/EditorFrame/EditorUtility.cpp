#include "AllHeader.h"

namespace EditorUtility
{
	void parseCSV(const string& fullContent, Vector<Vector<string>>& result)
	{
		Vector<string> row;
		string field;
		bool inQuotes = false;
		for (int i = 0; i < (int)fullContent.size(); ++i)
		{
			const char c = fullContent[i];
			if (c == '"')
			{
				if (inQuotes && i + 1 < fullContent.size() && fullContent[i + 1] == '"')
				{
					// 转义的双引号 ""
					field.push_back('"');
					++i;
				}
				else
				{
					// 进入或退出引号
					inQuotes = !inQuotes;
				}
			}
			else if (c == ',' && !inQuotes)
			{
				// 逗号分隔列
				row.emplace_back(move(field));
			}
			else if ((c == '\n' || c == '\r') && !inQuotes)
			{
				// 遇到换行，完成一行,即使空行也要放进去
				//if (!field.empty() || !row.isEmpty())
				{
					row.emplace_back(move(field));
					result.emplace_back(move(row));
				}
				// 处理 \r\n 的情况：如果当前是 \r 且下一个是 \n，就跳过 \n
				if (c == '\r' && i + 1 < fullContent.size() && fullContent[i + 1] == '\n')
				{
					++i;
				}
			}
			else
			{
				// 普通字符
				field.push_back(c);
			}
		}

		// 最后一行如果没加进去，要补上
		if (!field.empty() || !row.isEmpty())
		{
			row.emplace_back(move(field));
			result.emplace_back(move(row));
		}
	}

	void appendCSVString(string& file, const string& value, bool addCommaOrReturn)
	{
		string cell = value;
		// 判断是否需要加引号
		if ((cell.find(',') != -1) || (cell.find('"') != -1) ||
			(cell.find('\n') != -1) || (cell.find('\r') != -1))
		{
			string escaped;
			escaped.reserve(cell.size());
			for (char c : cell)
			{
				if (c == '"')
				{
					escaped += "\"\"";  // 替换 " 为 ""
				}
				else
				{
					escaped += c;
				}
			}
			cell = "\"" + escaped + "\"";
		}
		file += cell;
		if (addCommaOrReturn)
		{
			file += ",";
		}
		else
		{
			file += "\r\n";
		}
	}

	string getOwnerString(OWNER owner)
	{
		if (owner == OWNER::NONE)
		{
			return "None";
		}
		else if (owner == OWNER::SERVER)
		{
			return "Server";
		}
		else if (owner == OWNER::CLIENT)
		{
			return "Client";
		}
		else if (owner == OWNER::BOTH)
		{
			return "Both";
		}
		return "";
	}

	OWNER getOwner(const string& owner)
	{
		if (owner == "None")
		{
			return OWNER::NONE;
		}
		else if (owner == "Server")
		{
			return OWNER::SERVER;
		}
		else if (owner == "Client")
		{
			return OWNER::CLIENT;
		}
		else if (owner == "Both")
		{
			return OWNER::BOTH;
		}
		return OWNER::NONE;
	}

	int dialogYesNoCancel(const wxString& title, const wxString& message, const wxString& yesText, const wxString& noText, const wxString& cancelText)
	{
		CustomMessageBox dlg(mEditorFrame, title, message, yesText, noText, cancelText, wxYES | wxNO | wxCANCEL);
		return dlg.ShowModal();
	}

	int dialogYesNoCancel(const wxString& message, const wxString& yesText, const wxString& noText, const wxString& cancelText)
	{
		return dialogYesNoCancel("提示", message, yesText, noText, cancelText);
	}

	int dialogYesNoCancel(const wxString& title, const wxString& message)
	{
		return dialogYesNoCancel(title, message, "是", "否", "取消");
	}

	int dialogYesNoCancel(const wxString& message)
	{
		return dialogYesNoCancel("提示", message, "是", "否", "取消");
	}

	bool dialogYesNo(const wxString& title, const wxString& message, const wxString& yesText, const wxString& noText)
	{
		CustomMessageBox dlg(mEditorFrame, title, message, yesText, noText, "", wxYES | wxNO);
		return dlg.ShowModal() == wxID_YES;
	}

	bool dialogYesNo(const wxString& message, const wxString& yesText, const wxString& noText)
	{
		return dialogYesNo("提示", message, yesText, noText);
	}

	bool dialogYesNo(const wxString& title, const wxString& message)
	{
		return dialogYesNo(title, message, "是", "否");
	}

	bool dialogYesNo(const wxString& message)
	{
		return dialogYesNo("提示", message, "是", "否");
	}

	void dialogOK(const wxString& title, const wxString& message, const wxString& okText)
	{
		CustomMessageBox dlg(mEditorFrame, title, message, okText, "", "", wxYES);
		dlg.ShowModal();
	}

	void dialogOK(const wxString& title, const wxString& message)
	{
		dialogOK(title, message, "确定");
	}

	void dialogOK(const wxString& message)
	{
		dialogOK("提示", message, "确定");
	}
}