#pragma once

#include "FrameHeader.h"

// 自定义消息对话框（支持修改按钮文字）
class CustomMessageBox : public wxDialog
{
public:
    CustomMessageBox(wxWindow* parent, const wxString& title, const wxString& message, const wxString& yesText, const wxString& noText, const wxString& cancelText, int flag);
    // 返回结果（映射到标准 wxID_YES/wxID_NO/wxID_CANCEL）
    int GetResult() const { return m_result; }
protected:
    void OnButtonClick(wxCommandEvent& event);
protected:
    int m_result = wxID_CANCEL; // 默认返回取消
};