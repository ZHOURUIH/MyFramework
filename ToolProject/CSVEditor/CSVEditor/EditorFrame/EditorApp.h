#pragma once

#include "FrameHeader.h"

class EditorApp : public wxApp
{
public:
	EditorApp();
	virtual ~EditorApp() = default;
	virtual bool OnInit();

	DECLARE_EVENT_TABLE()
	void OnKeyDown(wxKeyEvent& event) { event.Skip(); }
	void OnKeyUp(wxKeyEvent& event) { event.Skip(); }
	void OnMouseWheel(wxMouseEvent& event) { event.Skip(); }
	void OnInitCmdLine(wxCmdLineParser& parser) override;
	bool OnCmdLineParsed(wxCmdLineParser& parser) override;
protected:
	string mFileToOpen;
	static const wxCmdLineEntryDesc mCmdLineDesc[];
};

DECLARE_APP(EditorApp)