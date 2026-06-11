#include "AllHeader.h"

IMPLEMENT_APP(EditorApp)

BEGIN_EVENT_TABLE(EditorApp, wxApp)
EVT_KEY_DOWN(OnKeyDown)
EVT_KEY_UP(OnKeyUp)
EVT_MOUSEWHEEL(OnMouseWheel)

END_EVENT_TABLE()

const wxCmdLineEntryDesc EditorApp::mCmdLineDesc[] =
{
	{ wxCMD_LINE_PARAM, nullptr, nullptr, "CSV file to open", wxCMD_LINE_VAL_STRING, wxCMD_LINE_PARAM_OPTIONAL },
	{ wxCMD_LINE_NONE }
};

EditorApp::EditorApp()
{
	mEditorApp = this;
	mCSVFile = new CSVFile();
	mUndoManager = new UndoManager();
}

bool EditorApp::OnInit()
{
	if (!wxApp::OnInit())
	{
		return false;
	}
	EditorFrame* mainFrame = new EditorFrame("CSVEditor", wxSize(1440, 900));
	mainFrame->init();
	mainFrame->Show(true);

    if (!mFileToOpen.empty())
	{
        mMainListWindow->openFile(mFileToOpen);
    } 
	return true;
}

void EditorApp::OnInitCmdLine(wxCmdLineParser& parser)
{
	parser.SetDesc(mCmdLineDesc);
}

bool EditorApp::OnCmdLineParsed(wxCmdLineParser& parser)
{
	if (parser.GetParamCount() > 0)
	{
		mFileToOpen = parser.GetParam(0);
	}
	return true;
}