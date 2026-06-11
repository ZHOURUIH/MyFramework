#pragma once

#include "FrameHeader.h"

class MainListWindow;
class CSVEditor;
class EditorFrame : public wxFrame
{
public:
	EditorFrame(wxString title, wxSize size);
	virtual ~EditorFrame() = default;
	void init();
	void destroy();
	void setup();
	// 刷新全部的可选中菜单和可选中工具按钮的选中状态
	void RefreshAllMenuToolCheckState();
	void Update(float elapsedTime);
	void Render() {}
	void KeyProcess() {}
	const string& getCSVPath() const { return mCSVPath; }
	
	DECLARE_EVENT_TABLE()
	void OnTimer(wxTimerEvent& event);
	void OnExit(wxCommandEvent& event);			// 菜单的退出
	void OnOpenFile(wxCommandEvent& event);		// 菜单的打开文件
	void OnNewFile(wxCommandEvent& event);
	void OnSaveFile(wxCommandEvent& event);		// 菜单的保存文件
	void OnConvertTable(wxCommandEvent& event);
	void OnAutoID(wxCommandEvent& event);
	void OnCopy(wxCommandEvent& event);			// 菜单的复制
	void OnPaste(wxCommandEvent& event);		// 菜单的粘贴
	void OnUndo(wxCommandEvent& event);			// 菜单的撤销
	void OnRedo(wxCommandEvent& event);			// 菜单的重做
	void OnAddRowToFirst(wxCommandEvent& event);
	void OnAddRowToEnd(wxCommandEvent& event);
	void OnFixedItemName(wxCommandEvent& event);
	void OnAddRowButton(wxCommandEvent& event);
	void OnCloseWindow(wxCloseEvent& event);	// 程序发出的关闭事件
protected:
	static void onDirty();
	static void onUndoChanged();
	void CreateMenu();
	void CreateToolBar();
	void CreateWindows();
	void CreateStatusBar();
	void UpdateStatus();
	void CreateEditorCore() {}
	void RefreshAllResource() {}
	void parseConfig(const string& filePath);
	void setStatusText(const string& text, int index);
protected:
	wxTimer* mTimer = nullptr;
	wxTextCtrl* mRowCountInput = nullptr;
	wxAuiManager mAuiManager = nullptr;
	string mCSVPath;				// 表格文件的路径,从配置文件中解析
	string mCSVToBinaryExePath;		// 表格转换工具的路径,从配置文件中解析
};