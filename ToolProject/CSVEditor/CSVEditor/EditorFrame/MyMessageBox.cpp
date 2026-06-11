#include "AllHeader.h"

CustomMessageBox::CustomMessageBox(wxWindow* parent, const wxString& title, const wxString& message, const wxString& yesText, const wxString& noText, const wxString& cancelText, int flag)
    : wxDialog(parent, wxID_ANY, title, wxDefaultPosition, wxSize(500, 300))
{
    // 布局控件
    wxBoxSizer* mainSizer = new wxBoxSizer(wxVERTICAL);
    // 添加消息文本
    mainSizer->Add(new wxStaticText(this, wxID_ANY, message), 0, wxALL | wxALIGN_CENTER, 10);
    // 创建按钮区
    wxStdDialogButtonSizer* btnSizer = new wxStdDialogButtonSizer();
    // 自定义按钮文字
    if (hasFlag(flag, wxYES))
    {
        btnSizer->AddButton(new wxButton(this, wxID_YES, yesText));
    }
    if (hasFlag(flag, wxNO))
    {
        btnSizer->AddButton(new wxButton(this, wxID_NO, noText));
    }
    if (hasFlag(flag, wxCANCEL))
    {
        btnSizer->AddButton(new wxButton(this, wxID_CANCEL, cancelText));
    }
    btnSizer->Realize();
    mainSizer->Add(btnSizer, 0, wxEXPAND | wxALL, 10);
    SetSizerAndFit(mainSizer);
    // 绑定按钮事件
    Bind(wxEVT_BUTTON, &CustomMessageBox::OnButtonClick, this);
}

void CustomMessageBox::OnButtonClick(wxCommandEvent& event)
{
    m_result = event.GetId(); // 记录点击的按钮ID
    EndModal(m_result);
}