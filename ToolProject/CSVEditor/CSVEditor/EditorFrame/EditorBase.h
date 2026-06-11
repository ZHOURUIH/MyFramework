#pragma once

#include "FrameHeader.h"

class CSVFile;
class EditorApp;
class EditorFrame;
class MainListWindow;
class UndoManager;

namespace EditorBase
{
	extern CSVFile* mCSVFile;
	extern EditorApp* mEditorApp;
	extern EditorFrame* mEditorFrame;
	extern MainListWindow* mMainListWindow;
	extern UndoManager* mUndoManager;
};

using namespace EditorBase;