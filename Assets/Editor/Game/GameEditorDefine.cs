using System.Collections.Generic;

public class GameEditorDefine : EditorDefine
{
	protected override List<string> getForceSingleFolder_Extension() 
	{
		return new()
		{
			"Audio/Ambient_SFX/",
			"Audio/BGM/",
		};
	}
	protected override List<string> getIgnoreCodeWidth_Extension() 
	{
		return new()
		{
			"/PacketRegister.cs",
			"/StateRegister.cs",
		};
	}
	protected override List<string> getIgnoreSystemFunctionCheck_Extension() 
	{
		return new()
		{
			"GameUtilityHotFix.cs",
		};
	}
}